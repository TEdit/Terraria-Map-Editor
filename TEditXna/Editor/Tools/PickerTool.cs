using System;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media.Imaging;
using TEditXNA.Terraria;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public sealed class PickerTool : BaseTool
    {
        public PickerTool(WorldViewModel worldViewModel)
            : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/eyedropper.png"));
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
            var curTile = _wvm.CurrentWorld.Tiles[x, y];
            if (!World.TileProperties[curTile.Type].IsFramed)
                _wvm.TilePicker.Tile = curTile.Type;
            else
            {
                var sprite = World.Sprites.FirstOrDefault(s => s.Tile == curTile.Type && s.Origin.X == curTile.U && s.Origin.Y == curTile.V);
                if (sprite == null)
                    sprite = World.Sprites.FirstOrDefault(s => s.Tile == curTile.Type);
                _wvm.SelectedSprite = sprite;
            }

            _wvm.TilePicker.Wall = curTile.Wall;
            _wvm.TilePicker.WallColor = curTile.WallColor;
            _wvm.TilePicker.TileColor = curTile.TileColor;
            _wvm.TilePicker.LiquidType = curTile.LiquidType;
            _wvm.TilePicker.BrickStyle = curTile.BrickStyle;
            _wvm.TilePicker.RedWireActive = curTile.WireRed;
            _wvm.TilePicker.BlueWireActive = curTile.WireGreen;
            _wvm.TilePicker.GreenWireActive = curTile.WireBlue;
            _wvm.TilePicker.YellowWireActive = curTile.WireYellow;
            _wvm.TilePicker.Actuator = curTile.Actuator;
            _wvm.TilePicker.ActuatorInActive = curTile.InActive;
        }

        private void PickmaskTile(int x, int y)
        {
            var curTile = _wvm.CurrentWorld.Tiles[x, y];
            if (!World.TileProperties[curTile.Type].IsFramed)
                _wvm.TilePicker.TileMask = curTile.Type;
            _wvm.TilePicker.WallMask = curTile.Wall;
        }
    }
}