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