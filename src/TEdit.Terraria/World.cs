using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TEdit.Common.Exceptions;
using TEdit.Common.Reactive;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Terraria.IO;
using TEdit.Terraria.Objects;

namespace TEdit.Terraria;

public partial class World
{
    private SemaphoreSlim _fileSemaphore = new SemaphoreSlim(0, 1);
    private static readonly object _fileLock = new object();

    /// <summary>
    ///     Triggered when an operation reports progress.
    /// </summary>
    public static event ProgressChangedEventHandler ProgressChanged;

    public World()
    {
        NPCs.Clear();
        Signs.Clear();
        Chests.Clear();
        CharacterNames.Clear();
        TileFrameImportant = WorldConfiguration.SettingsTileFrameImportant.ToArray(); // clone for "new" world. Loaded worlds will replace this with file data
    }

    public World(int height, int width, string title, int seed = -1)
        : this()
    {
        TilesWide = width;
        TilesHigh = height;
        Title = title;
        Random r = seed <= 0 ? new Random((int)DateTime.Now.Ticks) : new Random(seed);
        WorldId = r.Next(int.MaxValue);
        WorldGUID = Guid.NewGuid();
        Seed = "";
        NPCs.Clear();
        Signs.Clear();
        Chests.Clear();
        CharacterNames.Clear();
    }

    public static async Task SaveAsync(World world, string filename, bool resetTime = false, int versionOverride = 0, bool incrementRevision = true, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        await Task.Run(() =>
        {
            lock (_fileLock)
            {
                uint currentWorldVersion = world.Version;
                try
                {
                    // Set the world version for this save
                    if (versionOverride > 0)
                    {
                        world.Version = (uint)(versionOverride);
                    }

                    if (resetTime)
                    {
                        progress?.Report(new ProgressChangedEventArgs(0, "Resetting Time..."));
                    // world.ResetTime();
                    }

                    if (filename == null)
                        return;
                    string temp = filename + ".tmp";
                    using (var fs = new FileStream(temp, FileMode.Create))
                    {
                        using (var bw = new BinaryWriter(fs))
                        {
                            if (versionOverride < 0 || world.IsV0 || world.Version == 38)
                            {
                                bool addLight = (currentWorldVersion > world.Version); // Check if world is being downgraded.
                                world.Version = (uint)Math.Abs(versionOverride);
                                SaveV0(world, bw, addLight, progress);
                            }
                            else if (world.Version > 87 && world.Version != 38)
                            {
                                SaveV2(world, bw, incrementRevision, progress);
                            }
                            else
                            {
                                bool addLight = (currentWorldVersion >= 87); // Check if world is being downgraded.
                                SaveV1(world, bw, addLight, progress);
                            }

                            if (world.IsConsole)
                            {
                                ConsoleCompressor.CompressStream(fs);
                            }

                            bw.Close();
                            fs.Close();
                            // Make a backup of the current file if it exists
                            if (File.Exists(filename))
                            {
                                string backup = filename + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".TEdit";
                                File.Copy(filename, backup, true);
                            }

                            // Replace the actual file with temp save file
                            File.Copy(temp, filename, true);
                            // Delete temp save file
                            File.Delete(temp);
                            progress?.Report(new ProgressChangedEventArgs(100, "World Save Complete."));
                        }
                    }

                    world.LastSave = File.GetLastWriteTimeUtc(filename);
                }
                finally
                {
                    // Restore the version
                    if (versionOverride > 0)
                    {
                        world.Version = currentWorldVersion;
                    }
                }
            }
        });
    }

    public static void Save(
        World world,
        string filename,
        bool resetTime = false,
        int versionOverride = 0,
        bool incrementRevision = true,
        IProgress<ProgressChangedEventArgs>? progress = null)
    {
        lock (_fileLock)
        {
            uint currentWorldVersion = world.Version;
            try
            {
                // set the world version for this save
                if (versionOverride > 0) { world.Version = (uint)(versionOverride); }

                if (resetTime)
                {
                    progress?.Report(new ProgressChangedEventArgs(0, "Resetting Time..."));
                    //world.ResetTime();
                }

                if (filename == null)
                    return;

                string temp = filename + ".tmp";
                using (var fs = new FileStream(temp, FileMode.Create))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        if (versionOverride < 0 || world.IsV0 || world.Version == 38)
                        {
                            bool addLight = (currentWorldVersion > world.Version) ? true : false; // Check if world is being downgraded.
                            world.Version = (uint)Math.Abs(versionOverride);
                            SaveV0(world, bw, addLight, progress);
                        }
                        else if (world.Version > 87 && world.Version != 38)
                        {
                            SaveV2(world, bw, incrementRevision, progress);
                        }
                        else
                        {
                            bool addLight = (currentWorldVersion >= 87) ? true : false; // Check if world is being downgraded.
                            SaveV1(world, bw, addLight, progress);
                        }

                        if (world.IsConsole)
                        {
                            ConsoleCompressor.CompressStream(fs);
                        }

                        bw.Close();
                        fs.Close();

                        // make a backup of current file if it exists
                        if (File.Exists(filename))
                        {
                            string backup = filename + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".TEdit";
                            File.Copy(filename, backup, true);
                        }
                        // replace actual file with temp save file
                        File.Copy(temp, filename, true);
                        // delete temp save file
                        File.Delete(temp);
                        progress?.Report(new ProgressChangedEventArgs(0, "World Save Complete."));
                    }
                }

                world.LastSave = File.GetLastWriteTimeUtc(filename);
            }
            finally
            {
                // restore the version
                if (versionOverride > 0) { world.Version = currentWorldVersion; }
            }
        }
    }

    public static Task<WorldValidationStatus> ValidateWorldFileAsync(string filename, IProgress<ProgressChangedEventArgs>? progress = null)
        => Task.Run(() => ValidateWorldFile(filename, progress));

    public static WorldValidationStatus ValidateWorldFile(string filename, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        var w = new World();
        uint curVersion = 0;
        WorldValidationStatus status = new();

        lock (_fileLock)
        {
            try
            {
                using (var fs = File.OpenRead(filename))
                {
                    Stream stream = fs;
                    if (ConsoleCompressor.IsCompressed(fs))
                    {
                        w.IsConsole = true;
                        status.IsConsole = true;
                        stream = ConsoleCompressor.DecompressStream(fs);
                    }

                    using (var b = new BinaryReader(stream))
                    {
                        string twldPath = Path.Combine(
                            Path.GetDirectoryName(filename),
                            Path.GetFileNameWithoutExtension(filename) +
                            ".twld");

                        if (File.Exists(twldPath))
                        {
                            w.IsTModLoader = true;
                            status.IsTModLoader = true;
                        }

                        w.Version = b.ReadUInt32();
                        status.Version = w.Version;

                        // only perform this check for v38 and under
                        if (w.Version <= 38)
                        {
                            // save the stream position
                            var readerPos = b.BaseStream.Position;

                            // read title and seed
                            w.Title = b.ReadString();
                            int seed = b.ReadInt32();
                            // if seed = 0, use load V0
                            w.IsV0 = seed == 0 && w.Version <= 38;

                            // reset the stream
                            b.BaseStream.Position = readerPos;
                        }

                        curVersion = w.Version;

                        if (w.Version < WorldConfiguration.CompatibleVersion)
                        {
                            status.IsLegacy = true;
                        }

                        if (w.Version > 87)
                        {
                            status.LoaderVersion = 2;

                        }
                        else if (w.Version <= 38 && w.IsV0)
                        {
                            status.LoaderVersion = 0;
                        }
                        else
                        {
                            status.LoaderVersion = 1;
                        }

                        // reset the stream
                        b.BaseStream.Position = (long)0;

                        progress?.Report(new ProgressChangedEventArgs(0, "Loading File Header..."));
                        // read section pointers and tile frame data
                        if (!LoadSectionHeader(b, out _, out _, w))
                            throw new TEditFileFormatException("Invalid File Format Section");

                        if (w.IsChinese)
                        {
                            status.IsChinese = true;
                        }
                    }
                }
                w.LastSave = File.GetLastWriteTimeUtc(filename);
                status.LastSave = w.LastSave;
                status.IsValid = true;
            }
            catch (Exception ex)
            {
                // save error and return status
                status.IsValid = false;
                status.Message = ex.Message;
            }
        }

        return status;
    }

    public static Task<(World World, Exception Error)> LoadWorldAsync(string filename, bool headersOnly = false, IProgress<ProgressChangedEventArgs>? progress = null)
        => Task.Run(() => LoadWorld(filename, headersOnly, progress));

    public static (World World, Exception Error) LoadWorld(string filename, bool headersOnly = false, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        var w = new World();

        lock (_fileLock)
        {
            try
            {
                using (var fs = File.OpenRead(filename))
                {
                    Stream stream = fs;
                    if (ConsoleCompressor.IsCompressed(fs))
                    {
                        w.IsConsole = true;
                        stream = ConsoleCompressor.DecompressStream(fs);
                    }

                    using (var b = new BinaryReader(stream))
                    {
                        string twldPath = Path.Combine(
                            Path.GetDirectoryName(filename),
                            Path.GetFileNameWithoutExtension(filename) +
                            ".twld");

                        w.IsTModLoader = File.Exists(twldPath);

                        w.Version = b.ReadUInt32();

                        // only perform this check for v38 and under
                        if (w.Version <= 38)
                        {
                            // save the stream position
                            var readerPos = b.BaseStream.Position;

                            // read title and seed
                            w.Title = b.ReadString();
                            int seed = b.ReadInt32();
                            // if seed = 0, use load V0
                            w.IsV0 = seed == 0 && w.Version <= 38;

                            // reset the stream
                            b.BaseStream.Position = readerPos;
                        }

                        if (w.Version > 87)
                        {
                            LoadV2(b, w, headersOnly, progress);
                        }
                        else if (w.Version <= 38 && w.IsV0)
                        {
                            LoadV0(b, filename, w, headersOnly, progress);
                        }
                        else
                        {
                            LoadV1(b, filename, w, headersOnly, progress);
                        }
                    }
                }
                w.LastSave = File.GetLastWriteTimeUtc(filename);
            }
            catch (Exception ex)
            {
                return (w, ex);
            }
        }

        return (w, null);
    }

    public void ResetTime()
    {
        DayTime = true;
        Time = 13500.0;
        MoonPhase = 0;
        BloodMoon = false;
    }

    public bool ValidTileLocation(Vector2Int32 v)
    {
        return ValidTileLocation(v.X, v.Y);
    }

    public bool ValidTileLocation(int x, int y)
    {
        return (x >= 0 && y >= 0 && y < _tilesHigh && x < TilesWide);
    }

    public Chest GetChestAtTile(int x, int y, bool findOrigin = false)
    {
        Vector2Int32 anchor = findOrigin ? GetAnchor(x, y) : new Vector2Int32(x, y);
        return Chests.FirstOrDefault(c => (c.X == anchor.X) && (c.Y == anchor.Y));
    }

    public Sign GetSignAtTile(int x, int y, bool findOrigin = false)
    {
        Vector2Int32 anchor = findOrigin ? GetAnchor(x, y) : new Vector2Int32(x, y);
        return Signs.FirstOrDefault(c => (c.X == anchor.X) && (c.Y == anchor.Y));
    }

    public TileEntity GetTileEntityAtTile(int x, int y, bool findOrigin = false)
    {
        Vector2Int32 anchor = findOrigin ? GetAnchor(x, y) : new Vector2Int32(x, y);
        return TileEntities.FirstOrDefault(c => (c.PosX == anchor.X) && (c.PosY == anchor.Y));
    }

    public Vector2Int32 GetMannequin(int x, int y)
    {
        Tile tile = Tiles[x, y];
        x -= (tile.U % 100) % 36 / 18;
        y -= tile.V / 18;
        return new Vector2Int32(x, y);
    }

    public Vector2Int32 GetRack(int x, int y)
    {
        Tile tile = Tiles[x, y];
        if (tile.U >= 5000)
        {
            x -= ((tile.U / 5000) - 1) % 3;
        }
        else
        {
            x -= tile.U % 54 / 18;
        }
        y -= tile.V / 18;
        return new Vector2Int32(x, y);
    }

    public Vector2Int32 GetXmas(int x, int y)
    {
        Tile tile = Tiles[x, y];
        if (tile.U < 10)
        {
            x -= tile.U;
            y -= tile.V;
        }
        return new Vector2Int32(x, y);
    }

    public bool IsAnchor(int x, int y)
    {
        var anchor = GetAnchor(x, y);
        return anchor.X == x && anchor.Y == y;
    }

    // find upper left corner of sprites
    public Vector2Int32 GetAnchor(int x, int y)
    {
        Tile tile = Tiles[x, y];
        if (!TileFrameImportant[tile.Type]) { return new Vector2Int32(x, y); }

        TileProperty tileprop = WorldConfiguration.TileProperties[tile.Type];
        var size = tileprop.FrameSize[0];
        if (tileprop.IsFramed && (size.X > 1 || size.Y > 1 || tileprop.FrameSize.Length > 1))
        {
            if (tile.U == 0 && tile.V == 0)
            {
                new Vector2Int32(x, y);
            }

            var sprite = WorldConfiguration.Sprites2.FirstOrDefault(s => s.Tile == tile.Type);
            var style = sprite?.GetStyleFromUV(tile.GetUV());

            var sizeTiles = style?.SizeTiles ?? sprite?.SizeTiles?.FirstOrDefault() ?? tileprop.FrameSize.FirstOrDefault();


            int xShift = tile.U % ((tileprop.TextureGrid.X + 2) * sizeTiles.X) / (tileprop.TextureGrid.X + 2);
            int yShift = tile.V % ((tileprop.TextureGrid.Y + 2) * sizeTiles.Y) / (tileprop.TextureGrid.Y + 2);
            return new Vector2Int32(x - xShift, y - yShift);
        }
        else
            return new Vector2Int32(x, y);
    }


    public Task ValidAsync(IProgress<ProgressChangedEventArgs>? progress = null) => Task.Run(() => Validate(progress));

    public void Validate(IProgress<ProgressChangedEventArgs>? progress = null)
    {
        for (int x = 0; x < TilesWide; x++)
        {
            progress?.Report(new ProgressChangedEventArgs((int)(x / (float)TilesWide * 100.0), "Validating World..."));

            for (int y = 0; y < TilesHigh; y++)
            {
                Tile curTile = Tiles[x, y];

                if (curTile.Type == (int)TileType.IceByRod)
                    curTile.IsActive = false;

                ValSpecial(x, y);
            }
        }

        foreach (Chest chest in Chests.ToArray())
        {
            bool removed = false;
            for (int x = chest.X; x < chest.X + 1; x++)
            {
                for (int y = chest.Y; y < chest.Y + 1; y++)
                {
                    if (!Tiles[x, y].IsActive || !Tiles[x, y].IsChest())
                    {
                        Chests.Remove(chest);
                        removed = true;
                        break;
                    }
                }
                if (removed) break;
            }
        }

        foreach (Sign sign in Signs.ToArray())
        {
            if (sign.Text == null)
            {
                Signs.Remove(sign);
                continue;
            }

            bool removed = false;
            for (int x = sign.X; x < sign.X + 1; x++)
            {
                for (int y = sign.Y; y < sign.Y + 1; y++)
                {
                    if (!Tiles[x, y].IsActive || !Tiles[x, y].IsSign())
                    {
                        Signs.Remove(sign);
                        removed = true;
                        break;
                    }
                }
                if (removed) break;
            }
        }


        foreach (TileEntity tileEntity in TileEntities.ToArray())
        {
            int x = tileEntity.PosX;
            int y = tileEntity.PosY;
            var anchor = GetAnchor(x, y);
            if (!Tiles[anchor.X, anchor.Y].IsActive || !Tiles[x, y].IsTileEntity())
            {
                TileEntities.Remove(tileEntity);
            }
        }

        progress?.Report(new ProgressChangedEventArgs(0, "Validating Complete..."));

        if (Chests.Count > WorldConfiguration.MaxChests)
            throw new ArgumentOutOfRangeException($"Chest Count is {Chests.Count} which is greater than {WorldConfiguration.MaxChests}.");
        if (Signs.Count > WorldConfiguration.MaxSigns)
            throw new ArgumentOutOfRangeException($"Sign Count is {Signs.Count} which is greater than {WorldConfiguration.MaxSigns}.");
    }

    private void ValSpecial(int x, int y)
    {
        Tile curTile = Tiles[x, y];
        //validate chest entry exists
        if (curTile.IsChest())
        {
            if (IsAnchor(x, y) && GetChestAtTile(x, y, true) == null)
            {
                Chests.Add(new Chest(x, y));
            }
        }
        //validate sign entry exists
        else if (curTile.IsSign())
        {
            if (IsAnchor(x, y) && GetSignAtTile(x, y, true) == null)
            {
                Signs.Add(new Sign(x, y, string.Empty));
            }
        }
        //validate TileEntity
        else if (curTile.IsTileEntity())
        {
            if (IsAnchor(x, y) && GetTileEntityAtTile(x, y, true) == null)
            {
                var TE = TileEntity.CreateForTile(curTile, x, y, TileEntities.Count);
                TileEntities.Add(TE);
            }
        }
    }

    //public void FixNpcs()
    //{
    //    TEdit.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(
    //        () =>
    //        {
    //            int[] npcids = { 17, 18, 19, 20, 22, 54, 38, 107, 108, 124, 160, 178, 207, 208, 209, 227, 228, 229, 353, 369, 441 };
    //            var existingNpcIds = new HashSet<int>(CharacterNames.Select(c => c.Id));

    //            foreach (int npcid in npcids)
    //            {
    //                if (!existingNpcIds.Contains(npcid))
    //                    CharacterNames.Add(GetNewNpc(npcid));
    //            }
    //        });
    //}

    public bool SlopeCheck(Vector2Int32 a, Vector2Int32 b)
    {
        if (a.X < 0 || a.X >= Size.X) return false;
        if (b.X < 0 || b.X >= Size.X) return false;
        if (a.Y < 0 || a.Y >= Size.Y) return false;
        if (b.Y < 0 || b.Y >= Size.Y) return false;

        var ta = Tiles[a.X, a.Y];
        var tb = Tiles[b.X, b.Y];


        var tpa = WorldConfiguration.GetTileProperties(ta.Type);
        var tpb = WorldConfiguration.GetTileProperties(tb.Type);

        if (ta.IsActive == tb.IsActive && !tpa.IsFramed && !tpb.IsFramed) return true;


        return false;
    }
}
