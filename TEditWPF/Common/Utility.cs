using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrariaMapEditor.Common
{
    public static class Utility
    {
        public static bool IsNumeric(string val, System.Globalization.NumberStyles numberStyle)
        {
            Int32 result;
            return Int32.TryParse(val, numberStyle,
                System.Globalization.CultureInfo.CurrentCulture, out result);
        }
    }
}
