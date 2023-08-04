using System;

namespace TEdit.Geometry;

public struct Vector4Int32
{

    public int X;
    public int Y;
    public int Z;
    public int W;

    public Vector4Int32(int x, int y, int z, int w)
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

    public static bool Parse(string text, out Vector4Int32 vector)
    {
        vector = new Vector4Int32();
        if (string.IsNullOrWhiteSpace(text)) return false;

        var split = text.Split(',', 'x');
        if (split.Length != 4) return false;
        int x, y, z, w;
        if (int.TryParse(split[0], out x) ||
            int.TryParse(split[1], out y) ||
            int.TryParse(split[2], out z) ||
            int.TryParse(split[3], out w))
            return false;

        vector = new Vector4Int32(x, y, z, w);
        return true;
    }

    public bool Equals(Vector4Int32 other)
    {
        return other.W == W && other.X == X && other.Y == Y && other.Z == Z;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector4Int32)) return false;
        return Equals((Vector4Int32)obj);
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

    public static bool operator ==(Vector4Int32 left, Vector4Int32 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4Int32 left, Vector4Int32 right)
    {
        return !left.Equals(right);
    }
}
