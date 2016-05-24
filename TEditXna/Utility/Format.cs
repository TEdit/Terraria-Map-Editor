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
using System.Globalization;

namespace TEdit.Utility
{
    public static class Format
    {
        public static bool IsNumeric(this string val, NumberStyles numberStyle)
        {
            Int32 result;
            return Int32.TryParse(val, numberStyle,
                                  CultureInfo.CurrentCulture, out result);
        }
    }
}