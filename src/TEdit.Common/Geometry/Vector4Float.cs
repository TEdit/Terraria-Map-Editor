using System;

namespace TEdit.Geometry;

public struct Vector4Float
{

    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector4Float(float x, float y, float z, float w)
        : this()
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public override string ToString()
    {
        return $"({X:0.000},{Y:0.000},{Z:0.000},{W:0.000})";
    }

    public static bool Parse(string text, out Vector4Float vector)
    {
        vector = new Vector4Float();
        if (string.IsNullOrWhiteSpace(text)) return false;

        var split = text.Split(',', 'x');
        if (split.Length != 4) return false;
        int x, y, z, w;
        if (int.TryParse(split[0], out x) ||
            int.TryParse(split[1], out y) ||
            int.TryParse(split[2], out z) ||
            int.TryParse(split[3], out w))
            return false;

        vector = new Vector4Float(x, y, y, z);
        return true;
    }

    public static Vector4Float operator -(Vector4Float a, Vector4Float b)
    {
        return new Vector4Float(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    }

    public static Vector4Float operator +(Vector4Float a, Vector4Float b)
    {
        return new Vector4Float(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    }

    public static Vector4Float operator *(Vector4Float a, float scale)
    {
        return new Vector4Float(a.X * scale, a.Y * scale, a.Z * scale, a.W * scale);
    }

    public static Vector4Float operator /(Vector4Float a, float scale)
    {
        if (scale == 0) { throw new DivideByZeroException(); }
        return new Vector4Float(a.X / scale, a.Y / scale, a.Z / scale, a.W / scale);
    }
    public bool Equals(Vector4Float other)
    {
        return other.W.Equals(W) && other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector4Float)) return false;
        return Equals((Vector4Float)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int result = W.GetHashCode();
            result = result * 397 ^ X.GetHashCode();
            result = result * 397 ^ Y.GetHashCode();
            result = result * 397 ^ Z.GetHashCode();
            return result;
        }
    }

    public static bool operator ==(Vector4Float left, Vector4Float right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4Float left, Vector4Float right)
    {
        return !left.Equals(right);
    }
}
