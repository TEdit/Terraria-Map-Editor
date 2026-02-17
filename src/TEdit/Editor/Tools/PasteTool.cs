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
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public sealed class PasteTool : BaseTool
{
    public PasteTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/paste.png"));
        SymbolIcon = SymbolRegular.ClipboardPaste24;
        Name = "Paste";
        IsActive = false;
        ToolType = ToolType.Pixel;
    }

    public override void MouseDown(TileMouseState e)
    {
        var actions = GetActiveActions(e);
        if (actions.Contains("editor.draw"))
        {
            if (_wvm.Clipboard.Buffer != null)
                PasteClipboard(e.Location);
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
        _wvm.Clipboard.PasteBufferIntoWorld(_wvm.CurrentWorld, anchor);
        _wvm.UpdateRenderRegion(new RectangleInt32(anchor, _wvm.Clipboard.Buffer.Buffer.Size));

        // Clear selected chest to prevent rendering "open" offset on pasted chest tiles
        _wvm.SelectedChest = null;

        /* Heathtech */
        BlendRules.ResetUVCache(_wvm, anchor.X, anchor.Y, _wvm.Clipboard.Buffer.Buffer.Size.X, _wvm.Clipboard.Buffer.Buffer.Size.Y);
    }
}
