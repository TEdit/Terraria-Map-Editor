using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
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
            GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();
        }

        public static FileVersionInfo Version { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            ErrorLogging.Initialize();
            ErrorLogging.Log(string.Format("Starting TEdit {0}",ErrorLogging.Version));
            ErrorLogging.Log(string.Format("OS: {0}",Environment.OSVersion));

            Assembly asm = Assembly.GetExecutingAssembly();
            Version = FileVersionInfo.GetVersionInfo(asm.Location);

            if (!DependencyChecker.VerifyDotNet())
            {
                MessageBox.Show("Please install .Net 4.0", "Missing .Net", MessageBoxButton.OK, MessageBoxImage.Stop);
                ErrorLogging.LogException(new ApplicationException("MISSING .NET"));
                Shutdown();
            }
            else
            {
                ErrorLogging.Log(".Net >= 4.0");
            }

            if (!DependencyChecker.VerifyTerraria())
            {
                ErrorLogging.Log("Unable to locate Terraria. No texture data will be available.");
            }
            else
            {
                ErrorLogging.Log(string.Format("Terraria Data Path: {0}", DependencyChecker.PathToContent));
            }

            if (e.Args != null && e.Args.Count() > 0)
            {
                ErrorLogging.Log(string.Format("Command Line Open: {0}", e.Args[0]));
                this.Properties["OpenFile"] = e.Args[0];
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

                    this.Properties["OpenFile"] = fname;
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
