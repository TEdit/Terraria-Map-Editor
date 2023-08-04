using System;

namespace TEdit.Geometry;

public struct Vector3Short
{

    public short X;
    public short Y;
    public short Z;

    public Vector3Short(short x, short y, short z)
        : this()
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return $"({X:0},{Y:0},{Z:0})";
    }

    public static bool Parse(string text, out Vector3Short vector)
    {
        vector = new Vector3Short();
        if (string.IsNullOrWhiteSpace(text)) return false;

        var split = text.Split(',', 'x');
        if (split.Length != 3) return false;
        short x, y, z;
        if (short.TryParse(split[0], out x) ||
            short.TryParse(split[1], out y) ||
            short.TryParse(split[2], out z))
            return false;

        vector = new Vector3Short(x, y, z);
        return true;
    }

    public bool Equals(Vector3Short other)
    {
        return other.X == X && other.Y == Y && other.Z == Z;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector3Short)) return false;
        return Equals((Vector3Short)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int result = X;
            result = result * 397 ^ Y;
            result = result * 397 ^ Z;
            return result;
        }
    }

    public static bool operator ==(Vector3Short left, Vector3Short right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3Short left, Vector3Short right)
    {
        return !left.Equals(right);
    }
}
