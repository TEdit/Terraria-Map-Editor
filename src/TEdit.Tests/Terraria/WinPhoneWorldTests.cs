using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TEdit.Terraria.Tests;

public class WinPhoneWorldTests
{
    private const string V60FixturePath = ".\\WorldFiles\\win-phone.world";

    private static readonly string[] V49FixtureCandidates =
    [
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "_reference_sources", "winphone", "Terraria v1.0.0.0", "Tutorial.world")),
        Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "_reference_sources", "winphone", "Terraria v1.2.4.3", "Tutorial.world")),
        Path.GetFullPath(Path.Combine(
            Environment.CurrentDirectory,
            "..", "_reference_sources", "winphone", "Terraria v1.2.4.3", "Tutorial.world")),
    ];

    [Fact]
    public void LoadWorld_WinPhoneV60_ReadsNativeHeader()
    {
        Assert.True(File.Exists(V60FixturePath), $"Missing fixture: {V60FixturePath}");

        var (world, error) = World.LoadWorld(V60FixturePath);

        Assert.Null(error);
        Assert.True(world.IsWinPhone);
        Assert.Equal((uint)60, world.Version);
        Assert.Equal("ZSRR", world.Title);
        Assert.Equal(1750, world.TilesWide);
        Assert.Equal(1000, world.TilesHigh);
        Assert.Equal(250, world.SpawnX);
        Assert.Equal(267, world.SpawnY);
        Assert.Equal(876, world.DungeonX);
        Assert.Equal(298, world.DungeonY);
        Assert.Equal(1_750_000, world.Tiles.Length);
        Assert.True(ContainsActiveTile(world));
    }

    [Fact]
    public void LoadSave_WinPhoneV60_IsByteIdentical()
    {
        AssertLosslessRoundTrip(V60FixturePath, expectedVersion: 60, expectedTitle: "ZSRR", fullLoad: true);
    }

    [Fact]
    public void LoadSave_WinPhoneV49Tutorial_IsByteIdentical()
    {
        string[] fixtures = Array.FindAll(V49FixtureCandidates, File.Exists);
        Assert.Equal(2, fixtures.Length);
        foreach (string fixture in fixtures)
            AssertLosslessRoundTrip(fixture, expectedVersion: 49, expectedTitle: "Tutorial", fullLoad: true);
    }

    [Fact]
    public void ValidateWorldFile_WinPhoneV60_IsValidNativeContainer()
    {
        var status = World.ValidateWorldFile(V60FixturePath);

        Assert.Equal((uint)60, status.Version);
        Assert.True(status.IsLegacy);
        Assert.True(status.IsValid, status.Message);
        Assert.False(status.IsConsole);
        Assert.False(status.IsChinese);
        Assert.Equal(3, status.LoaderVersion);
    }

    [Fact]
    public void Save_WinPhoneEditedTile_ReloadsChange()
    {
        string[] fixtures = [V60FixturePath, Array.Find(V49FixtureCandidates, File.Exists)!];
        Assert.DoesNotContain(fixtures, string.IsNullOrEmpty);
        foreach (string fixture in fixtures)
        {
            var (world, error) = World.LoadWorld(fixture);
            Assert.Null(error);

            bool expectedWire = !world.Tiles[0, 0].WireRed;
            world.Tiles[0, 0].WireRed = expectedWire;
            string savedPath = Path.Combine(Path.GetTempPath(), $"tedit-winphone-edited-{Guid.NewGuid():N}.world");
            try
            {
                World.Save(world, savedPath);
                var (reloaded, reloadError) = World.LoadWorld(savedPath);

                Assert.Null(reloadError);
                Assert.Equal(expectedWire, reloaded.Tiles[0, 0].WireRed);
                Assert.True(World.ValidateWorldFile(savedPath).IsValid);
            }
            finally
            {
                File.Delete(savedPath);
                File.Delete(savedPath + ".tmp");
            }
        }
    }

    [Fact]
    public void Save_WinPhoneEditedSections_ReloadChanges()
    {
        var (world, error) = World.LoadWorld(V60FixturePath);
        Assert.Null(error);
        Assert.NotEmpty(world.Chests);
        Assert.NotEmpty(world.Signs);
        Assert.NotEmpty(world.NPCs);
        Assert.NotEmpty(world.CharacterNames);

        Chest chest = world.Chests[0];
        int itemIndex = 0;
        while (itemIndex < chest.MaxItems && chest.Items[itemIndex].NetId == 0)
            itemIndex++;
        Assert.True(itemIndex < chest.MaxItems);

        int expectedStack = chest.Items[itemIndex].StackSize + 1;
        string expectedSign = world.Signs[0].Text + " edited";
        bool expectedHomeless = !world.NPCs[0].IsHomeless;
        string expectedName = world.CharacterNames[0].Name + " Jr.";
        chest.Items[itemIndex].StackSize = expectedStack;
        world.Signs[0].Text = expectedSign;
        world.NPCs[0].IsHomeless = expectedHomeless;
        world.CharacterNames[0].Name = expectedName;

        string savedPath = Path.Combine(Path.GetTempPath(), $"tedit-winphone-sign-{Guid.NewGuid():N}.world");
        try
        {
            World.Save(world, savedPath);
            var (reloaded, reloadError) = World.LoadWorld(savedPath);

            Assert.Null(reloadError);
            Assert.Equal(expectedStack, reloaded.Chests[0].Items[itemIndex].StackSize);
            Assert.Equal(expectedSign, reloaded.Signs[0].Text);
            Assert.Equal(expectedHomeless, reloaded.NPCs[0].IsHomeless);
            Assert.Equal(expectedName, reloaded.CharacterNames[0].Name);
        }
        finally
        {
            File.Delete(savedPath);
            File.Delete(savedPath + ".tmp");
        }
    }

    [Fact]
    public void Save_WinPhoneEditableMetadata_ReloadsChanges()
    {
        string[] fixtures = [V60FixturePath, Array.Find(V49FixtureCandidates, File.Exists)!];
        Assert.DoesNotContain(fixtures, string.IsNullOrEmpty);

        foreach (string fixture in fixtures)
        {
            var (world, error) = World.LoadWorld(fixture);
            Assert.Null(error);

            string expectedTitle = world.Title + " Edit";
            int expectedWorldId = world.WorldId + 1;
            int expectedDungeonX = world.DungeonX - 1;
            int expectedDungeonY = world.DungeonY - 1;
            int expectedSpawnX = world.SpawnX + 1;
            int expectedSpawnY = world.SpawnY + 1;
            double expectedGround = world.GroundLevel + 1;
            double expectedRock = world.RockLevel + 1;
            double expectedTime = world.Time + 1;
            bool expectedDayTime = !world.DayTime;
            int expectedMoonPhase = (world.MoonPhase + 1) % 8;
            bool expectedBloodMoon = !world.BloodMoon;
            bool expectedCrimson = !world.IsCrimson;
            bool expectedRaining = !world.IsRaining;
            int expectedRainTime = world.TempRainTime + 60;
            float expectedMaxRain = world.TempMaxRain == 0.5f ? 0.25f : 0.5f;

            world.Title = expectedTitle;
            world.WorldId = expectedWorldId;
            world.DungeonX = expectedDungeonX;
            world.DungeonY = expectedDungeonY;
            world.SpawnX = expectedSpawnX;
            world.SpawnY = expectedSpawnY;
            world.GroundLevel = expectedGround;
            world.RockLevel = expectedRock;
            world.Time = expectedTime;
            world.DayTime = expectedDayTime;
            world.MoonPhase = expectedMoonPhase;
            world.BloodMoon = expectedBloodMoon;
            if (world.Version >= 58)
            {
                world.IsCrimson = expectedCrimson;
                world.IsRaining = expectedRaining;
                world.TempRainTime = expectedRainTime;
                world.TempMaxRain = expectedMaxRain;
            }

            string savedPath = Path.Combine(
                Path.GetTempPath(), $"tedit-winphone-metadata-{Guid.NewGuid():N}.world");
            try
            {
                World.Save(world, savedPath);
                var (reloaded, reloadError) = World.LoadWorld(savedPath);

                Assert.Null(reloadError);
                Assert.True(reloaded.IsWinPhone);
                Assert.Equal(expectedTitle, reloaded.Title);
                Assert.Equal(expectedWorldId, reloaded.WorldId);
                Assert.Equal(expectedDungeonX, reloaded.DungeonX);
                Assert.Equal(expectedDungeonY, reloaded.DungeonY);
                Assert.Equal(expectedSpawnX, reloaded.SpawnX);
                Assert.Equal(expectedSpawnY, reloaded.SpawnY);
                Assert.Equal(expectedGround, reloaded.GroundLevel);
                Assert.Equal(expectedRock, reloaded.RockLevel);
                Assert.Equal(expectedTime, reloaded.Time);
                Assert.Equal(expectedDayTime, reloaded.DayTime);
                Assert.Equal(expectedMoonPhase, reloaded.MoonPhase);
                Assert.Equal(expectedBloodMoon, reloaded.BloodMoon);
                if (world.Version >= 58)
                {
                    Assert.Equal(expectedCrimson, reloaded.IsCrimson);
                    Assert.Equal(expectedRaining, reloaded.IsRaining);
                    Assert.Equal(expectedRainTime, reloaded.TempRainTime);
                    Assert.Equal(expectedMaxRain, reloaded.TempMaxRain);
                }
                Assert.True(World.ValidateWorldFile(savedPath).IsValid);
            }
            finally
            {
                File.Delete(savedPath);
                File.Delete(savedPath + ".tmp");
            }
        }
    }

    [Fact]
    public void Save_WinPhoneProgressionAndWorldState_ReloadsChanges()
    {
        string[] fixtures = [V60FixturePath, Array.Find(V49FixtureCandidates, File.Exists)!];
        Assert.DoesNotContain(fixtures, string.IsNullOrEmpty);

        foreach (string fixture in fixtures)
        {
            var (world, error) = World.LoadWorld(fixture);
            Assert.Null(error);

            world.DownedBoss1EyeofCthulhu = !world.DownedBoss1EyeofCthulhu;
            world.DownedBoss2EaterofWorlds = !world.DownedBoss2EaterofWorlds;
            world.DownedBoss3Skeletron = !world.DownedBoss3Skeletron;
            world.SavedGoblin = !world.SavedGoblin;
            world.SavedWizard = !world.SavedWizard;
            world.SavedMech = !world.SavedMech;
            world.DownedGoblins = !world.DownedGoblins;
            world.DownedClown = !world.DownedClown;
            world.DownedFrost = !world.DownedFrost;
            world.ShadowOrbCount = world.ShadowOrbCount == 2 ? 1 : 2;
            world.ShadowOrbSmashed = true;
            world.SpawnMeteor = !world.SpawnMeteor;
            world.AltarCount += 1;
            world.HardMode = !world.HardMode;
            world.InvasionDelay = world.InvasionDelay == 6 ? 5 : 6;
            world.InvasionSize = world.InvasionSize == 123 ? 122 : 123;
            world.InvasionType = world.InvasionType == 3 ? 1 : 3;
            world.InvasionX = 321;

            if (world.Version >= 58)
            {
                world.DownedQueenBee = !world.DownedQueenBee;
                world.DownedMechBoss1TheDestroyer = !world.DownedMechBoss1TheDestroyer;
                world.DownedMechBoss2TheTwins = !world.DownedMechBoss2TheTwins;
                world.DownedMechBoss3SkeletronPrime = !world.DownedMechBoss3SkeletronPrime;
                world.DownedPlantBoss = !world.DownedPlantBoss;
                world.DownedGolemBoss = !world.DownedGolemBoss;
                world.DownedPirates = !world.DownedPirates;
                world.SavedOreTiersCobalt = world.SavedOreTiersCobalt == 107 ? 221 : 107;
                world.SavedOreTiersMythril = world.SavedOreTiersMythril == 108 ? 222 : 108;
                world.SavedOreTiersAdamantite = world.SavedOreTiersAdamantite == 111 ? 223 : 111;
            }

            string savedPath = Path.Combine(
                Path.GetTempPath(), $"tedit-winphone-state-{Guid.NewGuid():N}.world");
            try
            {
                World.Save(world, savedPath);
                var (reloaded, reloadError) = World.LoadWorld(savedPath);

                Assert.Null(reloadError);
                Assert.Equal(world.DownedBoss1EyeofCthulhu, reloaded.DownedBoss1EyeofCthulhu);
                Assert.Equal(world.DownedBoss2EaterofWorlds, reloaded.DownedBoss2EaterofWorlds);
                Assert.Equal(world.DownedBoss3Skeletron, reloaded.DownedBoss3Skeletron);
                Assert.Equal(world.SavedGoblin, reloaded.SavedGoblin);
                Assert.Equal(world.SavedWizard, reloaded.SavedWizard);
                Assert.Equal(world.SavedMech, reloaded.SavedMech);
                Assert.Equal(world.DownedGoblins, reloaded.DownedGoblins);
                Assert.Equal(world.DownedClown, reloaded.DownedClown);
                Assert.Equal(world.DownedFrost, reloaded.DownedFrost);
                Assert.True(reloaded.ShadowOrbSmashed);
                Assert.Equal(world.ShadowOrbCount, reloaded.ShadowOrbCount);
                Assert.Equal(world.SpawnMeteor, reloaded.SpawnMeteor);
                Assert.Equal(world.AltarCount, reloaded.AltarCount);
                Assert.Equal(world.HardMode, reloaded.HardMode);
                Assert.Equal(world.InvasionDelay, reloaded.InvasionDelay);
                Assert.Equal(world.InvasionSize, reloaded.InvasionSize);
                Assert.Equal(world.InvasionType, reloaded.InvasionType);
                Assert.Equal(world.InvasionX, reloaded.InvasionX);

                if (world.Version >= 58)
                {
                    Assert.Equal(world.DownedQueenBee, reloaded.DownedQueenBee);
                    Assert.Equal(world.DownedMechBoss1TheDestroyer, reloaded.DownedMechBoss1TheDestroyer);
                    Assert.Equal(world.DownedMechBoss2TheTwins, reloaded.DownedMechBoss2TheTwins);
                    Assert.Equal(world.DownedMechBoss3SkeletronPrime, reloaded.DownedMechBoss3SkeletronPrime);
                    Assert.Equal(world.DownedPlantBoss, reloaded.DownedPlantBoss);
                    Assert.Equal(world.DownedGolemBoss, reloaded.DownedGolemBoss);
                    Assert.Equal(world.DownedPirates, reloaded.DownedPirates);
                    Assert.Equal(world.SavedOreTiersCobalt, reloaded.SavedOreTiersCobalt);
                    Assert.Equal(world.SavedOreTiersMythril, reloaded.SavedOreTiersMythril);
                    Assert.Equal(world.SavedOreTiersAdamantite, reloaded.SavedOreTiersAdamantite);
                }
                Assert.True(World.ValidateWorldFile(savedPath).IsValid);
            }
            finally
            {
                File.Delete(savedPath);
                File.Delete(savedPath + ".tmp");
            }
        }
    }

    [Fact]
    public void Save_WinPhoneSparseSections_AddAndRemoveEntries()
    {
        string[] fixtures = [V60FixturePath, Array.Find(V49FixtureCandidates, File.Exists)!];
        Assert.DoesNotContain(fixtures, string.IsNullOrEmpty);

        foreach (string fixture in fixtures)
        {
            var (world, error) = World.LoadWorld(fixture);
            Assert.Null(error);
            Assert.NotEmpty(world.Chests);
            Assert.NotEmpty(world.NPCs);

            Chest removedChest = world.Chests[0];
            world.Chests.RemoveAt(0);
            int addedChestX = 12;
            int addedChestY = 34;
            while ((removedChest.X == addedChestX && removedChest.Y == addedChestY) ||
                   world.Chests.Any(chest => chest.X == addedChestX && chest.Y == addedChestY))
                addedChestX++;
            Chest addedChest = new(addedChestX, addedChestY) { MaxItems = world.Version < 58 ? 20 : 40 };
            addedChest.Items[0] = new Item(2, 1, 0);
            world.Chests.Add(addedChest);

            if (world.Signs.Count > 0)
                world.Signs.RemoveAt(0);
            world.Signs.Add(new Sign(14, 36, "Windows Phone sign"));

            world.NPCs.RemoveAt(0);
            world.NPCs.Add(new NPC
            {
                SpriteId = 17,
                Position = new Vector2FloatObservable(160, 320),
                IsHomeless = false,
                Home = new Vector2Int32Observable(10, 20),
            });

            string savedPath = Path.Combine(
                Path.GetTempPath(), $"tedit-winphone-sections-{Guid.NewGuid():N}.world");
            try
            {
                World.Save(world, savedPath);
                var (reloaded, reloadError) = World.LoadWorld(savedPath);

                Assert.Null(reloadError);
                Assert.DoesNotContain(reloaded.Chests, chest =>
                    chest.X == removedChest.X && chest.Y == removedChest.Y);
                Chest reloadedChest = Assert.Single(reloaded.Chests, chest =>
                    chest.X == addedChest.X && chest.Y == addedChest.Y);
                Assert.Equal(2, reloadedChest.Items[0].StackSize);
                Assert.Contains(reloaded.Signs, sign =>
                    sign.X == 14 && sign.Y == 36 && sign.Text == "Windows Phone sign");
                Assert.Contains(reloaded.NPCs, npc =>
                    npc.SpriteId == 17 && npc.Home.X == 10 && npc.Home.Y == 20);
            }
            finally
            {
                File.Delete(savedPath);
                File.Delete(savedPath + ".tmp");
            }
        }
    }

    [Fact]
    public void Reconstruct_WinPhoneDecodedSections_DoesNotCopyCachedSectionBytes()
    {
        string[] fixtures = [V60FixturePath, .. Array.FindAll(V49FixtureCandidates, File.Exists)];
        Assert.Equal(3, fixtures.Length);

        foreach (string fixture in fixtures)
        {
            byte[] expected = File.ReadAllBytes(fixture);
            var (world, error) = World.LoadWorld(fixture);
            Assert.Null(error);

            Array.Fill(
                world.WinPhoneSourceData!,
                (byte)0xA5,
                0,
                world.WinPhoneParsedLength);

            using MemoryStream output = new(expected.Length);
            World.SaveWinPhoneReconstructed(world, output);
            Assert.Equal(expected, output.ToArray());
        }
    }

    private static void AssertLosslessRoundTrip(
        string fixture,
        uint expectedVersion,
        string expectedTitle,
        bool fullLoad)
    {
        byte[] expected = File.ReadAllBytes(fixture);
        var (world, error) = World.LoadWorld(fixture, headersOnly: !fullLoad);

        Assert.Null(error);
        Assert.True(world.IsWinPhone);
        Assert.Equal(expectedVersion, world.Version);
        Assert.Equal(expectedTitle, world.Title);

        if (fullLoad)
        {
            Assert.NotEmpty(world.Chests);
            if (expectedVersion < 50)
                Assert.Empty(world.Signs);
            else
                Assert.NotEmpty(world.Signs);
            Assert.Equal(expectedVersion < 50 ? 2 : 1, world.NPCs.Count);
            Assert.Equal(expectedVersion < 50 ? 10 : 18, world.CharacterNames.Count);
            Assert.Equal(expectedVersion < 50 ? 35 : 79, world.WinPhoneMetadataPrimitives!.Length);
            Assert.Equal(expectedVersion < 50 ? 201_842 : expected.Length, world.WinPhoneParsedLength);
        }

        using var output = new MemoryStream(expected.Length);
        World.SaveWinPhoneUnchanged(world, output);

        Assert.Equal(expected, output.ToArray());

        if (fullLoad)
        {
            string savedPath = Path.Combine(Path.GetTempPath(), $"tedit-winphone-{Guid.NewGuid():N}.world");
            try
            {
                World.Save(world, savedPath);
                Assert.Equal(expected, File.ReadAllBytes(savedPath));
            }
            finally
            {
                File.Delete(savedPath);
                File.Delete(savedPath + ".tmp");
            }
        }
    }

    private static bool ContainsActiveTile(World world)
    {
        for (int x = 0; x < world.TilesWide; x++)
        for (int y = 0; y < world.TilesHigh; y++)
            if (world.Tiles[x, y].IsActive)
                return true;
        return false;
    }
}
