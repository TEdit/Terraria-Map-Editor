#if LOCAL_DIAGNOSTICS
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TEdit.Configuration;
using TEdit.Editor;
using TEdit.Terraria;
using TEdit.View.Popups;
using TEdit.ViewModel;

namespace TEdit;

internal sealed class LocalDiagnosticsServer : IDisposable
{
    private readonly Application _app;
    private readonly string _sessionPath;
    private readonly string _profile;
    private readonly string _pipeName = $"TEdit.Local.{Environment.ProcessId}.{Guid.NewGuid():N}";
    private readonly CancellationTokenSource _stop = new();
    private readonly JsonSerializerOptions _json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private string _lastOperation = "idle";
    private string? _operationId;

    internal LocalDiagnosticsServer(Application app, string sessionPath)
    {
        _app = app;
        _sessionPath = Path.GetFullPath(sessionPath);
        _profile = Path.GetFullPath(AppDataPaths.DataDir);
        Directory.CreateDirectory(Path.GetDirectoryName(_sessionPath)!);
        var listener = CreatePipe();
        using (var descriptor = new FileStream(_sessionPath, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
        {
            JsonSerializer.Serialize(descriptor, new
            {
                protocol = 1,
                pipe = _pipeName,
                pid = Environment.ProcessId,
                executable = Environment.ProcessPath,
                started = DateTimeOffset.UtcNow,
                profile = _profile,
                build = typeof(App).Assembly.GetName().Version?.ToString()
            }, _json);
        }
        _ = AcceptAsync(listener);
    }

    private NamedPipeServerStream CreatePipe() => new(_pipeName, PipeDirection.InOut,
        NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte,
        PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

    private async Task AcceptAsync(NamedPipeServerStream listener)
    {
        try
        {
            while (!_stop.IsCancellationRequested)
            {
                await listener.WaitForConnectionAsync(_stop.Token).ConfigureAwait(false);
                _ = ServeAsync(listener);
                listener = CreatePipe();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            listener.Dispose();
        }
    }

    private async Task ServeAsync(NamedPipeServerStream pipe)
    {
        using (pipe)
        using (var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 4096, true))
        using (var writer = new StreamWriter(pipe, new UTF8Encoding(false), 4096, true) { AutoFlush = true })
        {
            try
            {
                var line = await reader.ReadLineAsync(_stop.Token).ConfigureAwait(false);
                if (line is null || line.Length > 1024 * 1024)
                {
                    throw new ArgumentException("Request is empty or exceeds 1 MiB.");
                }
                using var request = JsonDocument.Parse(line);
                if (request.RootElement.TryGetProperty("op", out var op) && op.GetString() == "health")
                {
                    await writer.WriteLineAsync(JsonSerializer.Serialize(new
                    {
                        ok = true,
                        result = new { pid = Environment.ProcessId, pipe = _pipeName, profile = _profile, executable = Environment.ProcessPath }
                    }, _json)).ConfigureAwait(false);
                    return;
                }
                var result = await _app.Dispatcher.InvokeAsync(
                    () => DispatchAsync(request.RootElement.Clone()), DispatcherPriority.Background, _stop.Token)
                    .Task.Unwrap().ConfigureAwait(false);
                await writer.WriteLineAsync(JsonSerializer.Serialize(new { ok = true, result }, _json)).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                await writer.WriteLineAsync(JsonSerializer.Serialize(new { ok = false, error = error.ToString() }, _json)).ConfigureAwait(false);
            }
        }
    }

    private async Task<object> DispatchAsync(JsonElement request)
    {
        switch (request.GetProperty("op").GetString())
        {
            case "state":
                var vm = ViewModelLocator.WorldViewModel;
                var world = vm.CurrentWorld;
                return new
                {
                    pid = Environment.ProcessId,
                    profile = _profile,
                    executable = Environment.ProcessPath,
                    operation = _lastOperation,
                    operationId = _operationId,
                    window = _app.MainWindow is { } window ? new { window.Title, window.IsVisible, window.ActualWidth, window.ActualHeight } : null,
                    windows = _app.Windows.Cast<Window>().Select(w => new { w.Title, type = w.GetType().FullName, w.IsVisible, w.IsActive, w.ActualWidth, w.ActualHeight, text = ReadWindowText(w) }).ToArray(),
                    currentFile = vm.CurrentFile,
                    progress = vm.Progress is null ? null : new { vm.Progress.ProgressPercentage, message = vm.Progress.UserState?.ToString() },
                    world = world is null ? null : new { world.Title, vm.CurrentFile, width = world.TilesWide, height = world.TilesHigh, world.Version, world.SpawnX, world.SpawnY },
                    settings = new { UserSettingsService.Current.UpdateMode, UserSettingsService.Current.ShowSplashScreen, UserSettingsService.Current.MinimapBackground }
                };
            case "open-world":
                var path = RequireProfilePath(request.GetProperty("path").GetString()!, Path.Combine(_profile, "fixtures"));
                if (!File.Exists(path) || !path.EndsWith(".wld", StringComparison.OrdinalIgnoreCase))
                {
                    throw new FileNotFoundException("Open a .wld file from the diagnostic fixtures directory.", path);
                }
                var operationId = Guid.NewGuid().ToString("N");
                _operationId = operationId;
                _lastOperation = "running:open-world";
                ViewModelLocator.WorldViewModel.LoadWorld(path);
                foreach (var explorer in _app.Windows.Cast<Window>().OfType<WorldExplorerWindow>().Where(window => window.IsVisible).ToArray())
                {
                    explorer.Close();
                }
                _ = TrackOpenWorldAsync(path, operationId);
                return new { status = "accepted", operationId, file = path };
            case "paint-tile":
                var x = request.GetProperty("x").GetInt32();
                var y = request.GetProperty("y").GetInt32();
                var type = request.GetProperty("type").GetInt32();
                var paintVm = ViewModelLocator.WorldViewModel;
                var paintWorld = paintVm.CurrentWorld ?? throw new InvalidOperationException("Open a world first.");
                if (x < 1 || y < 1 || x >= paintWorld.TilesWide - 1 || y >= paintWorld.TilesHigh - 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(x), "Tile must be inside the world border.");
                }
                if (type < 0 || type >= WorldConfiguration.TileProperties.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(type), "Tile type is outside the loaded Terraria configuration.");
                }
                _lastOperation = "running:paint-tile";
                paintVm.TilePicker.PaintMode = PaintMode.TileAndWall;
                paintVm.TilePicker.TileStyleActive = true;
                paintVm.TilePicker.Tile = type;
                paintVm.TilePicker.IsEraser = false;
                await paintVm.WorldEditor.BeginOperationAsync();
                paintVm.WorldEditor.SetPixel(x, y, PaintMode.TileAndWall, false);
                await paintVm.WorldEditor.EndOperationAsync();
                var painted = paintWorld.Tiles[x, y];
                _lastOperation = "completed:paint-tile";
                return new { status = "completed", x, y, painted.IsActive, painted.Type };
            case "inspect-tile":
                var inspectWorld = ViewModelLocator.WorldViewModel.CurrentWorld ?? throw new InvalidOperationException("Open a world first.");
                var tileX = request.GetProperty("x").GetInt32();
                var tileY = request.GetProperty("y").GetInt32();
                if (tileX < 0 || tileY < 0 || tileX >= inspectWorld.TilesWide || tileY >= inspectWorld.TilesHigh)
                {
                    throw new ArgumentOutOfRangeException(nameof(tileX), "Tile is outside the world.");
                }
                var tile = inspectWorld.Tiles[tileX, tileY];
                return new { x = tileX, y = tileY, tile.IsActive, tile.Type, tile.Wall, tile.LiquidAmount };
            case "resolve-dialog":
                var dialogTitle = request.GetProperty("title").GetString();
                var choice = request.GetProperty("choice").GetString();
                if (choice is not ("Yes" or "No" or "Close"))
                {
                    throw new ArgumentException("Dialog choice must be Yes, No, or Close.");
                }
                var dialog = _app.Windows.Cast<Window>().FirstOrDefault(w => w != _app.MainWindow && w.IsVisible && w.Title == dialogTitle)
                    ?? throw new InvalidOperationException($"No visible dialog titled '{dialogTitle}' was found.");
                var button = FindButton(dialog, choice)
                    ?? throw new InvalidOperationException($"Dialog '{dialogTitle}' has no '{choice}' button.");
                var peer = UIElementAutomationPeer.CreatePeerForElement(button)
                    ?? throw new InvalidOperationException("The dialog button has no automation peer.");
                if (peer.GetPattern(PatternInterface.Invoke) is not IInvokeProvider invoke)
                {
                    throw new InvalidOperationException("The dialog button does not support Invoke.");
                }
                invoke.Invoke();
                return new { status = "dispatched", title = dialogTitle, choice };
            case "save":
                var saveVm = ViewModelLocator.WorldViewModel;
                var savePath = RequireProfilePath(saveVm.CurrentFile ?? string.Empty, Path.Combine(_profile, "fixtures"));
                var saveMethod = typeof(WorldViewModel).GetMethod("SaveWorldFileAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?? throw new MissingMethodException("TEdit save routine was not found.");
                _lastOperation = "running:save";
                await (Task)saveMethod.Invoke(saveVm, new object[] { 0u, true, false })!;
                _lastOperation = "completed:save";
                return new { status = "completed", file = savePath, bytes = new FileInfo(savePath).Length };
            case "setting":
                var settings = UserSettingsService.Current;
                var setting = request.GetProperty("name").GetString();
                if (setting != "MinimapBackground" || request.GetProperty("value").ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                {
                    throw new ArgumentException("Supported setting: MinimapBackground (boolean).");
                }
                settings.MinimapBackground = request.GetProperty("value").GetBoolean();
                return new { name = setting, value = settings.MinimapBackground, persistedTo = Path.Combine(_profile, "userSettings.json") };
            case "screenshot":
                var screenshotPath = RequireProfilePath(request.GetProperty("path").GetString()!, Path.Combine(_profile, "artifacts"));
                var activeWindowOnly = request.TryGetProperty("scope", out var scope) && scope.GetString() == "active";
                var main = activeWindowOnly
                    ? _app.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive) ?? throw new InvalidOperationException("No active window is available.")
                    : _app.MainWindow ?? throw new InvalidOperationException("Main window is unavailable.");
                main.UpdateLayout();
                var width = (int)Math.Ceiling(main.ActualWidth);
                var height = (int)Math.Ceiling(main.ActualHeight);
                if (width < 1 || height < 1 || (long)width * height > 32_000_000)
                {
                    throw new InvalidOperationException("Main window has no capturable layout.");
                }
                var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(main);
                Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
                using (var stream = File.Create(screenshotPath))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                }
                if (activeWindowOnly)
                {
                    return new { path = screenshotPath, width, height, renderer = "WPF RenderTargetBitmap", target = main.GetType().FullName };
                }
                var viewport = FindViewport(main) ?? throw new InvalidOperationException("The XNA world viewport is not in the main window visual tree.");
                var viewportPixels = await viewport.CaptureDiagnosticViewportAsync(TimeSpan.FromSeconds(10));
                var viewportDimensions = viewport.GraphicsDeviceDimensions;
                var viewportPath = Path.Combine(Path.GetDirectoryName(screenshotPath)!, Path.GetFileNameWithoutExtension(screenshotPath) + "-viewport.png");
                var viewportBitmap = BitmapSource.Create(
                    viewportDimensions.Width,
                    viewportDimensions.Height,
                    96,
                    96,
                    PixelFormats.Bgra32,
                    null,
                    viewportPixels,
                    viewportDimensions.Width * 4);
                using (var stream = File.Create(viewportPath))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(viewportBitmap));
                    encoder.Save(stream);
                }
                return new { path = screenshotPath, width, height, renderer = "WPF RenderTargetBitmap", includesXnaViewport = false, viewportPath, viewportRenderer = "MonoGame backbuffer readback" };
            case "shutdown":
                _ = Task.Run(async () =>
                {
                    await Task.Delay(250);
                    await _app.Dispatcher.InvokeAsync(() => _app.Shutdown());
                });
                return new { status = "accepted" };
            default:
                throw new ArgumentException("Supported operations: state, open-world, paint-tile, inspect-tile, save, setting, screenshot, shutdown.");
        }
    }

    private static TEdit.View.WorldRenderXna? FindViewport(DependencyObject root)
    {
        if (root is TEdit.View.WorldRenderXna viewport)
        {
            return viewport;
        }

        var children = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < children; i++)
        {
            if (FindViewport(VisualTreeHelper.GetChild(root, i)) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static Button? FindButton(DependencyObject root, string content)
    {
        if (root is Button button && string.Equals(button.Content?.ToString(), content, StringComparison.OrdinalIgnoreCase))
        {
            return button;
        }
        var children = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < children; i++)
        {
            if (FindButton(VisualTreeHelper.GetChild(root, i), content) is { } found)
            {
                return found;
            }
        }
        return null;
    }

    private async Task TrackOpenWorldAsync(string path, string operationId)
    {
        var deadline = DateTime.UtcNow.AddMinutes(3);
        try
        {
            while (DateTime.UtcNow < deadline && !_stop.IsCancellationRequested)
            {
                await Task.Delay(250).ConfigureAwait(false);
                var status = await _app.Dispatcher.InvokeAsync(() =>
                {
                    var vm = ViewModelLocator.WorldViewModel;
                    var progress = vm.Progress?.UserState?.ToString();
                    if (string.Equals(Path.GetFullPath(vm.CurrentFile ?? string.Empty), path, StringComparison.OrdinalIgnoreCase) &&
                        vm.CurrentWorld is { } world &&
                        (progress?.StartsWith("World loaded in", StringComparison.Ordinal) == true || progress == "Render Complete"))
                    {
                        return $"completed:open-world:{world.Title}:{world.TilesWide}:{world.TilesHigh}";
                    }
                    if (_app.Windows.Cast<Window>().Any(window => window.IsVisible && window is Wpf.Ui.Controls.MessageBox))
                    {
                        return "waiting:dialog";
                    }
                    return "running:open-world";
                });

                if (_operationId == operationId)
                {
                    _lastOperation = status;
                }
                if (status.StartsWith("completed:", StringComparison.Ordinal))
                {
                    return;
                }
            }
            if (_operationId == operationId)
            {
                _lastOperation = "timed-out:open-world";
            }
        }
        catch (Exception error)
        {
            if (_operationId == operationId)
            {
                _lastOperation = $"failed:open-world:{error.GetType().Name}";
            }
        }
    }

    private static string ReadWindowText(DependencyObject root)
    {
        var text = new System.Collections.Generic.List<string>();
        ReadWindowText(root, text, 3000);
        return string.Join("\n", text.Distinct());
    }

    private static void ReadWindowText(DependencyObject root, System.Collections.Generic.List<string> text, int budget)
    {
        if (budget <= 0)
        {
            return;
        }
        if (root is System.Windows.Controls.TextBlock block && !string.IsNullOrWhiteSpace(block.Text))
        {
            text.Add(block.Text);
        }
        if (root is System.Windows.Controls.TextBox box && !string.IsNullOrWhiteSpace(box.Text))
        {
            text.Add(box.Text);
        }
        var children = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < children && budget > 0; i++, budget--)
        {
            ReadWindowText(VisualTreeHelper.GetChild(root, i), text, budget);
        }
    }

    private string RequireProfilePath(string path, string root)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Path must be under {fullRoot}");
        }
        return fullPath;
    }

    public void Dispose()
    {
        _stop.Cancel();
        try
        {
            if (File.Exists(_sessionPath) && File.ReadAllText(_sessionPath).Contains(_pipeName, StringComparison.Ordinal))
            {
                File.Delete(_sessionPath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
#endif
