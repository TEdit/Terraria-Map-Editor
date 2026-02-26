using System.Collections.Generic;
using System.IO;
using TEdit.Common.IO;
using Xunit;
using Shouldly;

namespace TEdit.Terraria.Tests;

public class TagCompoundTests
{
    [Fact]
    public void RoundTrip_AllTypes()
    {
        var original = new TagCompound();
        original.Set("byte", (byte)42);
        original.Set("short", (short)1234);
        original.Set("int", 567890);
        original.Set("long", 123456789012345L);
        original.Set("float", 3.14f);
        original.Set("double", 2.71828);
        original.Set("byteArray", new byte[] { 1, 2, 3, 4, 5 });
        original.Set("string", "Hello, tModLoader!");
        original.Set("intArray", new int[] { 10, 20, 30 });

        var nested = new TagCompound();
        nested.Set("inner", "value");
        original.Set("compound", nested);

        var list = new List<int> { 100, 200, 300 };
        original.Set("intList", list);

        var tagList = new List<TagCompound>();
        var item1 = new TagCompound();
        item1.Set("name", "sword");
        tagList.Add(item1);
        original.Set("tagList", tagList);

        using var ms = new MemoryStream();
        TagIO.ToStream(original, ms, compressed: false);

        ms.Position = 0;
        var loaded = TagIO.FromStream(ms, compressed: false);

        loaded.GetByte("byte").ShouldBe((byte)42);
        loaded.GetShort("short").ShouldBe((short)1234);
        loaded.GetInt("int").ShouldBe(567890);
        loaded.GetLong("long").ShouldBe(123456789012345L);
        loaded.GetFloat("float").ShouldBe(3.14f);
        loaded.GetDouble("double").ShouldBe(2.71828);
        loaded.GetByteArray("byteArray").ShouldBe(new byte[] { 1, 2, 3, 4, 5 });
        loaded.GetString("string").ShouldBe("Hello, tModLoader!");
        loaded.GetIntArray("intArray").ShouldBe(new int[] { 10, 20, 30 });
        loaded.GetCompound("compound").GetString("inner").ShouldBe("value");
        loaded.GetList<int>("intList").ShouldBe(new List<int> { 100, 200, 300 });
        loaded.GetList<TagCompound>("tagList").Count.ShouldBe(1);
        loaded.GetList<TagCompound>("tagList")[0].GetString("name").ShouldBe("sword");
    }

    [Fact]
    public void RoundTrip_GzipCompressed()
    {
        var original = new TagCompound();
        original.Set("test", "compressed data");
        original.Set("number", 42);

        using var ms = new MemoryStream();
        TagIO.ToStream(original, ms, compressed: true);

        ms.Position = 0;
        var loaded = TagIO.FromStream(ms, compressed: true);

        loaded.GetString("test").ShouldBe("compressed data");
        loaded.GetInt("number").ShouldBe(42);
    }

    [Fact]
    public void RoundTrip_EmptyCompound()
    {
        var original = new TagCompound();

        using var ms = new MemoryStream();
        TagIO.ToStream(original, ms, compressed: false);

        ms.Position = 0;
        var loaded = TagIO.FromStream(ms, compressed: false);

        loaded.Count.ShouldBe(0);
    }

    [Fact]
    public void RoundTrip_EmptyList()
    {
        var original = new TagCompound();
        original.Set("empty", new List<int>());

        using var ms = new MemoryStream();
        TagIO.ToStream(original, ms, compressed: false);

        ms.Position = 0;
        var loaded = TagIO.FromStream(ms, compressed: false);

        loaded.GetList<int>("empty").Count.ShouldBe(0);
    }

    [Fact]
    public void RoundTrip_NestedCompounds()
    {
        var root = new TagCompound();
        var level1 = new TagCompound();
        var level2 = new TagCompound();
        level2.Set("deep", "value");
        level1.Set("child", level2);
        root.Set("parent", level1);

        using var ms = new MemoryStream();
        TagIO.ToStream(root, ms, compressed: false);

        ms.Position = 0;
        var loaded = TagIO.FromStream(ms, compressed: false);

        loaded.GetCompound("parent")
              .GetCompound("child")
              .GetString("deep")
              .ShouldBe("value");
    }

    [Fact]
    public void RoundTrip_BoolAsBytes()
    {
        var original = new TagCompound();
        original.Set("flag", (byte)1);

        using var ms = new MemoryStream();
        TagIO.ToStream(original, ms, compressed: false);

        ms.Position = 0;
        var loaded = TagIO.FromStream(ms, compressed: false);

        loaded.GetBool("flag").ShouldBeTrue();
        loaded.GetBool("missing").ShouldBeFalse();
    }

    [Fact]
    public void RoundTrip_UnicodeStrings()
    {
        var original = new TagCompound();
        original.Set("japanese", "\u30c6\u30e9\u30ea\u30a2"); // テラリア
        original.Set("emoji", "Hello \ud83c\udf0d");

        using var ms = new MemoryStream();
        TagIO.ToStream(original, ms, compressed: false);

        ms.Position = 0;
        var loaded = TagIO.FromStream(ms, compressed: false);

        loaded.GetString("japanese").ShouldBe("\u30c6\u30e9\u30ea\u30a2");
        loaded.GetString("emoji").ShouldBe("Hello \ud83c\udf0d");
    }

    [Fact]
    public void RoundTrip_ListOfCompounds()
    {
        var original = new TagCompound();

        var list = new List<TagCompound>();
        for (int i = 0; i < 3; i++)
        {
            var entry = new TagCompound();
            entry.Set("mod", $"Mod{i}");
            entry.Set("name", $"Tile{i}");
            entry.Set("framed", (byte)(i % 2));
            list.Add(entry);
        }
        original.Set("tileMap", list);

        using var ms = new MemoryStream();
        TagIO.ToStream(original, ms, compressed: false);

        ms.Position = 0;
        var loaded = TagIO.FromStream(ms, compressed: false);

        var loadedList = loaded.GetList<TagCompound>("tileMap");
        loadedList.Count.ShouldBe(3);
        loadedList[0].GetString("mod").ShouldBe("Mod0");
        loadedList[1].GetString("name").ShouldBe("Tile1");
        loadedList[2].GetBool("framed").ShouldBeFalse();
    }

    [Fact]
    public void RoundTrip_File()
    {
        var original = new TagCompound();
        original.Set("version", (short)1);
        original.Set("data", new byte[] { 0xFF, 0x00, 0xAB });

        string tempPath = Path.GetTempFileName();
        try
        {
            TagIO.ToFile(original, tempPath, compressed: true);
            var loaded = TagIO.FromFile(tempPath, compressed: true);

            loaded.GetShort("version").ShouldBe((short)1);
            loaded.GetByteArray("data").ShouldBe(new byte[] { 0xFF, 0x00, 0xAB });
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void BigEndian_ReadWrite_AllTypes()
    {
        using var ms = new MemoryStream();
        using (var writer = new BigEndianWriter(ms))
        {
            writer.Write((short)0x0102);
            writer.Write(0x01020304);
            writer.Write(0x0102030405060708L);
            writer.Write(1.0f);
            writer.Write(2.0);
            writer.Write("test");
        }

        ms.Position = 0;
        using var reader = new BigEndianReader(ms);

        reader.ReadInt16().ShouldBe((short)0x0102);
        reader.ReadInt32().ShouldBe(0x01020304);
        reader.ReadInt64().ShouldBe(0x0102030405060708L);
        reader.ReadSingle().ShouldBe(1.0f);
        reader.ReadDouble().ShouldBe(2.0);
        reader.ReadString().ShouldBe("test");
    }

    [Fact]
    public void TagCompound_GetDefault_ReturnsDefaults()
    {
        var tag = new TagCompound();

        tag.GetByte("missing").ShouldBe((byte)0);
        tag.GetShort("missing").ShouldBe((short)0);
        tag.GetInt("missing").ShouldBe(0);
        tag.GetLong("missing").ShouldBe(0L);
        tag.GetFloat("missing").ShouldBe(0f);
        tag.GetDouble("missing").ShouldBe(0d);
        tag.GetString("missing").ShouldBe(string.Empty);
        tag.GetByteArray("missing").ShouldBeNull();
        tag.GetBool("missing").ShouldBeFalse();
        tag.GetCompound("missing").Count.ShouldBe(0);
        tag.GetList<int>("missing").Count.ShouldBe(0);
    }

    [Fact]
    public void TagCompound_SetNull_RemovesKey()
    {
        var tag = new TagCompound();
        tag.Set("key", "value");
        tag.ContainsKey("key").ShouldBeTrue();

        tag.Set("key", null);
        tag.ContainsKey("key").ShouldBeFalse();
    }
}
