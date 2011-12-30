using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.MvvmLight;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public class FillTool : ObservableObject, ITool
    {
        private WorldViewModel _wvm;
        private WriteableBitmap _preview;

        private bool _isActive;

        public FillTool(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);

            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/paintcan.png"));
            Name = "Fill";
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

        public bool PreviewIsTexture { get { return false; } }
    }
}