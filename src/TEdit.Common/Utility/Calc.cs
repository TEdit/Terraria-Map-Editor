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

namespace TEdit.Utility
{
    public static class Calc
    {
        /// <summary>
        /// Returns a progress percentage in the range of 0 to 100. 
        /// </summary>
        /// <param name="index">The current counter position.</param>
        /// <param name="total">The maximum counter position.</param>
        /// <returns>Progress percentage (0-100)</returns>
        public static int ProgressPercentage(this int index, int total)
        {
            int val = (int)((float)index / total * 100.0f);

            if (val > 100)
                val = 100;
            if (val < 0)
                val = 0;

            return val;
        }


        public static double Lerp(double value1, double value2, double amount) => value1 + (value2 - value1) * amount;
        public static float Lerp(float value1, float value2, float amount) => value1 + (value2 - value1) * amount;


        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(max) > 0)
                return max;
            return value.CompareTo(min) < 0 ? min : value;
        }

        public static float GetLerpValue(float from, float to, float t, bool clamped = false)
        {
            if (clamped)
            {
                if ((double)from < (double)to)
                {
                    if ((double)t < (double)from)
                        return 0.0f;
                    if ((double)t > (double)to)
                        return 1f;
                }
                else
                {
                    if ((double)t < (double)to)
                        return 1f;
                    if ((double)t > (double)from)
                        return 0.0f;
                }
            }
            return (float)(((double)t - (double)from) / ((double)to - (double)from));
        }

        public static float Remap(
            float fromValue,
            float fromMin,
            float fromMax,
            float toMin,
            float toMax,
            bool clamped = true)
        {
            return Lerp(toMin, toMax, GetLerpValue(fromMin, fromMax, fromValue, clamped));
        }
    }
}
