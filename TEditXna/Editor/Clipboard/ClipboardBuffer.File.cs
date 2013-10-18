using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BCCL.Geometry.Primitives;
using TEditXna.Helper;
using TEditXNA.Terraria;
using TEditXNA.Terraria.Objects;

namespace TEditXna.Editor.Clipboard
{
    public partial class ClipboardBuffer
    {
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
                    bw.Write(World.CompatibleVersion);
                    bw.Write(Size.X);
                    bw.Write(Size.Y);

                    for (int x = 0; x < Size.X; x++)
                    {
                        int rle = 0;

                        for (int y = 0; y < Size.Y; y = y + rle + 1)
                        {
                            var curTile = Tiles[x, y];

                            World.WriteTileDataToStream(curTile, bw);

                            int rleTemp = 1;
                            while (y + rleTemp < Size.Y && curTile.Equals(Tiles[x, (y + rleTemp)]))
                                ++rleTemp;
                            rle = rleTemp - 1;
                            bw.Write((short)rle);
                        }
                    }
                    World.WriteChestDataToStream(Chests, bw);
                    World.WriteSignDataToStream(Signs, bw);

                    bw.Write(Name);
                    bw.Write(World.CompatibleVersion);
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
                using (var b = new BinaryReader(stream))
                {
                    string name = b.ReadString();
                    int version = b.ReadInt32();
                    uint tVersion = (uint)version;

                    if (version == 4)
                    {
                        b.Close();
                        stream.Close();
                        return Load4(filename);
                    }
                    else if (version == 3)
                    {
                        b.Close();
                        stream.Close();
                        return Load3(filename);
                    }
                    else if (version == 2)
                    {
                        b.Close();
                        stream.Close();
                        return Load2(filename);
                    }
                    else if (version < 2)
                    {
                        b.Close();
                        stream.Close();
                        return LoadOld(filename);
                    }


                    int sizeX = b.ReadInt32();
                    int sizeY = b.ReadInt32();
                    var buffer = new ClipboardBuffer(new Vector2Int32(sizeX, sizeY));
                    buffer.Name = name;

                    for (int x = 0; x < sizeX; ++x)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            var tile = World.ReadTileDataFromStream(b, tVersion);
                            // read complete, start compression
                            buffer.Tiles[x, y] = tile;

                            int rle = b.ReadInt16();
                            if (rle < 0)
                                throw new ApplicationException("Invalid Tile Data!");

                            if (rle > 0)
                            {
                                for (int k = y + 1; k < y + rle + 1; k++)
                                {
                                    var tcopy = (Tile)tile.Clone();
                                    buffer.Tiles[x, k] = tcopy;
                                }
                                y = y + rle;
                            }
                        }
                    }
                    buffer.Chests.Clear();
                    buffer.Chests.AddRange(World.ReadChestDataFromStream(b, tVersion));

                    buffer.Signs.Clear();
                    foreach (Sign sign in World.ReadSignDataFromStream(b))
                    {
                        if (buffer.Tiles[sign.X, sign.Y].IsActive && (int)buffer.Tiles[sign.X, sign.Y].Type == 55 && (int)buffer.Tiles[sign.X, sign.Y].Type == 85)
                            buffer.Signs.Add(sign);
                    }

                    string verifyName = b.ReadString();
                    int verifyVersion = b.ReadInt32();
                    int verifyX = b.ReadInt32();
                    int verifyY = b.ReadInt32();
                    if (buffer.Name == verifyName &&
                        version == verifyVersion &&
                        buffer.Size.X == verifyX &&
                        buffer.Size.Y == verifyY)
                    {
                        // valid;
                        return buffer;
                    }
                    b.Close();
                    return null;
                }
            }
        }

        public static ClipboardBuffer Load4(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var b = new BinaryReader(stream))
                {

                    string name = b.ReadString();
                    int version = b.ReadInt32();

                    int sizeX = b.ReadInt32();
                    int sizeY = b.ReadInt32();
                    var buffer = new ClipboardBuffer(new Vector2Int32(sizeX, sizeY));
                    buffer.Name = name;

                    for (int x = 0; x < sizeX; ++x)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            var tile = new Tile();

                            tile.IsActive = b.ReadBoolean();

                            TileProperty tileProperty = null;
                            if (tile.IsActive)
                            {
                                tile.Type = b.ReadByte();
                                tileProperty = World.TileProperties[tile.Type];

                                if (tile.Type == 127)
                                    tile.IsActive = false;

                                if (tileProperty.IsFramed)
                                {
                                    tile.U = b.ReadInt16();
                                    tile.V = b.ReadInt16();

                                    if (tile.Type == 144) //timer
                                        tile.V = 0;
                                }
                                else
                                {
                                    tile.U = -1;
                                    tile.V = -1;
                                }

                                if (b.ReadBoolean())
                                {
                                    tile.Color = b.ReadByte();
                                }
                            }

                            if (b.ReadBoolean())
                            {
                                tile.Wall = b.ReadByte();
                                if (b.ReadBoolean())
                                    tile.WallColor = b.ReadByte();
                            }

                            if (b.ReadBoolean())
                            {
                                tile.Liquid = b.ReadByte();
                                tile.IsLava = b.ReadBoolean();
                                tile.IsHoney = b.ReadBoolean();
                            }

                            tile.HasWire = b.ReadBoolean();
                            tile.HasWire2 = b.ReadBoolean();
                            tile.HasWire3 = b.ReadBoolean();
                            tile.HalfBrick = b.ReadBoolean();

                            if (tileProperty == null || !tileProperty.IsSolid)
                                tile.HalfBrick = false;

                            tile.Slope = b.ReadByte();
                            if (tileProperty == null || !tileProperty.IsSolid)
                                tile.Slope = 0;

                            tile.Actuator = b.ReadBoolean();
                            tile.InActive = b.ReadBoolean();

                            // read complete, start compression
                            buffer.Tiles[x, y] = tile;

                            int rle = b.ReadInt16();
                            if (rle < 0)
                                throw new ApplicationException("Invalid Tile Data!");

                            if (rle > 0)
                            {
                                for (int k = y + 1; k < y + rle + 1; k++)
                                {
                                    var tcopy = (Tile)tile.Clone();
                                    buffer.Tiles[x, k] = tcopy;
                                }
                                y = y + rle;
                            }
                        }
                    }
                    for (int i = 0; i < 1000; i++)
                    {
                        if (b.ReadBoolean())
                        {
                            var chest = new Chest(b.ReadInt32(), b.ReadInt32());
                            for (int slot = 0; slot < Chest.MaxItems; slot++)
                            {
                                if (slot < Chest.MaxItems)
                                {
                                    int stackSize = (int)b.ReadInt16();
                                    chest.Items[slot].StackSize = stackSize;

                                    if (chest.Items[slot].StackSize > 0)
                                    {
                                        chest.Items[slot].NetId = b.ReadInt32();
                                        chest.Items[slot].StackSize = stackSize;
                                        chest.Items[slot].Prefix = b.ReadByte();
                                    }
                                }
                            }
                            buffer.Chests.Add(chest);
                        }
                    }

                    for (int i = 0; i < 1000; i++)
                    {
                        if (b.ReadBoolean())
                        {
                            Sign sign = new Sign();
                            sign.Text = b.ReadString();
                            sign.X = b.ReadInt32();
                            sign.Y = b.ReadInt32();

                            if (buffer.Tiles[sign.X, sign.Y].IsActive && (int)buffer.Tiles[sign.X, sign.Y].Type == 55 && (int)buffer.Tiles[sign.X, sign.Y].Type == 85)
                                buffer.Signs.Add(sign);
                        }
                    }

                    string verifyName = b.ReadString();
                    int verifyVersion = b.ReadInt32();
                    int verifyX = b.ReadInt32();
                    int verifyY = b.ReadInt32();
                    if (buffer.Name == verifyName &&
                        version == verifyVersion &&
                        buffer.Size.X == verifyX &&
                        buffer.Size.Y == verifyY)
                    {
                        // valid;
                        return buffer;
                    }
                    b.Close();
                    return null;
                }
            }
        }

        public static ClipboardBuffer Load3(string filename, bool frame19 = false)
        {
            bool failed = false;
            try
            {
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    using (var br = new BinaryReader(stream))
                    {
                        string name = br.ReadString();
                        int version = br.ReadInt32();

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
                                    if (curTile.Type == 19) // fix for platforms
                                    {

                                        curTile.U = 0;
                                        curTile.V = 0;
                                        if (frame19)
                                        {
                                            curTile.U = br.ReadInt16();
                                            curTile.V = br.ReadInt16();
                                        }
                                    }
                                    else if (World.TileProperties[curTile.Type].IsFramed)
                                    {
                                        curTile.U = br.ReadInt16();
                                        curTile.V = br.ReadInt16();

                                        if (curTile.Type == 144) //timer
                                            curTile.V = 0;
                                    }
                                    else
                                    {
                                        curTile.U = -1;
                                        curTile.V = -1;
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
                                for (int j = 0; j < 20; ++j)
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

                        if (buffer.Name != br.ReadString() || version != br.ReadInt32() || buffer.Size.X != br.ReadInt32() || buffer.Size.Y != br.ReadInt32())
                        {
                            System.Windows.MessageBox.Show("Verification failed. Some schematic data may be missing.", "Legacy Schematic Version");
                        }

                        br.Close();
                        return buffer;
                    }
                }
            }
            catch (Exception)
            {
                failed = true;
            }

            if (failed && !frame19)
            {
                return Load3(filename, true);
            }

            return null;
        }

        public static ClipboardBuffer Load2(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    string name = reader.ReadString();
                    int version = reader.ReadInt32();
                    int maxx = reader.ReadInt32();
                    int maxy = reader.ReadInt32();

                    var buffer = new ClipboardBuffer(new Vector2Int32(maxx, maxy));

                    buffer.Name = string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(filename) : name;

                    try
                    {
                        for (int x = 0; x < maxx; x++)
                        {
                            for (int y = 0; y < maxy; y++)
                            {
                                var curTile = new Tile();
                                curTile.IsActive = reader.ReadBoolean();

                                if (curTile.IsActive)
                                {
                                    curTile.Type = reader.ReadByte();
                                    if (curTile.Type == 19) // fix for platforms
                                    {
                                        curTile.U = 0;
                                        curTile.V = 0;
                                    }
                                    else if (World.TileProperties[curTile.Type].IsFramed)
                                    {
                                        curTile.U = reader.ReadInt16();
                                        curTile.V = reader.ReadInt16();

                                        if (curTile.Type == 144) //timer
                                            curTile.V = 0;
                                    }
                                    else
                                    {
                                        curTile.U = -1;
                                        curTile.V = -1;
                                    }
                                }

                                if (reader.ReadBoolean())
                                    curTile.Wall = reader.ReadByte();

                                if (reader.ReadBoolean())
                                {
                                    curTile.Liquid = reader.ReadByte();
                                    curTile.IsLava = reader.ReadBoolean();
                                }

                                curTile.HasWire = reader.ReadBoolean();
                                buffer.Tiles[x, y] = curTile;
                            }

                        }

                    }
                    catch (Exception)
                    {
                        for (int x = 0; x < buffer.Size.X; x++)
                        {
                            for (int y = 0; y < buffer.Size.Y; y++)
                            {
                                if (buffer.Tiles[x, y] == null)
                                    buffer.Tiles[x, y] = new Tile();
                            }
                        }
                        return buffer;
                    }

                    for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
                    {
                        if (reader.ReadBoolean())
                        {
                            var chest = new Chest();
                            chest.X = reader.ReadInt32();
                            chest.Y = reader.ReadInt32();

                            for (int slot = 0; slot < 20; slot++)
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
                    string checkName = reader.ReadString();
                    int checkversion = reader.ReadInt32();
                    int checkx = reader.ReadInt32();
                    int checky = reader.ReadInt32();

                    if (checkName != buffer.Name || checkversion != version || checkx != maxx || checky != maxy)
                        System.Windows.MessageBox.Show("Verification failed. Some schematic data may be missing.", "Legacy Schematic Version");

                    return buffer;

                }
            }

            return null;
        }

        public static ClipboardBuffer LoadOld(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream))
                {
                    string name = reader.ReadString();
                    int version = reader.ReadInt32();
                    int maxx = reader.ReadInt32();
                    int maxy = reader.ReadInt32();

                    var buffer = new ClipboardBuffer(new Vector2Int32(maxx, maxy));

                    buffer.Name = string.IsNullOrWhiteSpace(name) ? Path.GetFileNameWithoutExtension(filename) : name;

                    try
                    {
                        for (int x = 0; x < buffer.Size.X; x++)
                        {
                            for (int y = 0; y < buffer.Size.Y; y++)
                            {
                                var tile = new Tile();

                                tile.IsActive = reader.ReadBoolean();

                                if (tile.IsActive)
                                {
                                    tile.Type = reader.ReadByte();

                                    if (tile.Type == 19)
                                    {
                                        tile.U = 0;
                                        tile.V = 0;
                                    }
                                    else if (World.TileProperties[tile.Type].IsFramed)
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

                    }
                    catch (Exception)
                    {
                        for (int x = 0; x < buffer.Size.X; x++)
                        {
                            for (int y = 0; y < buffer.Size.Y; y++)
                            {
                                if (buffer.Tiles[x, y] == null)
                                    buffer.Tiles[x, y] = new Tile();
                            }
                        }
                        return buffer;
                    }

                    for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
                    {
                        if (reader.ReadBoolean())
                        {
                            var chest = new Chest();
                            chest.X = reader.ReadInt32();
                            chest.Y = reader.ReadInt32();

                            for (int slot = 0; slot < 20; slot++)
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
            IsHoney = 0x80,
            IsActive = 0x40,
            IsLava = 0x20,
            HasWire = 0x10,
            HasWire2 = 0x08,
            HasWire3 = 0x04,
            Actuator = 0x02,
            InActive = 0x01,

            // still have 0x08, 0x04, 0x02, 0x01 available for additional flags
        }
        public byte TileArgsToByte(Tile tile)
        {
            byte b = 0;
            if (tile.IsActive)
                b |= (byte)TileFlags.IsActive;
            if (tile.IsLava)
                b |= (byte)TileFlags.IsLava;
            if (tile.HasWire)
                b |= (byte)TileFlags.HasWire;
            if (tile.IsHoney)
                b |= (byte)TileFlags.IsHoney;
            if (tile.HasWire2)
                b |= (byte)TileFlags.HasWire2;
            if (tile.HasWire3)
                b |= (byte)TileFlags.HasWire3;
            if (tile.Actuator)
                b |= (byte)TileFlags.Actuator;
            if (tile.InActive)
                b |= (byte)TileFlags.InActive;
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

            if ((flags & (byte)TileFlags.IsHoney) == (byte)TileFlags.IsHoney)
                tile.IsHoney = true;

            if ((flags & (byte)TileFlags.HasWire2) == (byte)TileFlags.HasWire2)
                tile.HasWire2 = true;

            if ((flags & (byte)TileFlags.HasWire3) == (byte)TileFlags.HasWire3)
                tile.HasWire3 = true;

            if ((flags & (byte)TileFlags.Actuator) == (byte)TileFlags.Actuator)
                tile.Actuator = true;

            if ((flags & (byte)TileFlags.InActive) == (byte)TileFlags.InActive)
                tile.InActive = true;
        }

    }
}
