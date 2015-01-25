using System;

namespace mpBackup.MpUtilities
{
    /// <summary>
    /// Defines an attribute containing a string representation of the member
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class StringValueAttribute : Attribute
    {
        private readonly string text;

        /// <summary>
        /// The text which belongs to this member
        /// </summary>
        public string Text { get { return text; } }

        /// <summary>
        /// Creates a new StringValue attribute with the specified text
        /// </summary>
        /// <param name="text"></param>
        public StringValueAttribute(string text)
        {
            text.ThrowIfNull("text");
            this.text = text;
        }
    }
}
