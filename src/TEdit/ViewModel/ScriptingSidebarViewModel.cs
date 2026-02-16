using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Scripting.Api;
using TEdit.Scripting.Engine;

namespace TEdit.ViewModel;

[IReactiveObject]
public partial class ScriptingSidebarViewModel
{
    private readonly WorldViewModel _wvm;
    private readonly IScriptEngine[] _engines;
    private CancellationTokenSource? _cts;
    private readonly Dispatcher _dispatcher;

    [Reactive] private string _scriptCode = "";
    [Reactive] private string _outputLog = "";
    [Reactive] private bool _isRunning;
    [Reactive] private double _progress;
    [Reactive] private int _selectedEngineIndex;
    [Reactive] private string? _selectedScriptPath;

    public ObservableCollection<ScriptFileInfo> Scripts { get; } = new();

    public string[] EngineNames { get; }

    public ScriptingSidebarViewModel(WorldViewModel wvm)
    {
        _wvm = wvm;
        _dispatcher = Dispatcher.CurrentDispatcher;
        _engines = new IScriptEngine[]
        {
            new JintScriptEngine(),
            new LuaScriptEngine()
        };
        EngineNames = _engines.Select(e => e.Name).ToArray();

        this.WhenAnyValue(x => x.SelectedScriptPath)
            .Subscribe(_ => LoadSelectedScript());

        LoadScriptFiles();
    }

    [ReactiveCommand]
    private async Task RunScript()
    {
        if (_isRunning) return;
        if (_wvm.CurrentWorld == null)
        {
            AppendLog("[Error] No world loaded.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_scriptCode))
        {
            AppendLog("[Error] No script code to run.");
            return;
        }

        IsRunning = true;
        Progress = 0;
        OutputLog = "";

        _cts = new CancellationTokenSource();
        var engine = _engines[_selectedEngineIndex];
        var code = _scriptCode;

        AppendLog($"[{engine.Name}] Running script...");

        var context = new ScriptExecutionContext
        {
            CancellationToken = _cts.Token,
            OnLog = msg => _dispatcher.BeginInvoke(() => AppendLog(msg)),
            OnWarn = msg => _dispatcher.BeginInvoke(() => AppendLog($"[Warn] {msg}")),
            OnError = msg => _dispatcher.BeginInvoke(() => AppendLog($"[Error] {msg}")),
            OnProgress = p => _dispatcher.BeginInvoke(() => Progress = p * 100)
        };

        ScriptResult result;
        ScriptApi? scriptApi = null;
        try
        {
            result = await Task.Run(() =>
            {
                scriptApi = new ScriptApi(_wvm, context);
                scriptApi.BeginExecution();
                return engine.Execute(code, scriptApi, context);
            }, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            result = new ScriptResult(false, "Script was cancelled", 0);
        }

        // EndExecution must run on UI thread (updates WriteableBitmap)
        if (result.Success && scriptApi != null)
            scriptApi.EndExecution();

        scriptApi?.Dispose();

        if (result.Success)
            AppendLog($"[Done] Completed in {result.ElapsedMs}ms");
        else
            AppendLog($"[Error] {result.Error} ({result.ElapsedMs}ms)");

        Progress = 0;
        IsRunning = false;
        _cts = null;
    }

    [ReactiveCommand]
    private void StopScript()
    {
        _cts?.Cancel();
    }

    [ReactiveCommand]
    private void NewScript()
    {
        ScriptCode = "";
        SelectedScriptPath = null;
    }

    [ReactiveCommand]
    private void SaveScript()
    {
        if (string.IsNullOrWhiteSpace(_selectedScriptPath))
        {
            var ext = _engines[_selectedEngineIndex].FileExtension;
            var dir = GetScriptsDirectory();
            var name = $"script_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
            SelectedScriptPath = Path.Combine(dir, name);
        }

        File.WriteAllText(_selectedScriptPath!, _scriptCode);
        AppendLog($"Saved: {Path.GetFileName(_selectedScriptPath)}");
        LoadScriptFiles();
    }

    [ReactiveCommand]
    private void OpenScriptsFolder()
    {
        var dir = GetScriptsDirectory();
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = dir,
            UseShellExecute = true
        });
    }

    public void LoadSelectedScript()
    {
        if (_selectedScriptPath == null || !File.Exists(_selectedScriptPath)) return;

        ScriptCode = File.ReadAllText(_selectedScriptPath);

        // Auto-detect engine from file extension
        var ext = Path.GetExtension(_selectedScriptPath).ToLowerInvariant();
        for (int i = 0; i < _engines.Length; i++)
        {
            if (_engines[i].FileExtension == ext)
            {
                SelectedEngineIndex = i;
                break;
            }
        }
    }

    private void LoadScriptFiles()
    {
        var dir = GetScriptsDirectory();
        Scripts.Clear();

        if (!Directory.Exists(dir)) return;

        var files = Directory.GetFiles(dir, "*.*")
            .Where(f => f.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                        f.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f);

        foreach (var file in files)
        {
            Scripts.Add(new ScriptFileInfo(Path.GetFileName(file), file));
        }
    }

    private void AppendLog(string message)
    {
        OutputLog += message + Environment.NewLine;
    }

    public static string GetScriptsDirectory()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TEdit", "Scripts");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public void EnsureExampleScripts()
    {
        var dir = GetScriptsDirectory();

        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var prefix = "TEdit.Scripting.Examples.";

        var extracted = false;
        foreach (var resourceName in assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(prefix)))
        {
            var fileName = resourceName[prefix.Length..];
            var destPath = Path.Combine(dir, fileName);

            if (!File.Exists(destPath))
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var fs = File.Create(destPath);
                    stream.CopyTo(fs);
                    extracted = true;
                }
            }
        }

        if (extracted)
            LoadScriptFiles();
    }
}

public record ScriptFileInfo(string Name, string FullPath);
