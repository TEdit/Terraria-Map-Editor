using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;

namespace TEdit.Common.Utility;

/// <summary>
/// Writes RGBA pixel data to a PNG file one scanline at a time,
/// avoiding the need to hold the full image in memory.
/// Produces valid 8-bit RGBA (color type 6) PNG files.
/// </summary>
public sealed class StreamingPngWriter : IDisposable
{
    private readonly Stream _output;
    private readonly int _width;
    private readonly int _height;
    private readonly MemoryStream _idatBuffer;
    private readonly DeflateStream _deflate;
    private int _rowsWritten;
    private bool _finished;

    // PNG writes IDAT payload as zlib (2-byte header + deflate + 4-byte adler32).
    // We handle the zlib wrapper manually so we can stream through DeflateStream.
    private uint _adler_a = 1;
    private uint _adler_b;

    private const int BytesPerPixel = 4; // RGBA
    private const int MaxIdatChunkSize = 65536;

    public StreamingPngWriter(Stream output, int width, int height)
        : this(output, width, height, CompressionLevel.Optimal) { }

    public StreamingPngWriter(Stream output, int width, int height, CompressionLevel compressionLevel)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        _output = output ?? throw new ArgumentNullException(nameof(output));
        _width = width;
        _height = height;

        _idatBuffer = new MemoryStream();
        // Write zlib header: CMF=0x78 (deflate, 32K window), FLG encodes FLEVEL + FCHECK
        // FLEVEL: 0=fastest, 2=default. FCHECK must satisfy (CMF*256+FLG) % 31 == 0
        byte cmf = 0x78;
        int flevel = compressionLevel == CompressionLevel.Fastest ? 0 : 2;
        int flg = flevel << 6; // FLEVEL in top 2 bits, FDICT=0
        int fcheck = (31 - ((cmf * 256 + flg) % 31)) % 31;
        flg |= fcheck;
        _idatBuffer.WriteByte(cmf);
        _idatBuffer.WriteByte((byte)flg);
        _deflate = new DeflateStream(_idatBuffer, compressionLevel, leaveOpen: true);

        WritePngSignature();
        WriteIhdrChunk();
    }

    /// <summary>
    /// Writes one scanline of pixel data. Data must be exactly width × 4 bytes (RGBA).
    /// </summary>
    public void WriteScanline(ReadOnlySpan<byte> rgbaRow)
    {
        if (_finished) throw new InvalidOperationException("PNG is already finalized.");
        if (_rowsWritten >= _height) throw new InvalidOperationException($"Already wrote {_height} rows.");
        if (rgbaRow.Length != _width * BytesPerPixel)
            throw new ArgumentException($"Expected {_width * BytesPerPixel} bytes, got {rgbaRow.Length}.");

        // PNG filter byte: 0 = None
        byte filterByte = 0;
        _deflate.WriteByte(filterByte);
        UpdateAdler(filterByte);

#if NETSTANDARD2_0
        var buffer = rgbaRow.ToArray();
        _deflate.Write(buffer, 0, buffer.Length);
#else
        _deflate.Write(rgbaRow);
#endif
        UpdateAdler(rgbaRow);

        _rowsWritten++;

        // Flush periodically to keep memory bounded
        if (_idatBuffer.Length >= MaxIdatChunkSize)
        {
            _deflate.Flush();
            FlushIdatChunks();
        }
    }

    /// <summary>
    /// Writes one scanline from a byte array (convenience overload).
    /// </summary>
    public void WriteScanline(byte[] rgbaRow) => WriteScanline(rgbaRow.AsSpan());

    /// <summary>
    /// Writes one scanline from a byte array with offset and count.
    /// </summary>
    public void WriteScanline(byte[] rgbaRow, int offset, int count)
        => WriteScanline(rgbaRow.AsSpan(offset, count));

    /// <summary>
    /// Finalizes the PNG file. Must be called after all scanlines are written.
    /// </summary>
    public void Finish()
    {
        if (_finished) return;
        _finished = true;

        if (_rowsWritten != _height)
            throw new InvalidOperationException($"Expected {_height} rows but only {_rowsWritten} were written.");

        // Finalize deflate stream
        _deflate.Dispose();

        // Write Adler-32 checksum (zlib footer)
        Span<byte> adler = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(adler, (_adler_b << 16) | _adler_a);
#if NETSTANDARD2_0
        _idatBuffer.Write(adler.ToArray(), 0, 4);
#else
        _idatBuffer.Write(adler);
#endif

        // Flush remaining IDAT data
        FlushIdatChunks();
        _idatBuffer.Dispose();

        // Write IEND
        WriteChunk("IEND"u8, ReadOnlySpan<byte>.Empty);
    }

    public void Dispose()
    {
        if (!_finished)
        {
            try { _deflate.Dispose(); } catch { }
            _idatBuffer.Dispose();
        }
    }

    private void WritePngSignature()
    {
        ReadOnlySpan<byte> sig = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
#if NETSTANDARD2_0
        _output.Write(sig.ToArray(), 0, 8);
#else
        _output.Write(sig);
#endif
    }

    private void WriteIhdrChunk()
    {
        Span<byte> data = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(data, _width);
        BinaryPrimitives.WriteInt32BigEndian(data.Slice(4), _height);
        data[8] = 8;  // bit depth
        data[9] = 6;  // color type: RGBA
        data[10] = 0; // compression method
        data[11] = 0; // filter method
        data[12] = 0; // interlace method

#if NETSTANDARD2_0
        WriteChunkFromArray("IHDR"u8, data.ToArray());
#else
        WriteChunk("IHDR"u8, data);
#endif
    }

    private void FlushIdatChunks()
    {
        var buffer = _idatBuffer.GetBuffer();
        var length = (int)_idatBuffer.Position;
        var offset = 0;

        while (offset < length)
        {
            int chunkSize = Math.Min(MaxIdatChunkSize, length - offset);
            WriteChunk("IDAT"u8, buffer.AsSpan(offset, chunkSize));
            offset += chunkSize;
        }

        _idatBuffer.Position = 0;
        _idatBuffer.SetLength(0);
    }

    private void WriteChunk(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        Span<byte> lengthBuf = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(lengthBuf, data.Length);
#if NETSTANDARD2_0
        _output.Write(lengthBuf.ToArray(), 0, 4);
        _output.Write(type.ToArray(), 0, type.Length);
        if (data.Length > 0)
            _output.Write(data.ToArray(), 0, data.Length);
#else
        _output.Write(lengthBuf);
        _output.Write(type);
        if (data.Length > 0)
            _output.Write(data);
#endif

        // CRC32 over type + data
        uint crc = Crc32(type, data);
        Span<byte> crcBuf = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBuf, crc);
#if NETSTANDARD2_0
        _output.Write(crcBuf.ToArray(), 0, 4);
#else
        _output.Write(crcBuf);
#endif
    }

#if NETSTANDARD2_0
    private void WriteChunkFromArray(ReadOnlySpan<byte> type, byte[] data)
    {
        WriteChunk(type, data.AsSpan());
    }
#endif

    private void UpdateAdler(byte b)
    {
        _adler_a = (_adler_a + b) % 65521;
        _adler_b = (_adler_b + _adler_a) % 65521;
    }

    private void UpdateAdler(ReadOnlySpan<byte> data)
    {
        // Process in blocks to avoid overflow
        const int nmax = 5552; // largest n such that 255*n*(n+1)/2 + (n+1)*(65520) <= 2^32-1
        int remaining = data.Length;
        int offset = 0;

        while (remaining > 0)
        {
            int blockLen = Math.Min(remaining, nmax);
            for (int i = 0; i < blockLen; i++)
            {
                _adler_a += data[offset + i];
                _adler_b += _adler_a;
            }
            _adler_a %= 65521;
            _adler_b %= 65521;
            offset += blockLen;
            remaining -= blockLen;
        }
    }

    // PNG CRC32 (ISO 3309 / ITU-T V.42)
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

    private static uint Crc32(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFF;
        for (int i = 0; i < type.Length; i++)
            crc = Crc32Table[(crc ^ type[i]) & 0xFF] ^ (crc >> 8);
        for (int i = 0; i < data.Length; i++)
            crc = Crc32Table[(crc ^ data[i]) & 0xFF] ^ (crc >> 8);
        return crc ^ 0xFFFFFFFF;
    }
}
