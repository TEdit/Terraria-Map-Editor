using Semver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using TEdit.Configuration;
using TEdit.Editor.Clipboard;
using TEdit.Framework.Threading;
using TEdit.Input;
using TEdit.Services;
using TEdit.Utility;
using ReactiveUI.Builder;
using TEdit.Terraria;
using TEdit.ViewModel;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace TEdit;


/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    static App()
    {
        var sw = Stopwatch.StartNew();

        DispatcherHelper.Initialize();
        Trace.WriteLine($"[Startup] static App: DispatcherHelper.Initialize: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        RxAppBuilder.CreateReactiveUIBuilder()
            .WithWpf()
            .BuildApp();
        Trace.WriteLine($"[Startup] static App: ReactiveUI init: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        var settings = UserSettingsService.Current;
        Trace.WriteLine($"[Startup] static App: UserSettingsService.Current: {sw.ElapsedMilliseconds}ms");

        switch (settings.Language)
        {
            case LanguageSelection.Automatic:
                //System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.CurrentCulture;
                break;
            case LanguageSelection.English:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en");
                break;
            case LanguageSelection.Russian:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru-RU");
                break;
            case LanguageSelection.Arabic:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ar-BH");
                break;
            case LanguageSelection.Polish:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pl-PL");
                break;
            case LanguageSelection.Chinese:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hans");
                break;
            case LanguageSelection.German:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de-DE");
                break;
            case LanguageSelection.Portuguese:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pt-BR");
                break;
            case LanguageSelection.Spanish:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-ES");
                break;
            case LanguageSelection.French:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr-FR");
                break;
            case LanguageSelection.Italian:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("it-IT");
                break;
            case LanguageSelection.Japanese:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ja-JP");
                break;
            case LanguageSelection.Korean:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ko-KR");
                break;
            case LanguageSelection.ChineseTraditional:
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hant");
                break;

        }
    }

    public static SemVersion Version { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var totalSw = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();

        var splashScreen = new SplashScreen("Images/te5-logo.png");
        splashScreen.Show(autoClose: true, topMost: true);
        ErrorLogging.LogDebug($"[Startup] SplashScreen.Show: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        // Initialize WPF UI theme
        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        ApplicationAccentColorManager.Apply(System.Windows.Media.Color.FromRgb(0x00, 0xA0, 0x00), ApplicationTheme.Dark);
        ErrorLogging.LogDebug($"[Startup] Theme init: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        // Read settings immediately.
        LoadAppSettings();
        ErrorLogging.LogDebug($"[Startup] LoadAppSettings: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        // Enable cross-thread access to Sprites2 collection (modified on graphics thread, bound to UI)
        BindingOperations.EnableCollectionSynchronization(
            WorldConfiguration.Sprites2,
            WorldConfiguration.Sprites2Lock);
        ErrorLogging.LogDebug($"[Startup] WorldConfiguration.Sprites2 init: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        Version = SemVersion.Parse(Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion, SemVersionStyles.Any);
        ErrorLogging.Initialize();
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        ErrorLogging.Log($"Starting TEdit {ErrorLogging.Version}");
        ErrorLogging.Log($"OS: {Environment.OSVersion}");
        ErrorLogging.LogDebug($"[Startup] ErrorLogging init: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        try
        {
            ErrorLogging.Log($"OS Name: {DependencyChecker.GetOsVersion()}");
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn("Failed to verify OS Version. TEdit may not run properly.");
            ErrorLogging.LogException(ex);
        }
        ErrorLogging.LogDebug($"[Startup] GetOsVersion: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        try
        {
            ErrorLogging.Log(DependencyChecker.GetDotNetVersion());
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn("Failed to verify .Net Framework Version. TEdit may not run properly.");
            ErrorLogging.LogException(ex);
        }
        ErrorLogging.LogDebug($"[Startup] GetDotNetVersion: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        try
        {
            int directXMajorVersion = DependencyChecker.GetDirectXMajorVersion();
            if (directXMajorVersion < 11)
            {
                ErrorLogging.LogWarn($"DirectX {directXMajorVersion} unsupported. DirectX 11 or higher is required.");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn("Failed to verify DirectX Version. TEdit may not run properly.");
            ErrorLogging.LogException(ex);
        }
        ErrorLogging.LogDebug($"[Startup] GetDirectXVersion: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        try
        {
            DependencyChecker.CheckPaths();
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn("Failed to verify Terraria Paths. TEdit may not run properly.");
            ErrorLogging.LogException(ex);
        }
        ErrorLogging.LogDebug($"[Startup] CheckPaths: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        try
        {
            if (!DependencyChecker.VerifyTerraria())
            {
                ErrorLogging.LogWarn("Unable to locate Terraria. No texture data will be available.");
            }
            else
            {
                ErrorLogging.Log($"Terraria v{DependencyChecker.GetTerrariaVersion() ?? "not found"}");
                ErrorLogging.Log($"Terraria Data Path: {DependencyChecker.PathToContent}");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.LogWarn("Failed to verify Terraria Paths. No texture data will be available.");
            ErrorLogging.LogException(ex);
        }
        ErrorLogging.LogDebug($"[Startup] VerifyTerraria: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        FileMaintenance.CleanupOldAutosaves();
        ErrorLogging.LogDebug($"[Startup] CleanupOldAutosaves: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        FileMaintenance.LogWorldBackupFiles();
        ErrorLogging.LogDebug($"[Startup] LogWorldBackupFiles: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        if (!string.IsNullOrEmpty(DependencyChecker.PathToWorlds))
        {
            FileMaintenance.MigrateLegacyTEditBackups(DependencyChecker.PathToWorlds, ViewModel.WorldViewModel.BackupPath);
        }
        ErrorLogging.LogDebug($"[Startup] MigrateLegacyBackups: {sw.ElapsedMilliseconds}ms");

        if (e.Args != null && e.Args.Count() > 0)
        {
            ErrorLogging.Log($"Command Line Open: {e.Args[0]}");
            Properties["OpenFile"] = e.Args[0];
        }

        sw.Restart();
        DispatcherHelper.Initialize();
        TaskFactoryHelper.Initialize();
        ErrorLogging.LogDebug($"[Startup] DispatcherHelper/TaskFactory init: {sw.ElapsedMilliseconds}ms");

        base.OnStartup(e);

        // Auto-detect: Mica requires Windows 11 22H2+ (build 22621)
        if (Environment.OSVersion.Version.Build < 22621 && UserSettingsService.Current.EnableMica)
        {
            ErrorLogging.Log("Mica disabled: Windows 11 22H2 or later required.");
            UserSettingsService.Current.EnableMica = false;
        }

        // When Mica is disabled, override backdrop on all FluentWindows at load time
        if (!UserSettingsService.Current.EnableMica)
        {
            EventManager.RegisterClassHandler(
                typeof(FluentWindow),
                FrameworkElement.LoadedEvent,
                new RoutedEventHandler((sender, _) =>
                {
                    if (sender is FluentWindow fw)
                        fw.WindowBackdropType = WindowBackdropType.None;
                }));
        }

        sw.Restart();
        // Create main window manually (StartupUri removed from App.xaml)
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
        ErrorLogging.LogDebug($"[Startup] MainWindow create+show: {sw.ElapsedMilliseconds}ms");

        ErrorLogging.LogDebug($"[Startup] === Total OnStartup: {totalSw.ElapsedMilliseconds}ms ===");

        // Fire-and-forget background update check via the ViewModel
        if (UserSettingsService.Current.UpdateMode != UpdateMode.Disabled)
        {
            var wvm = mainWindow.DataContext as WorldViewModel;
            if (wvm != null)
            {
                _ = wvm.StartupUpdateCheckAsync();
            }
        }
    }


    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
#if DEBUG
        throw (Exception)e.ExceptionObject;
#else
        ErrorLogging.LogException(e.ExceptionObject as Exception);
        System.Windows.MessageBox.Show("An unhandled exception has occurred. You may continue using TEdit, but operation may be unstable until the application has been restarted." + e.ExceptionObject.ToString(), "Unhandled Exception");
        // Current.Shutdown();
#endif
    }

    public static KeyboardShortcuts ShortcutKeys { get; } = new KeyboardShortcuts();
    public static InputService Input { get; } = new InputService();
    public static AppSettings AppConfig { get; private set; }
    public static IConfiguration Configuration { get; private set; }

    /// <summary>
    /// WPF-UI based dialog service for styled modal dialogs.
    /// </summary>
    public static TEditDialogService DialogService { get; } = new TEditDialogService();

    /// <summary>
    /// WPF-UI based snackbar service for non-blocking notifications.
    /// </summary>
    public static TEditSnackbarService SnackbarService { get; } = new TEditSnackbarService();

    public static void LoadAppSettings()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        Configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddYamlFile("appsettings.yaml", optional: false, reloadOnChange: false)
            .Build();

        // Bind app settings
        AppConfig = Configuration.GetSection("app").Get<AppSettings>() ?? new AppSettings();
        ClipboardBufferRenderer.ClipboardRenderSize = (int)Calc.Clamp(AppConfig.ClipboardRenderSize, 64, 4096);

        // Load shortcuts (action: keyCombo format, e.g., "Copy: ctrl+c")
        var shortcuts = Configuration.GetSection("shortcuts").Get<Dictionary<string, string>>() ?? new();
        foreach (var (action, keyCombo) in shortcuts)
        {
            ParseShortcut(keyCombo, out var key, out var modifiers);
            if (key == Key.None)
            {
                ErrorLogging.LogWarn($"[Settings] Invalid key in shortcut '{keyCombo}' for action '{action}'; skipping.");
                continue;
            }
            bool duplicate = ShortcutKeys.Add(action, key, modifiers);
            if (duplicate)
            {
                ErrorLogging.LogWarn($"[Settings] Duplicate shortcut {modifiers}+{key} -> using first binding, ignoring '{action}'.");
            }
        }

        // Initialize the new InputService (registers default actions and loads user customizations)
        Input.Initialize();
    }

    private static void ParseShortcut(string combo, out Key key, out ModifierKeys modifiers)
    {
        modifiers = ModifierKeys.None;
        key = Key.None;

        var parts = combo.ToLowerInvariant().Split('+');
        foreach (var part in parts)
        {
            switch (part.Trim())
            {
                case "ctrl":
                    modifiers |= ModifierKeys.Control;
                    break;
                case "shift":
                    modifiers |= ModifierKeys.Shift;
                    break;
                case "alt":
                    modifiers |= ModifierKeys.Alt;
                    break;
                default:
                    // Try to parse the key
                    if (Enum.TryParse<Key>(part, true, out var parsedKey))
                        key = parsedKey;
                    break;
            }
        }
    }
}
