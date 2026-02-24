using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.ViewModel;
using TEdit.Render;
using TEdit.UI;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public sealed class PencilTool : BaseTool
{
    private bool _isDrawing;
    private bool _isConstraining;
    private bool _isLineMode;
    private int _constrainDirection; // 0=horizontal, 1=vertical, 2=diagonal
    private bool _constrainDirectionLocked;
    private Vector2Int32 _anchorPoint;
    private Vector2Int32 _startPoint;
    private Vector2Int32 _endPoint;
    private Vector2Int32 _lineMouseDownPos;
    private bool _lineLastWasClick;

    // CAD wire routing state
    private bool _isCadWireMode;
    private bool _isCadAnchored;
    private WireRoutingMode _cadRoutingMode = WireRoutingMode.Elbow90;
    private bool? _cadVerticalFirstOverride; // null = auto-detect, true = vertical-first, false = horizontal-first
    private Vector2Int32 _cadAnchor;
    private readonly List<Vector2Int32> _cadPreviewPath = new();
    private Vector2Int32 _cadLastCursor;

    public PencilTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/pencil.png"));
        SymbolIcon = SymbolRegular.Edit24;
        Name = "Pencil";
        IsActive = false;
        ToolType = ToolType.Pixel;
    }

    /// <summary>Whether CAD wire routing mode is active.</summary>
    public bool IsCadWireMode => _isCadWireMode;

    /// <summary>Current CAD routing style.</summary>
    public WireRoutingMode CadRoutingMode => _cadRoutingMode;

    /// <summary>Current H/V override: null=auto, true=vertical-first, false=horizontal-first.</summary>
    public bool? CadVerticalFirstOverride => _cadVerticalFirstOverride;

    public override IReadOnlyList<Vector2Int32> CadPreviewPath => _cadPreviewPath;
    public override bool HasCadPreview => _isCadWireMode && _isCadAnchored && _cadPreviewPath.Count > 0;

    /// <summary>
    /// Cycle wire mode: Off → Wire90 → Wire45 → Off (normal) → Wire90...
    /// Anchor is preserved when cycling through Off/normal.
    /// </summary>
    public void CycleWireMode()
    {
        if (!_isCadWireMode)
        {
            _isCadWireMode = true;
            _cadRoutingMode = WireRoutingMode.Elbow90;
            // Recompute preview if anchor was preserved from previous cycle
            if (_isCadAnchored)
                ForceUpdateCadPreview();
        }
        else if (_cadRoutingMode == WireRoutingMode.Elbow90)
        {
            _cadRoutingMode = WireRoutingMode.Miter45;
            if (_isCadAnchored)
                ForceUpdateCadPreview();
        }
        else
        {
            // Miter45 → normal drawing (preserve anchor, just hide preview)
            _isCadWireMode = false;
            _cadPreviewPath.Clear();
        }
    }

    /// <summary>Set wire mode state directly (used for syncing between tools).</summary>
    public void SetWireState(bool enabled, WireRoutingMode mode, bool? verticalFirst)
    {
        _isCadWireMode = enabled;
        _cadRoutingMode = mode;
        _cadVerticalFirstOverride = verticalFirst;
        if (!enabled)
            _cadPreviewPath.Clear();
    }

    /// <summary>Exit CAD wire mode entirely. Clears anchor and preview.</summary>
    public void ExitCadWireMode()
    {
        _isCadWireMode = false;
        CancelCadWire();
    }

    /// <summary>Toggle H/V direction: auto → horizontal-first → vertical-first → auto.</summary>
    public void ToggleVerticalFirst()
    {
        if (_cadVerticalFirstOverride == null)
            _cadVerticalFirstOverride = false; // horizontal-first
        else if (_cadVerticalFirstOverride == false)
            _cadVerticalFirstOverride = true;  // vertical-first
        else
            _cadVerticalFirstOverride = null;  // auto-detect

        if (_isCadAnchored)
            ForceUpdateCadPreview();
    }

    /// <summary>Cancel the current CAD wire segment (anchor + preview). Stays in CAD mode.</summary>
    public void CancelCadWire()
    {
        _isCadAnchored = false;
        _cadPreviewPath.Clear();
    }

    public override void MouseDown(TileMouseState e)
    {
        // CAD wire mode intercept
        if (_isCadWireMode)
        {
            var actions = GetActiveActions(e);

            // Right-click cancels anchor
            if (actions.Contains("editor.secondary"))
            {
                CancelCadWire();
                return;
            }

            if (actions.Contains("editor.draw") || actions.Contains("editor.draw.line") || actions.Contains("editor.draw.constrain"))
            {
                if (!_isCadAnchored)
                {
                    // First click: set anchor, no drawing
                    _cadAnchor = e.Location;
                    _isCadAnchored = true;
                    _cadPreviewPath.Clear();
                    return;
                }
                else
                {
                    // Second click: commit the preview path
                    CommitCadPath();
                    // Chain: new anchor = old end for polyline
                    _cadAnchor = e.Location;
                    _cadPreviewPath.Clear();
                    return;
                }
            }
        }

        // Original pencil behavior
        var originalActions = GetActiveActions(e);

        if (!_isDrawing && !_isConstraining && !_isLineMode)
        {
            _anchorPoint = e.Location;
            _lineMouseDownPos = e.Location;
            _constrainDirectionLocked = false;

            // Continue polyline from previous endpoint if last line op was a click
            if (_lineLastWasClick && originalActions.Contains("editor.draw.line"))
            {
                _startPoint = _endPoint;
            }
            else
            {
                _startPoint = e.Location;
                _lineLastWasClick = false;
            }

            int totalTiles = _wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh;
            if (_wvm.CheckTiles == null || _wvm.CheckTiles.Length != totalTiles)
                _wvm.CheckTiles = new int[totalTiles];
            if (++_wvm.CheckTileGeneration <= 0)
            {
                _wvm.CheckTileGeneration = 1;
                Array.Clear(_wvm.CheckTiles, 0, _wvm.CheckTiles.Length);
            }
        }

        _isDrawing = originalActions.Contains("editor.draw");
        _isConstraining = originalActions.Contains("editor.draw.constrain");
        _isLineMode = originalActions.Contains("editor.draw.line");

        ProcessDraw(e.Location);
    }

    public override void MouseMove(TileMouseState e)
    {
        // CAD wire mode: update preview path
        if (_isCadWireMode && _isCadAnchored)
        {
            UpdateCadPreview(e.Location);
            return;
        }

        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");

        ProcessDraw(e.Location);
    }

    public override void MouseUp(TileMouseState e)
    {
        // CAD wire mode: clicks are handled in MouseDown, no action on MouseUp
        if (_isCadWireMode && _isCadAnchored)
            return;

        // Detect click vs drag for polyline continuation
        if (_isLineMode)
        {
            int dx = Math.Abs(e.Location.X - _lineMouseDownPos.X);
            int dy = Math.Abs(e.Location.Y - _lineMouseDownPos.Y);
            _lineLastWasClick = (dx <= 2 && dy <= 2);
        }
        else
        {
            _lineLastWasClick = false;
        }

        ProcessDraw(e.Location);

        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");
        _constrainDirectionLocked = false;

        _wvm.UndoManager.SaveUndo();
    }

    private void UpdateCadPreview(Vector2Int32 cursor)
    {
        // Skip recomputation if cursor tile hasn't changed
        if (_cadLastCursor.X == cursor.X && _cadLastCursor.Y == cursor.Y && _cadPreviewPath.Count > 0)
            return;

        _cadLastCursor = cursor;
        ComputeCadPath(cursor);
    }

    /// <summary>Force recompute even if cursor hasn't changed (used when mode/direction changes).</summary>
    private void ForceUpdateCadPreview()
    {
        ComputeCadPath(_cadLastCursor);
    }

    private void ComputeCadPath(Vector2Int32 cursor)
    {
        _cadPreviewPath.Clear();

        bool verticalFirst = _cadVerticalFirstOverride
            ?? WireRouter.DetectVerticalFirst(_cadAnchor, cursor);
        var points = _cadRoutingMode == WireRoutingMode.Elbow90
            ? WireRouter.Route90(_cadAnchor, cursor, verticalFirst)
            : WireRouter.RouteMiter(_cadAnchor, cursor, verticalFirst);

        _cadPreviewPath.AddRange(points);
    }

    private void CommitCadPath()
    {
        if (_cadPreviewPath.Count == 0) return;

        int totalTiles = _wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh;
        if (_wvm.CheckTiles == null || _wvm.CheckTiles.Length != totalTiles)
            _wvm.CheckTiles = new int[totalTiles];
        if (++_wvm.CheckTileGeneration <= 0)
        {
            _wvm.CheckTileGeneration = 1;
            Array.Clear(_wvm.CheckTiles, 0, _wvm.CheckTiles.Length);
        }

        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;

        foreach (var pixel in _cadPreviewPath)
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

            int index = pixel.X + pixel.Y * tilesWide;
            if (_wvm.CheckTiles[index] != generation)
            {
                _wvm.CheckTiles[index] = generation;
                if (_wvm.Selection.IsValid(pixel))
                {
                    _wvm.UndoManager.SaveTile(pixel);
                    _wvm.SetPixel(pixel.X, pixel.Y);
                    BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                }
            }
        }

        _wvm.UndoManager.SaveUndo();
    }

    private void ProcessDraw(Vector2Int32 tile)
    {
        Vector2Int32 p = tile;

        if (_isConstraining)
        {
            if (!_constrainDirectionLocked)
            {
                int dx = Math.Abs(tile.X - _anchorPoint.X);
                int dy = Math.Abs(tile.Y - _anchorPoint.Y);
                if (dx > 1 || dy > 1)
                {
                    _constrainDirection = ConstrainHelper.DetectDirection(dx, dy);
                    _constrainDirectionLocked = true;
                }
            }

            p = ConstrainHelper.Snap(tile, _anchorPoint, _constrainDirection);

            // Platform stair direction from constrained diagonal
            if (_wvm.TilePicker.PaintMode == PaintMode.Platform && _constrainDirection == 2)
            {
                int sx = Math.Sign(p.X - _anchorPoint.X);
                int sy = Math.Sign(p.Y - _anchorPoint.Y);
                // up-right → stair-right, up-left → stair-left, going down → flat
                if (sy < 0 && sx > 0) _wvm.TilePicker.PlatformStairDirection = 1;
                else if (sy < 0 && sx < 0) _wvm.TilePicker.PlatformStairDirection = -1;
                else _wvm.TilePicker.PlatformStairDirection = 0;
            }
            else
            {
                _wvm.TilePicker.PlatformStairDirection = 0;
            }

            DrawLine(p);
            _startPoint = p;
        }
        else if (_isLineMode)
        {
            _wvm.TilePicker.PlatformStairDirection = 0;
            DrawLineP2P(tile);
            _endPoint = tile;
        }
        else if (_isDrawing)
        {
            _wvm.TilePicker.PlatformStairDirection = 0;
            DrawLine(p);
            _startPoint = p;
            _endPoint = p;
        }
    }

    private void DrawLine(Vector2Int32 to)
    {
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;
        // Use thin line for Track (always) and Platform stair draws (diagonal constrain)
        bool useThin = _wvm.TilePicker.PaintMode == PaintMode.Track
            || (_wvm.TilePicker.PaintMode == PaintMode.Platform && _wvm.TilePicker.PlatformStairDirection != 0);
        var linePoints = useThin
            ? Shape.DrawLineThin(_startPoint, to)
            : Shape.DrawLineTool(_startPoint, to);
        foreach (Vector2Int32 pixel in linePoints)
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

            int index = pixel.X + pixel.Y * tilesWide;
            if (_wvm.CheckTiles[index] != generation)
            {
                _wvm.CheckTiles[index] = generation;
                if (_wvm.Selection.IsValid(pixel))
                {
                    _wvm.UndoManager.SaveTile(pixel);
                    _wvm.SetPixel(pixel.X, pixel.Y);

                    /* Heathtech */
                    BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                }
            }
        }
    }

    private void DrawLineP2P(Vector2Int32 endPoint)
    {
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;
        bool useThin = _wvm.TilePicker.PaintMode == PaintMode.Track
            || (_wvm.TilePicker.PaintMode == PaintMode.Platform && _wvm.TilePicker.PlatformStairDirection != 0);
        var linePoints = useThin
            ? Shape.DrawLineThin(_startPoint, _endPoint)
            : Shape.DrawLineTool(_startPoint, _endPoint);
        foreach (Vector2Int32 pixel in linePoints)
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

            int index = pixel.X + pixel.Y * tilesWide;
            if (_wvm.CheckTiles[index] != generation)
            {
                _wvm.CheckTiles[index] = generation;
                if (_wvm.Selection.IsValid(pixel))
                {
                    _wvm.UndoManager.SaveTile(pixel);
                    _wvm.SetPixel(pixel.X, pixel.Y);

                    /* Heathtech */
                    BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                }
            }
        }
    }
}
