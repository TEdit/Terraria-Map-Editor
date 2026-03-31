using System;
using System.IO;
using TEdit.Terraria;
using TEdit.Terraria.IO;
using Xunit;
using Xunit.Abstractions;

namespace TEdit.Terraria.Tests;

/// <summary>
/// Round-trip tests for console world files, focusing on ConsoleCompressor
/// and binary-level fidelity. TEdit's loader can load broken/invalid worlds
/// (feature for recovery), but the game cannot — so we verify binary output
/// matches the original, not just that TEdit can re-load it.
/// </summary>
public class ConsoleRoundTripTests
{
    private readonly ITestOutputHelper _output;

    public ConsoleRoundTripTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static bool IsValidWorldFile(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists) return false;
        if (info.Length < 500) return false;
        using var fs = File.OpenRead(path);
        var buf = new byte[8];
        if (fs.Read(buf, 0, 8) < 8) return false;
        var header = System.Text.Encoding.ASCII.GetString(buf);
        return !header.StartsWith("version ");
    }

    private static bool IsCompressedWorld(string path)
    {
        using var fs = File.OpenRead(path);
        return ConsoleCompressor.IsCompressed(fs);
    }

    /// <summary>
    /// Gets the raw (uncompressed) world bytes from a file, handling both
    /// compressed console worlds and uncompressed worlds.
    /// </summary>
    private static byte[] GetRawWorldBytes(string path)
    {
        using var fs = File.OpenRead(path);
        if (ConsoleCompressor.IsCompressed(fs))
        {
            using var ms = ConsoleCompressor.DecompressStream(fs);
            return ms.ToArray();
        }
        else
        {
            fs.Position = 0;
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            return ms.ToArray();
        }
    }

    /// <summary>
    /// Gets the raw (uncompressed) world bytes from a saved file.
    /// For compressed files, decompresses first. For uncompressed, reads directly.
    /// </summary>
    private static byte[] GetRawWorldBytesFromSaved(string path)
    {
        return GetRawWorldBytes(path);
    }

    // ─── ConsoleCompressor isolation tests ──────────────────────────

    /// <summary>
    /// Tests ConsoleCompressor in isolation: decompress → compress → decompress
    /// and verify the decompressed bytes are identical.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void ConsoleCompressor_RoundTrip_ByteIdentical(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        using var fs = File.OpenRead(fileName);
        if (!ConsoleCompressor.IsCompressed(fs))
        {
            _output.WriteLine("Skipped: not a compressed console world");
            return;
        }

        // Step 1: Decompress original
        byte[] originalDecompressed;
        using (var decompressed = ConsoleCompressor.DecompressStream(fs))
        {
            originalDecompressed = decompressed.ToArray();
        }
        _output.WriteLine($"Original decompressed size: {originalDecompressed.Length:N0} bytes");

        // Step 2: Write raw bytes to temp, compress, then decompress again
        var tempFile = fileName + ".compressor-test";
        try
        {
            File.WriteAllBytes(tempFile, originalDecompressed);

            using (var tempFs = new FileStream(tempFile, FileMode.Open, FileAccess.ReadWrite))
            {
                ConsoleCompressor.CompressStream(tempFs);
            }

            byte[] reDecompressed;
            using (var tempFs = File.OpenRead(tempFile))
            {
                Assert.True(ConsoleCompressor.IsCompressed(tempFs), "Re-compressed file should have signature");
                using var decompressed = ConsoleCompressor.DecompressStream(tempFs);
                reDecompressed = decompressed.ToArray();
            }

            _output.WriteLine($"Re-decompressed size: {reDecompressed.Length:N0} bytes");

            Assert.Equal(originalDecompressed.Length, reDecompressed.Length);
            int firstDiff = FindFirstDifference(originalDecompressed, reDecompressed);
            if (firstDiff >= 0)
            {
                DumpDifference("Compressor round-trip", originalDecompressed, reDecompressed, firstDiff);
                Assert.Fail($"Decompressed bytes differ at offset 0x{firstDiff:X8}");
            }
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Verifies that the outputSize header written by CompressStream matches
    /// the actual decompressed data size.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void ConsoleCompressor_OutputSize_Header_Matches(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        using var fs = File.OpenRead(fileName);
        if (!ConsoleCompressor.IsCompressed(fs)) return;

        using var br = new BinaryReader(fs, System.Text.Encoding.UTF8, leaveOpen: true);
        int signature = br.ReadInt32();
        int declaredSize = br.ReadInt32();

        fs.Position = 0;
        using var decompressed = ConsoleCompressor.DecompressStream(fs);
        int actualSize = (int)decompressed.Length;

        _output.WriteLine($"Declared size in header: {declaredSize:N0}");
        _output.WriteLine($"Actual decompressed size: {actualSize:N0}");

        Assert.Equal(declaredSize, actualSize);
    }

    // ─── Full world round-trip with binary comparison ───────────────

    /// <summary>
    /// Load → Save → binary compare the raw (pre-compression) world bytes.
    /// This is the critical test: if the serialized bytes differ, the game
    /// will reject the world even though TEdit's lenient loader accepts it.
    /// Works for both compressed and uncompressed console worlds.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Console_FullRoundTrip_RawBytesMatch(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        bool isCompressed = IsCompressedWorld(fileName);
        byte[] originalRaw = GetRawWorldBytes(fileName);

        _output.WriteLine($"Source: {fileName}");
        _output.WriteLine($"Compressed: {isCompressed}");
        _output.WriteLine($"Raw world size: {originalRaw.Length:N0} bytes");

        // Load world
        var (world, error) = World.LoadWorld(fileName);
        Assert.Null(error);

        _output.WriteLine($"World: {world.Title}, Version: {world.Version}, " +
                          $"Size: {world.TilesWide}x{world.TilesHigh}, IsConsole: {world.IsConsole}");

        // Save world
        var saveTest = fileName + ".roundtrip.test";
        try
        {
            World.Save(world, saveTest, incrementRevision: false);
            Assert.True(File.Exists(saveTest), "Saved file should exist");

            // Get raw bytes from saved file (decompress if needed)
            byte[] savedRaw = GetRawWorldBytesFromSaved(saveTest);

            _output.WriteLine($"Original raw: {originalRaw.Length:N0} bytes");
            _output.WriteLine($"Saved raw:    {savedRaw.Length:N0} bytes");

            if (originalRaw.Length != savedRaw.Length)
            {
                _output.WriteLine($"SIZE MISMATCH: delta={savedRaw.Length - originalRaw.Length} bytes");
            }

            int firstDiff = FindFirstDifference(originalRaw, savedRaw);
            if (firstDiff >= 0)
            {
                DumpDifference("Full round-trip", originalRaw, savedRaw, firstDiff);
                DumpDiffSummary(originalRaw, savedRaw);
                Assert.Fail($"Raw world bytes differ at offset 0x{firstDiff:X8}. See test output.");
            }

            Assert.Equal(originalRaw.Length, savedRaw.Length);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    /// <summary>
    /// Load → Save → Reload → compare key world properties.
    /// This is a weaker (semantic) check — useful for understanding what
    /// survives even if binary comparison fails.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Console_RoundTrip_WorldProperties_Match(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        var (w1, err1) = World.LoadWorld(fileName);
        Assert.Null(err1);

        var saveTest = fileName + ".props.test";
        try
        {
            World.Save(w1, saveTest, incrementRevision: false);

            var (w2, err2) = World.LoadWorld(saveTest);
            Assert.Null(err2);

            _output.WriteLine($"Original: {w1.Title} v{w1.Version} {w1.TilesWide}x{w1.TilesHigh}");
            _output.WriteLine($"Reloaded: {w2.Title} v{w2.Version} {w2.TilesWide}x{w2.TilesHigh}");

            Assert.Equal(w1.Title, w2.Title);
            Assert.Equal(w1.Version, w2.Version);
            Assert.Equal(w1.TilesWide, w2.TilesWide);
            Assert.Equal(w1.TilesHigh, w2.TilesHigh);
            Assert.Equal(w1.WorldId, w2.WorldId);
            Assert.Equal(w1.IsConsole, w2.IsConsole);
            Assert.Equal(w1.SpawnX, w2.SpawnX);
            Assert.Equal(w1.SpawnY, w2.SpawnY);

            // Tile data spot checks
            Assert.Equal(w1.Tiles[0, 0], w2.Tiles[0, 0]);
            Assert.Equal(w1.Tiles[w1.TilesWide / 2, w1.TilesHigh / 2],
                         w2.Tiles[w2.TilesWide / 2, w2.TilesHigh / 2]);
            Assert.Equal(w1.Tiles[w1.TilesWide - 1, w1.TilesHigh - 1],
                         w2.Tiles[w2.TilesWide - 1, w2.TilesHigh - 1]);

            Assert.Equal(w1.Chests.Count, w2.Chests.Count);
            Assert.Equal(w1.Signs.Count, w2.Signs.Count);
            Assert.Equal(w1.NPCs.Count, w2.NPCs.Count);

            _output.WriteLine($"Chests: {w1.Chests.Count}, Signs: {w1.Signs.Count}, NPCs: {w1.NPCs.Count}");
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    /// <summary>
    /// Double round-trip: Load → Save → Load → Save → binary compare raw bytes
    /// of the two saved files. Even if the first save drifts from the original,
    /// the second save must be identical to the first (idempotent serialization).
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Console_DoubleRoundTrip_Stable(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        var (w1, err1) = World.LoadWorld(fileName);
        Assert.Null(err1);

        var save1 = fileName + ".rt1.test";
        var save2 = fileName + ".rt2.test";
        try
        {
            World.Save(w1, save1, incrementRevision: false);

            var (w2, err2) = World.LoadWorld(save1);
            Assert.Null(err2);
            World.Save(w2, save2, incrementRevision: false);

            byte[] bytes1 = GetRawWorldBytes(save1);
            byte[] bytes2 = GetRawWorldBytes(save2);

            _output.WriteLine($"Save 1 raw: {bytes1.Length:N0} bytes");
            _output.WriteLine($"Save 2 raw: {bytes2.Length:N0} bytes");

            Assert.Equal(bytes1.Length, bytes2.Length);

            int firstDiff = FindFirstDifference(bytes1, bytes2);
            if (firstDiff >= 0)
            {
                DumpDifference("Double round-trip", bytes1, bytes2, firstDiff);
                Assert.Fail($"Double round-trip not stable: saves differ at offset 0x{firstDiff:X8}");
            }
        }
        finally
        {
            if (File.Exists(save1)) File.Delete(save1);
            if (File.Exists(save2)) File.Delete(save2);
        }
    }

    /// <summary>
    /// Verifies that an uncompressed console world (like ConsoleCookie.wld)
    /// is NOT wrapped in console compression after load/save when IsConsole is false.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    public void UncompressedConsoleWorld_StaysUncompressed(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        if (IsCompressedWorld(fileName))
        {
            _output.WriteLine("Skipped: file is compressed");
            return;
        }

        var (world, error) = World.LoadWorld(fileName);
        Assert.Null(error);

        _output.WriteLine($"IsConsole: {world.IsConsole}, Version: {world.Version}");

        var saveTest = fileName + ".uncomp.test";
        try
        {
            World.Save(world, saveTest, incrementRevision: false);

            // Verify the saved file is NOT compressed
            using var fs = File.OpenRead(saveTest);
            bool savedIsCompressed = ConsoleCompressor.IsCompressed(fs);
            _output.WriteLine($"Saved file compressed: {savedIsCompressed}");

            Assert.Equal(world.IsConsole, savedIsCompressed);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    // ─── Section pointer diagnostics ────────────────────────────────

    /// <summary>
    /// Parses section pointers from original and saved files, then compares
    /// section sizes to identify exactly which section is losing data.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    [InlineData(".\\WorldFiles\\ConsoleWorld6.wld")]
    public void Console_SectionPointer_Diagnostic(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        string[] sectionNames =
        [
            "Header Flags",   // 0
            "Tiles",          // 1
            "Chests",         // 2
            "Signs",          // 3
            "NPCs",           // 4
            "TileEntities",   // 5
            "PressurePlates", // 6
            "TownManager",    // 7
            "Bestiary",       // 8
            "CreativePowers", // 9
            "Footer"          // 10
        ];

        byte[] originalRaw = GetRawWorldBytes(fileName);
        var origPtrs = ParseSectionPointers(originalRaw);

        _output.WriteLine($"=== {Path.GetFileName(fileName)} ===");
        _output.WriteLine($"Original file raw size: {originalRaw.Length:N0} bytes");
        _output.WriteLine($"Original section count: {origPtrs.Length}");

        // Load and save
        var (world, error) = World.LoadWorld(fileName);
        Assert.Null(error);

        var saveTest = fileName + ".diag.test";
        try
        {
            World.Save(world, saveTest, incrementRevision: false);
            byte[] savedRaw = GetRawWorldBytes(saveTest);
            var savedPtrs = ParseSectionPointers(savedRaw);

            _output.WriteLine($"Saved file raw size:   {savedRaw.Length:N0} bytes");
            _output.WriteLine($"Saved section count:   {savedPtrs.Length}");
            _output.WriteLine($"Total delta:           {savedRaw.Length - originalRaw.Length:+#;-#;0} bytes");
            _output.WriteLine("");
            _output.WriteLine($"{"Section",-20} {"Orig Start",12} {"Orig Size",12} {"Save Start",12} {"Save Size",12} {"Delta",12}");
            _output.WriteLine(new string('-', 82));

            int maxSections = Math.Max(origPtrs.Length, savedPtrs.Length);
            for (int i = 0; i < maxSections; i++)
            {
                string name = i < sectionNames.Length ? sectionNames[i] : $"Section[{i}]";

                int origStart = i < origPtrs.Length ? origPtrs[i] : -1;
                int origEnd = (i + 1 < origPtrs.Length) ? origPtrs[i + 1] :
                              (i < origPtrs.Length) ? originalRaw.Length : -1;
                int origSize = (origStart >= 0 && origEnd >= 0) ? origEnd - origStart : -1;

                int saveStart = i < savedPtrs.Length ? savedPtrs[i] : -1;
                int saveEnd = (i + 1 < savedPtrs.Length) ? savedPtrs[i + 1] :
                              (i < savedPtrs.Length) ? savedRaw.Length : -1;
                int saveSize = (saveStart >= 0 && saveEnd >= 0) ? saveEnd - saveStart : -1;

                int delta = (origSize >= 0 && saveSize >= 0) ? saveSize - origSize : 0;
                string deltaStr = delta == 0 ? "OK" : $"{delta:+#;-#;0}";

                _output.WriteLine($"{name,-20} {origStart,12:N0} {origSize,12:N0} {saveStart,12:N0} {saveSize,12:N0} {deltaStr,12}");
            }

            // Also dump the header section (before first section pointer)
            if (origPtrs.Length > 0 && savedPtrs.Length > 0)
            {
                int origHeaderSize = origPtrs[0];
                int savedHeaderSize = savedPtrs[0];
                int headerDelta = savedHeaderSize - origHeaderSize;
                _output.WriteLine("");
                _output.WriteLine($"File header size: original={origHeaderSize:N0}, saved={savedHeaderSize:N0}, delta={headerDelta:+#;-#;0}");
            }
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    /// <summary>
    /// Parse section pointers from raw V2 world bytes.
    /// V2 header: version(4) + [if >= 140: header(7) + type(1) + revision(4) + flags(8)] + sectionCount(2) + pointers(4 each)
    /// </summary>
    private static int[] ParseSectionPointers(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);

        uint version = br.ReadUInt32();

        if (version >= 140)
        {
            br.ReadBytes(7); // header string
            br.ReadByte();   // file type
            br.ReadUInt32(); // revision
            br.ReadUInt64(); // flags
        }

        short sectionCount = br.ReadInt16();
        int[] pointers = new int[sectionCount];
        for (int i = 0; i < sectionCount; i++)
        {
            pointers[i] = br.ReadInt32();
        }

        return pointers;
    }

    /// <summary>
    /// Compare tile data between original load and round-tripped load.
    /// This catches silent data loss that section size changes might indicate.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Console_TileByTile_Comparison(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        var (w1, err1) = World.LoadWorld(fileName);
        Assert.Null(err1);

        var saveTest = fileName + ".tilediff.test";
        try
        {
            World.Save(w1, saveTest, incrementRevision: false);
            var (w2, err2) = World.LoadWorld(saveTest);
            Assert.Null(err2);

            Assert.Equal(w1.TilesWide, w2.TilesWide);
            Assert.Equal(w1.TilesHigh, w2.TilesHigh);

            int diffCount = 0;
            int firstDiffX = -1, firstDiffY = -1;
            Tile firstOrigTile = default, firstSavedTile = default;

            for (int x = 0; x < w1.TilesWide && diffCount < 100; x++)
            {
                for (int y = 0; y < w1.TilesHigh && diffCount < 100; y++)
                {
                    var t1 = w1.Tiles[x, y];
                    var t2 = w2.Tiles[x, y];
                    if (!t1.Equals(t2))
                    {
                        if (diffCount == 0)
                        {
                            firstDiffX = x;
                            firstDiffY = y;
                            firstOrigTile = t1;
                            firstSavedTile = t2;
                        }
                        diffCount++;
                        if (diffCount <= 10)
                        {
                            _output.WriteLine($"  Tile diff at [{x},{y}]:");
                            _output.WriteLine($"    Orig:  Active={t1.IsActive} Type={t1.Type} Wall={t1.Wall} U={t1.U} V={t1.V} Liquid={t1.LiquidAmount}/{t1.LiquidType} Color={t1.TileColor} WallColor={t1.WallColor} Brick={t1.BrickStyle}");
                            _output.WriteLine($"    Saved: Active={t2.IsActive} Type={t2.Type} Wall={t2.Wall} U={t2.U} V={t2.V} Liquid={t2.LiquidAmount}/{t2.LiquidType} Color={t2.TileColor} WallColor={t2.WallColor} Brick={t2.BrickStyle}");
                            _output.WriteLine($"    Flags: InActive={t1.InActive}→{t2.InActive} WireR={t1.WireRed}→{t2.WireRed} WireB={t1.WireBlue}→{t2.WireBlue} WireG={t1.WireGreen}→{t2.WireGreen} WireY={t1.WireYellow}→{t2.WireYellow} Actuator={t1.Actuator}→{t2.Actuator}");
                            _output.WriteLine($"    V144:  InvisBlock={t1.InvisibleBlock}→{t2.InvisibleBlock} InvisWall={t1.InvisibleWall}→{t2.InvisibleWall} BrightBlock={t1.FullBrightBlock}→{t2.FullBrightBlock} BrightWall={t1.FullBrightWall}→{t2.FullBrightWall}");
                        }
                    }
                }
            }

            _output.WriteLine($"Total tile differences: {diffCount}");
            Assert.True(diffCount == 0,
                $"Found {diffCount} tile(s) that differ after round-trip. First at [{firstDiffX},{firstDiffY}]");
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    /// <summary>
    /// Diagnostic: count active tiles, walls, liquids, and wires after loading
    /// to verify the loader actually populated tile data.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\ConsoleCookie.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Console_TileData_Diagnostic(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        var (world, error) = World.LoadWorld(fileName);
        Assert.Null(error);

        int totalTiles = world.TilesWide * world.TilesHigh;
        int activeTiles = 0;
        int walls = 0;
        int liquids = 0;
        int wires = 0;
        int emptyTiles = 0;
        var tileTypeCounts = new Dictionary<int, int>();

        for (int x = 0; x < world.TilesWide; x++)
        {
            for (int y = 0; y < world.TilesHigh; y++)
            {
                var tile = world.Tiles[x, y];
                bool isEmpty = !tile.IsActive && tile.Wall == 0 && tile.LiquidAmount == 0;

                if (tile.IsActive)
                {
                    activeTiles++;
                    tileTypeCounts.TryGetValue(tile.Type, out int count);
                    tileTypeCounts[tile.Type] = count + 1;
                }
                if (tile.Wall != 0) walls++;
                if (tile.LiquidAmount != 0) liquids++;
                if (tile.WireRed || tile.WireBlue || tile.WireGreen || tile.WireYellow) wires++;
                if (isEmpty) emptyTiles++;
            }
        }

        _output.WriteLine($"=== {Path.GetFileName(fileName)} ===");
        _output.WriteLine($"World: {world.Title} v{world.Version} {world.TilesWide}x{world.TilesHigh}");
        _output.WriteLine($"Total tile slots: {totalTiles:N0}");
        _output.WriteLine($"Active tiles:     {activeTiles:N0} ({100.0 * activeTiles / totalTiles:F1}%)");
        _output.WriteLine($"Walls:            {walls:N0} ({100.0 * walls / totalTiles:F1}%)");
        _output.WriteLine($"Liquids:          {liquids:N0} ({100.0 * liquids / totalTiles:F1}%)");
        _output.WriteLine($"Wires:            {wires:N0}");
        _output.WriteLine($"Completely empty: {emptyTiles:N0} ({100.0 * emptyTiles / totalTiles:F1}%)");
        _output.WriteLine($"Unique tile types: {tileTypeCounts.Count}");

        // Top 10 tile types by count
        var topTypes = tileTypeCounts.OrderByDescending(kv => kv.Value).Take(10);
        _output.WriteLine("Top tile types:");
        foreach (var kv in topTypes)
        {
            _output.WriteLine($"  Type {kv.Key}: {kv.Value:N0}");
        }

        // Fail if the world is suspiciously empty
        Assert.True(activeTiles > 0 || walls > 0,
            "World has no active tiles or walls — loader may have failed to read tile data");
    }

    /// <summary>
    /// THE KEY TEST: Load a console world, save unedited (control), then
    /// load again, make a minimal edit, and save. Compare both outputs.
    /// This reproduces the reported failure: edited console saves fail on PlayStation.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void Console_EditedVsUnedited_Save_Comparison(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        if (!IsCompressedWorld(fileName)) return;

        var uneditedSave = fileName + ".unedited.test";
        var editedSave = fileName + ".edited.test";

        try
        {
            // === CONTROL: load and save without editing ===
            var (w1, err1) = World.LoadWorld(fileName);
            Assert.Null(err1);
            Assert.True(w1.IsConsole);
            World.Save(w1, uneditedSave, incrementRevision: false);

            // === TEST: load, edit one tile, save ===
            var (w2, err2) = World.LoadWorld(fileName);
            Assert.Null(err2);
            Assert.True(w2.IsConsole);

            // Find a tile to edit (pick center of map)
            int editX = w2.TilesWide / 2;
            int editY = w2.TilesHigh / 2;
            var origTile = w2.Tiles[editX, editY];
            _output.WriteLine($"Editing tile [{editX},{editY}]: IsActive={origTile.IsActive} Type={origTile.Type} Wall={origTile.Wall}");

            // Make a minimal edit: toggle a dirt block
            w2.Tiles[editX, editY].IsActive = true;
            w2.Tiles[editX, editY].Type = 0; // dirt
            World.Save(w2, editedSave, incrementRevision: false);

            // === ANALYSIS: compare compressed file structure ===
            var uneditedInfo = new FileInfo(uneditedSave);
            var editedInfo = new FileInfo(editedSave);
            _output.WriteLine($"Unedited save size: {uneditedInfo.Length:N0} bytes");
            _output.WriteLine($"Edited save size:   {editedInfo.Length:N0} bytes");

            // Both should be console-compressed
            Assert.True(IsCompressedWorld(uneditedSave), "Unedited save should be compressed");
            Assert.True(IsCompressedWorld(editedSave), "Edited save should be compressed");

            // Check headers
            DumpCompressedHeader("Unedited", uneditedSave);
            DumpCompressedHeader("Edited", editedSave);

            // Decompress both and compare section pointers
            byte[] uneditedRaw = GetRawWorldBytes(uneditedSave);
            byte[] editedRaw = GetRawWorldBytes(editedSave);

            _output.WriteLine($"Unedited decompressed: {uneditedRaw.Length:N0} bytes");
            _output.WriteLine($"Edited decompressed:   {editedRaw.Length:N0} bytes");

            var uneditedPtrs = ParseSectionPointers(uneditedRaw);
            var editedPtrs = ParseSectionPointers(editedRaw);

            string[] sectionNames = ["Header Flags", "Tiles", "Chests", "Signs", "NPCs",
                "TileEntities", "PressurePlates", "TownManager", "Bestiary", "CreativePowers", "Footer"];

            _output.WriteLine($"\n{"Section",-20} {"Unedited Start",14} {"Unedited Size",14} {"Edited Start",14} {"Edited Size",14} {"Delta",10}");
            _output.WriteLine(new string('-', 86));

            for (int i = 0; i < Math.Max(uneditedPtrs.Length, editedPtrs.Length); i++)
            {
                string name = i < sectionNames.Length ? sectionNames[i] : $"Section[{i}]";
                int us = i < uneditedPtrs.Length ? uneditedPtrs[i] : -1;
                int ue = (i + 1 < uneditedPtrs.Length) ? uneditedPtrs[i + 1] : (i < uneditedPtrs.Length ? uneditedRaw.Length : -1);
                int uSize = (us >= 0 && ue >= 0) ? ue - us : -1;

                int es = i < editedPtrs.Length ? editedPtrs[i] : -1;
                int ee = (i + 1 < editedPtrs.Length) ? editedPtrs[i + 1] : (i < editedPtrs.Length ? editedRaw.Length : -1);
                int eSize = (es >= 0 && ee >= 0) ? ee - es : -1;

                int delta = (uSize >= 0 && eSize >= 0) ? eSize - uSize : 0;
                string deltaStr = delta == 0 ? "OK" : $"{delta:+#;-#;0}";
                _output.WriteLine($"{name,-20} {us,14:N0} {uSize,14:N0} {es,14:N0} {eSize,14:N0} {deltaStr,10}");
            }

            // Verify both can be re-loaded
            var (w3, err3) = World.LoadWorld(uneditedSave);
            Assert.Null(err3);
            _output.WriteLine($"\nUnedited reload: OK (v{w3.Version} {w3.TilesWide}x{w3.TilesHigh})");

            var (w4, err4) = World.LoadWorld(editedSave);
            Assert.Null(err4);
            _output.WriteLine($"Edited reload:   OK (v{w4.Version} {w4.TilesWide}x{w4.TilesHigh})");

            // Verify the edit actually persisted
            Assert.True(w4.Tiles[editX, editY].IsActive);
            Assert.Equal((ushort)0, w4.Tiles[editX, editY].Type);
            _output.WriteLine($"Edit persisted at [{editX},{editY}]: IsActive={w4.Tiles[editX, editY].IsActive} Type={w4.Tiles[editX, editY].Type}");

            // Check V2 footer validation on both
            ValidateV2Footer(uneditedRaw, "Unedited");
            ValidateV2Footer(editedRaw, "Edited");
        }
        finally
        {
            if (File.Exists(uneditedSave)) File.Delete(uneditedSave);
            if (File.Exists(editedSave)) File.Delete(editedSave);
        }
    }

    private void DumpCompressedHeader(string label, string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        int sig = br.ReadInt32();
        int declaredSize = br.ReadInt32();
        long compressedSize = fs.Length - 8; // total file minus header
        double ratio = 100.0 * compressedSize / declaredSize;
        _output.WriteLine($"  {label}: sig=0x{sig:X8} declared={declaredSize:N0} compressed={compressedSize:N0} ratio={ratio:F1}%");
    }

    private void ValidateV2Footer(byte[] raw, string label)
    {
        // V2 footer: bool(true) + title + worldId
        // Last few bytes should end with the world title and ID
        int len = raw.Length;
        if (len < 10)
        {
            _output.WriteLine($"  {label} footer: TOO SHORT");
            return;
        }

        // Read from the end to find the footer
        // The footer structure in V2 is at the last section pointer position
        var ptrs = ParseSectionPointers(raw);
        if (ptrs.Length == 0)
        {
            _output.WriteLine($"  {label} footer: NO SECTION POINTERS");
            return;
        }

        int footerStart = ptrs[^1];
        using var ms = new MemoryStream(raw);
        ms.Position = footerStart;
        using var br = new BinaryReader(ms);

        try
        {
            bool valid = br.ReadBoolean();
            string title = br.ReadString();
            int worldId = br.ReadInt32();
            long bytesRemaining = ms.Length - ms.Position;
            _output.WriteLine($"  {label} footer: valid={valid} title=\"{title}\" worldId={worldId} bytesAfter={bytesRemaining}");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"  {label} footer: PARSE ERROR - {ex.Message}");
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────

    private static int FindFirstDifference(byte[] a, byte[] b)
    {
        int minLen = Math.Min(a.Length, b.Length);
        for (int i = 0; i < minLen; i++)
        {
            if (a[i] != b[i]) return i;
        }
        return a.Length != b.Length ? minLen : -1;
    }

    private void DumpDifference(string label, byte[] original, byte[] saved, int firstDiff)
    {
        int minLen = Math.Min(original.Length, saved.Length);
        int start = Math.Max(0, firstDiff - 16);
        int end = Math.Min(minLen, firstDiff + 48);

        _output.WriteLine($"[{label}] First difference at offset 0x{firstDiff:X8} ({firstDiff}):");
        _output.WriteLine($"  Original [{start:X8}..{end:X8}]: {FormatHex(original, start, end)}");
        _output.WriteLine($"  Saved    [{start:X8}..{end:X8}]: {FormatHex(saved, start, end)}");
    }

    private void DumpDiffSummary(byte[] original, byte[] saved)
    {
        int minLen = Math.Min(original.Length, saved.Length);
        int diffCount = 0;
        int firstDiff = -1;
        int lastDiff = -1;
        for (int i = 0; i < minLen; i++)
        {
            if (original[i] != saved[i])
            {
                diffCount++;
                if (firstDiff < 0) firstDiff = i;
                lastDiff = i;
            }
        }

        if (original.Length != saved.Length)
            diffCount += Math.Abs(original.Length - saved.Length);

        _output.WriteLine($"  Total differing bytes: {diffCount:N0}");
        _output.WriteLine($"  Diff range: 0x{firstDiff:X8} - 0x{lastDiff:X8}");
        _output.WriteLine($"  Original size: {original.Length:N0}, Saved size: {saved.Length:N0}");
    }

    private static string FormatHex(byte[] data, int start, int end)
    {
        end = Math.Min(end, data.Length);
        var sb = new System.Text.StringBuilder();
        for (int i = start; i < end; i++)
        {
            if (i > start) sb.Append(' ');
            sb.Append(data[i].ToString("X2"));
        }
        return sb.ToString();
    }
}
