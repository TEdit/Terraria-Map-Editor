using System.Linq;
using System.Windows.Media.Imaging;
using TEdit.Terraria;
using TEdit.Geometry;
using TEdit.Configuration;
using System.Collections.Generic;

namespace TEdit.Editor.Clipboard
{

    public partial class ClipboardBuffer : ITileData
    {
        private string _name;
        private WriteableBitmap _preview;
        private Vector2Int32 _size;

        public ClipboardBuffer(
            Vector2Int32 size, 
            bool initTiles = false,
            bool[] tileFrameImportant = null)
        {
            Size = size;
            Tiles = new Tile[size.X, size.Y];
            TileFrameImportant = tileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;

            if (initTiles)
            {
                for (int x = 0; x < size.X; x++)
                {
                    for (int y = 0; y < size.Y; y++)
                    {
                        Tiles[x, y] = new Tile();
                    }
                }
            }
        }

        public bool[] TileFrameImportant { get; set; }
        public Tile[,] Tiles { get; set; }

        public string Name { get; set; }
        public Vector2Int32 Size
        {
            get { return _size; }
            set
            {
                _size = value;
                Tiles = new Tile[_size.X, _size.Y];
            }
        }

        public double RenderScale { get; set; }
        public List<Chest> Chests { get; } = new();
        public List<Sign> Signs { get; } = new();
        public List<TileEntity> TileEntities { get; } = new();

        // since we are using these functions to add chests into the world we don't need to check all spots, only the anchor spot
        public Chest GetChestAtTile(int x, int y, bool findOrigin = false)
        {
            return Chests.FirstOrDefault(c => (c.X == x) && (c.Y == y));
        }

        public Sign GetSignAtTile(int x, int y, bool findOrigin = false)
        {
            return Signs.FirstOrDefault(c => (c.X == x) && (c.Y == y));
        }

        public TileEntity GetTileEntityAtTile(int x, int y, bool findOrigin = false)
        {
        	return TileEntities.FirstOrDefault(c => (c.PosX == x) && (c.PosY == y));
        }

        public static ClipboardBuffer GetSelectionBuffer(World world, RectangleInt32 area)
        {
            var buffer = new ClipboardBuffer(
                new Vector2Int32(area.Width, area.Height),
                tileFrameImportant: world.TileFrameImportant);

            for (int x = 0; x < area.Width; x++)
            {
                for (int y = 0; y < area.Height; y++)
                {
                    Tile curTile = (Tile)world.Tiles[x + area.X, y + area.Y].Clone();

                    if (Tile.IsChest(curTile.Type))
                    {
                        if (buffer.GetChestAtTile(x, y) == null)
                        {
                            var anchor = world.GetAnchor(x + area.X, y + area.Y);
                            if (anchor.X == x + area.X && anchor.Y == y + area.Y)
                            {
                                var data = world.GetChestAtTile(x + area.X, y + area.Y);
                                if (data != null)
                                {
                                    var newChest = data.Copy();
                                    newChest.X = x;
                                    newChest.Y = y;
                                    buffer.Chests.Add(newChest);
                                }
                            }
                        }
                    }
                    if (Tile.IsSign(curTile.Type))
                    {
                        if (buffer.GetSignAtTile(x, y) == null)
                        {
                            var anchor = world.GetAnchor(x + area.X, y + area.Y);
                            if (anchor.X == x + area.X && anchor.Y == y + area.Y)
                            {
                                var data = world.GetSignAtTile(x + area.X, y + area.Y);
                                if (data != null)
                                {
                                    var newSign = data.Copy();
                                    newSign.X = x;
                                    newSign.Y = y;
                                    buffer.Signs.Add(newSign);
                                }
                            }
                        }
                    }
                    if (Tile.IsTileEntity(curTile.Type))
                    {
                        if (buffer.GetTileEntityAtTile(x, y) == null)
                        {
                            var anchor = world.GetAnchor(x + area.X, y + area.Y);
                            if (anchor.X == x + area.X && anchor.Y == y + area.Y)
                            {
                                var data = world.GetTileEntityAtTile(x + area.X, y + area.Y);
                                if (data != null)
                                {
                                    var newEntity = data.Copy();
                                    newEntity.PosX = (short)x;
                                    newEntity.PosY = (short)y;
                                    buffer.TileEntities.Add(newEntity);
                                }
                            }
                        }
                    }
                    buffer.Tiles[x, y] = curTile;
                }
            }

            return buffer;
        }
    }
}
