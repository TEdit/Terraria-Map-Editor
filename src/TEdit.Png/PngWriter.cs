using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;

namespace TEdit.Png;

/// <summary>
/// Static convenience methods for writing PNG files from pre-built pixel data.
/// </summary>
public static class PngWriter
{
    /// <summary>
    /// Writes raw filtered PNG image data (filter byte + RGBA per row) to a stream.
    /// Each row must be: 1 filter byte (typically 0) followed by width × 4 RGBA bytes.
    /// Total length: height × (1 + width × 4).
    /// </summary>
    public static void WriteFilteredRgba(Stream output, int width, int height, byte[] rawFilteredData)
    {
        PngChunkWriter.WriteSignature(output);
        PngChunkWriter.WriteIhdr(output, width, height);

        // IDAT: zlib header + deflate(rawFilteredData) + Adler-32
        using var idatMs = new MemoryStream();
        idatMs.WriteByte(0x78); // CMF: deflate, 32K window
        idatMs.WriteByte(0x01); // FLG: FLEVEL=0, FCHECK=1

        using (var deflate = new DeflateStream(idatMs, CompressionLevel.Fastest, leaveOpen: true))
        {
            deflate.Write(rawFilteredData, 0, rawFilteredData.Length);
        }

        // Adler-32
        var adler = new Adler32();
        adler.Update(rawFilteredData.AsSpan());
        Span<byte> adlerBuf = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(adlerBuf, adler.Value);
#if NETSTANDARD2_0
        idatMs.Write(adlerBuf.ToArray(), 0, 4);
#else
        idatMs.Write(adlerBuf);
#endif

        // Write IDAT chunk(s)
        var buf = idatMs.GetBuffer();
        int len = (int)idatMs.Position;
        int off = 0;
        while (off < len)
        {
            int chunkSize = Math.Min(65536, len - off);
            PngChunkWriter.WriteChunk(output, "IDAT"u8, buf.AsSpan(off, chunkSize));
            off += chunkSize;
        }

        PngChunkWriter.WriteIend(output);
    }

    /// <summary>
    /// Writes raw filtered PNG image data to a file.
    /// </summary>
    public static void WriteFilteredRgba(string path, int width, int height, byte[] rawFilteredData)
    {
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 65536);
        WriteFilteredRgba(fs, width, height, rawFilteredData);
    }
}
