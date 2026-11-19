using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Configuration;
using TEdit.Scripting;
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
    [Reactive] private bool _showApiReference;

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
            AppendLog(string.Format(Properties.Language.scripting_error, Properties.Language.find_no_world_loaded));
            return;
        }
        if (string.IsNullOrWhiteSpace(_scriptCode))
        {
            AppendLog(string.Format(Properties.Language.scripting_error, Properties.Language.scripting_empty));
            return;
        }

        IsRunning = true;
        Progress = 0;
        OutputLog = "";

        _cts = new CancellationTokenSource();
        var engine = _engines[_selectedEngineIndex];
        var code = _scriptCode;

        AppendLog(string.Format(Properties.Language.scripting_running, engine.Name));

        var findVm = ViewModelLocator.GetFindSidebarViewModel();

        var context = new ScriptExecutionContext
        {
            AccessProfile = Scripting.Execution.ScriptAccessProfile.LocalSidebar,
            TimeoutMs = UserSettingsService.Current.ScriptTimeoutSeconds * 1000,
            CancellationToken = _cts.Token,
            OnLog = msg => _dispatcher.BeginInvoke(() => AppendLog(msg)),
            OnWarn = msg => _dispatcher.BeginInvoke(() => AppendLog(string.Format(Properties.Language.scripting_warning, msg))),
            OnError = msg => _dispatcher.BeginInvoke(() => AppendLog(string.Format(Properties.Language.scripting_error, msg))),
            OnProgress = p => _dispatcher.BeginInvoke(() => Progress = p * 100),
            OnFindResult = (name, x, y, type, extra) => _dispatcher.BeginInvoke(() =>
            {
                findVm.Results.Add(new FindResultItem(name, x, y, type, extra));
            }),
            OnFindClear = () => _dispatcher.BeginInvoke(() =>
            {
                findVm.Results.Clear();
                findVm.CurrentResultIndex = -1;
                findVm.ShowCrosshair = false;
            }),
            OnFindNavigate = index => _dispatcher.BeginInvoke(() =>
            {
                if (index >= 0 && index < findVm.Results.Count)
                {
                    findVm.CurrentResultIndex = index;
                    findVm.SelectedResult = findVm.Results[index];
                    findVm.CrosshairTileX = findVm.Results[index].X;
                    findVm.CrosshairTileY = findVm.Results[index].Y;
                    findVm.ShowCrosshair = true;
                    _wvm.PanTo?.Invoke(findVm.Results[index].X, findVm.Results[index].Y);
                }
            }),
        };

        try
        {
            var run = await _wvm.ScriptExecution.ExecuteAsync(engine, code, context);
            var result = run.Script;
            if (result.Success)
                AppendLog(string.Format(Properties.Language.scripting_done, result.ElapsedMs));
            else
                AppendLog(string.Format(Properties.Language.scripting_elapsed, result.Error, result.ElapsedMs));
        }
        finally
        {
            Progress = 0;
            IsRunning = false;
            _cts.Dispose();
            _cts = null;
        }
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
        AppendLog(string.Format(Properties.Language.scripting_saved, Path.GetFileName(_selectedScriptPath)));
        LoadScriptFiles();
    }

    [ReactiveCommand]
    private void ToggleApiReference()
    {
        ShowApiReference = !ShowApiReference;
    }

    [ReactiveCommand]
    private void GenerateDocs()
    {
        var dir = GetScriptsDirectory();
        var path = Path.Combine(dir, "API_REFERENCE.md");
        File.WriteAllText(path, ScriptApiDocGenerator.GenerateMarkdown());
        AppendLog(string.Format(Properties.Language.scripting_generated, "API_REFERENCE.md"));
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
        var dir = Path.Combine(AppDataPaths.DataDir, "Scripts");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public void EnsureExampleScripts()
    {
        var dir = GetScriptsDirectory();

        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var prefix = "TEdit.Scripting.Examples.";

        foreach (var resourceName in assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(prefix)))
        {
            var fileName = resourceName[prefix.Length..];
            var destPath = Path.Combine(dir, fileName);

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var fs = File.Create(destPath);
                stream.CopyTo(fs);
            }
        }

        // Always regenerate API reference
        var docsPath = Path.Combine(dir, "API_REFERENCE.md");
        File.WriteAllText(docsPath, ScriptApiDocGenerator.GenerateMarkdown());

        LoadScriptFiles();
    }
}

public record ScriptFileInfo(string Name, string FullPath);
