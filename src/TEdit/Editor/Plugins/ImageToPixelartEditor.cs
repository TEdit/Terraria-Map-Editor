using TEdit.ViewModel;
using System.Windows;

namespace TEdit.Editor.Plugins
{
    public class ImageToPixelartEditor : BasePlugin
    {
        // Private field to keep a reference to the ImageToPixelartEditorView window.
        private ImageToPixelartEditorView _view;

        // Constructor for ImageToPixelartEditor.
        public ImageToPixelartEditor(WorldViewModel worldViewModel) : base(worldViewModel)
        {
            Name = "Image To Pixelart Editor";
        }

        // Method to execute the plugin functionality.
        public override void Execute()
        {
            // No need to wait for world load due to the nature of this plugin.
            // if (_wvm.CurrentWorld == null) return;

            // Initialize and show the ImageToPixelartEditorView window as a non-modal window.
            _view = new ImageToPixelartEditorView(_wvm); // Pass down "_wvm" instance to the new window.
            _view.Show(); // Open as non-modal window.

            // Subscribe to the main window's Closed event.
            Application.Current.MainWindow.Closed += MainWindow_Closed;
        }

        // Event handler for the main window's Closed event.
        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            // Close the ImageToPixelartEditorView window if it is open.
            _view?.Close();
        }
    }
}
