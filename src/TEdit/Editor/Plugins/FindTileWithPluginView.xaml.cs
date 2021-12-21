using System.Windows;

namespace TEdit.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for ReplaceAllPlugin.xaml
    /// </summary>
    public partial class FindTileWithPluginView : Window
    {
        public string TileToFind { get; private set; }
        public FindTileWithPluginView()
        {
            InitializeComponent();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            TileToFind = TileLookup.Text;
            Close();
        }
    }
}
