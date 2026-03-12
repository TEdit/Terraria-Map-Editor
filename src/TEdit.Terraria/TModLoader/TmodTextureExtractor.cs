using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using TEdit.Common;

namespace TEdit.Terraria.TModLoader;

/// <summary>
/// Reads .tmod archive files and extracts tile/wall texture data (PNG or rawimg bytes).
/// Ported from TEdit.ModScraper.TmodArchive for use in the main library.
/// </summary>
public class TmodTextureExtractor
{
    public string ModName { get; private set; } = string.Empty;
    public string ModVersion { get; private set; } = string.Empty;

    private readonly Dictionary<string, FileEntry> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _archivePath;

    private TmodTextureExtractor(string path)
    {
        _archivePath = path;
    }

    /// <summary>
    /// Opens a .tmod archive and parses its file table.
    /// </summary>
    public static TmodTextureExtractor Open(string path)
    {
        Debug.WriteLine($"TmodTextureExtractor: Opening archive {path}");
        var extractor = new TmodTextureExtractor(path);
        extractor.ReadHeader();
        Debug.WriteLine($"TmodTextureExtractor: Parsed {extractor.ModName} v{extractor.ModVersion}, {extractor._files.Count} files in archive");
        return extractor;
    }

    /// <summary>
    /// Extracts all tile textures from this archive.
    /// Returns a dictionary mapping tile name → raw PNG/rawimg bytes.
    /// </summary>
    public Dictionary<string, ExtractedTexture> ExtractTileTextures()
    {
        return ExtractTextures("Tiles");
    }

    /// <summary>
    /// Extracts all wall textures from this archive.
    /// Returns a dictionary mapping wall name → raw PNG/rawimg bytes.
    /// </summary>
    public Dictionary<string, ExtractedTexture> ExtractWallTextures()
    {
        return ExtractTextures("Walls");
    }

    /// <summary>
    /// Extracts all item textures from this archive.
    /// Returns a dictionary mapping item name → raw PNG/rawimg bytes.
    /// </summary>
    public Dictionary<string, ExtractedTexture> ExtractItemTextures()
    {
        return ExtractTextures("Items");
    }

    private Dictionary<string, ExtractedTexture> ExtractTextures(string category)
    {
        var results = new Dictionary<string, ExtractedTexture>(StringComparer.OrdinalIgnoreCase);
        int skippedOverlay = 0;
        int skippedDuplicate = 0;
        int skippedEmpty = 0;

        // Match paths containing the category as a directory segment.
        // Two-pass approach: first collect root-level paths (e.g. "Tiles/Ores/X.rawimg")
        // then nested paths (e.g. "Content/Tiles/X.rawimg"). Root paths take priority
        // so that actual tile sprites aren't shadowed by item icons from
        // "Items/Placeables/Tiles/X.rawimg".
        string catStartFwd = $"{category}/";
        string catStartBack = $@"{category}\";
        string catSegFwd = $"/{category}/";
        string catSegBack = $@"\{category}\";

        // Pass 1: root-level category paths (highest priority)
        foreach (var filePath in _files.Keys)
        {
            if (!filePath.StartsWith(catStartFwd, StringComparison.OrdinalIgnoreCase) &&
                !filePath.StartsWith(catStartBack, StringComparison.OrdinalIgnoreCase))
                continue;

            AddTextureResult(filePath, results, ref skippedOverlay, ref skippedDuplicate, ref skippedEmpty);
        }

        // Pass 2: nested category paths (e.g. "Content/Tiles/X", "CalamityMod/Tiles/X")
        // Skip paths under Items/ to avoid item icons shadowing tile textures.
        foreach (var filePath in _files.Keys)
        {
            if (filePath.IndexOf(catSegFwd, StringComparison.OrdinalIgnoreCase) < 0 &&
                filePath.IndexOf(catSegBack, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            // Skip Items/ paths — they contain small item icons, not tile/wall sprites
            if (filePath.StartsWith("Items/", StringComparison.OrdinalIgnoreCase) ||
                filePath.StartsWith(@"Items\", StringComparison.OrdinalIgnoreCase))
                continue;

            AddTextureResult(filePath, results, ref skippedOverlay, ref skippedDuplicate, ref skippedEmpty);
        }

        // Fallback: if category-based scan found nothing, scan ALL textures in the archive.
        // Some mods use flat or non-standard folder structures (e.g. MagicStorage uses
        // Components/ and Stations/ instead of Tiles/). Two-pass fallback: non-Items/
        // paths first so actual sprites take priority over item icons.
        if (results.Count == 0)
        {
            Debug.WriteLine($"TmodTextureExtractor: {ModName} {category}: No category matches, falling back to full archive scan");

            // Fallback pass 1: non-Items/ paths (actual tile/wall sprites)
            foreach (var filePath in _files.Keys)
            {
                if (filePath.StartsWith("Items/", StringComparison.OrdinalIgnoreCase) ||
                    filePath.StartsWith(@"Items\", StringComparison.OrdinalIgnoreCase))
                    continue;
                AddTextureResult(filePath, results, ref skippedOverlay, ref skippedDuplicate, ref skippedEmpty);
            }

            // Fallback pass 2: Items/ paths (only if name not already found above)
            foreach (var filePath in _files.Keys)
            {
                if (!filePath.StartsWith("Items/", StringComparison.OrdinalIgnoreCase) &&
                    !filePath.StartsWith(@"Items\", StringComparison.OrdinalIgnoreCase))
                    continue;
                AddTextureResult(filePath, results, ref skippedOverlay, ref skippedDuplicate, ref skippedEmpty);
            }
        }

        Debug.WriteLine($"TmodTextureExtractor: {ModName} {category}: {results.Count} textures extracted, {skippedOverlay} overlays skipped, {skippedDuplicate} duplicates, {skippedEmpty} empty");
        return results;
    }

    private bool AddTextureResult(string filePath, Dictionary<string, ExtractedTexture> results,
        ref int skippedOverlay, ref int skippedDuplicate, ref int skippedEmpty)
    {
        bool isPng = filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        bool isRawImg = filePath.EndsWith(".rawimg", StringComparison.OrdinalIgnoreCase);
        if (!isPng && !isRawImg) return false;

        string fileName = Path.GetFileNameWithoutExtension(filePath);

        if (IsOverlayTexture(fileName))
        {
            skippedOverlay++;
            return false;
        }

        if (results.ContainsKey(fileName))
        {
            skippedDuplicate++;
            return false;
        }

        byte[] data = GetFile(filePath);
        if (data == null || data.Length == 0)
        {
            skippedEmpty++;
            Debug.WriteLine($"TmodTextureExtractor: Empty/null data for {filePath}");
            return false;
        }

        results[fileName] = new ExtractedTexture
        {
            Data = data,
            IsRawImg = isRawImg,
            SourcePath = filePath,
        };
        return true;
    }

    /// <summary>
    /// Converts rawimg format (4-byte width, 4-byte height, RGBA pixel data) to RGBA pixel array.
    /// Returns (width, height, rgbaPixels) or null if the data is invalid.
    /// </summary>
    /// <summary>
    /// Decodes tModLoader's rawimg format.
    /// Format: int32 version (must be 1), int32 width, int32 height, then RGBA pixel data.
    /// Also supports legacy 2-int header (width, height) if version field doesn't match.
    /// </summary>
    public static (int Width, int Height, byte[] Rgba)? DecodeRawImg(byte[] data)
    {
        if (data.Length < 12)
        {
            Debug.WriteLine($"TmodTextureExtractor: DecodeRawImg failed - data too short ({data.Length} bytes)");
            return null;
        }

        int headerOffset;
        int firstInt = BitConverter.ToInt32(data, 0);

        if (firstInt == 1)
        {
            // tModLoader rawimg format: version(1), width, height, RGBA
            headerOffset = 4;
        }
        else
        {
            // Legacy format without version prefix: width, height, RGBA
            headerOffset = 0;
        }

        int width = BitConverter.ToInt32(data, headerOffset);
        int height = BitConverter.ToInt32(data, headerOffset + 4);
        int pixelDataOffset = headerOffset + 8;
        int expectedSize = pixelDataOffset + width * height * 4;

        if (width <= 0 || height <= 0 || width > 8192 || height > 8192 || data.Length < expectedSize)
        {
            Debug.WriteLine($"TmodTextureExtractor: DecodeRawImg failed - invalid dimensions {width}x{height} or size mismatch (have {data.Length}, need {expectedSize}, headerOffset={headerOffset})");
            return null;
        }

        Debug.WriteLine($"TmodTextureExtractor: DecodeRawImg {width}x{height} (headerOffset={headerOffset}, {data.Length} bytes)");
        byte[] rgba = new byte[width * height * 4];
        Buffer.BlockCopy(data, pixelDataOffset, rgba, 0, rgba.Length);
        return (width, height, rgba);
    }

    /// <summary>
    /// Reads texture width and height without allocating pixel data.
    /// Works for both PNG and rawimg formats.
    /// </summary>
    public static (int Width, int Height)? ReadTextureDimensions(byte[] data, bool isRawImg)
    {
        if (isRawImg)
        {
            if (data.Length < 12) return null;
            int headerOffset = BitConverter.ToInt32(data, 0) == 1 ? 4 : 0;
            int width = BitConverter.ToInt32(data, headerOffset);
            int height = BitConverter.ToInt32(data, headerOffset + 4);
            if (width <= 0 || height <= 0 || width > 8192 || height > 8192) return null;
            return (width, height);
        }

        // PNG: IHDR chunk at bytes 16-23 (big-endian width, height)
        if (data.Length < 24) return null;
        if (data[0] != 0x89 || data[1] != 0x50) return null; // Not PNG
        int w = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
        int h = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
        if (w <= 0 || h <= 0 || w > 8192 || h > 8192) return null;
        return (w, h);
    }

    /// <summary>
    /// Samples the average color from the first frame (top-left region) of a texture.
    /// For rawimg format, decodes pixels directly. For PNG, falls back to null (caller should use existing color).
    /// </summary>
    /// <param name="data">Raw texture bytes (rawimg or PNG).</param>
    /// <param name="isRawImg">True if data is rawimg format.</param>
    /// <param name="frameWidth">Width of the first frame to sample (default 16).</param>
    /// <param name="frameHeight">Height of the first frame to sample (default 16).</param>
    /// <returns>Average color of non-transparent pixels, or null if sampling failed.</returns>
    public static TEditColor? SampleFirstFrameColor(byte[] data, bool isRawImg, int frameWidth = 16, int frameHeight = 16)
    {
        if (!isRawImg)
            return null; // PNG decoding not available in library project; caller uses existing color

        var decoded = DecodeRawImg(data);
        if (decoded == null)
            return null;

        var (width, height, rgba) = decoded.Value;
        int sampleW = Math.Min(frameWidth, width);
        int sampleH = Math.Min(frameHeight, height);

        long totalR = 0, totalG = 0, totalB = 0;
        int count = 0;

        for (int y = 0; y < sampleH; y++)
        {
            for (int x = 0; x < sampleW; x++)
            {
                int offset = (y * width + x) * 4;
                byte a = rgba[offset + 3];
                if (a < 10) continue; // skip near-transparent pixels

                totalR += rgba[offset];
                totalG += rgba[offset + 1];
                totalB += rgba[offset + 2];
                count++;
            }
        }

        if (count == 0)
            return null;

        return new TEditColor(
            (byte)(totalR / count),
            (byte)(totalG / count),
            (byte)(totalB / count),
            (byte)255);
    }

    /// <summary>
    /// Computes the vertical frame stride from explicit CoordinateHeights.
    /// Stride = sum(heights) + (heights.Length - 1) * gapY + gapY (inter-frame gap).
    /// </summary>
    public static int ComputeStrideFromCoordinateHeights(short[] coordinateHeights, int gapY)
    {
        int sum = 0;
        for (int i = 0; i < coordinateHeights.Length; i++)
            sum += coordinateHeights[i];
        // Inter-row gaps + one inter-frame gap
        return sum + coordinateHeights.Length * gapY;
    }

    /// <summary>
    /// Computes the pixel height of one frame from CoordinateHeights (without trailing gap).
    /// FrameH = sum(heights) + (heights.Length - 1) * gapY.
    /// </summary>
    public static int ComputeFrameHeightFromCoordinateHeights(short[] coordinateHeights, int gapY)
    {
        int sum = 0;
        for (int i = 0; i < coordinateHeights.Length; i++)
            sum += coordinateHeights[i];
        return sum + (coordinateHeights.Length - 1) * gapY;
    }

    /// <summary>
    /// Detects the actual vertical frame stride by probing the texture for transparent gap rows.
    /// Terraria tiles commonly use CoordinateHeights where the bottom row is 2px taller (e.g. {16,18}
    /// for chests), causing a +2 stride shift per frame compared to uniform grid.
    /// Returns the detected stride, or the uniform stride if detection fails.
    /// </summary>
    /// <param name="data">Raw texture bytes (rawimg or PNG).</param>
    /// <param name="isRawImg">True if data is rawimg format.</param>
    /// <param name="gridY">Grid cell height (typically 16).</param>
    /// <param name="gapY">Gap between cells (typically 2).</param>
    /// <param name="fh">Frame height in tiles.</param>
    /// <returns>The vertical stride in pixels from one frame's start to the next.</returns>
    public static int DetectVerticalFrameStride(byte[] data, bool isRawImg, int gridY, int gapY, int fh)
    {
        int uniformStride = (gridY + gapY) * fh;
        if (fh <= 1 || !isRawImg) return uniformStride;

        var decoded = DecodeRawImg(data);
        if (decoded == null) return uniformStride;

        var (width, height, rgba) = decoded.Value;
        if (height <= uniformStride) return uniformStride;

        // Check if row at uniformStride is a transparent gap (uniform grid is correct)
        if (IsTransparentRow(rgba, width, uniformStride))
            return uniformStride;

        // Try the +2 bottom row variant: CoordinateHeights = {gridY, ..., gridY, gridY+2}
        // Frame height = (fh-1)*(gridY+gapY) + (gridY+2), stride = that + gapY
        int extendedStride = uniformStride + 2;
        if (extendedStride < height && IsTransparentRow(rgba, width, extendedStride))
            return extendedStride;

        // Try +4 for tiles with 2 extended rows
        int stride4 = uniformStride + 4;
        if (stride4 < height && IsTransparentRow(rgba, width, stride4))
            return stride4;

        return uniformStride;
    }

    /// <summary>
    /// Detects frame size in tiles by checking whether content bridges across cell boundaries.
    /// If the edge pixels of adjacent cells both have content, they belong to the same frame.
    /// Small 1x1 tiles like caged lights have padding and don't bridge; multi-cell tiles like
    /// chests/tables span the full cell and bridge across boundaries.
    /// Falls back to (cols, rows) — i.e. single frame — when pixel data is unavailable.
    /// </summary>
    public static (short FrameWidth, short FrameHeight) DetectFrameSize(
        byte[] data, bool isRawImg, int gridX, int gridY, int gapX, int gapY, int cols, int rows)
    {
        if (!isRawImg || cols < 1 || rows < 1)
            return ((short)cols, (short)rows);

        var decoded = DecodeRawImg(data);
        if (decoded == null)
            return ((short)cols, (short)rows);

        var (width, height, rgba) = decoded.Value;
        return DetectFrameSizeFromRgba(rgba, width, height, gridX, gridY, gapX, gapY, cols, rows);
    }

    /// <summary>
    /// Detects frame size from already-decoded RGBA pixel data.
    /// Used by the sprite editor where pixels are extracted from a BitmapSource.
    /// </summary>
    public static (short FrameWidth, short FrameHeight) DetectFrameSizeFromRgba(
        byte[] rgba, int width, int height, int gridX, int gridY, int gapX, int gapY, int cols, int rows)
    {
        if (cols < 1 || rows < 1)
            return ((short)cols, (short)rows);

        // Detect horizontal frame width: check successive column boundaries
        short fw = 1;
        for (int c = 0; c < cols - 1; c++)
        {
            // Right edge of cell c (last content pixel column)
            int rightEdgeX = c * (gridX + gapX) + gridX - 1;
            // Left edge of cell c+1 (first content pixel column)
            int leftEdgeX = (c + 1) * (gridX + gapX);

            if (rightEdgeX < 0 || leftEdgeX >= width) break;

            // Scan within the first row of cells (y from 0 to gridY)
            if (HasContentBridge(rgba, width, rightEdgeX, leftEdgeX,
                    0, Math.Min(gridY, height), isHorizontal: true))
                fw++;
            else
                break;
        }

        // Detect vertical frame height: check successive row boundaries
        short fh = 1;
        for (int r = 0; r < rows - 1; r++)
        {
            // Bottom edge of cell r (last content pixel row)
            int bottomEdgeY = r * (gridY + gapY) + gridY - 1;
            // Top edge of cell r+1 (first content pixel row)
            int topEdgeY = (r + 1) * (gridY + gapY);

            if (bottomEdgeY < 0 || topEdgeY >= height) break;

            // Scan within the first column of cells (x from 0 to gridX)
            if (HasContentBridge(rgba, width, bottomEdgeY, topEdgeY,
                    0, Math.Min(gridX, width), isHorizontal: false))
                fh++;
            else
                break;
        }

        return (fw, fh);
    }

    /// <summary>
    /// Checks if content reaches both sides of a cell boundary, indicating the cells
    /// belong to the same multi-cell frame. Instead of checking just the edge pixel column,
    /// checks a band of pixels near the edge (last/first 3 columns) to handle sprites with
    /// natural padding, anti-aliasing, or curved edges that don't reach the absolute last pixel.
    /// </summary>
    private static bool HasContentBridge(byte[] rgba, int width,
        int edge1, int edge2, int scanStart, int scanEnd, bool isHorizontal)
    {
        int range = scanEnd - scanStart;
        if (range <= 0) return false;

        // Check a band near each edge: 3 pixels deep (edge pixel ± 2 inward)
        const int bandDepth = 3;
        int samples = Math.Min(range, 8);
        int step = Math.Max(1, range / samples);
        int reachesLeft = 0;  // count of scan positions where side 1 has edge content
        int reachesRight = 0; // count of scan positions where side 2 has edge content
        int totalSampled = 0;

        for (int i = scanStart; i < scanEnd; i += step)
        {
            totalSampled++;
            bool side1 = false;
            bool side2 = false;

            // Check band near edge1 (inward = toward center of cell, so edge1, edge1-1, edge1-2)
            for (int d = 0; d < bandDepth && !side1; d++)
            {
                int pos1 = edge1 - d; // edge1 is right/bottom edge, so go inward (subtract)
                if (pos1 < 0) break;
                int offset1 = isHorizontal
                    ? (i * width + pos1) * 4
                    : (pos1 * width + i) * 4;
                if (offset1 + 3 < rgba.Length && rgba[offset1 + 3] >= 10)
                    side1 = true;
            }

            // Check band near edge2 (inward = edge2, edge2+1, edge2+2)
            for (int d = 0; d < bandDepth && !side2; d++)
            {
                int pos2 = edge2 + d; // edge2 is left/top edge, so go inward (add)
                int offset2 = isHorizontal
                    ? (i * width + pos2) * 4
                    : (pos2 * width + i) * 4;
                if (offset2 + 3 < rgba.Length && rgba[offset2 + 3] >= 10)
                    side2 = true;
            }

            if (side1) reachesLeft++;
            if (side2) reachesRight++;
        }

        // Both sides must have content near the edge for >25% of sampled positions
        return totalSampled > 0
            && reachesLeft > totalSampled / 4
            && reachesRight > totalSampled / 4;
    }

    /// <summary>
    /// Checks if a row in RGBA pixel data is fully transparent (all pixels have alpha &lt; 10).
    /// Samples up to 8 evenly-spaced pixels across the row for performance.
    /// </summary>
    private static bool IsTransparentRow(byte[] rgba, int width, int y)
    {
        // Sample up to 8 pixels across the row
        int samples = Math.Min(width, 8);
        int step = Math.Max(1, width / samples);
        for (int x = 0; x < width; x += step)
        {
            int offset = (y * width + x) * 4;
            if (offset + 3 >= rgba.Length) return false;
            if (rgba[offset + 3] >= 10) return false; // non-transparent pixel found
        }
        return true;
    }

    private static bool IsOverlayTexture(string fileName)
    {
        return fileName.EndsWith("_Glow", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_Highlight", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_Flame", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_Map", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_Wings", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_Head", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_Body", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith("_Legs", StringComparison.OrdinalIgnoreCase);
    }

    #region .tmod Archive Reading

    private byte[] GetFile(string internalPath)
    {
        if (!_files.TryGetValue(internalPath, out var entry))
            return null;

        using var fs = File.OpenRead(_archivePath);
        fs.Position = entry.Offset;

        byte[] data;
        if (entry.CompressedLength > 0 && entry.CompressedLength != entry.UncompressedLength)
        {
            byte[] compressed = new byte[entry.CompressedLength];
            fs.ReadExactly(compressed);
            data = Decompress(compressed, entry.UncompressedLength);
        }
        else
        {
            data = new byte[entry.UncompressedLength];
            fs.ReadExactly(data);
        }

        return data;
    }

    public IEnumerable<string> ListAllFiles() => _files.Keys;

    private void ReadHeader()
    {
        using var fs = File.OpenRead(_archivePath);
        using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);

        byte[] magic = br.ReadBytes(4);
        if (Encoding.ASCII.GetString(magic) != "TMOD")
            throw new InvalidDataException($"Not a valid .tmod file: {_archivePath}");

        // tModLoader version
        br.ReadString();

        // SHA1 hash (20 bytes) + Signature (256 bytes)
        br.ReadBytes(20);
        br.ReadBytes(256);

        // Data length
        br.ReadInt32();

        // Mod name and version
        ModName = br.ReadString();
        ModVersion = br.ReadString();

        // File count
        int fileCount = br.ReadInt32();

        // Build file table
        var entries = new List<(string Path, int Uncompressed, int Compressed)>();
        for (int i = 0; i < fileCount; i++)
        {
            string path = br.ReadString();
            int uncompressedLength = br.ReadInt32();
            int compressedLength = br.ReadInt32();
            entries.Add((path, uncompressedLength, compressedLength));
        }

        // Compute file data offsets
        long offset = fs.Position;
        foreach (var (path, uncompressed, compressed) in entries)
        {
            int storedSize = (compressed > 0 && compressed != uncompressed) ? compressed : uncompressed;
            _files[path] = new FileEntry
            {
                Offset = offset,
                UncompressedLength = uncompressed,
                CompressedLength = compressed,
            };
            offset += storedSize;
        }
    }

    private static byte[] Decompress(byte[] compressed, int uncompressedLength)
    {
        using var input = new MemoryStream(compressed);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        byte[] result = new byte[uncompressedLength];
        int totalRead = 0;
        while (totalRead < uncompressedLength)
        {
            int read = deflate.Read(result, totalRead, uncompressedLength - totalRead);
            if (read == 0) break;
            totalRead += read;
        }
        return result;
    }

    private struct FileEntry
    {
        public long Offset;
        public int UncompressedLength;
        public int CompressedLength;
    }

    #endregion
}

/// <summary>
/// Represents an extracted texture from a .tmod archive.
/// </summary>
public class ExtractedTexture
{
    /// <summary>Raw bytes (PNG data or rawimg data).</summary>
    public byte[] Data { get; set; }

    /// <summary>True if data is in tModLoader's rawimg format, false if PNG.</summary>
    public bool IsRawImg { get; set; }

    /// <summary>Original path within the .tmod archive.</summary>
    public string SourcePath { get; set; }
}
