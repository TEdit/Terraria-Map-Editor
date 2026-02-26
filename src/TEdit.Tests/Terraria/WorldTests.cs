using Xunit;
using System.IO;

namespace TEdit.Terraria.Tests;

public class WorldTests
{
    /// <summary>
    /// Checks if a .wld file is a valid binary world file (not a Git LFS pointer).
    /// LFS pointers are ~130 bytes of text starting with "version ".
    /// </summary>
    private static bool IsValidWorldFile(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists) return false;
        if (info.Length < 500) return false; // LFS pointers are ~130 bytes

        // Check first bytes for LFS pointer signature "version "
        using var fs = File.OpenRead(path);
        var buf = new byte[8];
        if (fs.Read(buf, 0, 8) < 8) return false;
        var header = System.Text.Encoding.ASCII.GetString(buf);
        return !header.StartsWith("version ");
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.wld")]
    public void LoadWorldV0_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\android.wld")]
    public void LoadWorldAndroid_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\console.wld")]
    public void LoadWorldConsole_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.2.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.0.6.wld")]
    [InlineData(".\\WorldFiles\\v1.0.6.1.wld")]
    [InlineData(".\\WorldFiles\\v1.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.0.1.wld")]
    public void LoadWorldV1_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.3.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.6.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.7.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.8.wld")]
    [InlineData(".\\WorldFiles\\v1.3.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.6.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.4.wld")]
    [InlineData(".\\WorldFiles\\v1.2.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.3.1.wld")]
    public void LoadWorldV2_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    public void LoadWorldV2_144x_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.5.0.wld")]
    [InlineData(".\\WorldFiles\\v1.4.5.5.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    [InlineData(".\\WorldFiles\\test-chest-145.wld")]
    public void LoadWorldV2_145x_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    public void ValidateConsoleWorld_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var status = World.ValidateWorldFile(fileName);
        Assert.True(status.IsValid, $"Validation failed: {status.Message}");
        Assert.True(status.IsConsole, "Expected console world");
    }

    [Theory]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    public void LoadAndValidateConsoleWorld_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        // Validate first (as WPF app does)
        var status = World.ValidateWorldFile(fileName);
        Assert.True(status.IsValid, $"Validation failed: {status.Message}");
        Assert.True(status.IsConsole);

        // Then load (as WPF app does after validation)
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
        Assert.True(w.IsConsole);
        Assert.True(w.TilesWide > 0, "World should have tiles");
        Assert.True(w.TilesHigh > 0, "World should have tiles");
    }

    [Theory]
    [InlineData(".\\WorldFiles\\console.wld")]
    [InlineData(".\\WorldFiles\\Challenge.wld")]
    [InlineData(".\\WorldFiles\\MAINWORLD.wld")]
    public void SaveWorld_Console_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);

        var saveTest = fileName + ".test";
        try
        {
            World.Save(w, saveTest, incrementRevision: false);
            var w2 = World.LoadWorld(saveTest);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.wld")]
    public void SaveWorldV0_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);

        var saveTest = fileName + ".test";
        try
        {
            World.Save(w, saveTest, versionOverride: -38, incrementRevision: false);
            var w2 = World.LoadWorld(saveTest);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.2.0.1.wld")]
    public void SaveWorldV1_Terraria1_2_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);

        var saveTest = fileName + ".test";
        try
        {
            World.Save(w, saveTest, incrementRevision: false);
            var w2 = World.LoadWorld(saveTest);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.1.1.wld")]
    public void SaveWorldV1_Terraria1_1_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);

        var saveTest = fileName + ".test";
        try
        {
            World.Save(w, saveTest, incrementRevision: false);
            var w2 = World.LoadWorld(saveTest);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.6.wld")]
    [InlineData(".\\WorldFiles\\v1.0.6.1.wld")]
    [InlineData(".\\WorldFiles\\v1.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.0.1.wld")]
    public void SaveWorldV1_Terraria1_0_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);

        var saveTest = fileName + ".test";
        try
        {
            World.Save(w, saveTest, incrementRevision: false);
            var w2 = World.LoadWorld(saveTest);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.5.wld")]
    public void SaveWorldV1_Terraria1_0_5_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);

        var saveTest = fileName + ".test";
        try
        {
            World.Save(w, saveTest, incrementRevision: false);
            var w2 = World.LoadWorld(saveTest);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.3.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.6.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.7.wld")]
    [InlineData(".\\WorldFiles\\v1.3.0.8.wld")]
    [InlineData(".\\WorldFiles\\v1.3.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.4.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.1.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.2.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.3.wld")]
    [InlineData(".\\WorldFiles\\v1.3.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.0.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.5.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.6.wld")]
    [InlineData(".\\WorldFiles\\v1.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.4.wld")]
    [InlineData(".\\WorldFiles\\v1.2.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.2.3.wld")]
    [InlineData(".\\WorldFiles\\v1.2.3.1.wld")]
    public void SaveWorldV2_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);

        var saveTest = fileName + ".test";
        try
        {
            World.Save(w, saveTest, incrementRevision: false);
            var w2 = World.LoadWorld(saveTest);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    public void SaveWorldV2_144x_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);

        var saveTest = fileName + ".test";
        try
        {
            World.Save(w, saveTest, incrementRevision: false);
            var w2 = World.LoadWorld(saveTest);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.5.0.wld")]
    [InlineData(".\\WorldFiles\\v1.4.5.5.wld")]
    public void SaveWorldV2_145x_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);

        var saveTest = fileName + ".test";
        try
        {
            World.Save(w, saveTest, incrementRevision: false);
            var w2 = World.LoadWorld(saveTest);
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.5.0.wld")]
    [InlineData(".\\WorldFiles\\console.wld")]
    public void SaveWorld_RLE_Compression_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        var fileInfo = new FileInfo(fileName);
        if (fileInfo.Length < 1_000_000)
        {
            // Skip files < 1MB (likely broken LFS links or stub files)
            return;
        }

        var originalSize = fileInfo.Length;

        var (world, errors) = World.LoadWorld(fileName);
        Assert.Null(errors);

        var saveTest = fileName + ".rle.test";
        try
        {
            World.Save(world, saveTest, incrementRevision: false);

            var savedSize = new FileInfo(saveTest).Length;
            double sizeRatio = (double)savedSize / originalSize;

            // Saved file should not be significantly larger (5% tolerance for metadata)
            Assert.True(sizeRatio <= 1.05,
                $"File grew from {originalSize:N0} to {savedSize:N0} bytes ({sizeRatio:P1}). " +
                $"RLE compression may not be working.");
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    /// <summary>
    /// Pads ShimmeredTownNPCs to the required size, matching the behavior of
    /// NpcListItem.IsShimmered setter which pads on demand.
    /// </summary>
    private static void EnsureShimmerCollectionSize(World world, int requiredSize)
    {
        while (world.ShimmeredTownNPCs.Count < requiredSize)
            world.ShimmeredTownNPCs.Add(0);
    }

    /// <summary>
    /// Verifies that NPC shimmer status survives a save/load round-trip.
    /// Regression test for https://github.com/TEdit/Terraria-Map-Editor/issues/2199
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    [InlineData(".\\WorldFiles\\v1.4.5.0.wld")]
    [InlineData(".\\WorldFiles\\v1.4.5.5.wld")]
    public void ShimmeredNPC_RoundTrip_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        var (world, errors) = World.LoadWorld(fileName);
        Assert.Null(errors);
        Assert.True(world.Version >= 268, "World version must support shimmer (>= 268)");

        // NPC sprite IDs to toggle (Merchant=17, Nurse=18, Guide=22)
        int[] testNpcIds = { 17, 18, 22 };
        int maxId = testNpcIds.Max();

        // Pad collection if needed (matches NpcListItem.IsShimmered setter behavior)
        EnsureShimmerCollectionSize(world, maxId + 1);

        // Record original shimmer state, then toggle each one
        var originalStates = new Dictionary<int, int>();
        foreach (var npcId in testNpcIds)
        {
            originalStates[npcId] = world.ShimmeredTownNPCs[npcId];
            world.ShimmeredTownNPCs[npcId] = world.ShimmeredTownNPCs[npcId] == 0 ? 1 : 0;
        }

        // Save and reload
        var saveTest = fileName + ".shimmer.test";
        try
        {
            World.Save(world, saveTest, incrementRevision: false);
            var (reloaded, reloadErrors) = World.LoadWorld(saveTest);
            Assert.Null(reloadErrors);

            // Verify each toggled NPC has the expected shimmer state
            foreach (var npcId in testNpcIds)
            {
                Assert.True(npcId < reloaded.ShimmeredTownNPCs.Count,
                    $"NPC ID {npcId} missing after reload (count={reloaded.ShimmeredTownNPCs.Count})");

                int expected = originalStates[npcId] == 0 ? 1 : 0;
                Assert.Equal(expected, reloaded.ShimmeredTownNPCs[npcId]);
            }
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    /// <summary>
    /// Verifies that shimmer state for all NPCs is preserved through save/load,
    /// not just the ones we explicitly toggle.
    /// </summary>
    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    public void ShimmeredNPC_AllEntries_Preserved_Test(string fileName)
    {
        if (!IsValidWorldFile(fileName)) return;

        var (world, errors) = World.LoadWorld(fileName);
        Assert.Null(errors);
        Assert.True(world.Version >= 268);

        // Pad to a known size and set a shimmer value
        EnsureShimmerCollectionSize(world, 100);
        world.ShimmeredTownNPCs[17] = 1; // Merchant shimmered
        world.ShimmeredTownNPCs[22] = 1; // Guide shimmered

        // Snapshot the entire shimmer collection
        var snapshot = world.ShimmeredTownNPCs.ToArray();

        var saveTest = fileName + ".shimmer-all.test";
        try
        {
            World.Save(world, saveTest, incrementRevision: false);
            var (reloaded, reloadErrors) = World.LoadWorld(saveTest);
            Assert.Null(reloadErrors);

            Assert.Equal(snapshot.Length, reloaded.ShimmeredTownNPCs.Count);

            for (int i = 0; i < snapshot.Length; i++)
            {
                Assert.Equal(snapshot[i], reloaded.ShimmeredTownNPCs[i]);
            }
        }
        finally
        {
            if (File.Exists(saveTest)) File.Delete(saveTest);
        }
    }

    [Fact]
    public void Tile_Equals_Test()
    {
        var tile1 = new Tile { IsActive = true, Type = 1, Wall = 5 };
        var tile2 = (Tile)tile1.Clone();

        // Cloned tiles should be equal
        Assert.True(tile1.Equals(tile2));
        Assert.True(tile1 == tile2);
        Assert.Equal(tile1.GetHashCode(), tile2.GetHashCode());

        // Different cache values should not affect comparison
        tile2.uvTileCache = 999;
        Assert.True(tile1.Equals(tile2));

        // Different serialized property should not be equal
        tile2.Type = 2;
        Assert.False(tile1.Equals(tile2));
        Assert.False(tile1 == tile2);
    }
}
