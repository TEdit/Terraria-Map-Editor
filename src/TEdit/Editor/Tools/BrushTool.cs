using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.ViewModel;
using TEdit.UI;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;


public class BrushToolBase : BaseTool
{
    protected bool _isDrawing;
    protected bool _isConstraining;
    protected bool _isLineMode;
    protected int _constrainDirection; // 0=horizontal, 1=vertical, 2=diagonal
    protected bool _constrainDirectionLocked;
    protected Vector2Int32 _anchorPoint;
    protected Vector2Int32 _startPoint;
    protected Vector2Int32 _endPoint;
    protected Vector2Int32 _leftPoint;
    protected Vector2Int32 _rightPoint;
    private bool _hasLastClickPosition;
    private Vector2Int32 _lineMouseDownPos;
    private bool _lineLastWasClick;

    // Spray mode — time-based interpolation instead of timer
    private readonly Stopwatch _sprayStopwatch = new Stopwatch();
    private long _sprayLastTickMs;
    private Vector2Int32 _sprayPrevPoint;
    private readonly Random _sprayRandom = new Random();
    private bool _sprayActive;

    // CAD wire bus routing state
    private bool _isCadWireMode;
    private bool _isCadAnchored;
    private WireRoutingMode _cadRoutingMode = WireRoutingMode.Elbow90;
    private bool? _cadVerticalFirstOverride; // null = auto-detect, true = vertical-first, false = horizontal-first
    private Vector2Int32 _cadAnchor;
    private readonly List<Vector2Int32> _cadPreviewPath = new();
    private Vector2Int32 _cadLastCursor;

    // Pre-allocated buffers to avoid per-frame GC pressure (Phase 2)
    private readonly List<Vector2Int32> _stampBuffer = new(160_000);
    private readonly List<Vector2Int32> _interiorBuffer = new(160_000);
    private readonly List<Vector2Int32> _lineBuffer = new(800);
    private readonly HashSet<Vector2Int32> _sweepSet = new(10_000);

    // Cached interior offset set for FillHollow — rebuilt on BrushChanged
    private readonly HashSet<Vector2Int32> _interiorOffsetSet = new();

    public BrushToolBase(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/paintbrush.png"));
        SymbolIcon = SymbolRegular.PaintBrush24;
        ToolType = ToolType.Brush;

        _wvm.Brush.BrushChanged += OnBrushChanged;
    }

    /// <summary>Whether this tool supports CAD wire routing. Override to disable.</summary>
    public virtual bool SupportsCadRouting => true;

    /// <summary>Whether CAD wire bus routing mode is active.</summary>
    public bool IsCadWireMode => _isCadWireMode;

    /// <summary>Current CAD routing style.</summary>
    public WireRoutingMode CadRoutingMode => _cadRoutingMode;

    /// <summary>Current H/V override: null=auto, true=vertical-first, false=horizontal-first.</summary>
    public bool? CadVerticalFirstOverride => _cadVerticalFirstOverride;

    public override IReadOnlyList<Vector2Int32> CadPreviewPath => _cadPreviewPath;
    public override bool HasCadPreview => _isCadWireMode && _isCadAnchored && _cadPreviewPath.Count > 0;

    public override Vector2Int32 LinePreviewAnchor => _endPoint;
    public override bool HasLinePreviewAnchor => _hasLastClickPosition;

    /// <summary>
    /// Cycle wire mode: Off → Wire90 → Wire45 → Off (normal) → Wire90...
    /// Anchor is preserved when cycling through Off/normal.
    /// </summary>
    public void CycleWireMode()
    {
        if (!SupportsCadRouting) return;

        if (!_isCadWireMode)
        {
            _isCadWireMode = true;
            _cadRoutingMode = WireRoutingMode.Elbow90;
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
        if (!SupportsCadRouting) enabled = false;
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

    private void OnBrushChanged(object sender, EventArgs e)
    {
        // Rebuild the interior offset set from cached offsets
        _interiorOffsetSet.Clear();
        _wvm.Brush.EnsureCacheValid();
        var interiorOffsets = _wvm.Brush.CachedInteriorOffsets;
        if (interiorOffsets != null)
        {
            foreach (var offset in interiorOffsets)
                _interiorOffsetSet.Add(offset);
        }
    }

    protected IList<Vector2Int32> GetShapePoints(Vector2Int32 center)
    {
        return _wvm.Brush.GetShapePoints(center);
    }

    private bool IsSimpleRectShape()
    {
        var brush = _wvm.Brush;
        return (brush.Shape == BrushShape.Square || brush.Height <= 1 || brush.Width <= 1)
            && !brush.HasTransform;
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

        // CAD wire bus mode intercept
        if (_isCadWireMode)
        {
            var cadActions = GetActiveActions(e);

            // Right-click cancels anchor
            if (cadActions.Contains("editor.secondary"))
            {
                CancelCadWire();
                return;
            }

            if (cadActions.Contains("editor.draw") || cadActions.Contains("editor.draw.line") || cadActions.Contains("editor.draw.constrain"))
            {
                if (!_isCadAnchored)
                {
                    // First click: set anchor and draw 1 tile
                    _cadAnchor = e.Location;
                    _isCadAnchored = true;
                    _cadPreviewPath.Clear();
                    DrawSingleTile(e.Location);
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
                    return;
                }
            }
        }

        var actions = GetActiveActions(e);

        // Start new stroke if not already active
        if (!_isDrawing && !_isConstraining && !_isLineMode)
        {
            _anchorPoint = e.Location;
            _lineMouseDownPos = e.Location;
            _constrainDirectionLocked = false;

            // Continue polyline from previous endpoint if last line op was a click
            if (_lineLastWasClick && actions.Contains("editor.draw.line"))
            {
                _startPoint = _endPoint;
            }
            else
            {
                _startPoint = e.Location;
                _lineLastWasClick = false;
            }

            // Re-use or allocate check tiles
            int totalTiles = _wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh;
            if (_wvm.CheckTiles == null || _wvm.CheckTiles.Length != totalTiles)
            {
                _wvm.CheckTiles = new int[totalTiles];
            }
            // Increment generation instead of Array.Clear
            if (++_wvm.CheckTileGeneration <= 0)
            {
                _wvm.CheckTileGeneration = 1;
                Array.Clear(_wvm.CheckTiles, 0, _wvm.CheckTiles.Length);
            }
        }

        // Determine drawing mode from actions
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");

        _hasLastClickPosition = true;

        if (_wvm.Brush.IsSpray && _isDrawing)
        {
            _sprayPrevPoint = e.Location;
            _sprayStopwatch.Restart();
            _sprayLastTickMs = 0;
            _sprayActive = true;
            // Spray immediately at the click point
            SprayAtPoint(e.Location);
        }
        else
        {
            ProcessDraw(e.Location);
        }
    }

    public override void MouseMove(TileMouseState e)
    {
        // CAD wire bus mode: update preview path
        if (_isCadWireMode && _isCadAnchored)
        {
            UpdateCadPreview(e.Location);
            return;
        }

        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");

        if (_wvm.Brush.IsSpray && _sprayActive)
        {
            ProcessSprayMove(e.Location);
        }
        else
        {
            ProcessDraw(e.Location);
        }
    }

    public override void MouseUp(TileMouseState e)
    {
        // CAD wire bus mode: clicks are handled in MouseDown, no action on MouseUp
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

        if (_sprayActive)
        {
            // Final spray along remaining movement
            ProcessSprayMove(e.Location);
            _sprayStopwatch.Stop();
            _sprayActive = false;
        }
        else
        {
            ProcessDraw(e.Location);
        }

        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");
        _constrainDirectionLocked = false;
        _wvm.UndoManager.SaveUndo();
    }

    private void ProcessSprayMove(Vector2Int32 currentPos)
    {
        long now = _sprayStopwatch.ElapsedMilliseconds;
        long elapsed = now - _sprayLastTickMs;
        int tickInterval = _wvm.Brush.SprayTickMs;

        int tickCount = (int)(elapsed / tickInterval);
        if (tickCount <= 0) return;

        // Interpolate spray positions along the movement line
        _lineBuffer.Clear();
        foreach (var p in Shape.DrawLineTool(_sprayPrevPoint, currentPos))
            _lineBuffer.Add(p);
        if (_lineBuffer.Count == 0) return;

        for (int i = 0; i < tickCount; i++)
        {
            // Distribute spray ticks evenly along the line segment
            float t = (float)(i + 1) / (tickCount + 1);
            int lineIndex = Math.Min((int)(t * _lineBuffer.Count), _lineBuffer.Count - 1);
            SprayAtPoint(_lineBuffer[lineIndex]);
        }

        _sprayLastTickMs += (long)tickCount * tickInterval;
        _sprayPrevPoint = currentPos;
    }

    private void SprayAtPoint(Vector2Int32 center)
    {
        _wvm.Brush.StampOffsets(center, _stampBuffer);
        if (_stampBuffer.Count == 0) return;

        // Do NOT reset CheckTiles per tick — the generation set in MouseDown
        // persists across the entire spray stroke so already-painted tiles are
        // skipped. This prevents duplicate undo saves and redundant SetPixel calls.

        // Partial Fisher-Yates: select SprayDensity% of points
        int count = Math.Max(1, _stampBuffer.Count * _wvm.Brush.SprayDensity / 100);
        for (int i = 0; i < count && i < _stampBuffer.Count; i++)
        {
            int j = _sprayRandom.Next(i, _stampBuffer.Count);
            (_stampBuffer[i], _stampBuffer[j]) = (_stampBuffer[j], _stampBuffer[i]);
        }

        FillSolid(_stampBuffer, count);
    }

    public override WriteableBitmap PreviewTool()
    {
        var brush = _wvm.Brush;
        var previewColor = Color.FromArgb(127, 0, 90, 255);

        // Use the exact same cached stamps as the painting path for pixel-perfect preview.
        {
            var origin = new Vector2Int32(0, 0);
            var stampBuffer = new List<Vector2Int32>();
            brush.StampOffsets(origin, stampBuffer);
            IList<Vector2Int32> points = stampBuffer;

            if (points.Count == 0)
            {
                _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
                return _preview;
            }

            // Find bounding box of origin-relative points
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            foreach (var p in points)
            {
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
            }

            int previewW = maxX - minX + 1;
            int previewH = maxY - minY + 1;
            var bmp = new WriteableBitmap(previewW, previewH, 96, 96, PixelFormats.Bgra32, null);
            bmp.Clear();

            // Offset = where origin (0,0) falls in the bitmap = -minX, -minY
            PreviewOffsetX = -minX;
            PreviewOffsetY = -minY;

            // Batch all pixel writes in a single Lock/Unlock
            int a = previewColor.A + 1;
            int argb = (previewColor.A << 24)
                       | ((byte)((previewColor.R * a) >> 8) << 16)
                       | ((byte)((previewColor.G * a) >> 8) << 8)
                       | ((byte)((previewColor.B * a) >> 8));
            bmp.Lock();
            unsafe
            {
                var pixels = (int*)bmp.BackBuffer;
                int stride = bmp.BackBufferStride / 4;
                foreach (var p in points)
                {
                    int px = p.X - minX;
                    int py = p.Y - minY;
                    pixels[py * stride + px] = argb;
                }
            }
            bmp.AddDirtyRect(new Int32Rect(0, 0, previewW, previewH));
            bmp.Unlock();

            _preview = bmp;
        }

        return _preview;
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

        int w = _wvm.Brush.Width;
        int h = _wvm.Brush.Height;

        var points = _cadRoutingMode == WireRoutingMode.Elbow90
            ? WireRouter.RouteBus90(_cadAnchor, cursor, w, h, verticalFirst)
            : WireRouter.RouteBusMiter(_cadAnchor, cursor, w, h, verticalFirst);

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
                }
            }
        }

        _wvm.UndoManager.SaveUndo();
    }

    private void DrawSingleTile(Vector2Int32 pixel)
    {
        if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) return;
        if (!_wvm.Selection.IsValid(pixel)) return;

        int totalTiles = _wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh;
        if (_wvm.CheckTiles == null || _wvm.CheckTiles.Length != totalTiles)
            _wvm.CheckTiles = new int[totalTiles];
        if (++_wvm.CheckTileGeneration <= 0)
        {
            _wvm.CheckTileGeneration = 1;
            Array.Clear(_wvm.CheckTiles, 0, _wvm.CheckTiles.Length);
        }

        _wvm.UndoManager.SaveTile(pixel);
        _wvm.SetPixel(pixel.X, pixel.Y);
        _wvm.UndoManager.SaveUndo();
    }

    protected void ProcessDraw(Vector2Int32 tile)
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

            DrawLine(p);
            _startPoint = p;
        }
        else if (_isLineMode)
        {
            // Point-to-point line drawing (preview line from start to current)
            DrawLineP2P(tile);
            _endPoint = tile;
        }
        else if (_isDrawing)
        {
            // Freehand drawing
            DrawLine(p);
            _startPoint = p;
            _endPoint = p;
        }
    }

    protected void DrawLine(Vector2Int32 to)
    {
        _lineBuffer.Clear();
        foreach (var p in Shape.DrawLineTool(_startPoint, to))
            _lineBuffer.Add(p);

        if (IsSimpleRectShape())
        {
            // Single point (first click or no movement) — fill at that point
            if (_lineBuffer.Count == 1)
            {
                FillRectangle(_lineBuffer[0]);
            }
            for (int i = 1; i < _lineBuffer.Count; i++)
            {
                FillRectangleLine(_lineBuffer[i - 1], _lineBuffer[i]);
            }
        }
        else
        {
            foreach (Vector2Int32 point in _lineBuffer)
            {
                FillShape(point);
            }
        }
    }

    protected void DrawLineP2P(Vector2Int32 endPoint)
    {
        _lineBuffer.Clear();
        foreach (var p in Shape.DrawLineTool(_startPoint, _endPoint))
            _lineBuffer.Add(p);

        if (IsSimpleRectShape())
        {
            if (_lineBuffer.Count == 1)
            {
                FillRectangle(_lineBuffer[0]);
            }
            for (int i = 1; i < _lineBuffer.Count; i++)
            {
                FillRectangleLine(_lineBuffer[i - 1], _lineBuffer[i]);
            }
        }
        else
        {
            foreach (Vector2Int32 point in _lineBuffer)
            {
                FillShape(point);
            }
        }
    }

    /// <summary>
    /// For line-shaped brushes, draw continuous lines between shape endpoints
    /// at consecutive stroke positions to avoid gaps during fast drawing.
    /// </summary>
    protected void FillLineShapeSweep(Vector2Int32 from, Vector2Int32 to)
    {
        var brush = _wvm.Brush;
        // Use FillRectangleCentered bounds to match GetLineShapePoints/preview exactly
        int left = -brush.Width / 2;
        int top = -brush.Height / 2;
        int right = left + brush.Width - 1;
        int bottom = top + brush.Height - 1;

        // Get the line segment endpoints (relative to center) for each sub-line
        (Vector2Int32 start, Vector2Int32 end)[] segments = brush.Shape switch
        {
            BrushShape.Right => [(new(left, bottom), new(right, top))],
            BrushShape.Left => [(new(left, top), new(right, bottom))],
            BrushShape.Cross => [
                (new(left, top), new(right, bottom)),
                (new(left, bottom), new(right, top))
            ],
            _ => []
        };

        // Apply flip/rotation to the relative endpoints
        if (brush.HasTransform)
        {
            var origin = new Vector2Int32(0, 0);
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = (
                    brush.TransformPoint(segments[i].start, origin),
                    brush.TransformPoint(segments[i].end, origin)
                );
            }
        }

        // Draw line segments at both endpoints, then fill the quad between them.
        // FillPolygon alone can miss boundary pixels on thin/degenerate quads.
        _sweepSet.Clear();
        foreach (var (relStart, relEnd) in segments)
        {
            var fromStart = new Vector2Int32(from.X + relStart.X, from.Y + relStart.Y);
            var fromEnd = new Vector2Int32(from.X + relEnd.X, from.Y + relEnd.Y);
            var toStart = new Vector2Int32(to.X + relStart.X, to.Y + relStart.Y);
            var toEnd = new Vector2Int32(to.X + relEnd.X, to.Y + relEnd.Y);

            // Draw the actual lines at both positions to guarantee all pixels
            foreach (var p in Shape.DrawLine(fromStart, fromEnd))
                _sweepSet.Add(p);
            if (from.X != to.X || from.Y != to.Y)
            {
                foreach (var p in Shape.DrawLine(toStart, toEnd))
                    _sweepSet.Add(p);
                // Fill the quad between the two line positions
                Vector2Int32[] quad = [fromStart, fromEnd, toEnd, toStart];
                foreach (var p in Fill.FillPolygon(quad))
                    _sweepSet.Add(p);
            }
        }

        // Apply thickness — need a temporary list for ThickenLine input
        _stampBuffer.Clear();
        _stampBuffer.AddRange(_sweepSet);
        var result = BrushSettings.ThickenLine(_stampBuffer, brush.Outline);
        FillSolid(result);
    }

    /// <summary>
    /// Fill the cached brush shape at <paramref name="point"/>, using cached offsets
    /// instead of recalculating geometry from scratch.
    /// </summary>
    protected void FillShape(Vector2Int32 point)
    {
        _wvm.Brush.StampOffsets(point, _stampBuffer);

        if (_wvm.Brush.IsOutline)
        {
            _wvm.Brush.StampInteriorOffsets(point, _interiorBuffer);
            FillHollowCached(_stampBuffer, point);
        }
        else
        {
            FillSolid(_stampBuffer);
        }
    }

    protected void FillRectangleLine(Vector2Int32 start, Vector2Int32 end)
    {
        _stampBuffer.Clear();
        foreach (var p in Fill.FillRectangleVectorCenter(start, end, new Vector2Int32(_wvm.Brush.Width, _wvm.Brush.Height)))
            _stampBuffer.Add(p);
        FillSolid(_stampBuffer);
    }

    protected void FillRectangle(Vector2Int32 point)
    {
        _stampBuffer.Clear();
        foreach (var p in Fill.FillRectangleCentered(point, new Vector2Int32(_wvm.Brush.Width, _wvm.Brush.Height)))
            _stampBuffer.Add(p);
        if (_wvm.Brush.IsOutline)
        {
            _interiorBuffer.Clear();
            foreach (var p in Fill.FillRectangleCentered(
                point,
                new Vector2Int32(
                    _wvm.Brush.Width - _wvm.Brush.Outline * 2,
                    _wvm.Brush.Height - _wvm.Brush.Outline * 2)))
                _interiorBuffer.Add(p);
            FillHollow(_stampBuffer, _interiorBuffer);
        }
        else
        {
            FillSolid(_stampBuffer);
        }
    }

    protected virtual void FillSolid(IList<Vector2Int32> area) => FillSolid(area, area.Count);

    protected virtual void FillSolid(IList<Vector2Int32> area, int count)
    {
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        bool anyModified = false;

        _wvm.WorldEditor.SuppressNotify = true;
        try
        {
            for (int i = 0; i < count; i++)
            {
                Vector2Int32 pixel = area[i];
                if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * tilesWide;
                if (_wvm.CheckTiles[index] != generation)
                {
                    _wvm.CheckTiles[index] = generation;
                    if (_wvm.Selection.IsValid(pixel))
                    {
                        _wvm.UndoManager.SaveTile(pixel);
                        _wvm.SetPixel(pixel.X, pixel.Y);

                        if (pixel.X < minX) minX = pixel.X;
                        if (pixel.Y < minY) minY = pixel.Y;
                        if (pixel.X > maxX) maxX = pixel.X;
                        if (pixel.Y > maxY) maxY = pixel.Y;
                        anyModified = true;
                    }
                }
            }
        }
        finally
        {
            _wvm.WorldEditor.SuppressNotify = false;
        }

        if (anyModified)
        {
            int w = maxX - minX + 1;
            int h = maxY - minY + 1;
            _wvm.QueueUVCacheReset(minX, minY, w, h);
        }
    }

    /// <summary>
    /// FillHollow using pre-built <see cref="_interiorOffsetSet"/> and center-relative lookup.
    /// Avoids allocating a new HashSet per call.
    /// </summary>
    private void FillHollowCached(IList<Vector2Int32> area, Vector2Int32 center)
    {
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        bool anyModified = false;

        _wvm.WorldEditor.SuppressNotify = true;
        try
        {
            // Draw the border
            if (_wvm.TilePicker.TileStyleActive)
            {
                foreach (Vector2Int32 pixel in area)
                {
                    var rel = new Vector2Int32(pixel.X - center.X, pixel.Y - center.Y);
                    if (_interiorOffsetSet.Contains(rel)) continue;
                    if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                    int index = pixel.X + pixel.Y * tilesWide;
                    if (_wvm.CheckTiles[index] != generation)
                    {
                        _wvm.CheckTiles[index] = generation;
                        if (_wvm.Selection.IsValid(pixel))
                        {
                            _wvm.UndoManager.SaveTile(pixel);
                            if (_wvm.TilePicker.WallStyleActive)
                            {
                                _wvm.TilePicker.WallStyleActive = false;
                                _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);
                                _wvm.TilePicker.WallStyleActive = true;
                            }
                            else
                                _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);

                            if (pixel.X < minX) minX = pixel.X;
                            if (pixel.Y < minY) minY = pixel.Y;
                            if (pixel.X > maxX) maxX = pixel.X;
                            if (pixel.Y > maxY) maxY = pixel.Y;
                            anyModified = true;
                        }
                    }
                }
            }

            // Draw the wall in the interior, exclude the border so no overlaps
            foreach (Vector2Int32 pixel in area)
            {
                var rel = new Vector2Int32(pixel.X - center.X, pixel.Y - center.Y);
                if (!_interiorOffsetSet.Contains(rel)) continue;
                if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * tilesWide;
                if (_wvm.CheckTiles[index] != generation)
                {
                    _wvm.CheckTiles[index] = generation;
                    if (_wvm.Selection.IsValid(pixel))
                    {
                        _wvm.UndoManager.SaveTile(pixel);
                        _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall, erase: true);

                        if (_wvm.TilePicker.WallStyleActive)
                        {
                            if (_wvm.TilePicker.TileStyleActive)
                            {
                                _wvm.TilePicker.TileStyleActive = false;
                                _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);
                                _wvm.TilePicker.TileStyleActive = true;
                            }
                            else
                                _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);
                        }

                        if (pixel.X < minX) minX = pixel.X;
                        if (pixel.Y < minY) minY = pixel.Y;
                        if (pixel.X > maxX) maxX = pixel.X;
                        if (pixel.Y > maxY) maxY = pixel.Y;
                        anyModified = true;
                    }
                }
            }
        }
        finally
        {
            _wvm.WorldEditor.SuppressNotify = false;
        }

        if (anyModified)
        {
            int w = maxX - minX + 1;
            int h = maxY - minY + 1;
            _wvm.QueueUVCacheReset(minX, minY, w, h);
        }
    }

    /// <summary>
    /// Legacy FillHollow for paths that pass explicit interior lists (e.g. FillRectangle).
    /// </summary>
    protected virtual void FillHollow(IList<Vector2Int32> area, IList<Vector2Int32> interrior)
    {
        var interiorSet = new HashSet<Vector2Int32>(interrior);
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        bool anyModified = false;

        _wvm.WorldEditor.SuppressNotify = true;
        try
        {
            // Draw the border
            if (_wvm.TilePicker.TileStyleActive)
            {
                foreach (Vector2Int32 pixel in area)
                {
                    if (interiorSet.Contains(pixel)) continue;
                    if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                    int index = pixel.X + pixel.Y * tilesWide;

                    if (_wvm.CheckTiles[index] != generation)
                    {
                        _wvm.CheckTiles[index] = generation;
                        if (_wvm.Selection.IsValid(pixel))
                        {
                            _wvm.UndoManager.SaveTile(pixel);
                            if (_wvm.TilePicker.WallStyleActive)
                            {
                                _wvm.TilePicker.WallStyleActive = false;
                                _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);
                                _wvm.TilePicker.WallStyleActive = true;
                            }
                            else
                                _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);

                            if (pixel.X < minX) minX = pixel.X;
                            if (pixel.Y < minY) minY = pixel.Y;
                            if (pixel.X > maxX) maxX = pixel.X;
                            if (pixel.Y > maxY) maxY = pixel.Y;
                            anyModified = true;
                        }
                    }
                }
            }

            // Draw the wall in the interrior, exclude the border so no overlaps
            foreach (Vector2Int32 pixel in interrior)
            {
                if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * tilesWide;
                if (_wvm.CheckTiles[index] != generation)
                {
                    _wvm.CheckTiles[index] = generation;
                    if (_wvm.Selection.IsValid(pixel))
                    {
                        _wvm.UndoManager.SaveTile(pixel);
                        _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall, erase: true);

                        if (_wvm.TilePicker.WallStyleActive)
                        {
                            if (_wvm.TilePicker.TileStyleActive)
                            {
                                _wvm.TilePicker.TileStyleActive = false;
                                _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);
                                _wvm.TilePicker.TileStyleActive = true;
                            }
                            else
                                _wvm.SetPixel(pixel.X, pixel.Y, mode: PaintMode.TileAndWall);
                        }

                        if (pixel.X < minX) minX = pixel.X;
                        if (pixel.Y < minY) minY = pixel.Y;
                        if (pixel.X > maxX) maxX = pixel.X;
                        if (pixel.Y > maxY) maxY = pixel.Y;
                        anyModified = true;
                    }
                }
            }
        }
        finally
        {
            _wvm.WorldEditor.SuppressNotify = false;
        }

        if (anyModified)
        {
            int w = maxX - minX + 1;
            int h = maxY - minY + 1;
            _wvm.QueueUVCacheReset(minX, minY, w, h);
        }
    }
}

public sealed class BrushTool : BrushToolBase
{
    public BrushTool(WorldViewModel worldViewModel) : base(worldViewModel)
    {
        Name = "Brush";
    }
}
