using System;
using System.Buffers.Binary;
using System.IO;

namespace TEdit.Png;

/// <summary>
/// Writes PNG chunks (length + type + data + CRC32) and the PNG signature.
/// </summary>
internal static class PngChunkWriter
{
    public static void WriteSignature(Stream output)
    {
        ReadOnlySpan<byte> sig = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
#if NETSTANDARD2_0
        output.Write(sig.ToArray(), 0, 8);
#else
        output.Write(sig);
#endif
    }

    public static void WriteIhdr(Stream output, int width, int height)
    {
        Span<byte> data = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(data, width);
        BinaryPrimitives.WriteInt32BigEndian(data.Slice(4), height);
        data[8] = 8;  // bit depth
        data[9] = 6;  // color type: RGBA
        data[10] = 0; // compression method
        data[11] = 0; // filter method
        data[12] = 0; // interlace method
        WriteChunk(output, "IHDR"u8, data);
    }

    public static void WriteIend(Stream output)
    {
        WriteChunk(output, "IEND"u8, ReadOnlySpan<byte>.Empty);
    }

    public static void WriteChunk(Stream output, ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> buf4 = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buf4, data.Length);
#if NETSTANDARD2_0
        output.Write(buf4.ToArray(), 0, 4);
        output.Write(type.ToArray(), 0, type.Length);
        if (data.Length > 0)
            output.Write(data.ToArray(), 0, data.Length);
#else
        output.Write(buf4);
        output.Write(type);
        if (data.Length > 0)
            output.Write(data);
#endif

        uint crc = PngCrc32.Compute(type, data);
        BinaryPrimitives.WriteUInt32BigEndian(buf4, crc);
#if NETSTANDARD2_0
        output.Write(buf4.ToArray(), 0, 4);
#else
        output.Write(buf4);
#endif
    }
}
