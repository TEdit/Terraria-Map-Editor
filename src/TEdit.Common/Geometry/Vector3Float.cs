using System;

namespace TEdit.Geometry;

public struct Vector3Float
{

    public float X;
    public float Y;
    public float Z;

    public Vector3Float(float x, float y, float z)
        : this()
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return $"({X:0.000},{Y:0.000},{Z:0.000})";
    }

    public static bool Parse(string text, out Vector3Float vector)
    {
        vector = new Vector3Float();
        if (string.IsNullOrWhiteSpace(text)) return false;

        var split = text.Split(',', 'x');
        if (split.Length != 3) return false;
        float x, y, z;
        if (float.TryParse(split[0], out x) ||
            float.TryParse(split[1], out y) ||
            float.TryParse(split[2], out z))
            return false;

        vector = new Vector3Float(x, y, z);
        return true;
    }

    public static Vector3Float operator -(Vector3Float a, Vector3Float b)
    {
        return new Vector3Float(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3Float operator +(Vector3Float a, Vector3Float b)
    {
        return new Vector3Float(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3Float operator *(Vector3Float a, float scale)
    {
        return new Vector3Float(a.X * scale, a.Y * scale, a.Z * scale);
    }

    public static Vector3Float operator /(Vector3Float a, float scale)
    {
        if (scale == 0) { throw new DivideByZeroException(); }
        return new Vector3Float(a.X / scale, a.Y / scale, a.Z / scale);
    }

    public bool Equals(Vector3Float other)
    {
        return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector3Float)) return false;
        return Equals((Vector3Float)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int result = X.GetHashCode();
            result = result * 397 ^ Y.GetHashCode();
            result = result * 397 ^ Z.GetHashCode();
            return result;
        }
    }

    public static bool operator ==(Vector3Float left, Vector3Float right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3Float left, Vector3Float right)
    {
        return !left.Equals(right);
    }
}
