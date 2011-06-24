using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TEditWPF.Common
{
    public static class Utility
    {
        public static bool IsNumeric(string val, NumberStyles numberStyle)
        {
            Int32 result;
            return Int32.TryParse(val, numberStyle,
                                  CultureInfo.CurrentCulture, out result);
        }


        /// <summary>
        /// Makes a deep copy of the specified Object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToCopy">Object to make a deep copy of.</param>
        /// <returns>Deep copy of the Object</returns>
        public static T Make<T>(T objectToCopy) where T : class
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, objectToCopy);
                ms.Position = 0;
                return (T) bf.Deserialize(ms);
            }
        }
    }
}