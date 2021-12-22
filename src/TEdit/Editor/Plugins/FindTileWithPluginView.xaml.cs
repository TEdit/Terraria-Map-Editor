using System.Windows;

namespace TEdit.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for ReplaceAllPlugin.xaml
    /// </summary>
    public partial class FindTileWithPluginView : Window
    {
        public string BlockToFind { get; private set; }
        public string WallToFind { get; private set; }
        public string SpriteToFind { get; private set; }
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
            BlockToFind = BlockLookup.Text;
            WallToFind = WallLookup.Text;
            Close();
        }
    }
}
