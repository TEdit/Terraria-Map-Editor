using System;
using System.IO;
using System.Windows;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Win32;

using TEdit.Terraria;
using TEdit.ViewModel;
using TEdit.Configuration;

using Color = Microsoft.Xna.Framework.Color;

namespace TEdit.Editor.Plugins
{
    public class PlayerMapRenderer : BasePlugin
    {
        // Constructor for PlayerMapRenderer
        public PlayerMapRenderer(WorldViewModel worldViewModel) : base(worldViewModel)
        {
            Name = "Render Player .map File";
        }

        // Method to execute the plugin functionality
        public async override void Execute()
        {
            // Ensure the world is loaded
            if (_wvm.CurrentWorld == null) return;

            // Prompt user to select the player file
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"My Games\Terraria\Players"),
                Filter = "Player Files (*.plr)|*.plr",
                Title = "Select a Player File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Use a try statement
                try
                {
                    string selectedFilePath = openFileDialog.FileName;

                    // Extract the filename without extension
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFilePath);

                    // Create a new folder with the filename in the InitialDirectory path
                    string newFolderPath = Path.Combine(openFileDialog.InitialDirectory, fileNameWithoutExtension);
                    Directory.CreateDirectory(newFolderPath);

                    // Generate the .map file and save it to the players folder
                    await MapBuilder.BuildMapAsync(_wvm.CurrentWorld, newFolderPath);

                    // Show the completion
                    MessageBox.Show("Map Generated!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {
                // Handle the case where the user cancels the file selection
                MessageBox.Show("No player file selected.");
            }
        }

        public class MapBuilder
        {
            public static async Task BuildMapAsync(World world, string outputPath)
            {
                await Task.Run(() =>
                {
                    // Grab the data on the current world
                    var version = world?.Version ?? WorldConfiguration.CompatibleVersion;
                    var saveData = WorldConfiguration.SaveConfiguration.GetData(version);
                    int maxTileID = saveData.MaxTileId + 1; // +1 is to include "count".
                    int maxWallID = saveData.MaxWallId + 1; // +1 is to include "count".

                    // Initialize MapHelper with max IDs
                    MapHelper.Initialize(maxTileID, maxWallID);

                    // Ensure the directory exists
                    if (!Directory.Exists(outputPath))
                    {
                        throw new DirectoryNotFoundException("The specified output directory does not exist.");
                    }

                    // Construct the file path
                    string fileName = $"{world.WorldGUID}.map";

                    // Define proper filename based on worldgen version
                    if (world.WorldGenVersion >= 777389080577UL)
                        fileName = $"{world.WorldGUID}.map";
                    else
                        fileName = $"{world.WorldId}.map";

                    string fullPath = Path.Combine(outputPath, fileName);

                    // Initiate the file streams
                    using MemoryStream memoryStream = new(4000);
                    using (BinaryWriter binaryWriter = new(memoryStream))
                    {
                        using DeflateStream deflateStream = new(memoryStream, CompressionLevel.Optimal);

                        // Write map metadata
                        binaryWriter.Write(world.Version);                                       // Write world version

                        binaryWriter.Write(27981915666277746UL | ((ulong)FileType.Map << 56));   // Map type identifier
                        binaryWriter.Write(world.FileRevision += 1U);                            // Write file revision
                        binaryWriter.Write((ulong)((long)((world.IsFavorite ? 1 : 0) & 1) | 0)); // Is favorite flag

                        binaryWriter.Write(world.Title);       // Write world name
                        binaryWriter.Write(world.WorldId);     // Write world ID
                        binaryWriter.Write(world.TilesHigh);   // Write max tiles Y
                        binaryWriter.Write(world.TilesWide);   // Write max tiles X
                        binaryWriter.Write((short)maxTileID);  // Write max tile count
                        binaryWriter.Write((short)maxWallID);  // Write max wall count

                        // Write the offsets and padding bytes
                        binaryWriter.Write((byte)4); // Offset
                        binaryWriter.Write((byte)0); // Padding byte

                        binaryWriter.Write((byte)0); // Padding byte
                        binaryWriter.Write((byte)1); // Offset

                        binaryWriter.Write((byte)0); // Padding byte
                        binaryWriter.Write((byte)1); // Offset

                        binaryWriter.Write((byte)0); // Padding byte
                        binaryWriter.Write((byte)1); // Offset

                        // Write tile and wall option counts using helper methods
                        WriteOptionCounts(binaryWriter, MapHelper.tileOptionCounts, maxTileID);
                        WriteOptionCounts(binaryWriter, MapHelper.wallOptionCounts, maxWallID);

                        // Write actual tile and wall counts
                        WriteCounts(binaryWriter, MapHelper.tileOptionCounts, maxTileID);
                        WriteCounts(binaryWriter, MapHelper.wallOptionCounts, maxWallID);

                        binaryWriter.Flush(); // Ensure all data is written to the binary writer

                        // Write map data to the deflate stream
                        WriteMapData(deflateStream, world);

                        deflateStream.Flush(); // Ensure all data is written to the deflate stream
                    }

                    // Save the file to the specified path
                    using FileStream fileStream = File.Open(fullPath, FileMode.Create);

                    // Write the memory stream contents to the file stream in chunks
                    while (fileStream.Position < (long)memoryStream.ToArray().Length)
                    {
                        fileStream.Write(memoryStream.ToArray(), (int)fileStream.Position, Math.Min(memoryStream.ToArray().Length - (int)fileStream.Position, 2048));
                    }
                });
            }

            #region Main Data Writers

            private static void WriteOptionCounts(BinaryWriter writer, int[] optionCounts, int maxId)
            {
                byte currentByte = 1; // Bit position indicator within a byte (1, 2, 4, 8, 16, 32, 64, 128)
                byte outputByte = 0; // Byte to accumulate bits indicating optionCounts[i] != 1

                for (int i = 0; i < maxId; i++)
                {
                    if (optionCounts[i] != 1)
                    {
                        outputByte |= currentByte; // Set the bit if the condition is met
                    }
                    if (currentByte == 128)
                    {
                        writer.Write(outputByte); // Write the accumulated byte to the writer
                        outputByte = 0; // Reset the accumulated byte
                        currentByte = 1; // Reset the bit position indicator
                    }
                    else
                    {
                        currentByte <<= 1; // Move to the next bit position
                    }
                }
                if (currentByte != 1)
                {
                    writer.Write(outputByte); // Write the last accumulated byte if any bits were set
                }
            }

            private static void WriteCounts(BinaryWriter writer, int[] counts, int maxId)
            {
                for (int i = 0; i < maxId; i++)
                {
                    if (counts[i] != 1)
                    {
                        writer.Write((byte)counts[i]); // Write the count if it's not equal to 1
                    }
                }
            }

            private static void WriteMapData(DeflateStream deflateStream, World world)
            {
                int bufferPosition = 0; // Initialize buffer position
                byte[] buffer = new byte[16384]; // Create a buffer to store data before writing to the stream

                // Loop through each tile in the world by height (y) and width (x)
                for (int y = 0; y < world.TilesHigh; y++)
                {
                    // Do progress loading here.

                    for (int x = 0; x < world.TilesWide; x++)
                    {
                        try
                        {
                            // Initialize bytes for color, flags, and light
                            byte colorByte = 0;
                            byte flagByte = 0;

                            // Flags to check if tile has light or extra data
                            bool hasLight = true;
                            bool hasExtraData = true;

                            // Initialize color and tileData
                            byte paintColor = 0;
                            ushort tileData;

                            // Default color offset for map encoding
                            byte colorOffset = 1;

                            // Check if the tile at (x, y) has liquid
                            if (world.Tiles[x, y].HasLiquid)
                            {
                                paintColor = 0;
                                tileData = MapHelper.tileLookup[326]; // Default to water (WaterfallBlock)

                                // Determine the type of liquid and set the corresponding tileData
                                if (world.Tiles[x, y].LiquidType == LiquidType.Water)
                                {
                                    tileData = MapHelper.tileLookup[326]; // WaterfallBlock
                                }
                                else if (world.Tiles[x, y].LiquidType == LiquidType.Lava)
                                {
                                    tileData = MapHelper.tileLookup[327]; // LavafallBlock
                                }
                                else if (world.Tiles[x, y].LiquidType == LiquidType.Honey)
                                {
                                    tileData = MapHelper.tileLookup[345]; // HoneyfallBlock
                                }
                                else if (world.Tiles[x, y].LiquidType == LiquidType.Shimmer)
                                {
                                    tileData = MapHelper.tileLookup[447]; // SillyBalloonPurple
                                }
                            }
                            // Check if the tile at (x, y) is active
                            else if (world.Tiles[x, y].IsActive)
                            {
                                paintColor = world.Tiles[x, y].TileColor;
                                tileData = MapHelper.tileLookup[world.Tiles[x, y].Type];
                            }
                            // Check if the tile at (x, y) has a wall
                            else if (world.Tiles[x, y].Wall > 0)
                            {
                                paintColor = world.Tiles[x, y].WallColor;
                                tileData = MapHelper.wallLookup[world.Tiles[x, y].Wall];
                            }
                            // Handle tiles with no specific type (empty tiles)
                            else
                            {
                                paintColor = 0;
                                tileData = MapHelper.tileLookup[350]; // Default to Space (MartianConduitPlating)

                                // Determine the layer and set appropriate tileData
                                if (y <= world.GroundLevel * 0.3499999940395355) // Space Layer
                                {
                                    tileData = MapHelper.tileLookup[350]; // MartianConduitPlating
                                }
                                else if (y <= world.GroundLevel && y > world.GroundLevel * 0.3499999940395355) // Sky Layer
                                {
                                    tileData = MapHelper.tileLookup[126]; // DiscoBall
                                }
                                else if (y >= world.GroundLevel && y < world.RockLevel) // Ground Layer
                                {
                                    tileData = MapHelper.wallLookup[2]; // DirtUnsafe
                                }
                                else if (y >= world.RockLevel && y < (world.TilesHigh - 200)) // Cavern Layer
                                {
                                    tileData = MapHelper.wallLookup[1]; // StoneWall
                                }
                                else if (y >= (world.TilesHigh - 200)) // Underground Layer
                                {
                                    tileData = MapHelper.tileLookup[234]; // Crimson Sand // TE - Wall:87 LihzahrdBrickUnsafe
                                }
                            }

                            // Populate flag and color bytes based on the tile properties
                            if (paintColor > 0)
                            {
                                flagByte |= (byte)(paintColor << 1);
                            }

                            if (flagByte != 0)
                            {
                                colorByte |= 1;
                            }

                            // Adjust colorByte for additional flags
                            colorByte |= (byte)(colorOffset << 1);

                            if (hasExtraData)
                            {
                                colorByte |= 32;
                            }

                            // Extend data if tileData id is over 255
                            if (tileData > 255)
                            {
                                colorByte |= 16;
                            }

                            // Write colorByte to buffer
                            if (hasExtraData)
                            {
                                buffer[bufferPosition] = colorByte;
                                bufferPosition++;
                            }

                            // Write flagByte to buffer if it's not zero
                            if (flagByte != 0)
                            {
                                buffer[bufferPosition] = flagByte;
                                bufferPosition++;
                            }

                            // Write tileData to buffer if hasLight is true
                            if (hasLight)
                            {
                                buffer[bufferPosition] = (byte)tileData;
                                bufferPosition++;
                                if (tileData > 255)
                                {
                                    buffer[bufferPosition] = (byte)(tileData >> 8);
                                    bufferPosition++;
                                }
                            }

                            // Write extra data to buffer if hasExtraData is true
                            if (hasExtraData)
                            {
                                buffer[bufferPosition] = 255; // This seems to represent max light
                                bufferPosition++;
                            }

                            // Write buffer to deflateStream if buffer is full
                            if (bufferPosition >= 4096)
                            {
                                deflateStream.Write(buffer, 0, bufferPosition);
                                bufferPosition = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Display error message if an exception occurs
                            MessageBox.Show(ex.ToString());
                        }
                    }
                }

                // Write remaining buffer data to deflateStream
                if (bufferPosition > 0)
                {
                    deflateStream.Write(buffer, 0, bufferPosition);
                }
            }
            #endregion
        }

        public static class MapHelper
        {
            // These arrays are placeholders for storing counts of tile and wall options
            public static int[] tileOptionCounts;
            public static int[] wallOptionCounts;
            public static ushort[] tileLookup;
            public static ushort[] wallLookup;

            // Initialize the tile and wall option counts with maximum possible IDs
            public static void Initialize(int maxTileID, int maxWallID)
            {
                try
                {
                    #region Define Tile Color Table

                    // "Array"
                    Color[][] tileArray = new Color[(int)maxTileID][];
                    for (int i = 0; i < (int)maxTileID; i++)
                    {
                        tileArray[i] = new Color[12];
                    }
                    tileArray[656][0] = new Color(21, 124, 212);
                    tileArray[624][0] = new Color(210, 91, 77);
                    tileArray[621][0] = new Color(250, 250, 250);
                    tileArray[622][0] = new Color(235, 235, 249);
                    tileArray[518][0] = new Color(26, 196, 84);
                    tileArray[518][1] = new Color(48, 208, 234);
                    tileArray[518][2] = new Color(135, 196, 26);
                    tileArray[519][0] = new Color(28, 216, 109);
                    tileArray[519][1] = new Color(107, 182, 0);
                    tileArray[519][2] = new Color(75, 184, 230);
                    tileArray[519][3] = new Color(208, 80, 80);
                    tileArray[519][4] = new Color(141, 137, 223);
                    tileArray[519][5] = new Color(182, 175, 130);
                    tileArray[549][0] = new Color(54, 83, 20);
                    tileArray[528][0] = new Color(182, 175, 130);
                    tileArray[529][0] = new Color(99, 150, 8);
                    tileArray[529][1] = new Color(139, 154, 64);
                    tileArray[529][2] = new Color(34, 129, 168);
                    tileArray[529][3] = new Color(180, 82, 82);
                    tileArray[529][4] = new Color(113, 108, 205);
                    Color color = new(151, 107, 75);
                    tileArray[0][0] = color;
                    tileArray[668][0] = color;
                    tileArray[5][0] = color;
                    tileArray[5][1] = new Color(182, 175, 130);
                    Color color2 = new(127, 127, 127);
                    tileArray[583][0] = color2;
                    tileArray[584][0] = color2;
                    tileArray[585][0] = color2;
                    tileArray[586][0] = color2;
                    tileArray[587][0] = color2;
                    tileArray[588][0] = color2;
                    tileArray[589][0] = color2;
                    tileArray[590][0] = color2;
                    tileArray[595][0] = color;
                    tileArray[596][0] = color;
                    tileArray[615][0] = color;
                    tileArray[616][0] = color;
                    tileArray[634][0] = new Color(145, 120, 120);
                    tileArray[633][0] = new Color(210, 140, 100);
                    tileArray[637][0] = new Color(200, 120, 75);
                    tileArray[638][0] = new Color(200, 120, 75);
                    tileArray[30][0] = color;
                    tileArray[191][0] = color;
                    tileArray[272][0] = new Color(121, 119, 101);
                    color = new Color(128, 128, 128);
                    tileArray[1][0] = color;
                    tileArray[38][0] = color;
                    tileArray[48][0] = color;
                    tileArray[130][0] = color;
                    tileArray[138][0] = color;
                    tileArray[664][0] = color;
                    tileArray[273][0] = color;
                    tileArray[283][0] = color;
                    tileArray[618][0] = color;
                    tileArray[654][0] = new Color(200, 44, 28);
                    tileArray[2][0] = new Color(28, 216, 94);
                    tileArray[477][0] = new Color(28, 216, 94);
                    tileArray[492][0] = new Color(78, 193, 227);
                    color = new Color(26, 196, 84);
                    tileArray[3][0] = color;
                    tileArray[192][0] = color;
                    tileArray[73][0] = new Color(27, 197, 109);
                    tileArray[52][0] = new Color(23, 177, 76);
                    tileArray[353][0] = new Color(28, 216, 94);
                    tileArray[20][0] = new Color(163, 116, 81);
                    tileArray[6][0] = new Color(140, 101, 80);
                    color = new Color(150, 67, 22);
                    tileArray[7][0] = color;
                    tileArray[47][0] = color;
                    tileArray[284][0] = color;
                    tileArray[682][0] = color;
                    tileArray[560][0] = color;
                    color = new Color(185, 164, 23);
                    tileArray[8][0] = color;
                    tileArray[45][0] = color;
                    tileArray[680][0] = color;
                    tileArray[560][2] = color;
                    color = new Color(185, 194, 195);
                    tileArray[9][0] = color;
                    tileArray[46][0] = color;
                    tileArray[681][0] = color;
                    tileArray[560][1] = color;
                    color = new Color(98, 95, 167);
                    tileArray[22][0] = color;
                    tileArray[140][0] = color;
                    tileArray[23][0] = new Color(141, 137, 223);
                    tileArray[24][0] = new Color(122, 116, 218);
                    tileArray[636][0] = new Color(122, 116, 218);
                    tileArray[25][0] = new Color(109, 90, 128);
                    tileArray[37][0] = new Color(104, 86, 84);
                    tileArray[39][0] = new Color(181, 62, 59);
                    tileArray[40][0] = new Color(146, 81, 68);
                    tileArray[41][0] = new Color(66, 84, 109);
                    tileArray[677][0] = new Color(66, 84, 109);
                    tileArray[481][0] = new Color(66, 84, 109);
                    tileArray[43][0] = new Color(84, 100, 63);
                    tileArray[678][0] = new Color(84, 100, 63);
                    tileArray[482][0] = new Color(84, 100, 63);
                    tileArray[44][0] = new Color(107, 68, 99);
                    tileArray[679][0] = new Color(107, 68, 99);
                    tileArray[483][0] = new Color(107, 68, 99);
                    tileArray[53][0] = new Color(186, 168, 84);
                    color = new Color(190, 171, 94);
                    tileArray[151][0] = color;
                    tileArray[154][0] = color;
                    tileArray[274][0] = color;
                    tileArray[328][0] = new Color(200, 246, 254);
                    tileArray[329][0] = new Color(15, 15, 15);
                    tileArray[54][0] = new Color(200, 246, 254);
                    tileArray[56][0] = new Color(43, 40, 84);
                    tileArray[75][0] = new Color(26, 26, 26);
                    tileArray[683][0] = new Color(100, 90, 190);
                    tileArray[57][0] = new Color(68, 68, 76);
                    color = new Color(142, 66, 66);
                    tileArray[58][0] = color;
                    tileArray[76][0] = color;
                    tileArray[684][0] = color;
                    color = new Color(92, 68, 73);
                    tileArray[59][0] = color;
                    tileArray[120][0] = color;
                    tileArray[60][0] = new Color(143, 215, 29);
                    tileArray[61][0] = new Color(135, 196, 26);
                    tileArray[74][0] = new Color(96, 197, 27);
                    tileArray[62][0] = new Color(121, 176, 24);
                    tileArray[233][0] = new Color(107, 182, 29);
                    tileArray[652][0] = tileArray[233][0];
                    tileArray[651][0] = tileArray[233][0];
                    tileArray[63][0] = new Color(110, 140, 182);
                    tileArray[64][0] = new Color(196, 96, 114);
                    tileArray[65][0] = new Color(56, 150, 97);
                    tileArray[66][0] = new Color(160, 118, 58);
                    tileArray[67][0] = new Color(140, 58, 166);
                    tileArray[68][0] = new Color(125, 191, 197);
                    tileArray[566][0] = new Color(233, 180, 90);
                    tileArray[70][0] = new Color(93, 127, 255);
                    color = new Color(182, 175, 130);
                    tileArray[71][0] = color;
                    tileArray[72][0] = color;
                    tileArray[190][0] = color;
                    tileArray[578][0] = new Color(172, 155, 110);
                    color = new Color(73, 120, 17);
                    tileArray[80][0] = color;
                    tileArray[484][0] = color;
                    tileArray[188][0] = color;
                    tileArray[80][1] = new Color(87, 84, 151);
                    tileArray[80][2] = new Color(34, 129, 168);
                    tileArray[80][3] = new Color(130, 56, 55);
                    color = new Color(11, 80, 143);
                    tileArray[107][0] = color;
                    tileArray[121][0] = color;
                    tileArray[685][0] = color;
                    color = new Color(91, 169, 169);
                    tileArray[108][0] = color;
                    tileArray[122][0] = color;
                    tileArray[686][0] = color;
                    color = new Color(128, 26, 52);
                    tileArray[111][0] = color;
                    tileArray[150][0] = color;
                    tileArray[109][0] = new Color(78, 193, 227);
                    tileArray[110][0] = new Color(48, 186, 135);
                    tileArray[113][0] = new Color(48, 208, 234);
                    tileArray[115][0] = new Color(33, 171, 207);
                    tileArray[112][0] = new Color(103, 98, 122);
                    color = new Color(238, 225, 218);
                    tileArray[116][0] = color;
                    tileArray[118][0] = color;
                    tileArray[117][0] = new Color(181, 172, 190);
                    tileArray[119][0] = new Color(107, 92, 108);
                    tileArray[123][0] = new Color(106, 107, 118);
                    tileArray[124][0] = new Color(73, 51, 36);
                    tileArray[131][0] = new Color(52, 52, 52);
                    tileArray[145][0] = new Color(192, 30, 30);
                    tileArray[146][0] = new Color(43, 192, 30);
                    color = new Color(211, 236, 241);
                    tileArray[147][0] = color;
                    tileArray[148][0] = color;
                    tileArray[152][0] = new Color(128, 133, 184);
                    tileArray[153][0] = new Color(239, 141, 126);
                    tileArray[155][0] = new Color(131, 162, 161);
                    tileArray[156][0] = new Color(170, 171, 157);
                    tileArray[157][0] = new Color(104, 100, 126);
                    color = new Color(145, 81, 85);
                    tileArray[158][0] = color;
                    tileArray[232][0] = color;
                    tileArray[575][0] = new Color(125, 61, 65);
                    tileArray[159][0] = new Color(148, 133, 98);
                    tileArray[161][0] = new Color(144, 195, 232);
                    tileArray[162][0] = new Color(184, 219, 240);
                    tileArray[163][0] = new Color(174, 145, 214);
                    tileArray[164][0] = new Color(218, 182, 204);
                    tileArray[170][0] = new Color(27, 109, 69);
                    tileArray[171][0] = new Color(33, 135, 85);
                    color = new Color(129, 125, 93);
                    tileArray[166][0] = color;
                    tileArray[175][0] = color;
                    tileArray[167][0] = new Color(62, 82, 114);
                    color = new Color(132, 157, 127);
                    tileArray[168][0] = color;
                    tileArray[176][0] = color;
                    color = new Color(152, 171, 198);
                    tileArray[169][0] = color;
                    tileArray[177][0] = color;
                    tileArray[179][0] = new Color(49, 134, 114);
                    tileArray[180][0] = new Color(126, 134, 49);
                    tileArray[181][0] = new Color(134, 59, 49);
                    tileArray[182][0] = new Color(43, 86, 140);
                    tileArray[183][0] = new Color(121, 49, 134);
                    tileArray[381][0] = new Color(254, 121, 2);
                    tileArray[687][0] = new Color(254, 121, 2);
                    tileArray[534][0] = new Color(114, 254, 2);
                    tileArray[689][0] = new Color(114, 254, 2);
                    tileArray[536][0] = new Color(0, 197, 208);
                    tileArray[690][0] = new Color(0, 197, 208);
                    tileArray[539][0] = new Color(208, 0, 126);
                    tileArray[688][0] = new Color(208, 0, 126);
                    tileArray[625][0] = new Color(220, 12, 237);
                    tileArray[691][0] = new Color(220, 12, 237);
                    tileArray[627][0] = new Color(255, 76, 76);
                    tileArray[627][1] = new Color(255, 195, 76);
                    tileArray[627][2] = new Color(195, 255, 76);
                    tileArray[627][3] = new Color(76, 255, 76);
                    tileArray[627][4] = new Color(76, 255, 195);
                    tileArray[627][5] = new Color(76, 195, 255);
                    tileArray[627][6] = new Color(77, 76, 255);
                    tileArray[627][7] = new Color(196, 76, 255);
                    tileArray[627][8] = new Color(255, 76, 195);
                    tileArray[512][0] = new Color(49, 134, 114);
                    tileArray[513][0] = new Color(126, 134, 49);
                    tileArray[514][0] = new Color(134, 59, 49);
                    tileArray[515][0] = new Color(43, 86, 140);
                    tileArray[516][0] = new Color(121, 49, 134);
                    tileArray[517][0] = new Color(254, 121, 2);
                    tileArray[535][0] = new Color(114, 254, 2);
                    tileArray[537][0] = new Color(0, 197, 208);
                    tileArray[540][0] = new Color(208, 0, 126);
                    tileArray[626][0] = new Color(220, 12, 237);
                    for (int j = 0; j < tileArray[628].Length; j++)
                    {
                        tileArray[628][j] = tileArray[627][j];
                    }
                    for (int k = 0; k < tileArray[692].Length; k++)
                    {
                        tileArray[692][k] = tileArray[627][k];
                    }
                    for (int l = 0; l < tileArray[160].Length; l++)
                    {
                        tileArray[160][l] = tileArray[627][l];
                    }
                    tileArray[184][0] = new Color(29, 106, 88);
                    tileArray[184][1] = new Color(94, 100, 36);
                    tileArray[184][2] = new Color(96, 44, 40);
                    tileArray[184][3] = new Color(34, 63, 102);
                    tileArray[184][4] = new Color(79, 35, 95);
                    tileArray[184][5] = new Color(253, 62, 3);
                    tileArray[184][6] = new Color(22, 123, 62);
                    tileArray[184][7] = new Color(0, 106, 148);
                    tileArray[184][8] = new Color(148, 0, 132);
                    tileArray[184][9] = new Color(122, 24, 168);
                    tileArray[184][10] = new Color(220, 20, 20);
                    tileArray[189][0] = new Color(223, 255, 255);
                    tileArray[193][0] = new Color(56, 121, 255);
                    tileArray[194][0] = new Color(157, 157, 107);
                    tileArray[195][0] = new Color(134, 22, 34);
                    tileArray[196][0] = new Color(147, 144, 178);
                    tileArray[197][0] = new Color(97, 200, 225);
                    tileArray[198][0] = new Color(62, 61, 52);
                    tileArray[199][0] = new Color(208, 80, 80);
                    tileArray[201][0] = new Color(203, 61, 64);
                    tileArray[205][0] = new Color(186, 50, 52);
                    tileArray[200][0] = new Color(216, 152, 144);
                    tileArray[202][0] = new Color(213, 178, 28);
                    tileArray[203][0] = new Color(128, 44, 45);
                    tileArray[204][0] = new Color(125, 55, 65);
                    tileArray[206][0] = new Color(124, 175, 201);
                    tileArray[208][0] = new Color(88, 105, 118);
                    tileArray[211][0] = new Color(191, 233, 115);
                    tileArray[213][0] = new Color(137, 120, 67);
                    tileArray[214][0] = new Color(103, 103, 103);
                    tileArray[221][0] = new Color(239, 90, 50);
                    tileArray[222][0] = new Color(231, 96, 228);
                    tileArray[223][0] = new Color(57, 85, 101);
                    tileArray[224][0] = new Color(107, 132, 139);
                    tileArray[225][0] = new Color(227, 125, 22);
                    tileArray[226][0] = new Color(141, 56, 0);
                    tileArray[229][0] = new Color(255, 156, 12);
                    tileArray[659][0] = new Color(247, 228, 254);
                    tileArray[230][0] = new Color(131, 79, 13);
                    tileArray[234][0] = new Color(53, 44, 41);
                    tileArray[235][0] = new Color(214, 184, 46);
                    tileArray[236][0] = new Color(149, 232, 87);
                    tileArray[237][0] = new Color(255, 241, 51);
                    tileArray[238][0] = new Color(225, 128, 206);
                    tileArray[655][0] = new Color(225, 128, 206);
                    tileArray[243][0] = new Color(198, 196, 170);
                    tileArray[248][0] = new Color(219, 71, 38);
                    tileArray[249][0] = new Color(235, 38, 231);
                    tileArray[250][0] = new Color(86, 85, 92);
                    tileArray[251][0] = new Color(235, 150, 23);
                    tileArray[252][0] = new Color(153, 131, 44);
                    tileArray[253][0] = new Color(57, 48, 97);
                    tileArray[254][0] = new Color(248, 158, 92);
                    tileArray[255][0] = new Color(107, 49, 154);
                    tileArray[256][0] = new Color(154, 148, 49);
                    tileArray[257][0] = new Color(49, 49, 154);
                    tileArray[258][0] = new Color(49, 154, 68);
                    tileArray[259][0] = new Color(154, 49, 77);
                    tileArray[260][0] = new Color(85, 89, 118);
                    tileArray[261][0] = new Color(154, 83, 49);
                    tileArray[262][0] = new Color(221, 79, 255);
                    tileArray[263][0] = new Color(250, 255, 79);
                    tileArray[264][0] = new Color(79, 102, 255);
                    tileArray[265][0] = new Color(79, 255, 89);
                    tileArray[266][0] = new Color(255, 79, 79);
                    tileArray[267][0] = new Color(240, 240, 247);
                    tileArray[268][0] = new Color(255, 145, 79);
                    tileArray[287][0] = new Color(79, 128, 17);
                    color = new Color(122, 217, 232);
                    tileArray[275][0] = color;
                    tileArray[276][0] = color;
                    tileArray[277][0] = color;
                    tileArray[278][0] = color;
                    tileArray[279][0] = color;
                    tileArray[280][0] = color;
                    tileArray[281][0] = color;
                    tileArray[282][0] = color;
                    tileArray[285][0] = color;
                    tileArray[286][0] = color;
                    tileArray[288][0] = color;
                    tileArray[289][0] = color;
                    tileArray[290][0] = color;
                    tileArray[291][0] = color;
                    tileArray[292][0] = color;
                    tileArray[293][0] = color;
                    tileArray[294][0] = color;
                    tileArray[295][0] = color;
                    tileArray[296][0] = color;
                    tileArray[297][0] = color;
                    tileArray[298][0] = color;
                    tileArray[299][0] = color;
                    tileArray[309][0] = color;
                    tileArray[310][0] = color;
                    tileArray[413][0] = color;
                    tileArray[339][0] = color;
                    tileArray[542][0] = color;
                    tileArray[632][0] = color;
                    tileArray[640][0] = color;
                    tileArray[643][0] = color;
                    tileArray[644][0] = color;
                    tileArray[645][0] = color;
                    tileArray[358][0] = color;
                    tileArray[359][0] = color;
                    tileArray[360][0] = color;
                    tileArray[361][0] = color;
                    tileArray[362][0] = color;
                    tileArray[363][0] = color;
                    tileArray[364][0] = color;
                    tileArray[391][0] = color;
                    tileArray[392][0] = color;
                    tileArray[393][0] = color;
                    tileArray[394][0] = color;
                    tileArray[414][0] = color;
                    tileArray[505][0] = color;
                    tileArray[543][0] = color;
                    tileArray[598][0] = color;
                    tileArray[521][0] = color;
                    tileArray[522][0] = color;
                    tileArray[523][0] = color;
                    tileArray[524][0] = color;
                    tileArray[525][0] = color;
                    tileArray[526][0] = color;
                    tileArray[527][0] = color;
                    tileArray[532][0] = color;
                    tileArray[533][0] = color;
                    tileArray[538][0] = color;
                    tileArray[544][0] = color;
                    tileArray[629][0] = color;
                    tileArray[550][0] = color;
                    tileArray[551][0] = color;
                    tileArray[553][0] = color;
                    tileArray[554][0] = color;
                    tileArray[555][0] = color;
                    tileArray[556][0] = color;
                    tileArray[558][0] = color;
                    tileArray[559][0] = color;
                    tileArray[580][0] = color;
                    tileArray[582][0] = color;
                    tileArray[599][0] = color;
                    tileArray[600][0] = color;
                    tileArray[601][0] = color;
                    tileArray[602][0] = color;
                    tileArray[603][0] = color;
                    tileArray[604][0] = color;
                    tileArray[605][0] = color;
                    tileArray[606][0] = color;
                    tileArray[607][0] = color;
                    tileArray[608][0] = color;
                    tileArray[609][0] = color;
                    tileArray[610][0] = color;
                    tileArray[611][0] = color;
                    tileArray[612][0] = color;
                    tileArray[619][0] = color;
                    tileArray[620][0] = color;
                    tileArray[630][0] = new Color(117, 145, 73);
                    tileArray[631][0] = new Color(122, 234, 225);
                    tileArray[552][0] = tileArray[53][0];
                    tileArray[564][0] = new Color(87, 127, 220);
                    tileArray[408][0] = new Color(85, 83, 82);
                    tileArray[409][0] = new Color(85, 83, 82);
                    tileArray[669][0] = new Color(83, 46, 57);
                    tileArray[670][0] = new Color(91, 87, 167);
                    tileArray[671][0] = new Color(23, 33, 81);
                    tileArray[672][0] = new Color(53, 133, 103);
                    tileArray[673][0] = new Color(11, 67, 80);
                    tileArray[674][0] = new Color(40, 49, 60);
                    tileArray[675][0] = new Color(21, 13, 77);
                    tileArray[676][0] = new Color(195, 201, 215);
                    tileArray[415][0] = new Color(249, 75, 7);
                    tileArray[416][0] = new Color(0, 160, 170);
                    tileArray[417][0] = new Color(160, 87, 234);
                    tileArray[418][0] = new Color(22, 173, 254);
                    tileArray[489][0] = new Color(255, 29, 136);
                    tileArray[490][0] = new Color(211, 211, 211);
                    tileArray[311][0] = new Color(117, 61, 25);
                    tileArray[312][0] = new Color(204, 93, 73);
                    tileArray[313][0] = new Color(87, 150, 154);
                    tileArray[4][0] = new Color(253, 221, 3);
                    tileArray[4][1] = new Color(253, 221, 3);
                    color = new Color(253, 221, 3);
                    tileArray[93][0] = color;
                    tileArray[33][0] = color;
                    tileArray[174][0] = color;
                    tileArray[100][0] = color;
                    tileArray[98][0] = color;
                    tileArray[173][0] = color;
                    color = new Color(119, 105, 79);
                    tileArray[11][0] = color;
                    tileArray[10][0] = color;
                    tileArray[593][0] = color;
                    tileArray[594][0] = color;
                    color = new Color(191, 142, 111);
                    tileArray[14][0] = color;
                    tileArray[469][0] = color;
                    tileArray[486][0] = color;
                    tileArray[488][0] = new Color(127, 92, 69);
                    tileArray[487][0] = color;
                    tileArray[487][1] = color;
                    tileArray[15][0] = color;
                    tileArray[15][1] = color;
                    tileArray[497][0] = color;
                    tileArray[18][0] = color;
                    tileArray[19][0] = color;
                    tileArray[19][1] = Color.Black;
                    tileArray[55][0] = color;
                    tileArray[79][0] = color;
                    tileArray[86][0] = color;
                    tileArray[87][0] = color;
                    tileArray[88][0] = color;
                    tileArray[89][0] = color;
                    tileArray[89][1] = color;
                    tileArray[89][2] = new Color(105, 107, 125);
                    tileArray[94][0] = color;
                    tileArray[101][0] = color;
                    tileArray[104][0] = color;
                    tileArray[106][0] = color;
                    tileArray[114][0] = color;
                    tileArray[128][0] = color;
                    tileArray[139][0] = color;
                    tileArray[172][0] = color;
                    tileArray[216][0] = color;
                    tileArray[269][0] = color;
                    tileArray[334][0] = color;
                    tileArray[471][0] = color;
                    tileArray[470][0] = color;
                    tileArray[475][0] = color;
                    tileArray[377][0] = color;
                    tileArray[380][0] = color;
                    tileArray[395][0] = color;
                    tileArray[573][0] = color;
                    tileArray[12][0] = new Color(174, 24, 69);
                    tileArray[665][0] = new Color(174, 24, 69);
                    tileArray[639][0] = new Color(110, 105, 255);
                    tileArray[13][0] = new Color(133, 213, 247);
                    color = new Color(144, 148, 144);
                    tileArray[17][0] = color;
                    tileArray[90][0] = color;
                    tileArray[96][0] = color;
                    tileArray[97][0] = color;
                    tileArray[99][0] = color;
                    tileArray[132][0] = color;
                    tileArray[142][0] = color;
                    tileArray[143][0] = color;
                    tileArray[144][0] = color;
                    tileArray[207][0] = color;
                    tileArray[209][0] = color;
                    tileArray[212][0] = color;
                    tileArray[217][0] = color;
                    tileArray[218][0] = color;
                    tileArray[219][0] = color;
                    tileArray[220][0] = color;
                    tileArray[228][0] = color;
                    tileArray[300][0] = color;
                    tileArray[301][0] = color;
                    tileArray[302][0] = color;
                    tileArray[303][0] = color;
                    tileArray[304][0] = color;
                    tileArray[305][0] = color;
                    tileArray[306][0] = color;
                    tileArray[307][0] = color;
                    tileArray[308][0] = color;
                    tileArray[567][0] = color;
                    tileArray[349][0] = new Color(144, 148, 144);
                    tileArray[531][0] = new Color(144, 148, 144);
                    tileArray[105][0] = new Color(144, 148, 144);
                    tileArray[105][1] = new Color(177, 92, 31);
                    tileArray[105][2] = new Color(201, 188, 170);
                    tileArray[137][0] = new Color(144, 148, 144);
                    tileArray[137][1] = new Color(141, 56, 0);
                    tileArray[137][2] = new Color(144, 148, 144);
                    tileArray[16][0] = new Color(140, 130, 116);
                    tileArray[26][0] = new Color(119, 101, 125);
                    tileArray[26][1] = new Color(214, 127, 133);
                    tileArray[36][0] = new Color(230, 89, 92);
                    tileArray[28][0] = new Color(151, 79, 80);
                    tileArray[28][1] = new Color(90, 139, 140);
                    tileArray[28][2] = new Color(192, 136, 70);
                    tileArray[28][3] = new Color(203, 185, 151);
                    tileArray[28][4] = new Color(73, 56, 41);
                    tileArray[28][5] = new Color(148, 159, 67);
                    tileArray[28][6] = new Color(138, 172, 67);
                    tileArray[28][7] = new Color(226, 122, 47);
                    tileArray[28][8] = new Color(198, 87, 93);
                    for (int m = 0; m < tileArray[653].Length; m++)
                    {
                        tileArray[653][m] = tileArray[28][m];
                    }
                    tileArray[29][0] = new Color(175, 105, 128);
                    tileArray[51][0] = new Color(192, 202, 203);
                    tileArray[31][0] = new Color(141, 120, 168);
                    tileArray[31][1] = new Color(212, 105, 105);
                    tileArray[32][0] = new Color(151, 135, 183);
                    tileArray[42][0] = new Color(251, 235, 127);
                    tileArray[50][0] = new Color(170, 48, 114);
                    tileArray[85][0] = new Color(192, 192, 192);
                    tileArray[69][0] = new Color(190, 150, 92);
                    tileArray[77][0] = new Color(238, 85, 70);
                    tileArray[81][0] = new Color(245, 133, 191);
                    tileArray[78][0] = new Color(121, 110, 97);
                    tileArray[141][0] = new Color(192, 59, 59);
                    tileArray[129][0] = new Color(255, 117, 224);
                    tileArray[129][1] = new Color(255, 117, 224);
                    tileArray[126][0] = new Color(159, 209, 229);
                    tileArray[125][0] = new Color(141, 175, 255);
                    tileArray[103][0] = new Color(141, 98, 77);
                    tileArray[95][0] = new Color(255, 162, 31);
                    tileArray[92][0] = new Color(213, 229, 237);
                    tileArray[91][0] = new Color(13, 88, 130);
                    tileArray[215][0] = new Color(254, 121, 2);
                    tileArray[592][0] = new Color(254, 121, 2);
                    tileArray[316][0] = new Color(157, 176, 226);
                    tileArray[317][0] = new Color(118, 227, 129);
                    tileArray[318][0] = new Color(227, 118, 215);
                    tileArray[319][0] = new Color(96, 68, 48);
                    tileArray[320][0] = new Color(203, 185, 151);
                    tileArray[321][0] = new Color(96, 77, 64);
                    tileArray[574][0] = new Color(76, 57, 44);
                    tileArray[322][0] = new Color(198, 170, 104);
                    tileArray[635][0] = new Color(145, 120, 120);
                    tileArray[149][0] = new Color(220, 50, 50);
                    tileArray[149][1] = new Color(0, 220, 50);
                    tileArray[149][2] = new Color(50, 50, 220);
                    tileArray[133][0] = new Color(231, 53, 56);
                    tileArray[133][1] = new Color(192, 189, 221);
                    tileArray[134][0] = new Color(166, 187, 153);
                    tileArray[134][1] = new Color(241, 129, 249);
                    tileArray[102][0] = new Color(229, 212, 73);
                    tileArray[35][0] = new Color(226, 145, 30);
                    tileArray[34][0] = new Color(235, 166, 135);
                    tileArray[136][0] = new Color(213, 203, 204);
                    tileArray[231][0] = new Color(224, 194, 101);
                    tileArray[239][0] = new Color(224, 194, 101);
                    tileArray[240][0] = new Color(120, 85, 60);
                    tileArray[240][1] = new Color(99, 50, 30);
                    tileArray[240][2] = new Color(153, 153, 117);
                    tileArray[240][3] = new Color(112, 84, 56);
                    tileArray[240][4] = new Color(234, 231, 226);
                    tileArray[241][0] = new Color(77, 74, 72);
                    tileArray[244][0] = new Color(200, 245, 253);
                    color = new Color(99, 50, 30);
                    tileArray[242][0] = color;
                    tileArray[245][0] = color;
                    tileArray[246][0] = color;
                    tileArray[242][1] = new Color(185, 142, 97);
                    tileArray[247][0] = new Color(140, 150, 150);
                    tileArray[271][0] = new Color(107, 250, 255);
                    tileArray[270][0] = new Color(187, 255, 107);
                    tileArray[581][0] = new Color(255, 150, 150);
                    tileArray[660][0] = new Color(255, 150, 150);
                    tileArray[572][0] = new Color(255, 186, 212);
                    tileArray[572][1] = new Color(209, 201, 255);
                    tileArray[572][2] = new Color(200, 254, 255);
                    tileArray[572][3] = new Color(199, 255, 211);
                    tileArray[572][4] = new Color(180, 209, 255);
                    tileArray[572][5] = new Color(255, 220, 214);
                    tileArray[314][0] = new Color(181, 164, 125);
                    tileArray[324][0] = new Color(228, 213, 173);
                    tileArray[351][0] = new Color(31, 31, 31);
                    tileArray[424][0] = new Color(146, 155, 187);
                    tileArray[429][0] = new Color(220, 220, 220);
                    tileArray[445][0] = new Color(240, 240, 240);
                    tileArray[21][0] = new Color(174, 129, 92);
                    tileArray[21][1] = new Color(233, 207, 94);
                    tileArray[21][2] = new Color(137, 128, 200);
                    tileArray[21][3] = new Color(160, 160, 160);
                    tileArray[21][4] = new Color(106, 210, 255);
                    tileArray[441][0] = tileArray[21][0];
                    tileArray[441][1] = tileArray[21][1];
                    tileArray[441][2] = tileArray[21][2];
                    tileArray[441][3] = tileArray[21][3];
                    tileArray[441][4] = tileArray[21][4];
                    tileArray[27][0] = new Color(54, 154, 54);
                    tileArray[27][1] = new Color(226, 196, 49);
                    color = new Color(246, 197, 26);
                    tileArray[82][0] = color;
                    tileArray[83][0] = color;
                    tileArray[84][0] = color;
                    color = new Color(76, 150, 216);
                    tileArray[82][1] = color;
                    tileArray[83][1] = color;
                    tileArray[84][1] = color;
                    color = new Color(185, 214, 42);
                    tileArray[82][2] = color;
                    tileArray[83][2] = color;
                    tileArray[84][2] = color;
                    color = new Color(167, 203, 37);
                    tileArray[82][3] = color;
                    tileArray[83][3] = color;
                    tileArray[84][3] = color;
                    tileArray[591][6] = color;
                    color = new Color(32, 168, 117);
                    tileArray[82][4] = color;
                    tileArray[83][4] = color;
                    tileArray[84][4] = color;
                    color = new Color(177, 69, 49);
                    tileArray[82][5] = color;
                    tileArray[83][5] = color;
                    tileArray[84][5] = color;
                    color = new Color(40, 152, 240);
                    tileArray[82][6] = color;
                    tileArray[83][6] = color;
                    tileArray[84][6] = color;
                    tileArray[591][1] = new Color(246, 197, 26);
                    tileArray[591][2] = new Color(76, 150, 216);
                    tileArray[591][3] = new Color(32, 168, 117);
                    tileArray[591][4] = new Color(40, 152, 240);
                    tileArray[591][5] = new Color(114, 81, 56);
                    tileArray[591][6] = new Color(141, 137, 223);
                    tileArray[591][7] = new Color(208, 80, 80);
                    tileArray[591][8] = new Color(177, 69, 49);
                    tileArray[165][0] = new Color(115, 173, 229);
                    tileArray[165][1] = new Color(100, 100, 100);
                    tileArray[165][2] = new Color(152, 152, 152);
                    tileArray[165][3] = new Color(227, 125, 22);
                    tileArray[178][0] = new Color(208, 94, 201);
                    tileArray[178][1] = new Color(233, 146, 69);
                    tileArray[178][2] = new Color(71, 146, 251);
                    tileArray[178][3] = new Color(60, 226, 133);
                    tileArray[178][4] = new Color(250, 30, 71);
                    tileArray[178][5] = new Color(166, 176, 204);
                    tileArray[178][6] = new Color(255, 217, 120);
                    color = new Color(99, 99, 99);
                    tileArray[185][0] = color;
                    tileArray[186][0] = color;
                    tileArray[187][0] = color;
                    tileArray[565][0] = color;
                    tileArray[579][0] = color;
                    color = new Color(114, 81, 56);
                    tileArray[185][1] = color;
                    tileArray[186][1] = color;
                    tileArray[187][1] = color;
                    tileArray[591][0] = color;
                    color = new Color(133, 133, 101);
                    tileArray[185][2] = color;
                    tileArray[186][2] = color;
                    tileArray[187][2] = color;
                    color = new Color(151, 200, 211);
                    tileArray[185][3] = color;
                    tileArray[186][3] = color;
                    tileArray[187][3] = color;
                    color = new Color(177, 183, 161);
                    tileArray[185][4] = color;
                    tileArray[186][4] = color;
                    tileArray[187][4] = color;
                    color = new Color(134, 114, 38);
                    tileArray[185][5] = color;
                    tileArray[186][5] = color;
                    tileArray[187][5] = color;
                    color = new Color(82, 62, 66);
                    tileArray[185][6] = color;
                    tileArray[186][6] = color;
                    tileArray[187][6] = color;
                    color = new Color(143, 117, 121);
                    tileArray[185][7] = color;
                    tileArray[186][7] = color;
                    tileArray[187][7] = color;
                    color = new Color(177, 92, 31);
                    tileArray[185][8] = color;
                    tileArray[186][8] = color;
                    tileArray[187][8] = color;
                    color = new Color(85, 73, 87);
                    tileArray[185][9] = color;
                    tileArray[186][9] = color;
                    tileArray[187][9] = color;
                    color = new Color(26, 196, 84);
                    tileArray[185][10] = color;
                    tileArray[186][10] = color;
                    tileArray[187][10] = color;
                    Color[] array2 = tileArray[647];
                    for (int n = 0; n < array2.Length; n++)
                    {
                        array2[n] = tileArray[186][n];
                    }
                    array2 = tileArray[648];
                    for (int num = 0; num < array2.Length; num++)
                    {
                        array2[num] = tileArray[187][num];
                    }
                    array2 = tileArray[650];
                    for (int num2 = 0; num2 < array2.Length; num2++)
                    {
                        array2[num2] = tileArray[185][num2];
                    }
                    array2 = tileArray[649];
                    for (int num3 = 0; num3 < array2.Length; num3++)
                    {
                        array2[num3] = tileArray[185][num3];
                    }
                    tileArray[227][0] = new Color(74, 197, 155);
                    tileArray[227][1] = new Color(54, 153, 88);
                    tileArray[227][2] = new Color(63, 126, 207);
                    tileArray[227][3] = new Color(240, 180, 4);
                    tileArray[227][4] = new Color(45, 68, 168);
                    tileArray[227][5] = new Color(61, 92, 0);
                    tileArray[227][6] = new Color(216, 112, 152);
                    tileArray[227][7] = new Color(200, 40, 24);
                    tileArray[227][8] = new Color(113, 45, 133);
                    tileArray[227][9] = new Color(235, 137, 2);
                    tileArray[227][10] = new Color(41, 152, 135);
                    tileArray[227][11] = new Color(198, 19, 78);
                    tileArray[373][0] = new Color(9, 61, 191);
                    tileArray[374][0] = new Color(253, 32, 3);
                    tileArray[375][0] = new Color(255, 156, 12);
                    tileArray[461][0] = new Color(212, 192, 100);
                    tileArray[461][1] = new Color(137, 132, 156);
                    tileArray[461][2] = new Color(148, 122, 112);
                    tileArray[461][3] = new Color(221, 201, 206);
                    tileArray[323][0] = new Color(182, 141, 86);
                    tileArray[325][0] = new Color(129, 125, 93);
                    tileArray[326][0] = new Color(9, 61, 191);
                    tileArray[327][0] = new Color(253, 32, 3);
                    tileArray[507][0] = new Color(5, 5, 5);
                    tileArray[508][0] = new Color(5, 5, 5);
                    tileArray[330][0] = new Color(226, 118, 76);
                    tileArray[331][0] = new Color(161, 172, 173);
                    tileArray[332][0] = new Color(204, 181, 72);
                    tileArray[333][0] = new Color(190, 190, 178);
                    tileArray[335][0] = new Color(217, 174, 137);
                    tileArray[336][0] = new Color(253, 62, 3);
                    tileArray[337][0] = new Color(144, 148, 144);
                    tileArray[338][0] = new Color(85, 255, 160);
                    tileArray[315][0] = new Color(235, 114, 80);
                    tileArray[641][0] = new Color(235, 125, 150);
                    tileArray[340][0] = new Color(96, 248, 2);
                    tileArray[341][0] = new Color(105, 74, 202);
                    tileArray[342][0] = new Color(29, 240, 255);
                    tileArray[343][0] = new Color(254, 202, 80);
                    tileArray[344][0] = new Color(131, 252, 245);
                    tileArray[345][0] = new Color(255, 156, 12);
                    tileArray[346][0] = new Color(149, 212, 89);
                    tileArray[642][0] = new Color(149, 212, 89);
                    tileArray[347][0] = new Color(236, 74, 79);
                    tileArray[348][0] = new Color(44, 26, 233);
                    tileArray[350][0] = new Color(55, 97, 155);
                    tileArray[352][0] = new Color(238, 97, 94);
                    tileArray[354][0] = new Color(141, 107, 89);
                    tileArray[355][0] = new Color(141, 107, 89);
                    tileArray[463][0] = new Color(155, 214, 240);
                    tileArray[491][0] = new Color(60, 20, 160);
                    tileArray[464][0] = new Color(233, 183, 128);
                    tileArray[465][0] = new Color(51, 84, 195);
                    tileArray[466][0] = new Color(205, 153, 73);
                    tileArray[356][0] = new Color(233, 203, 24);
                    tileArray[663][0] = new Color(24, 203, 233);
                    tileArray[357][0] = new Color(168, 178, 204);
                    tileArray[367][0] = new Color(168, 178, 204);
                    tileArray[561][0] = new Color(148, 158, 184);
                    tileArray[365][0] = new Color(146, 136, 205);
                    tileArray[366][0] = new Color(223, 232, 233);
                    tileArray[368][0] = new Color(50, 46, 104);
                    tileArray[369][0] = new Color(50, 46, 104);
                    tileArray[576][0] = new Color(30, 26, 84);
                    tileArray[370][0] = new Color(127, 116, 194);
                    tileArray[49][0] = new Color(89, 201, 255);
                    tileArray[372][0] = new Color(252, 128, 201);
                    tileArray[646][0] = new Color(108, 133, 140);
                    tileArray[371][0] = new Color(249, 101, 189);
                    tileArray[376][0] = new Color(160, 120, 92);
                    tileArray[378][0] = new Color(160, 120, 100);
                    tileArray[379][0] = new Color(251, 209, 240);
                    tileArray[382][0] = new Color(28, 216, 94);
                    tileArray[383][0] = new Color(221, 136, 144);
                    tileArray[384][0] = new Color(131, 206, 12);
                    tileArray[385][0] = new Color(87, 21, 144);
                    tileArray[386][0] = new Color(127, 92, 69);
                    tileArray[387][0] = new Color(127, 92, 69);
                    tileArray[388][0] = new Color(127, 92, 69);
                    tileArray[389][0] = new Color(127, 92, 69);
                    tileArray[390][0] = new Color(253, 32, 3);
                    tileArray[397][0] = new Color(212, 192, 100);
                    tileArray[396][0] = new Color(198, 124, 78);
                    tileArray[577][0] = new Color(178, 104, 58);
                    tileArray[398][0] = new Color(100, 82, 126);
                    tileArray[399][0] = new Color(77, 76, 66);
                    tileArray[400][0] = new Color(96, 68, 117);
                    tileArray[401][0] = new Color(68, 60, 51);
                    tileArray[402][0] = new Color(174, 168, 186);
                    tileArray[403][0] = new Color(205, 152, 186);
                    tileArray[404][0] = new Color(212, 148, 88);
                    tileArray[405][0] = new Color(140, 140, 140);
                    tileArray[406][0] = new Color(120, 120, 120);
                    tileArray[407][0] = new Color(255, 227, 132);
                    tileArray[411][0] = new Color(227, 46, 46);
                    tileArray[494][0] = new Color(227, 227, 227);
                    tileArray[421][0] = new Color(65, 75, 90);
                    tileArray[422][0] = new Color(65, 75, 90);
                    tileArray[425][0] = new Color(146, 155, 187);
                    tileArray[426][0] = new Color(168, 38, 47);
                    tileArray[430][0] = new Color(39, 168, 96);
                    tileArray[431][0] = new Color(39, 94, 168);
                    tileArray[432][0] = new Color(242, 221, 100);
                    tileArray[433][0] = new Color(224, 100, 242);
                    tileArray[434][0] = new Color(197, 193, 216);
                    tileArray[427][0] = new Color(183, 53, 62);
                    tileArray[435][0] = new Color(54, 183, 111);
                    tileArray[436][0] = new Color(54, 109, 183);
                    tileArray[437][0] = new Color(255, 236, 115);
                    tileArray[438][0] = new Color(239, 115, 255);
                    tileArray[439][0] = new Color(212, 208, 231);
                    tileArray[440][0] = new Color(238, 51, 53);
                    tileArray[440][1] = new Color(13, 107, 216);
                    tileArray[440][2] = new Color(33, 184, 115);
                    tileArray[440][3] = new Color(255, 221, 62);
                    tileArray[440][4] = new Color(165, 0, 236);
                    tileArray[440][5] = new Color(223, 230, 238);
                    tileArray[440][6] = new Color(207, 101, 0);
                    tileArray[419][0] = new Color(88, 95, 114);
                    tileArray[419][1] = new Color(214, 225, 236);
                    tileArray[419][2] = new Color(25, 131, 205);
                    tileArray[423][0] = new Color(245, 197, 1);
                    tileArray[423][1] = new Color(185, 0, 224);
                    tileArray[423][2] = new Color(58, 240, 111);
                    tileArray[423][3] = new Color(50, 107, 197);
                    tileArray[423][4] = new Color(253, 91, 3);
                    tileArray[423][5] = new Color(254, 194, 20);
                    tileArray[423][6] = new Color(174, 195, 215);
                    tileArray[420][0] = new Color(99, 255, 107);
                    tileArray[420][1] = new Color(99, 255, 107);
                    tileArray[420][4] = new Color(99, 255, 107);
                    tileArray[420][2] = new Color(218, 2, 5);
                    tileArray[420][3] = new Color(218, 2, 5);
                    tileArray[420][5] = new Color(218, 2, 5);
                    tileArray[476][0] = new Color(160, 160, 160);
                    tileArray[410][0] = new Color(75, 139, 166);
                    tileArray[480][0] = new Color(120, 50, 50);
                    tileArray[509][0] = new Color(50, 50, 60);
                    tileArray[657][0] = new Color(35, 205, 215);
                    tileArray[658][0] = new Color(200, 105, 230);
                    tileArray[412][0] = new Color(75, 139, 166);
                    tileArray[443][0] = new Color(144, 148, 144);
                    tileArray[442][0] = new Color(3, 144, 201);
                    tileArray[444][0] = new Color(191, 176, 124);
                    tileArray[446][0] = new Color(255, 66, 152);
                    tileArray[447][0] = new Color(179, 132, 255);
                    tileArray[448][0] = new Color(0, 206, 180);
                    tileArray[449][0] = new Color(91, 186, 240);
                    tileArray[450][0] = new Color(92, 240, 91);
                    tileArray[451][0] = new Color(240, 91, 147);
                    tileArray[452][0] = new Color(255, 150, 181);
                    tileArray[453][0] = new Color(179, 132, 255);
                    tileArray[453][1] = new Color(0, 206, 180);
                    tileArray[453][2] = new Color(255, 66, 152);
                    tileArray[454][0] = new Color(174, 16, 176);
                    tileArray[455][0] = new Color(48, 225, 110);
                    tileArray[456][0] = new Color(179, 132, 255);
                    tileArray[457][0] = new Color(150, 164, 206);
                    tileArray[457][1] = new Color(255, 132, 184);
                    tileArray[457][2] = new Color(74, 255, 232);
                    tileArray[457][3] = new Color(215, 159, 255);
                    tileArray[457][4] = new Color(229, 219, 234);
                    tileArray[458][0] = new Color(211, 198, 111);
                    tileArray[459][0] = new Color(190, 223, 232);
                    tileArray[460][0] = new Color(141, 163, 181);
                    tileArray[462][0] = new Color(231, 178, 28);
                    tileArray[467][0] = new Color(129, 56, 121);
                    tileArray[467][1] = new Color(255, 249, 59);
                    tileArray[467][2] = new Color(161, 67, 24);
                    tileArray[467][3] = new Color(89, 70, 72);
                    tileArray[467][4] = new Color(233, 207, 94);
                    tileArray[467][5] = new Color(254, 158, 35);
                    tileArray[467][6] = new Color(34, 221, 151);
                    tileArray[467][7] = new Color(249, 170, 236);
                    tileArray[467][8] = new Color(35, 200, 254);
                    tileArray[467][9] = new Color(190, 200, 200);
                    tileArray[467][10] = new Color(230, 170, 100);
                    tileArray[467][11] = new Color(165, 168, 26);
                    for (int num4 = 0; num4 < 12; num4++)
                    {
                        tileArray[468][num4] = tileArray[467][num4];
                    }
                    tileArray[472][0] = new Color(190, 160, 140);
                    tileArray[473][0] = new Color(85, 114, 123);
                    tileArray[474][0] = new Color(116, 94, 97);
                    tileArray[478][0] = new Color(108, 34, 35);
                    tileArray[479][0] = new Color(178, 114, 68);
                    tileArray[485][0] = new Color(198, 134, 88);
                    tileArray[492][0] = new Color(78, 193, 227);
                    tileArray[492][0] = new Color(78, 193, 227);
                    tileArray[493][0] = new Color(250, 249, 252);
                    tileArray[493][1] = new Color(240, 90, 90);
                    tileArray[493][2] = new Color(98, 230, 92);
                    tileArray[493][3] = new Color(95, 197, 238);
                    tileArray[493][4] = new Color(241, 221, 100);
                    tileArray[493][5] = new Color(213, 92, 237);
                    tileArray[494][0] = new Color(224, 219, 236);
                    tileArray[495][0] = new Color(253, 227, 215);
                    tileArray[496][0] = new Color(165, 159, 153);
                    tileArray[498][0] = new Color(202, 174, 165);
                    tileArray[499][0] = new Color(160, 187, 142);
                    tileArray[500][0] = new Color(254, 158, 35);
                    tileArray[501][0] = new Color(34, 221, 151);
                    tileArray[502][0] = new Color(249, 170, 236);
                    tileArray[503][0] = new Color(35, 200, 254);
                    tileArray[506][0] = new Color(61, 61, 61);
                    tileArray[510][0] = new Color(191, 142, 111);
                    tileArray[511][0] = new Color(187, 68, 74);
                    tileArray[520][0] = new Color(224, 219, 236);
                    tileArray[545][0] = new Color(255, 126, 145);
                    tileArray[530][0] = new Color(107, 182, 0);
                    tileArray[530][1] = new Color(23, 154, 209);
                    tileArray[530][2] = new Color(238, 97, 94);
                    tileArray[530][3] = new Color(113, 108, 205);
                    tileArray[546][0] = new Color(60, 60, 60);
                    tileArray[557][0] = new Color(60, 60, 60);
                    tileArray[547][0] = new Color(120, 110, 100);
                    tileArray[548][0] = new Color(120, 110, 100);
                    tileArray[562][0] = new Color(165, 168, 26);
                    tileArray[563][0] = new Color(165, 168, 26);
                    tileArray[571][0] = new Color(165, 168, 26);
                    tileArray[568][0] = new Color(248, 203, 233);
                    tileArray[569][0] = new Color(203, 248, 218);
                    tileArray[570][0] = new Color(160, 242, 255);
                    tileArray[597][0] = new Color(28, 216, 94);
                    tileArray[597][1] = new Color(183, 237, 20);
                    tileArray[597][2] = new Color(185, 83, 200);
                    tileArray[597][3] = new Color(131, 128, 168);
                    tileArray[597][4] = new Color(38, 142, 214);
                    tileArray[597][5] = new Color(229, 154, 9);
                    tileArray[597][6] = new Color(142, 227, 234);
                    tileArray[597][7] = new Color(98, 111, 223);
                    tileArray[597][8] = new Color(241, 233, 158);
                    tileArray[617][0] = new Color(233, 207, 94);
                    Color color3 = new(250, 100, 50);
                    tileArray[548][1] = color3;
                    tileArray[613][0] = color3;
                    tileArray[614][0] = color3;
                    tileArray[623][0] = new Color(220, 210, 245);
                    tileArray[661][0] = new Color(141, 137, 223);
                    tileArray[662][0] = new Color(208, 80, 80);
                    tileArray[666][0] = new Color(115, 60, 40);
                    tileArray[667][0] = new Color(247, 228, 254);

                    #endregion

                    #region Define Wall Color Table

                    // "Array4"
                    color = new Color(151, 107, 75);

                    Color[][] wallArray = new Color[(int)maxWallID][];
                    for (int i = 0; i < (int)maxWallID; i++)
                    {
                        wallArray[i] = new Color[2];
                    }
                    wallArray[158][0] = new Color(107, 49, 154);
                    wallArray[163][0] = new Color(154, 148, 49);
                    wallArray[162][0] = new Color(49, 49, 154);
                    wallArray[160][0] = new Color(49, 154, 68);
                    wallArray[161][0] = new Color(154, 49, 77);
                    wallArray[159][0] = new Color(85, 89, 118);
                    wallArray[157][0] = new Color(154, 83, 49);
                    wallArray[154][0] = new Color(221, 79, 255);
                    wallArray[166][0] = new Color(250, 255, 79);
                    wallArray[165][0] = new Color(79, 102, 255);
                    wallArray[156][0] = new Color(79, 255, 89);
                    wallArray[164][0] = new Color(255, 79, 79);
                    wallArray[155][0] = new Color(240, 240, 247);
                    wallArray[153][0] = new Color(255, 145, 79);
                    wallArray[169][0] = new Color(5, 5, 5);
                    wallArray[224][0] = new Color(57, 55, 52);
                    wallArray[323][0] = new Color(55, 25, 33);
                    wallArray[324][0] = new Color(60, 55, 145);
                    wallArray[325][0] = new Color(10, 5, 50);
                    wallArray[326][0] = new Color(30, 105, 75);
                    wallArray[327][0] = new Color(5, 45, 55);
                    wallArray[328][0] = new Color(20, 25, 35);
                    wallArray[329][0] = new Color(15, 10, 50);
                    wallArray[330][0] = new Color(153, 164, 187);
                    wallArray[225][0] = new Color(68, 68, 68);
                    wallArray[226][0] = new Color(148, 138, 74);
                    wallArray[227][0] = new Color(95, 137, 191);
                    wallArray[170][0] = new Color(59, 39, 22);
                    wallArray[171][0] = new Color(59, 39, 22);
                    color = new Color(52, 52, 52);
                    wallArray[1][0] = color;
                    wallArray[53][0] = color;
                    wallArray[52][0] = color;
                    wallArray[51][0] = color;
                    wallArray[50][0] = color;
                    wallArray[49][0] = color;
                    wallArray[48][0] = color;
                    wallArray[44][0] = color;
                    wallArray[346][0] = color;
                    wallArray[5][0] = color;
                    color = new Color(88, 61, 46);
                    wallArray[2][0] = color;
                    wallArray[16][0] = color;
                    wallArray[59][0] = color;
                    wallArray[3][0] = new Color(61, 58, 78);
                    wallArray[4][0] = new Color(73, 51, 36);
                    wallArray[6][0] = new Color(91, 30, 30);
                    color = new Color(27, 31, 42);
                    wallArray[7][0] = color;
                    wallArray[17][0] = color;
                    wallArray[331][0] = color;
                    color = new Color(32, 40, 45);
                    wallArray[94][0] = color;
                    wallArray[100][0] = color;
                    color = new Color(44, 41, 50);
                    wallArray[95][0] = color;
                    wallArray[101][0] = color;
                    color = new Color(31, 39, 26);
                    wallArray[8][0] = color;
                    wallArray[18][0] = color;
                    wallArray[332][0] = color;
                    color = new Color(36, 45, 44);
                    wallArray[98][0] = color;
                    wallArray[104][0] = color;
                    color = new Color(38, 49, 50);
                    wallArray[99][0] = color;
                    wallArray[105][0] = color;
                    color = new Color(41, 28, 36);
                    wallArray[9][0] = color;
                    wallArray[19][0] = color;
                    wallArray[333][0] = color;
                    color = new Color(72, 50, 77);
                    wallArray[96][0] = color;
                    wallArray[102][0] = color;
                    color = new Color(78, 50, 69);
                    wallArray[97][0] = color;
                    wallArray[103][0] = color;
                    wallArray[10][0] = new Color(74, 62, 12);
                    wallArray[334][0] = new Color(74, 62, 12);
                    wallArray[11][0] = new Color(46, 56, 59);
                    wallArray[335][0] = new Color(46, 56, 59);
                    wallArray[12][0] = new Color(75, 32, 11);
                    wallArray[336][0] = new Color(75, 32, 11);
                    wallArray[13][0] = new Color(67, 37, 37);
                    wallArray[338][0] = new Color(67, 37, 37);
                    color = new Color(15, 15, 15);
                    wallArray[14][0] = color;
                    wallArray[337][0] = color;
                    wallArray[20][0] = color;
                    wallArray[15][0] = new Color(52, 43, 45);
                    wallArray[22][0] = new Color(113, 99, 99);
                    wallArray[23][0] = new Color(38, 38, 43);
                    wallArray[24][0] = new Color(53, 39, 41);
                    wallArray[25][0] = new Color(11, 35, 62);
                    wallArray[339][0] = new Color(11, 35, 62);
                    wallArray[26][0] = new Color(21, 63, 70);
                    wallArray[340][0] = new Color(21, 63, 70);
                    wallArray[27][0] = new Color(88, 61, 46);
                    wallArray[27][1] = new Color(52, 52, 52);
                    wallArray[28][0] = new Color(81, 84, 101);
                    wallArray[29][0] = new Color(88, 23, 23);
                    wallArray[30][0] = new Color(28, 88, 23);
                    wallArray[31][0] = new Color(78, 87, 99);
                    color = new Color(69, 67, 41);
                    wallArray[34][0] = color;
                    wallArray[37][0] = color;
                    wallArray[32][0] = new Color(86, 17, 40);
                    wallArray[33][0] = new Color(49, 47, 83);
                    wallArray[35][0] = new Color(51, 51, 70);
                    wallArray[36][0] = new Color(87, 59, 55);
                    wallArray[38][0] = new Color(49, 57, 49);
                    wallArray[39][0] = new Color(78, 79, 73);
                    wallArray[45][0] = new Color(60, 59, 51);
                    wallArray[46][0] = new Color(48, 57, 47);
                    wallArray[47][0] = new Color(71, 77, 85);
                    wallArray[40][0] = new Color(85, 102, 103);
                    wallArray[41][0] = new Color(52, 50, 62);
                    wallArray[42][0] = new Color(71, 42, 44);
                    wallArray[43][0] = new Color(73, 66, 50);
                    wallArray[54][0] = new Color(40, 56, 50);
                    wallArray[55][0] = new Color(49, 48, 36);
                    wallArray[56][0] = new Color(43, 33, 32);
                    wallArray[57][0] = new Color(31, 40, 49);
                    wallArray[58][0] = new Color(48, 35, 52);
                    wallArray[60][0] = new Color(1, 52, 20);
                    wallArray[61][0] = new Color(55, 39, 26);
                    wallArray[62][0] = new Color(39, 33, 26);
                    wallArray[69][0] = new Color(43, 42, 68);
                    wallArray[70][0] = new Color(30, 70, 80);
                    wallArray[341][0] = new Color(100, 40, 1);
                    wallArray[342][0] = new Color(92, 30, 72);
                    wallArray[343][0] = new Color(42, 81, 1);
                    wallArray[344][0] = new Color(1, 81, 109);
                    wallArray[345][0] = new Color(56, 22, 97);
                    color = new Color(30, 80, 48);
                    wallArray[63][0] = color;
                    wallArray[65][0] = color;
                    wallArray[66][0] = color;
                    wallArray[68][0] = color;
                    color = new Color(53, 80, 30);
                    wallArray[64][0] = color;
                    wallArray[67][0] = color;
                    wallArray[78][0] = new Color(63, 39, 26);
                    wallArray[244][0] = new Color(63, 39, 26);
                    wallArray[71][0] = new Color(78, 105, 135);
                    wallArray[72][0] = new Color(52, 84, 12);
                    wallArray[73][0] = new Color(190, 204, 223);
                    color = new Color(64, 62, 80);
                    wallArray[74][0] = color;
                    wallArray[80][0] = color;
                    wallArray[75][0] = new Color(65, 65, 35);
                    wallArray[76][0] = new Color(20, 46, 104);
                    wallArray[77][0] = new Color(61, 13, 16);
                    wallArray[79][0] = new Color(51, 47, 96);
                    wallArray[81][0] = new Color(101, 51, 51);
                    wallArray[82][0] = new Color(77, 64, 34);
                    wallArray[83][0] = new Color(62, 38, 41);
                    wallArray[234][0] = new Color(60, 36, 39);
                    wallArray[84][0] = new Color(48, 78, 93);
                    wallArray[85][0] = new Color(54, 63, 69);
                    color = new Color(138, 73, 38);
                    wallArray[86][0] = color;
                    wallArray[108][0] = color;
                    color = new Color(50, 15, 8);
                    wallArray[87][0] = color;
                    wallArray[112][0] = color;
                    wallArray[109][0] = new Color(94, 25, 17);
                    wallArray[110][0] = new Color(125, 36, 122);
                    wallArray[111][0] = new Color(51, 35, 27);
                    wallArray[113][0] = new Color(135, 58, 0);
                    wallArray[114][0] = new Color(65, 52, 15);
                    wallArray[115][0] = new Color(39, 42, 51);
                    wallArray[116][0] = new Color(89, 26, 27);
                    wallArray[117][0] = new Color(126, 123, 115);
                    wallArray[118][0] = new Color(8, 50, 19);
                    wallArray[119][0] = new Color(95, 21, 24);
                    wallArray[120][0] = new Color(17, 31, 65);
                    wallArray[121][0] = new Color(192, 173, 143);
                    wallArray[122][0] = new Color(114, 114, 131);
                    wallArray[123][0] = new Color(136, 119, 7);
                    wallArray[124][0] = new Color(8, 72, 3);
                    wallArray[125][0] = new Color(117, 132, 82);
                    wallArray[126][0] = new Color(100, 102, 114);
                    wallArray[127][0] = new Color(30, 118, 226);
                    wallArray[128][0] = new Color(93, 6, 102);
                    wallArray[129][0] = new Color(64, 40, 169);
                    wallArray[130][0] = new Color(39, 34, 180);
                    wallArray[131][0] = new Color(87, 94, 125);
                    wallArray[132][0] = new Color(6, 6, 6);
                    wallArray[133][0] = new Color(69, 72, 186);
                    wallArray[134][0] = new Color(130, 62, 16);
                    wallArray[135][0] = new Color(22, 123, 163);
                    wallArray[136][0] = new Color(40, 86, 151);
                    wallArray[137][0] = new Color(183, 75, 15);
                    wallArray[138][0] = new Color(83, 80, 100);
                    wallArray[139][0] = new Color(115, 65, 68);
                    wallArray[140][0] = new Color(119, 108, 81);
                    wallArray[141][0] = new Color(59, 67, 71);
                    wallArray[142][0] = new Color(222, 216, 202);
                    wallArray[143][0] = new Color(90, 112, 105);
                    wallArray[144][0] = new Color(62, 28, 87);
                    wallArray[146][0] = new Color(120, 59, 19);
                    wallArray[147][0] = new Color(59, 59, 59);
                    wallArray[148][0] = new Color(229, 218, 161);
                    wallArray[149][0] = new Color(73, 59, 50);
                    wallArray[151][0] = new Color(102, 75, 34);
                    wallArray[167][0] = new Color(70, 68, 51);
                    Color color4 = new(125, 100, 100);
                    wallArray[316][0] = color4;
                    wallArray[317][0] = color4;
                    wallArray[172][0] = new Color(163, 96, 0);
                    wallArray[242][0] = new Color(5, 5, 5);
                    wallArray[243][0] = new Color(5, 5, 5);
                    wallArray[173][0] = new Color(94, 163, 46);
                    wallArray[174][0] = new Color(117, 32, 59);
                    wallArray[175][0] = new Color(20, 11, 203);
                    wallArray[176][0] = new Color(74, 69, 88);
                    wallArray[177][0] = new Color(60, 30, 30);
                    wallArray[183][0] = new Color(111, 117, 135);
                    wallArray[179][0] = new Color(111, 117, 135);
                    wallArray[178][0] = new Color(111, 117, 135);
                    wallArray[184][0] = new Color(25, 23, 54);
                    wallArray[181][0] = new Color(25, 23, 54);
                    wallArray[180][0] = new Color(25, 23, 54);
                    wallArray[182][0] = new Color(74, 71, 129);
                    wallArray[185][0] = new Color(52, 52, 52);
                    wallArray[186][0] = new Color(38, 9, 66);
                    wallArray[216][0] = new Color(158, 100, 64);
                    wallArray[217][0] = new Color(62, 45, 75);
                    wallArray[218][0] = new Color(57, 14, 12);
                    wallArray[219][0] = new Color(96, 72, 133);
                    wallArray[187][0] = new Color(149, 80, 51);
                    wallArray[235][0] = new Color(140, 75, 48);
                    wallArray[220][0] = new Color(67, 55, 80);
                    wallArray[221][0] = new Color(64, 37, 29);
                    wallArray[222][0] = new Color(70, 51, 91);
                    wallArray[188][0] = new Color(82, 63, 80);
                    wallArray[189][0] = new Color(65, 61, 77);
                    wallArray[190][0] = new Color(64, 65, 92);
                    wallArray[191][0] = new Color(76, 53, 84);
                    wallArray[192][0] = new Color(144, 67, 52);
                    wallArray[193][0] = new Color(149, 48, 48);
                    wallArray[194][0] = new Color(111, 32, 36);
                    wallArray[195][0] = new Color(147, 48, 55);
                    wallArray[196][0] = new Color(97, 67, 51);
                    wallArray[197][0] = new Color(112, 80, 62);
                    wallArray[198][0] = new Color(88, 61, 46);
                    wallArray[199][0] = new Color(127, 94, 76);
                    wallArray[200][0] = new Color(143, 50, 123);
                    wallArray[201][0] = new Color(136, 120, 131);
                    wallArray[202][0] = new Color(219, 92, 143);
                    wallArray[203][0] = new Color(113, 64, 150);
                    wallArray[204][0] = new Color(74, 67, 60);
                    wallArray[205][0] = new Color(60, 78, 59);
                    wallArray[206][0] = new Color(0, 54, 21);
                    wallArray[207][0] = new Color(74, 97, 72);
                    wallArray[208][0] = new Color(40, 37, 35);
                    wallArray[209][0] = new Color(77, 63, 66);
                    wallArray[210][0] = new Color(111, 6, 6);
                    wallArray[211][0] = new Color(88, 67, 59);
                    wallArray[212][0] = new Color(88, 87, 80);
                    wallArray[213][0] = new Color(71, 71, 67);
                    wallArray[214][0] = new Color(76, 52, 60);
                    wallArray[215][0] = new Color(89, 48, 59);
                    wallArray[223][0] = new Color(51, 18, 4);
                    wallArray[228][0] = new Color(160, 2, 75);
                    wallArray[229][0] = new Color(100, 55, 164);
                    wallArray[230][0] = new Color(0, 117, 101);
                    wallArray[236][0] = new Color(127, 49, 44);
                    wallArray[231][0] = new Color(110, 90, 78);
                    wallArray[232][0] = new Color(47, 69, 75);
                    wallArray[233][0] = new Color(91, 67, 70);
                    wallArray[237][0] = new Color(200, 44, 18);
                    wallArray[238][0] = new Color(24, 93, 66);
                    wallArray[239][0] = new Color(160, 87, 234);
                    wallArray[240][0] = new Color(6, 106, 255);
                    wallArray[245][0] = new Color(102, 102, 102);
                    wallArray[315][0] = new Color(181, 230, 29);
                    wallArray[246][0] = new Color(61, 58, 78);
                    wallArray[247][0] = new Color(52, 43, 45);
                    wallArray[248][0] = new Color(81, 84, 101);
                    wallArray[249][0] = new Color(85, 102, 103);
                    wallArray[250][0] = new Color(52, 52, 52);
                    wallArray[251][0] = new Color(52, 52, 52);
                    wallArray[252][0] = new Color(52, 52, 52);
                    wallArray[253][0] = new Color(52, 52, 52);
                    wallArray[254][0] = new Color(52, 52, 52);
                    wallArray[255][0] = new Color(52, 52, 52);
                    wallArray[314][0] = new Color(52, 52, 52);
                    wallArray[256][0] = new Color(40, 56, 50);
                    wallArray[257][0] = new Color(49, 48, 36);
                    wallArray[258][0] = new Color(43, 33, 32);
                    wallArray[259][0] = new Color(31, 40, 49);
                    wallArray[260][0] = new Color(48, 35, 52);
                    wallArray[261][0] = new Color(88, 61, 46);
                    wallArray[262][0] = new Color(55, 39, 26);
                    wallArray[263][0] = new Color(39, 33, 26);
                    wallArray[264][0] = new Color(43, 42, 68);
                    wallArray[265][0] = new Color(30, 70, 80);
                    wallArray[266][0] = new Color(78, 105, 135);
                    wallArray[267][0] = new Color(51, 47, 96);
                    wallArray[268][0] = new Color(101, 51, 51);
                    wallArray[269][0] = new Color(62, 38, 41);
                    wallArray[270][0] = new Color(59, 39, 22);
                    wallArray[271][0] = new Color(59, 39, 22);
                    wallArray[272][0] = new Color(111, 117, 135);
                    wallArray[273][0] = new Color(25, 23, 54);
                    wallArray[274][0] = new Color(52, 52, 52);
                    wallArray[275][0] = new Color(149, 80, 51);
                    wallArray[276][0] = new Color(82, 63, 80);
                    wallArray[277][0] = new Color(65, 61, 77);
                    wallArray[278][0] = new Color(64, 65, 92);
                    wallArray[279][0] = new Color(76, 53, 84);
                    wallArray[280][0] = new Color(144, 67, 52);
                    wallArray[281][0] = new Color(149, 48, 48);
                    wallArray[282][0] = new Color(111, 32, 36);
                    wallArray[283][0] = new Color(147, 48, 55);
                    wallArray[284][0] = new Color(97, 67, 51);
                    wallArray[285][0] = new Color(112, 80, 62);
                    wallArray[286][0] = new Color(88, 61, 46);
                    wallArray[287][0] = new Color(127, 94, 76);
                    wallArray[288][0] = new Color(143, 50, 123);
                    wallArray[289][0] = new Color(136, 120, 131);
                    wallArray[290][0] = new Color(219, 92, 143);
                    wallArray[291][0] = new Color(113, 64, 150);
                    wallArray[292][0] = new Color(74, 67, 60);
                    wallArray[293][0] = new Color(60, 78, 59);
                    wallArray[294][0] = new Color(0, 54, 21);
                    wallArray[295][0] = new Color(74, 97, 72);
                    wallArray[296][0] = new Color(40, 37, 35);
                    wallArray[297][0] = new Color(77, 63, 66);
                    wallArray[298][0] = new Color(111, 6, 6);
                    wallArray[299][0] = new Color(88, 67, 59);
                    wallArray[300][0] = new Color(88, 87, 80);
                    wallArray[301][0] = new Color(71, 71, 67);
                    wallArray[302][0] = new Color(76, 52, 60);
                    wallArray[303][0] = new Color(89, 48, 59);
                    wallArray[304][0] = new Color(158, 100, 64);
                    wallArray[305][0] = new Color(62, 45, 75);
                    wallArray[306][0] = new Color(57, 14, 12);
                    wallArray[307][0] = new Color(96, 72, 133);
                    wallArray[308][0] = new Color(67, 55, 80);
                    wallArray[309][0] = new Color(64, 37, 29);
                    wallArray[310][0] = new Color(70, 51, 91);
                    wallArray[311][0] = new Color(51, 18, 4);
                    wallArray[312][0] = new Color(78, 110, 51);
                    wallArray[313][0] = new Color(78, 110, 51);
                    wallArray[319][0] = new Color(105, 51, 108);
                    wallArray[320][0] = new Color(75, 30, 15);
                    wallArray[321][0] = new Color(91, 108, 130);
                    wallArray[322][0] = new Color(91, 108, 130);

                    #endregion

                    #region Define Color Table Counts

                    tileOptionCounts = new int[(int)maxTileID];
                    for (int tileCount = 0; tileCount < (int)maxTileID; tileCount++)
                    {
                        Color[] array = tileArray[tileCount];

                        int colorCount = 0;
                        while (colorCount < 12 && !(array[colorCount] == Color.Transparent))
                        {
                            colorCount++;
                        }
                        tileOptionCounts[tileCount] = colorCount;
                    }

                    wallOptionCounts = new int[(int)maxWallID];
                    for (int wallCount = 0; wallCount < (int)maxWallID; wallCount++)
                    {
                        Color[] array = wallArray[wallCount];

                        int colorCount = 0;
                        while (colorCount < 2 && !(array[colorCount] == Color.Transparent))
                        {
                            colorCount++;
                        }
                        wallOptionCounts[wallCount] = colorCount;
                    }

                    // Define initial position.
                    ushort Position = 0;

                    tileLookup = new ushort[(int)maxTileID];
                    for (int tileCount = 0; tileCount < (int)maxTileID; tileCount++)
                    {
                        if (tileOptionCounts[tileCount] > 0)
                        {
                            tileLookup[tileCount] = Position;
                            for (int colorCount = 0; colorCount < tileOptionCounts[tileCount]; colorCount++)
                            {
                                Position += 1;
                            }
                        }
                        else
                        {
                            tileLookup[tileCount] = 0;
                        }
                    }

                    wallLookup = new ushort[(int)maxWallID];
                    for (int wallCount = 0; wallCount < (int)maxWallID; wallCount++)
                    {
                        if (wallOptionCounts[wallCount] > 0)
                        {
                            wallLookup[wallCount] = Position;
                            for (int colorCount = 0; colorCount < wallOptionCounts[wallCount]; colorCount++)
                            {
                                Position += 1;
                            }
                        }
                        else
                        {
                            wallLookup[wallCount] = 0;
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
    }
}
