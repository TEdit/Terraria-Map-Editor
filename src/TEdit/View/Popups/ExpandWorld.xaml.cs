using System.Windows;

namespace TEdit.View.Popups
{
    public partial class ExpandWorldView : Window
    {
        public int _minWidth { get; }
        public int _maxWidth { get; }
        public int _minHeight { get; }
        public int _maxHeight { get; }
        public int _newWidth  { get; set; }
        public int _newHeight { get; set; }

        public ExpandWorldView(int currentWidth, int currentHeight)
        {
            InitializeComponent();

            _minWidth  = currentWidth;
            _minHeight = currentHeight;
            _maxWidth  = 8400;
            _maxHeight = 2400;

            // start sliders at current size
            _newWidth = currentWidth;
            _newHeight = currentHeight;

            DataContext = this;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
