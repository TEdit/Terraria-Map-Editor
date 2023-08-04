using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework;
using TEdit.ViewModel;
using TEdit.Geometry;
using TEdit.Render;
using TEdit.UI;
using TEdit.Editor.Clipboard;

namespace TEdit.Editor.Tools;

public sealed class PasteTool : BaseTool
{
    public PasteTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/paste.png"));
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
            var preview = _wvm.Clipboard.Buffer.Preview;
            PreviewScale = _wvm.Clipboard.Buffer.PreviewScale;
            return preview;
        }
        return base.PreviewTool();
    }

    private void PasteClipboard(Vector2Int32 anchor)
    {
        ErrorLogging.TelemetryClient?.TrackEvent("Paste");

        _wvm.Clipboard.PasteBufferIntoWorld(_wvm.CurrentWorld, anchor);
        _wvm.UpdateRenderRegion(new RectangleInt32(anchor, _wvm.Clipboard.Buffer.Buffer.Size));

        /* Heathtech */
        BlendRules.ResetUVCache(_wvm, anchor.X, anchor.Y, _wvm.Clipboard.Buffer.Buffer.Size.X, _wvm.Clipboard.Buffer.Buffer.Size.Y);
    }
}
