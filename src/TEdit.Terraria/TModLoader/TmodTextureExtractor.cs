using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

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

    private Dictionary<string, ExtractedTexture> ExtractTextures(string category)
    {
        var results = new Dictionary<string, ExtractedTexture>(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int skippedOverlay = 0;
        int skippedDuplicate = 0;
        int skippedEmpty = 0;

        string[] prefixes =
        [
            $"Content/{category}/",
            $"{ModName}/Content/{category}/",
            $"{category}/",
            $"Assets/{category}/",
        ];

        foreach (var prefix in prefixes)
        {
            foreach (var filePath in ListFiles(prefix))
            {
                bool isPng = filePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
                bool isRawImg = filePath.EndsWith(".rawimg", StringComparison.OrdinalIgnoreCase);

                if (!isPng && !isRawImg)
                    continue;

                string fileName = Path.GetFileNameWithoutExtension(filePath);

                // Skip glow masks, highlights, flame overlays, etc.
                if (IsOverlayTexture(fileName))
                {
                    skippedOverlay++;
                    continue;
                }

                if (!seen.Add(fileName))
                {
                    skippedDuplicate++;
                    continue;
                }

                byte[] data = GetFile(filePath);
                if (data == null || data.Length == 0)
                {
                    skippedEmpty++;
                    Debug.WriteLine($"TmodTextureExtractor: Empty/null data for {filePath}");
                    continue;
                }

                results[fileName] = new ExtractedTexture
                {
                    Data = data,
                    IsRawImg = isRawImg,
                    SourcePath = filePath,
                };
            }
        }

        Debug.WriteLine($"TmodTextureExtractor: {ModName} {category}: {results.Count} textures extracted, {skippedOverlay} overlays skipped, {skippedDuplicate} duplicates, {skippedEmpty} empty");
        return results;
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

    private IEnumerable<string> ListFiles(string prefix)
    {
        foreach (var path in _files.Keys)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                yield return path;
        }
    }

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
