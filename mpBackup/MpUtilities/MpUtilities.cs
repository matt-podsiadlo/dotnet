using System;
using System.Reflection;

namespace mpBackup.MpUtilities
{
    /// <summary>
    /// A class containing utility methods.
    /// </summary>
    public static class MpUtilities
    {
        /// <summary>
        /// Throws an exception if the string is null or empty
        /// </summary>
        public static void ThrowIfNullOrEmpty(this string str, string paramName)
        {
            str.ThrowIfNull(paramName);
            if (str.Length == 0)
            {
                throw new ArgumentException("Parameter was empty", paramName);
            }
        }

        /// <summary>
        /// Extension method on object, which throws a ArgumentNullException if obj is null
        /// </summary>
        public static void ThrowIfNull(this object obj, string paramName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Tries to convert the specified object to a string. Uses custom type converters if available.
        /// Returns null for a null object.
        /// </summary>
        public static string ConvertToString(object o)
        {
            if (o == null)
            {
                return null;
            }

            // Get the type converter if available
            if (o.GetType().IsEnum)
            {
                var enumConverter = new EnumStringValueTypeConverter();
                return (string)enumConverter.ConvertTo(o, typeof(string));
            }

            return o.ToString();
        }

        /// <summary>
        /// Retrieves the underlying type if the specified type is a Nullable, or the type itself otherwise.
        /// </summary>
        public static Type GetNonNullableType(Type type)
        {
            if (!type.IsGenericType || !typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            {
                return type; // Not a Nullable.
            }
            // First: Find the Nullable type.
            while (!(type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))))
            {
                type = type.BaseType;

                if (type == null)
                {
                    return null;
                }
            }
            // Return the type which is encapsulated by the Nullable.
            return type.GetGenericArguments()[0];
        }

        /// <summary>
        /// Returns the first matching custom attribute (or null) of the specified member
        /// </summary>
        public static T GetCustomAttribute<T>(this MemberInfo info) where T : Attribute
        {
            object[] results = info.GetCustomAttributes(typeof(T), false);
            return results.Length == 0 ? null : (T)results[0];
        }
    }

    
}
