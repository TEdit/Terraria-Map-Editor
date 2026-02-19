using System.IO.Compression;
using System.Text;

namespace TEdit.ModScraper;

/// <summary>
/// Reads .tmod archive files (tModLoader mod packages).
/// Parses the header and file table, and extracts individual files.
/// </summary>
public class TmodArchive
{
    public string ModName { get; private set; } = string.Empty;
    public string ModVersion { get; private set; } = string.Empty;
    public string TModLoaderVersion { get; private set; } = string.Empty;

    private readonly Dictionary<string, FileEntry> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _archivePath;
    private long _dataStartOffset;

    private TmodArchive(string path)
    {
        _archivePath = path;
    }

    /// <summary>
    /// Opens and parses a .tmod archive file.
    /// </summary>
    public static TmodArchive Open(string path)
    {
        var archive = new TmodArchive(path);
        archive.ReadHeader();
        return archive;
    }

    /// <summary>
    /// Extracts a single file from the archive by its internal path.
    /// Returns null if the file is not found.
    /// </summary>
    public byte[]? GetFile(string internalPath)
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

    /// <summary>
    /// Lists all files in the archive matching a path prefix.
    /// </summary>
    public IEnumerable<string> ListFiles(string? prefix = null)
    {
        foreach (var path in _files.Keys)
        {
            if (prefix == null || path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                yield return path;
        }
    }

    private void ReadHeader()
    {
        using var fs = File.OpenRead(_archivePath);
        using var br = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);

        // Magic: "TMOD"
        byte[] magic = br.ReadBytes(4);
        if (Encoding.ASCII.GetString(magic) != "TMOD")
            throw new InvalidDataException($"Not a valid .tmod file: {_archivePath}");

        // tModLoader version
        TModLoaderVersion = br.ReadString();

        // SHA1 hash (20 bytes)
        br.ReadBytes(20);

        // Signature (256 bytes)
        br.ReadBytes(256);

        // Data length (total remaining data)
        int dataLength = br.ReadInt32();
        long dataStart = fs.Position;

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

        // After the file table, the actual file data starts
        _dataStartOffset = fs.Position;
        long offset = _dataStartOffset;

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
}
