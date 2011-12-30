using System;
using System.Windows.Media.Imaging;
using TEditXNA.Terraria;
using TEditXna.View.Popups;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public sealed class ArrowTool : BaseTool
    {
        private ChestPopup _chestPopup;
        private SignPopup _signPopup;

        public ArrowTool(WorldViewModel worldViewModel) : base(worldViewModel)
        {
            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/cursor.png"));
            ToolType = ToolType.Pixel;
            Name = "Arrow";
        }

        public override void MouseDown(TileMouseState e)
        {
            ClosePopups();
        }

        public override void MouseUp(TileMouseState e)
        {
            ClosePopups();
            Chest chest = _wvm.CurrentWorld.GetChestAtTile(e.Location.X, e.Location.Y);
            if (chest != null)
            {
                if (_chestPopup == null)
                    _chestPopup = new ChestPopup(chest);
                else
                    _chestPopup.OpenChest(chest);

                _chestPopup.IsOpen = true;
            }

            Sign sign = _wvm.CurrentWorld.GetSignAtTile(e.Location.X, e.Location.Y);
            if (sign != null)
            {
                if (_signPopup == null)
                    _signPopup = new SignPopup(sign);
                else
                    _signPopup.OpenSign(sign);

                _signPopup.IsOpen = true;
            }
        }

        //private bool Check2x2(Vector2Int32 loc, Vector2Int32 hit)
        //{
        //    return (loc.X == hit.X || loc.X + 1 == hit.X) &&
        //           (loc.Y == hit.Y || loc.Y + 1 == hit.Y);
        //}

        private void ClosePopups()
        {
            if (_chestPopup != null)
            {
                _chestPopup.IsOpen = false;
                _chestPopup = null;
            }
            if (_signPopup != null)
            {
                _signPopup.IsOpen = false;
                _signPopup = null;
            }
        }
    }
}