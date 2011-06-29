using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Windows;

namespace TEdit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private CompositionContainer _container;

        [Import("MainWindow")]
        public new Window MainWindow
        {
            get { return base.MainWindow; }
            set { base.MainWindow = value; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
#if DEBUG
            // Don't trap unhandled exceptions in debug mode
#else
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //this.DispatcherUnhandledException += App_DispatcherUnhandledException;
#endif

            base.OnStartup(e);

            if (Compose())
            {
                MainWindow.Show();
            }
            else
            {
                Shutdown();
            }
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
#if DEBUG
            // Don't trap unhandled exceptions in debug mode
#else
            TEdit.Common.ErrorLogging.LogException(e.ExceptionObject, Common.ErrorLogging.ErrorLevel.Fatal);
            MessageBox.Show("An unhandled exception has occured. Please copy the log from \"log.txt\" to the GitHub Issues list.\r\nThe program will now exit.", "Unhandled Exception");
            Application.Current.Shutdown();
#endif
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (_container != null)
            {
                _container.Dispose();
            }
        }

        private bool Compose()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(IEmailService).Assembly));

            _container = new CompositionContainer(catalog);
            var batch = new CompositionBatch();
            batch.AddPart(this);

            try
            {
                _container.Compose(batch);
            }
            catch (CompositionException compositionException)
            {
                MessageBox.Show(compositionException.ToString());
                Shutdown(1);
                return false;
            }
            return true;
        }
    }
}