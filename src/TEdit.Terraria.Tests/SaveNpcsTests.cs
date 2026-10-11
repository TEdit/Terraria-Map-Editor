using System.IO;
using System.Linq;
using Shouldly;
using Xunit;

namespace TEdit.Terraria.Tests;

public class SaveNpcsTests
{
    private const int Guide = 22;
    private const int Merchant = 17;

    [Fact]
    public void SaveNPCs_NpcTooNewForVersion_IsSkippedAndLaterNpcsAreKept()
    {
        const uint version = 230;
        int tooNew = WorldConfiguration.SaveConfiguration.GetData((int)version).MaxNpcId + 1;

        var loaded = RoundTrip(version, preserveAll: false, Guide, tooNew, Merchant);

        loaded.NPCs.Select(n => n.SpriteId).ShouldBe([Guide, Merchant]);
        loaded.Mobs.Select(n => n.SpriteId).ShouldBe([Guide, Merchant]);
    }

    [Fact]
    public void SaveNPCs_PreV190NpcWithoutName_IsSkippedInsteadOfThrowing()
    {
        const uint version = 180;
        const int unknownId = 99999;

        var loaded = RoundTrip(version, preserveAll: true, Guide, unknownId, Merchant);

        loaded.NPCs.Select(n => n.SpriteId).ShouldBe([Guide, Merchant]);
        loaded.Mobs.Count.ShouldBe(2);
    }

    private static World RoundTrip(uint version, bool preserveAll, params int[] ids)
    {
        var world = new World { Version = version };
        foreach (int id in ids)
        {
            world.NPCs.Add(new NPC { SpriteId = id, Name = "npc", DisplayName = "npc", Position = new Vector2FloatObservable(1, 2), Home = new Vector2Int32Observable(3, 4) });
            world.Mobs.Add(new NPC { SpriteId = id, Position = new Vector2FloatObservable(1, 2) });
        }

        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            World.SaveNPCs(world, bw, (int)version, preserveAll);
        }

        ms.Position = 0;
        var loaded = new World { Version = version };
        using var br = new BinaryReader(ms);
        World.LoadNPCsData(br, loaded);
        return loaded;
    }
}
