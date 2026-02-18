using System;
using System.IO;
using System.Text;

namespace TEdit.Terraria;

public static class WorldHeaderPatcher
{
    private static readonly object _fileLock = new();

    private const int OffsetVersion = 0x00;
    private const int OffsetMagic = 0x04;
    private const int OffsetFileType = 0x0B;
    private const int OffsetFlags = 0x10;
    private const int MinVersion = 140;
    private const int CompressedMagic = 0x1AA2227E;

    /// <summary>
    /// Reads the world header by delegating to <see cref="World.ReadWorldHeader"/>.
    /// </summary>
    public static WorldHeaderInfo? ReadHeader(string filePath)
    {
        return World.ReadWorldHeader(filePath);
    }

    /// <summary>
    /// Patches the IsFavorite flag in-place at offset 0x10 (bit 0 of the UInt64 flags field).
    /// Only works for uncompressed world files with version >= 140.
    /// </summary>
    public static void SetFavorite(string filePath, bool isFavorite)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("World file not found.", filePath);

        lock (_fileLock)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            // Check for compressed world
            if (fs.Length < OffsetFlags + 8)
                throw new InvalidOperationException("File is too small to be a valid world file.");

            var reader = new BinaryReader(fs, Encoding.UTF8, leaveOpen: true);

            uint version = reader.ReadUInt32();
            if (version == unchecked((uint)CompressedMagic))
                throw new InvalidOperationException("Cannot patch favorite flag on compressed (console) world files.");

            if (version < MinVersion)
                throw new InvalidOperationException($"World version {version} is below the minimum ({MinVersion}) required for favorite flag support.");

            // Read and validate magic string
            byte[] magicBytes = reader.ReadBytes(7);
            string magic = Encoding.UTF8.GetString(magicBytes);

            if (magic != WorldConfiguration.DesktopHeader && magic != WorldConfiguration.ChineseHeader)
                throw new InvalidOperationException($"Invalid world header magic: \"{magic}\".");

            // Validate file type
            byte fileType = reader.ReadByte();
            if (fileType != (byte)FileType.World)
                throw new InvalidOperationException($"File type {fileType} is not a world file (expected {(byte)FileType.World}).");

            // Skip FileRevision (4 bytes) - we're now at offset 0x0C, need to get to 0x10
            // Actually we've already read: 4 (version) + 7 (magic) + 1 (fileType) = 12 bytes = 0x0C
            // Read FileRevision to advance to 0x10
            reader.ReadUInt32(); // FileRevision

            // Read current flags at offset 0x10
            ulong flags = reader.ReadUInt64();

            // Set or clear bit 0
            ulong newFlags = isFavorite
                ? flags | 1uL
                : flags & ~1uL;

            // Write back if changed
            if (newFlags != flags)
            {
                fs.Position = OffsetFlags;
                var writer = new BinaryWriter(fs, Encoding.UTF8, leaveOpen: true);
                writer.Write(newFlags);
                writer.Flush();
            }
        }
    }
}
