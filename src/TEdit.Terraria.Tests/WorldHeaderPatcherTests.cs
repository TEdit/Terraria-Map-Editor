using System.Text;
using Shouldly;

namespace TEdit.Terraria.Tests;

public class WorldHeaderPatcherTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    public void Dispose()
    {
        foreach (var f in _tempFiles)
        {
            try { File.Delete(f); } catch { }
        }
    }

    private string CreateMinimalWorldFile(
        uint version = 279,
        ulong flags = 0,
        string magic = "relogic",
        byte fileType = 0x02)
    {
        var path = Path.GetTempFileName();
        _tempFiles.Add(path);

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(fs, Encoding.UTF8);

        writer.Write(version);                          // 0x00: version (4 bytes)
        writer.Write(Encoding.UTF8.GetBytes(magic));    // 0x04: magic (7 bytes)
        writer.Write(fileType);                         // 0x0B: file type (1 byte)
        writer.Write((uint)0);                          // 0x0C: file revision (4 bytes)
        writer.Write(flags);                            // 0x10: flags (8 bytes)

        // Write some padding to make it a realistic-ish file
        writer.Write(new byte[64]);

        return path;
    }

    private ulong ReadFlagsFromFile(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        fs.Position = 0x10;
        using var reader = new BinaryReader(fs);
        return reader.ReadUInt64();
    }

    [Fact]
    public void SetFavorite_True_OnNonFavorite()
    {
        var path = CreateMinimalWorldFile(flags: 0x0);

        WorldHeaderPatcher.SetFavorite(path, true);

        ReadFlagsFromFile(path).ShouldBe(0x1uL);
    }

    [Fact]
    public void SetFavorite_False_OnFavorite()
    {
        var path = CreateMinimalWorldFile(flags: 0x1);

        WorldHeaderPatcher.SetFavorite(path, false);

        ReadFlagsFromFile(path).ShouldBe(0x0uL);
    }

    [Fact]
    public void SetFavorite_True_AlreadyTrue()
    {
        var path = CreateMinimalWorldFile(flags: 0x1);

        WorldHeaderPatcher.SetFavorite(path, true);

        ReadFlagsFromFile(path).ShouldBe(0x1uL);
    }

    [Fact]
    public void SetFavorite_RoundTrip_ReadBack()
    {
        var path = CreateMinimalWorldFile(flags: 0x0);

        WorldHeaderPatcher.SetFavorite(path, true);
        var flagsAfterSet = ReadFlagsFromFile(path);
        flagsAfterSet.ShouldBe(0x1uL);

        WorldHeaderPatcher.SetFavorite(path, false);
        var flagsAfterClear = ReadFlagsFromFile(path);
        flagsAfterClear.ShouldBe(0x0uL);
    }

    [Fact]
    public void SetFavorite_PreservesOtherBits()
    {
        ulong originalFlags = 0xFFFF_FFFF_FFFF_FFF0;
        var path = CreateMinimalWorldFile(flags: originalFlags);

        WorldHeaderPatcher.SetFavorite(path, true);

        ReadFlagsFromFile(path).ShouldBe(0xFFFF_FFFF_FFFF_FFF1uL);
    }

    [Fact]
    public void SetFavorite_NullPath_Throws()
    {
        Should.Throw<ArgumentNullException>(() =>
            WorldHeaderPatcher.SetFavorite(null!, true));
    }

    [Fact]
    public void SetFavorite_MissingFile_Throws()
    {
        Should.Throw<FileNotFoundException>(() =>
            WorldHeaderPatcher.SetFavorite(@"C:\nonexistent\fake.wld", true));
    }

    [Fact]
    public void SetFavorite_VersionBelow140_Throws()
    {
        var path = CreateMinimalWorldFile(version: 100);

        var ex = Should.Throw<InvalidOperationException>(() =>
            WorldHeaderPatcher.SetFavorite(path, true));

        ex.Message.ShouldContain("100");
        ex.Message.ShouldContain("140");
    }

    [Fact]
    public void SetFavorite_CompressedWorld_Throws()
    {
        // Write the compressed magic number as the version field
        var path = Path.GetTempFileName();
        _tempFiles.Add(path);

        using (var fs = new FileStream(path, FileMode.Create))
        using (var writer = new BinaryWriter(fs))
        {
            writer.Write(unchecked((uint)0x1AA2227E));
            writer.Write(new byte[100]);
        }

        Should.Throw<InvalidOperationException>(() =>
            WorldHeaderPatcher.SetFavorite(path, true))
            .Message.ShouldContain("compressed");
    }

    [Fact]
    public void SetFavorite_InvalidMagic_Throws()
    {
        var path = CreateMinimalWorldFile(magic: "INVALID");

        Should.Throw<InvalidOperationException>(() =>
            WorldHeaderPatcher.SetFavorite(path, true))
            .Message.ShouldContain("magic");
    }

    [Fact]
    public void SetFavorite_InvalidFileType_Throws()
    {
        var path = CreateMinimalWorldFile(fileType: 0x01); // Map, not World

        Should.Throw<InvalidOperationException>(() =>
            WorldHeaderPatcher.SetFavorite(path, true))
            .Message.ShouldContain("world");
    }

    [Fact]
    public void SetFavorite_ChineseMagic_Works()
    {
        var path = CreateMinimalWorldFile(magic: "xindong", flags: 0x0);

        WorldHeaderPatcher.SetFavorite(path, true);

        ReadFlagsFromFile(path).ShouldBe(0x1uL);
    }
}
