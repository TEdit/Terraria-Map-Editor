using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.MvvmLight;
using TEditXna.ViewModel;

namespace TEditXna.Editor.Tools
{
    public class SpriteTool : ObservableObject, ITool
    {
        private WorldViewModel _wvm;
        private WriteableBitmap _preview;

        private bool _isActive;

        public SpriteTool(WorldViewModel worldViewModel)
        {
            _wvm = worldViewModel;
            _preview = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            _preview.Clear();
            _preview.SetPixel(0, 0, 127, 0, 90, 255);

            Icon = new BitmapImage(new Uri(@"pack://application:,,,/TEditXna;component/Images/Tools/sprite.png"));
            Name = "Sprite";
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

        public void MouseDown(TileMouseState e)
        {
            var tiles = _wvm.SelectedSprite.GetTiles();
            for (int x = 0; x < _wvm.SelectedSprite.Size.X; x++)
            {
                int tilex = x + e.Location.X;
                for (int y = 0; y < _wvm.SelectedSprite.Size.Y; y++)
                {

                    int tiley = y + e.Location.Y;

                    _wvm.UndoManager.SaveTile(tilex, tiley);
                    var curtile = _wvm.CurrentWorld.Tiles[tilex, tiley];
                    curtile.IsActive = true;
                    curtile.Type = _wvm.SelectedSprite.Tile;
                    curtile.U = tiles[x, y].X;
                    curtile.V = tiles[x, y].Y;
                    _wvm.UpdateRenderPixel(tilex, tiley);
                }
            }
            _wvm.UndoManager.SaveUndo();
        }

        public void MouseMove(TileMouseState e)
        {
        }

        public void MouseUp(TileMouseState e)
        {

        }

        public void MouseWheel(TileMouseState e)
        {
        }

        public WriteableBitmap PreviewTool()
        {
            if (_wvm.SelectedSprite != null)
                return _wvm.SelectedSprite.Preview;
            return _preview;
        }

        public bool PreviewIsTexture
        {
            get
            {
                if (_wvm.SelectedSprite != null)
                    return _wvm.SelectedSprite.IsPreviewTexture;
                return false;
            }
        }
    }
}