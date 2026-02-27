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
    private bool _hasLastClickPosition;
    private Vector2Int32 _lineMouseDownPos;
    private bool _lineLastWasClick;

    // CAD wire routing state
    private bool _isCadWireMode;
    private bool _isCadAnchored;
    private WireRoutingMode _cadRoutingMode = WireRoutingMode.Elbow90;
    private bool? _cadVerticalFirstOverride; // null = auto-detect, true = vertical-first, false = horizontal-first
    private Vector2Int32 _cadAnchor;
    private readonly List<Vector2Int32> _cadPreviewPath = new();
    private readonly List<Vector2Int32> _cadPreviewTunnel = new();
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
    public override IReadOnlyList<Vector2Int32> CadPreviewTunnelPath => _cadPreviewTunnel;
    public override bool HasCadPreview => _isCadWireMode && _isCadAnchored && _cadPreviewPath.Count > 0;

    public override Vector2Int32 LinePreviewAnchor => _endPoint;
    public override bool HasLinePreviewAnchor => _hasLastClickPosition;

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
            _cadPreviewTunnel.Clear();
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
            _cadPreviewTunnel.Clear();
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
        // Wire trace intercept (Alt+Click)
        var traceActions = GetActiveActions(e);
        if (traceActions.Contains("editor.wire.trace"))
        {
            PerformWireTrace(e.Location);
            return;
        }

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
            _cadPreviewTunnel.Clear();
                    return;
                }
                else
                {
                    // Second click: commit the preview path
                    CommitCadPath();

                    if (_wvm.WireChainMode)
                    {
                        // Chain: new anchor = old end for polyline
                        _cadAnchor = e.Location;
                    }
                    else
                    {
                        // No chain: cancel anchor (like right-click)
                        _isCadAnchored = false;
                    }
                    _cadPreviewPath.Clear();
                    _cadPreviewTunnel.Clear();
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

        _hasLastClickPosition = true;
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
        _cadPreviewTunnel.Clear();

        bool verticalFirst = _cadVerticalFirstOverride
            ?? WireRouter.DetectVerticalFirst(_cadAnchor, cursor);

        // Track uses thin diagonals (1 tile per step); wires and platforms use staircase (4-connected)
        bool useThinDiag = _wvm.TilePicker.PaintMode == PaintMode.Track;
        var points = _cadRoutingMode == WireRoutingMode.Elbow90
            ? WireRouter.Route90(_cadAnchor, cursor, verticalFirst)
            : useThinDiag
                ? WireRouter.RouteMiterThin(_cadAnchor, cursor, verticalFirst)
                : WireRouter.RouteMiter(_cadAnchor, cursor, verticalFirst);

        _cadPreviewPath.AddRange(points);

        // Compute tunnel preview for Track mode with tunnel enabled
        if (_wvm.TilePicker.PaintMode == PaintMode.Track
            && _wvm.TilePicker.TrackMode == TrackMode.Track
            && _wvm.TilePicker.TrackTunnelEnabled)
        {
            int tunnelHeight = Math.Clamp(_wvm.TilePicker.TrackTunnelHeight, 1, 10);
            var pathSet = new HashSet<Vector2Int32>(_cadPreviewPath);
            foreach (var p in _cadPreviewPath)
            {
                for (int ty = p.Y - 1; ty >= p.Y - tunnelHeight; ty--)
                {
                    var above = new Vector2Int32(p.X, ty);
                    if (!pathSet.Contains(above))
                        _cadPreviewTunnel.Add(above);
                }
            }
        }
    }

    private static readonly Random _rng = new();

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

        // For platform miter, classify each tile's stair role and set frames after placement
        bool isPlatformMiter = _wvm.TilePicker.PaintMode == PaintMode.Platform
            && _cadRoutingMode == WireRoutingMode.Miter45;

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

        if (isPlatformMiter)
            ApplyPlatformStairFrames();

        _wvm.UndoManager.SaveUndo();
    }

    /// <summary>
    /// After committing a platform miter path, override U frames to use proper
    /// stair insets, stringers, landings, and endcaps based on each tile's role.
    /// </summary>
    private void ApplyPlatformStairFrames()
    {
        var path = _cadPreviewPath;
        if (path.Count < 2) return;

        // Determine stair direction from the overall path vector
        int dx = path[path.Count - 1].X - path[0].X;
        int dy = path[path.Count - 1].Y - path[0].Y;
        int sx = Math.Sign(dx);
        int sy = Math.Sign(dy);

        short styleV = (short)(_wvm.TilePicker.PlatformStyle * 18);

        // Pure vertical line → use Single frame (column 5, U=90) for all tiles
        if (dx == 0)
        {
            const int singleCol = 5;
            for (int i = 0; i < path.Count; i++)
            {
                var cur = path[i];
                if (!_wvm.CurrentWorld.ValidTileLocation(cur)) continue;
                ref var tile = ref _wvm.CurrentWorld.Tiles[cur.X, cur.Y];
                if (!tile.IsActive || tile.Type != 19) continue;
                tile.U = (short)(singleCol * 18);
                tile.V = styleV;
            }
            return;
        }

        // "Up-Right" when going right+up or left+down (sx*sy < 0)
        // "Up-Left" when going right+down or left+up (sx*sy > 0)
        bool isUpRight = sx * sy < 0;

        // Platform stair frame columns (U / 18)
        // Inset = top surface of stair step (3 random variants per direction)
        int[] insetCols = isUpRight
            ? new[] { 19, 21, 23 }   // Stair Inset Up-Right 1/2/3
            : new[] { 20, 22, 24 };  // Stair Inset Up-Left 1/2/3
        int stringerCol = isUpRight ? 9 : 11;     // Stringer (underside)
        int topLandingCol = isUpRight ? 12 : 13;        // Stair Top Landing R/L
        int topLandingEndcapCol = isUpRight ? 15 : 16; // Stair Top Landing R/L Endcap (solid neighbor)
        int botLandingCol = isUpRight ? 17 : 18;        // Stair Bottom Landing R/L
        const int topLandingLRCol = 14;                  // Stair Top Landing L-R (two diags meet)

        // When going upward (sy < 0), the H→V transition is at the bottom of the stairs
        // and V→H is at the top — opposite of going down. Swap landing types accordingly.
        bool goingUp = sy < 0;
        int hvLandingCol = goingUp ? botLandingCol : topLandingCol;
        int hvLandingEndcapCol = goingUp ? botLandingCol : topLandingEndcapCol;
        int vhLandingCol = goingUp ? topLandingCol : botLandingCol;
        int vhLandingEndcapCol = goingUp ? topLandingEndcapCol : botLandingCol;

        // Helper: check movement direction between two path points
        bool IsHorizontal(int idxFrom, int idxTo) =>
            path[idxTo].Y == path[idxFrom].Y && path[idxTo].X != path[idxFrom].X;
        bool IsVertical(int idxFrom, int idxTo) =>
            path[idxTo].X == path[idxFrom].X && path[idxTo].Y != path[idxFrom].Y;

        // Classify each tile using 3-tile context to distinguish transitions from staircase interior.
        // A tile is a landing only at the boundary between a straight run and the staircase.
        // Within the staircase, H-step tiles are insets and V-step tiles are stringers.
        for (int i = 0; i < path.Count; i++)
        {
            var cur = path[i];
            if (!_wvm.CurrentWorld.ValidTileLocation(cur)) continue;
            ref var tile = ref _wvm.CurrentWorld.Tiles[cur.X, cur.Y];
            if (!tile.IsActive || tile.Type != 19) continue;

            bool hasPrev = i > 0;
            bool hasNext = i < path.Count - 1;

            int dxPrev = hasPrev ? cur.X - path[i - 1].X : 0;
            int dyPrev = hasPrev ? cur.Y - path[i - 1].Y : 0;
            int dxNext = hasNext ? path[i + 1].X - cur.X : 0;
            int dyNext = hasNext ? path[i + 1].Y - cur.Y : 0;

            bool prevH = hasPrev && dyPrev == 0 && dxPrev != 0;
            bool prevV = hasPrev && dxPrev == 0 && dyPrev != 0;
            bool nextH = hasNext && dyNext == 0 && dxNext != 0;
            bool nextV = hasNext && dxNext == 0 && dyNext != 0;

            // 3-tile context: check directions before prev and after next
            bool prevPrevH = i >= 2 && IsHorizontal(i - 2, i - 1);
            bool prevPrevV = i >= 2 && IsVertical(i - 2, i - 1);
            bool nextNextH = i <= path.Count - 3 && IsHorizontal(i + 1, i + 2);
            bool nextNextV = i <= path.Count - 3 && IsVertical(i + 1, i + 2);

            int col;

            if (prevH && nextH)
            {
                // Flat horizontal — but check if this is the last flat tile before a staircase
                // (entry landing shifted here) or first flat tile after a staircase (exit landing)
                if (nextNextV && !goingUp)
                {
                    // Last flat H tile before staircase → entry landing (going down only;
                    // going up the entry landing shifts into the staircase instead)
                    bool outerSolid = HasSolidNeighbor(cur.X - dxPrev, cur.Y);
                    col = outerSolid ? hvLandingEndcapCol : hvLandingCol;
                }
                else if (prevPrevV && !goingUp)
                {
                    // First flat H tile after staircase → exit landing (going down only;
                    // going up the exit landing shifts into the staircase instead)
                    bool outerSolid = HasSolidNeighbor(cur.X + dxNext, cur.Y);
                    col = outerSolid ? vhLandingEndcapCol : vhLandingCol;
                }
                else
                {
                    // Normal flat — leave frame as-is
                    continue;
                }
            }
            else if (prevV && nextV)
            {
                // Vertical run — check for shifted landings from adjacent staircase
                if (nextNextH)
                {
                    // Last V tile before staircase → entry landing
                    col = hvLandingCol;
                }
                else if (prevPrevH)
                {
                    // First V tile after staircase → exit landing
                    col = vhLandingCol;
                }
                else
                {
                    // Interior straight vertical run — use Single frame (column 5)
                    col = 5;
                }
            }
            else if (!hasPrev && nextH)
            {
                // Start of path, leaving horizontally — check if next turns to staircase
                if (nextNextV)
                {
                    // Only one flat tile before staircase → entry landing
                    bool outerSolid = HasSolidNeighbor(cur.X - dxNext, cur.Y);
                    col = outerSolid ? hvLandingEndcapCol : hvLandingCol;
                }
                else
                {
                    continue; // flat
                }
            }
            else if (!hasPrev && nextV)
            {
                // Start of path, leaving vertically
                if (nextNextV)
                {
                    // Starting a straight V run → Single frame
                    col = 5;
                }
                else
                {
                    // Starting directly into staircase → landing
                    bool solidLeft = HasSolidNeighbor(cur.X - 1, cur.Y);
                    bool solidRight = HasSolidNeighbor(cur.X + 1, cur.Y);
                    col = (solidLeft || solidRight) ? hvLandingEndcapCol : hvLandingCol;
                }
            }
            else if (prevH && !hasNext)
            {
                // End of path, arriving horizontally — check if prev came from staircase
                if (prevPrevV)
                {
                    // Only one flat tile after staircase → exit landing
                    bool outerSolid = HasSolidNeighbor(cur.X + dxPrev, cur.Y);
                    col = outerSolid ? vhLandingEndcapCol : vhLandingCol;
                }
                else
                {
                    continue; // flat
                }
            }
            else if (prevV && !hasNext)
            {
                // End of path, arriving vertically — no flat run after, landing stays here
                if (prevPrevH)
                {
                    // Came from staircase → exit landing
                    col = vhLandingCol;
                }
                else
                {
                    // End of straight vertical run — Single frame
                    col = 5;
                }
            }
            else if (prevH && nextV)
            {
                // H→V direction change
                if (prevPrevH)
                {
                    if (goingUp)
                    {
                        // Going up: entry landing shifts INTO the staircase (this tile)
                        bool outerSolid = HasSolidNeighbor(cur.X - dxPrev, cur.Y);
                        col = outerSolid ? hvLandingEndcapCol : hvLandingCol;
                    }
                    else
                    {
                        // Going down: landing already on prev flat tile → this is inset
                        col = insetCols[_rng.Next(insetCols.Length)];
                    }
                }
                else if (!hasPrev)
                {
                    // Start of path with no flat run → landing here
                    col = hvLandingCol;
                }
                else
                {
                    // Interior staircase H-step
                    col = goingUp ? stringerCol : insetCols[_rng.Next(insetCols.Length)];
                }
            }
            else if (prevV && nextH)
            {
                // V→H direction change
                if (nextNextH)
                {
                    if (goingUp)
                    {
                        // Going up: exit landing shifts INTO the staircase (this tile)
                        bool outerSolid = HasSolidNeighbor(cur.X + dxNext, cur.Y);
                        col = outerSolid ? vhLandingEndcapCol : vhLandingCol;
                    }
                    else
                    {
                        // Going down: landing already on next flat tile → this is stringer
                        col = stringerCol;
                    }
                }
                else if (!hasNext)
                {
                    // End of path with no flat run → landing here
                    col = vhLandingCol;
                }
                else
                {
                    // Interior staircase V-step
                    col = goingUp ? insetCols[_rng.Next(insetCols.Length)] : stringerCol;
                }
            }
            else
            {
                // Fallback: use inset
                col = insetCols[0];
            }

            tile.U = (short)(col * 18);
            tile.V = styleV;
        }
    }

    /// <summary>Check if a tile location has a non-platform solid block.</summary>
    private bool HasSolidNeighbor(int x, int y)
    {
        if (!_wvm.CurrentWorld.ValidTileLocation(new Vector2Int32(x, y))) return false;
        var t = _wvm.CurrentWorld.Tiles[x, y];
        return t.IsActive && t.Type != 19; // solid but not a platform
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
