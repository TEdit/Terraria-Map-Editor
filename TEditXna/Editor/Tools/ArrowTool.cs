using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEditXNA.Terraria;
using TEdit.Geometry.Primitives;
using TEditXna.View.Popups;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public sealed class ArrowTool : BaseTool
    {
        //private ChestPopup _chestPopup;
        //private SignPopup _signPopup;

        public ArrowTool(WorldViewModel worldViewModel) : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/cursor.png"));
            ToolType = ToolType.Pixel;
            Name = "Arrow";
        }

        private bool _rightClick;
        public override void MouseDown(TileMouseState e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                _rightClick = true;
        }

        public override void MouseUp(TileMouseState e)
        {
            if (!_rightClick)
                return;

            _rightClick = false;

            Tile curTile = _wvm.CurrentWorld.Tiles[e.Location.X, e.Location.Y];
            if (Tile.IsChest(curTile.Type))
            {
                Chest chest = _wvm.CurrentWorld.GetChestAtTile(e.Location.X, e.Location.Y);
                if (chest != null)
                {
                    _wvm.SelectedChest = chest.Copy();
                    return;
                }
            }
            if (Tile.IsSign(curTile.Type))
            {
                Sign sign = _wvm.CurrentWorld.GetSignAtTile(e.Location.X, e.Location.Y);
                if (sign != null)
                {
                    _wvm.SelectedSign = sign.Copy();
                    return;
                }
            }
            if (curTile.Type == 395)
            {
                TileEntity frame = _wvm.CurrentWorld.GetTileEntityAtTile(e.Location.X, e.Location.Y);
                if (frame != null)
                {
                    _wvm.SelectedItemFrame = frame.CopyFrame();
                    return;
                }
            }
            if (curTile.Type == 128 || curTile.Type == 269)
            {
                Vector2Int32 MannLocation = _wvm.CurrentWorld.GetMannequin(e.Location.X, e.Location.Y);
                _wvm.SelectedMannHead = _wvm.CurrentWorld.Tiles[MannLocation.X, MannLocation.Y].U / 100;
                _wvm.SelectedMannBody = _wvm.CurrentWorld.Tiles[MannLocation.X, MannLocation.Y + 1].U / 100;
                _wvm.SelectedMannLegs = _wvm.CurrentWorld.Tiles[MannLocation.X, MannLocation.Y + 2].U / 100;
                _wvm.SelectedMannequin = MannLocation;
            }
        }
    }
}