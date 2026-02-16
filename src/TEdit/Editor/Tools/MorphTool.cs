using System;
using System.Collections.Generic;
using System.Windows.Input;
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
    private bool _constrainVertical;
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
            _wvm.CheckTiles = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
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
        var bmp = new WriteableBitmap(_wvm.Brush.Width + 1, _wvm.Brush.Height + 1, 96, 96, PixelFormats.Bgra32, null);

        bmp.Clear();
        if (_wvm.Brush.Shape == BrushShape.Square)
            bmp.FillRectangle(0, 0, _wvm.Brush.Width, _wvm.Brush.Height, Color.FromArgb(127, 0, 90, 255));
        else
            bmp.FillEllipse(0, 0, _wvm.Brush.Width, _wvm.Brush.Height, Color.FromArgb(127, 0, 90, 255));

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

    private void DrawLine(Vector2Int32 to)
    {
        IEnumerable<Vector2Int32> area;
        foreach (Vector2Int32 point in Shape.DrawLineTool(_startPoint, to))
        {
            if (_wvm.Brush.Shape == BrushShape.Round)
            {
                area = Fill.FillEllipseCentered(point, new Vector2Int32(_wvm.Brush.Width / 2, _wvm.Brush.Height / 2));
                FillSolid(area);
            }
            else if (_wvm.Brush.Shape == BrushShape.Square)
            {
                area = Fill.FillRectangleCentered(point, new Vector2Int32(_wvm.Brush.Width, _wvm.Brush.Height));
                FillSolid(area);
            }
        }
    }

    private void DrawLineP2P(Vector2Int32 endPoint)
    {
        IEnumerable<Vector2Int32> area;
        foreach (Vector2Int32 point in Shape.DrawLineTool(_startPoint, _endPoint))
        {
            if (_wvm.Brush.Shape == BrushShape.Round)
            {
                area = Fill.FillEllipseCentered(point, new Vector2Int32(_wvm.Brush.Width / 2, _wvm.Brush.Height / 2));
                FillSolid(area);
            }
            else if (_wvm.Brush.Shape == BrushShape.Square)
            {
                area = Fill.FillRectangleCentered(point, new Vector2Int32(_wvm.Brush.Width, _wvm.Brush.Height));
                FillSolid(area);
            }
        }
    }

    private void FillSolid(IEnumerable<Vector2Int32> area)
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
