using System.Text;
using System.Text.Json;
using Shouldly;
using TEdit.Common.Serialization;
using TEdit.Terraria.DataModel;

namespace TEdit.Terraria.Tests.DataModel;

public class SaveVersionManagerTests
{
    private static SaveVersionManager CreateTestManager()
    {
        var mgr = new SaveVersionManager
        {
            GameVersionToSaveVersion = new Dictionary<string, uint>
            {
                ["1.3.0.1"] = 146,
                ["1.4.0.1"] = 227,
                ["1.4.5.2"] = 315,
            },
            SaveVersions =
            [
                new SaveVersionData
                {
                    SaveVersion = 146,
                    GameVersion = "1.3.0.1",
                    MaxTileId = 470,
                    MaxWallId = 225,
                    MaxNpcId = 540,
                    FramedTileIds = [3, 4, 5, 10, 11],
                },
                new SaveVersionData
                {
                    SaveVersion = 227,
                    GameVersion = "1.4.0.1",
                    MaxTileId = 623,
                    MaxWallId = 316,
                    MaxNpcId = 670,
                    FramedTileIds = [3, 4, 5, 10, 11, 470],
                },
                new SaveVersionData
                {
                    SaveVersion = 315,
                    GameVersion = "1.4.5.2",
                    MaxTileId = 752,
                    MaxWallId = 366,
                    MaxNpcId = 696,
                    FramedTileIds = [3, 4, 5, 10, 11, 470, 623],
                },
            ],
        };
        mgr.BuildIndexes();
        return mgr;
    }

    [Fact]
    public void GetMaxVersion_ReturnsHighestSaveVersion()
    {
        var mgr = CreateTestManager();
        mgr.GetMaxVersion().ShouldBe(315);
    }

    [Fact]
    public void GetData_ReturnsCorrectVersion()
    {
        var mgr = CreateTestManager();

        var data = mgr.GetData(146);
        data.GameVersion.ShouldBe("1.3.0.1");
        data.MaxTileId.ShouldBe(470);
    }

    [Fact]
    public void GetData_ClampsToMax_WhenVersionTooHigh()
    {
        var mgr = CreateTestManager();

        var data = mgr.GetData(9999);
        data.SaveVersion.ShouldBe(315);
    }

    [Fact]
    public void GetDataForGameVersion_ExactMatch()
    {
        var mgr = CreateTestManager();

        var data = mgr.GetDataForGameVersion("1.4.0.1");
        data.GameVersion.ShouldBe("1.4.0.1");
    }

    [Fact]
    public void GetDataForGameVersion_FallbackToClosest()
    {
        var mgr = CreateTestManager();

        // 1.4.3.0 doesn't exist but should fall back to 1.4.0.1
        var data = mgr.GetDataForGameVersion("1.4.3.0");
        data.GameVersion.ShouldBe("1.4.0.1");
    }

    [Fact]
    public void GetTileFramesForVersion_ReturnsCorrectArray()
    {
        var mgr = CreateTestManager();

        var frames = mgr.GetTileFramesForVersion(146);
        frames[3].ShouldBe(true);
        frames[4].ShouldBe(true);
        frames[0].ShouldBe(false);
    }

    [Fact]
    public void RoundTrip_JsonSerialization()
    {
        var original = CreateTestManager();

        var json = JsonSerializer.Serialize(original, TEditJsonSerializer.DefaultOptions);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var restored = SaveVersionManager.Load(stream);

        restored.GetMaxVersion().ShouldBe(315);
        restored.SaveVersions.Count.ShouldBe(3);
        restored.GetData(146).MaxTileId.ShouldBe(470);
    }

    [Fact]
    public void Load_FromEmbeddedResource_Works()
    {
        using var stream = TEdit.Terraria.Loaders.JsonDataLoader.GetDataStream("versions.json");
        var mgr = SaveVersionManager.Load(stream);

        mgr.GetMaxVersion().ShouldBeGreaterThan(0);
        mgr.SaveVersions.Count.ShouldBeGreaterThan(0);
    }
}
