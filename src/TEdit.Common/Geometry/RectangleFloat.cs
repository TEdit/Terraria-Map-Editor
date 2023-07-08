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

    public float Left { get { return X2 > X1 ? X1 : X2; } }
    public float Right { get { return X2 > X1 ? X2 : X1; } }
    public float Top { get { return Y2 > Y1 ? Y1 : Y2; } }
    public float Bottom { get { return Y2 > Y1 ? Y2 : Y1; } }
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

        return xabs > Width && yabs > Height;
    }

    public static RectangleFloat FromLrtb(float left, float right, float top, float bottom)
    {
        return new RectangleFloat(left, top, right, bottom);
    }
}
