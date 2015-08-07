using Microsoft.Xna.Framework;
using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TEditXNA.Terraria;
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

        private bool _leftClick = false;
        private bool _rightClick = false;

        private Vector2 _lastLeftPoint;

        public override void MouseDown(TileMouseState e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _leftClick = true;
                _lastLeftPoint = new Vector2(e.Location.X, e.Location.Y);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                _rightClick = true;
            }
        }

        public override void MouseMove(TileMouseState e)
        {
            if (_leftClick)
            {
                Vector2 current = new Vector2(e.Location.X, e.Location.Y);
                Vector2 distance = current - _lastLeftPoint;
                _wvm.RequestDragCommand.Execute(distance);
            }
        }

        public override void MouseUp(TileMouseState e)
        {
            if (_leftClick)
            {
                _leftClick = false;
            }
            if (_rightClick)
            {
                _rightClick = false;

                Chest chest = _wvm.CurrentWorld.GetChestAtTile(e.Location.X, e.Location.Y);
                if (chest != null)
                {
                    _wvm.SelectedChest = chest.Copy();
                    return;
                }

                Sign sign = _wvm.CurrentWorld.GetSignAtTile(e.Location.X, e.Location.Y);
                if (sign != null)
                {
                    _wvm.SelectedSign = sign.Copy();
                    return;
                }
            }
        }
    }
}