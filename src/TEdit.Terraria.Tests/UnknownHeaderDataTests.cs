using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace TEdit.Terraria.Tests;

public class UnknownHeaderDataTests
{
    private static byte[] SaveHeader(uint readAtVersion)
    {
        var world = new World(10, 10, "Test", 1) { Version = WorldConfiguration.CompatibleVersion };
        world.UnknownData = [0xA1, 0xB2, 0xC3];
        world.UnknownDataVersion = readAtVersion;

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        World.SaveHeaderFlags(world, bw, (int)world.Version);
        return ms.ToArray();
    }

    [Fact]
    public void SaveHeaderFlags_SameVersion_WritesUnknownBytesBack()
    {
        var bytes = SaveHeader(WorldConfiguration.CompatibleVersion);

        bytes.TakeLast(3).ShouldBe(new byte[] { 0xA1, 0xB2, 0xC3 });
    }

    [Fact]
    public void SaveHeaderFlags_OtherVersion_LeavesUnknownBytesOut()
    {
        var same = SaveHeader(WorldConfiguration.CompatibleVersion);
        var other = SaveHeader(WorldConfiguration.CompatibleVersion + 1);

        other.Length.ShouldBe(same.Length - 3);
    }
}
