using System;
using System.IO;
using TEdit.Common.IO;
using TEdit.Terraria.TModLoader;
using Shouldly;
using Xunit;

namespace TEdit.Terraria.Tests;

public class TwldTruncationTests
{
    private static TwldData DataWithModTile(ushort saveType)
    {
        var data = new TwldData();
        data.TileMap.Add(new ModTileEntry { SaveType = saveType, ModName = "TestMod", Name = "TestTile" });
        data.SaveTypeToTileMapIndex[saveType] = 0;
        return data;
    }

    [Fact]
    public void ParseTileDataDense_TruncatedStream_Throws()
    {
        // 2x2 world, but only the first position is present.
        byte[] tileData = [0, 0];

        Should.Throw<InvalidDataException>(() =>
            TwldFile.ParseTileDataDense(tileData, DataWithModTile(700), 2, 2));
    }

    [Fact]
    public void ParseTileDataDense_UnknownSaveType_Throws()
    {
        // saveType 999 (little-endian) is not in the tile map.
        byte[] tileData = [0xE7, 0x03, 0];

        Should.Throw<InvalidDataException>(() =>
            TwldFile.ParseTileDataDense(tileData, DataWithModTile(700), 1, 1));
    }

    [Fact]
    public void ParseWallDataDense_TruncatedStream_Throws()
    {
        Should.Throw<InvalidDataException>(() =>
            TwldFile.ParseWallDataDense([0, 0], new TwldData(), 2, 2));
    }

    [Theory]
    [InlineData(10)]             // fewer bytes than declared
    [InlineData(int.MaxValue)]   // corrupt length must not allocate 2 GB
    public void FromStream_ByteArrayShorterThanLength_ThrowsEndOfStream(int declaredLength)
    {
        using var ms = new MemoryStream();
        using (var w = new BigEndianWriter(ms))
        {
            w.Write((byte)10);   // root compound
            w.Write("");
            w.Write((byte)7);    // byte array
            w.Write("data");
            w.Write(declaredLength);
            w.Write(new byte[] { 1, 2, 3 });
        }
        ms.Position = 0;

        Should.Throw<EndOfStreamException>(() => TagIO.FromStream(ms, compressed: false));
    }
}
