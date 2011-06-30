using System.Windows;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;

namespace TEdit.Tools.Clipboard
{
    public class ClipboardBuffer
    {
        public ClipboardBuffer(PointInt32 size)
        {
            Size = size;
            Tiles = new Tile[size.X, size.Y];
        }

        private PointInt32 _size;
        public PointInt32 Size
        {
            get { return this._size; }
            set
            {
                if (this._size != value)
                {
                    this._size = value;
                    Tiles = new Tile[_size.X, _size.Y];
                }
            }
        }

        public Tile[,] Tiles { get; set; }

        public static ClipboardBuffer GetBufferedRegion(World world, Int32Rect area)
        {
            var buffer = new ClipboardBuffer(new PointInt32(area.Width, area.Height));

            for (int x = 0; x < area.Width; x++)
            {
                for (int y = 0; y < area.Height; y++)
                {
                    Tile curTile = (Tile)world.Tiles[x + area.X, y + area.Y].Clone();
                    buffer.Tiles[x, y] = curTile;
                }
            }

            return buffer;
        }

        public static void PasteBufferIntoWorld(World world, ClipboardBuffer buffer, PointInt32 TopLeft)
        {
            for (int x = 0; x < buffer.Size.X; x++)
            {
                for (int y = 0; y < buffer.Size.Y; y++)
                {
                    if (world.IsPointInWorld(x + TopLeft.X, y + TopLeft.Y))
                    {
                        Tile curTile = (Tile) buffer.Tiles[x, y].Clone();
                        world.Tiles[x + TopLeft.X, y + TopLeft.Y] = curTile;
                    }
                }
            }
        }

    }
}