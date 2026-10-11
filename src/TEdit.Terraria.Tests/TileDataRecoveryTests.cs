using System.IO;
using Shouldly;

namespace TEdit.Terraria.Tests;

public class TileDataRecoveryTests
{
    private const int Size = 6;

    private static byte[] SaveTileSection(int version, bool[] frameImportant)
    {
        var tiles = new Tile[Size, Size];
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                // Alternate dirt and stone so no run-length compression hides the column ends.
                tiles[x, y].IsActive = true;
                tiles[x, y].Type = (ushort)((x + y) % 2 == 0 ? 0 : 1);
            }
        }

        using var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            World.SaveTiles(tiles, version, Size, Size, bw, frameImportant);
        return ms.ToArray();
    }

    [Fact]
    public void LoadTileData_CompleteSection_IsNotFlaggedAsRecovered()
    {
        int version = (int)WorldConfiguration.CompatibleVersion;
        bool[] frameImportant = WorldConfiguration.SettingsTileFrameImportant;
        byte[] data = SaveTileSection(version, frameImportant);

        var tiles = World.LoadTileData(new BinaryReader(new MemoryStream(data)), Size, Size, version, frameImportant, out bool recovered);

        recovered.ShouldBeFalse();
        tiles[Size - 1, Size - 1].IsActive.ShouldBeTrue();
    }

    [Fact]
    public void LoadTileData_TruncatedSection_IsFlaggedAsRecovered()
    {
        int version = (int)WorldConfiguration.CompatibleVersion;
        bool[] frameImportant = WorldConfiguration.SettingsTileFrameImportant;
        byte[] data = SaveTileSection(version, frameImportant);
        byte[] truncated = data[..(data.Length / 2)];

        var tiles = World.LoadTileData(new BinaryReader(new MemoryStream(truncated)), Size, Size, version, frameImportant, out bool recovered);

        recovered.ShouldBeTrue();
        tiles[Size - 1, Size - 1].IsActive.ShouldBeFalse();
    }
}
