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
public struct Vector3<T> where T : struct
{
    public T X;
    public T Y;
    public T Z;

    public Vector3(T x, T y, T z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString()
    {
        return $"({X},{Y},{Z})";
    }

    public bool Equals(Vector3<T> other)
    {
        return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector3<T>)) return false;
        return Equals((Vector3<T>)obj);
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

    public static bool operator ==(Vector3<T> left, Vector3<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3<T> left, Vector3<T> right)
    {
        return !left.Equals(right);
    }
}
