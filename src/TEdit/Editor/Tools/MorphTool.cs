using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using TEdit.Geometry;
using TEdit.ViewModel;
using TEdit.Terraria;
using TEdit.Render;
using TEdit.Terraria.DataModel;
using TEdit.UI;
using Wpf.Ui.Controls;

namespace TEdit.Editor.Tools;

public sealed class MorphTool : BrushToolBase
{
    private MorphBiomeData _targetBiome;
    private MorphBiomeDataApplier _biomeMorpher;

    public MorphTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/biome_new.png"));
        SymbolIcon = SymbolRegular.TreeEvergreen20;
        Name = "Morph";
    }

    public override void MouseDown(TileMouseState e)
    {
        // Set up morph target before base starts drawing (which may spray immediately)
        _targetBiome = null;
        if (!_isDrawing && !_isConstraining && !_isLineMode)
        {
            WorldConfiguration.MorphSettings.Biomes.TryGetValue(_wvm.MorphToolOptions.TargetBiome, out _targetBiome);
            _biomeMorpher = _targetBiome != null ? MorphBiomeDataApplier.GetMorpher(_targetBiome) : null;
        }

        base.MouseDown(e);
    }

    protected override void FillSolid(IList<Vector2Int32> area, int count)
    {
        int generation = _wvm.CheckTileGeneration;
        int tilesWide = _wvm.CurrentWorld.TilesWide;

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
                    var grownPlants = MorphTile(pixel);
                    _wvm.UpdateRenderPixel(pixel);

                    /* Heathtech */
                    BlendRules.ResetUVCache(_wvm, pixel.X, pixel.Y, 1, 1);

                    if (grownPlants != null)
                    {
                        foreach (var pos in grownPlants)
                        {
                            _wvm.UndoManager.SaveTile(pos);
                            _wvm.UpdateRenderPixel(pos);
                            BlendRules.ResetUVCache(_wvm, pos.X, pos.Y, 1, 1);
                        }
                    }
                }
            }
        }
    }

    private List<Vector2Int32> MorphTile(Vector2Int32 p)
    {
        if (_targetBiome == null || _biomeMorpher == null) { return null; }
        ref var curtile = ref _wvm.CurrentWorld.Tiles[p.X, p.Y];
        var level = MorphBiomeDataApplier.ComputeMorphLevel(_wvm.CurrentWorld, p.Y);
        return _biomeMorpher.ApplyMorph(_wvm.MorphToolOptions, _wvm.CurrentWorld, ref curtile, level, p);
    }
}
