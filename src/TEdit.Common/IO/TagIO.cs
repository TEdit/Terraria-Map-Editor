using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace TEdit.Common.IO;

/// <summary>
/// Binary serializer/deserializer for TagCompound data, compatible with tModLoader's NBT format.
/// Uses GZip compression and big-endian byte order.
/// </summary>
/// <remarks>
/// Tag type IDs:
///  0 = end marker
///  1 = byte
///  2 = short
///  3 = int
///  4 = long
///  5 = float
///  6 = double
///  7 = byte[]
///  8 = string
///  9 = list
/// 10 = TagCompound
/// 11 = int[]
/// </remarks>
public static class TagIO
{
    private const byte TagEnd = 0;
    private const byte TagByte = 1;
    private const byte TagShort = 2;
    private const byte TagInt = 3;
    private const byte TagLong = 4;
    private const byte TagFloat = 5;
    private const byte TagDouble = 6;
    private const byte TagByteArray = 7;
    private const byte TagString = 8;
    private const byte TagList = 9;
    private const byte TagCompoundId = 10;
    private const byte TagIntArray = 11;

    #region Public API

    /// <summary>
    /// Reads a TagCompound from a file (GZip compressed by default).
    /// </summary>
    public static TagCompound FromFile(string path, bool compressed = true)
    {
        using var fs = File.OpenRead(path);
        return FromStream(fs, compressed);
    }

    /// <summary>
    /// Writes a TagCompound to a file (GZip compressed by default).
    /// </summary>
    public static void ToFile(TagCompound tag, string path, bool compressed = true)
    {
        using var fs = File.Create(path);
        ToStream(tag, fs, compressed);
    }

    /// <summary>
    /// Reads a TagCompound from a stream.
    /// </summary>
    public static TagCompound FromStream(Stream stream, bool compressed = true)
    {
        if (compressed)
        {
            // Decompress entire GZip to memory first, then parse
            using var gzip = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true);
            using var ms = new MemoryStream();
            gzip.CopyTo(ms);
            ms.Position = 0;

            using var reader = new BigEndianReader(ms);
            return ReadRootTag(reader);
        }
        else
        {
            using var reader = new BigEndianReader(stream);
            return ReadRootTag(reader);
        }
    }

    /// <summary>
    /// Writes a TagCompound to a stream.
    /// </summary>
    public static void ToStream(TagCompound tag, Stream stream, bool compressed = true)
    {
        if (compressed)
        {
            using var gzip = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
            using var writer = new BigEndianWriter(gzip);
            WriteRootTag(tag, writer);
        }
        else
        {
            using var writer = new BigEndianWriter(stream);
            WriteRootTag(tag, writer);
        }
    }

    #endregion

    #region Read

    private static TagCompound ReadRootTag(BigEndianReader reader)
    {
        byte typeId = reader.ReadByte();
        if (typeId != TagCompoundId)
            throw new IOException($"Expected root compound tag (10), got {typeId}");

        // Root tag name (usually empty string)
        reader.ReadString();

        return ReadCompound(reader);
    }

    private static TagCompound ReadCompound(BigEndianReader reader)
    {
        var tag = new TagCompound();

        while (true)
        {
            byte typeId = reader.ReadByte();
            if (typeId == TagEnd)
                break;

            string name = reader.ReadString();
            object value = ReadPayload(reader, typeId);
            tag[name] = value;
        }

        return tag;
    }

    private static object ReadPayload(BigEndianReader reader, byte typeId)
    {
        return typeId switch
        {
            TagByte => reader.ReadByte(),
            TagShort => reader.ReadInt16(),
            TagInt => reader.ReadInt32(),
            TagLong => reader.ReadInt64(),
            TagFloat => reader.ReadSingle(),
            TagDouble => reader.ReadDouble(),
            TagByteArray => ReadByteArray(reader),
            TagString => reader.ReadString(),
            TagList => ReadList(reader),
            TagCompoundId => ReadCompound(reader),
            TagIntArray => ReadIntArray(reader),
            _ => throw new IOException($"Unknown tag type: {typeId}")
        };
    }

    private static byte[] ReadByteArray(BigEndianReader reader)
    {
        int length = reader.ReadInt32();
        if (length < 0) throw new IOException($"Invalid byte array length: {length}");
        return reader.ReadBytes(length);
    }

    private static int[] ReadIntArray(BigEndianReader reader)
    {
        int length = reader.ReadInt32();
        if (length < 0) throw new IOException($"Invalid int array length: {length}");
        var arr = new int[length];
        for (int i = 0; i < length; i++)
            arr[i] = reader.ReadInt32();
        return arr;
    }

    private static IList ReadList(BigEndianReader reader)
    {
        byte elemTypeId = reader.ReadByte();
        int count = reader.ReadInt32();
        if (count < 0) throw new IOException($"Invalid list count: {count}");

        return elemTypeId switch
        {
            TagByte => ReadTypedList<byte>(reader, count, r => r.ReadByte()),
            TagShort => ReadTypedList<short>(reader, count, r => r.ReadInt16()),
            TagInt => ReadTypedList<int>(reader, count, r => r.ReadInt32()),
            TagLong => ReadTypedList<long>(reader, count, r => r.ReadInt64()),
            TagFloat => ReadTypedList<float>(reader, count, r => r.ReadSingle()),
            TagDouble => ReadTypedList<double>(reader, count, r => r.ReadDouble()),
            TagByteArray => ReadTypedList<byte[]>(reader, count, ReadByteArray),
            TagString => ReadTypedList<string>(reader, count, r => r.ReadString()),
            TagList => ReadTypedList<IList>(reader, count, ReadList),
            TagCompoundId => ReadTypedList<TagCompound>(reader, count, ReadCompound),
            TagIntArray => ReadTypedList<int[]>(reader, count, ReadIntArray),
            TagEnd when count == 0 => new List<object>(), // empty list with no type
            _ => throw new IOException($"Unknown list element type: {elemTypeId}")
        };
    }

    private static List<T> ReadTypedList<T>(BigEndianReader reader, int count, Func<BigEndianReader, T> readFunc)
    {
        var list = new List<T>(count);
        for (int i = 0; i < count; i++)
            list.Add(readFunc(reader));
        return list;
    }

    #endregion

    #region Write

    private static void WriteRootTag(TagCompound tag, BigEndianWriter writer)
    {
        writer.Write(TagCompoundId);
        writer.Write(string.Empty); // root tag name
        WriteCompound(tag, writer);
    }

    private static void WriteCompound(TagCompound tag, BigEndianWriter writer)
    {
        foreach (var kvp in tag)
        {
            byte typeId = GetTypeId(kvp.Value);
            writer.Write(typeId);
            writer.Write(kvp.Key);
            WritePayload(writer, kvp.Value, typeId);
        }
        writer.Write(TagEnd);
    }

    private static void WritePayload(BigEndianWriter writer, object value, byte typeId)
    {
        switch (typeId)
        {
            case TagByte:
                writer.Write(value is bool b ? (byte)(b ? 1 : 0) : (byte)value);
                break;
            case TagShort:
                writer.Write(Convert.ToInt16(value));
                break;
            case TagInt:
                writer.Write(Convert.ToInt32(value));
                break;
            case TagLong:
                writer.Write(Convert.ToInt64(value));
                break;
            case TagFloat:
                writer.Write(Convert.ToSingle(value));
                break;
            case TagDouble:
                writer.Write(Convert.ToDouble(value));
                break;
            case TagByteArray:
                WriteByteArray(writer, (byte[])value);
                break;
            case TagString:
                writer.Write((string)value);
                break;
            case TagList:
                WriteList(writer, (IList)value);
                break;
            case TagCompoundId:
                WriteCompound((TagCompound)value, writer);
                break;
            case TagIntArray:
                WriteIntArray(writer, (int[])value);
                break;
            default:
                throw new IOException($"Cannot write tag type: {typeId}");
        }
    }

    private static void WriteByteArray(BigEndianWriter writer, byte[] data)
    {
        writer.Write(data.Length);
        writer.Write(data);
    }

    private static void WriteIntArray(BigEndianWriter writer, int[] data)
    {
        writer.Write(data.Length);
        foreach (int val in data)
            writer.Write(val);
    }

    private static void WriteList(BigEndianWriter writer, IList list)
    {
        if (list.Count == 0)
        {
            writer.Write(TagEnd); // element type = end (no type for empty list)
            writer.Write(0); // count
            return;
        }

        byte elemTypeId = GetTypeId(list[0]);
        writer.Write(elemTypeId);
        writer.Write(list.Count);

        foreach (object item in list)
            WritePayload(writer, item, elemTypeId);
    }

    #endregion

    #region Type mapping

    private static byte GetTypeId(object value)
    {
        return value switch
        {
            bool => TagByte, // bool stored as byte (tModLoader convention)
            byte => TagByte,
            short => TagShort,
            int => TagInt,
            long => TagLong,
            float => TagFloat,
            double => TagDouble,
            byte[] => TagByteArray,
            int[] => TagIntArray,
            string => TagString,
            TagCompound => TagCompoundId,
            IList => TagList, // must be after byte[], int[], TagCompound (all implement IList or IEnumerable)
            _ => throw new IOException($"Unsupported tag value type: {value?.GetType()?.Name ?? "null"}")
        };
    }

    #endregion
}
