using Xunit;
using TEdit.Terraria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;

namespace TEdit.Terraria.Tests;

public class WorldTests
{
    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.wld")]
    public void LoadWorldV0_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\xbox-legacy-world.wld")]
    public void LoadWorldXboxLegacy_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
        Assert.NotNull(w);

        // Debug: Check what was loaded
        Console.WriteLine($"Chests loaded: {w.Chests.Count}");
        Console.WriteLine($"Signs loaded: {w.Signs.Count}");
        Console.WriteLine($"NPCs loaded: {w.NPCs.Count}");

        // Verify we have some chests
        Assert.True(w.Chests.Count > 0, "Expected to load at least one chest");

        // Verify chests have items
        var chestsWithItems = w.Chests.Count(c => c.Items.Any(i => i.StackSize > 0));
        Console.WriteLine($"Chests with items: {chestsWithItems}");
    }

    [Theory]
    [InlineData(".\\WorldFiles\\xbox-legacy-world.wld")]
    public void LoadSaveWorldXboxLegacy_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
        Assert.Null(er);
        Assert.NotNull(w);

        Console.WriteLine($"Original world IsXbox: {w.IsXbox}");
        Console.WriteLine($"Original world Version: {w.Version}");
        Console.WriteLine($"Original signs count: {w.Signs.Count}");

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // Load the saved file
        var (w2, er2) = World.LoadWorld(saveTest);
        Assert.Null(er2);
        Assert.NotNull(w2);

        Console.WriteLine($"Reloaded world IsXbox: {w2.IsXbox}");
        Console.WriteLine($"Reloaded world Version: {w2.Version}");
        Console.WriteLine($"Reloaded signs count: {w2.Signs.Count}");

        // Verify header properties match
        Assert.Equal(w.Title, w2.Title);
        Assert.Equal(w.WorldId, w2.WorldId);
        Assert.Equal(w.XboxWorldTimestamp, w2.XboxWorldTimestamp);
        Assert.Equal(w.RightWorld, w2.RightWorld);
        Assert.Equal(w.BottomWorld, w2.BottomWorld);
        Assert.Equal(w.TilesHigh, w2.TilesHigh);
        Assert.Equal(w.TilesWide, w2.TilesWide);
        Assert.Equal(w.SpawnX, w2.SpawnX);
        Assert.Equal(w.SpawnY, w2.SpawnY);
        Assert.Equal(w.GroundLevel, w2.GroundLevel);
        Assert.Equal(w.RockLevel, w2.RockLevel);
        Assert.Equal(w.Time, w2.Time);
        Assert.Equal(w.DayTime, w2.DayTime);
        Assert.Equal(w.MoonPhase, w2.MoonPhase);
        Assert.Equal(w.BloodMoon, w2.BloodMoon);
        Assert.Equal(w.DungeonX, w2.DungeonX);
        Assert.Equal(w.DungeonY, w2.DungeonY);
        Assert.Equal(w.DownedBoss1EyeofCthulhu, w2.DownedBoss1EyeofCthulhu);
        Assert.Equal(w.DownedBoss2EaterofWorlds, w2.DownedBoss2EaterofWorlds);
        Assert.Equal(w.DownedBoss3Skeletron, w2.DownedBoss3Skeletron);
        Assert.Equal(w.SavedGoblin, w2.SavedGoblin);
        Assert.Equal(w.SavedWizard, w2.SavedWizard);
        Assert.Equal(w.SavedMech, w2.SavedMech);
        Assert.Equal(w.DownedGoblins, w2.DownedGoblins);
        Assert.Equal(w.DownedClown, w2.DownedClown);
        Assert.Equal(w.DownedFrost, w2.DownedFrost);
        Assert.Equal(w.ShadowOrbSmashed, w2.ShadowOrbSmashed);
        Assert.Equal(w.SpawnMeteor, w2.SpawnMeteor);
        Assert.Equal(w.ShadowOrbCount, w2.ShadowOrbCount);
        Assert.Equal(w.AltarCount, w2.AltarCount);
        Assert.Equal(w.HardMode, w2.HardMode);
        Assert.Equal(w.InvasionDelay, w2.InvasionDelay);
        Assert.Equal(w.InvasionSize, w2.InvasionSize);
        Assert.Equal(w.InvasionType, w2.InvasionType);
        Assert.Equal(w.InvasionX, w2.InvasionX);

        // Verify all tiles are identical
        for (int x = 0; x < w.TilesWide; x++)
        {
            for (int y = 0; y < w.TilesHigh; y++)
            {
                var tile1 = w.Tiles[x, y];
                var tile2 = w2.Tiles[x, y];

                // Tile 127 is an invalid/placeholder tile that gets removed during save
                // Skip type 127 tiles - they should become inactive in the saved version
                if (tile1.Type == 127 && tile1.IsActive)
                {
                    Assert.False(tile2.IsActive);
                    continue;
                }

                Assert.Equal(tile1.IsActive, tile2.IsActive);

                if (tile1.IsActive)
                {
                    Assert.Equal(tile1.Type, tile2.Type);
                    Assert.Equal(tile1.U, tile2.U);
                    Assert.Equal(tile1.V, tile2.V);
                }

                Assert.Equal(tile1.Wall, tile2.Wall);
                Assert.Equal(tile1.LiquidAmount, tile2.LiquidAmount);
                Assert.Equal(tile1.LiquidType, tile2.LiquidType);
                Assert.Equal(tile1.WireRed, tile2.WireRed);
                Assert.Equal(tile1.WireGreen, tile2.WireGreen);
                Assert.Equal(tile1.WireBlue, tile2.WireBlue);
                Assert.Equal(tile1.WireYellow, tile2.WireYellow);
            }
        }

        // Verify chests are preserved
        Assert.Equal(w.Chests.Count, w2.Chests.Count);
        Console.WriteLine($"Original chests: {w.Chests.Count}, Saved chests: {w2.Chests.Count}");

        for (int i = 0; i < Math.Min(w.Chests.Count, w2.Chests.Count); i++)
        {
            var chest1 = w.Chests[i];
            var chest2 = w2.Chests[i];
            Assert.Equal(chest1.X, chest2.X);
            Assert.Equal(chest1.Y, chest2.Y);

            // Xbox format only has 20 slots per chest, not 40
            int slotsToCompare = Math.Min(20, Math.Min(chest1.Items.Count, chest2.Items.Count));

            for (int slot = 0; slot < slotsToCompare; slot++)
            {
                if (chest1.Items[slot].StackSize != chest2.Items[slot].StackSize)
                {
                    Console.WriteLine($"Chest {i}, Slot {slot}: StackSize mismatch - Expected {chest1.Items[slot].StackSize}, Actual {chest2.Items[slot].StackSize}");
                }
                Assert.Equal(chest1.Items[slot].StackSize, chest2.Items[slot].StackSize);

                // Only validate NetId and Prefix for items with StackSize > 0
                // Empty slots (StackSize == 0) don't have meaningful NetId/Prefix values
                if (chest1.Items[slot].StackSize > 0)
                {
                    if (chest1.Items[slot].NetId != chest2.Items[slot].NetId)
                    {
                        Console.WriteLine($"Chest {i}, Slot {slot}: NetId mismatch - Expected {chest1.Items[slot].NetId}, Actual {chest2.Items[slot].NetId}");
                    }
                    Assert.Equal(chest1.Items[slot].NetId, chest2.Items[slot].NetId);

                    if (chest1.Items[slot].Prefix != chest2.Items[slot].Prefix)
                    {
                        Console.WriteLine($"Chest {i}, Slot {slot}: Prefix mismatch - Expected {chest1.Items[slot].Prefix}, Actual {chest2.Items[slot].Prefix}");
                    }
                    Assert.Equal(chest1.Items[slot].Prefix, chest2.Items[slot].Prefix);
                }
            }
        }        // Verify signs are preserved
        Assert.Equal(w.Signs.Count, w2.Signs.Count);
        Console.WriteLine($"Original signs: {w.Signs.Count}, Saved signs: {w2.Signs.Count}");

        // Check for specific tombstone text
        var tombstones = w.Signs.Where(s =>
            s.Text.Contains("fell to their death") ||
            s.Text.Contains("vital organs were ruptured") ||
            s.Text.Contains("let their arms get torn off") ||
            s.Text.Contains("got impaled") ||
            s.Text.Contains("was slain")).ToList();

        Console.WriteLine($"Tombstones found in original: {tombstones.Count}");
        if (tombstones.Count > 0)
        {
            Console.WriteLine($"Sample tombstone: '{tombstones[0].Text}'");
        }

        for (int i = 0; i < Math.Min(w.Signs.Count, w2.Signs.Count); i++)
        {
            var sign1 = w.Signs[i];
            var sign2 = w2.Signs[i];
            Assert.Equal(sign1.X, sign2.X);
            Assert.Equal(sign1.Y, sign2.Y);
            Assert.Equal(sign1.Text, sign2.Text);
        }
    }

    [Theory]
    [InlineData(".\\WorldFiles\\android.wld")]
    public void LoadWorldAndroid_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\console.wld")]
    public void LoadWorldConsole_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
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
        var (w, er) = World.LoadWorld(fileName);
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
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    public void LoadWorldV2_144x_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\console.wld")]
    public void SaveWorld_Console_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.wld")]

    public void SaveWorldV0_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, versionOverride: -38, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
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
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.1.wld")]
    [InlineData(".\\WorldFiles\\v1.1.2.wld")]
    [InlineData(".\\WorldFiles\\v1.1.1.wld")]
    public void SaveWorldV1_Terraria1_1_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
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
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.0.5.wld")]

    public void SaveWorldV1_Terraria1_0_5_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
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
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }

    [Theory]
    [InlineData(".\\WorldFiles\\v1.4.4.1.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.2.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.3.wld")]
    [InlineData(".\\WorldFiles\\v1.4.4.4.wld")]
    public void SaveWorldV2_144x_Test(string fileName)
    {
        var (w, er) = World.LoadWorld(fileName);

        var saveTest = fileName + ".test";
        World.Save(w, saveTest, incrementRevision: false);

        // essentially, just a save and load test
        var w2 = World.LoadWorld(saveTest);
    }
}
