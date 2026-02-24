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
    private enum PasteState { Idle, Floating, Dragging, Resizing }

    private enum ResizeEdge { None, TopLeft, Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left }

    private PasteState _state = PasteState.Idle;
    private Vector2Int32 _floatingAnchor;
    private Vector2Int32 _dragStartMouse;
    private Vector2Int32 _dragStartAnchor;

    private ClipboardBuffer _floatingBuffer;
    private ClipboardBufferPreview _floatingPreview;
    private bool _isSyncingToViewModel;

    // Resize state
    private ClipboardBuffer _originalBuffer;     // Pre-resize snapshot for clean resampling
    private ResizeEdge _resizeEdge;              // Which handle is being dragged
    private Vector2Int32 _resizeStartMouse;      // Mouse pos at resize start
    private Vector2Int32 _resizeStartAnchor;     // Anchor at resize start
    private Vector2Int32 _resizeStartSize;       // Size at resize start

    private const int HandleHitSize = 2; // tiles

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
                var edge = GetResizeEdge(e.Location);
                if (edge != ResizeEdge.None)
                {
                    _resizeEdge = edge;
                    _resizeStartMouse = e.Location;
                    _resizeStartAnchor = _floatingAnchor;
                    _resizeStartSize = _floatingBuffer.Size;
                    if (_originalBuffer == null)
                        _originalBuffer = _floatingBuffer.Clone();
                    _state = PasteState.Resizing;
                }
                else
                {
                    _dragStartMouse = e.Location;
                    _dragStartAnchor = _floatingAnchor;
                    _state = PasteState.Dragging;
                }
                break;
        }
    }

    public override void MouseMove(TileMouseState e)
    {
        switch (_state)
        {
            case PasteState.Dragging:
                _floatingAnchor = new Vector2Int32(
                    _dragStartAnchor.X + (e.Location.X - _dragStartMouse.X),
                    _dragStartAnchor.Y + (e.Location.Y - _dragStartMouse.Y));
                SyncViewModelPosition();
                break;

            case PasteState.Resizing:
                var (newAnchor, newSize) = CalcResize(e.Location);
                if (newSize.X != _floatingBuffer.Size.X || newSize.Y != _floatingBuffer.Size.Y)
                {
                    _floatingAnchor = newAnchor;
                    _floatingBuffer = _originalBuffer.Resize(newSize.X, newSize.Y);
                    _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
                    SyncViewModelState();
                    _wvm.PreviewChange();
                }
                break;
        }
    }

    public override void MouseUp(TileMouseState e)
    {
        if (_state == PasteState.Dragging || _state == PasteState.Resizing)
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
        _originalBuffer = null; // Re-snapshot on next resize
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    public void RotateCCW()
    {
        if (_state == PasteState.Idle || _floatingBuffer == null)
            return;

        _floatingBuffer = _floatingBuffer.Rotate().Rotate().Rotate();
        _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
        _originalBuffer = null; // Re-snapshot on next resize
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    public void FlipH()
    {
        if (_state == PasteState.Idle || _floatingBuffer == null)
            return;

        _floatingBuffer = _floatingBuffer.FlipX();
        _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
        _originalBuffer = null; // Re-snapshot on next resize
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    public void FlipV()
    {
        if (_state == PasteState.Idle || _floatingBuffer == null)
            return;

        _floatingBuffer = _floatingBuffer.FlipY();
        _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
        _originalBuffer = null; // Re-snapshot on next resize
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    public override CursorHint GetCursorHint(Vector2Int32 tilePos)
    {
        if (_state == PasteState.Resizing)
            return EdgeToCursorHint(_resizeEdge);

        if (_state == PasteState.Dragging)
            return CursorHint.Move;

        if (_state == PasteState.Floating)
        {
            var edge = GetResizeEdge(tilePos);
            if (edge != ResizeEdge.None)
                return EdgeToCursorHint(edge);

            // Inside the paste bounds = move
            if (_floatingBuffer != null &&
                tilePos.X >= _floatingAnchor.X && tilePos.X < _floatingAnchor.X + _floatingBuffer.Size.X &&
                tilePos.Y >= _floatingAnchor.Y && tilePos.Y < _floatingAnchor.Y + _floatingBuffer.Size.Y)
                return CursorHint.Move;
        }

        return CursorHint.Default;
    }

    private static CursorHint EdgeToCursorHint(ResizeEdge edge) => edge switch
    {
        ResizeEdge.Top or ResizeEdge.Bottom => CursorHint.SizeNS,
        ResizeEdge.Left or ResizeEdge.Right => CursorHint.SizeWE,
        ResizeEdge.TopLeft or ResizeEdge.BottomRight => CursorHint.SizeNWSE,
        ResizeEdge.TopRight or ResizeEdge.BottomLeft => CursorHint.SizeNESW,
        _ => CursorHint.Default,
    };

    /// <summary>
    /// Sets the floating anchor position from the UI (e.g. textbox input).
    /// </summary>
    public void SetAnchor(int x, int y)
    {
        if (_state == PasteState.Idle || _isSyncingToViewModel) return;
        if (_floatingAnchor.X == x && _floatingAnchor.Y == y) return;
        _floatingAnchor = new Vector2Int32(x, y);
    }

    /// <summary>
    /// Resizes the floating buffer from the UI (e.g. size textbox input).
    /// </summary>
    public void ResizeFromUI(int w, int h)
    {
        if (_state == PasteState.Idle || _isSyncingToViewModel) return;
        if (_floatingBuffer == null) return;
        if (w <= 0 || h <= 0) return;
        if (w == _floatingBuffer.Size.X && h == _floatingBuffer.Size.Y) return;

        if (_originalBuffer == null)
            _originalBuffer = _floatingBuffer.Clone();

        _floatingBuffer = _originalBuffer.Resize(w, h);
        _floatingPreview = new ClipboardBufferPreview(_floatingBuffer);
        SyncViewModelState();
        _wvm.PreviewChange();
    }

    private ResizeEdge GetResizeEdge(Vector2Int32 mousePos)
    {
        if (_floatingBuffer == null) return ResizeEdge.None;

        int left = _floatingAnchor.X;
        int top = _floatingAnchor.Y;
        int right = left + _floatingBuffer.Size.X;
        int bottom = top + _floatingBuffer.Size.Y;
        int midX = (left + right) / 2;
        int midY = (top + bottom) / 2;

        int mx = mousePos.X;
        int my = mousePos.Y;

        bool nearLeft = Math.Abs(mx - left) <= HandleHitSize;
        bool nearRight = Math.Abs(mx - right) <= HandleHitSize;
        bool nearTop = Math.Abs(my - top) <= HandleHitSize;
        bool nearBottom = Math.Abs(my - bottom) <= HandleHitSize;
        bool nearMidX = Math.Abs(mx - midX) <= HandleHitSize;
        bool nearMidY = Math.Abs(my - midY) <= HandleHitSize;

        // Corner handles have priority
        if (nearLeft && nearTop) return ResizeEdge.TopLeft;
        if (nearRight && nearTop) return ResizeEdge.TopRight;
        if (nearLeft && nearBottom) return ResizeEdge.BottomLeft;
        if (nearRight && nearBottom) return ResizeEdge.BottomRight;

        // Edge midpoint handles
        if (nearMidX && nearTop) return ResizeEdge.Top;
        if (nearMidX && nearBottom) return ResizeEdge.Bottom;
        if (nearLeft && nearMidY) return ResizeEdge.Left;
        if (nearRight && nearMidY) return ResizeEdge.Right;

        return ResizeEdge.None;
    }

    private (Vector2Int32 anchor, Vector2Int32 size) CalcResize(Vector2Int32 mousePos)
    {
        int deltaX = mousePos.X - _resizeStartMouse.X;
        int deltaY = mousePos.Y - _resizeStartMouse.Y;

        int newW = _resizeStartSize.X;
        int newH = _resizeStartSize.Y;
        int anchorX = _resizeStartAnchor.X;
        int anchorY = _resizeStartAnchor.Y;

        switch (_resizeEdge)
        {
            case ResizeEdge.Right:
                newW = Math.Max(1, _resizeStartSize.X + deltaX);
                break;
            case ResizeEdge.Bottom:
                newH = Math.Max(1, _resizeStartSize.Y + deltaY);
                break;
            case ResizeEdge.Left:
                newW = Math.Max(1, _resizeStartSize.X - deltaX);
                anchorX = _resizeStartAnchor.X + (_resizeStartSize.X - newW);
                break;
            case ResizeEdge.Top:
                newH = Math.Max(1, _resizeStartSize.Y - deltaY);
                anchorY = _resizeStartAnchor.Y + (_resizeStartSize.Y - newH);
                break;
            case ResizeEdge.BottomRight:
                newW = Math.Max(1, _resizeStartSize.X + deltaX);
                newH = Math.Max(1, _resizeStartSize.Y + deltaY);
                break;
            case ResizeEdge.BottomLeft:
                newW = Math.Max(1, _resizeStartSize.X - deltaX);
                newH = Math.Max(1, _resizeStartSize.Y + deltaY);
                anchorX = _resizeStartAnchor.X + (_resizeStartSize.X - newW);
                break;
            case ResizeEdge.TopRight:
                newW = Math.Max(1, _resizeStartSize.X + deltaX);
                newH = Math.Max(1, _resizeStartSize.Y - deltaY);
                anchorY = _resizeStartAnchor.Y + (_resizeStartSize.Y - newH);
                break;
            case ResizeEdge.TopLeft:
                newW = Math.Max(1, _resizeStartSize.X - deltaX);
                newH = Math.Max(1, _resizeStartSize.Y - deltaY);
                anchorX = _resizeStartAnchor.X + (_resizeStartSize.X - newW);
                anchorY = _resizeStartAnchor.Y + (_resizeStartSize.Y - newH);
                break;
        }

        return (new Vector2Int32(anchorX, anchorY), new Vector2Int32(newW, newH));
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
        _originalBuffer = null;
        _state = PasteState.Idle;
    }

    private void SyncViewModelState()
    {
        _isSyncingToViewModel = true;
        _wvm.IsPasteFloating = _state != PasteState.Idle;
        SyncViewModelPosition();
        var size = _floatingBuffer?.Size ?? default;
        _wvm.PasteSizeW = size.X;
        _wvm.PasteSizeH = size.Y;
        _isSyncingToViewModel = false;
    }

    private void SyncViewModelPosition()
    {
        _isSyncingToViewModel = true;
        _wvm.PasteAnchorX = _floatingAnchor.X;
        _wvm.PasteAnchorY = _floatingAnchor.Y;
        _isSyncingToViewModel = false;
    }
}
