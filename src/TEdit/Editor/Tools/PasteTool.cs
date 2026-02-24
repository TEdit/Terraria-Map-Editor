using System;
using System.Windows.Media.Imaging;
using TEdit.Editor.Clipboard;
using TEdit.ViewModel;
using TEdit.Geometry;
using TEdit.Render;
using TEdit.UI;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public sealed class PasteTool : BaseTool
{
    private enum PasteState { Idle, Floating, Dragging }

    private PasteState _state = PasteState.Idle;
    private Vector2Int32 _floatingAnchor;
    private Vector2Int32 _dragStartMouse;
    private Vector2Int32 _dragStartAnchor;

    private ClipboardBuffer _floatingBuffer;
    private ClipboardBufferPreview _floatingPreview;
    private bool _isSyncingToViewModel;

    public PasteTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/paste.png"));
        SymbolIcon = SymbolRegular.ClipboardPaste24;
        Name = "Paste";
        IsActive = false;
        ToolType = ToolType.Pixel;
    }

    public override bool IsFloatingPaste => _state != PasteState.Idle;
    public override Vector2Int32 FloatingPasteAnchor => _floatingAnchor;
    public override Vector2Int32 FloatingPasteSize => _floatingBuffer?.Size ?? default;

    public override void MouseDown(TileMouseState e)
    {
        var actions = GetActiveActions(e);
        if (!actions.Contains("editor.draw"))
            return;

        switch (_state)
        {
            case PasteState.Idle:
                if (_wvm.Clipboard.Buffer != null)
                {
                    _floatingBuffer = _wvm.Clipboard.Buffer.Buffer.Clone();
                    _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
                    _floatingAnchor = e.Location;
                    _state = PasteState.Floating;
                    SyncViewModelState();
                    _wvm.PreviewChange();
                }
                break;

            case PasteState.Floating:
                _dragStartMouse = e.Location;
                _dragStartAnchor = _floatingAnchor;
                _state = PasteState.Dragging;
                break;
        }
    }

    public override void MouseMove(TileMouseState e)
    {
        if (_state == PasteState.Dragging)
        {
            _floatingAnchor = new Vector2Int32(
                _dragStartAnchor.X + (e.Location.X - _dragStartMouse.X),
                _dragStartAnchor.Y + (e.Location.Y - _dragStartMouse.Y));
            SyncViewModelPosition();
        }
    }

    public override void MouseUp(TileMouseState e)
    {
        if (_state == PasteState.Dragging)
        {
            _state = PasteState.Floating;
        }
    }

    public override WriteableBitmap PreviewTool()
    {
        if (_state != PasteState.Idle && _floatingPreview != null)
        {
            PreviewScale = _floatingPreview.PreviewScale;
            return _floatingPreview.Preview;
        }

        if (_wvm.Clipboard.Buffer != null)
        {
            var preview = _wvm.Clipboard.Buffer.Preview;
            PreviewScale = _wvm.Clipboard.Buffer.PreviewScale;
            return preview;
        }
        return base.PreviewTool();
    }

    public override void AcceptPaste()
    {
        if (_state == PasteState.Idle || _floatingBuffer == null)
            return;

        PasteFloatingBuffer();
        ClearFloatingState();
        SyncViewModelState();
        _wvm.Clipboard.Buffer = null;
        _wvm.PreviewChange();
    }

    public override void CancelPaste()
    {
        if (_state == PasteState.Idle)
            return;

        ClearFloatingState();
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    public void RotateCW()
    {
        if (_state == PasteState.Idle || _floatingBuffer == null)
            return;

        _floatingBuffer = _floatingBuffer.Rotate();
        _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    public void RotateCCW()
    {
        if (_state == PasteState.Idle || _floatingBuffer == null)
            return;

        _floatingBuffer = _floatingBuffer.Rotate().Rotate().Rotate();
        _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    public void FlipH()
    {
        if (_state == PasteState.Idle || _floatingBuffer == null)
            return;

        _floatingBuffer = _floatingBuffer.FlipX();
        _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    public void FlipV()
    {
        if (_state == PasteState.Idle || _floatingBuffer == null)
            return;

        _floatingBuffer = _floatingBuffer.FlipY();
        _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    /// <summary>
    /// Sets the floating anchor position from the UI (e.g. textbox input).
    /// </summary>
    public void SetAnchor(int x, int y)
    {
        if (_state == PasteState.Idle || _isSyncingToViewModel) return;
        if (_floatingAnchor.X == x && _floatingAnchor.Y == y) return;
        _floatingAnchor = new Vector2Int32(x, y);
    }

    private void PasteFloatingBuffer()
    {
        _wvm.Clipboard.PasteBufferIntoWorld(_wvm.CurrentWorld, _floatingAnchor, _floatingBuffer);
        _wvm.UpdateRenderRegion(new RectangleInt32(_floatingAnchor, _floatingBuffer.Size));
        _wvm.SelectedChest = null;
        BlendRules.ResetUVCache(_wvm, _floatingAnchor.X, _floatingAnchor.Y,
            _floatingBuffer.Size.X, _floatingBuffer.Size.Y);
    }

    private void ClearFloatingState()
    {
        _floatingBuffer = null;
        _floatingPreview = null;
        _state = PasteState.Idle;
    }

    private void SyncViewModelState()
    {
        _wvm.IsPasteFloating = _state != PasteState.Idle;
        SyncViewModelPosition();
        var size = _floatingBuffer?.Size ?? default;
        _wvm.PasteSizeW = size.X;
        _wvm.PasteSizeH = size.Y;
    }

    private void SyncViewModelPosition()
    {
        _isSyncingToViewModel = true;
        _wvm.PasteAnchorX = _floatingAnchor.X;
        _wvm.PasteAnchorY = _floatingAnchor.Y;
        _isSyncingToViewModel = false;
    }
}
