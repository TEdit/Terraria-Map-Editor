using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using Microsoft.Xna.Framework;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public sealed class PasteTool : BaseTool
    {
        public PasteTool(WorldViewModel worldViewModel) : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/paste.png"));
            Name = "Paste";
            IsActive = false;
            ToolType = ToolType.Pixel;
        }

        public override void MouseDown(TileMouseState e)
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

        public override WriteableBitmap PreviewTool()
        {
            if (_wvm.Clipboard.Buffer != null)
                return _wvm.Clipboard.Buffer.Preview;

            return base.PreviewTool();
        }

        private void PasteClipboard(Vector2Int32 anchor)
        {
            _wvm.Clipboard.PasteBufferIntoWorld(anchor);
            _wvm.UpdateRenderRegion(new Rectangle(anchor.X, anchor.Y, _wvm.Clipboard.Buffer.Size.X, _wvm.Clipboard.Buffer.Size.Y));
        }
    }
}