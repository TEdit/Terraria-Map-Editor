using System.IO;
using System.Text;
using TEdit.Terraria;
using Xunit;

namespace TEdit.Tests.Terraria;

public class WorldBackgroundPreservationTests
{
    [Theory]
    [InlineData(211)]
    [InlineData(279)]
    [InlineData(326)]
    public void ConsoleJungleBackgroundsAndTreeTopVariation_RemainIndependent(int version)
    {
        var source = NewWorld(version);
        source.JungleBackStyle = 1;
        source.BgJungle = 2;
        source.TreeTopVariations[5] = 4;

        byte[] bytes = SaveHeader(source);
        Assert.Equal(4, source.TreeTopVariations[5]); // Saving must not overwrite tree-top edits.
        var loaded = LoadHeader(bytes, version);
        Assert.Equal(1, loaded.JungleBackStyle);
        Assert.Equal(2, loaded.BgJungle);
        Assert.Equal(4, loaded.TreeTopVariations[5]);
        Assert.Equal(bytes, SaveHeader(loaded));

        loaded.JungleBackStyle = 0;
        var edited = LoadHeader(SaveHeader(loaded), version);
        Assert.Equal(0, edited.JungleBackStyle);
        Assert.Equal(2, edited.BgJungle);
        Assert.Equal(4, edited.TreeTopVariations[5]);
    }

    [Fact]
    public void Pre211TreeTops_RetainExistingInitialization()
    {
        var source = NewWorld(210);
        source.JungleBackStyle = 1;
        source.BgJungle = 4;
        var loaded = LoadHeader(SaveHeader(source), 210);
        // Pre-211 migration is deliberately outside the console preservation fix.
        Assert.Equal(1, loaded.TreeTopVariations[5]);
        Assert.Equal(1, loaded.JungleBackStyle);
    }

    [Fact]
    public void NonConsoleJungleBackground_RetainsExistingSynchronization()
    {
        var source = NewWorld(279);
        source.JungleBackStyle = 1;
        source.BgJungle = 2;
        source.TreeTopVariations[5] = 4;
        byte[] independentFields = SaveHeader(source);

        source.IsConsole = false;
        SaveHeader(source);
        Assert.Equal(1, source.TreeTopVariations[5]);

        var loaded = LoadHeader(independentFields, 279, isConsole: false);
        Assert.Equal(4, loaded.JungleBackStyle);
        Assert.Equal(2, loaded.BgJungle);
        Assert.Equal(4, loaded.TreeTopVariations[5]);
    }

    [Theory]
    [InlineData(3236, 3074, 6400, 3074, 3074, 6400)]
    [InlineData(300, 200, 100, 100, 100, 100)]
    [InlineData(100, 200, 300, 100, 200, 300)]
    [InlineData(200, 200, 200, 200, 200, 200)]
    public void SerializedCaveBoundaries_ArePreservedOnlyForConsoleWorlds(int first, int second, int third,
        int regularFirst, int regularSecond, int regularThird)
    {
        var source = NewWorld(279);
        source.CaveBackX2 = 6000;
        source.CaveBackX1 = 4000;
        source.CaveBackX0 = 2000;
        source.CaveBackStyle0 = 1;
        source.CaveBackStyle1 = 3;
        source.CaveBackStyle2 = 2;
        source.CaveBackStyle3 = 4;
        byte[] bytes = SaveHeader(source);

        // Inject the original on-disk values independently of the constrained model setters.
        // This exact block follows the tree styles in WorldFile.LoadHeader (format 279).
        using var marker = new MemoryStream();
        using (var writer = new BinaryWriter(marker, Encoding.UTF8, leaveOpen: true))
        {
            foreach (int value in new[] { 2000, 4000, 6000, 1, 3, 2, 4 })
            {
                writer.Write(value);
            }
        }
        int offset = bytes.AsSpan().IndexOf(marker.ToArray());
        Assert.True(offset >= 0);
        Assert.Equal(-1, bytes.AsSpan(offset + 1).IndexOf(marker.ToArray()));
        using (var writer = new BinaryWriter(new MemoryStream(bytes)))
        {
            writer.BaseStream.Position = offset;
            writer.Write(first);
            writer.Write(second);
            writer.Write(third);
        }

        var loaded = LoadHeader(bytes, 279);
        Assert.Equal(new[] { first, second, third }, new[] { loaded.CaveBackX0, loaded.CaveBackX1, loaded.CaveBackX2 });
        Assert.Equal(new[] { first, second, third }, loaded.CaveBackX.Take(3));
        Assert.Equal(bytes, SaveHeader(loaded));

        var regular = LoadHeader(bytes, 279, isConsole: false);
        Assert.Equal(new[] { regularFirst, regularSecond, regularThird },
            new[] { regular.CaveBackX0, regular.CaveBackX1, regular.CaveBackX2 });
    }

    [Fact]
    public void CaveBoundaryEditorConstraints_RemainEnabled()
    {
        var world = NewWorld(279);
        world.CaveBackX2 = 300;
        world.CaveBackX1 = 200;
        world.CaveBackX0 = 100;
        world.CaveBackX0 = 250;
        Assert.Equal(200, world.CaveBackX0);
        world.CaveBackX1 = 400;
        Assert.Equal(300, world.CaveBackX1);
        world.CaveBackX2 = 100;
        Assert.Equal(300, world.CaveBackX2);
    }

    private static World NewWorld(int version) => new()
    {
        Version = (uint)version, IsConsole = true, Title = "Background preservation", Seed = "test"
    };

    private static byte[] SaveHeader(World world)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        World.SaveHeaderFlags(world, writer, (int)world.Version);
        return stream.ToArray();
    }

    private static World LoadHeader(byte[] bytes, int version, bool isConsole = true)
    {
        using var reader = new BinaryReader(new MemoryStream(bytes));
        var world = NewWorld(version);
        world.IsConsole = isConsole;
        World.LoadHeaderFlags(reader, world, bytes.Length);
        Assert.Equal(bytes.Length, reader.BaseStream.Position);
        return world;
    }
}
