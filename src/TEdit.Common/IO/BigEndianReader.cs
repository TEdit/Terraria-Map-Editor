using System;
using System.IO;
using System.Text;

namespace TEdit.Common.IO;

/// <summary>
/// BinaryReader that reads multi-byte values in big-endian order.
/// Compatible with tModLoader's BigEndianReader.
/// </summary>
public class BigEndianReader : BinaryReader
{
    private readonly byte[] _buf = new byte[8];

    public BigEndianReader(Stream input) : base(input, Encoding.UTF8, leaveOpen: true) { }

    private void ReadExact(int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = BaseStream.Read(_buf, offset, count - offset);
            if (read == 0)
                throw new EndOfStreamException();
            offset += read;
        }
    }

    public override short ReadInt16()
    {
        ReadExact(2);
        return (short)((_buf[0] << 8) | _buf[1]);
    }

    public override ushort ReadUInt16()
    {
        ReadExact(2);
        return (ushort)((_buf[0] << 8) | _buf[1]);
    }

    public override int ReadInt32()
    {
        ReadExact(4);
        return (_buf[0] << 24) | (_buf[1] << 16) | (_buf[2] << 8) | _buf[3];
    }

    public override long ReadInt64()
    {
        ReadExact(8);
        return ((long)_buf[0] << 56) | ((long)_buf[1] << 48) | ((long)_buf[2] << 40) | ((long)_buf[3] << 32) |
               ((long)_buf[4] << 24) | ((long)_buf[5] << 16) | ((long)_buf[6] << 8) | _buf[7];
    }

    public override float ReadSingle()
    {
        ReadExact(4);
        // Reverse for big-endian
        byte[] le = { _buf[3], _buf[2], _buf[1], _buf[0] };
        return BitConverter.ToSingle(le, 0);
    }

    public override double ReadDouble()
    {
        ReadExact(8);
        byte[] le = { _buf[7], _buf[6], _buf[5], _buf[4], _buf[3], _buf[2], _buf[1], _buf[0] };
        return BitConverter.ToDouble(le, 0);
    }

    /// <summary>
    /// Reads a UTF-8 string prefixed by a big-endian Int16 byte length.
    /// </summary>
    public override string ReadString()
    {
        short len = ReadInt16();
        if (len < 0) throw new IOException($"Invalid string length: {len}");
        if (len == 0) return string.Empty;
        byte[] buf = ReadBytes(len);
        if (buf.Length < len)
            throw new EndOfStreamException();
        return Encoding.UTF8.GetString(buf);
    }
}
