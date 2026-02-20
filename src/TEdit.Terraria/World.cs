using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using TEdit.Common.Exceptions;
using TEdit.Geometry;
using TEdit.Terraria.IO;
using TEdit.Terraria.Objects;
using TEdit.Terraria.TModLoader;

namespace TEdit.Terraria;

public partial class World
{
    public static readonly string[] TeamNames = ["Red", "Green", "Blue", "Yellow", "Pink", "White"];
    public const int TeamCount = 6;

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

        this.WhenAnyValue(x => x.TeamBasedSpawnsSeed)
            .Subscribe(enabled =>
            {
                if (enabled && TeamSpawns.Count == 0)
                {
                    for (int i = 0; i < TeamCount; i++)
                    {
                        TeamSpawns.Add(new Vector2Int32Observable(SpawnX, SpawnY));
                    }
                }
            });
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

                    // Strip mod tile/wall data before vanilla save
                    if (world.IsTModLoader && world.TwldData != null)
                    {
                        progress?.Report(new ProgressChangedEventArgs(0, "Preparing tModLoader data..."));
                        TwldFile.StripFromWorld(world, world.TwldData);
                    }

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
                                // Check if world is being downgraded below v26.
                                // currentWorldVersion = whatever version the file is now.
                                // world.Version       = the target (downgrade‐to) version.
                                bool addLight = world.Version <= 25
                                             && currentWorldVersion > world.Version;
                                SaveV1(world, bw, addLight, progress);
                            }

                            if (world.IsConsole)
                            {
                                ConsoleCompressor.CompressStream(fs);
                            }

                            bw.Close();
                            fs.Close();
                            // Replace the actual file with temp save file
                            File.Copy(temp, filename, true);
                            // Delete temp save file
                            File.Delete(temp);
                            progress?.Report(new ProgressChangedEventArgs(100, "World Save Complete."));
                        }
                    }

                    // Save .twld sidecar and re-apply mod data to in-memory world
                    if (world.IsTModLoader && world.TwldData != null)
                    {
                        progress?.Report(new ProgressChangedEventArgs(0, "Saving tModLoader data..."));
                        TwldFile.RebuildModChestItems(world, world.TwldData);
                        TwldFile.Save(filename, world.TwldData);
                        TwldFile.ReapplyToWorld(world, world.TwldData);
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

                // Strip mod tile/wall data before vanilla save (writes air in .wld)
                if (world.IsTModLoader && world.TwldData != null)
                {
                    progress?.Report(new ProgressChangedEventArgs(0, "Preparing tModLoader data..."));
                    TwldFile.StripFromWorld(world, world.TwldData);
                }

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
                            // Check if world is being downgraded below v26.
                            // currentWorldVersion = whatever version the file is now.
                            // world.Version       = the target (downgrade‐to) version.
                            bool addLight = world.Version <= 25
                                         && currentWorldVersion > world.Version;
                            SaveV1(world, bw, addLight, progress);
                        }

                        if (world.IsConsole)
                        {
                            ConsoleCompressor.CompressStream(fs);
                        }

                        bw.Close();
                        fs.Close();

                        // replace actual file with temp save file
                        File.Copy(temp, filename, true);
                        // delete temp save file
                        File.Delete(temp);
                        progress?.Report(new ProgressChangedEventArgs(0, "World Save Complete."));
                    }
                }

                // Save .twld sidecar and re-apply mod data to in-memory world
                if (world.IsTModLoader && world.TwldData != null)
                {
                    progress?.Report(new ProgressChangedEventArgs(0, "Saving tModLoader data..."));
                    TwldFile.Save(filename, world.TwldData);
                    TwldFile.ReapplyToWorld(world, world.TwldData);
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

                        // Check if the world version is less then recorded in the config.
                        if (w.Version < WorldConfiguration.CompatibleVersion)
                        {
                            // Save the stream position.
                            var readerPos = b.BaseStream.Position;

                            // Check if the world file contains all-zeros (corrupt).
                            // Use b.BaseStream (the active stream) instead of fs,
                            // because for console worlds fs may be closed after decompression.
                            const int BufferSize = 8192;
                            var buffer = new byte[BufferSize];
                            bool foundNonZero = false;
                            int read;
                            while ((read = b.BaseStream.Read(buffer, 0, BufferSize)) > 0)
                            {
                                for (int i = 0; i < read; i++)
                                    if (buffer[i] != 0)
                                    {
                                        foundNonZero = true;
                                        break;
                                    }
                                if (foundNonZero) break;
                            }
                            if (!foundNonZero)
                            {
                                // This world file contains no data, error out.
                                status.IsCorrupt = true;
                                return status;
                            }

                            // Reset the stream.
                            b.BaseStream.Position = readerPos;

                            // World file is not empty, must be an alpha.
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

                        if (w.Version > WorldConfiguration.CompatibleVersion)
                        {
                            status.IsPreeminent = true;
                        }
                    }
                }
                w.LastSave = File.GetLastWriteTimeUtc(filename);
                status.LastSave = w.LastSave;
                status.IsValid = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"World validation failed: {ex.Message}");
                status.IsValid = false;
                status.Message = ex.Message;
            }
        }

        return status;
    }

    /// <summary>
    /// Reads only the world header (title, version, dimensions, etc.) without loading tiles/chests/NPCs.
    /// Returns null if the file cannot be read.
    /// </summary>
    public static WorldHeaderInfo ReadWorldHeader(string filename)
    {
        lock (_fileLock)
        {
            try
            {
                var fi = new FileInfo(filename);
                if (!fi.Exists) return null;

                var w = new World();
                int[] sectionPointers = null;

                using var fs = File.OpenRead(filename);
                Stream stream = fs;
                if (ConsoleCompressor.IsCompressed(fs))
                {
                    stream = ConsoleCompressor.DecompressStream(fs);
                }

                using var b = new BinaryReader(stream);

                string twldPath = Path.Combine(
                    Path.GetDirectoryName(filename),
                    Path.GetFileNameWithoutExtension(filename) + ".twld");
                w.IsTModLoader = File.Exists(twldPath);

                w.Version = b.ReadUInt32();

                if (w.Version > 87)
                {
                    // Reset stream and load section header
                    b.BaseStream.Position = 0;
                    if (!LoadSectionHeader(b, out _, out sectionPointers, w))
                        return MakeCorruptHeader(filename, fi, "Invalid section header");

                    // Load header flags (title, dimensions, etc.)
                    LoadHeaderFlags(b, w, sectionPointers[0]);
                }
                else
                {
                    // Legacy format - read basic info
                    if (w.Version <= 38)
                    {
                        var pos = b.BaseStream.Position;
                        w.Title = b.ReadString();
                        b.BaseStream.Position = pos;
                    }
                }

                // Check for zero dimensions
                if (w.TilesWide <= 0 || w.TilesHigh <= 0)
                {
                    return MakeCorruptHeader(filename, fi, "Invalid dimensions (0x0)",
                        w.Title, w.Version, w.IsTModLoader);
                }

                // Quick footer validation: seek to footer position and validate
                string footerError = null;
                if (sectionPointers != null && w.Version > 87)
                {
                    try
                    {
                        // The last section pointer is the footer position
                        int footerPos = sectionPointers[^1];
                        if (footerPos > 0 && footerPos < b.BaseStream.Length)
                        {
                            b.BaseStream.Position = footerPos;

                            if (!b.ReadBoolean())
                                footerError = "Invalid footer flag";
                            else if (b.ReadString() != w.Title)
                                footerError = "Footer title mismatch";
                            else if (b.ReadInt32() != w.WorldId)
                                footerError = "Footer world ID mismatch";
                        }
                        else
                        {
                            footerError = "Footer position out of range";
                        }
                    }
                    catch
                    {
                        footerError = "Footer unreadable";
                    }
                }

                return new WorldHeaderInfo(
                    Title: w.Title ?? Path.GetFileNameWithoutExtension(filename),
                    Version: w.Version,
                    TilesWide: w.TilesWide,
                    TilesHigh: w.TilesHigh,
                    LastSave: fi.LastWriteTimeUtc,
                    FileSizeBytes: fi.Length,
                    FilePath: filename,
                    IsTModLoader: w.IsTModLoader,
                    IsFavorite: w.IsFavorite,
                    IsCrimson: w.IsCrimson,
                    GameMode: w.GameMode,
                    IsHardMode: w.HardMode,
                    Seed: w.Seed ?? "",
                    IsCorrupt: footerError != null,
                    CorruptReason: footerError);
            }
            catch (Exception ex)
            {
                return MakeCorruptHeader(filename, new FileInfo(filename), SanitizeMessage(ex));
            }
        }
    }

    private static WorldHeaderInfo MakeCorruptHeader(string filename, FileInfo fi, string reason,
        string title = null, uint version = 0, bool isTModLoader = false)
    {
        return new WorldHeaderInfo(
            Title: title ?? Path.GetFileNameWithoutExtension(filename),
            Version: version,
            TilesWide: 0,
            TilesHigh: 0,
            LastSave: fi.Exists ? fi.LastWriteTimeUtc : DateTime.MinValue,
            FileSizeBytes: fi.Exists ? fi.Length : 0,
            FilePath: filename,
            IsTModLoader: isTModLoader,
            IsFavorite: false,
            IsCrimson: false,
            GameMode: 0,
            IsHardMode: false,
            Seed: "",
            IsCorrupt: true,
            CorruptReason: reason);
    }

    /// <summary>
    /// Extracts a clean, displayable error message from an exception.
    /// Exception messages from binary parsing can contain garbled non-printable characters.
    /// </summary>
    private static string SanitizeMessage(Exception ex)
    {
        string typeName = ex.GetType().Name;

        // Use the exception type as a readable prefix
        string prefix = typeName switch
        {
            nameof(TEditFileFormatException) => "Invalid format",
            "EndOfStreamException" => "Unexpected end of file",
            "IOException" => "File read error",
            "OutOfMemoryException" => "File too large or corrupt",
            _ => "Read error"
        };

        // Only include the message if it contains printable characters
        string msg = ex.Message;
        if (string.IsNullOrEmpty(msg))
            return prefix;

        // Strip non-printable characters
        var clean = new System.Text.StringBuilder(msg.Length);
        foreach (char c in msg)
        {
            if (c >= ' ' && c < 127)
                clean.Append(c);
        }

        string cleaned = clean.ToString().Trim();
        if (cleaned.Length == 0 || cleaned.Length < msg.Length / 2)
            return prefix; // message was mostly garbled

        // Truncate long messages
        if (cleaned.Length > 120)
            cleaned = cleaned[..120] + "...";

        return $"{prefix}: {cleaned}";
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

                // Load .twld sidecar if this is a tModLoader world
                if (w.IsTModLoader && !headersOnly)
                {
                    progress?.Report(new ProgressChangedEventArgs(0, "Loading tModLoader data..."));
                    var twldData = TwldFile.Load(filename);
                    if (twldData != null)
                    {
                        w.TwldData = twldData;
                        TwldFile.ApplyToWorld(w, twldData);
                        TwldFile.ApplyModChestItems(w, twldData);
                    }
                }
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
        if (tile.Type >= TileFrameImportant.Length || !TileFrameImportant[tile.Type]) { return new Vector2Int32(x, y); }

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

    /// <summary>
    /// Determines tree type based on biome at position.
    /// Used for tree trunk textures (Tiles_5_{type}).
    /// </summary>
    /// <param name="x">X position in world</param>
    /// <param name="y">Y position in world</param>
    /// <returns>Tree type (-1 to 6) for use with GetTree()</returns>
    public int GetTreeTypeAtPosition(int x, int y)
    {
        // Bounds check
        if (x < 0 || x >= TilesWide || y < 0 || y >= TilesHigh)
            return -1;

        // Find biome tile by scanning downward (up to 100 tiles)
        for (int i = 0; i < 100; i++)
        {
            if (y + i >= TilesHigh) break;
            var tile = Tiles[x, y + i];
            if (tile == null || !tile.IsActive) continue;

            switch (tile.Type)
            {
                case 2:   return -1; // Normal grass (default tree)
                case 23:  return 0;  // Corruption grass
                case 60:  return (y + i > GroundLevel) ? 5 : 1; // Jungle grass (underground vs surface)
                case 70:  return 6;  // Mushroom grass
                case 109: return 2;  // Hallow grass
                case 147: return 3;  // Snow block
                case 199: return 4;  // Crimson grass
                default: continue; // Keep scanning if not a recognized biome tile
            }
        }
        return -1; // Default to normal tree
    }

    /// <summary>
    /// Determines tree style based on biome at position.
    /// Used for tree top/branch textures (Tree_Tops_X, Tree_Branches_X).
    /// </summary>
    /// <param name="x">X position in world</param>
    /// <param name="y">Y position in world</param>
    /// <returns>Tree style index (0-18) for use with GetTreeTops/GetTreeBranches</returns>
    public int GetTreeStyleAtPosition(int x, int y)
    {
        int treeType = GetTreeTypeAtPosition(x, y);

        // Map tree type to tree style
        return treeType switch
        {
            0 => 1,  // Corruption
            1 => BgJungle == 1 ? 11 : 2,  // Jungle (variant based on background)
            2 => 3,  // Hallow
            3 => GetSnowTreeStyle(x),  // Snow (complex variants)
            4 => 5,  // Crimson
            5 => 13, // Underground Jungle
            6 => 14, // Mushroom
            _ => GetNormalTreeStyle(x)  // Normal forest (varies by X position)
        };
    }

    /// <summary>
    /// Gets normal forest tree style based on X position and world tree style settings.
    /// </summary>
    private int GetNormalTreeStyle(int x)
    {
        int style = x <= TreeX0 ? TreeStyle0 :
                    x <= TreeX1 ? TreeStyle1 :
                    x <= TreeX2 ? TreeStyle2 : TreeStyle3;
        if (style == 0) return 0;
        return style == 5 ? 10 : 5 + style;
    }

    /// <summary>
    /// Gets snow tree style with complex variants based on X position and background setting.
    /// </summary>
    private int GetSnowTreeStyle(int x)
    {
        if (BgSnow == 0) return x % 10 == 0 ? 18 : 12;
        if (BgSnow is 2 or 3 or 32 or 4 or 42)
        {
            bool isLeft = x < TilesWide / 2;
            return BgSnow % 2 == 0 ? (isLeft ? 16 : 17) : (isLeft ? 17 : 16);
        }
        return 4;
    }

    /// <summary>
    /// Determines palm tree type based on sand biome at position.
    /// Used for palm tree textures (Tiles_323 and Tree_Tops_15).
    /// </summary>
    /// <param name="x">X position in world</param>
    /// <param name="y">Y position in world</param>
    /// <returns>Palm tree type (0-3) based on sand type below</returns>
    public int GetPalmTreeTypeAtPosition(int x, int y)
    {
        // Bounds check
        if (x < 0 || x >= TilesWide || y < 0 || y >= TilesHigh)
            return 0;

        // Find sand tile by scanning downward (up to 100 tiles)
        for (int i = 0; i < 100; i++)
        {
            if (y + i >= TilesHigh) break;
            var tile = Tiles[x, y + i];
            if (tile == null || !tile.IsActive) continue;

            switch (tile.Type)
            {
                case 53:  return 0; // Sand (normal palm)
                case 234: return 1; // Crimsand (crimson palm)
                case 116: return 2; // Pearlsand (hallowed palm)
                case 112: return 3; // Ebonsand (corruption palm)
                default: continue; // Keep scanning if not a recognized sand tile
            }
        }
        return 0; // Default to normal palm
    }
}
