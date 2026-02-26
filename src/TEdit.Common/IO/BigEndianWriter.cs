using System;
using System.IO;
using System.Text;

namespace TEdit.Common.IO;

/// <summary>
/// BinaryWriter that writes multi-byte values in big-endian order.
/// Compatible with tModLoader's BigEndianWriter.
/// </summary>
public class BigEndianWriter : BinaryWriter
{
    private readonly byte[] _buf = new byte[8];

    public BigEndianWriter(Stream output) : base(output, Encoding.UTF8, leaveOpen: true) { }

    public override void Write(short value)
    {
        _buf[0] = (byte)(value >> 8);
        _buf[1] = (byte)value;
        BaseStream.Write(_buf, 0, 2);
    }

    public override void Write(ushort value)
    {
        _buf[0] = (byte)(value >> 8);
        _buf[1] = (byte)value;
        BaseStream.Write(_buf, 0, 2);
    }

    public override void Write(int value)
    {
        _buf[0] = (byte)(value >> 24);
        _buf[1] = (byte)(value >> 16);
        _buf[2] = (byte)(value >> 8);
        _buf[3] = (byte)value;
        BaseStream.Write(_buf, 0, 4);
    }

    public override void Write(long value)
    {
        _buf[0] = (byte)(value >> 56);
        _buf[1] = (byte)(value >> 48);
        _buf[2] = (byte)(value >> 40);
        _buf[3] = (byte)(value >> 32);
        _buf[4] = (byte)(value >> 24);
        _buf[5] = (byte)(value >> 16);
        _buf[6] = (byte)(value >> 8);
        _buf[7] = (byte)value;
        BaseStream.Write(_buf, 0, 8);
    }

    public override void Write(float value)
    {
        byte[] le = BitConverter.GetBytes(value);
        _buf[0] = le[3];
        _buf[1] = le[2];
        _buf[2] = le[1];
        _buf[3] = le[0];
        BaseStream.Write(_buf, 0, 4);
    }

    public override void Write(double value)
    {
        byte[] le = BitConverter.GetBytes(value);
        _buf[0] = le[7];
        _buf[1] = le[6];
        _buf[2] = le[5];
        _buf[3] = le[4];
        _buf[4] = le[3];
        _buf[5] = le[2];
        _buf[6] = le[1];
        _buf[7] = le[0];
        BaseStream.Write(_buf, 0, 8);
    }

    /// <summary>
    /// Writes a UTF-8 string prefixed by a big-endian Int16 byte length.
    /// </summary>
    public override void Write(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
        Write((short)bytes.Length);
        BaseStream.Write(bytes, 0, bytes.Length);
    }
}
