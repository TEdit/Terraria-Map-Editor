using Semver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using TEdit.Configuration;
using TEdit.Editor;
using TEdit.Editor.Clipboard;
using TEdit.Framework.Threading;
using TEdit.Input;
using TEdit.Services;
using TEdit.Utility;
using ReactiveUI;
using ReactiveUI.Builder;
using TEdit.Terraria;
using TEdit.ViewModel;
using Wpf.Ui.Appearance;

namespace TEdit;


/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    static App()
    {
        DispatcherHelper.Initialize();

        RxAppBuilder.CreateReactiveUIBuilder()
            .WithWpf()
            .BuildApp();

        switch (UserSettingsService.Current.Language)
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
        // Show splash - autoClose fades when main window appears, topMost keeps it above
        var splashScreen = new SplashScreen("Images/te5-logo.png");
        splashScreen.Show(autoClose: true, topMost: true);

        // Initialize WPF UI theme
        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        ApplicationAccentColorManager.Apply(System.Windows.Media.Color.FromRgb(0x00, 0xA0, 0x00), ApplicationTheme.Dark);

        // Read settings immediately.
        LoadAppSettings();

        // Enable cross-thread access to Sprites2 collection (modified on graphics thread, bound to UI)
        BindingOperations.EnableCollectionSynchronization(
            WorldConfiguration.Sprites2,
            WorldConfiguration.Sprites2Lock);

        Version = SemVersion.Parse(Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion, SemVersionStyles.Any);
        ErrorLogging.Initialize();
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        ErrorLogging.Log($"Starting TEdit {ErrorLogging.Version}");
        ErrorLogging.Log($"OS: {Environment.OSVersion}");

        try
        {
            ErrorLogging.Log($"OS Name: {DependencyChecker.GetOsVersion()}");
        }
        catch (Exception ex)
        {
            ErrorLogging.Log("Failed to verify OS Version. TEdit may not run properly.");
            ErrorLogging.LogException(ex);
        }


        try
        {
            ErrorLogging.Log(DependencyChecker.GetDotNetVersion());
        }
        catch (Exception ex)
        {
            ErrorLogging.Log("Failed to verify .Net Framework Version. TEdit may not run properly.");
            ErrorLogging.LogException(ex);
        }

        try
        {
            int directXMajorVersion = DependencyChecker.GetDirectXMajorVersion();
            if (directXMajorVersion < 11)
            {
                ErrorLogging.Log($"DirectX {directXMajorVersion} unsupported. DirectX 11 or higher is required.");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.Log("Failed to verify DirectX Version. TEdit may not run properly.");
            ErrorLogging.LogException(ex);
        }

        try
        {
            DependencyChecker.CheckPaths();
        }
        catch (Exception ex)
        {
            ErrorLogging.Log("Failed to verify Terraria Paths. TEdit may not run properly.");
            ErrorLogging.LogException(ex);
        }


        try
        {
            if (!DependencyChecker.VerifyTerraria())
            {
                ErrorLogging.Log("Unable to locate Terraria. No texture data will be available.");
            }
            else
            {
                ErrorLogging.Log($"Terraria v{DependencyChecker.GetTerrariaVersion() ?? "not found"}");
                ErrorLogging.Log($"Terraria Data Path: {DependencyChecker.PathToContent}");
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.Log("Failed to verify Terraria Paths. No texture data will be available.");
            ErrorLogging.LogException(ex);
        }


        FileMaintenance.CleanupOldAutosaves();
        FileMaintenance.LogWorldBackupFiles();

        if (e.Args != null && e.Args.Count() > 0)
        {
            ErrorLogging.Log($"Command Line Open: {e.Args[0]}");
            Properties["OpenFile"] = e.Args[0];
        }

        //if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null &&
        //    AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null &&
        //    AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length > 0)
        //{
        //    string fname = "No filename given";
        //    try
        //    {
        //        fname = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0];

        //        // It comes in as a URI; this helps to convert it to a path.
        //        var uri = new Uri(fname);
        //        fname = uri.LocalPath;

        //        Properties["OpenFile"] = fname;
        //    }
        //    catch (Exception ex)
        //    {
        //        // For some reason, this couldn't be read as a URI.
        //        // Do what you must...
        //        ErrorLogging.LogException(ex);
        //    }
        //}

        DispatcherHelper.Initialize();
        TaskFactoryHelper.Initialize();

        base.OnStartup(e);

        // Create main window manually (StartupUri removed from App.xaml)
        // TEMP: Using TestWindow to debug ContentDialogHost
        //var mainWindow = new TestWindow();
        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();

        // Fire-and-forget background update check
        if (UserSettingsService.Current.CheckUpdates)
        {
            _ = CheckForUpdatesAsync();
        }
    }

    private static async Task CheckForUpdatesAsync()
    {
        try
        {
            var updateService = new Services.UpdateService(UserSettingsService.Current.UpdateChannel);
            if (!updateService.IsInstalled) return;

            bool downloaded = await updateService.CheckAndDownloadAsync();
            if (downloaded)
            {
                var result = await DialogService.ShowMessageAsync(
                    "A new version of TEdit has been downloaded. Restart now to apply the update?",
                    "Update Available",
                    UI.Xaml.Dialog.DialogButton.YesNo,
                    UI.Xaml.Dialog.DialogImage.Question);

                if (result == UI.Xaml.Dialog.DialogResponse.Yes)
                {
                    updateService.ApplyAndRestart();
                }
            }
        }
        catch (Exception ex)
        {
            ErrorLogging.Log($"[Update] Background check failed: {ex.Message}");
        }
    }


    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
#if DEBUG
        throw (Exception)e.ExceptionObject;
#else
        ErrorLogging.LogException(e.ExceptionObject as Exception);
        MessageBox.Show("An unhandled exception has occurred. You may continue using TEdit, but operation may be unstable until the application has been restarted." + e.ExceptionObject.ToString(), "Unhandled Exception");
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
                ErrorLogging.Log($"[Settings] Invalid key in shortcut '{keyCombo}' for action '{action}'; skipping.");
                continue;
            }
            bool duplicate = ShortcutKeys.Add(action, key, modifiers);
            if (duplicate)
            {
                ErrorLogging.Log($"[Settings] Duplicate shortcut {modifiers}+{key} -> using first binding, ignoring '{action}'.");
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
