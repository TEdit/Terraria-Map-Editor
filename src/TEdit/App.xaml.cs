using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using TEdit.MvvmLight.Threading;
using TEdit.Properties;
using TEdit.Terraria;
using TEdit.ViewModel;
using DispatcherHelper = GalaSoft.MvvmLight.Threading.DispatcherHelper;

namespace TEdit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
            

            switch (Settings.Default.Language)
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
                    System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
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
            }
        }

        public static string Version { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
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
            Assembly asm = Assembly.GetExecutingAssembly();

            Version = FileVersionInfo.GetVersionInfo(asm.Location).ProductVersion;

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


            if (e.Args != null && e.Args.Count() > 0)
            {
                ErrorLogging.Log($"Command Line Open: {e.Args[0]}");
                Properties["OpenFile"] = e.Args[0];
            }

            if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null &&
                AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null &&
                AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length > 0)
            {
                string fname = "No filename given";
                try
                {
                    fname = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0];

                    // It comes in as a URI; this helps to convert it to a path.
                    var uri = new Uri(fname);
                    fname = uri.LocalPath;

                    Properties["OpenFile"] = fname;
                }
                catch (Exception ex)
                {
                    // For some reason, this couldn't be read as a URI.
                    // Do what you must...
                    ErrorLogging.LogException(ex);
                }
            }
            DispatcherHelper.Initialize();
            TaskFactoryHelper.Initialize();

            base.OnStartup(e);
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG
            throw (Exception)e.ExceptionObject;
#else
            ErrorLogging.LogException(e.ExceptionObject as Exception);
            MessageBox.Show("An unhandled exception has occurred. You may continue using TEdit, but operation may be unstable until the application has been restarted.", "Unhandled Exception");
            // Current.Shutdown();
#endif
        }
    }
}
