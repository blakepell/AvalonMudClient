﻿using System.Collections.Generic;
using System.Linq;
using Cysharp.Text;

namespace MoonSharp.Interpreter.Serialization
{
    public static class SerializationExtensions
    {
        private static HashSet<string> LUAKEYWORDS = new HashSet<string>
        {
            "and", "break", "do", "else", "elseif", "end", "false", "for", "function", "goto", "if", "in", "local",
            "nil", "not", "or", "repeat", "return", "then", "true", "until", "while"
        };

        public static string Serialize(this Table table, bool prefixReturn = false, int tabs = 0)
        {
            if (table.OwnerScript != null)
            {
                throw new ScriptRuntimeException("Table is not a prime table.");
            }

            string tabStr = new string('\t', tabs);

            using (var sb = ZString.CreateStringBuilder())
            {
                if (prefixReturn)
                {
                    sb.Append("return ");
                }

                if (!table.Values.Any())
                {
                    sb.Append("${ }");
                    return sb.ToString();
                }

                sb.AppendLine("${");

                foreach (var tp in table.Pairs)
                {
                    sb.Append(tabStr);
                    string key = IsStringIdentifierValid(tp.Key) ? tp.Key.String : $"[{SerializeValue(tp.Key, tabs + 1)}]";
                    sb.AppendFormat("\t{0} = {1},\n", key, SerializeValue(tp.Value, tabs + 1));
                }

                sb.Append(tabStr);
                sb.Append("}");

                if (tabs == 0)
                {
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        private static bool IsStringIdentifierValid(DynValue dynValue)
        {
            if (dynValue.Type != DataType.String)
            {
                return false;
            }

            if (dynValue.String.Length == 0)
            {
                return false;
            }

            if (LUAKEYWORDS.Contains(dynValue.String))
            {
                return false;
            }

            if (!char.IsLetter(dynValue.String[0]) && (dynValue.String[0] != '_'))
            {
                return false;
            }

            foreach (char c in dynValue.String)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    return false;
                }
            }

            return true;
        }

        public static string SerializeValue(this DynValue dynValue, int tabs = 0)
        {
            if (dynValue.Type == DataType.Nil || dynValue.Type == DataType.Void)
            {
                return "nil";
            }

            if (dynValue.Type == DataType.Tuple)
            {
                return (dynValue.Tuple.Any() ? SerializeValue(dynValue.Tuple[0], tabs) : "nil");
            }

            if (dynValue.Type == DataType.Number)
            {
                return dynValue.Number.ToString("r");
            }

            if (dynValue.Type == DataType.Boolean)
            {
                return dynValue.Boolean ? "true" : "false";
            }

            if (dynValue.Type == DataType.String)
            {
                return EscapeString(dynValue.String ?? "");
            }

            if (dynValue.Type == DataType.Table && dynValue.Table.OwnerScript == null)
            {
                return Serialize(dynValue.Table, false, tabs);
            }

            throw new ScriptRuntimeException("Value is not a primitive value or a prime table.");
        }

        /// <summary>
        /// Escapes a string for common escape sequences.
        /// </summary>
        /// <param name="s"></param>
        private static string EscapeString(string s)
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                sb.Append(s);
                sb.Replace(@"\", @"\\");
                sb.Replace("\n", @"\n");
                sb.Replace("\r", @"\r");
                sb.Replace("\t", @"\t");
                sb.Replace("\a", @"\a");
                sb.Replace("\f", @"\f");
                sb.Replace("\b", @"\b");
                sb.Replace("\v", @"\v");
                sb.Replace("\"", "\\\"");
                sb.Replace("\'", @"\'");
                sb.AppendFormat("\"{0}\"", s);
                return sb.ToString();
            }
        }
    }
}