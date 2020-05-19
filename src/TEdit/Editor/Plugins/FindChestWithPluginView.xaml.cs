using System.Windows;

namespace TEditXna.Editor.Plugins
{
    /// <summary>
    /// Interaction logic for ReplaceAllPlugin.xaml
    /// </summary>
    public partial class FindChestWithPluginView : Window
    {
        public string ItemToFind { get; private set; }
        public FindChestWithPluginView()
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
            ItemToFind = ItemLookup.Text;
            Close();
        }
    }
}
