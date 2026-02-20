using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TEdit.Common.IO;
using TEdit.Terraria.TModLoader;
using Xunit;
using Shouldly;

namespace TEdit.Terraria.Tests.TModLoader;

public class TmodTextureExtractorTests
{
    #region DecodeRawImg

    [Fact]
    public void DecodeRawImg_VersionedFormat_DecodesCorrectly()
    {
        // tModLoader rawimg: version(1), width, height, RGBA
        int width = 16;
        int height = 18;
        var data = new byte[12 + width * height * 4];

        // Version = 1
        BitConverter.GetBytes(1).CopyTo(data, 0);
        // Width = 16
        BitConverter.GetBytes(width).CopyTo(data, 4);
        // Height = 18
        BitConverter.GetBytes(height).CopyTo(data, 8);

        // Fill pixel data with a recognizable pattern
        for (int i = 0; i < width * height; i++)
        {
            int offset = 12 + i * 4;
            data[offset] = 0xFF;     // R
            data[offset + 1] = 0x80; // G
            data[offset + 2] = 0x40; // B
            data[offset + 3] = 0xFF; // A
        }

        var result = TmodTextureExtractor.DecodeRawImg(data);

        result.ShouldNotBeNull();
        result.Value.Width.ShouldBe(width);
        result.Value.Height.ShouldBe(height);
        result.Value.Rgba.Length.ShouldBe(width * height * 4);
        result.Value.Rgba[0].ShouldBe((byte)0xFF); // R of first pixel
        result.Value.Rgba[1].ShouldBe((byte)0x80); // G of first pixel
    }

    [Fact]
    public void DecodeRawImg_LegacyFormat_DecodesCorrectly()
    {
        // Legacy format without version prefix: width, height, RGBA
        // Uses a width that's NOT 1 to distinguish from versioned format
        int width = 32;
        int height = 32;
        var data = new byte[8 + width * height * 4];

        BitConverter.GetBytes(width).CopyTo(data, 0);
        BitConverter.GetBytes(height).CopyTo(data, 4);

        var result = TmodTextureExtractor.DecodeRawImg(data);

        result.ShouldNotBeNull();
        result.Value.Width.ShouldBe(width);
        result.Value.Height.ShouldBe(height);
    }

    [Fact]
    public void DecodeRawImg_TooShort_ReturnsNull()
    {
        var data = new byte[8]; // Less than 12 bytes minimum
        var result = TmodTextureExtractor.DecodeRawImg(data);
        result.ShouldBeNull();
    }

    [Fact]
    public void DecodeRawImg_InvalidDimensions_ReturnsNull()
    {
        var data = new byte[12];
        BitConverter.GetBytes(1).CopyTo(data, 0);      // version
        BitConverter.GetBytes(-1).CopyTo(data, 4);      // invalid width
        BitConverter.GetBytes(10).CopyTo(data, 8);      // height

        var result = TmodTextureExtractor.DecodeRawImg(data);
        result.ShouldBeNull();
    }

    [Fact]
    public void DecodeRawImg_DataTooSmallForDimensions_ReturnsNull()
    {
        var data = new byte[20]; // Way too small for 100x100
        BitConverter.GetBytes(1).CopyTo(data, 0);       // version
        BitConverter.GetBytes(100).CopyTo(data, 4);      // width
        BitConverter.GetBytes(100).CopyTo(data, 8);      // height

        var result = TmodTextureExtractor.DecodeRawImg(data);
        result.ShouldBeNull();
    }

    [Fact]
    public void DecodeRawImg_PixelDataPreserved()
    {
        int width = 2;
        int height = 2;
        var data = new byte[12 + width * height * 4];

        BitConverter.GetBytes(1).CopyTo(data, 0);       // version
        BitConverter.GetBytes(width).CopyTo(data, 4);
        BitConverter.GetBytes(height).CopyTo(data, 8);

        // Set specific pixel values
        // Pixel (0,0): red
        data[12] = 255; data[13] = 0; data[14] = 0; data[15] = 255;
        // Pixel (1,0): green
        data[16] = 0; data[17] = 255; data[18] = 0; data[19] = 255;
        // Pixel (0,1): blue
        data[20] = 0; data[21] = 0; data[22] = 255; data[23] = 255;
        // Pixel (1,1): white
        data[24] = 255; data[25] = 255; data[26] = 255; data[27] = 255;

        var result = TmodTextureExtractor.DecodeRawImg(data);

        result.ShouldNotBeNull();
        var rgba = result.Value.Rgba;

        // Pixel (0,0) = red
        rgba[0].ShouldBe((byte)255);
        rgba[1].ShouldBe((byte)0);
        rgba[2].ShouldBe((byte)0);

        // Pixel (1,0) = green
        rgba[4].ShouldBe((byte)0);
        rgba[5].ShouldBe((byte)255);
        rgba[6].ShouldBe((byte)0);

        // Pixel (0,1) = blue
        rgba[8].ShouldBe((byte)0);
        rgba[9].ShouldBe((byte)0);
        rgba[10].ShouldBe((byte)255);
    }

    #endregion

    #region Name-to-VirtualId Mapping

    [Fact]
    public void BuildTileNameToVirtualIdMap_ReturnsCorrectMappings()
    {
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry { SaveType = 1, ModName = "CalamityMod", Name = "AstralMonolith", FrameImportant = false });
        data.TileMap.Add(new ModTileEntry { SaveType = 2, ModName = "CalamityMod", Name = "AstralChest", FrameImportant = true });
        data.TileMap.Add(new ModTileEntry { SaveType = 3, ModName = "ThoriumMod", Name = "ThoriumOre", FrameImportant = false });

        // Simulate virtual ID assignment (base = 752 for tiles)
        data.MapIndexToVirtualTileId[0] = 752;
        data.MapIndexToVirtualTileId[1] = 753;
        data.MapIndexToVirtualTileId[2] = 754;

        var map = TwldFile.BuildTileNameToVirtualIdMap(data);

        map.Count.ShouldBe(3);
        map[("CalamityMod", "AstralMonolith")].ShouldBe((ushort)752);
        map[("CalamityMod", "AstralChest")].ShouldBe((ushort)753);
        map[("ThoriumMod", "ThoriumOre")].ShouldBe((ushort)754);
    }

    [Fact]
    public void BuildWallNameToVirtualIdMap_ReturnsCorrectMappings()
    {
        var data = new TwldData();
        data.WallMap.Add(new ModWallEntry { SaveType = 1, ModName = "CalamityMod", Name = "AstralWall" });
        data.WallMap.Add(new ModWallEntry { SaveType = 2, ModName = "ThoriumMod", Name = "ThoriumWall" });

        data.MapIndexToVirtualWallId[0] = 366;
        data.MapIndexToVirtualWallId[1] = 367;

        var map = TwldFile.BuildWallNameToVirtualIdMap(data);

        map.Count.ShouldBe(2);
        map[("CalamityMod", "AstralWall")].ShouldBe((ushort)366);
        map[("ThoriumMod", "ThoriumWall")].ShouldBe((ushort)367);
    }

    [Fact]
    public void BuildTileNameToVirtualIdMap_CaseInsensitive()
    {
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry { SaveType = 1, ModName = "CalamityMod", Name = "AstralMonolith" });
        data.MapIndexToVirtualTileId[0] = 752;

        var map = TwldFile.BuildTileNameToVirtualIdMap(data);

        // Case-insensitive lookup should work
        map.TryGetValue(("calamitymod", "astralmonolith"), out ushort id).ShouldBeTrue();
        id.ShouldBe((ushort)752);
    }

    [Fact]
    public void BuildTileNameToVirtualIdMap_EmptyData_ReturnsEmpty()
    {
        var data = new TwldData();
        var map = TwldFile.BuildTileNameToVirtualIdMap(data);
        map.Count.ShouldBe(0);
    }

    [Fact]
    public void BuildTileNameToVirtualIdMap_NullData_ReturnsEmpty()
    {
        var map = TwldFile.BuildTileNameToVirtualIdMap(null);
        map.Count.ShouldBe(0);
    }

    #endregion

    #region GetUsedModNames

    [Fact]
    public void GetUsedModNames_ReturnsModList()
    {
        var data = new TwldData();
        data.Header = new TagCompound();
        data.Header.Set("usedMods", new List<string> { "CalamityMod", "ThoriumMod", "CalamityModMusic" });

        var mods = TwldFile.GetUsedModNames(data);

        mods.Count.ShouldBe(3);
        mods.ShouldContain("CalamityMod");
        mods.ShouldContain("ThoriumMod");
        mods.ShouldContain("CalamityModMusic");
    }

    [Fact]
    public void GetUsedModNames_NullData_ReturnsEmpty()
    {
        var mods = TwldFile.GetUsedModNames(null);
        mods.Count.ShouldBe(0);
    }

    [Fact]
    public void GetUsedModNames_NoHeader_ReturnsEmpty()
    {
        var data = new TwldData { Header = null };
        var mods = TwldFile.GetUsedModNames(data);
        mods.Count.ShouldBe(0);
    }

    [Fact]
    public void GetUsedModNames_EmptyHeader_ReturnsEmpty()
    {
        var data = new TwldData();
        // Header exists but has no "usedMods" key
        var mods = TwldFile.GetUsedModNames(data);
        mods.Count.ShouldBe(0);
    }

    #endregion

    #region Integration Tests with Real .tmod Files

    private static readonly string CalamityTmodPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        "Steam", "steamapps", "workshop", "content", "1281930");

    private static string FindCalamityTmod()
    {
        if (!Directory.Exists(CalamityTmodPath))
            return null;

        foreach (var workshopDir in Directory.EnumerateDirectories(CalamityTmodPath))
        {
            foreach (var versionDir in Directory.EnumerateDirectories(workshopDir))
            {
                string tmodPath = Path.Combine(versionDir, "CalamityMod.tmod");
                if (File.Exists(tmodPath))
                    return tmodPath;
            }
        }
        return null;
    }

    [SkippableFact]
    public void CalamityTmod_Opens_And_ExtractsTileTextures()
    {
        string tmodPath = FindCalamityTmod();
        Skip.If(tmodPath == null, "CalamityMod.tmod not found in Steam workshop");

        var extractor = TmodTextureExtractor.Open(tmodPath);

        extractor.ModName.ShouldBe("CalamityMod");
        extractor.ModVersion.ShouldNotBeNullOrEmpty();

        var tiles = extractor.ExtractTileTextures();

        tiles.Count.ShouldBeGreaterThan(100, "Calamity should have 100+ tile textures");

        // Verify some known Calamity tiles exist
        // Case-insensitive dictionary, so direct lookup works
        tiles.ShouldContainKey("AstralMonolith");

        // All textures should have non-empty data
        foreach (var (name, tex) in tiles)
        {
            tex.Data.Length.ShouldBeGreaterThan(0, $"Texture {name} has empty data");
        }
    }

    [SkippableFact]
    public void CalamityTmod_ExtractsWallTextures()
    {
        string tmodPath = FindCalamityTmod();
        Skip.If(tmodPath == null, "CalamityMod.tmod not found in Steam workshop");

        var extractor = TmodTextureExtractor.Open(tmodPath);
        var walls = extractor.ExtractWallTextures();

        walls.Count.ShouldBeGreaterThan(10, "Calamity should have 10+ wall textures");
    }

    [SkippableFact]
    public void CalamityTmod_RawImgTextures_DecodeToValidDimensions()
    {
        string tmodPath = FindCalamityTmod();
        Skip.If(tmodPath == null, "CalamityMod.tmod not found in Steam workshop");

        var extractor = TmodTextureExtractor.Open(tmodPath);
        var tiles = extractor.ExtractTileTextures();

        int rawImgCount = 0;
        int pngCount = 0;
        int decodeErrors = 0;

        foreach (var (name, tex) in tiles)
        {
            if (tex.IsRawImg)
            {
                rawImgCount++;
                var decoded = TmodTextureExtractor.DecodeRawImg(tex.Data);
                if (decoded == null)
                {
                    decodeErrors++;
                    continue;
                }

                decoded.Value.Width.ShouldBeGreaterThan(0, $"Texture {name} has zero width");
                decoded.Value.Height.ShouldBeGreaterThan(0, $"Texture {name} has zero height");

                // Tile textures should be reasonable size (not 1xN)
                decoded.Value.Width.ShouldBeGreaterThan(1,
                    $"Texture {name} has width=1 which suggests rawimg header parsing error. " +
                    $"DataLen={tex.Data.Length}, first 16 bytes: {BitConverter.ToString(tex.Data, 0, Math.Min(16, tex.Data.Length))}");
            }
            else
            {
                pngCount++;
            }
        }

        decodeErrors.ShouldBe(0, $"Failed to decode {decodeErrors} of {rawImgCount} rawimg textures");

        // Log distribution for diagnostics
        (rawImgCount + pngCount).ShouldBe(tiles.Count);
    }

    [SkippableFact]
    public void CalamityTmod_TextureMapping_MatchesTwldEntries()
    {
        string tmodPath = FindCalamityTmod();
        Skip.If(tmodPath == null, "CalamityMod.tmod not found in Steam workshop");

        string twldPath = @"D:\dev\ai\tedit\tModLoaderData\Worlds\Calamity2.twld";
        string wldPath = @"D:\dev\ai\tedit\tModLoaderData\Worlds\Calamity2.wld";
        Skip.IfNot(File.Exists(twldPath), "Calamity2.twld test fixture not found");

        WorldConfiguration.Initialize();

        var twldData = TwldFile.Load(wldPath);
        twldData.ShouldNotBeNull();

        // Assign virtual IDs
        ushort virtualTileBase = (ushort)WorldConfiguration.TileCount;
        for (int i = 0; i < twldData.TileMap.Count; i++)
            twldData.MapIndexToVirtualTileId[i] = (ushort)(virtualTileBase + i);

        var tileNameToId = TwldFile.BuildTileNameToVirtualIdMap(twldData);
        var extractor = TmodTextureExtractor.Open(tmodPath);
        var tileTextures = extractor.ExtractTileTextures();

        // Count how many .twld tile entries have matching textures in the .tmod
        int matched = 0;
        int unmatched = 0;
        foreach (var entry in twldData.TileMap.Where(e => e.ModName == "CalamityMod"))
        {
            if (tileTextures.ContainsKey(entry.Name))
                matched++;
            else
                unmatched++;
        }

        // Most Calamity tiles should have matching textures
        matched.ShouldBeGreaterThan(0, "No tile textures matched twld entries");
        double matchRate = (double)matched / (matched + unmatched);
        matchRate.ShouldBeGreaterThan(0.5, $"Only {matched}/{matched + unmatched} ({matchRate:P0}) tiles matched textures");
    }

    #endregion
}
