using System.Collections.Generic;
using System.IO;
using BCCL.Geometry.Primitives;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Undo
{
    public class UndoBuffer
    {
        private readonly IList<UndoTile> _tiles = new List<UndoTile>();
        private readonly IList<Sign> _signs = new List<Sign>();
        private readonly IList<Chest> _chests = new List<Chest>();

        public IList<Chest> Chests
        {
            get { return _chests; }
        } 

        public IList<Sign> Signs
        {
            get { return _signs; }
        }
        public IList<UndoTile> Tiles
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
                        var curTile = Tiles[i].Tile;
                        bw.Write(curTile.IsActive);
                        if (curTile.IsActive)
                        {
                            bw.Write(curTile.Type);
                            if (World.TileProperties[curTile.Type].IsFramed)
                            {
                                bw.Write(curTile.U);
                                bw.Write(curTile.V);
                            }
                        }
                        if ((int)curTile.Wall > 0)
                        {
                            bw.Write(true);
                            bw.Write(curTile.Wall);
                        }
                        else
                            bw.Write(false);

                        if ((int)curTile.Liquid > 0)
                        {
                            bw.Write(true);
                            bw.Write(curTile.Liquid);
                            bw.Write(curTile.IsLava);
                        }
                        else
                            bw.Write(false);

                        bw.Write(curTile.HasWire);
                    }
                    for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
                    {
                        if (chestIndex >= Chests.Count)
                        {
                            bw.Write(false);
                        }
                        else
                        {
                            Chest curChest = Chests[chestIndex];
                            bw.Write(true);
                            bw.Write(curChest.X);
                            bw.Write(curChest.Y);
                            for (int j = 0; j < Chest.MaxItems; ++j)
                            {
                                if (curChest.Items.Count > j)
                                {
                                    bw.Write((byte)curChest.Items[j].StackSize);
                                    if (curChest.Items[j].StackSize > 0)
                                    {
                                        bw.Write(curChest.Items[j].NetId);
                                        bw.Write(curChest.Items[j].Prefix);
                                    }
                                }
                                else
                                    bw.Write((byte)0);
                            }
                        }
                    }
                    for (int signIndex = 0; signIndex < 1000; signIndex++)
                    {
                        if (signIndex >= Signs.Count || string.IsNullOrWhiteSpace(Signs[signIndex].Text))
                        {
                            bw.Write(false);
                        }
                        else
                        {
                            var curSign = Signs[signIndex];
                            bw.Write(true);
                            bw.Write(curSign.Text);
                            bw.Write(curSign.X);
                            bw.Write(curSign.Y);
                        }
                    }
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
                        var curTile = new Tile();
                        curTile.IsActive = br.ReadBoolean();

                        if (curTile.IsActive)
                        {
                            curTile.Type = br.ReadByte();
                            if (World.TileProperties[curTile.Type].IsFramed)
                            {
                                curTile.U = br.ReadInt16();
                                curTile.V = br.ReadInt16();
                            }
                        }

                        if (br.ReadBoolean())
                            curTile.Wall = br.ReadByte();

                        if (br.ReadBoolean())
                        {
                            curTile.Liquid = br.ReadByte();
                            curTile.IsLava = br.ReadBoolean();
                        }

                        curTile.HasWire = br.ReadBoolean();
                        buffer.Tiles.Add(new UndoTile(new Vector2Int32(x,y), curTile));
                    }

                    for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
                    {
                        if (br.ReadBoolean())
                        {
                            var curChest = new Chest(br.ReadInt32(), br.ReadInt32());
                            for (int j = 0; j < Chest.MaxItems; ++j)
                            {
                                curChest.Items[j].StackSize = br.ReadByte();

                                if (curChest.Items[j].StackSize > 0)
                                {
                                    curChest.Items[j].NetId = br.ReadInt32();
                                    curChest.Items[j].Prefix = br.ReadByte();
                                }
                                else
                                {
                                    curChest.Items[j].NetId = 0;
                                }
                            }

                            buffer.Chests.Add(curChest);
                        }
                    }
                    for (int signIndex = 0; signIndex < 1000; signIndex++)
                    {
                        if (br.ReadBoolean())
                        {
                            string text = br.ReadString();
                            int x = br.ReadInt32();
                            int y = br.ReadInt32();
                            buffer.Signs.Add(new Sign(x, y, text));
                        }
                    }
                }
            }
            return buffer;
        }
    }
}