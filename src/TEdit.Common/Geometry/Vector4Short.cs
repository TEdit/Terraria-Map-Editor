using System;

namespace TEdit.Geometry;

public struct Vector4Short
{

    public short X;
    public short Y;
    public short Z;
    public short W;

    public Vector4Short(short x, short y, short z, short w)
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

    public static bool Parse(string text, out Vector4Short vector)
    {
        vector = new Vector4Short();
        if (string.IsNullOrWhiteSpace(text)) return false;

        var split = text.Split(',', 'x');
        if (split.Length != 4) return false;
        short x, y, z, w;
        if (short.TryParse(split[0], out x) ||
            short.TryParse(split[1], out y) ||
            short.TryParse(split[2], out z) ||
            short.TryParse(split[3], out w))
            return false;

        vector = new Vector4Short(x, y, z, w);
        return true;
    }

    public bool Equals(Vector4Short other)
    {
        return other.W == W && other.X == X && other.Y == Y && other.Z == Z;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector4Short)) return false;
        return Equals((Vector4Short)obj);
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

    public static bool operator ==(Vector4Short left, Vector4Short right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4Short left, Vector4Short right)
    {
        return !left.Equals(right);
    }
}
