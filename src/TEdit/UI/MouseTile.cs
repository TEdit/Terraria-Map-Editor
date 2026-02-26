using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
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
    private Vector2Short _uV;

    [Reactive]
    private Tile _tile;

    public MouseTile()
    {
        this.WhenAnyValue(x => x.Tile)
            .Subscribe(tile =>
            {
                if (tile == null) return;
                UpdateTileInfo(tile);
            });
    }

    private static string GetPaintName(byte colorId)
    {
        if (colorId == 0) return null;
        if (colorId < WorldConfiguration.PaintProperties.Count)
            return WorldConfiguration.PaintProperties[colorId].Name;
        return $"Unknown ({colorId})";
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
        string tilePaint = GetPaintName(tile.TileColor);
        string wallPaint = GetPaintName(tile.WallColor);

        if (tilePaint != null && wallPaint != null)
            Paint = $"Tile: {tilePaint}, Wall: {wallPaint}";
        else if (tilePaint != null)
            Paint = $"Tile: {tilePaint}";
        else if (wallPaint != null)
            Paint = $"Wall: {wallPaint}";
        else
            Paint = "None";
    }
}
