using System;
using System.Text;

namespace MoonSharp.Interpreter.Interop.Converters
{
    internal static class StringConversions
    {
        internal static StringSubtype GetStringSubtype(Type desiredType)
        {
            if (desiredType == typeof(string))
            {
                return StringSubtype.String;
            }

            if (desiredType == typeof(StringBuilder))
            {
                return StringSubtype.StringBuilder;
            }

            if (desiredType == typeof(char))
            {
                return StringSubtype.Char;
            }

            return StringSubtype.None;
        }


        internal static object ConvertString(StringSubtype stringSubType, string str, Type desiredType, DataType dataType)
        {
            switch (stringSubType)
            {
                case StringSubtype.String:
                    return str;
                case StringSubtype.StringBuilder:
                    return new StringBuilder(str);
                case StringSubtype.Char:
                    if (!string.IsNullOrEmpty(str))
                    {
                        return str[0];
                    }

                    break;
                case StringSubtype.None:
                default:
                    break;
            }

            throw ScriptRuntimeException.ConvertObjectFailed(dataType, desiredType);
        }

        internal enum StringSubtype
        {
            None,
            String,
            StringBuilder,
            Char
        }
    }
}