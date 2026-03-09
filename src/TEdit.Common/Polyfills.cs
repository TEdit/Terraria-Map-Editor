#if NETSTANDARD2_0
using System;

namespace System
{
    internal static class MathF
    {
        public static float Sqrt(float x) => (float)Math.Sqrt(x);
        public static float Ceiling(float x) => (float)Math.Ceiling(x);
        public static float Floor(float x) => (float)Math.Floor(x);
    }
}
#endif
