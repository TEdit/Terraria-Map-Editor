using System;
using System.IO;
using System.IO.Compression;

namespace TEdit.Export;

/// <summary>
/// Writes raw filtered PNG image data (filter byte + RGBA per row) to a PNG file.
/// Used for 256×256 leaflet tiles where all scanlines are pre-built in memory.
/// </summary>
internal static class TilePngWriter
{
    public static void Write(string path, int width, int height, byte[] rawFilteredData)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 65536);

        // PNG signature
        ReadOnlySpan<byte> sig = [137, 80, 78, 71, 13, 10, 26, 10];
        fs.Write(sig);

        // IHDR
        Span<byte> ihdr = stackalloc byte[13];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(ihdr, width);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(ihdr.Slice(4), height);
        ihdr[8] = 8; ihdr[9] = 6; ihdr[10] = 0; ihdr[11] = 0; ihdr[12] = 0;
        WriteChunk(fs, "IHDR"u8, ihdr);

        // IDAT: zlib header + deflate(rawFilteredData) + Adler-32
        using var idatMs = new MemoryStream();
        idatMs.WriteByte(0x78); // CMF: deflate, 32K window
        idatMs.WriteByte(0x01); // FLG: FLEVEL=0, FCHECK=1 → (0x78*256+0x01)%31 = 30721%31 = 0 ✓

        using (var deflate = new DeflateStream(idatMs, CompressionLevel.Fastest, leaveOpen: true))
        {
            deflate.Write(rawFilteredData, 0, rawFilteredData.Length);
        }

        // Adler-32
        uint adlerA = 1, adlerB = 0;
        const int nmax = 5552;
        int remaining = rawFilteredData.Length;
        int offset = 0;
        while (remaining > 0)
        {
            int blockLen = Math.Min(remaining, nmax);
            for (int i = 0; i < blockLen; i++)
            {
                adlerA += rawFilteredData[offset + i];
                adlerB += adlerA;
            }
            adlerA %= 65521;
            adlerB %= 65521;
            offset += blockLen;
            remaining -= blockLen;
        }
        Span<byte> adler = stackalloc byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(adler, (adlerB << 16) | adlerA);
        idatMs.Write(adler);

        // Write IDAT chunk(s) from the compressed buffer
        var idatBuf = idatMs.GetBuffer();
        int idatLen = (int)idatMs.Position;
        int idatOff = 0;
        while (idatOff < idatLen)
        {
            int chunkSize = Math.Min(65536, idatLen - idatOff);
            WriteChunk(fs, "IDAT"u8, idatBuf.AsSpan(idatOff, chunkSize));
            idatOff += chunkSize;
        }

        // IEND
        WriteChunk(fs, "IEND"u8, ReadOnlySpan<byte>.Empty);
    }

    private static void WriteChunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> buf4 = stackalloc byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(buf4, data.Length);
        output.Write(buf4);
        output.Write(type);
        if (data.Length > 0)
            output.Write(data);

        // CRC32 over type + data
        uint crc = 0xFFFFFFFF;
        for (int i = 0; i < type.Length; i++)
            crc = Crc32Table[(crc ^ type[i]) & 0xFF] ^ (crc >> 8);
        for (int i = 0; i < data.Length; i++)
            crc = Crc32Table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
        crc ^= 0xFFFFFFFF;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(buf4, crc);
        output.Write(buf4);
    }

    private static readonly uint[] Crc32Table = InitCrc32Table();
    private static uint[] InitCrc32Table()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            table[n] = c;
        }
        return table;
    }
}
