using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TerrariaMapEditor
{
    static class Program
    {
        const string url = @"http://github.com/BinaryConstruct/Terraria-Map-Editor/issues";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SetExceptionHandling();

            //try
            //{
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormMain());
            //}
            //catch (Exception err)
            //{
            //    TerrariaMapEditor.Common.ErrorLogging.LogException(err, Common.ErrorLogging.ErrorLevel.Fatal);
            //    MessageBox.Show(String.Format("An unhandled exception has occured. Please copy the log from {0} to {1}.\r\nThe program will now exit.", Properties.Settings.Default.LogFile, url), "Unhandled Exception", MessageBoxButtons.OK);
            //    Application.Exit();
            //}

        }

        [System.Diagnostics.Conditional("RELEASE")]
        static void SetExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            TerrariaMapEditor.Common.ErrorLogging.LogException(e.Exception, Common.ErrorLogging.ErrorLevel.Fatal);
            MessageBox.Show(String.Format("An unhandled exception has occured. Please copy the log from {0} to {1}.\r\nThe program will now exit.", Properties.Settings.Default.LogFile, url), "Unhandled Exception", MessageBoxButtons.OK);
            Application.Exit();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            TerrariaMapEditor.Common.ErrorLogging.LogException(e.ExceptionObject as Exception, Common.ErrorLogging.ErrorLevel.Fatal);
            MessageBox.Show(String.Format("An unhandled exception has occured. Please copy the log from {0} to {1}.\r\nThe program will now exit.", Properties.Settings.Default.LogFile, url), "Unhandled Exception", MessageBoxButtons.OK);
            Application.Exit();
        }

        static void HandleException(Exception e)
        {
            TerrariaMapEditor.Common.ErrorLogging.LogException(e, Common.ErrorLogging.ErrorLevel.Fatal);
            MessageBox.Show(String.Format("An unhandled exception has occured. Please copy the log from {0} to {1}.\r\nThe program will now exit.", Properties.Settings.Default.LogFile, url), "Unhandled Exception", MessageBoxButtons.OK);
            Application.Exit();
        }
       
    }
}
