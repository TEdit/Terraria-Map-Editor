using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.ViewModel;
using TEdit.Render;
using TEdit.UI;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;


public class BrushToolBase : BaseTool
{
    protected bool _isDrawing;
    protected bool _isConstraining;
    protected bool _isLineMode;
    protected bool _constrainVertical;
    protected bool _constrainDirectionLocked;
    protected Vector2Int32 _anchorPoint;
    protected Vector2Int32 _startPoint;
    protected Vector2Int32 _endPoint;
    protected Vector2Int32 _leftPoint;
    protected Vector2Int32 _rightPoint;

    // Spray mode — time-based interpolation instead of timer
    private readonly Stopwatch _sprayStopwatch = new Stopwatch();
    private long _sprayLastTickMs;
    private Vector2Int32 _sprayPrevPoint;
    private readonly Random _sprayRandom = new Random();
    private bool _sprayActive;

    public BrushToolBase(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/paintbrush.png"));
        SymbolIcon = SymbolRegular.PaintBrush24;
        ToolType = ToolType.Brush;
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
        var actions = GetActiveActions(e);

        // Start new stroke if not already active
        if (!_isDrawing && !_isConstraining && !_isLineMode)
        {
            _startPoint = e.Location;
            _anchorPoint = e.Location;
            _constrainDirectionLocked = false;
            
            // Re-use or allocate check tiles
            int totalTiles = _wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh;
            if (_wvm.CheckTiles == null || _wvm.CheckTiles.Length != totalTiles)
            {
                _wvm.CheckTiles = new bool[totalTiles];
            }
            else
            {
                Array.Clear(_wvm.CheckTiles, 0, _wvm.CheckTiles.Length);
            }
        }

        // Determine drawing mode from actions
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");

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
        var line = Shape.DrawLineTool(_sprayPrevPoint, currentPos).ToList();
        if (line.Count == 0) return;

        for (int i = 0; i < tickCount; i++)
        {
            // Distribute spray ticks evenly along the line segment
            float t = (float)(i + 1) / (tickCount + 1);
            int lineIndex = Math.Min((int)(t * line.Count), line.Count - 1);
            SprayAtPoint(line[lineIndex]);
        }

        _sprayLastTickMs += (long)tickCount * tickInterval;
        _sprayPrevPoint = currentPos;
    }

    private void SprayAtPoint(Vector2Int32 center)
    {
        var points = GetShapePoints(center);
        if (points.Count == 0) return;

        // Reset check tiles each tick so spray re-paints
        if (_wvm.CheckTiles != null)
        {
            Array.Clear(_wvm.CheckTiles, 0, _wvm.CheckTiles.Length);
        }

        // Partial Fisher-Yates: select SprayDensity% of points
        int count = Math.Max(1, points.Count * _wvm.Brush.SprayDensity / 100);
        var selected = new List<Vector2Int32>(count);

        var working = points.ToArray();
        for (int i = 0; i < count && i < working.Length; i++)
        {
            int j = _sprayRandom.Next(i, working.Length);
            (working[i], working[j]) = (working[j], working[i]);
            selected.Add(working[i]);
        }

        FillSolid(selected);
    }

    public override WriteableBitmap PreviewTool()
    {
        var brush = _wvm.Brush;
        var previewColor = Color.FromArgb(127, 0, 90, 255);

        bool usePointPreview = brush.HasTransform ||
            brush.Shape == BrushShape.Star ||
            brush.Shape == BrushShape.Triangle ||
            brush.Shape == BrushShape.Crescent ||
            brush.Shape == BrushShape.Donut ||
            brush.Shape == BrushShape.Cross;

        if (usePointPreview)
        {
            // Generate points at origin center, then find actual bounding box
            var center = new Vector2Int32(brush.Width / 2, brush.Height / 2);
            var points = GetShapePoints(center);

            if (points.Count == 0)
            {
                _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
                return _preview;
            }

            // Find bounding box of transformed points
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

            // Track where the center falls within the bitmap
            PreviewOffsetX = center.X - minX;
            PreviewOffsetY = center.Y - minY;

            // Offset points so min corner maps to (0,0)
            foreach (var p in points)
            {
                int px = p.X - minX;
                int py = p.Y - minY;
                bmp.SetPixel(px, py, previewColor);
            }

            _preview = bmp;
        }
        else
        {
            PreviewOffsetX = -1;
            PreviewOffsetY = -1;
            int previewW = brush.Width + 1;
            int previewH = brush.Height + 1;
            var bmp = new WriteableBitmap(previewW, previewH, 96, 96, PixelFormats.Bgra32, null);
            bmp.Clear();

            if (IsSimpleRectShape())
            {
                bmp.FillRectangle(0, 0, brush.Width, brush.Height, previewColor);
            }
            else if (brush.Shape == BrushShape.Left)
            {
                bmp.DrawLine(0, 0, brush.Width, brush.Height, previewColor);
            }
            else if (brush.Shape == BrushShape.Right)
            {
                bmp.DrawLine(0, brush.Height, brush.Width, 0, previewColor);
            }
            else
            {
                bmp.FillEllipse(0, 0, brush.Width, brush.Height, previewColor);
            }

            _preview = bmp;
        }

        return _preview;
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
                    _constrainVertical = dx < dy;
                    _constrainDirectionLocked = true;
                }
            }

            if (_constrainVertical)
                p.X = _anchorPoint.X;
            else
                p.Y = _anchorPoint.Y;

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
        var line = Shape.DrawLineTool(_startPoint, to).ToList();
        if (IsSimpleRectShape())
        {
            // Single point (first click or no movement) — fill at that point
            if (line.Count == 1)
            {
                FillRectangle(line[0]);
            }
            for (int i = 1; i < line.Count; i++)
            {
                FillRectangleLine(line[i - 1], line[i]);
            }
        }
        else
        {
            foreach (Vector2Int32 point in line)
            {
                FillShape(point);
            }
        }
    }

    protected void DrawLineP2P(Vector2Int32 endPoint)
    {
        var line = Shape.DrawLineTool(_startPoint, _endPoint).ToList();

        if (IsSimpleRectShape())
        {
            if (line.Count == 1)
            {
                FillRectangle(line[0]);
            }
            for (int i = 1; i < line.Count; i++)
            {
                FillRectangleLine(line[i - 1], line[i]);
            }
        }
        else
        {
            foreach (Vector2Int32 point in line)
            {
                FillShape(point);
            }
        }
    }

    private static bool IsLineShape(BrushShape shape) =>
        shape == BrushShape.Right || shape == BrushShape.Left || shape == BrushShape.Cross;

    private static bool ShapeUsesParametricOutline(BrushShape shape) =>
        shape == BrushShape.Square || shape == BrushShape.Round;

    protected void FillShape(Vector2Int32 point)
    {
        var area = GetShapePoints(point);
        if (_wvm.Brush.IsOutline)
        {
            if (IsLineShape(_wvm.Brush.Shape))
            {
                // Line shapes: outline = line thickness
                FillSolid(ThickenLine(area, _wvm.Brush.Outline));
            }
            else
            {
                IList<Vector2Int32> interiorPoints;
                if (ShapeUsesParametricOutline(_wvm.Brush.Shape))
                {
                    int interiorWidth = Math.Max(1, _wvm.Brush.Width - _wvm.Brush.Outline * 2);
                    int interiorHeight = Math.Max(1, _wvm.Brush.Height - _wvm.Brush.Outline * 2);
                    interiorPoints = _wvm.Brush.GetShapePoints(point, interiorWidth, interiorHeight);
                }
                else
                {
                    interiorPoints = ErodeShape(area, _wvm.Brush.Outline);
                }
                FillHollow(area, interiorPoints);
            }
        }
        else
        {
            FillSolid(area);
        }
    }

    /// <summary>
    /// Erode a shape by N pixels using a bitmask. A point is interior if
    /// all points within Chebyshev distance N are also in the shape.
    /// Uses separable 1D erosion (horizontal then vertical) for O(n) performance.
    /// </summary>
    private static IList<Vector2Int32> ErodeShape(IList<Vector2Int32> points, int outline)
    {
        if (points.Count == 0 || outline <= 0) return points;

        // Find bounding box
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        foreach (var p in points)
        {
            if (p.X < minX) minX = p.X;
            if (p.X > maxX) maxX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.Y > maxY) maxY = p.Y;
        }

        int w = maxX - minX + 1;
        int h = maxY - minY + 1;
        if (w <= outline * 2 || h <= outline * 2) return [];

        // Build bitmask
        var mask = new bool[w * h];
        foreach (var p in points)
            mask[(p.X - minX) + (p.Y - minY) * w] = true;

        // Horizontal erosion: for each row, a pixel survives if all pixels
        // in [x-outline, x+outline] on the same row are set.
        var hEroded = new bool[w * h];
        for (int y = 0; y < h; y++)
        {
            int rowOff = y * w;
            // Running count of consecutive set pixels ending at x
            int run = 0;
            for (int x = 0; x < w; x++)
            {
                run = mask[rowOff + x] ? run + 1 : 0;
                // A pixel at x survives horizontal erosion if the run
                // from (x - 2*outline) to x is at least (2*outline + 1)
                if (run >= outline * 2 + 1)
                    hEroded[rowOff + x - outline] = true;
            }
        }

        // Vertical erosion on hEroded result
        var result = new List<Vector2Int32>();
        for (int x = 0; x < w; x++)
        {
            int run = 0;
            for (int y = 0; y < h; y++)
            {
                run = hEroded[x + y * w] ? run + 1 : 0;
                if (run >= outline * 2 + 1)
                    result.Add(new Vector2Int32(minX + x, minY + y - outline));
            }
        }

        return result;
    }

    /// <summary>
    /// Expand line points by a given thickness using a circular kernel.
    /// </summary>
    private static IList<Vector2Int32> ThickenLine(IList<Vector2Int32> linePoints, int thickness)
    {
        if (thickness <= 1) return linePoints;

        int r = thickness - 1;
        int r2 = r * r;
        var expanded = new HashSet<Vector2Int32>(linePoints.Count * (2 * r + 1));
        foreach (var lp in linePoints)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (dx * dx + dy * dy <= r2)
                        expanded.Add(new Vector2Int32(lp.X + dx, lp.Y + dy));
                }
            }
        }
        return expanded.ToList();
    }

    protected void FillRectangleLine(Vector2Int32 start, Vector2Int32 end)
    {
        var area = Fill.FillRectangleVectorCenter(start, end, new Vector2Int32(_wvm.Brush.Width, _wvm.Brush.Height)).ToList();
        FillSolid(area);
    }

    protected void FillRectangle(Vector2Int32 point)
    {
        var area = Fill.FillRectangleCentered(point, new Vector2Int32(_wvm.Brush.Width, _wvm.Brush.Height)).ToList();
        if (_wvm.Brush.IsOutline)
        {

            var interrior = Fill.FillRectangleCentered(
                point,
                new Vector2Int32(
                    _wvm.Brush.Width - _wvm.Brush.Outline * 2,
                    _wvm.Brush.Height - _wvm.Brush.Outline * 2)).ToList();
            FillHollow(area, interrior);
        }
        else
        {
            FillSolid(area);
        }
    }

    protected virtual void FillSolid(IList<Vector2Int32> area)
    {
        foreach (Vector2Int32 pixel in area)
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

            int index = pixel.X + pixel.Y * _wvm.CurrentWorld.TilesWide;
            if (!_wvm.CheckTiles[index])
            {
                _wvm.CheckTiles[index] = true;
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

    protected virtual void FillHollow(IList<Vector2Int32> area, IList<Vector2Int32> interrior)
    {
        var interiorSet = new HashSet<Vector2Int32>(interrior);
        IEnumerable<Vector2Int32> border = area.Where(p => !interiorSet.Contains(p));

        // Draw the border
        if (_wvm.TilePicker.TileStyleActive)
        {
            foreach (Vector2Int32 pixel in border)
            {
                if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

                int index = pixel.X + pixel.Y * _wvm.CurrentWorld.TilesWide;

                if (!_wvm.CheckTiles[index])
                {
                    _wvm.CheckTiles[index] = true;
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

                        /* Heathtech */
                        BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                    }
                }
            }
        }

        // Draw the wall in the interrior, exclude the border so no overlaps
        foreach (Vector2Int32 pixel in interrior)
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

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

                /* Heathtech */
                BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
            }
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
