using System.Collections.Generic;
using System.IO;
using BCCL.Geometry.Primitives;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Undo
{
    public class UndoBuffer
    {
        private readonly List<UndoTile> _tiles = new List<UndoTile>();
        private readonly List<Sign> _signs = new List<Sign>();
        private readonly List<Chest> _chests = new List<Chest>();

        public List<Chest> Chests
        {
            get { return _chests; }
        }

        public List<Sign> Signs
        {
            get { return _signs; }
        }
        public List<UndoTile> Tiles
        {
            get { return _tiles; }
        }

        public void Write(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(_tiles.Count);

                    for (int i = 0; i < _tiles.Count; i++)
                    {
                        var curLocation = Tiles[i].Location;
                        bw.Write(curLocation.X);
                        bw.Write(curLocation.Y);
                        World.WriteTileDataToStreamV1(Tiles[i].Tile, bw);
                    }
                    World.WriteChestDataToStreamV1(Chests, bw);
                    World.WriteSignDataToStreamV1(Signs, bw);
                    bw.Close();
                }

            }
        }

        public static UndoBuffer Read(string filename)
        {
            var buffer = new UndoBuffer();

            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var br = new BinaryReader(stream))
                {
                    var tilecount = br.ReadInt32();
                    for (int i = 0; i < tilecount; i++)
                    {
                        int x = br.ReadInt32();
                        int y = br.ReadInt32();
                        var curTile = World.ReadTileDataFromStreamV1(br, World.CompatibleVersion);
                        buffer.Tiles.Add(new UndoTile(new Vector2Int32(x, y), curTile));
                    }
                    buffer.Chests.Clear();
                    buffer.Chests.AddRange(World.ReadChestDataFromStreamV1(br, World.CompatibleVersion));

                    buffer.Signs.Clear();
                    buffer.Signs.AddRange(World.ReadSignDataFromStreamV1(br));
                }
            }
            return buffer;
        }

        public static IEnumerable<UndoTile> ReadUndoTilesFromStream(BinaryReader br)
        {
            var tilecount = br.ReadInt32();
            for (int i = 0; i < tilecount; i++)
            {
                int x = br.ReadInt32();
                int y = br.ReadInt32();
                var curTile = World.ReadTileDataFromStreamV1(br, World.CompatibleVersion);
                yield return new UndoTile(new Vector2Int32(x, y), curTile);
            }
        }
    }
}