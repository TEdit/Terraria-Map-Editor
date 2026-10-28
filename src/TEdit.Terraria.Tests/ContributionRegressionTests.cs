using System.Text;
using TEdit.Terraria.Player;

namespace TEdit.Terraria.Tests;

public class ContributionRegressionTests
{
    [Fact]
    public void DowngradeNpcFiltering_DoesNotDropLaterSupportedNpcs()
    {
        const int version = 230;
        int unsupported = WorldConfiguration.SaveConfiguration.GetData(version).MaxNpcId + 1;
        byte[] Save(bool preserveAll, params int[] ids)
        {
            var world = new World { Version = version };
            foreach (int id in ids)
            {
                world.NPCs.Add(new NPC { SpriteId = id, Name = "npc", DisplayName = "npc", Position = new Vector2FloatObservable(1, 2), Home = new Vector2Int32Observable(3, 4) });
                world.Mobs.Add(new NPC { SpriteId = id, Position = new Vector2FloatObservable(1, 2) });
            }
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            World.SaveNPCs(world, writer, version, preserveAll);
            return stream.ToArray();
        }
        Assert.Equal(Save(false, 22, 17), Save(false, 22, unsupported, 17));
        Assert.True(Save(true, 22, unsupported, 17).Length > Save(false, 22, 17).Length);
    }

    [Fact]
    public void LoadedEmptyHatRack_HasIndependentModItemSlots()
    {
        // Reference TEHatRack: type, id, x, y, then a zero occupied-slot bitmask.
        byte[] data = [(byte)TileEntityType.HatRack, 0, 0, 0, 0, 10, 0, 10, 0, 0];
        using var reader = new BinaryReader(new MemoryStream(data));
        var entity = new TileEntity();
        entity.Load(reader, 325);
        Assert.NotSame(entity.Items[0], entity.Items[1]);
        Assert.NotSame(entity.Dyes[0], entity.Dyes[1]);
        entity.Items[0].ModName = "TestMod";
        entity.Items[0].ModItemName = "Hat";
        Assert.False(entity.Items[1].IsModItem);
        var copy = entity.Copy();
        Assert.NotSame(entity.Items[0], copy.Items[0]);
        Assert.True(copy.Items[0].IsModItem);
        Assert.False(copy.Items[1].IsModItem);
    }

    [Theory]
    [InlineData(215)]
    [InlineData(216)]
    [InlineData(325)]
    public void Signs_CountAndRecordsMatchTerraria(int version)
    {
        // Terraria 1.4.5.8 WorldFile.SaveSigns excludes null text from BOTH passes.
        var signs = new List<Sign> { new(1, 2, "first"), new(3, 4, null!), new(5, 6, "") };
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        World.SaveSigns(signs, writer, version);
        writer.Write(0x12345678);
        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        Assert.Equal(2, reader.ReadInt16());
        Assert.Equal("first", reader.ReadString());
        Assert.Equal(1, reader.ReadInt32());
        Assert.Equal(2, reader.ReadInt32());
        Assert.Equal("", reader.ReadString());
        Assert.Equal(5, reader.ReadInt32());
        Assert.Equal(6, reader.ReadInt32());
        Assert.Equal(0x12345678, reader.ReadInt32());
        Assert.Equal(stream.Length, stream.Position);
    }

    [Fact]
    public void Signs_LegacyLimitCountsOnlyWrittenRecords()
    {
        var signs = Enumerable.Range(0, 1005).Select(i => new Sign(i, 0, i < 5 ? null! : "s")).ToList();
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        World.SaveSigns(signs, writer, 215);
        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        Assert.Equal(1000, reader.ReadInt16());
        for (int i = 5; i < 1005; i++)
        {
            Assert.Equal("s", reader.ReadString());
            Assert.Equal(i, reader.ReadInt32());
            Assert.Equal(0, reader.ReadInt32());
        }
        Assert.Equal(stream.Length, stream.Position);
    }

    [Theory]
    [InlineData((int)TileType.HatRack)]
    [InlineData((int)TileType.DisplayDoll)]
    public void EntitySlots_DoNotShareMutableItems(int tileType)
    {
        var entity = TileEntity.CreateForTile(new Tile { IsActive = true, Type = (ushort)tileType }, 10, 10, 0);
        Assert.NotSame(entity.Items[0], entity.Items[1]);
        Assert.NotSame(entity.Dyes[0], entity.Dyes[1]);
        entity.Items[0].Id = 42;
        Assert.Equal(0, entity.Items[1].Id);
        entity.Dyes[0].Id = 1007;
        Assert.Equal(0, entity.Dyes[1].Id);
    }

    [Theory]
    [InlineData(317)]
    [InlineData(321)]
    [InlineData(322)]
    [InlineData(323)]
    [InlineData(324)]
    [InlineData(326)]
    public void PlayerEquipment_MatchesReferenceWireLayout(int version)
    {
        // Independently parse the changed sections using Terraria Player.Deserialize,
        // EquipmentLoadout.Deserialize and Item.DeserializeFrom version gates.
        var player = new PlayerCharacter { Name = "reference", Version = version, UsedAegisCrystal = true };
        player.Armor[0] = new PlayerItem(88, 1, 15, true);
        player.Dye[0] = new PlayerItem(1007, 1, 0, true);
        player.Loadouts[1].Armor[0] = new PlayerItem(90, 1, 3, true);
        using var stream = new MemoryStream();
        PlayerFile.Save(stream, player);
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        stream.Position = 24; // version, metadata magic, revision, favorite
        Assert.Equal("reference", reader.ReadString());
        stream.Position += 1 + 8 + 4 + 1 + 1 + 2 + 1 + 1 + 16; // core, appearance, stats
        stream.Position += 4; // extra accessory, two torch flags, bread
        if (version >= 324) Assert.False(reader.ReadBoolean());
        Assert.True(reader.ReadBoolean()); // Aegis must not shift into the reserved flag
        stream.Position += 5 + 1 + 12 + 21; // remaining permanent flags, DD2, counters, RGB colors
        for (int i = 0; i < 30; i++)
        {
            Assert.Equal(i == 0 ? 88 : i == 20 ? 1007 : 0, reader.ReadInt32());
            Assert.Equal(i == 0 ? 15 : 0, reader.ReadByte());
            if (version >= 322) Assert.Equal(i is 0 or 20, reader.ReadBoolean());
        }

        int loadoutLength = 3 * (30 * (version >= 322 ? 10 : 9) + 10);
        stream.Position = stream.Length - 13 - loadoutLength - 4; // empty refunds/dialogues, voice
        Assert.Equal(0, reader.ReadInt32());
        for (int set = 0; set < 3; set++)
        {
            for (int slot = 0; slot < 30; slot++)
            {
                bool populated = set == 1 && slot == 0;
                Assert.Equal(populated ? 90 : 0, reader.ReadInt32());
                Assert.Equal(populated ? 1 : 0, reader.ReadInt32());
                Assert.Equal(populated ? 3 : 0, reader.ReadByte());
                if (version >= 322) Assert.Equal(populated, reader.ReadBoolean());
            }
            for (int i = 0; i < 10; i++) Assert.False(reader.ReadBoolean());
        }
        Assert.Equal(stream.Length - 13, stream.Position);
        stream.Position = 0;
        var loaded = PlayerFile.Load(stream);
        Assert.Equal(version, loaded.Version);
        Assert.Equal(88, loaded.Armor[0].NetId);
        Assert.Equal(version >= 322, loaded.Armor[0].Favorited);
        Assert.Equal(version >= 322, loaded.Loadouts[1].Armor[0].Favorited);
        Assert.Equal(stream.Length, stream.Position);
    }
}
