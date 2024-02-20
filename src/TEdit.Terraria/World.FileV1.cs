using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Helper;
using TEdit.Terraria.Objects;
using TEdit.Utility;
using Vector2 = TEdit.Geometry.Vector2Float;

namespace TEdit.Terraria;

public partial class World
{
    public static Dictionary<string, short> _legacyItemLookup { get; private set; }

    public static void SaveV1(World world, BinaryWriter bw, bool ForceLighting, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        var version = world.Version;

        bw.Write(world.Version);
        bw.Write(world.Title);
        bw.Write(world.WorldId);
        bw.Write((int)world.LeftWorld);
        bw.Write((int)world.RightWorld);
        bw.Write((int)world.TopWorld);
        bw.Write((int)world.BottomWorld);
        bw.Write(world.TilesHigh);
        bw.Write(world.TilesWide);

        if (version >= 63)
        {
            bw.Write((byte)world.MoonType);
        }

        if (version >= 44)
        {
            bw.Write(world.TreeX0);
            bw.Write(world.TreeX1);
            bw.Write(world.TreeX2);
            bw.Write(world.TreeStyle0);
            bw.Write(world.TreeStyle1);
            bw.Write(world.TreeStyle2);
            bw.Write(world.TreeStyle3);
        }

        if (version >= 60)
        {
            bw.Write(world.CaveBackX0);
            bw.Write(world.CaveBackX1);
            bw.Write(world.CaveBackX2);
            bw.Write(world.CaveBackStyle0);
            bw.Write(world.CaveBackStyle1);
            bw.Write(world.CaveBackStyle2);
            bw.Write(world.CaveBackStyle3);
            bw.Write(world.IceBackStyle);

            if (version >= 61)
            {
                bw.Write(world.JungleBackStyle);
                bw.Write(world.HellBackStyle);
            }
        }

        bw.Write(world.SpawnX);
        bw.Write(world.SpawnY);
        bw.Write(world.GroundLevel);
        bw.Write(world.RockLevel);

        bw.Write(world.Time);
        bw.Write(world.DayTime);
        bw.Write(world.MoonPhase);
        bw.Write(world.BloodMoon);

        if (version >= 70)
        {
            bw.Write(world.IsEclipse);
        }

        bw.Write(world.DungeonX);
        bw.Write(world.DungeonY);

        if (version >= 56)
        {
            bw.Write(world.IsCrimson);
        }

        bw.Write(world.DownedBoss1EyeofCthulhu);
        bw.Write(world.DownedBoss2EaterofWorlds);
        bw.Write(world.DownedBoss3Skeletron);

        if (version >= 66)
        {
            bw.Write(world.DownedQueenBee);
        }

        if (version >= 44)
        {
            bw.Write(world.DownedMechBoss1TheDestroyer);
            bw.Write(world.DownedMechBoss2TheTwins);
            bw.Write(world.DownedMechBoss3SkeletronPrime);
            bw.Write(world.DownedMechBossAny);
        }

        if (version >= 64)
        {
            bw.Write(world.DownedPlantBoss);
            bw.Write(world.DownedGolemBoss);
        }

        if (version >= 29)
        {
            bw.Write(world.SavedGoblin);
            bw.Write(world.SavedWizard);

            if (version >= 34)
            {
                bw.Write(world.SavedMech);
            }
            bw.Write(world.DownedGoblins);
        }

        if (version >= 32)
        {
            bw.Write(world.DownedClown);
        }

        if (version >= 37)
        {
            bw.Write(world.DownedFrost);
        }

        if (version >= 56)
        {
            bw.Write(world.DownedPirates);
        }

        bw.Write(world.ShadowOrbSmashed);
        bw.Write(world.SpawnMeteor);
        bw.Write((byte)world.ShadowOrbCount);

        if (version >= 23)
        {
            bw.Write(world.AltarCount);
            bw.Write(world.HardMode);
        }

        bw.Write(world.InvasionDelay);
        bw.Write(world.InvasionSize);
        bw.Write(world.InvasionType);
        bw.Write(world.InvasionX);

        if (version >= 53)
        {
            bw.Write(world.IsRaining);
            bw.Write(world.TempRainTime);
            bw.Write(world.TempMaxRain);
        }

        if (version >= 54)
        {
            bw.Write(world.SavedOreTiersCobalt);
            bw.Write(world.SavedOreTiersMythril);
            bw.Write(world.SavedOreTiersAdamantite);
        }

        if (version >= 55)
        {
            bw.Write(world.BgTree);
            bw.Write(world.BgCorruption);
            bw.Write(world.BgJungle);
        }
        if (version >= 60)
        {
            bw.Write(world.BgSnow);
            bw.Write(world.BgHallow);
            bw.Write(world.BgCrimson);
            bw.Write(world.BgDesert);
            bw.Write(world.BgOcean);
        }

        if (version >= 60)
        {
            bw.Write((int)world.CloudBgActive);
        }

        if (version >= 62)
        {
            bw.Write(world.NumClouds);
            bw.Write(world.WindSpeedSet);
        }

        var saveData = WorldConfiguration.SaveConfiguration.GetData((int)version);
        var frames = WorldConfiguration.SaveConfiguration.GetTileFramesForVersion((int)version);

        for (int x = 0; x < world.TilesWide; ++x)
        {
            progress?.Report(
                 new ProgressChangedEventArgs(x.ProgressPercentage(world.TilesWide), "Saving Tiles..."));

            int rle = 0;
            for (int y = 0; y < world.TilesHigh; y = y + rle + 1)
            {
                Tile curTile = world.Tiles[x, y];

                WriteTileDataToStreamV1(curTile, bw, world.Version, frames, saveData.MaxTileId, saveData.MaxWallId, ForceLighting);

                if (version >= 25)
                {
                    int rleTemp = 1;
                    while (y + rleTemp < world.TilesHigh && curTile.Equals(world.Tiles[x, (y + rleTemp)]))
                        ++rleTemp;
                    rle = rleTemp - 1;
                    bw.Write((short)rle);
                }
            }
        }

        int chestSize = (world.Version < 48) ? 20 : 40;

        progress?.Report(new ProgressChangedEventArgs(100, "Saving Chests..."));
        WriteChestDataToStreamV1(world.Chests, bw, world.Version);
        progress?.Report(new ProgressChangedEventArgs(100, "Saving Signs..."));
        WriteSignDataToStreamV1(world.Signs, bw, world.Version);
        progress?.Report(new ProgressChangedEventArgs(100, "Saving NPC Data..."));

        foreach (NPC curNpc in world.NPCs.Where(n => n.SpriteId <= saveData.MaxNpcId))
        {
            bw.Write(true);
            bw.Write(curNpc.Name);
            bw.Write(curNpc.Position.X);
            bw.Write(curNpc.Position.Y);
            bw.Write(curNpc.IsHomeless);
            bw.Write(curNpc.Home.X);
            bw.Write(curNpc.Home.Y);
        }

        bw.Write(false);

        progress?.Report(new ProgressChangedEventArgs(100, "Saving NPC Names..."));

        if (version >= 31)
        {
            bw.Write(world.GetNpc(17).Name);
            bw.Write(world.GetNpc(18).Name);
            bw.Write(world.GetNpc(19).Name);
            bw.Write(world.GetNpc(20).Name);
            bw.Write(world.GetNpc(22).Name);
            bw.Write(world.GetNpc(54).Name);
            bw.Write(world.GetNpc(38).Name);
            bw.Write(world.GetNpc(107).Name);
            bw.Write(world.GetNpc(108).Name);

            if (version >= 35)
            {
                bw.Write(world.GetNpc(124).Name);

                if (version >= 65)
                {
                    bw.Write(world.GetNpc(160).Name);
                    bw.Write(world.GetNpc(178).Name);
                    bw.Write(world.GetNpc(207).Name);
                    bw.Write(world.GetNpc(208).Name);
                    bw.Write(world.GetNpc(209).Name);
                    bw.Write(world.GetNpc(227).Name);
                    bw.Write(world.GetNpc(228).Name);
                    bw.Write(world.GetNpc(229).Name);
                }

                if (version >= 79)
                {
                    bw.Write(world.GetNpc(353).Name);
                }
            }
        }

        if (version >= 7)
        {
            progress?.Report(new ProgressChangedEventArgs(100, "Saving Validation Data..."));
            bw.Write(true);
            bw.Write(world.Title);
            bw.Write(world.WorldId);
        }
    }

    public static void WriteSignDataToStreamV1(IList<Sign> signs, BinaryWriter bw, uint version)
    {
        for (int i = 0; i < 1000; ++i)
        {
            if (i >= signs.Count || string.IsNullOrWhiteSpace(signs[i].Text))
            {
                bw.Write(false);
            }
            else
            {
                Sign curSign = signs[i];
                bw.Write(true);
                bw.Write(curSign.Text);
                bw.Write(curSign.X);
                bw.Write(curSign.Y);
            }
        }
    }

    public static void WriteChestDataToStreamV1(IList<Chest> chests, BinaryWriter bw, uint version)
    {
        int chestSize = (version < 58) ? 20 : 40;

        var maxItemId = WorldConfiguration.SaveConfiguration.SaveVersions[(int)version].MaxItemId;

        for (int i = 0; i < 1000; ++i)
        {
            if (i >= chests.Count)
            {
                bw.Write(false);
                continue;
            }

            Chest chest = chests[i];
            bw.Write(true);
            bw.Write(chest.X);
            bw.Write(chest.Y);
            if (version >= 85)
            {
                var chestName = chest.Name;
                if (chestName.Length > 20)
                {
                    chestName = chestName.Substring(0, 20);
                }
                bw.Write(chestName);
            }


            for (int slot = 0; slot < chestSize; slot++)
            {
                Item item = chest.Items[slot];

                if (item.NetId == 0)
                    item.StackSize = 0;

                if (item != null && item.NetId <= maxItemId)
                {
                    if (version < 59)
                    {
                        bw.Write((byte)Math.Min(byte.MaxValue, item.StackSize));
                    }
                    else
                    {
                        bw.Write((short)item.StackSize);
                    }

                    if (item.StackSize > 0)
                    {
                        if (version >= 38)
                        {
                            bw.Write(item.NetId);
                        }
                        else
                        {
                            bw.Write(ToLegacyName((short)item.NetId, version));
                        }

                        if (version >= 36)
                        {
                            bw.Write(item.Prefix);
                        }
                    }
                }
                else
                {
                    if (version < 59)
                    {
                        bw.Write((byte)0);
                    }
                    else
                    {
                        bw.Write((short)0);
                    }
                }
            }
        }
    }

    public static String ToLegacyName(short netId, uint release)
    {
        if (_legacyItemLookup == null)
            _legacyItemLookup = GenerateLegacyItemDictionary();

        var reverseLookup = _legacyItemLookup.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        if (!reverseLookup.TryGetValue(netId, out string name))
            return "Torch";

        if (release <= 4)
        {
            switch (name)
            {
                case "Jungle Hat":
                    name = "Cobalt Helmet";
                    break;
                case "Jungle Shirt":
                    name = "Cobalt Breastplate";
                    break;
                case "Jungle Pants":
                    name = "Cobalt Greaves";
                    break;
            }
        }

        if (release <= 13 && name == "Jungle Spores")
            name = "Jungle Rose";

        if (release <= 20)
        {
            switch (name)
            {
                case "Gills Potion":
                    name = "Gills potion";
                    break;
                case "Thorn Chakram":
                    name = "Thorn Chakrum";
                    break;
                case "Ball O' Hurt":
                    name = "Ball 'O Hurt";
                    break;
            }
        }

        if (release <= 38 && name == "Shiny Red Balloon")
        {
            name = "Shiney Red Ballon";
        }

        if (release <= 41 && name == "Chain")
            name = "Iron Chain";

        if (release <= 44 && name == "Shadow Orb")
            name = "Orb of Light";

        if (release <= 46)
        {
            if (name == "Black Thread")
                name = "Black Dye";

            if (name == "Green Thread")
                name = "Green Dye";
        }

        return name;
    }

    public static short FromLegacyName(string name, int release)
    {
        if (_legacyItemLookup == null)
            _legacyItemLookup = GenerateLegacyItemDictionary();

        if (release <= 4)
        {
            switch (name)
            {
                case "Cobalt Helmet":
                    name = "Jungle Hat";
                    break;
                case "Cobalt Breastplate":
                    name = "Jungle Shirt";
                    break;
                case "Cobalt Greaves":
                    name = "Jungle Pants";
                    break;
            }
        }

        if (release <= 13 && name == "Jungle Rose")
            name = "Jungle Spores";

        if (release <= 20)
        {
            switch (name)
            {
                case "Gills potion":
                    name = "Gills Potion";
                    break;
                case "Thorn Chakrum":
                    name = "Thorn Chakram";
                    break;
                case "Ball 'O Hurt":
                    name = "Ball O' Hurt";
                    break;
            }
        }

        if (release <= 38 && name == "Shiney Red Ballon")
        {
            name = "Shiny Red Balloon";
        }

        if (release <= 41 && name == "Iron Chain")
            name = "Chain";

        if (release <= 44 && name == "Orb of Light")
            name = "Shadow Orb";

        if (release <= 46)
        {
            if (name == "Black Dye")
                name = "Black Thread";

            if (name == "Green Dye")
                name = "Green Thread";
        }

        if (_legacyItemLookup.TryGetValue(name, out short value))
            return value;

        return 0;
    }

    private static bool[] GetFramesV0()
    {
        var frames = new bool[76];
        foreach (var frame in new[] { 3, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 20, 21, 24, 26, 27, 28, 29, 31, 33, 34, 35, 36, 42, 50, 55, 61, 71, 72, 73, 74 })
        {
            frames[frame] = true;
        }
        return frames;
    }

    private static Dictionary<string, short> GenerateLegacyItemDictionary()
    {
        return new Dictionary<string, short> {
            { "Iron Pickaxe", 1 },
            { "Dirt Block", 2 },
            { "Stone Block", 3 },
            { "Iron Broadsword", 4 },
            { "Mushroom", 5 },
            { "Iron Shortsword", 6 },
            { "Iron Hammer", 7 },
            { "Torch", 8 },
            { "Wood", 9 },
            { "Iron Axe", 10 },
            { "Iron Ore", 11 },
            { "Copper Ore", 12 },
            { "Gold Ore", 13 },
            { "Silver Ore", 14 },
            { "Copper Watch", 15 },
            { "Silver Watch", 16 },
            { "Gold Watch", 17 },
            { "Depth Meter", 18 },
            { "Gold Bar", 19 },
            { "Copper Bar", 20 },
            { "Silver Bar", 21 },
            { "Iron Bar", 22 },
            { "Gel", 23 },
            { "Wooden Sword", 24 },
            { "Wooden Door", 25 },
            { "Stone Wall", 26 },
            { "Acorn", 27 },
            { "Lesser Healing Potion", 28 },
            { "Life Crystal", 29 },
            { "Dirt Wall", 30 },
            { "Bottle", 31 },
            { "Wooden Table", 32 },
            { "Furnace", 33 },
            { "Wooden Chair", 34 },
            { "Iron Anvil", 35 },
            { "Work Bench", 36 },
            { "Goggles", 37 },
            { "Lens", 38 },
            { "Wooden Bow", 39 },
            { "Wooden Arrow", 40 },
            { "Flaming Arrow", 41 },
            { "Shuriken", 42 },
            { "Suspicious Looking Eye", 43 },
            { "Demon Bow", 44 },
            { "War Axe of the Night", 45 },
            { "Light's Bane", 46 },
            { "Unholy Arrow", 47 },
            { "Chest", 48 },
            { "Band of Regeneration", 49 },
            { "Magic Mirror", 50 },
            { "Jester's Arrow", 51 },
            { "Angel Statue", 52 },
            { "Cloud in a Bottle", 53 },
            { "Hermes Boots", 54 },
            { "Enchanted Boomerang", 55 },
            { "Demonite Ore", 56 },
            { "Demonite Bar", 57 },
            { "Heart", 58 },
            { "Corrupt Seeds", 59 },
            { "Vile Mushroom", 60 },
            { "Ebonstone Block", 61 },
            { "Grass Seeds", 62 },
            { "Sunflower", 63 },
            { "Vilethorn", 64 },
            { "Starfury", 65 },
            { "Purification Powder", 66 },
            { "Vile Powder", 67 },
            { "Rotten Chunk", 68 },
            { "Worm Tooth", 69 },
            { "Worm Food", 70 },
            { "Copper Coin", 71 },
            { "Silver Coin", 72 },
            { "Gold Coin", 73 },
            { "Platinum Coin", 74 },
            { "Fallen Star", 75 },
            { "Copper Greaves", 76 },
            { "Iron Greaves", 77 },
            { "Silver Greaves", 78 },
            { "Gold Greaves", 79 },
            { "Copper Chainmail", 80 },
            { "Iron Chainmail", 81 },
            { "Silver Chainmail", 82 },
            { "Gold Chainmail", 83 },
            { "Grappling Hook", 84 },
            { "Chain", 85 },
            { "Shadow Scale", 86 },
            { "Piggy Bank", 87 },
            { "Mining Helmet", 88 },
            { "Copper Helmet", 89 },
            { "Iron Helmet", 90 },
            { "Silver Helmet", 91 },
            { "Gold Helmet", 92 },
            { "Wood Wall", 93 },
            { "Wood Platform", 94 },
            { "Flintlock Pistol", 95 },
            { "Musket", 96 },
            { "Musket Ball", 97 },
            { "Minishark", 98 },
            { "Iron Bow", 99 },
            { "Shadow Greaves", 100 },
            { "Shadow Scalemail", 101 },
            { "Shadow Helmet", 102 },
            { "Nightmare Pickaxe", 103 },
            { "The Breaker", 104 },
            { "Candle", 105 },
            { "Copper Chandelier", 106 },
            { "Silver Chandelier", 107 },
            { "Gold Chandelier", 108 },
            { "Mana Crystal", 109 },
            { "Lesser Mana Potion", 110 },
            { "Band of Starpower", 111 },
            { "Flower of Fire", 112 },
            { "Magic Missile", 113 },
            { "Dirt Rod", 114 },
            { "Shadow Orb", 115 },
            { "Meteorite", 116 },
            { "Meteorite Bar", 117 },
            { "Hook", 118 },
            { "Flamarang", 119 },
            { "Molten Fury", 120 },
            { "Fiery Greatsword", 121 },
            { "Molten Pickaxe", 122 },
            { "Meteor Helmet", 123 },
            { "Meteor Suit", 124 },
            { "Meteor Leggings", 125 },
            { "Bottled Water", 126 },
            { "Space Gun", 127 },
            { "Rocket Boots", 128 },
            { "Gray Brick", 129 },
            { "Gray Brick Wall", 130 },
            { "Red Brick", 131 },
            { "Red Brick Wall", 132 },
            { "Clay Block", 133 },
            { "Blue Brick", 134 },
            { "Blue Brick Wall", 135 },
            { "Chain Lantern", 136 },
            { "Green Brick", 137 },
            { "Green Brick Wall", 138 },
            { "Pink Brick", 139 },
            { "Pink Brick Wall", 140 },
            { "Gold Brick", 141 },
            { "Gold Brick Wall", 142 },
            { "Silver Brick", 143 },
            { "Silver Brick Wall", 144 },
            { "Copper Brick", 145 },
            { "Copper Brick Wall", 146 },
            { "Spike", 147 },
            { "Water Candle", 148 },
            { "Book", 149 },
            { "Cobweb", 150 },
            { "Necro Helmet", 151 },
            { "Necro Breastplate", 152 },
            { "Necro Greaves", 153 },
            { "Bone", 154 },
            { "Muramasa", 155 },
            { "Cobalt Shield", 156 },
            { "Aqua Scepter", 157 },
            { "Lucky Horseshoe", 158 },
            { "Shiny Red Balloon", 159 },
            { "Harpoon", 160 },
            { "Spiky Ball", 161 },
            { "Ball O' Hurt", 162 },
            { "Blue Moon", 163 },
            { "Handgun", 164 },
            { "Water Bolt", 165 },
            { "Bomb", 166 },
            { "Dynamite", 167 },
            { "Grenade", 168 },
            { "Sand Block", 169 },
            { "Glass", 170 },
            { "Sign", 171 },
            { "Ash Block", 172 },
            { "Obsidian", 173 },
            { "Hellstone", 174 },
            { "Hellstone Bar", 175 },
            { "Mud Block", 176 },
            { "Sapphire", 177 },
            { "Ruby", 178 },
            { "Emerald", 179 },
            { "Topaz", 180 },
            { "Amethyst", 181 },
            { "Diamond", 182 },
            { "Glowing Mushroom", 183 },
            { "Star", 184 },
            { "Ivy Whip", 185 },
            { "Breathing Reed", 186 },
            { "Flipper", 187 },
            { "Healing Potion", 188 },
            { "Mana Potion", 189 },
            { "Blade of Grass", 190 },
            { "Thorn Chakram", 191 },
            { "Obsidian Brick", 192 },
            { "Obsidian Skull", 193 },
            { "Mushroom Grass Seeds", 194 },
            { "Jungle Grass Seeds", 195 },
            { "Wooden Hammer", 196 },
            { "Star Cannon", 197 },
            { "Blue Phaseblade", 198 },
            { "Red Phaseblade", 199 },
            { "Green Phaseblade", 200 },
            { "Purple Phaseblade", 201 },
            { "White Phaseblade", 202 },
            { "Yellow Phaseblade", 203 },
            { "Meteor Hamaxe", 204 },
            { "Empty Bucket", 205 },
            { "Water Bucket", 206 },
            { "Lava Bucket", 207 },
            { "Jungle Rose", 208 },
            { "Stinger", 209 },
            { "Vine", 210 },
            { "Feral Claws", 211 },
            { "Anklet of the Wind", 212 },
            { "Staff of Regrowth", 213 },
            { "Hellstone Brick", 214 },
            { "Whoopie Cushion", 215 },
            { "Shackle", 216 },
            { "Molten Hamaxe", 217 },
            { "Flamelash", 218 },
            { "Phoenix Blaster", 219 },
            { "Sunfury", 220 },
            { "Hellforge", 221 },
            { "Clay Pot", 222 },
            { "Nature's Gift", 223 },
            { "Bed", 224 },
            { "Silk", 225 },
            { "Lesser Restoration Potion", 226 },
            { "Restoration Potion", 227 },
            { "Jungle Hat", 228 },
            { "Jungle Shirt", 229 },
            { "Jungle Pants", 230 },
            { "Molten Helmet", 231 },
            { "Molten Breastplate", 232 },
            { "Molten Greaves", 233 },
            { "Meteor Shot", 234 },
            { "Sticky Bomb", 235 },
            { "Black Lens", 236 },
            { "Sunglasses", 237 },
            { "Wizard Hat", 238 },
            { "Top Hat", 239 },
            { "Tuxedo Shirt", 240 },
            { "Tuxedo Pants", 241 },
            { "Summer Hat", 242 },
            { "Bunny Hood", 243 },
            { "Plumber's Hat", 244 },
            { "Plumber's Shirt", 245 },
            { "Plumber's Pants", 246 },
            { "Hero's Hat", 247 },
            { "Hero's Shirt", 248 },
            { "Hero's Pants", 249 },
            { "Fish Bowl", 250 },
            { "Archaeologist's Hat", 251 },
            { "Archaeologist's Jacket", 252 },
            { "Archaeologist's Pants", 253 },
            { "Black Thread", 254 },
            { "Green Thread", 255 },
            { "Ninja Hood", 256 },
            { "Ninja Shirt", 257 },
            { "Ninja Pants", 258 },
            { "Leather", 259 },
            { "Red Hat", 260 },
            { "Goldfish", 261 },
            { "Robe", 262 },
            { "Robot Hat", 263 },
            { "Gold Crown", 264 },
            { "Hellfire Arrow", 265 },
            { "Sandgun", 266 },
            { "Guide Voodoo Doll", 267 },
            { "Diving Helmet", 268 },
            { "Familiar Shirt", 269 },
            { "Familiar Pants", 270 },
            { "Familiar Wig", 271 },
            { "Demon Scythe", 272 },
            { "Night's Edge", 273 },
            { "Dark Lance", 274 },
            { "Coral", 275 },
            { "Cactus", 276 },
            { "Trident", 277 },
            { "Silver Bullet", 278 },
            { "Throwing Knife", 279 },
            { "Spear", 280 },
            { "Blowpipe", 281 },
            { "Glowstick", 282 },
            { "Seed", 283 },
            { "Wooden Boomerang", 284 },
            { "Aglet", 285 },
            { "Sticky Glowstick", 286 },
            { "Poisoned Knife", 287 },
            { "Obsidian Skin Potion", 288 },
            { "Regeneration Potion", 289 },
            { "Swiftness Potion", 290 },
            { "Gills Potion", 291 },
            { "Ironskin Potion", 292 },
            { "Mana Regeneration Potion", 293 },
            { "Magic Power Potion", 294 },
            { "Featherfall Potion", 295 },
            { "Spelunker Potion", 296 },
            { "Invisibility Potion", 297 },
            { "Shine Potion", 298 },
            { "Night Owl Potion", 299 },
            { "Battle Potion", 300 },
            { "Thorns Potion", 301 },
            { "Water Walking Potion", 302 },
            { "Archery Potion", 303 },
            { "Hunter Potion", 304 },
            { "Gravitation Potion", 305 },
            { "Gold Chest", 306 },
            { "Daybloom Seeds", 307 },
            { "Moonglow Seeds", 308 },
            { "Blinkroot Seeds", 309 },
            { "Deathweed Seeds", 310 },
            { "Waterleaf Seeds", 311 },
            { "Fireblossom Seeds", 312 },
            { "Daybloom", 313 },
            { "Moonglow", 314 },
            { "Blinkroot", 315 },
            { "Deathweed", 316 },
            { "Waterleaf", 317 },
            { "Fireblossom", 318 },
            { "Shark Fin", 319 },
            { "Feather", 320 },
            { "Tombstone", 321 },
            { "Mime Mask", 322 },
            { "Antlion Mandible", 323 },
            { "Illegal Gun Parts", 324 },
            { "The Doctor's Shirt", 325 },
            { "The Doctor's Pants", 326 },
            { "Golden Key", 327 },
            { "Shadow Chest", 328 },
            { "Shadow Key", 329 },
            { "Obsidian Brick Wall", 330 },
            { "Jungle Spores", 331 },
            { "Loom", 332 },
            { "Piano", 333 },
            { "Dresser", 334 },
            { "Bench", 335 },
            { "Bathtub", 336 },
            { "Red Banner", 337 },
            { "Green Banner", 338 },
            { "Blue Banner", 339 },
            { "Yellow Banner", 340 },
            { "Lamp Post", 341 },
            { "Tiki Torch", 342 },
            { "Barrel", 343 },
            { "Chinese Lantern", 344 },
            { "Cooking Pot", 345 },
            { "Safe", 346 },
            { "Skull Lantern", 347 },
            { "Trash Can", 348 },
            { "Candelabra", 349 },
            { "Pink Vase", 350 },
            { "Mug", 351 },
            { "Keg", 352 },
            { "Ale", 353 },
            { "Bookcase", 354 },
            { "Throne", 355 },
            { "Bowl", 356 },
            { "Bowl of Soup", 357 },
            { "Toilet", 358 },
            { "Grandfather Clock", 359 },
            { "Armor Statue", 360 },
            { "Goblin Battle Standard", 361 },
            { "Tattered Cloth", 362 },
            { "Sawmill", 363 },
            { "Cobalt Ore", 364 },
            { "Mythril Ore", 365 },
            { "Adamantite Ore", 366 },
            { "Pwnhammer", 367 },
            { "Excalibur", 368 },
            { "Hallowed Seeds", 369 },
            { "Ebonsand Block", 370 },
            { "Cobalt Hat", 371 },
            { "Cobalt Helmet", 372 },
            { "Cobalt Mask", 373 },
            { "Cobalt Breastplate", 374 },
            { "Cobalt Leggings", 375 },
            { "Mythril Hood", 376 },
            { "Mythril Helmet", 377 },
            { "Mythril Hat", 378 },
            { "Mythril Chainmail", 379 },
            { "Mythril Greaves", 380 },
            { "Cobalt Bar", 381 },
            { "Mythril Bar", 382 },
            { "Cobalt Chainsaw", 383 },
            { "Mythril Chainsaw", 384 },
            { "Cobalt Drill", 385 },
            { "Mythril Drill", 386 },
            { "Adamantite Chainsaw", 387 },
            { "Adamantite Drill", 388 },
            { "Dao of Pow", 389 },
            { "Mythril Halberd", 390 },
            { "Adamantite Bar", 391 },
            { "Glass Wall", 392 },
            { "Compass", 393 },
            { "Diving Gear", 394 },
            { "GPS", 395 },
            { "Obsidian Horseshoe", 396 },
            { "Obsidian Shield", 397 },
            { "Tinkerer's Workshop", 398 },
            { "Cloud in a Balloon", 399 },
            { "Adamantite Headgear", 400 },
            { "Adamantite Helmet", 401 },
            { "Adamantite Mask", 402 },
            { "Adamantite Breastplate", 403 },
            { "Adamantite Leggings", 404 },
            { "Spectre Boots", 405 },
            { "Adamantite Glaive", 406 },
            { "Toolbelt", 407 },
            { "Pearlsand Block", 408 },
            { "Pearlstone Block", 409 },
            { "Mining Shirt", 410 },
            { "Mining Pants", 411 },
            { "Pearlstone Brick", 412 },
            { "Iridescent Brick", 413 },
            { "Mudstone Brick", 414 },
            { "Cobalt Brick", 415 },
            { "Mythril Brick", 416 },
            { "Pearlstone Brick Wall", 417 },
            { "Iridescent Brick Wall", 418 },
            { "Mudstone Brick Wall", 419 },
            { "Cobalt Brick Wall", 420 },
            { "Mythril Brick Wall", 421 },
            { "Holy Water", 422 },
            { "Unholy Water", 423 },
            { "Silt Block", 424 },
            { "Fairy Bell", 425 },
            { "Breaker Blade", 426 },
            { "Blue Torch", 427 },
            { "Red Torch", 428 },
            { "Green Torch", 429 },
            { "Purple Torch", 430 },
            { "White Torch", 431 },
            { "Yellow Torch", 432 },
            { "Demon Torch", 433 },
            { "Clockwork Assault Rifle", 434 },
            { "Cobalt Repeater", 435 },
            { "Mythril Repeater", 436 },
            { "Dual Hook", 437 },
            { "Star Statue", 438 },
            { "Sword Statue", 439 },
            { "Slime Statue", 440 },
            { "Goblin Statue", 441 },
            { "Shield Statue", 442 },
            { "Bat Statue", 443 },
            { "Fish Statue", 444 },
            { "Bunny Statue", 445 },
            { "Skeleton Statue", 446 },
            { "Reaper Statue", 447 },
            { "Woman Statue", 448 },
            { "Imp Statue", 449 },
            { "Gargoyle Statue", 450 },
            { "Gloom Statue", 451 },
            { "Hornet Statue", 452 },
            { "Bomb Statue", 453 },
            { "Crab Statue", 454 },
            { "Hammer Statue", 455 },
            { "Potion Statue", 456 },
            { "Spear Statue", 457 },
            { "Cross Statue", 458 },
            { "Jellyfish Statue", 459 },
            { "Bow Statue", 460 },
            { "Boomerang Statue", 461 },
            { "Boot Statue", 462 },
            { "Chest Statue", 463 },
            { "Bird Statue", 464 },
            { "Axe Statue", 465 },
            { "Corrupt Statue", 466 },
            { "Tree Statue", 467 },
            { "Anvil Statue", 468 },
            { "Pickaxe Statue", 469 },
            { "Mushroom Statue", 470 },
            { "Eyeball Statue", 471 },
            { "Pillar Statue", 472 },
            { "Heart Statue", 473 },
            { "Pot Statue", 474 },
            { "Sunflower Statue", 475 },
            { "King Statue", 476 },
            { "Queen Statue", 477 },
            { "Piranha Statue", 478 },
            { "Planked Wall", 479 },
            { "Wooden Beam", 480 },
            { "Adamantite Repeater", 481 },
            { "Adamantite Sword", 482 },
            { "Cobalt Sword", 483 },
            { "Mythril Sword", 484 },
            { "Moon Charm", 485 },
            { "Ruler", 486 },
            { "Crystal Ball", 487 },
            { "Disco Ball", 488 },
            { "Sorcerer Emblem", 489 },
            { "Warrior Emblem", 490 },
            { "Ranger Emblem", 491 },
            { "Demon Wings", 492 },
            { "Angel Wings", 493 },
            { "Magical Harp", 494 },
            { "Rainbow Rod", 495 },
            { "Ice Rod", 496 },
            { "Neptune's Shell", 497 },
            { "Mannequin", 498 },
            { "Greater Healing Potion", 499 },
            { "Greater Mana Potion", 500 },
            { "Pixie Dust", 501 },
            { "Crystal Shard", 502 },
            { "Clown Hat", 503 },
            { "Clown Shirt", 504 },
            { "Clown Pants", 505 },
            { "Flamethrower", 506 },
            { "Bell", 507 },
            { "Harp", 508 },
            { "Red Wrench", 509 },
            { "Wire Cutter", 510 },
            { "Active Stone Block", 511 },
            { "Inactive Stone Block", 512 },
            { "Lever", 513 },
            { "Laser Rifle", 514 },
            { "Crystal Bullet", 515 },
            { "Holy Arrow", 516 },
            { "Magic Dagger", 517 },
            { "Crystal Storm", 518 },
            { "Cursed Flames", 519 },
            { "Soul of Light", 520 },
            { "Soul of Night", 521 },
            { "Cursed Flame", 522 },
            { "Cursed Torch", 523 },
            { "Adamantite Forge", 524 },
            { "Mythril Anvil", 525 },
            { "Unicorn Horn", 526 },
            { "Dark Shard", 527 },
            { "Light Shard", 528 },
            { "Red Pressure Plate", 529 },
            { "Wire", 530 },
            { "Spell Tome", 531 },
            { "Star Cloak", 532 },
            { "Megashark", 533 },
            { "Shotgun", 534 },
            { "Philosopher's Stone", 535 },
            { "Titan Glove", 536 },
            { "Cobalt Naginata", 537 },
            { "Switch", 538 },
            { "Dart Trap", 539 },
            { "Boulder", 540 },
            { "Green Pressure Plate", 541 },
            { "Gray Pressure Plate", 542 },
            { "Brown Pressure Plate", 543 },
            { "Mechanical Eye", 544 },
            { "Cursed Arrow", 545 },
            { "Cursed Bullet", 546 },
            { "Soul of Fright", 547 },
            { "Soul of Might", 548 },
            { "Soul of Sight", 549 },
            { "Gungnir", 550 },
            { "Hallowed Plate Mail", 551 },
            { "Hallowed Greaves", 552 },
            { "Hallowed Helmet", 553 },
            { "Cross Necklace", 554 },
            { "Mana Flower", 555 },
            { "Mechanical Worm", 556 },
            { "Mechanical Skull", 557 },
            { "Hallowed Headgear", 558 },
            { "Hallowed Mask", 559 },
            { "Slime Crown", 560 },
            { "Light Disc", 561 },
            { "Music Box (Overworld Day)", 562 },
            { "Music Box (Eerie)", 563 },
            { "Music Box (Night)", 564 },
            { "Music Box (Title)", 565 },
            { "Music Box (Underground)", 566 },
            { "Music Box (Boss 1)", 567 },
            { "Music Box (Jungle)", 568 },
            { "Music Box (Corruption)", 569 },
            { "Music Box (Underground Corruption)", 570 },
            { "Music Box (The Hallow)", 571 },
            { "Music Box (Boss 2)", 572 },
            { "Music Box (Underground Hallow)", 573 },
            { "Music Box (Boss 3)", 574 },
            { "Soul of Flight", 575 },
            { "Music Box", 576 },
            { "Demonite Brick", 577 },
            { "Hallowed Repeater", 578 },
            { "Drax", 579 },
            { "Explosives", 580 },
            { "Inlet Pump", 581 },
            { "Outlet Pump", 582 },
            { "1 Second Timer", 583 },
            { "3 Second Timer", 584 },
            { "5 Second Timer", 585 },
            { "Candy Cane Block", 586 },
            { "Candy Cane Wall", 587 },
            { "Santa Hat", 588 },
            { "Santa Shirt", 589 },
            { "Santa Pants", 590 },
            { "Green Candy Cane Block", 591 },
            { "Green Candy Cane Wall", 592 },
            { "Snow Block", 593 },
            { "Snow Brick", 594 },
            { "Snow Brick Wall", 595 },
            { "Blue Light", 596 },
            { "Red Light", 597 },
            { "Green Light", 598 },
            { "Blue Present", 599 },
            { "Green Present", 600 },
            { "Yellow Present", 601 },
            { "Snow Globe", 602 },
            { "Carrot", 603 },
            { "Yellow Phasesaber", 3769 },
            { "White Phasesaber", 3768 },
            { "Purple Phasesaber", 3767 },
            { "Green Phasesaber", 3766 },
            { "Red Phasesaber", 3765 },
            { "Blue Phasesaber", 3764 },
            { "Platinum Bow", 3480 },
            { "Platinum Hammer", 3481 },
            { "Platinum Axe", 3482 },
            { "Platinum Shortsword", 3483 },
            { "Platinum Broadsword", 3484 },
            { "Platinum Pickaxe", 3485 },
            { "Tungsten Bow", 3486 },
            { "Tungsten Hammer", 3487 },
            { "Tungsten Axe", 3488 },
            { "Tungsten Shortsword", 3489 },
            { "Tungsten Broadsword", 3490 },
            { "Tungsten Pickaxe", 3491 },
            { "Lead Bow", 3492 },
            { "Lead Hammer", 3493 },
            { "Lead Axe", 3494 },
            { "Lead Shortsword", 3495 },
            { "Lead Broadsword", 3496 },
            { "Lead Pickaxe", 3497 },
            { "Tin Bow", 3498 },
            { "Tin Hammer", 3499 },
            { "Tin Axe", 3500 },
            { "Tin Shortsword", 3501 },
            { "Tin Broadsword", 3502 },
            { "Tin Pickaxe", 3503 },
            { "Copper Bow", 3504 },
            { "Copper Hammer", 3505 },
            { "Copper Axe", 3506 },
            { "Copper Shortsword", 3507 },
            { "Copper Broadsword", 3508 },
            { "Copper Pickaxe", 3509 },
            { "Silver Bow", 3510 },
            { "Silver Hammer", 3511 },
            { "Silver Axe", 3512 },
            { "Silver Shortsword", 3513 },
            { "Silver Broadsword", 3514 },
            { "Silver Pickaxe", 3515 },
            { "Gold Bow", 3516 },
            { "Gold Hammer", 3517 },
            { "Gold Axe", 3518 },
            { "Gold Shortsword", 3519 },
            { "Gold Broadsword", 3520 },
            { "Gold Pickaxe", 3521 }
        };
    }

    public static void WriteTileDataToStreamV1(Tile tile, BinaryWriter bw, uint version, bool[] frameIds, int maxTileId, int maxWallId, bool forceLighting = false)
    {
        // Fix chandelier objects.
        if (tile.IsActive && (tile.Type == (int)TileType.Chandelier))
        {
            // The wiki seems to be wrong on early variants.
            if (version < 36 && (tile.U > 36 || tile.V > 36)) // Max type: copper ON.
            {
                tile.IsActive = false;
            }
            else if (version < 72 && (tile.U > 90 || tile.V > 36)) // Max type: copper OFF.
            {
                tile.IsActive = false;
            }
            else if (version < 93 && (tile.U > 90 || tile.V > 360)) // Max type: jackelier OFF.
            {
                tile.IsActive = false;
            }
        }

        // Prevent these tiles from saving.
        if (tile.Type == (int)TileType.IceByRod ||
            tile.Type == (int)TileType.MysticSnakeRope ||
            tile.Type > byte.MaxValue ||
            tile.Type > maxTileId)
        {
            tile.IsActive = false;
        }

        bw.Write(tile.IsActive);
        if (tile.IsActive)
        {
            bw.Write((byte)tile.Type);
            if (version < 28 && tile.Type == (int)(TileType.Torch) ||
               (version < 40 && tile.Type == (int)TileType.Platform) ||
               (version < 195 && tile.Type == (int)TileType.WaterCandle))
            {
                // skip
            }
            else if (version < 72 &&
                (tile.Type == 35 || tile.Type == 36 || tile.Type == 170 || tile.Type == 171 || tile.Type == 172))
            {
                bw.Write((Int16)tile.U);
                bw.Write((Int16)tile.V);
            }
            else if (frameIds[tile.Type])
            {
                bw.Write((Int16)tile.U);
                bw.Write((Int16)tile.V);
            }

            if (version >= 48)
            {
                if (tile.TileColor > 0)
                {
                    bw.Write(true);
                    bw.Write((byte)tile.TileColor);
                }
                else
                    bw.Write(false);
            }
        }

        // Force add lighting to downgraded worlds.
        if (forceLighting)
        {
            bw.Write(true);
        }
        else if (version <= 25)
        {
            bw.Write(tile.v0_Lit); // legacy hasLight
        }

        if (tile.Wall > 0 && tile.Wall <= byte.MaxValue && tile.Wall <= maxWallId)
        {
            bw.Write(true);
            bw.Write((byte)tile.Wall);

            if (version >= 48)
            {
                if (tile.WallColor > 0)
                {
                    bw.Write(true);
                    bw.Write((byte)tile.WallColor);
                }
                else
                    bw.Write(false);
            }
        }
        else
            bw.Write(false);

        if (tile.LiquidAmount > 0)
        {
            bw.Write(true);
            bw.Write((byte)tile.LiquidAmount);
            bw.Write(tile.LiquidType == LiquidType.Lava);
            if (version >= 51)
            {
                bw.Write(tile.LiquidType == LiquidType.Honey);
            }
        }
        else
            bw.Write(false);

        if (version >= 33)
        {
            bw.Write(tile.WireRed);
        }
        if (version >= 43)
        {
            bw.Write(tile.WireGreen);
            bw.Write(tile.WireBlue);
        }

        if (version >= 41)
        {
            bw.Write(tile.BrickStyle != 0);

            if (version >= 49)
            {
                bw.Write((byte)tile.BrickStyle);
            }
        }

        if (version >= 42)
        {
            bw.Write(tile.Actuator);
            bw.Write(tile.InActive);
        }
    }

    public static void LoadV0(BinaryReader reader, string filename, World w, bool headersOnly = false, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        uint version = w.Version;
        w.Title = reader.ReadString();
        w.IsV0 = true;

        w.LeftWorld = reader.ReadInt32();
        w.RightWorld = reader.ReadInt32();
        w.TopWorld = reader.ReadInt32();
        w.BottomWorld = reader.ReadInt32();
        w.TilesHigh = reader.ReadInt32();
        w.TilesWide = reader.ReadInt32();


        w.SpawnX = reader.ReadInt32();
        w.SpawnY = reader.ReadInt32();
        w.GroundLevel = (int)reader.ReadDouble();
        w.RockLevel = (int)reader.ReadDouble();

        w.Time = reader.ReadDouble();
        w.DayTime = reader.ReadBoolean();
        w.MoonPhase = reader.ReadInt32();
        w.BloodMoon = reader.ReadBoolean();

        if (version >= 28)
        {
            w.DungeonX = reader.ReadInt32();
            w.DungeonY = reader.ReadInt32();
        }
        if (version >= 24)
        {
            w.DownedBoss1EyeofCthulhu = reader.ReadBoolean();
            w.DownedBoss2EaterofWorlds = reader.ReadBoolean();
        }

        if (version >= 28)
        {
            w.DownedBoss3Skeletron = reader.ReadBoolean();
        }
        else
        {
            w.DownedBoss3Skeletron = true;
        }

        if (version >= 26)
        {
            w.ShadowOrbSmashed = reader.ReadBoolean();
            w.SpawnMeteor = reader.ReadBoolean();
        }

        if (version >= 27)
        {
            w.InvasionDelay = reader.ReadInt32();
            w.InvasionSize = reader.ReadInt32();
            w.InvasionType = reader.ReadInt32();
            w.InvasionX = reader.ReadDouble();
        }

        w.Tiles = new Tile[w.TilesWide, w.TilesHigh];
        for (int i = 0; i < w.TilesWide; i++)
        {
            for (int j = 0; j < w.TilesHigh; j++)
            {
                w.Tiles[i, j] = new Tile();
            }
        }

        var frames = GetFramesV0();
        w.TileFrameImportant = frames;

        if (headersOnly) { return; }

        for (int x = 0; x < w.TilesWide; ++x)
        {
            progress?.Report(
                 new ProgressChangedEventArgs(x.ProgressPercentage(w.TilesWide), "Loading Tiles..."));

            for (int y = 0; y < w.TilesHigh; y++)
            {
                Tile tile = w.Tiles[x, y];

                tile.IsActive = reader.ReadBoolean();
                if (tile.IsActive)
                {
                    tile.Type = reader.ReadByte();

                    if (frames[tile.Type])
                    {
                        tile.U = reader.ReadInt16();
                        tile.V = reader.ReadInt16();
                    }
                }
                tile.v0_Lit = reader.ReadBoolean();
                if (reader.ReadBoolean())
                {
                    tile.Wall = reader.ReadByte();
                }

                if (version >= 34)
                {
                    if (reader.ReadBoolean())
                    {
                        tile.LiquidAmount = reader.ReadByte();
                        tile.LiquidType = LiquidType.Water;

                        if (version >= 35)
                        {
                            if (reader.ReadBoolean())
                            {
                                tile.LiquidType = LiquidType.Lava;
                            }
                        }
                    }
                }

                w.Tiles[x, y] = tile;
            }
        }

        progress?.Report(new ProgressChangedEventArgs(100, "Loading Chests..."));
        for (int i = 0; i < 1000; i++)
        {
            const int v0_chestSize = 20;

            if (reader.ReadBoolean())
            {
                var chest = new Chest(reader.ReadInt32(), reader.ReadInt32());

                for (int slot = 0; slot < v0_chestSize; slot++)
                {
                    int stackSize = reader.ReadByte();
                    chest.Items[slot].StackSize = stackSize;

                    if (stackSize > 0)
                    {
                        string itemName = reader.ReadString();
                        chest.Items[slot].NetId = FromLegacyName(itemName, (int)version);
                    }

                }

                w.Chests.Add(chest);
            }
        }

        if (version >= 33)
        {
            progress?.Report(new ProgressChangedEventArgs(100, "Loading Signs..."));
            for (int i = 0; i < 1000; i++)
            {
                if (reader.ReadBoolean())
                {
                    string text = reader.ReadString();

                    int X = reader.ReadInt32();
                    int Y = reader.ReadInt32();

                    if (w.Tiles[X, Y].IsActive && w.Tiles[X, Y].Type == 55)
                    {
                        var sign = new Sign(X, Y, text);

                        w.Signs.Add(sign);
                    }
                }
            }
        }

        if (version >= 20)
        {
            w.NPCs.Clear();
            progress?.Report(new ProgressChangedEventArgs(100, "Loading NPC Data..."));
            while (reader.ReadBoolean())
            {
                var npc = new NPC();

                npc.Name = reader.ReadString();
                npc.Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                npc.IsHomeless = reader.ReadBoolean();
                npc.Home = new Vector2Int32(reader.ReadInt32(), reader.ReadInt32());
                npc.SpriteId = 0;

                if (!string.IsNullOrWhiteSpace(npc.Name) && WorldConfiguration.NpcIds.ContainsKey(npc.Name))
                    npc.SpriteId = WorldConfiguration.NpcIds[npc.Name];

                w.NPCs.Add(npc);
            }
        }
    }

    public static void SaveV0(World world, BinaryWriter bw, bool ForceLighting, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        // Get the max values from json.
        var saveData = WorldConfiguration.SaveConfiguration.GetData(38);

        var version = world.Version;
        bw.Write(38); // Should be 38?
        bw.Write(world.Title);
        bw.Write((int)world.LeftWorld);
        bw.Write((int)world.RightWorld);
        bw.Write((int)world.TopWorld);
        bw.Write((int)world.BottomWorld);
        bw.Write(world.TilesHigh);
        bw.Write(world.TilesWide);
        bw.Write(world.SpawnX);
        bw.Write(world.SpawnY);
        bw.Write(world.GroundLevel);
        bw.Write(world.RockLevel);
        bw.Write(world.Time);
        bw.Write(world.DayTime);
        bw.Write(world.MoonPhase);
        bw.Write(world.BloodMoon);
        bw.Write(world.DungeonX);
        bw.Write(world.DungeonY);
        bw.Write(world.DownedBoss1EyeofCthulhu);
        bw.Write(world.DownedBoss2EaterofWorlds);
        bw.Write(world.DownedBoss3Skeletron);
        bw.Write(world.ShadowOrbSmashed);
        bw.Write(world.SpawnMeteor);
        bw.Write(world.InvasionDelay);
        bw.Write(world.InvasionSize);
        bw.Write(world.InvasionType);
        bw.Write(world.InvasionX);

        var frames = GetFramesV0();

        for (int x = 0; x < world.TilesWide; x++)
        {
            progress?.Report(
                 new ProgressChangedEventArgs(x.ProgressPercentage(world.TilesWide), "Removing Future Tiles..."));

            for (int y = 0; y < world.TilesHigh; y++)
            {
                Tile tile = world.Tiles[x, y];

                // Fix chandelier objects.
                if (tile.IsActive && (tile.Type == (int)TileType.Chandelier))
                {
                    // The wiki seems to be wrong on early variants.
                    if (tile.U > 36 || tile.V > 36) // Max type: copper ON.
                    {
                        tile.IsActive = false;
                    }
                }

                // Prevent these tiles from saving.
                if (tile.Type == (int)TileType.IceByRod ||
                    tile.Type == (int)TileType.MysticSnakeRope ||
                    tile.Type > byte.MaxValue ||
                    tile.Type > saveData.MaxTileId)
                {
                    tile.IsActive = false;
                }
            }
        }

        progress?.Report(new ProgressChangedEventArgs(100, "Settling Sand..."));
        int[] _tileSand = {
            53,  // Sand Block
            112, // Ebonsand Block
            116, // Pearlsand
            123, // silt
            224, // slush block
            234, // Crimsand block
        };

        for (int y = world.TilesHigh - 1; y > 0; y--)
        {
            for (int x = 0; x < world.TilesWide; x++)
            {
                var curTile = world.Tiles[x, y];
                if (_tileSand.Contains(curTile.Type))
                {
                    // check if tile below current tile is empty and move sand to there if it is.
                    int shiftAmmount = 1;
                    while (shiftAmmount + y < world.TilesHigh && !world.Tiles[x, y + shiftAmmount].IsActive)
                        shiftAmmount++;
                    shiftAmmount--;

                    if (shiftAmmount > 0)
                    {
                        var belowTile = world.Tiles[x, y + shiftAmmount];
                        if (!belowTile.IsActive)
                        {
                            belowTile.IsActive = true;
                            belowTile.Type = curTile.Type;
                            curTile.IsActive = false;
                        }
                    }
                }
            }
        }

        for (int x = 0; x < world.TilesWide; x++)
        {
            progress?.Report(
                 new ProgressChangedEventArgs(x.ProgressPercentage(world.TilesWide), "Saving Tiles..."));

            for (int y = 0; y < world.TilesHigh; y++)
            {
                Tile tile = world.Tiles[x, y];

                // Fix chandelier objects.
                if (tile.IsActive && (tile.Type == (int)TileType.Chandelier))
                {
                    // The wiki seems to be wrong on early variants.
                    if (tile.U > 36 || tile.V > 36) // Max type: copper ON.
                    {
                        tile.IsActive = false;
                    }
                }

                // Prevent these tiles from saving.
                if (tile.Type == (int)TileType.IceByRod ||
                    tile.Type == (int)TileType.MysticSnakeRope ||
                    tile.Type > byte.MaxValue ||
                    tile.Type > saveData.MaxTileId)
                {
                    tile.IsActive = false;
                }

                //if (bw.BaseStream.Position >= 0x11037) Debugger.Break();

                bw.Write(tile.IsActive);
                if (tile.IsActive)
                {
                    bw.Write((byte)tile.Type);

                    if (frames[tile.Type])
                    {
                        bw.Write(tile.U);
                        bw.Write(tile.V);
                    }
                }

                // Check if downgrade is occuring and if so add forced lighting.
                if (ForceLighting)
                    bw.Write(true);
                else
                    bw.Write((bool)tile.v0_Lit);

                if (tile.Wall > 0 && tile.Wall <= byte.MaxValue && tile.Wall <= saveData.MaxWallId)
                {
                    bw.Write(true);
                    bw.Write((byte)tile.Wall);
                }
                else
                {
                    bw.Write(false);
                }

                if (tile.LiquidAmount > 0 && tile.LiquidType != LiquidType.None)
                {
                    bw.Write(true);
                    bw.Write(tile.LiquidAmount);
                    bw.Write(tile.LiquidType == LiquidType.Lava);
                }
                else
                {
                    bw.Write(false);
                }
            }
        }

        progress?.Report(new ProgressChangedEventArgs(100, "Saving Chests..."));

        for (int i = 0; i < 1000; i++)
        {
            const int v0_chestSize = 20;

            if (i >= world.Chests.Count)
            {
                bw.Write(false);
                continue;
            }

            Chest chest = world.Chests[i];
            bw.Write(true);
            bw.Write(chest.X);
            bw.Write(chest.Y);

            for (int slot = 0; slot < v0_chestSize; slot++)
            {
                if (chest.Items[slot].StackSize > byte.MaxValue)
                {
                    bw.Write(byte.MaxValue);
                }
                else
                {
                    bw.Write((byte)chest.Items[slot].StackSize);
                }

                if (chest.Items[slot].StackSize > 0)
                {
                    bw.Write(ToLegacyName((short)chest.Items[slot].NetId, version));
                }
            }

        }

        progress?.Report(new ProgressChangedEventArgs(100, "Saving Signs..."));
        for (int i = 0; i < 1000; i++)
        {
            if (i >= world.Signs.Count)
            {
                bw.Write(false);
            }
            else
            {
                Sign curSign = world.Signs[i];
                bw.Write(true);
                bw.Write(curSign.Text);
                bw.Write(curSign.X);
                bw.Write(curSign.Y);
            }
        }

        progress?.Report(new ProgressChangedEventArgs(100, "Saving NPC Data..."));
        foreach (NPC curNpc in world.NPCs)
        {
            bw.Write(true);
            bw.Write(curNpc.Name);
            bw.Write(curNpc.Position.X);
            bw.Write(curNpc.Position.Y);
            bw.Write(curNpc.IsHomeless);
            bw.Write(curNpc.Home.X);
            bw.Write(curNpc.Home.Y);
        }
        bw.Write(false);
    }

    public static void LoadV1(BinaryReader reader, string filename, World w, bool headersOnly = false, IProgress<ProgressChangedEventArgs>? progress = null)
    {
        uint version = w.Version;
        w.Title = reader.ReadString();

        w.WorldId = reader.ReadInt32();
        w.Rand = new Random(w.WorldId);

        w.LeftWorld = reader.ReadInt32();
        w.RightWorld = reader.ReadInt32();
        w.TopWorld = reader.ReadInt32();
        w.BottomWorld = reader.ReadInt32();
        w.TilesHigh = reader.ReadInt32();
        w.TilesWide = reader.ReadInt32();

        //if (w.TilesHigh > 10000 || w.TilesWide > 10000 || w.TilesHigh <= 0 || w.TilesWide <= 0)
        //    throw new FileLoadException(string.Format("Invalid File: {0}", filename));

        if (version >= 63)
        {
            w.MoonType = reader.ReadByte();
        }
        else
        {
            w.MoonType = (byte)w.Rand.Next(WorldConfiguration.MaxMoons);
        }

        if (version >= 44)
        {
            w.TreeX0 = reader.ReadInt32();
            w.TreeX1 = reader.ReadInt32();
            w.TreeX2 = reader.ReadInt32();
            w.TreeStyle0 = reader.ReadInt32();
            w.TreeStyle1 = reader.ReadInt32();
            w.TreeStyle2 = reader.ReadInt32();
            w.TreeStyle3 = reader.ReadInt32();
        }

        if (version >= 60)
        {
            w.CaveBackX0 = reader.ReadInt32();
            w.CaveBackX1 = reader.ReadInt32();
            w.CaveBackX2 = reader.ReadInt32();
            w.CaveBackStyle0 = reader.ReadInt32();
            w.CaveBackStyle1 = reader.ReadInt32();
            w.CaveBackStyle2 = reader.ReadInt32();
            w.CaveBackStyle3 = reader.ReadInt32();
            w.IceBackStyle = reader.ReadInt32();

            if (version >= 61)
            {
                w.JungleBackStyle = reader.ReadInt32();
                w.HellBackStyle = reader.ReadInt32();
            }
        }
        else
        {
            w.CaveBackX[0] = w.TilesWide / 2;
            w.CaveBackX[1] = w.TilesWide;
            w.CaveBackX[2] = w.TilesWide;
            w.CaveBackStyle0 = 0;
            w.CaveBackStyle1 = 1;
            w.CaveBackStyle2 = 2;
            w.CaveBackStyle3 = 3;
            w.IceBackStyle = 0;
            w.JungleBackStyle = 0;
            w.HellBackStyle = 0;
        }

        w.SpawnX = reader.ReadInt32();
        w.SpawnY = reader.ReadInt32();
        w.GroundLevel = (int)reader.ReadDouble();
        w.RockLevel = (int)reader.ReadDouble();

        // read world flags
        w.Time = reader.ReadDouble();
        w.DayTime = reader.ReadBoolean();
        w.MoonPhase = reader.ReadInt32();
        w.BloodMoon = reader.ReadBoolean();

        if (version >= 70)
        {
            w.IsEclipse = reader.ReadBoolean();
        }

        w.DungeonX = reader.ReadInt32();
        w.DungeonY = reader.ReadInt32();

        if (version >= 56)
        {
            w.IsCrimson = reader.ReadBoolean();
        }
        else
        {
            w.IsCrimson = false;
        }

        w.DownedBoss1EyeofCthulhu = reader.ReadBoolean();
        w.DownedBoss2EaterofWorlds = reader.ReadBoolean();
        w.DownedBoss3Skeletron = reader.ReadBoolean();

        if (version >= 66)
        {
            w.DownedQueenBee = reader.ReadBoolean();
        }

        if (version >= 44)
        {
            w.DownedMechBoss1TheDestroyer = reader.ReadBoolean();
            w.DownedMechBoss2TheTwins = reader.ReadBoolean();
            w.DownedMechBoss3SkeletronPrime = reader.ReadBoolean();
            w.DownedMechBossAny = reader.ReadBoolean();
        }

        if (version >= 64)
        {
            w.DownedPlantBoss = reader.ReadBoolean();
            w.DownedGolemBoss = reader.ReadBoolean();
        }

        if (version >= 29)
        {
            w.SavedGoblin = reader.ReadBoolean();
            w.SavedWizard = reader.ReadBoolean();

            if (version >= 34)
            {
                w.SavedMech = reader.ReadBoolean();
            }
            w.DownedGoblins = reader.ReadBoolean();
        }
        if (version >= 32)
        {
            w.DownedClown = reader.ReadBoolean();
        }
        if (version >= 37)
        {
            w.DownedFrost = reader.ReadBoolean();
        }
        if (version >= 56)
        {
            w.DownedPirates = reader.ReadBoolean();
        }

        w.ShadowOrbSmashed = reader.ReadBoolean();
        w.SpawnMeteor = reader.ReadBoolean();
        w.ShadowOrbCount = reader.ReadByte();

        if (version >= 23)
        {
            w.AltarCount = reader.ReadInt32();
            w.HardMode = reader.ReadBoolean();
        }

        w.InvasionDelay = reader.ReadInt32();
        w.InvasionSize = reader.ReadInt32();
        w.InvasionType = reader.ReadInt32();
        w.InvasionX = reader.ReadDouble();

        if (version >= 53)
        {
            w.IsRaining = reader.ReadBoolean();
            w.TempRainTime = reader.ReadInt32();
            w.TempMaxRain = reader.ReadSingle();
        }
        if (version >= 54)
        {
            w.SavedOreTiersCobalt = reader.ReadInt32();
            w.SavedOreTiersMythril = reader.ReadInt32();
            w.SavedOreTiersAdamantite = reader.ReadInt32();
        }
        else if (version < 23 || w.AltarCount != 0)
        {
            w.SavedOreTiersCobalt = 107;
            w.SavedOreTiersMythril = 108;
            w.SavedOreTiersAdamantite = 111;
        }
        else
        {
            w.SavedOreTiersCobalt = -1;
            w.SavedOreTiersMythril = -1;
            w.SavedOreTiersAdamantite = -1;
        }

        if (version >= 55)
        {
            w.BgTree = reader.ReadByte();
            w.BgCorruption = reader.ReadByte();
            w.BgJungle = reader.ReadByte();
        }
        if (version >= 60)
        {
            w.BgSnow = reader.ReadByte();
            w.BgHallow = reader.ReadByte();
            w.BgCorruption = reader.ReadByte();
            w.BgDesert = reader.ReadByte();
            w.BgOcean = reader.ReadByte();
        }

        if (version >= 60)
        {
            w.CloudBgActive = reader.ReadInt32();
        }
        else
        {
            w.CloudBgActive = -w.Rand.Next(8640, 86400);
        }

        if (version >= 62)
        {
            w.NumClouds = reader.ReadInt16();
            w.WindSpeedSet = reader.ReadSingle();
        }

        w.Tiles = new Tile[w.TilesWide, w.TilesHigh];
        for (int i = 0; i < w.TilesWide; i++)
        {
            for (int j = 0; j < w.TilesHigh; j++)
            {
                w.Tiles[i, j] = new Tile();
            }
        }

        bool[] tileFrameImportant = WorldConfiguration.SaveConfiguration.GetTileFramesForVersion((int)version);

        if (headersOnly) { return; }

        for (int x = 0; x < w.TilesWide; ++x)
        {
            progress?.Report(
                 new ProgressChangedEventArgs(x.ProgressPercentage(w.TilesWide), "Loading Tiles..."));

            for (int y = 0; y < w.TilesHigh; y++)
            {
                Tile tile = ReadTileDataFromStreamV1(reader, version, tileFrameImportant);

                // read complete, start compression
                w.Tiles[x, y] = tile;

                if (version >= 25)
                {
                    int rle = reader.ReadInt16();

                    if (rle < 0)
                        throw new ApplicationException("Invalid Tile Data!");

                    if (rle > 0)
                    {
                        for (int k = y + 1; k < y + rle + 1; k++)
                        {
                            var tcopy = (Tile)tile.Clone();
                            w.Tiles[x, k] = tcopy;
                        }
                        y = y + rle;
                    }
                }
            }
        }

        progress?.Report(new ProgressChangedEventArgs(100, "Loading Chests..."));
        w.Chests.Clear();
        w.Chests.AddRange(ReadChestDataFromStreamV1(reader, version));

        progress?.Report(new ProgressChangedEventArgs(100, "Loading Signs..."));
        w.Signs.Clear();

        foreach (Sign sign in ReadSignDataFromStreamV1(reader, version))
        {
            if (w.Tiles[sign.X, sign.Y].IsActive && w.Tiles[sign.X, sign.Y].IsSign())
            {
                w.Signs.Add(sign);
            }
        }

        w.NPCs.Clear();
        progress?.Report(new ProgressChangedEventArgs(100, "Loading NPC Data..."));
        while (reader.ReadBoolean())
        {
            var npc = new NPC();

            npc.Name = reader.ReadString();
            npc.Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            npc.IsHomeless = reader.ReadBoolean();
            npc.Home = new Vector2Int32(reader.ReadInt32(), reader.ReadInt32());
            npc.SpriteId = 0;

            if (!string.IsNullOrWhiteSpace(npc.Name) && WorldConfiguration.NpcIds.ContainsKey(npc.Name))
                npc.SpriteId = WorldConfiguration.NpcIds[npc.Name];

            w.NPCs.Add(npc);
        }

        if (version >= 31 && version <= 83)
        {
            progress?.Report(new ProgressChangedEventArgs(100, "Loading NPC Names..."));
            w.CharacterNames.Add(new NpcName(17, reader.ReadString()));
            w.CharacterNames.Add(new NpcName(18, reader.ReadString()));
            w.CharacterNames.Add(new NpcName(19, reader.ReadString()));
            w.CharacterNames.Add(new NpcName(20, reader.ReadString()));
            w.CharacterNames.Add(new NpcName(22, reader.ReadString()));
            w.CharacterNames.Add(new NpcName(54, reader.ReadString()));
            w.CharacterNames.Add(new NpcName(38, reader.ReadString()));
            w.CharacterNames.Add(new NpcName(107, reader.ReadString()));
            w.CharacterNames.Add(new NpcName(108, reader.ReadString()));

            if (version >= 35)
            {
                w.CharacterNames.Add(new NpcName(124, reader.ReadString()));

                if (version >= 65)
                {
                    w.CharacterNames.Add(new NpcName(160, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(178, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(207, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(208, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(209, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(227, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(228, reader.ReadString()));
                    w.CharacterNames.Add(new NpcName(229, reader.ReadString()));

                    if (version >= 79)
                    {
                        w.CharacterNames.Add(new NpcName(353, reader.ReadString()));
                    }
                    else
                    {
                        w.CharacterNames.Add(GetNewNpc(353));
                    }
                }
                else
                {
                    // set defaults
                    w.CharacterNames.Add(GetNewNpc(160));
                    w.CharacterNames.Add(GetNewNpc(178));
                    w.CharacterNames.Add(GetNewNpc(207));
                    w.CharacterNames.Add(GetNewNpc(208));
                    w.CharacterNames.Add(GetNewNpc(209));
                    w.CharacterNames.Add(GetNewNpc(227));
                    w.CharacterNames.Add(GetNewNpc(228));
                    w.CharacterNames.Add(GetNewNpc(229));
                }
            }
            else
            {
                w.CharacterNames.Add(new NpcName(124, "Nancy"));
            }
        }
        else
        {
            // set defaults
            w.CharacterNames.Add(GetNewNpc(17));
            w.CharacterNames.Add(GetNewNpc(18));
            w.CharacterNames.Add(GetNewNpc(19));
            w.CharacterNames.Add(GetNewNpc(20));
            w.CharacterNames.Add(GetNewNpc(22));
            w.CharacterNames.Add(GetNewNpc(54));
            w.CharacterNames.Add(GetNewNpc(38));
            w.CharacterNames.Add(GetNewNpc(107));
            w.CharacterNames.Add(GetNewNpc(108));
            w.CharacterNames.Add(GetNewNpc(124));
            w.CharacterNames.Add(GetNewNpc(160));
            w.CharacterNames.Add(GetNewNpc(178));
            w.CharacterNames.Add(GetNewNpc(207));
            w.CharacterNames.Add(GetNewNpc(208));
            w.CharacterNames.Add(GetNewNpc(209));
            w.CharacterNames.Add(GetNewNpc(227));
            w.CharacterNames.Add(GetNewNpc(228));
            w.CharacterNames.Add(GetNewNpc(229));
            w.CharacterNames.Add(GetNewNpc(353));
        }

        if (version >= 7)
        {
            progress?.Report(new ProgressChangedEventArgs(100, "Validating File..."));
            bool validation = reader.ReadBoolean();
            string checkTitle = reader.ReadString();
            int checkVersion = reader.ReadInt32();
            if (validation && checkTitle == w.Title && checkVersion == w.WorldId)
            {
                //w.loadSuccess = true;
            }
            else
            {
                reader.Close();
                throw new FileLoadException(
                    $"Error reading world file validation parameters! {filename}");
            }
        }
        progress?.Report(new ProgressChangedEventArgs(0, "World Load Complete."));
    }

    public static IEnumerable<Sign> ReadSignDataFromStreamV1(BinaryReader b, uint version)
    {
        for (int i = 0; i < 1000; i++)
        {
            if (b.ReadBoolean())
            {
                var sign = new Sign();
                sign.Text = b.ReadString();
                sign.X = b.ReadInt32();
                sign.Y = b.ReadInt32();

                yield return sign;
            }
        }
    }

    public static IEnumerable<Chest> ReadChestDataFromStreamV1(BinaryReader b, uint version)
    {
        int chestSize = (version < 58) ? 20 : 40;

        for (int i = 0; i < 1000; i++)
        {
            if (b.ReadBoolean())
            {
                var chest = new Chest(b.ReadInt32(), b.ReadInt32());

                if (version >= 85)
                {
                    var chestName = b.ReadString();
                    if (chestName.Length > 20)
                    {
                        chestName = chestName.Substring(0, 20);
                    }
                    chest.Name = chestName;
                    //b.WriteBinary(chestName);
                }

                for (int slot = 0; slot < chestSize; slot++)
                {
                    if (slot < chestSize)
                    {
                        int stackSize = version < 59 ? b.ReadByte() : b.ReadInt16();
                        chest.Items[slot].StackSize = stackSize;

                        if (chest.Items[slot].StackSize > 0)
                        {
                            if (version >= 38)
                                chest.Items[slot].NetId = b.ReadInt32();
                            else
                                chest.Items[slot].NetId = FromLegacyName(b.ReadString(), (int)version);

                            chest.Items[slot].StackSize = stackSize;
                            // Read prefix
                            if (version >= 36)
                            {
                                chest.Items[slot].Prefix = b.ReadByte();
                            }
                        }
                    }
                }
                yield return chest;
            }
        }
    }

    public static Tile ReadTileDataFromStreamV1(BinaryReader b, uint version, bool[] frames)
    {
        var tile = new Tile();
        tile.IsActive = b.ReadBoolean();

        if (tile.IsActive)
        {
            tile.Type = b.ReadByte();

            if (tile.Type == (int)TileType.IceByRod || tile.Type == (int)TileType.MysticSnakeRope)
                tile.IsActive = false;

            if (version < 72 &&
                (tile.Type == 35 || tile.Type == 36 || tile.Type == 170 || tile.Type == 171 || tile.Type == 172))
            {
                tile.U = b.ReadInt16();
                tile.V = b.ReadInt16();
            }
            else if (!frames[tile.Type])
            {
                tile.U = -1;
                tile.V = -1;
            }
            else if (version < 28 && tile.Type == (int)(TileType.Torch))
            {
                // torches didn't have extra in older versions.
                tile.U = 0;
                tile.V = 0;
            }
            else if (version < 40 && tile.Type == (int)TileType.Platform)
            {
                tile.U = 0;
                tile.V = 0;
            }
            else if (version < 195 && tile.Type == (int)TileType.WaterCandle)
            {
                tile.U = 0;
                tile.V = 0;
            }
            else
            {
                tile.U = b.ReadInt16();
                tile.V = b.ReadInt16();

                if (tile.Type == (int)TileType.Timer)
                    tile.V = 0;
            }


            if (version >= 48 && b.ReadBoolean())
            {
                tile.TileColor = b.ReadByte();
            }
        }

        //skip obsolete hasLight
        if (version <= 25)
            tile.v0_Lit = b.ReadBoolean();

        if (b.ReadBoolean())
        {
            tile.Wall = b.ReadByte();

            if (version >= 48 && b.ReadBoolean())
            {
                tile.WallColor = b.ReadByte();
            }
        }

        if (b.ReadBoolean())
        {
            tile.LiquidType = LiquidType.Water;
            tile.LiquidAmount = b.ReadByte();
            if (b.ReadBoolean()) tile.LiquidType = LiquidType.Lava;

            if (version >= 51)
            {
                if (b.ReadBoolean()) tile.LiquidType = LiquidType.Honey;
            }
        }

        if (version >= 33)
        {
            tile.WireRed = b.ReadBoolean();
        }
        if (version >= 43)
        {
            tile.WireGreen = b.ReadBoolean();
            tile.WireBlue = b.ReadBoolean();
        }

        if (version >= 41)
        {
            bool isHalfBrick = b.ReadBoolean();

            var tileProperty = WorldConfiguration.TileProperties[tile.Type];

            if (isHalfBrick && tileProperty?.HasSlopes == true)
            {
                tile.BrickStyle = BrickStyle.HalfBrick;
            }

            if (version >= 49)
            {
                tile.BrickStyle = (BrickStyle)b.ReadByte();

                if (tileProperty == null || !tileProperty.HasSlopes)
                    tile.BrickStyle = 0;
            }
        }

        if (version >= 42)
        {
            tile.Actuator = b.ReadBoolean();
            tile.InActive = b.ReadBoolean();
        }

        return tile;
    }
}
