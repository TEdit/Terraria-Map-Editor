using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Properties;
using TEdit.Terraria;
using TEdit.Geometry;

namespace TEdit.UI;

public partial class MouseTile : ReactiveObject
{
    [Reactive]
    private TileMouseState _mouseState = new TileMouseState();

    [Reactive]
    private string _tileExtras;

    [Reactive]
    private string _tileName;

    [Reactive]
    private string _wallName;

    [Reactive]
    private string _paint;

    [Reactive]
    private string _depthText;

    [Reactive]
    private Vector2Short _uV;

    [Reactive]
    private Tile _tile;

    public MouseTile()
    {
        this.WhenAnyValue(x => x.Tile)
            .Subscribe(tile =>
            {
                UpdateTileInfo(tile);
            });
    }

    private void UpdateTileInfo(Tile tile)
    {
        // Tile name
        if (WorldConfiguration.TileProperties.Count > tile.Type)
        {
            var tileProperty = WorldConfiguration.TileProperties[tile.Type];
            TileName = tile.IsActive ? $"{tileProperty.Name} ({tile.Type})" : "[empty]";
        }
        else
        {
            TileName = $"INVALID TILE ({tile.Type})";
        }

        // Wall name
        if (WorldConfiguration.WallProperties.Count > tile.Wall)
            WallName = $"{WorldConfiguration.WallProperties[tile.Wall].Name} ({tile.Wall})";
        else
            WallName = $"INVALID WALL ({tile.Wall})";

        // UV
        UV = new Vector2Short(tile.U, tile.V);

        // Extras
        var extras = tile.LiquidAmount > 0
            ? $"{tile.LiquidType}: {tile.LiquidAmount}"
            : string.Empty;

        if (tile.InActive)
            extras += " Inactive";

        if (tile.Actuator)
            extras += " Actuator";

        if (tile.WireRed || tile.WireBlue || tile.WireGreen || tile.WireYellow)
        {
            extras += string.IsNullOrWhiteSpace(extras) ? "Wire " : ", Wire ";
            if (tile.WireRed) extras += "R";
            if (tile.WireGreen) extras += "G";
            if (tile.WireBlue) extras += "B";
            if (tile.WireYellow) extras += "Y";
        }

        TileExtras = extras;

        // Paint
        if (tile.TileColor > 0)
        {
            Paint = tile.WallColor > 0
                ? $"Tile: {WorldConfiguration.PaintProperties[tile.TileColor].Name}, Wall: {WorldConfiguration.PaintProperties[tile.WallColor].Name}"
                : $"Tile: {WorldConfiguration.PaintProperties[tile.TileColor].Name}";
        }
        else if (tile.WallColor > 0)
        {
            Paint = $"Wall: {WorldConfiguration.PaintProperties[tile.WallColor].Name}";
        }
        else
        {
            Paint = "None";
        }
    }

    /// <summary>
    /// Updates depth display text using Terraria's in-game GPS formulas.
    /// 1 tile = 2 feet. Depth is relative to surface; compass is relative to world center.
    /// </summary>
    public void UpdateDepth(int tileX, int tileY, int tilesWide, int tilesHigh, double groundLevel, double rockLevel)
    {
        // Compass: feet east/west from center
        int compassFeet = tileX * 2 - tilesWide;
        string compass;
        if (compassFeet > 0)
            compass = string.Format(Language.depth_east, compassFeet);
        else if (compassFeet < 0)
            compass = string.Format(Language.depth_west, -compassFeet);
        else
            compass = Language.depth_center;

        // Depth: feet above/below surface
        int depthFeet = (int)(tileY * 2 - groundLevel * 2);

        // Layer determination (matches Terraria source)
        string layer;
        if (tileY > tilesHigh - 204)
            layer = Language.depth_layer_underworld;
        else if (tileY > rockLevel)
            layer = Language.depth_layer_caverns;
        else if (depthFeet > 0)
            layer = Language.depth_layer_underground;
        else
        {
            // Space check: same formula as Terraria
            float sizeRatio = (float)tilesWide / 4200f;
            float sizeRatioSq = sizeRatio * sizeRatio;
            float spaceCheck = (float)((tileY - (65.0 + 10.0 * sizeRatioSq)) / (groundLevel / 5.0));
            layer = spaceCheck < 1.0f ? Language.depth_layer_space : Language.depth_layer_surface;
        }

        int absFeet = Math.Abs(depthFeet);
        string depth = absFeet != 0
            ? $"{string.Format(Language.depth_feet, absFeet)} {layer}"
            : $"{Language.depth_level} {layer}";

        DepthText = $"{compass}, {depth}";
    }
}
