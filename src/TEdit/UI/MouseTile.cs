using System;
using System.Collections.Generic;
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

    private static string GetLiquidName(LiquidType type) => type switch
    {
        LiquidType.Water => Language.LiquidType_Water,
        LiquidType.Lava => Language.LiquidType_Lava,
        LiquidType.Honey => Language.LiquidType_Honey,
        LiquidType.Shimmer => Language.LiquidType_Shimmer,
        _ => type.ToString(),
    };

    private static string GetPaintName(byte colorId)
    {
        if (colorId == 0) return null;
        if (colorId < WorldConfiguration.PaintProperties.Count)
            return WorldConfiguration.PaintProperties[colorId].LocalizedName;
        return string.Format(Language.statusbar_unknown_paint, colorId);
    }

    private void UpdateTileInfo(Tile tile)
    {
        // Tile name
        if (WorldConfiguration.TileProperties.Count > tile.Type)
        {
            var tileProperty = WorldConfiguration.TileProperties[tile.Type];
            TileName = tile.IsActive ? $"{tileProperty.LocalizedName} ({tile.Type})" : Language.statusbar_empty_tile;
        }
        else
        {
            TileName = string.Format(Language.statusbar_invalid_tile, tile.Type);
        }

        // Wall name
        if (WorldConfiguration.WallProperties.Count > tile.Wall)
            WallName = $"{WorldConfiguration.WallProperties[tile.Wall].LocalizedName} ({tile.Wall})";
        else
            WallName = string.Format(Language.statusbar_invalid_wall, tile.Wall);

        // UV
        UV = new Vector2Short(tile.U, tile.V);

        // Extras
        var extras = new List<string>();
        if (tile.LiquidAmount > 0)
            extras.Add($"{GetLiquidName(tile.LiquidType)}: {tile.LiquidAmount}");

        if (tile.InActive)
            extras.Add(Language.mask_section_inactive);

        if (tile.Actuator)
            extras.Add(Language.mask_section_actuator);

        if (tile.WireRed || tile.WireBlue || tile.WireGreen || tile.WireYellow)
        {
            var colors = new List<string>();
            if (tile.WireRed) colors.Add(Language.tool_paint_wire_red);
            if (tile.WireGreen) colors.Add(Language.tool_paint_wire_green);
            if (tile.WireBlue) colors.Add(Language.tool_paint_wire_blue);
            if (tile.WireYellow) colors.Add(Language.tool_paint_wire_yellow);
            extras.Add($"{Language.PaintMode_Wire}: {string.Join(", ", colors)}");
        }

        TileExtras = string.Join(", ", extras);

        // Paint
        string tilePaint = GetPaintName(tile.TileColor);
        string wallPaint = GetPaintName(tile.WallColor);

        if (tilePaint != null && wallPaint != null)
            Paint = $"{Language.statusbar_tile_label}: {tilePaint}, {Language.statusbar_wall_label}: {wallPaint}";
        else if (tilePaint != null)
            Paint = $"{Language.statusbar_tile_label}: {tilePaint}";
        else if (wallPaint != null)
            Paint = $"{Language.statusbar_wall_label}: {wallPaint}";
        else
            Paint = Language.common_none;
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
