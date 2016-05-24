/* 
Copyright (c) 2011 BinaryConstruct
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System;
using TEdit.Utility;

namespace TEdit.Geometry.Primitives
{
    [Serializable]
    public class Rectangle<T> where T : struct
    {
        public T X1;
        public T X2;
        public T Y1;
        public T Y2;

        public Rectangle(T x1, T y1, T x2, T y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public T Left { get { return GenericOperator.Operator.GreaterThan(X2, X1) ? X1 : X2; } }
        public T Right { get { return GenericOperator.Operator.GreaterThan(X2, X1) ? X2 : X1; } }
        public T Top { get { return GenericOperator.Operator.GreaterThan(Y2, Y1) ? Y1 : Y2; } }
        public T Bottom { get { return GenericOperator.Operator.GreaterThan(Y2, Y1) ? Y2 : Y1; } }
        public T Height { get { return GenericOperator.Operator.Subtract(Right, Left); } }
        public T Width { get { return GenericOperator.Operator.Subtract(Bottom, Top); } }

        public bool Contains(T x, T y)
        {
            T zero = default(T);
            T xabs = GenericOperator.Operator.Subtract(x, Left);
            T yabs = GenericOperator.Operator.Subtract(y, Top);
            if (GenericOperator.Operator.LessThan(xabs, zero))
                return false;

            if (GenericOperator.Operator.LessThan(yabs, zero))
                return false;

            return GenericOperator.Operator.LessThan(xabs, Width) && GenericOperator.Operator.LessThan(yabs, Height);
        }

        public static Rectangle<T> FromLrtb(T left, T right, T top, T bottom)
        {
            return new Rectangle<T>(left, top, right, bottom);
        }
    }

    [Serializable]
    public class RectangleInt32
    {
        public int X1;
        public int X2;
        public int Y1;
        public int Y2;

        public RectangleInt32(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public int Left { get { return (X2 > X1) ? X1 : X2; } }
        public int Right { get { return (X2 > X1) ? X2 : X1; } }
        public int Top { get { return (Y2 > Y1) ? Y1 : Y2; } }
        public int Bottom { get { return (Y2 > Y1) ? Y2 : Y1; } }
        public int Height { get { return Right - Left; } }
        public int Width { get { return Bottom - Top; } }

        public bool Contains(int x, int y)
        {
            int xabs = x - Left;
            int yabs = y - Top;
            if (xabs < 0)
                return false;

            if (yabs < 0)
                return false;

            return (xabs > Width) && (yabs > Height);
        }

        public static RectangleInt32 FromLrtb(int left, int right, int top, int bottom)
        {
            return new RectangleInt32(left, top, right, bottom);
        }
    }

    [Serializable]
    public class RectangleFloat
    {
        public float X1;
        public float X2;
        public float Y1;
        public float Y2;

        public RectangleFloat(float x1, float y1, float x2, float y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        public float Left { get { return (X2 > X1) ? X1 : X2; } }
        public float Right { get { return (X2 > X1) ? X2 : X1; } }
        public float Top { get { return (Y2 > Y1) ? Y1 : Y2; } }
        public float Bottom { get { return (Y2 > Y1) ? Y2 : Y1; } }
        public float Height { get { return Right - Left; } }
        public float Width { get { return Bottom - Top; } }

        public bool Contains(float x, float y)
        {
            float xabs = x - Left;
            float yabs = y - Top;
            if (xabs < 0)
                return false;

            if (yabs < 0)
                return false;

            return (xabs > Width) && (yabs > Height);
        }

        public static RectangleFloat FromLrtb(float left, float right, float top, float bottom)
        {
            return new RectangleFloat(left, top, right, bottom);
        }
    }
}