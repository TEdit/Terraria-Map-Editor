using System;

namespace TEdit.Geometry;

public struct Vector2Short
{
    public short X;
    public short Y;

    public Vector2Short(short x, short y)
        : this()
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X:0},{Y:0})";
    }

    public static bool Parse(string text, out Vector2Short vector)
    {
        vector = new Vector2Short();
        if (string.IsNullOrWhiteSpace(text)) return false;

        var split = text.Split(',', 'x');
        if (split.Length != 2) return false;
        short x, y;
        if (short.TryParse(split[0], out x) ||
            short.TryParse(split[1], out y))
            return false;

        vector = new Vector2Short(x, y);
        return true;
    }

    public readonly static Vector2Short Zero = new Vector2Short(0, 0);
    public readonly static Vector2Short One = new Vector2Short(1, 1);
    public short Height => Y;
    public short Width => X;

    public static Vector2Short operator +(Vector2Short a, Vector2Short b) => new Vector2Short((short)(a.X + b.X), (short)(a.Y + b.Y));
    public static Vector2Short operator -(Vector2Short a, Vector2Short b) => new Vector2Short((short)(a.X - b.X), (short)(a.Y - b.Y));
    public static Vector2Short operator *(Vector2Short a, Vector2Short b) => new Vector2Short((short)(a.X * b.X), (short)(a.Y * b.Y));
    public static Vector2Short operator /(Vector2Short a, Vector2Short b) => new Vector2Short((short)(a.X / b.X), (short)(a.Y / b.Y));
    public static Vector2Short operator *(Vector2Short a, short b) => new Vector2Short((short)(a.X * b), (short)(a.Y * b));
    public static Vector2Short operator /(Vector2Short a, short b) => new Vector2Short((short)(a.X / b), (short)(a.Y / b));

    public bool Equals(Vector2Short other)
    {
        return other.Y == Y && other.X == X;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector2Short)) return false;
        return Equals((Vector2Short)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Y * 397 ^ X;
        }
    }

    public static bool operator ==(Vector2Short left, Vector2Short right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2Short left, Vector2Short right)
    {
        return !left.Equals(right);
    }
}
