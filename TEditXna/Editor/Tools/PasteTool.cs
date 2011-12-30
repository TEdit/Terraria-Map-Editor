using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using Microsoft.Xna.Framework;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public class PasteTool : ObservableObject, ITool
    {
        private WorldViewModel _wvm;
        private WriteableBitmap _preview;

        private bool _isActive;

        public PasteTool(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);

            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/paste.png"));
            Name = "Paste";
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

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                PasteClipboard(e.Location);
            }
            if (e.RightButton == MouseButtonState.Pressed && e.LeftButton == MouseButtonState.Released)
            {
                _wvm.Clipboard.Buffer = null;
                _wvm.PreviewChange();
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
            if (_wvm.Clipboard.Buffer != null)
                return _wvm.Clipboard.Buffer.Preview;

            return _preview;
        }

        private void PasteClipboard(Vector2Int32 anchor)
        {
            _wvm.Clipboard.PasteBufferIntoWorld(anchor);
            _wvm.UpdateRenderRegion(new Rectangle(anchor.X, anchor.Y, _wvm.Clipboard.Buffer.Size.X, _wvm.Clipboard.Buffer.Size.Y));
        }

        public bool PreviewIsTexture { get { return false; } }
    }
}