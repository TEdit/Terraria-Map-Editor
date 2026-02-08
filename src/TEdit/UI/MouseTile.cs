using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.Terraria;

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
}
