using Avalon.Common.Colors;
using System;
using Avalon.Colors;
using Avalon.Common.Models;
using Argus.Extensions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Media;

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
        public static int IndexOfNextColorCode(this ReadOnlySpan<char> span, ReadOnlySpan<char> value, int startIndex)
        {
            // Look up both the starting position of the value and the second value that
            // it isn't supposed to be after those positions.
            int index = span.SafeIndexOf(value, startIndex);

            // Not found at all, return -1.
            if (index == -1)
            {
                return -1;
            }

            int reverseIndex = span.SafeIndexOf(AnsiColors.Reverse.AnsiCode.AsSpan(), startIndex);
            int underlineIndex = span.SafeIndexOf(AnsiColors.Underline.AnsiCode.AsSpan(), startIndex);

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

                reverseIndex = span.SafeIndexOf(AnsiColors.Reverse.AnsiCode.AsSpan(), start);
                underlineIndex = span.SafeIndexOf(AnsiColors.Underline.AnsiCode.AsSpan(), start);

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

        /// <summary>
        /// Returns the Text from the RichTextBox.
        /// </summary>
        /// <param name="rtb"></param>
        public static string Text(this RichTextBox rtb)
        {
            var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            return textRange?.Text ?? "";
        }

        /// <summary>
        /// Clears all properties/formatting in the RichTextBox.
        /// </summary>
        /// <param name="rtb"></param>
        public static void ClearAllProperties(this RichTextBox rtb)
        {           
            var tr = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);            
            tr?.ClearAllProperties();
        }

        /// <summary>
        /// Clears only the backgrounds.
        /// </summary>
        /// <param name="rtb"></param>
        public static void ClearAllBackgrounds(this RichTextBox rtb)
        {
            var tr = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            tr?.ApplyPropertyValue(TextElement.BackgroundProperty, null);
        }

        /// <summary>
        /// This method highlights a word with a given color in a RichTextBox.  This clears all
        /// formatting
        /// </summary>
        /// <param name="rtb">RichTextBox Control</param>
        /// <param name="word">The word which you need to highlighted</param>
        /// <param name="color">The color with which you highlight</param>
        public static void HighlightWord(this RichTextBox rtb, string word, SolidColorBrush color)
        {
            // Current word at the pointer
            rtb.ClearAllBackgrounds();

            var tr = rtb.Document.ContentStart.FindText(word);

            if (tr == null)
            {
                return;
            }

            tr.ApplyPropertyValue(TextElement.BackgroundProperty, color);
        }

        /// <summary>
        /// Finds the first TextRange with the matching text starting at the position provided.
        /// </summary>
        /// <param name="rtb"></param>
        /// <param name="word"></param>
        /// <param name="startingPosition"></param>
        public static TextRange FindText(this RichTextBox rtb, string word, int startingPosition = 0)
        {
            return FindText(rtb.Document.ContentStart, word);
        }

        /// <summary>
        /// Finds the first TextRange with the matching text starting at the position provided.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="word"></param>
        /// <param name="startingPosition"></param>
        public static TextRange FindText(this TextPointer position, string word, int startingPosition = 0)
        {
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(word, startingPosition);

                    if (indexInRun >= 0)
                    {
                        var start = position.GetPositionAtOffset(indexInRun);
                        var end = start.GetPositionAtOffset(word.Length);
                        return new TextRange(start, end);
                    }
                }

                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            // Position will be null if "word" is not found.
            return null;
        }

        /// <summary>
        /// Clears the selection in the RichTextBox leaving the cursor position where it was.
        /// </summary>
        /// <param name="rtb"></param>
        public static void SelectClear(this RichTextBox rtb)
        {
            rtb.Selection.Select(rtb.CaretPosition, rtb.CaretPosition);
        }

        /// <summary>
        /// Clears the selection in the RichTextBox leaving the cursor position where it was.
        /// </summary>
        /// <param name="rtb"></param>
        /// <param name="text"></param>
        /// <param name="startIndex"></param>
        /// <param name="setFocus"></param>
        public static int SelectFind(this RichTextBox rtb, string text, int startIndex = 0, bool setFocus = false)
        {
            var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);

            // Clear previous selection if there was one.
            rtb.Selection.Select(textRange.Start, textRange.Start);
            
            textRange.ClearAllProperties();
            
            var index = textRange.Text.IndexOf(text, startIndex, StringComparison.OrdinalIgnoreCase);
            if (index > -1)
            {
                var textPointerStart = textRange.Start.GetPositionAtOffset(index);
                var textPointerEnd = textRange.Start.GetPositionAtOffset(index + text.Length);

                var textRangeSelection = new TextRange(textPointerStart, textPointerEnd);
                textRangeSelection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                rtb.Selection.Select(textRangeSelection.Start, textRangeSelection.End);

                if (setFocus)
                {
                    rtb.Focus();
                }
            }

            return index;
        }

    }

}
