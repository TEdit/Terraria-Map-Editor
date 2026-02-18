using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.ViewModel;
using System.Linq;
using TEdit.Terraria;
using TEdit.Render;
using TEdit.UI;
using TEdit.Terraria.DataModel;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public sealed class MorphTool : BaseTool
{
    private bool _isDrawing;
    private bool _isConstraining;
    private bool _isLineMode;
    private int _constrainDirection; // 0=horizontal, 1=vertical, 2=diagonal
    private bool _constrainDirectionLocked;
    private Vector2Int32 _anchorPoint;
    private Vector2Int32 _startPoint;
    private Vector2Int32 _endPoint;

    private MorphBiomeData _targetBiome;
    private MorphBiomeDataApplier _biomeMorpher;

    private int _dirtLayer;
    private int _rockLayer;
    private int _deepRockLayer;
    private int _hellLayer;


    public MorphTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/biome_new.png"));
        SymbolIcon = SymbolRegular.TreeEvergreen20;
        Name = "Morph";
        ToolType = ToolType.Brush;
    }

    public override void MouseDown(TileMouseState e)
    {
        var actions = GetActiveActions(e);

        _targetBiome = null;
        if (!_isDrawing && !_isConstraining && !_isLineMode)
        {
            WorldConfiguration.MorphSettings.Biomes.TryGetValue(_wvm.MorphToolOptions.TargetBiome, out _targetBiome);
            _biomeMorpher = MorphBiomeDataApplier.GetMorpher(_targetBiome);
            _startPoint = e.Location;
            _anchorPoint = e.Location;
            _constrainDirectionLocked = false;
            _dirtLayer = (int)_wvm.CurrentWorld.GroundLevel;
            _rockLayer = (int)_wvm.CurrentWorld.RockLevel;
            _deepRockLayer = (int)(_wvm.CurrentWorld.RockLevel + ((_wvm.CurrentWorld.TilesHigh - _wvm.CurrentWorld.RockLevel) / 2));
            _hellLayer = (int)(_wvm.CurrentWorld.TilesHigh - 200);
            int totalTiles = _wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh;
            if (_wvm.CheckTiles == null || _wvm.CheckTiles.Length != totalTiles)
                _wvm.CheckTiles = new int[totalTiles];
            if (++_wvm.CheckTileGeneration <= 0)
            {
                _wvm.CheckTileGeneration = 1;
                Array.Clear(_wvm.CheckTiles, 0, _wvm.CheckTiles.Length);
            }
        }

        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");

        ProcessDraw(e.Location);
    }

    public override void MouseMove(TileMouseState e)
    {
        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");
        ProcessDraw(e.Location);
    }

    public override void MouseUp(TileMouseState e)
    {
        ProcessDraw(e.Location);
        var actions = GetActiveActions(e);
        _isDrawing = actions.Contains("editor.draw");
        _isConstraining = actions.Contains("editor.draw.constrain");
        _isLineMode = actions.Contains("editor.draw.line");
        _constrainDirectionLocked = false;
        _wvm.UndoManager.SaveUndo();
    }

    public override WriteableBitmap PreviewTool()
    {
        var brush = _wvm.Brush;
        int previewW = brush.Width + 1;
        int previewH = brush.Height + 1;
        var bmp = new WriteableBitmap(previewW, previewH, 96, 96, PixelFormats.Bgra32, null);
        bmp.Clear();

        var previewColor = Color.FromArgb(127, 0, 90, 255);

        if (brush.HasTransform ||
            (brush.Shape != BrushShape.Square && brush.Shape != BrushShape.Round))
        {
            var center = new Vector2Int32(brush.Width / 2, brush.Height / 2);
            var points = GetMorphShapePoints(center);
            foreach (var p in points)
            {
                if (p.X >= 0 && p.X < previewW && p.Y >= 0 && p.Y < previewH)
                    bmp.SetPixel(p.X, p.Y, previewColor);
            }
        }
        else if (brush.Shape == BrushShape.Square)
        {
            bmp.FillRectangle(0, 0, brush.Width, brush.Height, previewColor);
        }
        else
        {
            bmp.FillEllipse(0, 0, brush.Width, brush.Height, previewColor);
        }

        _preview = bmp;
        return _preview;
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

            DrawLine(p);
            _startPoint = p;
        }
        else if (_isLineMode)
        {
            DrawLineP2P(tile);
            _endPoint = tile;
        }
        else if (_isDrawing)
        {
            DrawLine(p);
            _startPoint = p;
            _endPoint = p;
        }
    }

    private IList<Vector2Int32> GetMorphShapePoints(Vector2Int32 center)
    {
        var brush = _wvm.Brush;
        IEnumerable<Vector2Int32> points = brush.Shape switch
        {
            BrushShape.Round => Fill.FillEllipseCentered(center, new Vector2Int32(brush.Width / 2, brush.Height / 2)),
            BrushShape.Star => Fill.FillStarCentered(center, Math.Min(brush.Width, brush.Height) / 2,
                Math.Min(brush.Width, brush.Height) / 4, 5),
            BrushShape.Triangle => Fill.FillTriangleCentered(center, brush.Width / 2, brush.Height / 2),
            BrushShape.Crescent => Fill.FillCrescentCentered(center,
                Math.Min(brush.Width, brush.Height) / 2,
                (int)(Math.Min(brush.Width, brush.Height) / 2 * 0.75),
                Math.Min(brush.Width, brush.Height) / 4),
            BrushShape.Donut => Fill.FillDonutCentered(center,
                Math.Min(brush.Width, brush.Height) / 2,
                Math.Max(1, Math.Min(brush.Width, brush.Height) / 4)),
            _ => Fill.FillRectangleCentered(center, new Vector2Int32(brush.Width, brush.Height)),
        };

        if (brush.HasTransform)
            points = Fill.ApplyTransform(points, center, brush.Rotation, brush.FlipHorizontal, brush.FlipVertical);

        return points.ToList();
    }

    private void DrawLine(Vector2Int32 to)
    {
        foreach (Vector2Int32 point in Shape.DrawLineTool(_startPoint, to))
        {
            var area = GetMorphShapePoints(point);
            FillSolid(area);
        }
    }

    private void DrawLineP2P(Vector2Int32 endPoint)
    {
        foreach (Vector2Int32 point in Shape.DrawLineTool(_startPoint, _endPoint))
        {
            var area = GetMorphShapePoints(point);
            FillSolid(area);
        }
    }

    private void FillSolid(IEnumerable<Vector2Int32> area)
    {
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;
        foreach (Vector2Int32 pixel in area)
        {
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

            int index = pixel.X + pixel.Y * tilesWide;
            if (_wvm.CheckTiles[index] != generation)
            {
                _wvm.CheckTiles[index] = generation;
                if (_wvm.Selection.IsValid(pixel))
                {
                    _wvm.UndoManager.SaveTile(pixel);
                    MorphTile(pixel);
                    _wvm.UpdateRenderPixel(pixel);

                    /* Heathtech */
                    BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                }
            }
        }
    }

    private void MorphTile(Vector2Int32 p)
    {
        if (_targetBiome == null) { return; }
        var curtile = _wvm.CurrentWorld.Tiles[p.X, p.Y];

        MorphLevel level = MorphLevel.Sky;
        if (p.Y > _hellLayer) { level = MorphLevel.Hell; }
        else if (p.Y > _deepRockLayer) { level = MorphLevel.DeepRock; }
        else if (p.Y > _rockLayer) { level = MorphLevel.Rock; }
        else if (p.Y > _dirtLayer) { level = MorphLevel.Dirt; }

        _biomeMorpher.ApplyMorph(_wvm.MorphToolOptions, curtile, level, p);
    }

    public void MorphTileExternal(Vector2Int32 p)
    {
        // Always use Purify.
        WorldConfiguration.MorphSettings.Biomes.TryGetValue("Purify", out _targetBiome);
        _biomeMorpher = MorphBiomeDataApplier.GetMorpher(_targetBiome);

        var curtile = _wvm.CurrentWorld.Tiles[p.X, p.Y];

        MorphLevel level = MorphLevel.Sky;
        if (p.Y > _hellLayer) { level = MorphLevel.Hell; }
        else if (p.Y > _deepRockLayer) { level = MorphLevel.DeepRock; }
        else if (p.Y > _rockLayer) { level = MorphLevel.Rock; }
        else if (p.Y > _dirtLayer) { level = MorphLevel.Dirt; }

        _biomeMorpher.ApplyMorph(_wvm.MorphToolOptions, curtile, level, p);
        _wvm.UpdateRenderPixel(p);
    }
}
