/* 
Copyright (c) 2011 BinaryConstruct
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

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
    }
}