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

namespace TEdit.Geometry.Primitives
{
    #region Generics
    [Serializable]
    public struct Vector2<T> where T : struct
    {
        public T X;
        public T Y;

        public Vector2(T x, T y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        #region Equality

        public bool Equals(Vector2<T> other)
        {
            return other.X.Equals(X) && other.Y.Equals(Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector2<T>)) return false;
            return Equals((Vector2<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static bool operator ==(Vector2<T> left, Vector2<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2<T> left, Vector2<T> right)
        {
            return !left.Equals(right);
        }

        #endregion
    }

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

        #region Equality

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
                result = (result * 397) ^ Y.GetHashCode();
                result = (result * 397) ^ Z.GetHashCode();
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
        #endregion
    }

    [Serializable]
    public struct Vector4<T> where T : struct
    {
        public T X;
        public T Y;
        public T Z;
        public T W;

        public Vector4(T x, T y, T z, T w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override string ToString()
        {
            return $"({X},{Y},{Z},{W})";
        }

        #region Equality

        public bool Equals(Vector4<T> other)
        {
            return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z) && other.W.Equals(W);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (Vector4<T>)) return false;
            return Equals((Vector4<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = X.GetHashCode();
                result = (result*397) ^ Y.GetHashCode();
                result = (result*397) ^ Z.GetHashCode();
                result = (result*397) ^ W.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(Vector4<T> left, Vector4<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4<T> left, Vector4<T> right)
        {
            return !left.Equals(right);
        }

        #endregion
    }


    #endregion

    #region Int32
    [Serializable]
    public struct Vector2Int32
    {
        public Int32 X;
        public Int32 Y;

        public Vector2Int32(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X:0},{Y:0})";
        }

        public static bool Parse(string text, out Vector2Int32 vector)
        {
            vector = new Vector2Int32();
            if (string.IsNullOrWhiteSpace(text)) return false;

            var split = text.Split(',', 'x');
            if (split.Length != 2) return false;
            int x, y;
            if (int.TryParse(split[0], out x) ||
                int.TryParse(split[1], out y))
                return false;

            vector = new Vector2Int32(x, y);
            return true;
        }

        #region Equality
        public bool Equals(Vector2Int32 other)
        {
            return other.Y == Y && other.X == X;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector2Int32)) return false;
            return Equals((Vector2Int32)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Y * 397) ^ X;
            }
        }

        public static bool operator ==(Vector2Int32 left, Vector2Int32 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2Int32 left, Vector2Int32 right)
        {
            return !left.Equals(right);
        }
        #endregion
    }

    [Serializable]
    public struct Vector3Int32
    {
        public Int32 X;
        public Int32 Y;
        public Int32 Z;

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

        #region Equality

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
                result = (result * 397) ^ Y;
                result = (result * 397) ^ Z;
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
        #endregion
    }

    [Serializable]
    public struct Vector4Int32
    {

        public Int32 X;
        public Int32 Y;
        public Int32 Z;
        public Int32 W;

        public Vector4Int32(Int32 x, Int32 y, Int32 z, Int32 w)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override string ToString()
        {
            return $"({X:0},{Y:0},{Z:0},{W:0})";
        }

        public static bool Parse(string text, out Vector4Int32 vector)
        {
            vector = new Vector4Int32();
            if (string.IsNullOrWhiteSpace(text)) return false;

            var split = text.Split(',', 'x');
            if (split.Length != 4) return false;
            int x, y, z, w;
            if (int.TryParse(split[0], out x) ||
                int.TryParse(split[1], out y) ||
                int.TryParse(split[2], out z) ||
                int.TryParse(split[3], out w))
                return false;

            vector = new Vector4Int32(x, y, z, w);
            return true;
        }

        #region Equality

        public bool Equals(Vector4Int32 other)
        {
            return other.W == W && other.X == X && other.Y == Y && other.Z == Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector4Int32)) return false;
            return Equals((Vector4Int32)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = W;
                result = (result * 397) ^ X;
                result = (result * 397) ^ Y;
                result = (result * 397) ^ Z;
                return result;
            }
        }

        public static bool operator ==(Vector4Int32 left, Vector4Int32 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4Int32 left, Vector4Int32 right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
    #endregion

    #region Float

    [Serializable]
    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
            : this()
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X:0.000},{Y:0.000})";
        }

        public static bool Parse(string text, out Vector2 vector)
        {
            vector = new Vector2();
            if (string.IsNullOrWhiteSpace(text)) return false;

            var split = text.Split(',', 'x');
            if (split.Length != 2) return false;
            float x, y;
            if (float.TryParse(split[0], out x) ||
                float.TryParse(split[1], out y))
                return false;

            vector = new Vector2(x, y);
            return true;
        }

        #region Equality

        public bool Equals(Vector2 other)
        {
            return other.X.Equals(X) && other.Y.Equals(Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector2)) return false;
            return Equals((Vector2)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !left.Equals(right);
        }

        #endregion
    }

    [Serializable]
    public struct Vector3
    {

        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
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

        public static bool Parse(string text, out Vector3 vector)
        {
            vector = new Vector3();
            if (string.IsNullOrWhiteSpace(text)) return false;

            var split = text.Split(',', 'x');
            if (split.Length != 3) return false;
            float x, y, z;
            if (float.TryParse(split[0], out x) ||
                float.TryParse(split[1], out y) ||
                float.TryParse(split[2], out z))
                return false;

            vector = new Vector3(x, y, z);
            return true;
        }

        #region Equality

        public bool Equals(Vector3 other)
        {
            return other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector3)) return false;
            return Equals((Vector3)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = X.GetHashCode();
                result = (result * 397) ^ Y.GetHashCode();
                result = (result * 397) ^ Z.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !left.Equals(right);
        }

        #endregion
    }

    [Serializable]
    public struct Vector4
    {

        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4(float x, float y, float z, float w)
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

        public static bool Parse(string text, out Vector4 vector)
        {
            vector = new Vector4();
            if (string.IsNullOrWhiteSpace(text)) return false;

            var split = text.Split(',', 'x');
            if (split.Length != 4) return false;
            int x, y, z, w;
            if (int.TryParse(split[0], out x) ||
                int.TryParse(split[1], out y) ||
                int.TryParse(split[2], out z) ||
                int.TryParse(split[3], out w))
                return false;

            vector = new Vector4(x, y, y, z);
            return true;
        }

        #region Equality

        public bool Equals(Vector4 other)
        {
            return other.W.Equals(W) && other.X.Equals(X) && other.Y.Equals(Y) && other.Z.Equals(Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector4)) return false;
            return Equals((Vector4)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = W.GetHashCode();
                result = (result * 397) ^ X.GetHashCode();
                result = (result * 397) ^ Y.GetHashCode();
                result = (result * 397) ^ Z.GetHashCode();
                return result;
            }
        }

        public static bool operator ==(Vector4 left, Vector4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4 left, Vector4 right)
        {
            return !left.Equals(right);
        }

        #endregion
    }

    #endregion

    #region Short
    [Serializable]
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

        #region Equality
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
                return (Y * 397) ^ X;
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
        #endregion
    }

    [Serializable]
    public struct Vector3Short
    {

        public short X;
        public short Y;
        public short Z;

        public Vector3Short(short x, short y, short z)
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

        public static bool Parse(string text, out Vector3Short vector)
        {
            vector = new Vector3Short();
            if (string.IsNullOrWhiteSpace(text)) return false;

            var split = text.Split(',', 'x');
            if (split.Length != 3) return false;
            short x, y, z;
            if (short.TryParse(split[0], out x) ||
                short.TryParse(split[1], out y) ||
                short.TryParse(split[2], out z))
                return false;

            vector = new Vector3Short(x, y, z);
            return true;
        }

        #region Equality

        public bool Equals(Vector3Short other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector3Short)) return false;
            return Equals((Vector3Short)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = X;
                result = (result * 397) ^ Y;
                result = (result * 397) ^ Z;
                return result;
            }
        }

        public static bool operator ==(Vector3Short left, Vector3Short right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3Short left, Vector3Short right)
        {
            return !left.Equals(right);
        }
        #endregion
    }

    [Serializable]
    public struct Vector4Short
    {

        public short X;
        public short Y;
        public short Z;
        public short W;

        public Vector4Short(short x, short y, short z, short w)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override string ToString()
        {
            return $"({X:0},{Y:0},{Z:0},{W:0})";
        }

        public static bool Parse(string text, out Vector4Short vector)
        {
            vector = new Vector4Short();
            if (string.IsNullOrWhiteSpace(text)) return false;

            var split = text.Split(',', 'x');
            if (split.Length != 4) return false;
            short x, y, z, w;
            if (short.TryParse(split[0], out x) ||
                short.TryParse(split[1], out y) ||
                short.TryParse(split[2], out z) ||
                short.TryParse(split[3], out w))
                return false;

            vector = new Vector4Short(x, y, z, w);
            return true;
        }

        #region Equality

        public bool Equals(Vector4Short other)
        {
            return other.W == W && other.X == X && other.Y == Y && other.Z == Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector4Short)) return false;
            return Equals((Vector4Short)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = W;
                result = (result * 397) ^ X;
                result = (result * 397) ^ Y;
                result = (result * 397) ^ Z;
                return result;
            }
        }

        public static bool operator ==(Vector4Short left, Vector4Short right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4Short left, Vector4Short right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
    #endregion

    #region Byte
    [Serializable]
    public struct Vector2Byte
    {
        public byte X;
        public byte Y;

        public Vector2Byte(byte x, byte y)
            : this()
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"({X:0},{Y:0})";
        }

        #region Equality
        public bool Equals(Vector2Byte other)
        {
            return other.Y == Y && other.X == X;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector2Byte)) return false;
            return Equals((Vector2Byte)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Y * 397) ^ X;
            }
        }

        public static bool operator ==(Vector2Byte left, Vector2Byte right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2Byte left, Vector2Byte right)
        {
            return !left.Equals(right);
        }
        #endregion
    }

    [Serializable]
    public struct Vector3Byte
    {

        public byte X;
        public byte Y;
        public byte Z;

        public Vector3Byte(byte x, byte y, byte z)
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

        #region Equality

        public bool Equals(Vector3Byte other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector3Byte)) return false;
            return Equals((Vector3Byte)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = X;
                result = (result * 397) ^ Y;
                result = (result * 397) ^ Z;
                return result;
            }
        }

        public static bool operator ==(Vector3Byte left, Vector3Byte right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3Byte left, Vector3Byte right)
        {
            return !left.Equals(right);
        }
        #endregion
    }

    [Serializable]
    public struct Vector4Byte
    {

        public byte X;
        public byte Y;
        public byte Z;
        public byte W;

        public Vector4Byte(byte x, byte y, byte z, byte w)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public override string ToString()
        {
            return $"({X:0},{Y:0},{Z:0},{W:0})";
        }

        #region Equality

        public bool Equals(Vector4Byte other)
        {
            return other.W == W && other.X == X && other.Y == Y && other.Z == Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Vector4Byte)) return false;
            return Equals((Vector4Byte)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = W;
                result = (result * 397) ^ X;
                result = (result * 397) ^ Y;
                result = (result * 397) ^ Z;
                return result;
            }
        }

        public static bool operator ==(Vector4Byte left, Vector4Byte right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4Byte left, Vector4Byte right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
    #endregion
}