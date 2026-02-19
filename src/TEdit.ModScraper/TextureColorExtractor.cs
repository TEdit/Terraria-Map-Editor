using SkiaSharp;
using TEdit.Common;

namespace TEdit.ModScraper;

/// <summary>
/// Extracts representative colors from tile/wall textures.
/// Supports both PNG and tModLoader's .rawimg format.
/// </summary>
public static class TextureColorExtractor
{
    /// <summary>
    /// Samples the average color from texture data (PNG or rawimg format).
    /// </summary>
    public static TEditColor SampleAverageColor(byte[] data, bool isRawImg = false)
    {
        if (isRawImg)
            return SampleRawImg(data);

        return SamplePng(data);
    }

    /// <summary>
    /// Loads a PNG from raw bytes and samples the average color of non-transparent pixels.
    /// </summary>
    private static TEditColor SamplePng(byte[] pngData)
    {
        using var bitmap = SKBitmap.Decode(pngData);
        if (bitmap == null)
            return FallbackGray;

        return SampleBitmap(bitmap);
    }

    /// <summary>
    /// Reads tModLoader's .rawimg format: 4-byte width, 4-byte height, then RGBA pixel data.
    /// </summary>
    private static TEditColor SampleRawImg(byte[] data)
    {
        if (data.Length < 8)
            return FallbackGray;

        int width = BitConverter.ToInt32(data, 0);
        int height = BitConverter.ToInt32(data, 4);
        int expectedSize = 8 + width * height * 4;

        if (width <= 0 || height <= 0 || width > 8192 || height > 8192 || data.Length < expectedSize)
            return FallbackGray;

        double totalR = 0, totalG = 0, totalB = 0;
        double totalWeight = 0;
        int offset = 8;

        for (int i = 0; i < width * height; i++)
        {
            byte r = data[offset];
            byte g = data[offset + 1];
            byte b = data[offset + 2];
            byte a = data[offset + 3];
            offset += 4;

            if (a < 10) continue;

            double luminance = 0.299 * r + 0.587 * g + 0.114 * b;
            double weight = (0.5 + luminance / 510.0) * (a / 255.0);

            totalR += r * weight;
            totalG += g * weight;
            totalB += b * weight;
            totalWeight += weight;
        }

        if (totalWeight < 0.001)
            return FallbackGray;

        return new TEditColor(
            (byte)Math.Clamp(totalR / totalWeight, 0, 255),
            (byte)Math.Clamp(totalG / totalWeight, 0, 255),
            (byte)Math.Clamp(totalB / totalWeight, 0, 255),
            (byte)255);
    }

    private static TEditColor SampleBitmap(SKBitmap bitmap)
    {
        double totalR = 0, totalG = 0, totalB = 0;
        double totalWeight = 0;

        var pixels = bitmap.Pixels;
        for (int i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];
            if (pixel.Alpha < 10) continue;

            double luminance = 0.299 * pixel.Red + 0.587 * pixel.Green + 0.114 * pixel.Blue;
            double weight = (0.5 + luminance / 510.0) * (pixel.Alpha / 255.0);

            totalR += pixel.Red * weight;
            totalG += pixel.Green * weight;
            totalB += pixel.Blue * weight;
            totalWeight += weight;
        }

        if (totalWeight < 0.001)
            return FallbackGray;

        return new TEditColor(
            (byte)Math.Clamp(totalR / totalWeight, 0, 255),
            (byte)Math.Clamp(totalG / totalWeight, 0, 255),
            (byte)Math.Clamp(totalB / totalWeight, 0, 255),
            (byte)255);
    }

    private static readonly TEditColor FallbackGray = new TEditColor((byte)128, (byte)128, (byte)128, (byte)255);
}
