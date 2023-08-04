using System;

namespace TEdit.Geometry;

public struct Vector4Byte
{

    public byte X;
    public byte Y;
    public byte Z;
    public byte W;

    public Vector4Byte(byte x, byte y, byte z, byte w)
        : this()
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public override string ToString()
    {
        return $"({X:0},{Y:0},{Z:0},{W:0})";
    }

    public bool Equals(Vector4Byte other)
    {
        return other.W == W && other.X == X && other.Y == Y && other.Z == Z;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector4Byte)) return false;
        return Equals((Vector4Byte)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int result = W;
            result = result * 397 ^ X;
            result = result * 397 ^ Y;
            result = result * 397 ^ Z;
            return result;
        }
    }

    public static bool operator ==(Vector4Byte left, Vector4Byte right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4Byte left, Vector4Byte right)
    {
        return !left.Equals(right);
    }
}
