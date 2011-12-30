using System;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using TEditXna.View.Popups;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public class ArrowTool : ObservableObject, ITool
    {
        private WriteableBitmap _preview;

        private bool _isActive;
        private WorldViewModel _wvm;
        public ArrowTool(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);

            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/cursor.png"));
            Name = "Arrow";
            IsActive = false;
        }

        public string Name { get; private set; }

        public ToolType ToolType { get { return ToolType.Pixel; } }

        public BitmapImage Icon { get; private set; }

        public bool IsActive
        {
            get { return _isActive; }
            set { Set("IsActive", ref _isActive, value); }
        }


        private ChestPopup _chestPopup;
        private SignPopup _signPopup;
        public void MouseDown(TileMouseState e)
        {
            ClosePopups();
        }

        public void MouseMove(TileMouseState e)
        {
        }

        public void MouseUp(TileMouseState e)
        {
            ClosePopups();
            var chest = _wvm.CurrentWorld.GetChestAtTile(e.Location.X, e.Location.Y);
            if (chest != null)
            {
                if (_chestPopup == null)
                    _chestPopup = new ChestPopup(chest);
                else
                    _chestPopup.OpenChest(chest);

                _chestPopup.IsOpen = true;
            }

            var sign = _wvm.CurrentWorld.GetSignAtTile(e.Location.X, e.Location.Y);
            if (sign != null)
            {
                if (_signPopup == null)
                    _signPopup = new SignPopup(sign);
                else
                    _signPopup.OpenSign(sign);

                _signPopup.IsOpen = true;
            }
            //// From Terrafirma
            //foreach (Chest c in _world.Chests)
            //{
            //    //chests are 2x2, and their x/y is upper left corner
            //    if (Check2x2(c.Location, e.Tile))
            //    {
            //        _chestPopup = new ChestPopup(c);
            //        _chestPopup.IsOpen = true;
            //    }
            //}
            //foreach (Sign s in _world.Signs)
            //{
            //    //signs are 2x2, and their x/y is upper left corner
            //    if (Check2x2(s.Location, e.Tile))
            //    {
            //        _signPopup = new SignPopup(s);
            //        _signPopup.IsOpen = true;
            //    }
            //}
            //return false;
        }

        private bool Check2x2(Vector2Int32 loc, Vector2Int32 hit)
        {
            return (loc.X == hit.X || loc.X + 1 == hit.X) &&
                   (loc.Y == hit.Y || loc.Y + 1 == hit.Y);
        }

        public void MouseWheel(TileMouseState e)
        {
        }

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

        public WriteableBitmap PreviewTool()
        {
            return _preview;
        }

        public bool PreviewIsTexture { get { return false; } }
    }
}