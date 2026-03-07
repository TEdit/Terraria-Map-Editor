using System.IO;
using System.Windows.Media.Imaging;
using Shouldly;
using TEdit.Png;
using Xunit;

namespace TEdit.Tests.Utility;

public class StreamingPngWriterTests
{
    [Fact]
    public void SinglePixelRedPng_IsValidAndCorrect()
    {
        var ms = new MemoryStream();

        using (var writer = new StreamingPngWriter(ms, 1, 1))
        {
            writer.WriteScanline(new byte[] { 255, 0, 0, 255 }); // red RGBA
            writer.Finish();
        }

        ms.Position = 0;
        var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
        var frame = decoder.Frames[0];

        frame.PixelWidth.ShouldBe(1);
        frame.PixelHeight.ShouldBe(1);

        var pixels = new byte[4];
        frame.CopyPixels(pixels, 4, 0);

        // WPF decodes as BGRA
        pixels[0].ShouldBe((byte)0);   // B
        pixels[1].ShouldBe((byte)0);   // G
        pixels[2].ShouldBe((byte)255); // R
        pixels[3].ShouldBe((byte)255); // A
    }

    [Fact]
    public void SmallImage_RoundTripsCorrectly()
    {
        const int width = 10;
        const int height = 5;
        var ms = new MemoryStream();

        using (var writer = new StreamingPngWriter(ms, width, height))
        {
            for (int y = 0; y < height; y++)
            {
                var row = new byte[width * 4];
                for (int x = 0; x < width; x++)
                {
                    int i = x * 4;
                    row[i] = (byte)(x * 25);       // R
                    row[i + 1] = (byte)(y * 50);    // G
                    row[i + 2] = 128;                // B
                    row[i + 3] = 255;                // A
                }
                writer.WriteScanline(row);
            }
            writer.Finish();
        }

        ms.Position = 0;
        var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
        var frame = decoder.Frames[0];

        frame.PixelWidth.ShouldBe(width);
        frame.PixelHeight.ShouldBe(height);

        var pixels = new byte[width * height * 4];
        frame.CopyPixels(pixels, width * 4, 0);

        // Verify a few known pixels (WPF returns BGRA)
        // Pixel (0,0): R=0, G=0, B=128, A=255
        pixels[0].ShouldBe((byte)128); // B
        pixels[1].ShouldBe((byte)0);   // G
        pixels[2].ShouldBe((byte)0);   // R
        pixels[3].ShouldBe((byte)255); // A

        // Pixel (5,2): R=125, G=100, B=128, A=255
        int offset = (2 * width + 5) * 4;
        pixels[offset].ShouldBe((byte)128);     // B
        pixels[offset + 1].ShouldBe((byte)100); // G
        pixels[offset + 2].ShouldBe((byte)125); // R
        pixels[offset + 3].ShouldBe((byte)255); // A
    }

    [Fact]
    public void TransparentPixels_PreserveAlpha()
    {
        var ms = new MemoryStream();

        using (var writer = new StreamingPngWriter(ms, 2, 1))
        {
            writer.WriteScanline(new byte[]
            {
                255, 0, 0, 128,  // semi-transparent red
                0, 255, 0, 0     // fully transparent green
            });
            writer.Finish();
        }

        ms.Position = 0;
        var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
        var frame = decoder.Frames[0];
        var pixels = new byte[8];
        frame.CopyPixels(pixels, 8, 0);

        // Pixel 0: semi-transparent red (BGRA)
        pixels[2].ShouldBe((byte)255); // R
        pixels[3].ShouldBe((byte)128); // A

        // Pixel 1: fully transparent green
        pixels[7].ShouldBe((byte)0); // A
    }

    [Fact]
    public void LargeImage_ExceedsIdatChunkSize()
    {
        // Force multiple IDAT chunks by writing a large-ish image
        const int width = 1024;
        const int height = 64;
        var ms = new MemoryStream();

        using (var writer = new StreamingPngWriter(ms, width, height))
        {
            var row = new byte[width * 4];
            for (int y = 0; y < height; y++)
            {
                // Fill with a pattern
                for (int x = 0; x < width; x++)
                {
                    int i = x * 4;
                    row[i] = (byte)(x & 0xFF);
                    row[i + 1] = (byte)(y & 0xFF);
                    row[i + 2] = (byte)((x + y) & 0xFF);
                    row[i + 3] = 255;
                }
                writer.WriteScanline(row);
            }
            writer.Finish();
        }

        ms.Position = 0;
        var decoder = new PngBitmapDecoder(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
        var frame = decoder.Frames[0];

        frame.PixelWidth.ShouldBe(width);
        frame.PixelHeight.ShouldBe(height);

        // Spot-check a pixel
        var pixels = new byte[width * height * 4];
        frame.CopyPixels(pixels, width * 4, 0);

        // Pixel (100, 30): R=100, G=30, B=130, A=255
        int offset = (30 * width + 100) * 4;
        pixels[offset].ShouldBe((byte)130);     // B
        pixels[offset + 1].ShouldBe((byte)30);  // G
        pixels[offset + 2].ShouldBe((byte)100); // R
        pixels[offset + 3].ShouldBe((byte)255); // A
    }

    [Fact]
    public void Finish_ThrowsIfRowCountMismatch()
    {
        var ms = new MemoryStream();
        using var writer = new StreamingPngWriter(ms, 2, 3);

        writer.WriteScanline(new byte[8]);
        // Only wrote 1 of 3 rows

        Should.Throw<InvalidOperationException>(() => writer.Finish());
    }

    [Fact]
    public void WriteScanline_ThrowsIfWrongLength()
    {
        var ms = new MemoryStream();
        using var writer = new StreamingPngWriter(ms, 4, 1);

        // Width is 4, so expect 16 bytes, not 8
        Should.Throw<ArgumentException>(() => writer.WriteScanline(new byte[8]));
    }

    [Fact]
    public void WriteScanline_ThrowsIfTooManyRows()
    {
        var ms = new MemoryStream();
        using var writer = new StreamingPngWriter(ms, 1, 1);

        writer.WriteScanline(new byte[4]);
        // Second row should throw
        Should.Throw<InvalidOperationException>(() => writer.WriteScanline(new byte[4]));
    }

    [Fact]
    public void Constructor_ThrowsForInvalidDimensions()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => new StreamingPngWriter(new MemoryStream(), 0, 1));
        Should.Throw<ArgumentOutOfRangeException>(() => new StreamingPngWriter(new MemoryStream(), 1, 0));
        Should.Throw<ArgumentOutOfRangeException>(() => new StreamingPngWriter(new MemoryStream(), -1, 1));
        Should.Throw<ArgumentNullException>(() => new StreamingPngWriter(null!, 1, 1));
    }

    [Fact]
    public void WriteScanline_AfterFinish_Throws()
    {
        var ms = new MemoryStream();
        using var writer = new StreamingPngWriter(ms, 1, 1);

        writer.WriteScanline(new byte[4]);
        writer.Finish();

        Should.Throw<InvalidOperationException>(() => writer.WriteScanline(new byte[4]));
    }

    [Fact]
    public void OutputStartsWithPngSignature()
    {
        var ms = new MemoryStream();
        using (var writer = new StreamingPngWriter(ms, 1, 1))
        {
            writer.WriteScanline(new byte[4]);
            writer.Finish();
        }

        ms.Position = 0;
        var sig = new byte[8];
        ms.Read(sig, 0, 8);

        sig.ShouldBe(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
    }

    [Fact]
    public void FileOutput_ProducesReadablePng()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"streaming_png_test_{Guid.NewGuid()}.png");
        try
        {
            const int width = 100;
            const int height = 50;

            using (var fs = File.Create(tempPath))
            using (var writer = new StreamingPngWriter(fs, width, height))
            {
                var row = new byte[width * 4];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = x * 4;
                        row[i] = (byte)(x * 2);
                        row[i + 1] = (byte)(y * 5);
                        row[i + 2] = 100;
                        row[i + 3] = 255;
                    }
                    writer.WriteScanline(row);
                }
                writer.Finish();
            }

            // Read back and verify
            using var fs2 = File.OpenRead(tempPath);
            var decoder = new PngBitmapDecoder(fs2, BitmapCreateOptions.None, BitmapCacheOption.Default);
            var frame = decoder.Frames[0];

            frame.PixelWidth.ShouldBe(width);
            frame.PixelHeight.ShouldBe(height);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
