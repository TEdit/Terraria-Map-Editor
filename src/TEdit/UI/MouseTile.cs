using System;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.Configuration;

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

    [Reactive]
    private string _ingameLocation;

    // IngameLocation property is provided by ReactiveUI Source Generator for _ingameLocation field.

    private void UpdateIngameLocation()
    {
        try
        {
            var loc = MouseState?.Location;
            if (loc == null) { IngameLocation = string.Empty; return; }

            var app = System.Windows.Application.Current;
            var vm = app?.MainWindow?.DataContext as TEdit.ViewModel.WorldViewModel;
            var world = vm?.CurrentWorld;

            if (world == null)
            {
                IngameLocation = string.Empty;
                return;
            }

            // Use explicit World properties exposed in WorldViewModel
            int tilesWide = world.TilesWide;
            int tilesHigh = world.TilesHigh;

            int surfaceY = Convert.ToInt32(Math.Round(world.GroundLevel)); // 表层深度
            int rockLayer = Convert.ToInt32(Math.Round(world.RockLevel));   // 洞穴深度

            int centerX = (tilesWide > 0) ? tilesWide / 2 : 0;

            var x = loc.Value.X;
            var y = loc.Value.Y;

            string xPart;
            if (centerX == 0)
            {
                xPart = $"X:{x}";
            }
            else if (x < centerX)
            {
                xPart = $"西:{(centerX - x) * 2}";
            }
            else
            {
                xPart = $"东:{(x - centerX) * 2}";
            }

            string yPart;
            if (y < surfaceY)
            {

                yPart = $"地表:{(surfaceY - y) * 2}";
            }
            else if (y < rockLayer)
            {
                yPart = $"地下:{(y - surfaceY) * 2}";
            }
            else
            {
                yPart = $"洞穴:{(y - surfaceY) * 2}";
            }

            IngameLocation = $"{xPart} {yPart}";
        }
        catch
        {
            IngameLocation = string.Empty;
        }
    }

    public MouseTile()
    {
        this.WhenAnyValue(x => x.Tile)
            .Subscribe(tile =>
            {
                if (tile == null) return;
                UpdateTileInfo(tile);
            });

        // Update ingame location when mouse state reference or its Location changes
        this.WhenAnyValue(x => x.MouseState)
            .Subscribe(_ => UpdateIngameLocation());
        this.WhenAnyValue(x => x.MouseState.Location)
            .Subscribe(_ => UpdateIngameLocation());
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
