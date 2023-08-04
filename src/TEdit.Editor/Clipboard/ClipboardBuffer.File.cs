using System;
using System.IO;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Terraria;
using TEdit.Terraria.Objects;

namespace TEdit.Editor.Clipboard;

public partial class ClipboardBuffer
{
    public void Save(string filename, uint? version)
    {
        // Catch pngs that are real color
        if (string.Equals(".png", Path.GetExtension(filename), StringComparison.InvariantCultureIgnoreCase))
        {
            //Preview.SavePng(filename);
            return;
        }

        Name = Path.GetFileNameWithoutExtension(filename);
        using (var stream = new FileStream(filename, FileMode.Create))
        {
            using (var bw = new BinaryWriter(stream))
            {
                SaveV4(bw, version ?? WorldConfiguration.CompatibleVersion);
                bw.Close();
            }
        }
    }

    private void SaveV1(BinaryWriter bw, uint version)
    {
        var saveData = WorldConfiguration.SaveConfiguration.GetData(version);
        bw.Write(Name);
        bw.Write(version);
        bw.Write(Size.X);
        bw.Write(Size.Y);

        for (int x = 0; x < Size.X; x++)
        {
            int rle = 0;

            for (int y = 0; y < Size.Y; y = y + rle + 1)
            {
                var curTile = Tiles[x, y];

                var frames = WorldConfiguration.SaveConfiguration.GetData((int)version).GetFrames();
                World.WriteTileDataToStreamV1(curTile, bw, version, frames, saveData.MaxTileId, saveData.MaxWallId);

                int rleTemp = 1;
                while (y + rleTemp < Size.Y && curTile.Equals(Tiles[x, (y + rleTemp)]))
                    ++rleTemp;
                rle = rleTemp - 1;
                bw.Write((short)rle);
            }
        }
        World.WriteChestDataToStreamV1(Chests, bw, version);
        World.WriteSignDataToStreamV1(Signs, bw, version);

        bw.Write(Name);
        bw.Write(WorldConfiguration.CompatibleVersion);
        bw.Write(Size.X);
        bw.Write(Size.Y);
    }

    private void SaveV2(BinaryWriter bw, uint version)
    {
        bw.Write(Name);
        bw.Write(version);
        bw.Write(Size.X);
        bw.Write(Size.Y);

        var frames = WorldConfiguration.SaveConfiguration.GetData((int)version).GetFrames();
        World.SaveTiles(Tiles, (int)version, Size.X, Size.Y, bw, frames);
        World.SaveChests(Chests, bw, (int)version);
        World.SaveSigns(Signs, bw, (int)version);

        bw.Write(Name);
        bw.Write(WorldConfiguration.CompatibleVersion);
        bw.Write(Size.X);
        bw.Write(Size.Y);
    }

    private void SaveV3(BinaryWriter bw, uint version)
    {
        bw.Write(Name);
        bw.Write(version);
        bw.Write(Size.X);
        bw.Write(Size.Y);

        var frames = WorldConfiguration.SaveConfiguration.GetData((int)version).GetFrames();
        World.SaveTiles(Tiles, (int)version, Size.X, Size.Y, bw, frames);
        World.SaveChests(Chests, bw, (int)version);
        World.SaveSigns(Signs, bw, (int)version);
        World.SaveTileEntities(TileEntities, bw);

        bw.Write(Name);
        bw.Write(WorldConfiguration.CompatibleVersion);
        bw.Write(Size.X);
        bw.Write(Size.Y);
    }

    private void SaveV4(BinaryWriter bw, uint version)
    {
        var tileFrameImportant = this.TileFrameImportant ?? WorldConfiguration.SettingsTileFrameImportant;

        bw.Write(Name);
        bw.Write(version + 10000);
        World.WriteBitArray(bw, tileFrameImportant);
        bw.Write(Size.X);
        bw.Write(Size.Y);

        // Issues that need addressed. If the current game version does not exist
        // within the TerrariaVersionTileData.json, then the frames feature breaks all
        // together. I suggest using the max tileframe version per TEdit release and using
        // that or not allowing saves above the max release not be allowed to be saved.

        // Check if the version attemting to save exists.
        var frames = new bool[0];
        if ((int)version <= WorldConfiguration.CompatibleVersion) 
        {
            // World version is not a future release
            frames = WorldConfiguration.SaveConfiguration.GetData((int)version).GetFrames();
        }
        else
        {
            // World version is a future release. Lets downgrade it some.
            frames = WorldConfiguration.SaveConfiguration.GetData((int)WorldConfiguration.CompatibleVersion).GetFrames();
        }
        
        World.SaveTiles(Tiles, (int)version, Size.X, Size.Y, bw, frames);
        World.SaveChests(Chests, bw, (int)version);
        World.SaveSigns(Signs, bw, (int)version);
        World.SaveTileEntities(TileEntities, bw);

        bw.Write(Name);
        bw.Write(version);
        bw.Write(Size.X);
        bw.Write(Size.Y);
    }

    private static ClipboardBuffer LoadV4(BinaryReader b, string name, int version)
    {
        var tileFrameImportant = World.ReadBitArray(b);
        int sizeX = b.ReadInt32();
        int sizeY = b.ReadInt32();
        var buffer = new ClipboardBuffer(new Vector2Int32(sizeX, sizeY), tileFrameImportant: tileFrameImportant);
        buffer.Name = name;

        buffer.Tiles = World.LoadTileData(b, sizeX, sizeY, version, tileFrameImportant);
        buffer.Chests.AddRange(World.LoadChestData(b));
        buffer.Signs.AddRange(World.LoadSignData(b));
        buffer.TileEntities.AddRange(World.LoadTileEntityData(b, (uint)version));

        string verifyName = b.ReadString();
        int verifyVersion = b.ReadInt32();
        int verifyX = b.ReadInt32();
        int verifyY = b.ReadInt32();
        
        b.Close();

        if (buffer.Name == verifyName &&
            version <= verifyVersion &&
            buffer.Size.X == verifyX &&
            buffer.Size.Y == verifyY)
        {
            // valid;
            return buffer;
        }

        return null;
    }

    private static ClipboardBuffer LoadV3(BinaryReader b, string name, int version)
    {
        int sizeX = b.ReadInt32();
        int sizeY = b.ReadInt32();
        var buffer = new ClipboardBuffer(new Vector2Int32(sizeX, sizeY));
        buffer.Name = name;

        var frames = WorldConfiguration.SaveConfiguration.GetData(version).GetFrames();
        buffer.Tiles = World.LoadTileData(b, sizeX, sizeY, version, WorldConfiguration.SettingsTileFrameImportant);
        buffer.Chests.AddRange(World.LoadChestData(b));
        buffer.Signs.AddRange(World.LoadSignData(b));
        buffer.TileEntities.AddRange(World.LoadTileEntityData(b, (uint)version));

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

    private static ClipboardBuffer LoadV2(BinaryReader b, string name, uint tVersion, int version)
    {
        int sizeX = b.ReadInt32();
        int sizeY = b.ReadInt32();
        var buffer = new ClipboardBuffer(new Vector2Int32(sizeX, sizeY));
        buffer.Name = name;

        var frames = WorldConfiguration.SaveConfiguration.GetData(version).GetFrames();
        buffer.Tiles = World.LoadTileData(b, sizeX, sizeY, version, WorldConfiguration.SettingsTileFrameImportant);
        buffer.Chests.AddRange(World.LoadChestData(b));
        buffer.Signs.AddRange(World.LoadSignData(b));

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

    public static ClipboardBuffer Load(string filename)
    {
        string ext = Path.GetExtension(filename);
        if (string.Equals(ext, ".jpg", StringComparison.InvariantCultureIgnoreCase) || string.Equals(ext, ".png", StringComparison.InvariantCultureIgnoreCase) || string.Equals(ext, ".bmp", StringComparison.InvariantCultureIgnoreCase))
           throw new NotImplementedException();
//                return LoadFromImage(filename);

        using (var stream = new FileStream(filename, FileMode.Open))
        {
            using (var b = new BinaryReader(stream))
            {

                string name = b.ReadString();
                int version = b.ReadInt32();
                uint tVersion = (uint)version;

                // check all the old versions
                if (version > 10000)
                {
                    tVersion = (uint)(version - 10000);
                    return LoadV4(b, name, (int)tVersion);
                }
                if (version < 222)
                {
                    return LoadV2(b, name, tVersion, version);
                }
                if (version < 78)
                {
                    return Load5(b, name, tVersion, version);
                }
                else if (version == 4)
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
                else
                {
                // not and old version, use new version
                   return LoadV3(b, name, version);
                }
            }
        }
    }

    #region Old Versions

    //public static ClipboardBuffer LoadFromImage(string filename)
    //{
    //    var urifrompath = new Uri(filename);
    //    var bmp = new BitmapImage(urifrompath);
    //    if (bmp.Width > 8400)
    //        return null;
    //    if (bmp.Height > 2400)
    //        return null;



    //    string name = Path.GetFileNameWithoutExtension(filename);
    //    var buffer = new ClipboardBuffer(new Vector2Int32(bmp.PixelWidth, bmp.PixelHeight));
    //    buffer.Name = name;

    //    var wbmp = new WriteableBitmap(bmp);
    //    if (wbmp.Format.BitsPerPixel < 32)
    //        return null;
    //    wbmp.Lock();
    //    unsafe
    //    {
    //        var pixels = (int*)wbmp.BackBuffer;
    //        for (int y = 0; y < bmp.PixelHeight; y++)
    //        {
    //            int row = y * bmp.PixelWidth;
    //            for (int x = 0; x < bmp.PixelWidth; x++)
    //            {

    //                buffer.Tiles[x, y] = TileFromColor(pixels[x + row]);
    //            }
    //        }

    //    }
    //    wbmp.Unlock();

    //    return buffer;
    //}

    private static Tile TileFromColor(int color)
    {
        byte a = (byte)(color >> 24);
        byte r = (byte)(color >> 16);
        byte g = (byte)(color >> 8);
        byte b = (byte)(color >> 0);
        var tile = new Tile();

        // try and find a matching brick
        var tileProperty = WorldConfiguration.GetBrickFromColor(a, r, g, b);
        if (tileProperty != null && !tileProperty.IsFramed)
        {
            tile.IsActive = true;
            tile.Type = (ushort)tileProperty.Id;
        }

        // try and find a matching wall
        var wallproperty = WorldConfiguration.GetWallFromColor(a, r, g, b);
        if (wallproperty != null && !tile.IsActive)
        {
            tile.Wall = (byte)wallproperty.Id;
        }
        return tile;
    }

    private static ClipboardBuffer Load5(BinaryReader b, string name, uint tVersion, int version)
    {
        int sizeX = b.ReadInt32();
        int sizeY = b.ReadInt32();
        var buffer = new ClipboardBuffer(new Vector2Int32(sizeX, sizeY));
        buffer.Name = name;
        bool[] tileFrameImportant = WorldConfiguration.SaveConfiguration.GetTileFramesForVersion((int)version);

        for (int x = 0; x < sizeX; ++x)
        {
            for (int y = 0; y < sizeY; y++)
            {
                var tile = World.ReadTileDataFromStreamV1(b, tVersion, tileFrameImportant);
                // read complete, start compression
                buffer.Tiles[x, y] = tile;

                int rle = b.ReadInt16();
                if (rle < 0)
                    throw new ApplicationException("Invalid Tile Data!");

                if (rle > 0)
                {
                    for (int k = y + 1; k < y + rle + 1; k++)
                    {
                        var tcopy = (Tile) tile.Clone();
                        buffer.Tiles[x, k] = tcopy;
                    }
                    y = y + rle;
                }
            }
        }
        buffer.Chests.Clear();
        buffer.Chests.AddRange(World.ReadChestDataFromStreamV1(b, tVersion));

        buffer.Signs.Clear();
        foreach (Sign sign in World.ReadSignDataFromStreamV1(b, tVersion))
        {
            if (buffer.Tiles[sign.X, sign.Y].IsActive && buffer.Tiles[sign.X, sign.Y].IsSign())
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
                            tileProperty = WorldConfiguration.TileProperties[tile.Type];

                            if (tile.Type == (int)TileType.IceByRod)
                                tile.IsActive = false;

                            if (tileProperty.IsFramed)
                            {
                                tile.U = b.ReadInt16();
                                tile.V = b.ReadInt16();

                                if (tile.Type == (int)TileType.Timer)
                                    tile.V = 0;
                            }
                            else
                            {
                                tile.U = -1;
                                tile.V = -1;
                            }

                            if (b.ReadBoolean())
                            {
                                tile.TileColor = b.ReadByte();
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
                            tile.LiquidType = LiquidType.Water;
                            
                            tile.LiquidAmount = b.ReadByte();
                            bool IsLava = b.ReadBoolean();
                            if (IsLava) tile.LiquidType = LiquidType.Lava;
                            bool IsHoney = b.ReadBoolean();
                            if (IsHoney) tile.LiquidType = LiquidType.Honey;
                        }

                        tile.WireRed = b.ReadBoolean();
                        tile.WireGreen = b.ReadBoolean();
                        tile.WireBlue = b.ReadBoolean();

                        bool isHalfBrick = b.ReadBoolean();

                       
                        var brickByte = b.ReadByte();
                        if (tileProperty == null || !tileProperty.HasSlopes)
                            tile.BrickStyle = 0;
                        else
                            tile.BrickStyle = (BrickStyle)brickByte;

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

                        if (buffer.Tiles[sign.X, sign.Y].IsActive && buffer.Tiles[sign.X, sign.Y].IsSign())
                        {
                            buffer.Signs.Add(sign);
                        }
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
                                if (curTile.Type == (int)TileType.Platform)
                                {

                                    curTile.U = 0;
                                    curTile.V = 0;
                                    if (frame19)
                                    {
                                        curTile.U = br.ReadInt16();
                                        curTile.V = br.ReadInt16();
                                    }
                                }
                                else if (WorldConfiguration.TileProperties[curTile.Type].IsFramed)
                                {
                                    curTile.U = br.ReadInt16();
                                    curTile.V = br.ReadInt16();

                                    if (curTile.Type == (int)TileType.Timer)
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
                                curTile.LiquidType = LiquidType.Water;
                                curTile.LiquidAmount = br.ReadByte();
                                if (br.ReadBoolean()) // lava byte
                                    curTile.LiquidType = LiquidType.Lava;
                            }

                            curTile.WireRed = br.ReadBoolean();
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
                        if (!frame19)
                        {
                            br.Close();
                            return Load3(filename, true);
                        }
                        else
                            throw new TEdit.Common.Exceptions.TEditFileFormatException("Verification failed. Some schematic data may be missing.");
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
                                if (curTile.Type == (int)TileType.Platform)
                                {
                                    curTile.U = 0;
                                    curTile.V = 0;
                                }
                                else if (WorldConfiguration.TileProperties[curTile.Type].IsFramed)
                                {
                                    curTile.U = reader.ReadInt16();
                                    curTile.V = reader.ReadInt16();

                                    if (curTile.Type == (int)TileType.Timer)
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
                                curTile.LiquidAmount = reader.ReadByte();
                                bool lava = reader.ReadBoolean();
                                curTile.LiquidType = lava ? LiquidType.Lava : LiquidType.Water;
                            }

                            curTile.WireRed = reader.ReadBoolean();
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
                        if (buffer.Tiles[x, y].IsActive && buffer.Tiles[x, y].IsSign())
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
                    throw new TEdit.Common.Exceptions.TEditFileFormatException("Verification failed. Some schematic data may be missing.");

                return buffer;

            }
        }
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

                                if (tile.Type == (int)TileType.Platform)
                                {
                                    tile.U = 0;
                                    tile.V = 0;
                                }
                                else if (WorldConfiguration.TileProperties[tile.Type].IsFramed)
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
                                tile.LiquidType = LiquidType.Water;
                                tile.LiquidAmount = reader.ReadByte();
                                if (reader.ReadBoolean()) // lava
                                    tile.LiquidType = LiquidType.Lava;
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
                        if (buffer.Tiles[x, y].IsActive && buffer.Tiles[x, y].IsSign())
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

    #endregion
}
