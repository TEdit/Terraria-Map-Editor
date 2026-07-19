using System;
using System.IO;
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
        Assert.Equal(876, world.SpawnX);
        Assert.Equal(298, world.SpawnY);
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
    public void Save_WinPhoneEditedTile_RejectsLossySave()
    {
        var (world, error) = World.LoadWorld(V60FixturePath);
        Assert.Null(error);

        world.Tiles[0, 0].WireRed = !world.Tiles[0, 0].WireRed;
        string savedPath = Path.Combine(Path.GetTempPath(), $"tedit-winphone-edited-{Guid.NewGuid():N}.world");
        try
        {
            Assert.Throws<NotSupportedException>(() => World.Save(world, savedPath));
        }
        finally
        {
            File.Delete(savedPath);
            File.Delete(savedPath + ".tmp");
        }
    }

    [Fact]
    public void Save_WinPhoneEditedSign_RejectsLossySave()
    {
        var (world, error) = World.LoadWorld(V60FixturePath);
        Assert.Null(error);
        Assert.NotEmpty(world.Signs);

        world.Signs[0].Text += " edited";
        string savedPath = Path.Combine(Path.GetTempPath(), $"tedit-winphone-sign-{Guid.NewGuid():N}.world");
        try
        {
            Assert.Throws<NotSupportedException>(() => World.Save(world, savedPath));
        }
        finally
        {
            File.Delete(savedPath);
            File.Delete(savedPath + ".tmp");
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
