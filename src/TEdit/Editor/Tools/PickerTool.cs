using System;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media.Imaging;
using TEdit.ViewModel;
using TEdit.Geometry;
using System.Collections.Generic;
using TEdit.Terraria.Objects;
using TEdit.Terraria.Editor;
using TEdit.Configuration;
using TEdit.UI;

namespace TEdit.Editor.Tools;

public sealed class PickerTool : BaseTool
{
    public PickerTool(WorldViewModel worldViewModel)
        : base(worldViewModel)
    {
        Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/eyedropper.png"));
        ToolType = ToolType.Pixel;
        Name = "Picker";
    }

    public override void MouseDown(TileMouseState e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            PickTile(e.Location.X, e.Location.Y);
        }
        else if (e.RightButton == MouseButtonState.Pressed)
        {
            PickmaskTile(e.Location.X, e.Location.Y);
        }
    }

    public override void MouseMove(TileMouseState e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            PickTile(e.Location.X, e.Location.Y);
        }
        else if (e.RightButton == MouseButtonState.Pressed)
        {
            PickmaskTile(e.Location.X, e.Location.Y);
        }
    }

    private void PickTile(int x, int y)
    {
		// Get the current tile at the specified coordinates.
        var curTile = _wvm.CurrentWorld.Tiles[x, y];
		
		if (!WorldConfiguration.TileProperties[curTile.Type].IsFramed && !curTile.IsActive) // Check if the tile is not active.
			_wvm.TilePicker.Tile = -1;
        else if (!WorldConfiguration.TileProperties[curTile.Type].IsFramed) // Check if the tile is not framed.
            _wvm.TilePicker.Tile = curTile.Type;
        else
        {
			// Retrieve the corresponding sprite configuration for the tile type.
            var spriteConfig = WorldConfiguration.Sprites2.FirstOrDefault(s => s.Tile == curTile.Type);
            if (spriteConfig != null)
            {
				// Get the style from the UV coordinates of the current tile.
                var sprite = spriteConfig.GetStyleFromUV(new Vector2Short(curTile.U, curTile.V));
                if (sprite == null)
                {
					// If no specific style is found, use the first style available.
                    sprite = spriteConfig.Styles.FirstOrDefault();
                }

                if (sprite != null)
                {
					// Set the selected sprite item and the corresponding sprite sheet.
                    _wvm.SelectedSpriteSheet = spriteConfig; // Set the corresponding SpriteSheet.
					_wvm.SelectedSpriteItem = (SpriteItemPreview)sprite;
                }
            }
        }

		// Set the other properties of the TilePicker from the current tile.
        _wvm.TilePicker.Wall = curTile.Wall;
        _wvm.TilePicker.WallColor = curTile.WallColor;
        _wvm.TilePicker.TileColor = curTile.TileColor;
        _wvm.TilePicker.LiquidType = curTile.LiquidType;
        _wvm.TilePicker.BrickStyle = curTile.BrickStyle;
        _wvm.TilePicker.RedWireActive = curTile.WireRed;
        _wvm.TilePicker.BlueWireActive = curTile.WireBlue;
        _wvm.TilePicker.GreenWireActive = curTile.WireGreen;
        _wvm.TilePicker.YellowWireActive = curTile.WireYellow;
        _wvm.TilePicker.Actuator = curTile.Actuator;
        _wvm.TilePicker.ActuatorInActive = curTile.InActive;

        // Get Picker For JunctionBoxes.
        if (curTile.Type != 424)
        {
            _wvm.TilePicker.JunctionBoxMode = JunctionBoxMode.None;
        }
        if (curTile.Type == 424 && curTile.U == 18)
        {
            _wvm.TilePicker.JunctionBoxMode = JunctionBoxMode.Left;
        }
        if (curTile.Type == 424 && curTile.U == 0)
        {
            _wvm.TilePicker.JunctionBoxMode = JunctionBoxMode.Normal;
        }
        if (curTile.Type == 424 && curTile.U == 36)
        {
            _wvm.TilePicker.JunctionBoxMode = JunctionBoxMode.Right;
        }

        // Get Picker For Liquid Amount.
        if (curTile.LiquidAmount == 0)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.ZeroPercent;
        else if (curTile.LiquidAmount <= 0x1A)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.TenPercent;
        else if (curTile.LiquidAmount <= 0x33)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.TwentyPercent;
        else if (curTile.LiquidAmount <= 0x4D)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.ThirtyPercent;
        else if (curTile.LiquidAmount <= 0x66)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.FourtyPercent;
        else if (curTile.LiquidAmount <= 0x80)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.FiftyPercent;
        else if (curTile.LiquidAmount <= 0x99)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.SixtyPercent;
        else if (curTile.LiquidAmount <= 0xB3)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.SeventyPercent;
        else if (curTile.LiquidAmount <= 0xCC)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.EightyPercent;
        else if (curTile.LiquidAmount <= 0xE6)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.NinteyPercent;
        else if (curTile.LiquidAmount <= 0xFF)
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.OneHundredPercent;
        else
            _wvm.TilePicker.LiquidAmountMode = LiquidAmountMode.OneHundredPercent;
    }

    private void PickmaskTile(int x, int y)
    {
        var curTile = _wvm.CurrentWorld.Tiles[x, y];
        if (!WorldConfiguration.TileProperties[curTile.Type].IsFramed)
            _wvm.TilePicker.TileMask = curTile.Type;
        _wvm.TilePicker.WallMask = curTile.Wall;
    }
}
