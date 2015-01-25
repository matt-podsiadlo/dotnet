using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace mpBackup.MpUtilities
{
    /// <summary>
    /// A specialized type converter which will convert any enum type into the string 
    /// specified in the StringValue attribute.
    /// </summary>
    public class EnumStringValueTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return typeof(Enum).IsAssignableFrom(MpUtilities.GetNonNullableType(sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType.Equals(typeof(string));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
            {
                return null;
            }

            Type enumType = value.GetType();
            if (!CanConvertTo(context, destinationType) || !CanConvertFrom(context, enumType))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }

            // Get the field in the enum.
            FieldInfo field = enumType.GetField(value.ToString());

            // Check if this element has a StringValueAttribute.
            StringValueAttribute attribute = field.GetCustomAttribute<StringValueAttribute>();

            // If it has an attribute, then return the specified text, otherwise use the default overload.
            return attribute != null ? attribute.Text : value.ToString();
        }
    }
}
