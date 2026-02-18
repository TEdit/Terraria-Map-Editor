using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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

    // Spray mode
    private DispatcherTimer _sprayTimer;
    private Vector2Int32 _sprayCenter;
    private Random _sprayRandom = new Random();

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

        if (IsSimpleRectShape())
        {
            FillRectangle(_startPoint);
        }

        if (_wvm.Brush.IsSpray && _isDrawing)
        {
            _sprayCenter = e.Location;
            StartSprayTimer();
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

        if (_wvm.Brush.IsSpray && _sprayTimer != null && _sprayTimer.IsEnabled)
        {
            _sprayCenter = e.Location;
        }
        else
        {
            ProcessDraw(e.Location);
        }
    }

    public override void MouseUp(TileMouseState e)
    {
        StopSprayTimer();

        if (!_wvm.Brush.IsSpray)
            ProcessDraw(e.Location);

        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");
        _constrainDirectionLocked = false;
        _wvm.UndoManager.SaveUndo();
    }

    private void StartSprayTimer()
    {
        if (_sprayTimer == null)
        {
            _sprayTimer = new DispatcherTimer();
            _sprayTimer.Tick += SprayTimer_Tick;
        }
        _sprayTimer.Interval = TimeSpan.FromMilliseconds(_wvm.Brush.SprayTickMs);
        _sprayTimer.Start();
        // Fire immediately on first tick
        SprayTimer_Tick(null, EventArgs.Empty);
    }

    private void StopSprayTimer()
    {
        _sprayTimer?.Stop();
    }

    private void SprayTimer_Tick(object sender, EventArgs e)
    {
        var points = GetShapePoints(_sprayCenter);
        if (points.Count == 0) return;

        // Reset check tiles each tick so spray re-paints
        if (_wvm.CheckTiles != null)
        {
            Array.Clear(_wvm.CheckTiles, 0, _wvm.CheckTiles.Length);
        }

        // Partial Fisher-Yates: select SprayDensity% of points
        int count = Math.Max(1, points.Count * _wvm.Brush.SprayDensity / 100);
        var selected = new List<Vector2Int32>(count);

        // Copy to working array for shuffle
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
        // For shapes with transform, use pixel-accurate preview
        int previewW = brush.Width + 1;
        int previewH = brush.Height + 1;
        var bmp = new WriteableBitmap(previewW, previewH, 96, 96, PixelFormats.Bgra32, null);
        bmp.Clear();

        var previewColor = Color.FromArgb(127, 0, 90, 255);
        var center = new Vector2Int32(brush.Width / 2, brush.Height / 2);

        if (brush.HasTransform ||
            brush.Shape == BrushShape.Star ||
            brush.Shape == BrushShape.Triangle ||
            brush.Shape == BrushShape.Crescent ||
            brush.Shape == BrushShape.Donut)
        {
            // Pixel-accurate preview using GetShapePoints
            var points = GetShapePoints(center);
            foreach (var p in points)
            {
                if (p.X >= 0 && p.X < previewW && p.Y >= 0 && p.Y < previewH)
                    bmp.SetPixel(p.X, p.Y, previewColor);
            }
        }
        else if (IsSimpleRectShape())
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

    protected void FillShape(Vector2Int32 point)
    {
        var area = GetShapePoints(point);
        if (_wvm.Brush.IsOutline && _wvm.Brush.Shape != BrushShape.Right && _wvm.Brush.Shape != BrushShape.Left)
        {
            int interiorWidth = Math.Max(1, _wvm.Brush.Width - _wvm.Brush.Outline * 2);
            int interiorHeight = Math.Max(1, _wvm.Brush.Height - _wvm.Brush.Outline * 2);
            var interiorPoints = _wvm.Brush.GetShapePoints(point, interiorWidth, interiorHeight);
            FillHollow(area, interiorPoints);
        }
        else
        {
            FillSolid(area);
        }
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
        IEnumerable<Vector2Int32> border = area.Except(interrior).ToList();

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
