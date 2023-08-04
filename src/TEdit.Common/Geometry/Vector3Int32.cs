using System;

namespace TEdit.Geometry;

public struct Vector3Int32
{
    public int X;
    public int Y;
    public int Z;

    public Vector3Int32(int x, int y, int z)
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

    public static bool Parse(string text, out Vector3Int32 vector)
    {
        vector = new Vector3Int32();
        if (string.IsNullOrWhiteSpace(text)) return false;

        var split = text.Split(',', 'x');
        if (split.Length != 3) return false;
        int x, y, z;
        if (int.TryParse(split[0], out x) ||
            int.TryParse(split[1], out y) ||
            int.TryParse(split[2], out z))
            return false;

        vector = new Vector3Int32(x, y, z);
        return true;
    }

    public bool Equals(Vector3Int32 other)
    {
        return other.X == X && other.Y == Y && other.Z == Z;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector3Int32)) return false;
        return Equals((Vector3Int32)obj);
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

    public static bool operator ==(Vector3Int32 left, Vector3Int32 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3Int32 left, Vector3Int32 right)
    {
        return !left.Equals(right);
    }
}
