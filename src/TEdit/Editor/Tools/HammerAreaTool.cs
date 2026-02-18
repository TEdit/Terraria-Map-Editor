using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.ViewModel;
using TEdit.Terraria;
using TEdit.Render;

namespace TEdit.Editor.Tools;

public sealed class HammerAreaTool : BrushToolBase
{
    public HammerAreaTool(WorldViewModel worldViewModel) : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/hammer.png"));
        VectorIcon = System.Windows.Application.Current.TryFindResource("HammerIcon") as ImageSource;
        Name = "Hammer";
    }

    protected override void FillSolid(IList<Vector2Int32> area) => FillSolid(area, area.Count);

    protected override void FillSolid(IList<Vector2Int32> area, int count)
    {
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;
        for (int idx = 0; idx < count; idx++)
        {
            var pixel = area[idx];
            if (!_wvm.CurrentWorld.ValidTileLocation(pixel)) continue;

            int index = pixel.X + pixel.Y * tilesWide;
            if (_wvm.CheckTiles[index] != generation)
            {
                _wvm.CheckTiles[index] = generation;

                if (_wvm.Selection.IsValid(pixel))
                {
                    var p = GetBrickStyle(pixel);

                    if (p != null)
                    {
                        _wvm.UndoManager.SaveTile(pixel);
                        _wvm.CurrentWorld.Tiles[pixel.X, pixel.Y].BrickStyle = p.Value;
                        BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);
                    }
                }
            }
        }
    }

    private BrickStyle? GetBrickStyle(Vector2Int32 v)
    {
        var t = _wvm.CurrentWorld.Tiles[v.X, v.Y];
        var tp = WorldConfiguration.GetTileProperties(t.Type);
        if (!t.IsActive || t.LiquidType != LiquidType.None || tp.IsFramed) return null;

        bool up = _wvm.CurrentWorld.SlopeCheck(v, new Vector2Int32(v.X, v.Y - 1));
        bool down = _wvm.CurrentWorld.SlopeCheck(v, new Vector2Int32(v.X, v.Y + 1));
        bool upLeft = _wvm.CurrentWorld.SlopeCheck(v, new Vector2Int32(v.X - 1, v.Y - 1));
        bool left = _wvm.CurrentWorld.SlopeCheck(v, new Vector2Int32(v.X - 1, v.Y));
        bool upRight = _wvm.CurrentWorld.SlopeCheck(v, new Vector2Int32(v.X + 1, v.Y - 1));
        bool right = _wvm.CurrentWorld.SlopeCheck(v, new Vector2Int32(v.X + 1, v.Y));
        bool downLeft = _wvm.CurrentWorld.SlopeCheck(v, new Vector2Int32(v.X - 1, v.Y + 1));
        bool downRight = _wvm.CurrentWorld.SlopeCheck(v, new Vector2Int32(v.X + 1, v.Y + 1));

        var mask = new BitsByte(up, upRight, right, downRight, down, downLeft, left, upLeft);
        var maskValue = mask.Value;

        if (maskValue == byte.MinValue || maskValue == byte.MaxValue) return null;

        if (!up && !down) return null;

        if (!up && left && !right && (downRight || !upRight)) return BrickStyle.SlopeTopRight;
        if (!up && right && !left && (downLeft || !upLeft)) return BrickStyle.SlopeTopLeft;

        if (!down && left && !right && (!downRight || upRight)) return BrickStyle.SlopeBottomRight;
        if (!down && right && !left && (!downLeft || upLeft)) return BrickStyle.SlopeBottomLeft;

        return null;
    }

    protected override void FillHollow(IList<Vector2Int32> area, IList<Vector2Int32> interrior)
    {
        //IEnumerable<Vector2Int32> border = area.Except(interrior);
        FillSolid(area);
    }
}
