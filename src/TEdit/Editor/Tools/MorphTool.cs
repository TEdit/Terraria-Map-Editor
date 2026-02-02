using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.ViewModel;
using System.Linq;
using TEdit.Configuration;
using TEdit.Render;
using TEdit.UI;
using TEdit.Configuration.BiomeMorph;

namespace TEdit.Editor.Tools;

public sealed class MorphTool : BaseTool
{

    private bool _isLeftDown;
    private bool _isRightDown;
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
        Name = "Morph";
        ToolType = ToolType.Brush;
    }

    public override void MouseDown(TileMouseState e)
    {
        _targetBiome = null;
        if (!_isRightDown && !_isLeftDown)
        {
            WorldConfiguration.MorphSettings.Biomes.TryGetValue(_wvm.MorphToolOptions.TargetBiome, out _targetBiome);
            _biomeMorpher = MorphBiomeDataApplier.GetMorpher(_targetBiome);
            _startPoint = e.Location;
            _dirtLayer = (int)_wvm.CurrentWorld.GroundLevel;
            _rockLayer = (int)_wvm.CurrentWorld.RockLevel;
            _deepRockLayer = (int)(_wvm.CurrentWorld.RockLevel + ((_wvm.CurrentWorld.TilesHigh - _wvm.CurrentWorld.RockLevel) / 2));
            _hellLayer = (int)(_wvm.CurrentWorld.TilesHigh - 200);
            _wvm.CheckTiles = new bool[_wvm.CurrentWorld.TilesWide * _wvm.CurrentWorld.TilesHigh];
        }

        _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
        _isRightDown = (e.RightButton == MouseButtonState.Pressed);
        CheckDirectionandDraw(e.Location);
    }

    public override void MouseMove(TileMouseState e)
    {
        _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
        _isRightDown = (e.RightButton == MouseButtonState.Pressed);
        CheckDirectionandDraw(e.Location);
    }

    public override void MouseUp(TileMouseState e)
    {
        CheckDirectionandDraw(e.Location);
        _isLeftDown = (e.LeftButton == MouseButtonState.Pressed);
        _isRightDown = (e.RightButton == MouseButtonState.Pressed);
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

    private void CheckDirectionandDraw(Vector2Int32 tile)
    {
        Vector2Int32 p = tile;
        Vector2Int32 p2 = tile;
        if (_isRightDown)
        {
            if (_isLeftDown)
                p.X = _startPoint.X;
            else
                p.Y = _startPoint.Y;

            DrawLine(p);
            _startPoint = p;
        }
        else if (_isLeftDown)
        {
            if ((Keyboard.IsKeyUp(Key.LeftShift)) && (Keyboard.IsKeyUp(Key.RightShift)))
            {
                DrawLine(p);
                _startPoint = p;
                _endPoint = p;
            }
            else if ((Keyboard.IsKeyDown(Key.LeftShift)) || (Keyboard.IsKeyDown(Key.RightShift)))
            {
                DrawLineP2P(p2);
                _endPoint = p2;
            }
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
