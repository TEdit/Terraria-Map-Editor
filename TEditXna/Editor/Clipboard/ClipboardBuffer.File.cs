using System.IO;
using BCCL.Geometry.Primitives;
using TEditXNA.Terraria;

namespace TEditXna.Editor.Clipboard
{
    public partial class ClipboardBuffer
    {
        public const int SchematicVersion = 2;
        public void Save(string filename)
        {
            Name = Path.GetFileNameWithoutExtension(filename);
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                using (var bw = new BinaryWriter(stream))
                {
                    bw.Write(Name);
                    bw.Write(SchematicVersion);
                    bw.Write(Size.X);
                    bw.Write(Size.Y);

                    for (int x = 0; x < Size.X; x++)
                    {
                        for (int y = 0; y < Size.Y; y++)
                        {
                            var curTile = Tiles[x, y];
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
                                        bw.Write(curChest.Items[j].ItemName);
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

                    bw.Write(Name);
                    bw.Write(SchematicVersion);
                    bw.Write(Size.X);
                    bw.Write(Size.Y);
                    bw.Close();
                }
            }
        }

        public static ClipboardBuffer Load(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var br = new BinaryReader(stream))
                {
                    string name = br.ReadString();
                    int version = br.ReadInt32();

                    if (name != Path.GetFileNameWithoutExtension(name))
                    {
                        br.Close();
                        stream.Close();
                        return LoadOld(filename);
                    }

                    int sizeX = br.ReadInt32();
                    int sizeY = br.ReadInt32();
                    var buffer = new ClipboardBuffer(new Vector2Int32(sizeX, sizeY));
                    buffer.Name = name;

                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
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
                            buffer.Tiles[x, y] = curTile;
                        }
                    }
                    for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
                    {
                        if (br.ReadBoolean())
                        {
                            var curChest = new Chest(br.ReadInt32(), br.ReadInt32());
                            for (int j = 0; j < Chest.MaxItems; ++j)
                            {
                                var item = new Item();
                                item.StackSize = br.ReadByte();
                     
                                if (item.StackSize > 0)
                                {                        
                                    item.ItemName = br.ReadString();
                                    item.Prefix = br.ReadByte();
                                }
                                else
                                {
                                    item.ItemName = "[empty]";
                                }
                                
                                curChest.Items.Add(item);
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
                            buffer.Signs.Add(new Sign(x,y,text));
                        }
                    }

                    if (buffer.Name == br.ReadString() &&
                        version == br.ReadInt32() &&
                        buffer.Size.X == br.ReadInt32() &&
                        buffer.Size.Y == br.ReadInt32())
                    {
                        // valid;
                        return buffer;
                    }
                    br.Close();
                    return null;
                }
            }
        }

        public static ClipboardBuffer LoadOld(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    int maxx = reader.ReadInt32();
                    int maxy = reader.ReadInt32();

                    var buffer = new ClipboardBuffer(new Vector2Int32(maxx, maxy));
                    buffer.Name = Path.GetFileNameWithoutExtension(filename);

                    for (int x = 0; x < buffer.Size.X; x++)
                    {
                        for (int y = 0; y < buffer.Size.Y; y++)
                        {
                            var tile = new Tile();

                            tile.IsActive = reader.ReadBoolean();

                            if (tile.IsActive)
                            {
                                tile.Type = reader.ReadByte();

                                if (World.TileProperties[tile.Type].IsFramed && tile.Type != 4)
                                {
                                    tile.U = reader.ReadInt16();
                                    tile.V = reader.ReadInt16();
                                }
                                else
                                {
                                    tile.U = -1;
                                    tile.V = -1;
                                }
                            }

                            // trash old lighted value
                            reader.ReadBoolean();

                            if (reader.ReadBoolean())
                            {
                                tile.Wall = reader.ReadByte();
                            }

                            if (reader.ReadBoolean())
                            {
                                tile.Liquid = reader.ReadByte();
                                tile.IsLava = reader.ReadBoolean();
                            }

                            buffer.Tiles[x, y] = tile;
                        }
                    }

                    for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
                    {
                        if (reader.ReadBoolean())
                        {
                            var chest = new Chest();
                            chest.X = reader.ReadInt32();
                            chest.Y = reader.ReadInt32();

                            for (int slot = 0; slot < Chest.MaxItems; slot++)
                            {
                                var item = new Item();
                                byte stackSize = reader.ReadByte();
                                if (stackSize > 0)
                                {
                                    string itemName = reader.ReadString();
                                    item.ItemName = itemName;
                                    item.StackSize = stackSize;
                                }
                                chest.Items.Add(item);
                            }

                            //Chests[chestIndex] = chest;
                            buffer.Chests.Add(chest);
                        }
                    }
                    for (int signIndex = 0; signIndex < 1000; signIndex++)
                    {
                        if (reader.ReadBoolean())
                        {
                            string signText = reader.ReadString();
                            int x = reader.ReadInt32();
                            int y = reader.ReadInt32();
                            if (buffer.Tiles[x, y].IsActive && (buffer.Tiles[x, y].Type == 55 || buffer.Tiles[x, y].Type == 85))
                            // validate tile location
                            {
                                var sign = new Sign(x,y,signText);
                                //Signs[signIndex] = sign;
                                buffer.Signs.Add(sign);
                            }
                        }
                    }

                    int checkx = reader.ReadInt32();
                    int checky = reader.ReadInt32();

                    if (checkx == maxx && checky == maxy)
                        return buffer;

                }
            }

            return null;
        }
    }
}