using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEdit.Configuration;
using TEdit.Geometry;
using TEdit.Terraria.Objects;
using TEdit.Utility;
using Vector2 = TEdit.Geometry.Vector2Float;

#nullable enable

namespace TEdit.Terraria
{
    public partial class World
    {
        /// <summary>
        /// Gets the Xbox-specific tileFrameImportant array based on the original Xbox 360 implementation.
        /// This array defines which tile types need their frame coordinates (U/V) saved.
        /// </summary>
        private static bool[] GetXboxTileFrameImportant()
        {
            bool[] tileFrameImportant = new bool[150];

            // Based on Xbox 360 Terraria reference implementation
            tileFrameImportant[3] = true;
            tileFrameImportant[4] = true;
            tileFrameImportant[5] = true;
            tileFrameImportant[10] = true;
            tileFrameImportant[11] = true;
            tileFrameImportant[12] = true;
            tileFrameImportant[13] = true;
            tileFrameImportant[14] = true;
            tileFrameImportant[15] = true;
            tileFrameImportant[16] = true;
            tileFrameImportant[17] = true;
            tileFrameImportant[18] = true;
            tileFrameImportant[20] = true;
            tileFrameImportant[21] = true;
            tileFrameImportant[24] = true;
            tileFrameImportant[26] = true;
            tileFrameImportant[27] = true;
            tileFrameImportant[28] = true;
            tileFrameImportant[29] = true;
            tileFrameImportant[31] = true;
            tileFrameImportant[33] = true;
            tileFrameImportant[34] = true;
            tileFrameImportant[35] = true;
            tileFrameImportant[36] = true;
            tileFrameImportant[42] = true;
            tileFrameImportant[50] = true;
            tileFrameImportant[55] = true;
            tileFrameImportant[61] = true;
            tileFrameImportant[71] = true;
            tileFrameImportant[72] = true;
            tileFrameImportant[73] = true;
            tileFrameImportant[74] = true;
            tileFrameImportant[77] = true;
            tileFrameImportant[78] = true;
            tileFrameImportant[79] = true;
            tileFrameImportant[81] = true;
            tileFrameImportant[82] = true;
            tileFrameImportant[83] = true;
            tileFrameImportant[84] = true;
            tileFrameImportant[85] = true;
            tileFrameImportant[86] = true;
            tileFrameImportant[87] = true;
            tileFrameImportant[88] = true;
            tileFrameImportant[89] = true;
            tileFrameImportant[90] = true;
            tileFrameImportant[91] = true;
            tileFrameImportant[92] = true;
            tileFrameImportant[93] = true;
            tileFrameImportant[94] = true;
            tileFrameImportant[95] = true;
            tileFrameImportant[96] = true;
            tileFrameImportant[97] = true;
            tileFrameImportant[98] = true;
            tileFrameImportant[99] = true;
            tileFrameImportant[100] = true;
            tileFrameImportant[101] = true;
            tileFrameImportant[102] = true;
            tileFrameImportant[103] = true;
            tileFrameImportant[104] = true;
            tileFrameImportant[105] = true;
            tileFrameImportant[106] = true;
            tileFrameImportant[110] = true;
            tileFrameImportant[113] = true;
            tileFrameImportant[114] = true;
            tileFrameImportant[125] = true;
            tileFrameImportant[126] = true;
            tileFrameImportant[128] = true;
            tileFrameImportant[129] = true;
            tileFrameImportant[132] = true;
            tileFrameImportant[133] = true;
            tileFrameImportant[134] = true;
            tileFrameImportant[135] = true;
            tileFrameImportant[136] = true;
            tileFrameImportant[137] = true;
            tileFrameImportant[138] = true;
            tileFrameImportant[139] = true;
            tileFrameImportant[141] = true;
            tileFrameImportant[142] = true;
            tileFrameImportant[143] = true;
            tileFrameImportant[144] = true;
            tileFrameImportant[149] = true;

            return tileFrameImportant;
        }

        public static void SaveXbox(World world, BinaryWriter bw, bool incrementRevision = true, IProgress<ProgressChangedEventArgs>? progress = null)
        {
            // Xbox worlds use version 47-87
            uint version = world.Version;

            if (version > 87 || version < 47)
            {
                throw new InvalidOperationException($"Xbox save only supports versions 47-87, current version: {version}");
            }

            // Write version
            bw.Write(version);

            // For version > 46, we need to write CRC32 placeholder
            long crcPosition = 0;
            if (version > 46)
            {
                crcPosition = bw.BaseStream.Position;
                bw.Write((uint)0); // Placeholder for CRC32
            }

            bw.Write(world.Title);
            bw.Write(world.WorldId);

            if (version >= 48)
            {
                bw.Write(world.XboxWorldTimestamp);
            }

            bw.Write((int)world.RightWorld);
            bw.Write((short)world.BottomWorld);
            bw.Write((short)world.TilesHigh);
            bw.Write((short)world.TilesWide);

            bw.Write((short)world.SpawnX);
            bw.Write((short)world.SpawnY);
            bw.Write((short)world.GroundLevel);
            bw.Write((short)world.RockLevel);

            bw.Write((float)world.Time);
            bw.Write(world.DayTime);
            bw.Write((byte)world.MoonPhase);
            bw.Write(world.BloodMoon);

            bw.Write((short)world.DungeonX);
            bw.Write((short)world.DungeonY);

            bw.Write(world.DownedBoss1EyeofCthulhu);
            bw.Write(world.DownedBoss2EaterofWorlds);
            bw.Write(world.DownedBoss3Skeletron);
            bw.Write(world.SavedGoblin);
            bw.Write(world.SavedWizard);
            bw.Write(world.SavedMech);
            bw.Write(world.DownedGoblins);
            bw.Write(world.DownedClown);
            bw.Write(world.DownedFrost);

            bw.Write(world.ShadowOrbSmashed);
            bw.Write(world.SpawnMeteor);
            bw.Write((byte)world.ShadowOrbCount);

            bw.Write(world.AltarCount);
            bw.Write(world.HardMode);

            bw.Write((byte)world.InvasionDelay);
            bw.Write((short)world.InvasionSize);
            bw.Write((byte)world.InvasionType);
            bw.Write((float)world.InvasionX);

            // Save tiles
            progress?.Report(new ProgressChangedEventArgs(0, "Saving Tiles..."));

            var saveData = WorldConfiguration.SaveConfiguration.GetData((int)version);
            bool[] frameIds = GetXboxTileFrameImportant();

            for (int x = 0; x < world.TilesWide; x++)
            {
                if ((x & 0x1F) == 0)
                {
                    progress?.Report(new ProgressChangedEventArgs((int)(x.ProgressPercentage(world.TilesWide) * 0.8), "Saving Tiles..."));
                }

                int rleCount = 0;
                for (int y = 0; y < world.TilesHigh; y = y + rleCount + 1)
                {
                    Tile tile = world.Tiles[x, y];

                    // Remove invalid tile 127 before saving
                    if (tile.Type == 127)
                    {
                        tile.IsActive = false;
                    }

                    // Write active state
                    if (tile.IsActive)
                    {
                        bw.Write(true);
                        bw.Write((byte)tile.Type);

                        if (tile.Type < frameIds.Length && frameIds[tile.Type])
                        {
                            bw.Write(tile.U);
                            bw.Write(tile.V);
                        }
                    }
                    else
                    {
                        bw.Write(false);
                    }

                    // Write wall
                    bw.Write((byte)tile.Wall);

                    // Write liquid
                    bw.Write((byte)tile.LiquidAmount);
                    if (tile.LiquidAmount > 0)
                    {
                        bw.Write(tile.LiquidType == LiquidType.Lava);
                    }

                    // Write wire flags
                    byte flags = 0;
                    if (tile.WireRed) flags |= 0x01;
                    if (tile.WireBlue) flags |= 0x02;
                    if (tile.WireGreen) flags |= 0x04;
                    bw.Write(flags);

                    // RLE compression - count consecutive identical tiles
                    rleCount = 0;
                    while (y + rleCount + 1 < world.TilesHigh)
                    {
                        Tile nextTile = world.Tiles[x, y + rleCount + 1];

                        // Compare tiles for RLE (must match all saved properties)
                        if (tile.IsActive != nextTile.IsActive) break;
                        if (tile.IsActive)
                        {
                            if (tile.Type != nextTile.Type) break;
                            if (tile.Type < frameIds.Length && frameIds[tile.Type])
                            {
                                if (tile.U != nextTile.U || tile.V != nextTile.V) break;
                            }
                        }
                        if (tile.Wall != nextTile.Wall) break;
                        if (tile.LiquidAmount != nextTile.LiquidAmount) break;
                        if (tile.LiquidAmount > 0 && tile.LiquidType != nextTile.LiquidType) break;
                        if (tile.WireRed != nextTile.WireRed) break;
                        if (tile.WireBlue != nextTile.WireBlue) break;
                        if (tile.WireGreen != nextTile.WireGreen) break;

                        rleCount++;
                        if (rleCount >= 32767) break; // Max for 2-byte encoding
                    }

                    if (rleCount < 128)
                    {
                        bw.Write((byte)rleCount);
                    }
                    else
                    {
                        bw.Write((byte)((rleCount & 0x7F) | 0x80));
                        bw.Write((byte)((rleCount >> 7) & 0xFF));
                    }
                }
            }

            // Save chests
            progress?.Report(new ProgressChangedEventArgs(80, "Saving Chests..."));
            for (int i = 0; i < 1000; i++)
            {
                if (i < world.Chests.Count)
                {
                    Chest chest = world.Chests[i];
                    bw.Write(true);
                    bw.Write((short)chest.X);
                    bw.Write((short)chest.Y);

                    for (int slot = 0; slot < 20; slot++)
                    {
                        if (slot < chest.Items.Count && chest.Items[slot].StackSize > 0)
                        {
                            bw.Write((byte)chest.Items[slot].StackSize);
                            bw.Write((short)chest.Items[slot].NetId);
                            bw.Write((byte)chest.Items[slot].Prefix);
                        }
                        else
                        {
                            bw.Write((byte)0);
                        }
                    }
                }
                else
                {
                    bw.Write(false);
                }
            }

            // Save signs
            progress?.Report(new ProgressChangedEventArgs(90, "Saving Signs..."));
            for (int i = 0; i < 1000; i++)
            {
                if (i < world.Signs.Count)
                {
                    Sign sign = world.Signs[i];
                    bw.Write(true);
                    bw.Write(sign.Text ?? "");
                    bw.Write((short)sign.X);
                    bw.Write((short)sign.Y);
                }
                else
                {
                    bw.Write(false);
                }
            }

            // Save NPCs
            progress?.Report(new ProgressChangedEventArgs(95, "Saving NPCs..."));
            foreach (var npc in world.NPCs)
            {
                bw.Write(true);
                bw.Write((byte)npc.SpriteId);
                bw.Write(npc.Position.X);
                bw.Write(npc.Position.Y);
                bw.Write(npc.IsHomeless);
                bw.Write((short)npc.Home.X);
                bw.Write((short)npc.Home.Y);
            }
            bw.Write(false); // End of NPCs

            // Save NPC names
            string[] npcNames = new string[10];
            foreach (var npc in world.CharacterNames)
            {
                int index = GetXboxNpcNameIndex(npc.Id);
                if (index >= 0 && index < npcNames.Length)
                {
                    npcNames[index] = npc.Name ?? "";
                }
            }

            for (int i = 0; i < 10; i++)
            {
                bw.Write(npcNames[i] ?? "");
            }

            // Calculate and write CRC32 if needed
            // Note: CRC32 must be calculated on the entire data from position 8 onwards
            // If the underlying stream is a MemoryStream, we can access its buffer
            if (version > 46 && bw.BaseStream is MemoryStream ms)
            {
                long endPosition = ms.Position;

                // Calculate CRC32 on data from position 8 to end
                byte[] buffer = ms.GetBuffer();
                int dataLength = (int)endPosition - 8;

                var crc = new Crc32();
                crc.Append(new ReadOnlySpan<byte>(buffer, 8, dataLength));
                byte[] hash = crc.GetCurrentHash();
                uint crcValue = BitConverter.ToUInt32(hash, 0);

                // Write CRC32 at position 4
                ms.Position = crcPosition;
                bw.Write(crcValue);
                ms.Position = endPosition;
            }
            else if (version > 46)
            {
                // If not a MemoryStream, write 0 for CRC32
                // (CRC validation can be skipped when loading if CRC is 0)
                long endPosition = bw.BaseStream.Position;
                bw.BaseStream.Position = crcPosition;
                bw.Write((uint)0);
                bw.BaseStream.Position = endPosition;
            }

            progress?.Report(new ProgressChangedEventArgs(100, "Save Complete."));
        }

        public static void LoadXbox(BinaryReader b, World w, bool headersOnly = false, IProgress<ProgressChangedEventArgs>? progress = null)
        {

            uint version = w.Version;
            w.IsXbox = true;
            if (version > 87)
            {
                throw new InvalidOperationException($"Xbox version {version} is not supported");
            }

            // Check CRC32 for version > 46
            if (version > 46)
            {
                uint expectedCrc = b.ReadUInt32();

                long dataStart = 8;
                long dataLength = b.BaseStream.Length - dataStart;
                byte[] data = b.ReadBytes((int)dataLength);

                var crc = new Crc32();
                crc.Append(data);
                byte[] hash = crc.GetCurrentHash();
                uint actualCrc = BitConverter.ToUInt32(hash, 0);

                if (expectedCrc != actualCrc && expectedCrc != 0)
                {
                    throw new InvalidOperationException("Invalid CRC32");
                }

                b.BaseStream.Position = dataStart;
            }

            w.Title = b.ReadString();
            w.WorldId = b.ReadInt32();

            if (version >= 48)
            {
                w.XboxWorldTimestamp = b.ReadInt32();
            }

            w.RightWorld = b.ReadInt32();
            w.BottomWorld = b.ReadInt16();
            w.TilesHigh = b.ReadInt16();
            w.TilesWide = b.ReadInt16();

            w.SpawnX = b.ReadInt16();
            w.SpawnY = b.ReadInt16();
            w.GroundLevel = b.ReadInt16();
            w.RockLevel = b.ReadInt16();

            w.Time = b.ReadSingle();
            w.DayTime = b.ReadBoolean();
            w.MoonPhase = b.ReadByte();
            w.BloodMoon = b.ReadBoolean();

            w.DungeonX = b.ReadInt16();
            w.DungeonY = b.ReadInt16();

            w.DownedBoss1EyeofCthulhu = b.ReadBoolean();
            w.DownedBoss2EaterofWorlds = b.ReadBoolean();
            w.DownedBoss3Skeletron = b.ReadBoolean();
            w.SavedGoblin = b.ReadBoolean();
            w.SavedWizard = b.ReadBoolean();
            w.SavedMech = b.ReadBoolean();
            w.DownedGoblins = b.ReadBoolean();
            w.DownedClown = b.ReadBoolean();
            w.DownedFrost = b.ReadBoolean();

            w.ShadowOrbSmashed = b.ReadBoolean();
            w.SpawnMeteor = b.ReadBoolean();
            w.ShadowOrbCount = b.ReadByte();

            w.AltarCount = b.ReadInt32();
            w.HardMode = b.ReadBoolean();

            w.InvasionDelay = b.ReadByte();
            w.InvasionSize = b.ReadInt16();
            w.InvasionType = b.ReadByte();
            w.InvasionX = b.ReadSingle();

            // Initialize tiles
            w.Tiles = new Tile[w.TilesWide, w.TilesHigh];
            for (int i = 0; i < w.TilesWide; i++)
            {
                for (int j = 0; j < w.TilesHigh; j++)
                {
                    w.Tiles[i, j] = new Tile();
                }
            }

            bool[] frameIds = GetXboxTileFrameImportant();
            w.TileFrameImportant = frameIds;

            if (headersOnly) return;

            // Load tiles
            progress?.Report(new ProgressChangedEventArgs(0, "Loading Tiles..."));

            for (int x = 0; x < w.TilesWide; x++)
            {
                if ((x & 0x1F) == 0)
                {
                    progress?.Report(new ProgressChangedEventArgs((int)(x.ProgressPercentage(w.TilesWide) * 0.8), "Loading Tiles..."));
                }

                int rleCount = 0;
                for (int y = 0; y < w.TilesHigh; y = y + rleCount + 1)
                {
                    Tile tile = w.Tiles[x, y];

                    // Read active state - Xbox format uses byte (0 or 1), not boolean
                    byte active = b.ReadByte();
                    tile.IsActive = active != 0;

                    if (tile.IsActive)
                    {
                        tile.Type = b.ReadByte();

                        // Fix invalid tile 127
                        if (tile.Type == 127)
                        {
                            tile.IsActive = false;
                        }

                        // Check frame important BEFORE the active check (matches reference)
                        if (tile.Type < frameIds.Length && frameIds[tile.Type])
                        {
                            tile.U = b.ReadInt16();
                            tile.V = b.ReadInt16();

                            // Fix timer tile frame
                            if (tile.Type == 144)
                            {
                                tile.V = 0;
                            }
                        }
                        else
                        {
                            tile.U = -1;
                            tile.V = -1;
                        }
                    }
                    else
                    {
                        // Inactive tiles get default values
                        tile.Type = 0;
                        tile.U = -1;
                        tile.V = -1;
                    }

                    // Read wall
                    tile.Wall = b.ReadByte();

                    // Read liquid
                    tile.LiquidAmount = b.ReadByte();
                    if (tile.LiquidAmount > 0)
                    {
                        bool isLava = b.ReadBoolean();
                        tile.LiquidType = isLava ? LiquidType.Lava : LiquidType.Water;
                    }

                    // Read wire flags
                    byte flags = b.ReadByte();
                    tile.WireRed = (flags & 0x01) != 0;
                    tile.WireBlue = (flags & 0x02) != 0;
                    tile.WireGreen = (flags & 0x04) != 0;

                    // Read RLE count
                    rleCount = b.ReadByte();
                    if ((rleCount & 0x80) != 0)
                    {
                        rleCount &= 0x7F;
                        rleCount |= b.ReadByte() << 7;
                    }

                    // Copy tile for RLE
                    for (int i = 0; i < rleCount && y + i + 1 < w.TilesHigh; i++)
                    {
                        Tile copiedTile = (Tile)tile.Clone();
                        w.Tiles[x, y + i + 1] = copiedTile;
                    }
                }
            }

            // Load chests
            progress?.Report(new ProgressChangedEventArgs(80, "Loading Chests..."));
            for (int i = 0; i < 1000; i++)
            {
                if (b.ReadBoolean())
                {
                    var chest = new Chest(b.ReadInt16(), b.ReadInt16());

                    for (int slot = 0; slot < 20; slot++)
                    {
                        byte stackSize = b.ReadByte();
                        if (stackSize > 0)
                        {
                            short netId = b.ReadInt16();
                            byte prefix = b.ReadByte();

                            if (slot < chest.Items.Count)
                            {
                                chest.Items[slot].StackSize = stackSize;
                                chest.Items[slot].NetId = netId;
                                chest.Items[slot].Prefix = prefix;
                            }
                        }
                    }

                    w.Chests.Add(chest);
                }
            }

            // Load signs
            progress?.Report(new ProgressChangedEventArgs(90, "Loading Signs..."));
            for (int i = 0; i < 1000; i++)
            {
                if (b.ReadBoolean())
                {
                    string text = b.ReadString();
                    int x = b.ReadInt16();
                    int y = b.ReadInt16();

                    // Xbox format: load all signs without tile type validation
                    // The Xbox version doesn't validate tile types when loading signs
                    if (w.ValidTileLocation(x, y))
                    {
                        var sign = new Sign(x, y, text);
                        w.Signs.Add(sign);
                    }
                }
            }

            // Load NPCs
            progress?.Report(new ProgressChangedEventArgs(95, "Loading NPCs..."));
            w.NPCs.Clear();

            while (b.ReadBoolean())
            {
                var npc = new NPC
                {
                    SpriteId = b.ReadByte(),
                    Position = new Vector2(b.ReadSingle(), b.ReadSingle()),
                    IsHomeless = b.ReadBoolean(),
                    Home = new Vector2Int32(b.ReadInt16(), b.ReadInt16())
                };

                w.NPCs.Add(npc);
            }

            // Load NPC names
            int[] npcIds = { 17, 18, 19, 20, 22, 54, 38, 107, 108, 124 };
            w.CharacterNames.Clear();

            for (int i = 0; i < 10; i++)
            {
                string name = b.ReadString();
                if (!string.IsNullOrWhiteSpace(name) && i < npcIds.Length)
                {
                    w.CharacterNames.Add(new NpcName(npcIds[i], name));
                }
            }

            progress?.Report(new ProgressChangedEventArgs(100, "Load Complete."));
        }

        private static int GetXboxNpcNameIndex(int npcId)
        {
            // Map NPC IDs to their index in the Xbox name array
            return npcId switch
            {
                17 => 0,   // Merchant
                18 => 1,   // Nurse
                19 => 2,   // Arms Dealer
                20 => 3,   // Dryad
                22 => 4,   // Guide
                54 => 5,   // Clothier
                38 => 6,   // Demolitionist
                107 => 7,  // Goblin Tinkerer
                108 => 8,  // Wizard
                124 => 9,  // Mechanic
                _ => -1
            };
        }
    }
}

