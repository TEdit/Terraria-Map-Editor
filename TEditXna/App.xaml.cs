using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;

namespace TEditXna
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            BCCL.MvvmLight.Threading.DispatcherHelper.Initialize();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!DependencyChecker.VerifyDotNet())
            {
                MessageBox.Show("Please install .Net 4.0", "Missing .Net", MessageBoxButton.OK, MessageBoxImage.Stop);
                ErrorLogging.LogException(new ApplicationException("MISSING .NET"));
                Shutdown();
            }

            if (!DependencyChecker.VerifyXna())
            {
                MessageBox.Show("Please install XNA Framework 4.0", "Missing XNA", MessageBoxButton.OK, MessageBoxImage.Stop);
                ErrorLogging.LogException(new ApplicationException("MISSING XNA"));
                Shutdown();
            }

            if (!DependencyChecker.VerifyTerraria())
            {
                ErrorLogging.Log("Unable to locate Terraria. No texture data will be available.");
            }

            if (e.Args != null && e.Args.Count() > 0)
            {
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

            BCCL.MvvmLight.Threading.TaskFactoryHelper.Initialize();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            base.OnStartup(e);
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG
            throw (Exception)e.ExceptionObject;
#else
            ErrorLogging.LogException(e.ExceptionObject);
            MessageBox.Show("An unhandled exception has occured. Please copy the log from \"log.txt\" to the GitHub Issues list.\r\nThe program will now exit.", "Unhandled Exception");
            Current.Shutdown();
#endif
        }
    }
}
