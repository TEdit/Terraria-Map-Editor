using System;

namespace TEdit.Common
{

    public static class EnumerationExtensions
    {

        /* Boolean checks */
        public static bool Has<T>(this Enum type, T value)
        {
            // will work with multi-type enums
            try   { return (((int) (object) type &  (int) (object) value) != 0); }
            catch { return false; }
        }
        // duplicate niceity
        public static bool HasAnyOf<T>(this Enum type, T value) { return Has(type, value); }

        public static bool HasAllOf<T>(this Enum type, T value)
        {
            // will demand all of them
            try   { return (((int) (object) type &  (int) (object) value) == (int) (object) value); }
            catch { return false; }
        }

        public static bool Is<T>(this Enum type, T value)
        {
            try   { return   (int) (object) type == (int) (object) value; }
            catch { return false; }
        }
        public static bool IsSingular(this Enum type)
        {
            try   { return (((int) (object) type &  ((int) (object) type - 1)) == 0); }  // power of two: (x & (x − 1)) == 0
            catch { return false; }
        }


        /* NOT Boolean checks */
        // uses up more code space than "!", but a little bit more descriptive
        public static bool HasNo<T>(this Enum type, T value)
        {
            try   { return (((int) (object) type &  (int) (object) value) == 0); }
            catch { return false; }
        }
        public static bool HasNoneOf<T>(this Enum type, T value) { return HasNo(type, value); }

        public static bool HasNotAllOf<T>(this Enum type, T value)
        {
            try   { return (((int) (object) type &  (int) (object) value) != (int) (object) value); }
            catch { return false; }
        }

        public static bool IsNot<T>(this Enum type, T value)
        {
            try   { return   (int) (object) type != (int) (object) value; }
            catch { return false; }
        }
        public static bool IsMultiple(this Enum type)
        {
            try   { return (((int) (object) type &  ((int) (object) type - 1)) != 0); }
            catch { return false; }
        }

        /* Flag change methods */
        public static T Add<T>(this Enum type, T value)
        {
            try   { return (T) (object) (((int) (object) type | (int) (object) value)); }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not append value from enumerated type '{0}'.",
                        typeof (T).Name
                        ), ex);
            }
        }

        public static T Remove<T>(this Enum type, T value)
        {
            try { return (T) (object) (((int) (object) type & ~(int) (object) value)); }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not remove value from enumerated type '{0}'.",
                        typeof (T).Name
                        ), ex);
            }
        }

        public static T Filter<T>(this Enum type, T value)
        {
            try { return (T) (object) (((int) (object) type & (int) (object) value)); }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format(
                        "Could not filter value from enumerated type '{0}'.",
                        typeof (T).Name
                        ), ex);
            }
        }

        /* Casting methods */
        public static T Convert<T>(this Enum type, string value) where T : struct
        {
            T result;
            if (!Enum.TryParse(value, true, out result))  // if this result fails, fallback to the original value
            {
                Enum.TryParse(type.ToString(), true, out result);
            }
            return result;
        }

        public static T Convert<T>(this Enum type, System.Xml.Linq.XAttribute value) where T : struct
            { return Convert<T>(type, (string)value); }

        // works for all numbers
        public static T Convert<T, I>(this Enum type, I value) where T : struct
            { return (T)Enum.ToObject(typeof(T), value); }

    }
}