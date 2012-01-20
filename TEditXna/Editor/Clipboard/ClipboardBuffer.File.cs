using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using TEditXNA.Terraria;
using TEditXNA.Terraria.Objects;

namespace TEditXna.Editor.Clipboard
{
    public partial class ClipboardBuffer
    {
        public const int SchematicVersion = 3;

        public void Save(string filename, bool isFalseColor = false)
        {
            if (isFalseColor)
            {
                SaveFalseColor(filename);
                return;
            }

            // Catch pngs that are real color
            if (string.Equals(".png", Path.GetExtension(filename), StringComparison.InvariantCultureIgnoreCase))
            {
                Preview.SavePng(filename);
                return;
            }

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

                    bw.Write(Name);
                    bw.Write(SchematicVersion);
                    bw.Write(Size.X);
                    bw.Write(Size.Y);
                    bw.Close();
                }
            }
        }

        public static ClipboardBuffer LoadFromImage(string filename)
        {
            var urifrompath = new Uri(filename);
            var bmp = new BitmapImage(urifrompath);
            if (bmp.Width > 8400)
                return null;
            if (bmp.Height > 2400)
                return null;



            string name = Path.GetFileNameWithoutExtension(filename);
            var buffer = new ClipboardBuffer(new Vector2Int32(bmp.PixelWidth, bmp.PixelHeight));
            buffer.Name = name;

            var wbmp = new WriteableBitmap(bmp);
            if (wbmp.Format.BitsPerPixel < 32)
                return null;
            wbmp.Lock();
            unsafe
            {
                var pixels = (int*)wbmp.BackBuffer;
                for (int y = 0; y < bmp.PixelHeight; y++)
                {
                    int row = y * bmp.PixelWidth;
                    for (int x = 0; x < bmp.PixelWidth; x++)
                    {

                        buffer.Tiles[x, y] = TileFromColor(pixels[x + row]);
                    }
                }

            }
            wbmp.Unlock();

            return buffer;
        }

        private static Tile TileFromColor(int color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            var tile = new Tile();

            // try and find a matching brick
            var tileProperty = World.GetBrickFromColor(a, r, g, b);
            if (tileProperty != null && !tileProperty.IsFramed)
            {
                tile.IsActive = true;
                tile.Type = (byte)tileProperty.Id;
            }

            // try and find a matching wall
            var wallproperty = World.GetWallFromColor(a, r, g, b);
            if (wallproperty != null && !tile.IsActive)
            {
                tile.Wall = (byte)wallproperty.Id;
            }
            return tile;
        }

        public static ClipboardBuffer Load(string filename)
        {
            string ext = Path.GetExtension(filename);
            if (string.Equals(ext, ".jpg", StringComparison.InvariantCultureIgnoreCase) || string.Equals(ext, ".png", StringComparison.InvariantCultureIgnoreCase) || string.Equals(ext, ".bmp", StringComparison.InvariantCultureIgnoreCase))
                return LoadFromImage(filename);

            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var br = new BinaryReader(stream))
                {
                    string name = br.ReadString();
                    int version = br.ReadInt32();

                    if (name != Path.GetFileNameWithoutExtension(filename))
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
                                curChest.Items[j].StackSize = br.ReadByte();

                                if (curChest.Items[j].StackSize > 0)
                                {
                                    if (version >= 3)
                                        curChest.Items[j].NetId = br.ReadInt32();
                                    else
                                        curChest.Items[j].SetFromName(br.ReadString());
                                    curChest.Items[j].Prefix = br.ReadByte();
                                }
                                else
                                {
                                    curChest.Items[j].SetFromName("[empty]");
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
                                byte stackSize = reader.ReadByte();
                                if (stackSize > 0)
                                {
                                    string itemName = reader.ReadString();
                                    chest.Items[slot].SetFromName(itemName);
                                    chest.Items[slot].StackSize = stackSize;
                                }
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
                                var sign = new Sign(x, y, signText);
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

        public void SaveFalseColor(string filename)
        {
            var wbmp = new WriteableBitmap(Size.X, Size.Y, 96, 96, PixelFormats.Bgra32, null);

            for (int y = 0; y < Size.Y; y++)
            {

                for (int x = 0; x < Size.X; x++)
                {
                    var curtile = Tiles[x, y];
                    byte a = TileArgsToByte(curtile);
                    byte r = curtile.Type;
                    byte g = curtile.Wall;
                    byte b = curtile.Liquid;

                    wbmp.SetPixel(x, y, a, r, g, b);
                }
            }
            wbmp.SavePng(filename);
        }


        public static ClipboardBuffer LoadFalseColor(string filename)
        {
            var urifrompath = new Uri(filename);
            var bmp = new BitmapImage(urifrompath);
            if (bmp.Width > 8400)
                return null;
            if (bmp.Height > 2400)
                return null;

            string name = Path.GetFileNameWithoutExtension(filename);
            var buffer = new ClipboardBuffer(new Vector2Int32(bmp.PixelWidth, bmp.PixelHeight));
            buffer.Name = name;

            var wbmp = new WriteableBitmap(bmp);
            if (wbmp.Format.BitsPerPixel < 32)
                return null;
            wbmp.Lock();
            unsafe
            {
                var pixels = (int*)wbmp.BackBuffer;
                for (int y = 0; y < bmp.PixelHeight; y++)
                {
                    int row = y * bmp.PixelWidth;
                    for (int x = 0; x < bmp.PixelWidth; x++)
                    {

                        buffer.Tiles[x, y] = TileFromFalseColor(pixels[x + row]);
                    }
                }

            }
            wbmp.Unlock();

            return buffer;
        }


        private static Tile TileFromFalseColor(int color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            var tile = new Tile();

            AppendTileFlagsFromByte(ref tile, a);
            // try and find a matching brick

            var tileProperty = World.TileProperties.FirstOrDefault(t => t.Id == r);
            if (tileProperty != null && !tileProperty.IsFramed)
            {
                tile.Type = (byte)tileProperty.Id;
            }
            else
            {
                // disable missing and framed tiles
                tile.IsActive = false;
            }

            // try and find a matching wall
            var wallproperty = World.WallProperties[g];
            if (wallproperty != null && !tile.IsActive)
            {
                tile.Wall = (byte)wallproperty.Id;
            }

            tile.Liquid = b;


            return tile;
        }

        [Flags]
        public enum TileFlags : byte
        {
            IsActive = 0x40,
            IsLava = 0x20,
            HasWire = 0x10,

            // still have 0x08, 0x04, 0x02, 0x01 available for additional flags
        }
        public byte TileArgsToByte(Tile tile)
        {
            byte b = 0;
            b |= 0x80; // turn on max bit so we can see stuff (otherwise alpha is too low)

            if (tile.IsActive)
                b |= (byte)TileFlags.IsActive;
            if (tile.IsLava)
                b |= (byte)TileFlags.IsLava;
            if (tile.HasWire)
                b |= (byte)TileFlags.HasWire;

            return b;
        }

        public static void AppendTileFlagsFromByte(ref Tile tile, byte flags)
        {
            if ((flags & (byte)TileFlags.IsActive) == (byte)TileFlags.IsActive)
                tile.IsActive = true;

            if ((flags & (byte)TileFlags.IsLava) == (byte)TileFlags.IsLava)
                tile.IsLava = true;

            if ((flags & (byte)TileFlags.HasWire) == (byte)TileFlags.HasWire)
                tile.HasWire = true;
        }

    }
}