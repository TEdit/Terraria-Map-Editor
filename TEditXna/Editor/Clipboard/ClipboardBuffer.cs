using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Geometry.Primitives;
using GalaSoft.MvvmLight;
using Microsoft.Xna.Framework;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Clipboard
{
    public partial class ClipboardBuffer : ObservableObject
    {
        public static int ClipboardRenderSize = 512;

        public ClipboardBuffer(Vector2Int32 size)
        {
            Size = size;
            Tiles = new Tile[size.X, size.Y];
        }

        private string _name;
        private WriteableBitmap _preview;

        public WriteableBitmap Preview
        {
            get { return _preview; }
            set { Set("Preview", ref _preview, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        private Vector2Int32 _size;


        public Vector2Int32 Size
        {
            get { return _size; }
            set
            {
                Set("Size", ref _size, value);
                Tiles = new Tile[_size.X, _size.Y];
            }
        }


        public Tile[,] Tiles { get; set; }

        private readonly ObservableCollection<Chest> _chests = new ObservableCollection<Chest>();
        private readonly ObservableCollection<Sign> _signs = new ObservableCollection<Sign>();
        private readonly ObservableCollection<TileEntity> _tileEntities = new ObservableCollection<TileEntity>();

        public ObservableCollection<Chest> Chests
        {
            get { return _chests; }
        }

        private double _renderScale;


        public double RenderScale
        {
            get { return _renderScale; }
            set { Set("RenderScale", ref _renderScale, value); }
        }

        public ObservableCollection<Sign> Signs
        {
            get { return _signs; }
        }

        public ObservableCollection<TileEntity> TileEntities
        {
            get { return _tileEntities; }
        }
        // since we are using these functions to add chests into the world we don't need to check all spots, only the anchor spot
        public Chest GetChestAtTile(int x, int y, int tileType)
        {
            return Chests.FirstOrDefault(c => (c.X == x) && (c.Y == y));
        }

        public Sign GetSignAtTile(int x, int y)
        {
            return Signs.FirstOrDefault(c => (c.X == x) && (c.Y == y));
        }

        public TileEntity GetTileEntityAtTile(int x, int y)
        {
        	return TileEntities.FirstOrDefault(c => (c.PosX == x) && (c.PosY == y));
        }

        public void RenderBuffer()
        {
            double scale = Math.Max((double)Size.X / ClipboardRenderSize, (double)Size.Y / ClipboardRenderSize);

            int previewX = Size.X;
            int previewY = Size.Y;
            if (scale > 1.0)
            {
                previewX = (int)MathHelper.Clamp((float)Math.Min(ClipboardRenderSize, Size.X / scale), 1, ClipboardRenderSize);
                previewY = (int)MathHelper.Clamp((float)Math.Min(ClipboardRenderSize, Size.Y / scale), 1, ClipboardRenderSize);
            }
            else
                scale = 1;

            var bmp = new WriteableBitmap(previewX, previewY, 96, 96, PixelFormats.Bgra32, null);
            for (int x = 0; x < previewX; x++)
            {
                int tileX = (int)MathHelper.Clamp((float)(scale * x), 0, Size.X - 1);

                for (int y = 0; y < previewY; y++)
                {
                    int tileY = (int)MathHelper.Clamp((float)(scale * y), 0, Size.Y - 1);

                    var color = Render.PixelMap.GetTileColor(Tiles[tileX, tileY], Microsoft.Xna.Framework.Color.Transparent);
                    bmp.SetPixel(x, y, color.A, color.R, color.G, color.B);
                }
            }
            Preview = bmp;
            RenderScale = scale;
        }
    }
}
