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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace TEdit.Utility
{
    public static class Serialization
    {
        #region XML

        public static bool WriteXml<T>(this T obj, string file)
        {
            string path = Path.GetDirectoryName(file);

            if (!String.IsNullOrWhiteSpace(path))
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);


            var xs = new XmlSerializer(typeof(T));
            using (var sw = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(sw, obj);
                sw.Close();
            }

            return true;
        }

        public static T ReadXml<T>(string file)
        {
            var xs = new XmlSerializer(typeof(T));
            using (var sr = new StreamReader(file))
            {
                return (T)xs.Deserialize(sr);
            }
        }

        /// <summary>Serializes an object of type T in to an xml string</summary>
        /// <typeparam name="T">Any class type</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <returns>A string that represents Xml, empty otherwise</returns>
        public static string SerializeXml<T>(this T obj) where T : class, new()
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        /// <summary>Deserializes an xml string in to an object of Type T</summary>
        /// <typeparam name="T">Any class type</typeparam>
        /// <param name="xml">Xml as string to deserialize from</param>
        /// <returns>A new object of type T is successful, null if failed</returns>
        public static T DeserializeXml<T>(string xml) where T : class, new()
        {
            if (xml == null) throw new ArgumentNullException("xml");

            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        #endregion

        #region Binary

        public static bool WriteBinary<T>(this T obj, string file)
        {
            string path = Path.GetDirectoryName(file);

            if (!String.IsNullOrWhiteSpace(path))
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);


            var xs = new BinaryFormatter();
            using (var sw = new FileStream(file, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(sw, obj);
                sw.Close();
            }

            return true;
        }

        public static T ReadBinary<T>(string file)
        {
            var xs = new BinaryFormatter();
            using (var sr = new FileStream(file, FileMode.Open, FileAccess.Write))
            {
                return (T)xs.Deserialize(sr);
            }
        }

        /// <summary>Serializes an object of type T in to an xml string</summary>
        /// <typeparam name="T">Any class type</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <returns>A string that represents binary, empty otherwise</returns>
        public static string SerializeBinary<T>(this T obj) where T : class, new()
        {
            if (obj == null) throw new ArgumentNullException("obj");

            var serializer = new BinaryFormatter();
            using (var writer = new MemoryStream())
            {
                serializer.Serialize(writer, obj);
                writer.Position = 0;
                return new StreamReader(writer).ReadToEnd();
            }
        }

        /// <summary>Deserializes an xml string in to an object of Type T</summary>
        /// <typeparam name="T">Any class type</typeparam>
        /// <param name="data">Data as string to deserialize from</param>
        /// <returns>A new object of type T is successful, null if failed</returns>
        public static T DeserializeBinary<T>(string data) where T : class, new()
        {
            if (data == null) throw new ArgumentNullException("data");

            var serializer = new BinaryFormatter();
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.Write(data);
                ms.Position = 0;
                return (T)serializer.Deserialize(ms);
            }
        }

        #endregion

        #region Cloning and Copying
        /// <summary>
        /// Makes a deep copy of the specified Object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToCopy">Object to make a deep copy of.</param>
        /// <returns>Deep copy of the Object</returns>
        public static T DeepCopy<T>(this T objectToCopy) where T : class
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, objectToCopy);
                ms.Position = 0;
                return (T)bf.Deserialize(ms);
            }
        }

        public static T Clone<T>(this T obj)
        {
            T copy = default(T);
            using (var stream = new MemoryStream())
            {
                var ser = new DataContractSerializer(typeof(T));
                ser.WriteObject(stream, obj);
                stream.Position = 0;
                copy = (T)ser.ReadObject(stream);
            }
            return copy;
        }

        public static void CopyValuesTo<T>(this T source, T dest)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite);

            foreach (var property in properties)
            {
                if (property.GetSetMethod() == null) continue;
                property.SetValue(dest, property.GetValue(source, null), null);
            }
        }
        #endregion
    }
}