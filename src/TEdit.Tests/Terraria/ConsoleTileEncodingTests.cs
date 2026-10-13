using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using TEdit.Terraria;
using Xunit;
using Xunit.Abstractions;

namespace TEdit.Tests.Terraria;

public class ConsoleTileEncodingTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("console.wld", 32)]
    [InlineData("Challenge.wld", 0)]
    [InlineData("MAINWORLD.wld", 0)]
    public void Format279_RoundTrip_PreservesEveryTilePayloadAndNonTileSection(string name, int expectedIgnoredShapes)
    {
        string source = Path.Combine(AppContext.BaseDirectory, "WorldFiles", name);
        byte[] original = ReadConsole(source);
        var before = ReadHeader(original);
        string temporary = Path.Combine(Path.GetTempPath(), $"tedit-console-audit-{Guid.NewGuid():N}.wld");
        try
        {
            var (world, error) = World.LoadWorld(source);
            Assert.Null(error);
            Assert.True(world.IsConsole);
            Assert.Equal(279u, world.Version);
            World.Save(world, temporary, incrementRevision: false);
            byte[] saved = ReadConsole(temporary);
            var after = ReadHeader(saved);
            Assert.Equal(before.Width, after.Width);
            Assert.Equal(before.Height, after.Height);
            Assert.Equal(before.Frames, after.Frames);
            output.WriteLine($"{name}: version 279 -> 279; {before.Width}x{before.Height}; raw bytes {original.Length} -> {saved.Length}");
            output.WriteLine($"Original SHA256: {Convert.ToHexString(SHA256.HashData(original))}");
            output.WriteLine($"Saved SHA256: {Convert.ToHexString(SHA256.HashData(saved))}");

            // Section pointer values may move, but every other file-header byte must survive.
            var originalHeader = original[..before.Pointers[0]];
            var savedHeader = saved[..after.Pointers[0]];
            originalHeader.AsSpan(26, 44).Clear();
            savedHeader.AsSpan(26, 44).Clear();
            Assert.Equal(originalHeader, savedHeader);
            var changedSections = new List<int>();
            for (int section = 0; section < before.Pointers.Length; section++)
            {
                int originalEnd = section + 1 < before.Pointers.Length ? before.Pointers[section + 1] : original.Length;
                int savedEnd = section + 1 < after.Pointers.Length ? after.Pointers[section + 1] : saved.Length;
                var originalBytes = original.AsSpan(before.Pointers[section], originalEnd - before.Pointers[section]);
                var savedBytes = saved.AsSpan(after.Pointers[section], savedEnd - after.Pointers[section]);
                bool identical = originalBytes.SequenceEqual(savedBytes);
                output.WriteLine($"Section {section}: {originalBytes.Length} -> {savedBytes.Length} bytes; identical={identical}");
                if (section != 1 && !identical)
                {
                    changedSections.Add(section);
                    int shown = 0;
                    for (int i = 0; i < Math.Min(originalBytes.Length, savedBytes.Length) && shown < 12; i++)
                    {
                        if (originalBytes[i] != savedBytes[i])
                        {
                            output.WriteLine($"  byte +0x{i:X} (original 0x{before.Pointers[section] + i:X}): {originalBytes[i]:X2} -> {savedBytes[i]:X2}");
                            shown++;
                        }
                    }
                }
            }

            // Optional investigation artifacts. Normal test runs only use temporary files.
            string? evidence = Environment.GetEnvironmentVariable("TEDIT_CONSOLE_AUDIT_DIR");
            if (!string.IsNullOrEmpty(evidence))
            {
                Directory.CreateDirectory(evidence);
                File.WriteAllBytes(Path.Combine(evidence, name + ".original.raw"), original);
                File.WriteAllBytes(Path.Combine(evidence, name + ".saved.raw"), saved);
            }

            var originalReader = new TileRecords(original, before);
            var savedReader = new TileRecords(saved, after);
            int changedPayloads = 0;
            int ignoredShapes = 0;
            int changedRunExamples = 0;
            int originalRemaining = 0, savedRemaining = 0;
            Record originalRecord = default, savedRecord = default;
            for (int x = 0; x < before.Width; x++)
            {
                for (int y = 0; y < before.Height;)
                {
                    if (originalRemaining == 0)
                    {
                        originalRecord = originalReader.Read(before.Height - y);
                        originalRemaining = originalRecord.Count;
                    }
                    if (savedRemaining == 0)
                    {
                        savedRecord = savedReader.Read(before.Height - y);
                        savedRemaining = savedRecord.Count;
                    }
                    int count = Math.Min(originalRemaining, savedRemaining);
                    bool samePayload = (original[originalRecord.Offset] & 63) == (saved[savedRecord.Offset] & 63)
                        && original.AsSpan(originalRecord.Offset + 1, originalRecord.PayloadLength - 1)
                            .SequenceEqual(saved.AsSpan(savedRecord.Offset + 1, savedRecord.PayloadLength - 1));
                    if (!samePayload && IsReferenceIgnoredShapeRemoval(original, originalRecord, saved, savedRecord))
                    {
                        ignoredShapes += count;
                        samePayload = true;
                    }
                    if (!samePayload)
                    {
                        if (changedPayloads < 20)
                        {
                            output.WriteLine($"PAYLOAD DIFFERENCE [{x},{y}] count={count}: original @{originalRecord.Offset:X} {originalReader.Hex(originalRecord)}; saved @{savedRecord.Offset:X} {savedReader.Hex(savedRecord)}");
                        }
                        changedPayloads += count;
                    }
                    else if (originalRecord.Count != savedRecord.Count && changedRunExamples < 8)
                    {
                        output.WriteLine($"RLE difference [{x},{y}]: original @{originalRecord.Offset:X} {originalReader.Hex(originalRecord)} count={originalRecord.Count}; saved @{savedRecord.Offset:X} {savedReader.Hex(savedRecord)} count={savedRecord.Count}");
                        changedRunExamples++;
                    }
                    y += count;
                    originalRemaining -= count;
                    savedRemaining -= count;
                }
                Assert.Equal(0, originalRemaining);
                Assert.Equal(0, savedRemaining);
            }
            Assert.Equal(before.Pointers[2], originalReader.Position);
            Assert.Equal(after.Pointers[2], savedReader.Position);
            output.WriteLine($"Original records={originalReader.Count}; RLE modes={string.Join(",", originalReader.Modes)}; payload bytes={originalReader.PayloadBytes}; RLE bytes={originalReader.RleBytes}");
            output.WriteLine($"Saved records={savedReader.Count}; RLE modes={string.Join(",", savedReader.Modes)}; payload bytes={savedReader.PayloadBytes}; RLE bytes={savedReader.RleBytes}");
            output.WriteLine($"Compared {before.Width * before.Height} tile positions; changed non-RLE payloads={changedPayloads}");
            Assert.Equal(0, changedPayloads);
            Assert.Equal(expectedIgnoredShapes, ignoredShapes);
            Assert.Empty(changedSections);
        }
        finally
        {
            File.Delete(temporary);
            File.Delete(temporary + ".tmp");
        }
    }

    [Theory]
    [InlineData("031034", "0234", true)]
    [InlineData("031234", "030234", true)]
    [InlineData("23107E02", "227E02", true)]
    [InlineData("031001", "0201", false)] // Solid tile slope.
    [InlineData("031083", "0283", false)] // NonSolidSaveSlopes exception 131.
    [InlineData("031034", "032034", false)] // Different, nonzero shape.
    [InlineData("031234", "0234", false)] // Wire loss.
    [InlineData("07103401", "063402", false)] // Wall changed.
    [InlineData("0B10347F", "0A347E", false)] // Liquid amount changed.
    [InlineData("0311083403", "0301083404", false)] // Paint changed.
    [InlineData("03110234", "0234", false)] // Actuator removed.
    [InlineData("0311010234", "0301010434", false)] // Coating changed.
    [InlineData("0350A55A002400", "02A55B002400", false)] // Frame changed.
    [InlineData("031034", "023E", false)] // Tile type changed.
    public void ShapeException_AcceptsOnlyReferenceIgnoredShapeRemoval(string originalHex, string savedHex, bool expected)
    {
        byte[] original = Convert.FromHexString(originalHex);
        byte[] saved = Convert.FromHexString(savedHex);
        bool[] frames = new bool[693];
        frames[165] = true;
        var before = new TileRecords(original, new Header([0, 0, original.Length], frames, 1, 1)).Read(1);
        var after = new TileRecords(saved, new Header([0, 0, saved.Length], frames, 1, 1)).Read(1);
        Assert.Equal(expected, IsReferenceIgnoredShapeRemoval(original, before, saved, after));
    }

    private static bool IsReferenceIgnoredShapeRemoval(byte[] original, Record before, byte[] saved, Record after)
    {
        // Verified against 1.4.4.9 Main.tileSolid and TileID.Sets.NonSolidSaveSlopes.
        // Restrict the exception to the eight types observed in these format-279 fixtures.
        // Keep every other flag and body byte; header-chain bits may disappear with an empty header.
        return before.Type is 52 or 62 or 165 or 178 or 323 or 373 or 375 or 638
            && (before.H2 & 0x70) != 0 && (after.H2 & 0x70) == 0
            && (before.H1 & 0x3E) == (after.H1 & 0x3E)
            && (before.H2 & 0x8E) == (after.H2 & 0x8E)
            && (before.H3 & 0xFE) == (after.H3 & 0xFE) && before.H4 == after.H4
            && original.AsSpan(before.BodyOffset, before.Offset + before.PayloadLength - before.BodyOffset)
                .SequenceEqual(saved.AsSpan(after.BodyOffset, after.Offset + after.PayloadLength - after.BodyOffset));
    }

    private static byte[] ReadConsole(string path)
    {
        using var file = File.OpenRead(path);
        using var reader = new BinaryReader(file);
        Assert.Equal(0x1AA2227E, reader.ReadInt32());
        int declaredSize = reader.ReadInt32();
        using var zlib = new ZLibStream(file, CompressionMode.Decompress);
        using var raw = new MemoryStream();
        zlib.CopyTo(raw);
        Assert.Equal(declaredSize, raw.Length);
        return raw.ToArray();
    }

    private sealed record Header(int[] Pointers, bool[] Frames, int Width, int Height);

    private static Header ReadHeader(byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        Assert.Equal(279, reader.ReadInt32());
        Assert.Equal("relogic", new string(reader.ReadChars(7)));
        Assert.Equal(2, reader.ReadByte());
        reader.BaseStream.Position = 24;
        Assert.Equal(11, reader.ReadInt16());
        int[] pointers = Enumerable.Range(0, 11).Select(_ => reader.ReadInt32()).ToArray();
        int frameCount = reader.ReadInt16();
        Assert.Equal(693, frameCount);
        byte[] packedFrames = reader.ReadBytes((frameCount + 7) / 8);
        bool[] frames = Enumerable.Range(0, frameCount).Select(i => (packedFrames[i / 8] & (1 << (i % 8))) != 0).ToArray();
        Assert.Equal(pointers[0], reader.BaseStream.Position);
        reader.ReadString(); // title
        reader.ReadString(); // seed, version >= 180
        reader.ReadUInt64(); // world generator version
        reader.ReadBytes(16); // world UUID
        reader.ReadInt32(); // world ID
        reader.ReadBytes(16); // world bounds
        int height = reader.ReadInt32();
        int width = reader.ReadInt32();
        return new Header(pointers, frames, width, height);
    }

    private readonly record struct Record(int Offset, int PayloadLength, int Length, int Count,
        int BodyOffset, int Type, byte H1, byte H2, byte H3, byte H4);

    // Independent wire reader based on format-279 WorldFile.LoadWorldTiles.
    // It retains ALL non-RLE bytes, including flags that TEdit's Tile model might ignore.
    private sealed class TileRecords(byte[] data, Header header)
    {
        public int Position { get; private set; } = header.Pointers[1];
        public int Count { get; private set; }
        public int PayloadBytes { get; private set; }
        public int RleBytes { get; private set; }
        public int[] Modes { get; } = new int[4];

        public string Hex(Record record) => Convert.ToHexString(data.AsSpan(record.Offset, record.Length));

        public Record Read(int remainingInColumn)
        {
            int start = Position;
            byte h1 = data[Position++];
            byte h2 = (h1 & 1) != 0 ? data[Position++] : (byte)0;
            byte h3 = (h2 & 1) != 0 ? data[Position++] : (byte)0;
            byte h4 = (h3 & 1) != 0 ? data[Position++] : (byte)0;
            int bodyOffset = Position;
            int type = -1;
            if ((h1 & 2) != 0)
            {
                type = data[Position++];
                if ((h1 & 32) != 0)
                {
                    type |= data[Position++] << 8;
                }
                Assert.InRange(type, 0, header.Frames.Length - 1);
                if (header.Frames[type])
                {
                    Position += 4;
                }
                if ((h3 & 8) != 0)
                {
                    Position++;
                }
            }
            if ((h1 & 4) != 0)
            {
                Position++;
                if ((h3 & 16) != 0)
                {
                    Position++;
                }
            }
            if ((h1 & 24) != 0)
            {
                Position++;
            }
            if ((h3 & 64) != 0)
            {
                Position++;
            }
            int payloadLength = Position - start;
            int mode = h1 >> 6;
            int repeats = mode switch
            {
                0 => 0,
                1 => data[Position++],
                _ => ReadRepeatInt16()
            };
            Assert.InRange(repeats, 0, remainingInColumn - 1);
            Assert.InRange(Position, header.Pointers[1], header.Pointers[2]);
            Count++;
            Modes[mode]++;
            PayloadBytes += payloadLength;
            RleBytes += Position - start - payloadLength;
            return new Record(start, payloadLength, Position - start, repeats + 1,
                bodyOffset, type, h1, h2, h3, h4);
        }

        private int ReadRepeatInt16()
        {
            int repeats = (short)(data[Position] | data[Position + 1] << 8);
            Position += 2;
            return repeats;
        }
    }
}
