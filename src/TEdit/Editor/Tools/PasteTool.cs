using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using Microsoft.Xna.Framework;
using TEditXna.ViewModel;
using TEditXna.Terraria.Objects;

namespace TEditXna.Editor.Tools
{
    public sealed class PasteTool : BaseTool
    {
        public PasteTool(WorldViewModel worldViewModel)
            : base(worldViewModel)
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
                if (_wvm.Clipboard.Buffer != null)
                    PasteClipboard(e.Location);
            }
            if (e.RightButton == MouseButtonState.Pressed && e.LeftButton == MouseButtonState.Released)
            {
               
                _wvm.SetTool.Execute(_wvm.Tools.FirstOrDefault(t => t.Name == "Arrow"));
            }
        }

        public override WriteableBitmap PreviewTool()
        {
            if (_wvm.Clipboard.Buffer != null)
            {
                PreviewScale = _wvm.Clipboard.Buffer.RenderScale;
                return _wvm.Clipboard.Buffer.Preview;
            }
            return base.PreviewTool();
        }

        private void PasteClipboard(Vector2Int32 anchor)
        {
            _wvm.Clipboard.PasteBufferIntoWorld(anchor);
            _wvm.UpdateRenderRegion(new Rectangle(anchor.X, anchor.Y, _wvm.Clipboard.Buffer.Size.X, _wvm.Clipboard.Buffer.Size.Y));

            /* Heathtech */
            BlendRules.ResetUVCache(_wvm, anchor.X, anchor.Y, _wvm.Clipboard.Buffer.Size.X, _wvm.Clipboard.Buffer.Size.Y);
        }
    }
}