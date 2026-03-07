using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;

namespace TEdit.Png;

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
    private Adler32 _adler;

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
        _adler = new Adler32();

        _idatBuffer = new MemoryStream();
        // Zlib header: CMF=0x78 (deflate, 32K window), FLG with FLEVEL + FCHECK
        byte cmf = 0x78;
        int flevel = compressionLevel == CompressionLevel.Fastest ? 0 : 2;
        int flg = flevel << 6;
        int fcheck = (31 - ((cmf * 256 + flg) % 31)) % 31;
        flg |= fcheck;
        _idatBuffer.WriteByte(cmf);
        _idatBuffer.WriteByte((byte)flg);
        _deflate = new DeflateStream(_idatBuffer, compressionLevel, leaveOpen: true);

        PngChunkWriter.WriteSignature(_output);
        PngChunkWriter.WriteIhdr(_output, _width, _height);
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
        _adler.Update(filterByte);

#if NETSTANDARD2_0
        var buffer = rgbaRow.ToArray();
        _deflate.Write(buffer, 0, buffer.Length);
#else
        _deflate.Write(rgbaRow);
#endif
        _adler.Update(rgbaRow);

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

        _deflate.Dispose();

        // Zlib footer: Adler-32 checksum
        Span<byte> adler = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(adler, _adler.Value);
#if NETSTANDARD2_0
        _idatBuffer.Write(adler.ToArray(), 0, 4);
#else
        _idatBuffer.Write(adler);
#endif

        FlushIdatChunks();
        _idatBuffer.Dispose();

        PngChunkWriter.WriteIend(_output);
    }

    public void Dispose()
    {
        if (!_finished)
        {
            try { _deflate.Dispose(); } catch { }
            _idatBuffer.Dispose();
        }
    }

    private void FlushIdatChunks()
    {
        var buffer = _idatBuffer.GetBuffer();
        var length = (int)_idatBuffer.Position;
        var offset = 0;

        while (offset < length)
        {
            int chunkSize = Math.Min(MaxIdatChunkSize, length - offset);
            PngChunkWriter.WriteChunk(_output, "IDAT"u8, buffer.AsSpan(offset, chunkSize));
            offset += chunkSize;
        }

        _idatBuffer.Position = 0;
        _idatBuffer.SetLength(0);
    }
}
