using Avalon.Common.Colors;
using System;
using Avalon.Colors;
using Avalon.Common.Models;

namespace Avalon.Extensions
{
    /// <summary>
    /// Extension methods for internal types.
    /// </summary>
    public static class ExtensionMethods
    {

        /// <summary>
        /// Converts the specified string into a Line for rendering.  ANSI codes are removed from the value
        /// that is put into the Text property.
        /// </summary>
        /// <param name="text"></param>
        public static Line ToLine(this string text)
        {
            var line = new Line
            {
                Text = Colorizer.RemoveAllAnsiCodes(text),
                FormattedText = text,
            };

            return line;
        }

        /// <summary>
        /// Reports the zero based index of the first occurence of a matching string.
        /// </summary>
        /// <param name="span">The Span to search.</param>
        /// <param name="value">The string to search for.</param>
        /// <param name="startIndex"></param>
        /// <returns>Returns the zero based index or a -1 if the string isn't found or the
        /// startIndex greater than the length of the string.
        /// </returns>
        public static int SafeIndexOf(this ReadOnlySpan<char> span, string value, int startIndex)
        {
            if (startIndex > span.Length - 1)
            {
                return -1;
            }

            return span.IndexOf(value, startIndex);
        }

        /// <summary>
        /// Returns the zero based index of the first occurence of a matching string.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int IndexOf(this ReadOnlySpan<char> span, string value, int startIndex)
        {
            var indexInSlice = span.Slice(startIndex).IndexOf(value.AsSpan(), StringComparison.Ordinal);

            if (indexInSlice == -1)
            {
                return -1;
            }
            
            return startIndex + indexInSlice;
        }

        ///// <summary>
        ///// Reports the zero based index of the first occurence of a ANSI escape code that
        ///// isn't one of the special formatting codes that we support (underline, reverse
        ///// text, etc.).
        ///// </summary>
        ///// <remarks>
        ///// All non supported ansi sequences should be stripped from the string
        ///// before we get to this point. E.g. this makes an assumption that input
        ///// is validated before it gets here.
        ///// </remarks>
        ///// <param name="str">The string to search.</param>
        ///// <param name="value">The string to search for.</param>
        ///// <param name="startIndex">The starting position.</param>
        //public static int IndexOfNextColorCode(this string str, string value, int startIndex)
        //{
        //    // Look up both the starting position of the value and the second value that
        //    // it isn't supposed to be after those positions.
        //    int index = str.SafeIndexOf(value, startIndex);

        //    // Not found at all, return -1.
        //    if (index == -1)
        //    {
        //        return -1;
        //    }

        //    int reverseIndex = str.SafeIndexOf(AnsiColors.Reverse, startIndex);
        //    int underlineIndex = str.SafeIndexOf(AnsiColors.Underline, startIndex);

        //    // Index isn't -1 and the notIndex was not found, return the index.
        //    if (index != reverseIndex && index != underlineIndex)
        //    {
        //        return index;
        //    }

        //    // index and notIndex are equal, search for the next location where index exists
        //    // but notIndex does not.
        //    int start = index;

        //    while ((str.SafeIndexOf(value, start)) >= 0)
        //    {
        //        index = str.SafeIndexOf(value, start);

        //        // Not found at all, return -1.
        //        if (index == -1)
        //        {
        //            return -1;
        //        }

        //        reverseIndex = str.SafeIndexOf(AnsiColors.Reverse, start);
        //        underlineIndex = str.SafeIndexOf(AnsiColors.Underline, start);

        //        // Index isn't -1 and the notIndex was not found, return the index.
        //        if (index != reverseIndex && index != underlineIndex)
        //        {
        //            return index;
        //        }

        //        // Increment the start position and search for the next occurrence at the top
        //        // of the loop.
        //        start = index + value.Length;
        //    }

        //    // No instances of the value were found where notValue wasn't found at the same
        //    // position.  Return -1;
        //    return -1;
        //}

        /// <summary>
        /// Reports the zero based index of the first occurence of a ANSI escape code that
        /// isn't one of the special formatting codes that we support (underline, reverse
        /// text, etc.).
        /// </summary>
        /// <remarks>
        /// All non supported ansi sequences should be stripped from the string
        /// before we get to this point. E.g. this makes an assumption that input
        /// is validated before it gets here.
        /// </remarks>
        /// <param name="span">The Span to search.</param>
        /// <param name="value">The string to search for.</param>
        /// <param name="startIndex">The starting position.</param>
        public static int IndexOfNextColorCode(this ReadOnlySpan<char> span, string value, int startIndex)
        {
            // Look up both the starting position of the value and the second value that
            // it isn't supposed to be after those positions.
            int index = span.SafeIndexOf(value, startIndex);

            // Not found at all, return -1.
            if (index == -1)
            {
                return -1;
            }

            int reverseIndex = span.SafeIndexOf(AnsiColors.Reverse, startIndex);
            int underlineIndex = span.SafeIndexOf(AnsiColors.Underline, startIndex);

            // Index isn't -1 and the notIndex was not found, return the index.
            if (index != reverseIndex && index != underlineIndex)
            {
                return index;
            }

            // index and notIndex are equal, search for the next location where index exists
            // but notIndex does not.
            int start = index;

            while ((span.SafeIndexOf(value, start)) >= 0)
            {
                index = span.SafeIndexOf(value, start);

                // Not found at all, return -1.
                if (index == -1)
                {
                    return -1;
                }

                reverseIndex = span.SafeIndexOf(AnsiColors.Reverse, start);
                underlineIndex = span.SafeIndexOf(AnsiColors.Underline, start);

                // Index isn't -1 and the notIndex was not found, return the index.
                if (index != reverseIndex && index != underlineIndex)
                {
                    return index;
                }

                // Increment the start position and search for the next occurrence at the top
                // of the loop.
                start = index + value.Length;
            }

            // No instances of the value were found where notValue wasn't found at the same
            // position.  Return -1;
            return -1;
        }

    }

}
