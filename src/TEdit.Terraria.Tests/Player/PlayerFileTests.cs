using Shouldly;
using TEdit.Terraria.Player;

namespace TEdit.Terraria.Tests.Player;

public class PlayerFileTests
{
    private static readonly string PlayersDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "my games", "Terraria", "Players");

    [Fact]
    public void EncryptDecrypt_RoundTrip()
    {
        byte[] original = new byte[256];
        Random.Shared.NextBytes(original);

        byte[] encrypted = PlayerFile.Encrypt(original);
        byte[] decrypted = PlayerFile.Decrypt(encrypted);

        // Decrypted may have padding bytes at the end, so just check the original portion
        decrypted.Length.ShouldBeGreaterThanOrEqualTo(original.Length);
        decrypted.AsSpan(0, original.Length).SequenceEqual(original).ShouldBeTrue();
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_InMemory()
    {
        var original = CreateTestCharacter();

        // Serialize to stream
        using var ms = new MemoryStream();
        PlayerFile.Save(ms, original);

        // Deserialize from stream
        ms.Position = 0;
        var loaded = PlayerFile.Load(ms);

        AssertCharactersEqual(original, loaded);
    }

    public static IEnumerable<object[]> GetPlrFiles()
    {
        if (!Directory.Exists(PlayersDir))
            yield break;

        foreach (var file in Directory.GetFiles(PlayersDir, "*.plr"))
            yield return [file];
    }

    [Theory]
    [MemberData(nameof(GetPlrFiles))]
    public void Load_RealPlrFile_DoesNotThrow(string path)
    {
        var player = PlayerFile.Load(path);
        player.Name.ShouldNotBeNullOrEmpty();
    }

    [Theory]
    [MemberData(nameof(GetPlrFiles))]
    public void RoundTrip_RealPlrFile_PreservesData(string path)
    {
        var original = PlayerFile.Load(path);

        // Save to unencrypted stream, reload from unencrypted stream
        using var ms = new MemoryStream();
        PlayerFile.Save(ms, original);

        ms.Position = 0;
        var reloaded = PlayerFile.Load(ms);

        AssertCharactersEqual(original, reloaded);
    }

    [Theory]
    [MemberData(nameof(GetPlrFiles))]
    public void RoundTrip_RealPlrFile_ByteLevel(string path)
    {
        // Decrypt original to check version
        byte[] originalData = PlayerFile.Decrypt(File.ReadAllBytes(path));
        int originalVersion = BitConverter.ToInt32(originalData, 0);

        // Load and immediately save (unencrypted stream)
        var player = PlayerFile.Load(path);

        using var ms = new MemoryStream();
        PlayerFile.Save(ms, player);
        byte[] savedData = ms.ToArray();

        // Version (4 bytes) should be preserved
        int savedVersion = BitConverter.ToInt32(savedData, 0);
        savedVersion.ShouldBe(originalVersion, "Version should be preserved on save");

        // Metadata: version(4) + magic(8) + revision(4) + favorite(8) = 24 bytes
        // Revision increments by 1 on save, so skip bytes 12-15 (the revision field)
        int metadataEnd = originalVersion >= 135 ? 24 : 4;

        // Compare serialized data after metadata (skipping revision)
        var originalSpan = originalData.AsSpan(metadataEnd);
        var savedSpan = savedData.AsSpan(metadataEnd);

        int minLen = Math.Min(originalSpan.Length, savedSpan.Length);
        for (int i = 0; i < minLen; i++)
        {
            if (originalSpan[i] != savedSpan[i])
            {
                Assert.Fail($"Byte mismatch at offset {metadataEnd + i} (data offset {i}): " +
                           $"original=0x{originalSpan[i]:X2}, saved=0x{savedSpan[i]:X2}. " +
                           $"File: {Path.GetFileName(path)}, version: {originalVersion}");
            }
        }
    }

    private static PlayerCharacter CreateTestCharacter()
    {
        var player = new PlayerCharacter
        {
            Name = "TestPlayer",
            Difficulty = 3, // Journey
            PlayTimeTicks = TimeSpan.FromHours(10).Ticks,
            Team = 1,
            StatLife = 400,
            StatLifeMax = 500,
            StatMana = 200,
            StatManaMax = 200,
            ExtraAccessory = true,
            UnlockedBiomeTorches = true,
            UsingBiomeTorches = true,
            AteArtisanBread = true,
            AnglerQuestsFinished = 42,
            BartenderQuestLog = 3,
            GolferScoreAccumulated = 100,
            VoiceVariant = 1,
        };

        player.Appearance.Hair = 5;
        player.Appearance.HairDye = 2;
        player.Appearance.SkinVariant = 1;

        // Add a test inventory item
        player.Inventory[0] = new PlayerItem(1, 1, 0, true);
        player.Inventory[1] = new PlayerItem(29, 1, 82);

        // Add a spawn point
        player.SpawnPoints.Add(new SpawnPoint(100, 200, 12345, "TestWorld"));

        // Add creative sacrifices
        player.CreativeSacrifices["Terraria/DirtBlock"] = 100;

        return player;
    }

    private static void AssertCharactersEqual(PlayerCharacter a, PlayerCharacter b)
    {
        // Core properties
        b.Name.ShouldBe(a.Name);
        b.Difficulty.ShouldBe(a.Difficulty);
        b.PlayTimeTicks.ShouldBe(a.PlayTimeTicks);
        b.Team.ShouldBe(a.Team);

        // Appearance
        b.Appearance.Hair.ShouldBe(a.Appearance.Hair);
        b.Appearance.HairDye.ShouldBe(a.Appearance.HairDye);
        b.Appearance.SkinVariant.ShouldBe(a.Appearance.SkinVariant);
        b.Appearance.HairColor.ShouldBe(a.Appearance.HairColor);
        b.Appearance.SkinColor.ShouldBe(a.Appearance.SkinColor);

        // Stats
        b.StatLife.ShouldBe(a.StatLife);
        b.StatLifeMax.ShouldBe(a.StatLifeMax);
        b.StatMana.ShouldBe(a.StatMana);
        b.StatManaMax.ShouldBe(a.StatManaMax);
        b.ExtraAccessory.ShouldBe(a.ExtraAccessory);

        // Flags
        b.UnlockedBiomeTorches.ShouldBe(a.UnlockedBiomeTorches);
        b.AteArtisanBread.ShouldBe(a.AteArtisanBread);

        // Inventory spot check
        for (int i = 0; i < PlayerConstants.MaxInventorySlots; i++)
        {
            b.Inventory[i].NetId.ShouldBe(a.Inventory[i].NetId, $"Inventory[{i}].NetId");
            b.Inventory[i].StackSize.ShouldBe(a.Inventory[i].StackSize, $"Inventory[{i}].StackSize");
            b.Inventory[i].Prefix.ShouldBe(a.Inventory[i].Prefix, $"Inventory[{i}].Prefix");
            b.Inventory[i].Favorited.ShouldBe(a.Inventory[i].Favorited, $"Inventory[{i}].Favorited");
        }

        // Spawn points
        b.SpawnPoints.Count.ShouldBe(a.SpawnPoints.Count);
        for (int i = 0; i < a.SpawnPoints.Count; i++)
        {
            b.SpawnPoints[i].X.ShouldBe(a.SpawnPoints[i].X);
            b.SpawnPoints[i].Y.ShouldBe(a.SpawnPoints[i].Y);
            b.SpawnPoints[i].WorldId.ShouldBe(a.SpawnPoints[i].WorldId);
            b.SpawnPoints[i].WorldName.ShouldBe(a.SpawnPoints[i].WorldName);
        }

        // Banks
        for (int i = 0; i < PlayerConstants.MaxBankSlots; i++)
        {
            b.Bank1[i].NetId.ShouldBe(a.Bank1[i].NetId, $"Bank1[{i}].NetId");
            b.Bank4[i].NetId.ShouldBe(a.Bank4[i].NetId, $"Bank4[{i}].NetId");
            b.Bank4[i].Favorited.ShouldBe(a.Bank4[i].Favorited, $"Bank4[{i}].Favorited");
        }

        // Loadouts
        b.CurrentLoadoutIndex.ShouldBe(a.CurrentLoadoutIndex);
        for (int l = 0; l < PlayerConstants.MaxLoadouts; l++)
        {
            for (int i = 0; i < PlayerConstants.MaxArmorSlots; i++)
                b.Loadouts[l].Armor[i].NetId.ShouldBe(a.Loadouts[l].Armor[i].NetId, $"Loadout[{l}].Armor[{i}].NetId");
        }

        // Creative sacrifices
        b.CreativeSacrifices.Count.ShouldBe(a.CreativeSacrifices.Count);

        // Voice
        b.VoiceVariant.ShouldBe(a.VoiceVariant);
        b.VoicePitchOffset.ShouldBe(a.VoicePitchOffset);

        // Dialogues
        b.OneTimeDialoguesSeen.Count.ShouldBe(a.OneTimeDialoguesSeen.Count);
    }
}
