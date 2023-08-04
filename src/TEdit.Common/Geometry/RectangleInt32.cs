using System;

namespace TEdit.Geometry;

public struct RectangleInt32 : IEquatable<RectangleInt32>
{
    private static RectangleInt32 emptyRectangle;

    //
    // Summary:
    //     The x coordinate of the top-left corner of this Microsoft.Xna.Framework.Rectangle.
    public int X;

    //
    // Summary:
    //     The y coordinate of the top-left corner of this Microsoft.Xna.Framework.Rectangle.
    public int Y;

    //
    // Summary:
    //     The width of this Microsoft.Xna.Framework.Rectangle.
    public int Width;

    //
    // Summary:
    //     The height of this Microsoft.Xna.Framework.Rectangle.
    public int Height;

    //
    // Summary:
    //     Returns a Microsoft.Xna.Framework.RectangleInt32 with X=0, Y=0, Width=0, Height=0.
    public static RectangleInt32 Empty => emptyRectangle;

    //
    // Summary:
    //     Returns the x coordinate of the left edge of this Microsoft.Xna.Framework.Rectangle.
    public int Left => X;

    //
    // Summary:
    //     Returns the x coordinate of the right edge of this Microsoft.Xna.Framework.Rectangle.
    public int Right => X + Width;

    //
    // Summary:
    //     Returns the y coordinate of the top edge of this Microsoft.Xna.Framework.Rectangle.
    public int Top => Y;

    //
    // Summary:
    //     Returns the y coordinate of the bottom edge of this Microsoft.Xna.Framework.Rectangle.
    public int Bottom => Y + Height;

    //
    // Summary:
    //     Whether or not this Microsoft.Xna.Framework.RectangleInt32 has a Microsoft.Xna.Framework.Rectangle.Width
    //     and Microsoft.Xna.Framework.Rectangle.Height of 0, and a Microsoft.Xna.Framework.Rectangle.Location
    //     of (0, 0).
    public bool IsEmpty
    {
        get
        {
            if (Width == 0 && Height == 0 && X == 0)
            {
                return Y == 0;
            }

            return false;
        }
    }

    //
    // Summary:
    //     The top-left coordinates of this Microsoft.Xna.Framework.Rectangle.
    public Vector2Int32 Location
    {
        get
        {
            return new Vector2Int32(X, Y);
        }
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    //
    // Summary:
    //     The width-height coordinates of this Microsoft.Xna.Framework.Rectangle.
    public Vector2Int32 Size
    {
        get
        {
            return new Vector2Int32(Width, Height);
        }
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    //
    // Summary:
    //     A Microsoft.Xna.Framework.Vector2Int32 located in the center of this Microsoft.Xna.Framework.Rectangle.
    //
    // Remarks:
    //     If Microsoft.Xna.Framework.Rectangle.Width or Microsoft.Xna.Framework.Rectangle.Height
    //     is an odd number, the center Vector2Int32 will be rounded down.
    public Vector2Int32 Center => new Vector2Int32(X + Width / 2, Y + Height / 2);

    internal string DebugDisplayString => X + "  " + Y + "  " + Width + "  " + Height;

    //
    // Summary:
    //     Creates a new instance of Microsoft.Xna.Framework.RectangleInt32 struct, with the
    //     specified position, width, and height.
    //
    // Parameters:
    //   x:
    //     The x coordinate of the top-left corner of the created Microsoft.Xna.Framework.Rectangle.
    //
    //   y:
    //     The y coordinate of the top-left corner of the created Microsoft.Xna.Framework.Rectangle.
    //
    //   width:
    //     The width of the created Microsoft.Xna.Framework.Rectangle.
    //
    //   height:
    //     The height of the created Microsoft.Xna.Framework.Rectangle.
    public RectangleInt32(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    //
    // Summary:
    //     Creates a new instance of Microsoft.Xna.Framework.RectangleInt32 struct, with the
    //     specified location and size.
    //
    // Parameters:
    //   location:
    //     The x and y coordinates of the top-left corner of the created Microsoft.Xna.Framework.Rectangle.
    //
    //   size:
    //     The width and height of the created Microsoft.Xna.Framework.Rectangle.
    public RectangleInt32(Vector2Int32 location, Vector2Int32 size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.X;
        Height = size.Y;
    }

    //
    // Summary:
    //     Compares whether two Microsoft.Xna.Framework.RectangleInt32 instances are equal.
    //
    // Parameters:
    //   a:
    //     Microsoft.Xna.Framework.RectangleInt32 instance on the left of the equal sign.
    //
    //   b:
    //     Microsoft.Xna.Framework.RectangleInt32 instance on the right of the equal sign.
    //
    // Returns:
    //     true if the instances are equal; false otherwise.
    public static bool operator ==(RectangleInt32 a, RectangleInt32 b)
    {
        if (a.X == b.X && a.Y == b.Y && a.Width == b.Width)
        {
            return a.Height == b.Height;
        }

        return false;
    }

    //
    // Summary:
    //     Compares whether two Microsoft.Xna.Framework.RectangleInt32 instances are not equal.
    //
    // Parameters:
    //   a:
    //     Microsoft.Xna.Framework.RectangleInt32 instance on the left of the not equal sign.
    //
    //   b:
    //     Microsoft.Xna.Framework.RectangleInt32 instance on the right of the not equal sign.
    //
    // Returns:
    //     true if the instances are not equal; false otherwise.
    public static bool operator !=(RectangleInt32 a, RectangleInt32 b)
    {
        return !(a == b);
    }

    //
    // Summary:
    //     Gets whether or not the provided coordinates lie within the bounds of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   x:
    //     The x coordinate of the Vector2Int32 to check for containment.
    //
    //   y:
    //     The y coordinate of the Vector2Int32 to check for containment.
    //
    // Returns:
    //     true if the provided coordinates lie inside this Microsoft.Xna.Framework.Rectangle;
    //     false otherwise.
    public bool Contains(int x, int y)
    {
        if (X <= x && x < X + Width && Y <= y)
        {
            return y < Y + Height;
        }

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the provided coordinates lie within the bounds of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   x:
    //     The x coordinate of the Vector2Int32 to check for containment.
    //
    //   y:
    //     The y coordinate of the Vector2Int32 to check for containment.
    //
    // Returns:
    //     true if the provided coordinates lie inside this Microsoft.Xna.Framework.Rectangle;
    //     false otherwise.
    public bool Contains(float x, float y)
    {
        if ((float)X <= x && x < (float)(X + Width) && (float)Y <= y)
        {
            return y < (float)(Y + Height);
        }

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.Vector2Int32 lies within the
    //     bounds of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   value:
    //     The coordinates to check for inclusion in this Microsoft.Xna.Framework.Rectangle.
    //
    // Returns:
    //     true if the provided Microsoft.Xna.Framework.Vector2Int32 lies inside this Microsoft.Xna.Framework.Rectangle;
    //     false otherwise.
    public bool Contains(Vector2Int32 value)
    {
        if (X <= value.X && value.X < X + Width && Y <= value.Y)
        {
            return value.Y < Y + Height;
        }

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.Vector2Int32 lies within the
    //     bounds of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   value:
    //     The coordinates to check for inclusion in this Microsoft.Xna.Framework.Rectangle.
    //
    //   result:
    //     true if the provided Microsoft.Xna.Framework.Vector2Int32 lies inside this Microsoft.Xna.Framework.Rectangle;
    //     false otherwise. As an output parameter.
    public void Contains(ref Vector2Int32 value, out bool result)
    {
        result = X <= value.X && value.X < X + Width && Y <= value.Y && value.Y < Y + Height;
    }

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.Vector2 lies within
    //     the bounds of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   value:
    //     The coordinates to check for inclusion in this Microsoft.Xna.Framework.Rectangle.
    //
    // Returns:
    //     true if the provided Microsoft.Xna.Framework.Vector2 lies inside this Microsoft.Xna.Framework.Rectangle;
    //     false otherwise.
    public bool Contains(Vector2Float value)
    {
        if ((float)X <= value.X && value.X < (float)(X + Width) && (float)Y <= value.Y)
        {
            return value.Y < (float)(Y + Height);
        }

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.Vector2 lies within
    //     the bounds of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   value:
    //     The coordinates to check for inclusion in this Microsoft.Xna.Framework.Rectangle.
    //
    //   result:
    //     true if the provided Microsoft.Xna.Framework.Vector2 lies inside this Microsoft.Xna.Framework.Rectangle;
    //     false otherwise. As an output parameter.
    public void Contains(ref Vector2Float value, out bool result)
    {
        result = (float)X <= value.X && value.X < (float)(X + Width) && (float)Y <= value.Y && value.Y < (float)(Y + Height);
    }

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.RectangleInt32 lies within
    //     the bounds of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   value:
    //     The Microsoft.Xna.Framework.RectangleInt32 to check for inclusion in this Microsoft.Xna.Framework.Rectangle.
    //
    // Returns:
    //     true if the provided Microsoft.Xna.Framework.Rectangle's bounds lie entirely
    //     inside this Microsoft.Xna.Framework.Rectangle; false otherwise.
    public bool Contains(RectangleInt32 value)
    {
        if (X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y)
        {
            return value.Y + value.Height <= Y + Height;
        }

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the provided Microsoft.Xna.Framework.RectangleInt32 lies within
    //     the bounds of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   value:
    //     The Microsoft.Xna.Framework.RectangleInt32 to check for inclusion in this Microsoft.Xna.Framework.Rectangle.
    //
    //   result:
    //     true if the provided Microsoft.Xna.Framework.Rectangle's bounds lie entirely
    //     inside this Microsoft.Xna.Framework.Rectangle; false otherwise. As an output
    //     parameter.
    public void Contains(ref RectangleInt32 value, out bool result)
    {
        result = X <= value.X && value.X + value.Width <= X + Width && Y <= value.Y && value.Y + value.Height <= Y + Height;
    }

    //
    // Summary:
    //     Compares whether current instance is equal to specified System.Object.
    //
    // Parameters:
    //   obj:
    //     The System.Object to compare.
    //
    // Returns:
    //     true if the instances are equal; false otherwise.
    public override bool Equals(object obj)
    {
        if (obj is RectangleInt32)
        {
            return this == (RectangleInt32)obj;
        }

        return false;
    }

    //
    // Summary:
    //     Compares whether current instance is equal to specified Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   other:
    //     The Microsoft.Xna.Framework.RectangleInt32 to compare.
    //
    // Returns:
    //     true if the instances are equal; false otherwise.
    public bool Equals(RectangleInt32 other)
    {
        return this == other;
    }

    //
    // Summary:
    //     Gets the hash code of this Microsoft.Xna.Framework.Rectangle.
    //
    // Returns:
    //     Hash code of this Microsoft.Xna.Framework.Rectangle.
    public override int GetHashCode()
    {
        return (((17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode()) * 23 + Width.GetHashCode()) * 23 + Height.GetHashCode();
    }

    //
    // Summary:
    //     Adjusts the edges of this Microsoft.Xna.Framework.RectangleInt32 by specified horizontal
    //     and vertical amounts.
    //
    // Parameters:
    //   horizontalAmount:
    //     Value to adjust the left and right edges.
    //
    //   verticalAmount:
    //     Value to adjust the top and bottom edges.
    public void Inflate(int horizontalAmount, int verticalAmount)
    {
        X -= horizontalAmount;
        Y -= verticalAmount;
        Width += horizontalAmount * 2;
        Height += verticalAmount * 2;
    }

    //
    // Summary:
    //     Adjusts the edges of this Microsoft.Xna.Framework.RectangleInt32 by specified horizontal
    //     and vertical amounts.
    //
    // Parameters:
    //   horizontalAmount:
    //     Value to adjust the left and right edges.
    //
    //   verticalAmount:
    //     Value to adjust the top and bottom edges.
    public void Inflate(float horizontalAmount, float verticalAmount)
    {
        X -= (int)horizontalAmount;
        Y -= (int)verticalAmount;
        Width += (int)horizontalAmount * 2;
        Height += (int)verticalAmount * 2;
    }

    //
    // Summary:
    //     Gets whether or not the other Microsoft.Xna.Framework.RectangleInt32 intersects with
    //     this rectangle.
    //
    // Parameters:
    //   value:
    //     The other RectangleInt32 for testing.
    //
    // Returns:
    //     true if other Microsoft.Xna.Framework.RectangleInt32 intersects with this rectangle;
    //     false otherwise.
    public bool Intersects(RectangleInt32 value)
    {
        if (value.Left < Right && Left < value.Right && value.Top < Bottom)
        {
            return Top < value.Bottom;
        }

        return false;
    }

    //
    // Summary:
    //     Gets whether or not the other Microsoft.Xna.Framework.RectangleInt32 intersects with
    //     this rectangle.
    //
    // Parameters:
    //   value:
    //     The other RectangleInt32 for testing.
    //
    //   result:
    //     true if other Microsoft.Xna.Framework.RectangleInt32 intersects with this rectangle;
    //     false otherwise. As an output parameter.
    public void Intersects(ref RectangleInt32 value, out bool result)
    {
        result = value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;
    }

    //
    // Summary:
    //     Creates a new Microsoft.Xna.Framework.RectangleInt32 that contains overlapping region
    //     of two other rectangles.
    //
    // Parameters:
    //   value1:
    //     The first Microsoft.Xna.Framework.Rectangle.
    //
    //   value2:
    //     The second Microsoft.Xna.Framework.Rectangle.
    //
    // Returns:
    //     Overlapping region of the two rectangles.
    public static RectangleInt32 Intersect(RectangleInt32 value1, RectangleInt32 value2)
    {
        Intersect(ref value1, ref value2, out var result);
        return result;
    }

    //
    // Summary:
    //     Creates a new Microsoft.Xna.Framework.RectangleInt32 that contains overlapping region
    //     of two other rectangles.
    //
    // Parameters:
    //   value1:
    //     The first Microsoft.Xna.Framework.Rectangle.
    //
    //   value2:
    //     The second Microsoft.Xna.Framework.Rectangle.
    //
    //   result:
    //     Overlapping region of the two rectangles as an output parameter.
    public static bool Intersect(ref RectangleInt32 value1, ref RectangleInt32 value2, out RectangleInt32 result)
    {
        if (value1.Intersects(value2))
        {
            int num = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
            int num2 = Math.Max(value1.X, value2.X);
            int num3 = Math.Max(value1.Y, value2.Y);
            int num4 = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
            result = new RectangleInt32(num2, num3, num - num2, num4 - num3);
            return true;
        }
        else
        {
            result = new RectangleInt32(0, 0, 0, 0);
            return false;
        }
    }

    //
    // Summary:
    //     Changes the Microsoft.Xna.Framework.Rectangle.Location of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   offsetX:
    //     The x coordinate to add to this Microsoft.Xna.Framework.Rectangle.
    //
    //   offsetY:
    //     The y coordinate to add to this Microsoft.Xna.Framework.Rectangle.
    public void Offset(int offsetX, int offsetY)
    {
        X += offsetX;
        Y += offsetY;
    }

    //
    // Summary:
    //     Changes the Microsoft.Xna.Framework.Rectangle.Location of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   offsetX:
    //     The x coordinate to add to this Microsoft.Xna.Framework.Rectangle.
    //
    //   offsetY:
    //     The y coordinate to add to this Microsoft.Xna.Framework.Rectangle.
    public void Offset(float offsetX, float offsetY)
    {
        X += (int)offsetX;
        Y += (int)offsetY;
    }

    //
    // Summary:
    //     Changes the Microsoft.Xna.Framework.Rectangle.Location of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   amount:
    //     The x and y components to add to this Microsoft.Xna.Framework.Rectangle.
    public void Offset(Vector2Int32 amount)
    {
        X += amount.X;
        Y += amount.Y;
    }

    //
    // Summary:
    //     Changes the Microsoft.Xna.Framework.Rectangle.Location of this Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   amount:
    //     The x and y components to add to this Microsoft.Xna.Framework.Rectangle.
    public void Offset(Vector2Float amount)
    {
        X += (int)amount.X;
        Y += (int)amount.Y;
    }

    //
    // Summary:
    //     Returns a System.String representation of this Microsoft.Xna.Framework.Rectangle
    //     in the format: {X:[Microsoft.Xna.Framework.Rectangle.X] Y:[Microsoft.Xna.Framework.Rectangle.Y]
    //     Width:[Microsoft.Xna.Framework.Rectangle.Width] Height:[Microsoft.Xna.Framework.Rectangle.Height]}
    //
    // Returns:
    //     System.String representation of this Microsoft.Xna.Framework.Rectangle.
    public override string ToString()
    {
        return "{X:" + X + " Y:" + Y + " Width:" + Width + " Height:" + Height + "}";
    }

    //
    // Summary:
    //     Creates a new Microsoft.Xna.Framework.RectangleInt32 that completely contains two
    //     other rectangles.
    //
    // Parameters:
    //   value1:
    //     The first Microsoft.Xna.Framework.Rectangle.
    //
    //   value2:
    //     The second Microsoft.Xna.Framework.Rectangle.
    //
    // Returns:
    //     The union of the two rectangles.
    public static RectangleInt32 Union(RectangleInt32 value1, RectangleInt32 value2)
    {
        int num = Math.Min(value1.X, value2.X);
        int num2 = Math.Min(value1.Y, value2.Y);
        return new RectangleInt32(num, num2, Math.Max(value1.Right, value2.Right) - num, Math.Max(value1.Bottom, value2.Bottom) - num2);
    }

    //
    // Summary:
    //     Creates a new Microsoft.Xna.Framework.RectangleInt32 that completely contains two
    //     other rectangles.
    //
    // Parameters:
    //   value1:
    //     The first Microsoft.Xna.Framework.Rectangle.
    //
    //   value2:
    //     The second Microsoft.Xna.Framework.Rectangle.
    //
    //   result:
    //     The union of the two rectangles as an output parameter.
    public static void Union(ref RectangleInt32 value1, ref RectangleInt32 value2, out RectangleInt32 result)
    {
        result.X = Math.Min(value1.X, value2.X);
        result.Y = Math.Min(value1.Y, value2.Y);
        result.Width = Math.Max(value1.Right, value2.Right) - result.X;
        result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
    }

    //
    // Summary:
    //     Deconstruction method for Microsoft.Xna.Framework.Rectangle.
    //
    // Parameters:
    //   x:
    //
    //   y:
    //
    //   width:
    //
    //   height:
    public void Deconstruct(out int x, out int y, out int width, out int height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }
}
