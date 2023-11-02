/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System.Text;

namespace MoonSharp.Interpreter.Wpf.Types
{
    /// <summary>
    /// A thin wrapper around the string builder that Lua can consume.
    /// </summary>
    public class LuaStringBuilder
    {
        private StringBuilder _sb;

        public LuaStringBuilder()
        {
            _sb = new();
        }

        public LuaStringBuilder(string text)
        {
            _sb = new(text);
        }

        [MoonSharpModuleMethod(Description = "Appends the specified text to the string builder.",
                               ParameterCount = 1)]
        public void Append(string text)
        {
            _sb.Append(text);
        }

        [MoonSharpModuleMethod(Description = "Appends the specified text and then a line terminator to the string builder.",
                               ParameterCount = 1)]
        public void AppendLine(string text)
        {
            _sb.AppendLine(text);
        }

        [MoonSharpModuleMethod(Description = "Renders the string builder to a realized string.")]
        public override string ToString()
        {
            return _sb.ToString();
        }

        [MoonSharpModuleMethod(Description = "Clears the contents of the StringBuilder")]
        public void Clear()
        {
            _sb.Clear();
        }

        [MoonSharpModuleMethod(Description = "Replaces one value with another value.",
                               ParameterCount = 2)]
        public void Replace(string oldValue, string newValue)
        {
            _sb.Replace(oldValue, newValue);
        }

        [MoonSharpModuleMethod(Description = "Inserts a value at the specified index.",
                               ParameterCount = 2)]
        public void Insert(int index, string value)
        {
            _sb.Insert(index, value);
        }

        [MoonSharpModuleMethod(Description = "Removes the specified number of characters at the start index.",
                               ParameterCount = 2)]
        public void Remove(int startIndex, int length)
        {
            _sb.Remove(startIndex, length);
        }

        [MoonSharpModuleMethod(Description = "The length of the string builder.",
                               ParameterCount = 0)]
        public int Length()
        {
            return _sb.Length;
        }
    }
}
