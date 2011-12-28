using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.MvvmLight;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public class ArrowTool : ObservableObject, ITool
    {
        private WriteableBitmap _preview;

        private bool _isActive;

        public ArrowTool(WorldViewModel worldViewModel)
        {
            _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);

            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/cursor.png"));
            Name = "Arrow";
            IsActive = false;
        }

        public string Name { get; private set; }

        public ToolType ToolType { get { return ToolType.Pixel; } }

        public BitmapImage Icon { get; private set; }

        public bool IsActive
        {
            get { return _isActive; }
            set { Set("IsActive", ref _isActive, value); }
        }

        public void MouseDown(TileMouseState e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                // Check chests and signs and show popup
            }
        }

        public void MouseMove(TileMouseState e)
        {
        }

        public void MouseUp(TileMouseState e)
        {

        }

        public void MouseWheel(TileMouseState e)
        {
        }

        public WriteableBitmap PreviewTool()
        {
            return _preview;
        }
    }
}