using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using BCCL.MvvmLight;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Clipboard
{
    public partial class ClipboardBuffer : ObservableObject
    {
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

        public ObservableCollection<Chest> Chests
        {
            get { return _chests; }
        }

        public ObservableCollection<Sign> Signs
        {
            get { return _signs; }
        }

        public Chest GetChestAtTile(int x, int y)
        {
            return Chests.FirstOrDefault(c => (c.X == x || c.X == x - 1) && (c.Y == y || c.Y == y - 1));
        }

        public Sign GetSignAtTile(int x, int y)
        {
            return Signs.FirstOrDefault(c => (c.X == x || c.X == x - 1) && (c.Y == y || c.Y == y - 1));
        }

        public void RenderBuffer()
        {
            var bmp = new WriteableBitmap(Size.X, Size.Y, 96, 96, PixelFormats.Bgra32, null);
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    var color = Render.PixelMap.GetTileColor(Tiles[x, y], Microsoft.Xna.Framework.Color.Transparent);
                    bmp.SetPixel(x, y, color.A, color.R, color.G, color.B);
                }
            }
            Preview = bmp;
        }
    }
}