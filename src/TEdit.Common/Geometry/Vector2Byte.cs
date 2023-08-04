using System;

namespace TEdit.Geometry;

public struct Vector2Byte
{
    public byte X;
    public byte Y;

    public Vector2Byte(byte x, byte y)
        : this()
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X:0},{Y:0})";
    }

    public bool Equals(Vector2Byte other)
    {
        return other.Y == Y && other.X == X;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector2Byte)) return false;
        return Equals((Vector2Byte)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Y * 397 ^ X;
        }
    }

    public static bool operator ==(Vector2Byte left, Vector2Byte right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2Byte left, Vector2Byte right)
    {
        return !left.Equals(right);
    }
}
