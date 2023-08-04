using System;

namespace TEdit.Utility;

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
    //
    // Summary:
    //     Linearly interpolates between two values. This method is a less efficient, more
    //     precise version of Microsoft.Xna.Framework.MathHelper.Lerp(System.Single,System.Single,System.Single).
    //     See remarks for more info.
    //
    // Parameters:
    //   value1:
    //     Source value.
    //
    //   value2:
    //     Destination value.
    //
    //   amount:
    //     Value between 0 and 1 indicating the weight of value2.
    //
    // Returns:
    //     Interpolated value.
    //
    // Remarks:
    //     This method performs the linear interpolation based on the following formula:
    //     ((1 - amount) * value1) + (value2 * amount)
    //     . Passing amount a value of 0 will cause value1 to be returned, a value of 1
    //     will cause value2 to be returned. This method does not have the floating point
    //     precision issue that Microsoft.Xna.Framework.MathHelper.Lerp(System.Single,System.Single,System.Single)
    //     has. i.e. If there is a big gap between value1 and value2 in magnitude (e.g.
    //     value1=10000000000000000, value2=1), right at the edge of the interpolation range
    //     (amount=1), Microsoft.Xna.Framework.MathHelper.Lerp(System.Single,System.Single,System.Single)
    //     will return 0 (whereas it should return 1). This also holds for value1=10^17,
    //     value2=10; value1=10^18,value2=10^2... so on. For an in depth explanation of
    //     the issue, see below references: Relevant Wikipedia Article: https://en.wikipedia.org/wiki/Linear_interpolation#Programming_language_support
    //     Relevant StackOverflow Answer: http://stackoverflow.com/questions/4353525/floating-point-linear-interpolation#answer-23716956
    public static float LerpPrecise(float value1, float value2, float amount)
    {
        return (1f - amount) * value1 + value2 * amount;
    }

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
