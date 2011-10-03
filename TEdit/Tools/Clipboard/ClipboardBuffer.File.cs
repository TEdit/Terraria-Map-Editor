using System.IO;
using TEdit.Common.Structures;
using TEdit.Properties;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;

namespace TEdit.Tools.Clipboard
{
    public partial class ClipboardBuffer
    {
        public void Save(string filename)
        {
            this.Name = Path.GetFileNameWithoutExtension(filename);
            using (var stream = new FileStream(filename, FileMode.Create))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Size.X);
                    writer.Write(Size.Y);

                    for (int x = 0; x < Size.X; x++)
                    {
                        for (int y = 0; y < Size.Y; y++)
                        {
                            writer.Write(Tiles[x, y].IsActive);
                            if (Tiles[x, y].IsActive)
                            {
                                writer.Write(Tiles[x, y].Type);
                                if (WorldSettings.Tiles[Tiles[x, y].Type].IsFramed)
                                {
                                    writer.Write(Tiles[x, y].Frame.X);
                                    writer.Write(Tiles[x, y].Frame.Y);
                                }
                            }
                            writer.Write(Tiles[x, y].IsLighted);
                            if (Tiles[x, y].Wall > 0)
                            {
                                writer.Write(true);
                                writer.Write(Tiles[x, y].Wall);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                            if (Tiles[x, y].Liquid > 0)
                            {
                                writer.Write(true);
                                writer.Write(Tiles[x, y].Liquid);
                                writer.Write(Tiles[x, y].IsLava);
                            }
                            else
                            {
                                writer.Write(false);
                            }
                        }
                    }
                    for (int chestIndex = 0; chestIndex < World.MaxChests; chestIndex++)
                    {
                        //if (Chests[chestIndex] == null)
                        if (chestIndex >= Chests.Count)
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(Chests[chestIndex].Location.X);
                            writer.Write(Chests[chestIndex].Location.Y);
                            for (int slot = 0; slot < Chest.MaxItems; slot++)
                            {
                                if (Chests[chestIndex].Items.Count > slot)
                                {
                                    writer.Write((byte)Chests[chestIndex].Items[slot].StackSize);
                                    if (Chests[chestIndex].Items[slot].StackSize > 0)
                                    {
                                        writer.Write(Chests[chestIndex].Items[slot].Name);
                                    }
                                }
                                else
                                {
                                    writer.Write((byte)0);
                                }
                            }
                        }
                    }
                    for (int signIndex = 0; signIndex < World.MaxSigns; signIndex++)
                    {
                        //if (Signs[signIndex] == null)
                        if (signIndex >= Signs.Count)
                        {
                            writer.Write(false);
                        }
                        else if (string.IsNullOrWhiteSpace(Signs[signIndex].Text))
                        {
                            writer.Write(false);
                        }
                        else
                        {
                            writer.Write(true);
                            writer.Write(Signs[signIndex].Text);
                            writer.Write(Signs[signIndex].Location.X);
                            writer.Write(Signs[signIndex].Location.Y);
                        }
                    }

                    writer.Write(Size.X);
                    writer.Write(Size.Y);
                    writer.Close();
                }
            }
        }

        public static ClipboardBuffer Load(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    int maxx = reader.ReadInt32();
                    int maxy = reader.ReadInt32();

                    var buffer = new ClipboardBuffer(new PointInt32(maxx, maxy));
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
                                if (WorldSettings.Tiles[tile.Type].IsFramed)
                                    tile.Frame = new PointShort(reader.ReadInt16(), reader.ReadInt16());
                                else
                                    tile.Frame = new PointShort(-1, -1);
                            }

                            tile.IsLighted = reader.ReadBoolean();

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

                    for (int chestIndex = 0; chestIndex < World.MaxChests; chestIndex++)
                    {
                        if (reader.ReadBoolean())
                        {
                            var chest = new Chest();
                            chest.Location = new PointInt32(reader.ReadInt32(), reader.ReadInt32());

                            for (int slot = 0; slot < Chest.MaxItems; slot++)
                            {
                                var item = new Item();
                                byte stackSize = reader.ReadByte();
                                if (stackSize > 0)
                                {
                                    string itemName = reader.ReadString();
                                    item.Name = itemName;
                                    item.StackSize = stackSize;
                                }
                                chest.Items.Add(item);
                            }

                            //Chests[chestIndex] = chest;
                            buffer.Chests.Add(chest);
                        }
                    }
                    for (int signIndex = 0; signIndex < World.MaxSigns; signIndex++)
                    {
                        if (reader.ReadBoolean())
                        {
                            string signText = reader.ReadString();
                            int x = reader.ReadInt32();
                            int y = reader.ReadInt32();
                            if (buffer.Tiles[x, y].IsActive && (buffer.Tiles[x, y].Type == 55 || buffer.Tiles[x, y].Type == 85))
                            // validate tile location
                            {
                                var sign = new Sign();
                                sign.Location = new PointInt32(x, y);
                                sign.Text = signText;

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