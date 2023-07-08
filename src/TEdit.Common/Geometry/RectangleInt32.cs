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

namespace TEdit.Geometry;

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

    public int Left { get { return X2 > X1 ? X1 : X2; } }
    public int Right { get { return X2 > X1 ? X2 : X1; } }
    public int Top { get { return Y2 > Y1 ? Y1 : Y2; } }
    public int Bottom { get { return Y2 > Y1 ? Y2 : Y1; } }
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

        return xabs > Width && yabs > Height;
    }

    public static RectangleInt32 FromLrtb(int left, int right, int top, int bottom)
    {
        return new RectangleInt32(left, top, right, bottom);
    }
}
