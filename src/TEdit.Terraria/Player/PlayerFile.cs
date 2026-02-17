using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TEdit.Common;

namespace TEdit.Terraria.Player;

public static class PlayerFile
{
    private static readonly byte[] EncryptionKey = Encoding.Unicode.GetBytes("h3y_gUyZ");
    private const ulong ReLogicMagic = 27981915666277746UL;

    public static PlayerCharacter Load(string path)
    {
        byte[] fileBytes = File.ReadAllBytes(path);
        byte[] decrypted = Decrypt(fileBytes);
        using var ms = new MemoryStream(decrypted);
        return Load(ms);
    }

    public static PlayerCharacter Load(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        int version = reader.ReadInt32();

        uint revision = 0;
        bool isFavorite = false;
        if (version >= 135)
        {
            ReadFileMetadata(reader, out revision, out isFavorite);
        }

        var player = Deserialize(reader, version);
        player.Version = version;
        player.FileRevision = revision;
        player.IsFavorite = isFavorite;
        return player;
    }

    public static void Save(string path, PlayerCharacter player)
    {
        player.LastSaveTime = DateTime.UtcNow.ToBinary();
        using var ms = new MemoryStream();
        Save(ms, player);
        byte[] encrypted = Encrypt(ms.ToArray());
        File.WriteAllBytes(path, encrypted);
    }

    public static void Save(Stream stream, PlayerCharacter player)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(player.Version);
        if (player.Version >= 135)
            WriteFileMetadata(writer, player.FileRevision + 1, player.IsFavorite);
        Serialize(writer, player);
        writer.Flush();
    }

    internal static byte[] Decrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.Key = EncryptionKey;
        aes.IV = EncryptionKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;

        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using var result = new MemoryStream();
        cs.CopyTo(result);
        return result.ToArray();
    }

    internal static byte[] Encrypt(byte[] data)
    {
        // Pad to 16-byte boundary for AES
        int blockSize = 16;
        int paddedLength = (data.Length + blockSize - 1) / blockSize * blockSize;
        if (paddedLength > data.Length)
        {
            Array.Resize(ref data, paddedLength);
        }

        using var aes = Aes.Create();
        aes.Key = EncryptionKey;
        aes.IV = EncryptionKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
        }
        return ms.ToArray();
    }

    private static void ReadFileMetadata(BinaryReader reader, out uint revision, out bool isFavorite)
    {
        ulong header = reader.ReadUInt64();
        ulong magic = header & 0x00FFFFFFFFFFFFFFUL;
        if (magic != ReLogicMagic)
            throw new FormatException("Not a valid Re-Logic file.");

        byte type = (byte)((header >> 56) & 0xFF);
        if (type != (byte)FileType.Player)
            throw new FormatException($"Expected Player file type but found {type}.");

        revision = reader.ReadUInt32();
        ulong flags = reader.ReadUInt64();
        isFavorite = (flags & 1UL) == 1UL;
    }

    private static void WriteFileMetadata(BinaryWriter writer, uint revision, bool isFavorite)
    {
        ulong header = ReLogicMagic | ((ulong)FileType.Player << 56);
        writer.Write(header);
        writer.Write(revision);
        writer.Write((ulong)(isFavorite ? 1 : 0));
    }

    internal static void Serialize(BinaryWriter w, PlayerCharacter p)
    {
        int v = p.Version;
        var a = p.Appearance;

        // Core — always written
        w.Write(p.Name);
        if (v >= 17)
            w.Write(p.Difficulty);
        if (v >= 138)
            w.Write(p.PlayTimeTicks);
        w.Write(a.Hair);
        if (v >= 82)
            w.Write(a.HairDye);
        if (v >= 283)
            w.Write(p.Team);

        // HideVisibleAccessory
        if (v >= 124)
        {
            BitsByte bb1 = default;
            for (int i = 0; i < 8; i++)
                bb1[i] = p.HideVisibleAccessory[i];
            w.Write((byte)bb1);

            BitsByte bb2 = default;
            for (int i = 0; i < 2; i++)
                bb2[i] = p.HideVisibleAccessory[i + 8];
            w.Write((byte)bb2);
        }
        else if (v >= 83)
        {
            BitsByte bb = default;
            for (int i = 0; i < 8; i++)
                bb[i] = p.HideVisibleAccessory[i];
            w.Write((byte)bb);
        }

        if (v >= 119)
            w.Write(p.HideMisc);

        if (v >= 107)
            w.Write(a.SkinVariant);
        else if (v > 17)
            w.Write(a.Male);

        // Stats
        w.Write(p.StatLife);
        w.Write(p.StatLifeMax);
        w.Write(p.StatMana);
        w.Write(p.StatManaMax);
        if (v >= 125)
            w.Write(p.ExtraAccessory);

        // Flags
        if (v >= 229)
        {
            w.Write(p.UnlockedBiomeTorches);
            w.Write(p.UsingBiomeTorches);
            if (v >= 256)
                w.Write(p.AteArtisanBread);
            if (v >= 260)
            {
                w.Write(p.UsedAegisCrystal);
                w.Write(p.UsedAegisFruit);
                w.Write(p.UsedArcaneCrystal);
                w.Write(p.UsedGalaxyPearl);
                w.Write(p.UsedGummyWorm);
                w.Write(p.UsedAmbrosia);
            }
        }
        if (v >= 182)
            w.Write(p.DownedDD2EventAnyDifficulty);
        if (v >= 128)
            w.Write(p.TaxMoney);
        if (v >= 254)
        {
            w.Write(p.NumberOfDeathsPVE);
            w.Write(p.NumberOfDeathsPVP);
        }

        // Colors (RGB only — always written)
        WriteColor(w, a.HairColor);
        WriteColor(w, a.SkinColor);
        WriteColor(w, a.EyeColor);
        WriteColor(w, a.ShirtColor);
        WriteColor(w, a.UnderShirtColor);
        WriteColor(w, a.PantsColor);
        WriteColor(w, a.ShoeColor);

        // Equipment — v38+ uses int-based IDs
        if (v >= 38)
        {
            // Armor
            if (v >= 124)
            {
                for (int i = 0; i < PlayerConstants.MaxArmorSlots; i++)
                {
                    w.Write(p.Armor[i].NetId);
                    w.Write(p.Armor[i].Prefix);
                }
            }

            // Dye
            if (v >= 47)
            {
                int dyeCount = v >= 124 ? PlayerConstants.MaxDyeSlots : (v >= 81 ? 8 : 3);
                for (int i = 0; i < dyeCount; i++)
                {
                    w.Write(p.Dye[i].NetId);
                    w.Write(p.Dye[i].Prefix);
                }
            }

            // Inventory
            if (v >= 58)
            {
                for (int i = 0; i < PlayerConstants.MaxInventorySlots; i++)
                {
                    w.Write(p.Inventory[i].NetId);
                    w.Write(p.Inventory[i].StackSize);
                    w.Write(p.Inventory[i].Prefix);
                    if (v >= 114)
                        w.Write(p.Inventory[i].Favorited);
                }
            }

            // MiscEquips + MiscDyes
            if (v >= 117)
            {
                for (int i = 0; i < PlayerConstants.MaxMiscEquipSlots; i++)
                {
                    if (v < 136 && i == 1) continue;
                    w.Write(p.MiscEquips[i].NetId);
                    w.Write(p.MiscEquips[i].Prefix);
                    w.Write(p.MiscDyes[i].NetId);
                    w.Write(p.MiscDyes[i].Prefix);
                }
            }

            // Banks
            if (v >= 58)
            {
                WriteBankItems(w, p.Bank1);
                WriteBankItems(w, p.Bank2);
            }

            if (v >= 182)
                WriteBankItems(w, p.Bank3);

            if (v >= 198)
            {
                for (int i = 0; i < PlayerConstants.MaxBankSlots; i++)
                {
                    w.Write(p.Bank4[i].NetId);
                    w.Write(p.Bank4[i].StackSize);
                    w.Write(p.Bank4[i].Prefix);
                    if (v >= 255)
                        w.Write(p.Bank4[i].Favorited);
                }
            }

            if (v >= 199)
                w.Write(p.VoidVaultInfo);
        }

        // Buffs
        if (v >= 11)
        {
            int buffCount = v >= 252 ? PlayerConstants.MaxBuffSlots : (v >= 74 ? 22 : 10);
            for (int i = 0; i < buffCount; i++)
            {
                w.Write(p.Buffs[i].Type);
                w.Write(p.Buffs[i].Time);
            }
        }

        // Spawn points
        for (int i = 0; i < p.SpawnPoints.Count; i++)
        {
            w.Write(p.SpawnPoints[i].X);
            w.Write(p.SpawnPoints[i].Y);
            w.Write(p.SpawnPoints[i].WorldId);
            w.Write(p.SpawnPoints[i].WorldName);
        }
        w.Write(-1); // sentinel

        // Misc state
        if (v >= 16)
            w.Write(p.HbLocked);
        if (v >= 115)
        {
            for (int i = 0; i < PlayerConstants.MaxHideInfoSlots; i++)
                w.Write(p.HideInfo[i]);
        }
        if (v >= 98)
            w.Write(p.AnglerQuestsFinished);
        if (v >= 162)
        {
            for (int i = 0; i < PlayerConstants.MaxDpadBindings; i++)
                w.Write(p.DpadRadialBindings[i]);
        }
        if (v >= 164)
        {
            int count = v >= 230 ? 12 : (v >= 197 ? 11 : (v >= 167 ? 10 : 8));
            for (int i = 0; i < count; i++)
                w.Write(p.BuilderAccStatus[i]);
        }
        if (v >= 181)
            w.Write(p.BartenderQuestLog);
        if (v >= 200)
        {
            w.Write(p.Dead);
            if (p.Dead)
                w.Write(p.RespawnTimer);
        }
        if (v >= 202)
            w.Write(p.LastSaveTime);
        if (v >= 206)
            w.Write(p.GolferScoreAccumulated);

        // Creative sacrifices
        if (v >= 218)
            WriteCreativeSacrifices(w, p.CreativeSacrifices, v);

        // Temporary item slots
        if (v >= 214)
            WriteTemporaryItems(w, p.TemporaryItems);

        // Journey powers
        if (v >= 220)
            p.JourneyPowers.Save(w);

        // Super cart
        if (v >= 253)
        {
            BitsByte superCart = default;
            superCart[0] = p.UnlockedSuperCart;
            superCart[1] = p.EnabledSuperCart;
            w.Write((byte)superCart);
        }

        // Loadouts
        if (v >= 262)
        {
            w.Write(p.CurrentLoadoutIndex);
            for (int i = 0; i < PlayerConstants.MaxLoadouts; i++)
                SerializeLoadout(w, p.Loadouts[i]);
        }

        // Voice
        if (v >= 280)
            w.Write((byte)p.VoiceVariant);
        if (v >= 281)
            w.Write(p.VoicePitchOffset);

        // Crafting refunds
        if (v >= 300)
        {
            w.Write(p.CraftingRefundItems.Count);
            foreach (var item in p.CraftingRefundItems)
            {
                w.Write(item.NetId);
                w.Write(item.StackSize);
                w.Write(item.Prefix);
            }
        }

        // One-time dialogues
        if (v >= 310)
        {
            w.Write(p.OneTimeDialoguesSeen.Count);
            foreach (var dialogue in p.OneTimeDialoguesSeen)
                w.Write(dialogue);
        }
    }

    internal static PlayerCharacter Deserialize(BinaryReader r, int version)
    {
        var p = new PlayerCharacter();
        var a = p.Appearance;

        // Name
        p.Name = r.ReadString();

        // Difficulty
        if (version >= 10)
        {
            if (version >= 17)
                p.Difficulty = r.ReadByte();
            else if (r.ReadBoolean())
                p.Difficulty = 2; // hardcore
        }

        // Play time
        if (version >= 138)
            p.PlayTimeTicks = r.ReadInt64();

        // Hair
        a.Hair = r.ReadInt32();
        if (a.Hair >= PlayerConstants.MaxHairStyles)
            a.Hair = 0;

        if (version >= 82)
            a.HairDye = r.ReadByte();

        // Team (v283+)
        if (version >= 283)
            p.Team = r.ReadByte();

        // Hide visible accessory
        if (version >= 124)
        {
            BitsByte bb1 = r.ReadByte();
            for (int i = 0; i < 8; i++)
                p.HideVisibleAccessory[i] = bb1[i];
            BitsByte bb2 = r.ReadByte();
            for (int i = 0; i < 2; i++)
                p.HideVisibleAccessory[i + 8] = bb2[i];
        }
        else if (version >= 83)
        {
            BitsByte bb = r.ReadByte();
            for (int i = 0; i < 8; i++)
                p.HideVisibleAccessory[i] = bb[i];
        }

        if (version >= 119)
            p.HideMisc = r.ReadByte();

        // Skin variant / gender
        if (version <= 17)
        {
            // Derive from hair
            bool male = a.Hair != 5 && a.Hair != 6 && a.Hair != 9 && a.Hair != 11;
            a.SkinVariant = (byte)(male ? 0 : 4);
        }
        else if (version < 107)
        {
            bool male = r.ReadBoolean();
            a.SkinVariant = (byte)(male ? 0 : 4);
        }
        else
        {
            a.SkinVariant = r.ReadByte();
        }

        if (version < 161 && a.SkinVariant == 7)
            a.SkinVariant = 9;

        // Stats
        p.StatLife = r.ReadInt32();
        p.StatLifeMax = r.ReadInt32();
        if (p.StatLifeMax > 500) p.StatLifeMax = 500;
        p.StatMana = r.ReadInt32();
        p.StatManaMax = r.ReadInt32();
        if (p.StatManaMax > 200) p.StatManaMax = 200;
        if (p.StatMana > 400) p.StatMana = 400;

        if (version >= 125)
            p.ExtraAccessory = r.ReadBoolean();

        // Biome torches and consumable flags
        if (version >= 229)
        {
            p.UnlockedBiomeTorches = r.ReadBoolean();
            p.UsingBiomeTorches = r.ReadBoolean();
            if (version >= 256)
                p.AteArtisanBread = r.ReadBoolean();
            if (version >= 260)
            {
                p.UsedAegisCrystal = r.ReadBoolean();
                p.UsedAegisFruit = r.ReadBoolean();
                p.UsedArcaneCrystal = r.ReadBoolean();
                p.UsedGalaxyPearl = r.ReadBoolean();
                p.UsedGummyWorm = r.ReadBoolean();
                p.UsedAmbrosia = r.ReadBoolean();
            }
        }

        if (version >= 182)
            p.DownedDD2EventAnyDifficulty = r.ReadBoolean();

        if (version >= 128)
            p.TaxMoney = r.ReadInt32();

        if (version >= 254)
        {
            p.NumberOfDeathsPVE = r.ReadInt32();
            p.NumberOfDeathsPVP = r.ReadInt32();
        }

        // Colors
        a.HairColor = ReadColor(r);
        a.SkinColor = ReadColor(r);
        a.EyeColor = ReadColor(r);
        a.ShirtColor = ReadColor(r);
        a.UnderShirtColor = ReadColor(r);
        a.PantsColor = ReadColor(r);
        a.ShoeColor = ReadColor(r);

        // Equipment
        if (version >= 38)
        {
            ReadArmorAndDye(r, p, version);
            ReadInventory(r, p, version);
            ReadMiscEquips(r, p, version);
            ReadBanks(r, p, version);
        }
        else
        {
            ReadLegacyEquipment(r, p, version);
        }

        // Fix old inventory layout
        if (version < 58)
        {
            for (int i = 40; i < 48; i++)
            {
                p.Inventory[i + 10] = p.Inventory[i].Copy();
                p.Inventory[i] = new PlayerItem();
            }
        }

        // Buffs
        if (version >= 11)
        {
            int buffCount = 22;
            if (version < 74) buffCount = 10;
            if (version >= 252) buffCount = PlayerConstants.MaxBuffSlots;

            int slot = 0;
            for (int i = 0; i < buffCount; i++)
            {
                int type = r.ReadInt32();
                int time = r.ReadInt32();
                if (type != 0 && slot < PlayerConstants.MaxBuffSlots)
                {
                    p.Buffs[slot].Type = type;
                    p.Buffs[slot].Time = time;
                    slot++;
                }
            }
        }

        // Spawn points
        for (int i = 0; i < PlayerConstants.MaxSpawnPoints; i++)
        {
            int spX = r.ReadInt32();
            if (spX == -1) break;
            int spY = r.ReadInt32();
            int spI = r.ReadInt32();
            string spN = r.ReadString();
            p.SpawnPoints.Add(new SpawnPoint(spX, spY, spI, spN));
        }

        // Misc state
        if (version >= 16)
            p.HbLocked = r.ReadBoolean();

        if (version >= 115)
        {
            for (int i = 0; i < PlayerConstants.MaxHideInfoSlots; i++)
                p.HideInfo[i] = r.ReadBoolean();
        }

        if (version >= 98)
            p.AnglerQuestsFinished = r.ReadInt32();

        if (version >= 162)
        {
            for (int i = 0; i < PlayerConstants.MaxDpadBindings; i++)
                p.DpadRadialBindings[i] = r.ReadInt32();
        }

        if (version >= 164)
        {
            int count = 8;
            if (version >= 167) count = 10;
            if (version >= 197) count = 11;
            if (version >= 230) count = PlayerConstants.MaxBuilderAccStatus;
            for (int i = 0; i < count; i++)
                p.BuilderAccStatus[i] = r.ReadInt32();
            if (version < 210)
                p.BuilderAccStatus[0] = 1;
        }

        if (version >= 181)
            p.BartenderQuestLog = r.ReadInt32();

        if (version >= 200)
        {
            p.Dead = r.ReadBoolean();
            if (p.Dead)
                p.RespawnTimer = Math.Clamp(r.ReadInt32(), 0, 60000);
        }

        p.LastSaveTime = 0;
        if (version >= 202)
            p.LastSaveTime = r.ReadInt64();

        if (version >= 206)
            p.GolferScoreAccumulated = r.ReadInt32();

        // Creative sacrifices
        if (version >= 218)
            ReadCreativeSacrifices(r, p, version);

        // Temporary item slots
        if (version >= 214)
            ReadTemporaryItems(r, p);

        // Journey powers
        if (version >= 220)
            p.JourneyPowers.Load(r, (uint)version);

        // Super cart
        if (version >= 253)
        {
            BitsByte sc = r.ReadByte();
            p.UnlockedSuperCart = sc[0];
            p.EnabledSuperCart = sc[1];
        }

        // Loadouts
        if (version >= 262)
        {
            p.CurrentLoadoutIndex = Math.Clamp(r.ReadInt32(), 0, PlayerConstants.MaxLoadouts - 1);
            for (int i = 0; i < PlayerConstants.MaxLoadouts; i++)
                DeserializeLoadout(r, p.Loadouts[i]);
        }

        // Voice
        if (version >= 280)
        {
            p.VoiceVariant = r.ReadByte();
        }
        else
        {
            p.VoiceVariant = a.Male ? 1 : 2;
        }

        if (version >= 281)
            p.VoicePitchOffset = r.ReadSingle();

        // Crafting refunds
        if (version >= 300)
        {
            int refundCount = r.ReadInt32();
            for (int i = 0; i < refundCount; i++)
            {
                int type = r.ReadInt32();
                int stack = r.ReadInt32();
                byte prefix = r.ReadByte();
                p.CraftingRefundItems.Add(new PlayerItem(type, stack, prefix));
            }
        }

        // One-time dialogues
        if (version >= 310)
        {
            int dialogueCount = r.ReadInt32();
            for (int i = 0; i < dialogueCount; i++)
                p.OneTimeDialoguesSeen.Add(r.ReadString());
        }

        return p;
    }

    private static void ReadArmorAndDye(BinaryReader r, PlayerCharacter p, int version)
    {
        if (version < 124)
        {
            int count = version >= 81 ? 16 : 11;
            for (int i = 0; i < count; i++)
            {
                int idx = i >= 8 ? i + 2 : i;
                int type = r.ReadInt32();
                byte prefix = r.ReadByte();
                if (idx < PlayerConstants.MaxArmorSlots)
                    p.Armor[idx] = new PlayerItem(type, type != 0 ? 1 : 0, prefix);
            }
        }
        else
        {
            for (int i = 0; i < PlayerConstants.MaxArmorSlots; i++)
            {
                int type = r.ReadInt32();
                byte prefix = r.ReadByte();
                p.Armor[i] = new PlayerItem(type, type != 0 ? 1 : 0, prefix);
            }
        }

        if (version >= 47)
        {
            int dyeCount = 3;
            if (version >= 81) dyeCount = 8;
            if (version >= 124) dyeCount = PlayerConstants.MaxDyeSlots;
            for (int i = 0; i < dyeCount; i++)
            {
                int type = r.ReadInt32();
                byte prefix = r.ReadByte();
                p.Dye[i] = new PlayerItem(type, type != 0 ? 1 : 0, prefix);
            }
        }
    }

    private static void ReadInventory(BinaryReader r, PlayerCharacter p, int version)
    {
        if (version >= 58)
        {
            for (int i = 0; i < PlayerConstants.MaxInventorySlots; i++)
            {
                int type = r.ReadInt32();
                int stack = r.ReadInt32();
                byte prefix = r.ReadByte();
                bool favorited = version >= 114 && r.ReadBoolean();
                p.Inventory[i] = new PlayerItem(type, stack, prefix, favorited);
            }
        }
        else
        {
            int count = version >= 15 ? 48 : 44;
            for (int i = 0; i < count; i++)
            {
                int type = r.ReadInt32();
                int stack = r.ReadInt32();
                byte prefix = r.ReadByte();
                p.Inventory[i] = new PlayerItem(type, stack, prefix);
            }
        }
    }

    private static void ReadMiscEquips(BinaryReader r, PlayerCharacter p, int version)
    {
        if (version < 117) return;

        for (int i = 0; i < PlayerConstants.MaxMiscEquipSlots; i++)
        {
            if (version < 136 && i == 1) continue;

            int eType = r.ReadInt32();
            byte ePrefix = r.ReadByte();
            p.MiscEquips[i] = new PlayerItem(eType, eType != 0 ? 1 : 0, ePrefix);

            int dType = r.ReadInt32();
            byte dPrefix = r.ReadByte();
            p.MiscDyes[i] = new PlayerItem(dType, dType != 0 ? 1 : 0, dPrefix);
        }
    }

    private static void ReadBanks(BinaryReader r, PlayerCharacter p, int version)
    {
        int bankSize = version >= 58 ? PlayerConstants.MaxBankSlots : 20;

        for (int i = 0; i < bankSize; i++)
            p.Bank1[i] = ReadBankItem(r);

        for (int i = 0; i < bankSize; i++)
            p.Bank2[i] = ReadBankItem(r);

        if (version >= 182)
        {
            for (int i = 0; i < PlayerConstants.MaxBankSlots; i++)
                p.Bank3[i] = ReadBankItem(r);
        }

        if (version >= 198)
        {
            for (int i = 0; i < PlayerConstants.MaxBankSlots; i++)
            {
                int type = r.ReadInt32();
                int stack = r.ReadInt32();
                byte prefix = r.ReadByte();
                bool favorited = version >= 255 && r.ReadBoolean();
                p.Bank4[i] = new PlayerItem(type, stack, prefix, favorited);
            }
        }

        if (version >= 199)
            p.VoidVaultInfo = r.ReadByte();
    }

    private static void ReadLegacyEquipment(BinaryReader r, PlayerCharacter p, int version)
    {
        // Pre-v38: string-based item names — we read them but can't resolve IDs
        // This is extremely old format; read the data to not corrupt the stream
        for (int i = 0; i < 8; i++)
        {
            string name = r.ReadString();
            byte prefix = version >= 36 ? r.ReadByte() : (byte)0;
            // Can't resolve legacy names without ItemID.FromLegacyName
        }

        if (version >= 6)
        {
            for (int i = 10; i < 13; i++)
            {
                string name = r.ReadString();
                byte prefix = version >= 36 ? r.ReadByte() : (byte)0;
            }
        }

        int invCount = version >= 15 ? 48 : 44;
        for (int i = 0; i < invCount; i++)
        {
            string name = r.ReadString();
            int stack = r.ReadInt32();
            byte prefix = version >= 36 ? r.ReadByte() : (byte)0;
        }

        for (int i = 0; i < 20; i++)
        {
            string name = r.ReadString();
            int stack = r.ReadInt32();
            byte prefix = version >= 36 ? r.ReadByte() : (byte)0;
        }

        if (version >= 20)
        {
            for (int i = 0; i < 20; i++)
            {
                string name = r.ReadString();
                int stack = r.ReadInt32();
                byte prefix = version >= 36 ? r.ReadByte() : (byte)0;
            }
        }
    }

    private static void ReadCreativeSacrifices(BinaryReader r, PlayerCharacter p, int version)
    {
        // v282+ has a leading bool (was used for a different format flag)
        if (version >= 282)
            r.ReadBoolean();

        int count = r.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            string key = r.ReadString();
            int value = r.ReadInt32();
            p.CreativeSacrifices[key] = value;
        }
    }

    private static void ReadTemporaryItems(BinaryReader r, PlayerCharacter p)
    {
        BitsByte bb = r.ReadByte();
        if (bb[0])
            p.TemporaryItems.MouseItem = ReadItemSavingAndLoading(r);
        if (bb[1])
            p.TemporaryItems.CreativeMenuItem = ReadItemSavingAndLoading(r);
        if (bb[2])
            p.TemporaryItems.GuideItem = ReadItemSavingAndLoading(r);
        if (bb[3])
            p.TemporaryItems.ReforgeItem = ReadItemSavingAndLoading(r);
    }

    private static void WriteCreativeSacrifices(BinaryWriter w, Dictionary<string, int> sacrifices, int version)
    {
        if (version >= 282)
            w.Write(false);
        w.Write(sacrifices.Count);
        foreach (var kvp in sacrifices)
        {
            w.Write(kvp.Key);
            w.Write(kvp.Value);
        }
    }

    private static void WriteTemporaryItems(BinaryWriter w, TemporaryItemSlots slots)
    {
        BitsByte bb = default;
        bb[0] = slots.MouseItem != null && slots.MouseItem.NetId != 0;
        bb[1] = slots.CreativeMenuItem != null && slots.CreativeMenuItem.NetId != 0;
        bb[2] = slots.GuideItem != null && slots.GuideItem.NetId != 0;
        bb[3] = slots.ReforgeItem != null && slots.ReforgeItem.NetId != 0;
        w.Write((byte)bb);

        if (bb[0]) WriteItemSavingAndLoading(w, slots.MouseItem!);
        if (bb[1]) WriteItemSavingAndLoading(w, slots.CreativeMenuItem!);
        if (bb[2]) WriteItemSavingAndLoading(w, slots.GuideItem!);
        if (bb[3]) WriteItemSavingAndLoading(w, slots.ReforgeItem!);
    }

    private static PlayerItem ReadItemSavingAndLoading(BinaryReader r)
    {
        int type = r.ReadInt32();
        int stack = r.ReadInt32();
        byte prefix = r.ReadByte();
        return new PlayerItem(type, stack, prefix);
    }

    private static void WriteItemSavingAndLoading(BinaryWriter w, PlayerItem item)
    {
        w.Write(item.NetId);
        w.Write(item.StackSize);
        w.Write(item.Prefix);
    }

    private static PlayerItem ReadBankItem(BinaryReader r)
    {
        int type = r.ReadInt32();
        int stack = r.ReadInt32();
        byte prefix = r.ReadByte();
        return new PlayerItem(type, stack, prefix);
    }

    private static void WriteBankItems(BinaryWriter w, System.Collections.ObjectModel.ObservableCollection<PlayerItem> items)
    {
        for (int i = 0; i < PlayerConstants.MaxBankSlots; i++)
        {
            w.Write(items[i].NetId);
            w.Write(items[i].StackSize);
            w.Write(items[i].Prefix);
        }
    }

    private static void SerializeLoadout(BinaryWriter w, EquipmentLoadout loadout)
    {
        for (int i = 0; i < PlayerConstants.MaxArmorSlots; i++)
        {
            w.Write(loadout.Armor[i].NetId);
            w.Write(loadout.Armor[i].StackSize);
            w.Write(loadout.Armor[i].Prefix);
        }
        for (int i = 0; i < PlayerConstants.MaxDyeSlots; i++)
        {
            w.Write(loadout.Dye[i].NetId);
            w.Write(loadout.Dye[i].StackSize);
            w.Write(loadout.Dye[i].Prefix);
        }
        for (int i = 0; i < PlayerConstants.MaxDyeSlots; i++)
            w.Write(loadout.Hide[i]);
    }

    private static void DeserializeLoadout(BinaryReader r, EquipmentLoadout loadout)
    {
        for (int i = 0; i < PlayerConstants.MaxArmorSlots; i++)
        {
            int type = r.ReadInt32();
            int stack = r.ReadInt32();
            byte prefix = r.ReadByte();
            loadout.Armor[i] = new PlayerItem(type, stack, prefix);
        }
        for (int i = 0; i < PlayerConstants.MaxDyeSlots; i++)
        {
            int type = r.ReadInt32();
            int stack = r.ReadInt32();
            byte prefix = r.ReadByte();
            loadout.Dye[i] = new PlayerItem(type, stack, prefix);
        }
        for (int i = 0; i < PlayerConstants.MaxDyeSlots; i++)
            loadout.Hide[i] = r.ReadBoolean();
    }

    private static TEditColor ReadColor(BinaryReader r)
    {
        byte red = r.ReadByte();
        byte green = r.ReadByte();
        byte blue = r.ReadByte();
        return new TEditColor(red, green, blue);
    }

    private static void WriteColor(BinaryWriter w, TEditColor color)
    {
        w.Write(color.R);
        w.Write(color.G);
        w.Write(color.B);
    }
}
