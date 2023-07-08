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

namespace TEdit.Geometry;

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
        T zero = default;
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
