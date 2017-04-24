using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using TEdit.MvvmLight.Threading;
using DispatcherHelper = GalaSoft.MvvmLight.Threading.DispatcherHelper;

namespace TEditXna
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }

        public static FileVersionInfo Version { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            ErrorLogging.Initialize();
            ErrorLogging.Log($"Starting TEdit {ErrorLogging.Version}");
            ErrorLogging.Log($"OS: {Environment.OSVersion}");

            Assembly asm = Assembly.GetExecutingAssembly();
            Version = FileVersionInfo.GetVersionInfo(asm.Location);

            try
            {
                int directxMajorVersion = DependencyChecker.GetDirectxMajorVersion();
                if (directxMajorVersion < 11)
                {
                    ErrorLogging.Log($"DirectX {directxMajorVersion} unsupported. DirectX 11 or higher is required.");
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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            base.OnStartup(e);
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG
            throw (Exception)e.ExceptionObject;
#else
            ErrorLogging.LogException(e.ExceptionObject);
            MessageBox.Show(string.Format("An unhandled exception has occured. Please copy the log from:\r\n{0}\r\n to the GitHub Issues list.\r\nThe program will now exit.", ErrorLogging.LogFilePath), "Unhandled Exception");
            Current.Shutdown();
#endif
        }
    }
}
