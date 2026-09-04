using System.IO;
using Xunit;

namespace TEdit.Terraria.Tests;

public class WorldFileV2HeaderTests
{
    [Fact]
    public void Version326_LightningSeedFlags_RoundTrip()
    {
        var source = new World
        {
            Version = 326,
            Title = "Header test",
            Seed = "seed",
            MoreLightningSeed = true,
            NoLightningSeed = true,
            WorldManifestData = "manifest"
        };

        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            World.SaveHeaderFlags(source, writer, 326);
        }

        var target = new World { Version = 326 };
        stream.Position = 0;
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        World.LoadHeaderFlags(reader, target, (int)stream.Length);

        Assert.True(target.MoreLightningSeed);
        Assert.True(target.NoLightningSeed);
        Assert.Equal("manifest", target.WorldManifestData);
        Assert.Equal(stream.Length, stream.Position);
    }
}
