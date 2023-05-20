/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Colors;
using Avalon.Common.Colors;
using Avalon.Common.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Avalon.Extensions
{
    /// <summary>
    /// Extension methods for internal types.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns visibility to visible or collapsed (does not reserves space) if not visible.
        /// </summary>
        /// <param name="value"></param>
        public static Visibility ToVisibleOrCollapse(this bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Returns visibility to visible or hidden (reserves space) if not visible.
        /// </summary>
        /// <param name="value"></param>
        public static Visibility ToVisibleOrHidden(this bool value)
        {
            return value ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Whether or not the current Window is being shown as a modal.
        /// </summary>
        /// <param name="window"></param>
        public static bool IsModal(this Window window)
        {
            return (bool)typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(window);
        }

        /// <summary>
        /// Converts the specified string into a Line for rendering.  ANSI codes are removed from the value
        /// that is put into the Text property.
        /// </summary>
        /// <param name="text"></param>
        public static Line ToLine(this string text)
        {
            return new Line
            {
                Text = Colorizer.RemoveAllAnsiCodes(text),
                FormattedText = text,
            };
        }

        /// <summary>
        /// Reports the zero based index of the first occurrence of a ANSI escape code that
        /// isn't one of the special formatting codes that we support (underline, reverse
        /// text, etc.).
        /// </summary>
        /// <remarks>
        /// All non supported ansi sequences should be stripped from the string
        /// before we get to this point. E.g. this makes an assumption that input
        /// is validated before it gets here.
        /// </remarks>
        /// <param name="span">The Span to search.</param>
        /// <param name="startIndex">The starting position.</param>
        public static int IndexOfNextColorCode(this ReadOnlySpan<char> span, int startIndex)
        {
            // Look up both the starting position of the value and the second value that
            // it isn't supposed to be after those positions.
            int index = span.SafeIndexOf('\x1B', startIndex);

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

            while ((span.SafeIndexOf('\x1B', start)) >= 0)
            {
                index = span.SafeIndexOf('\x1B', start);

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
                start = index + 1;
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
            return textRange.Text ?? "";
        }

        /// <summary>
        /// Sets the text in a RichTextBox.
        /// </summary>
        /// <param name="rtb"></param>
        /// <param name="text"></param>
        public static void SetText(this RichTextBox rtb, string text)
        {
            rtb.Document.Blocks.Clear();
            rtb.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        /// <summary>
        /// Gets the text from a RichTextBox.
        /// </summary>
        /// <param name="rtb"></param>
        public static string GetText(this RichTextBox rtb)
        {
            return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
        }

        /// <summary>
        /// Clears all properties/formatting in the RichTextBox.
        /// </summary>
        /// <param name="rtb"></param>
        public static void ClearAllProperties(this RichTextBox rtb)
        {           
            var tr = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);            
            tr.ClearAllProperties();
        }

        /// <summary>
        /// Clears only the backgrounds.
        /// </summary>
        /// <param name="rtb"></param>
        public static void ClearAllBackgrounds(this RichTextBox rtb)
        {
            var tr = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            tr.ApplyPropertyValue(TextElement.BackgroundProperty, null);
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
            tr?.ApplyPropertyValue(TextElement.BackgroundProperty, color);
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
                    int indexInRun = textRun.IndexOf(word, startingPosition, StringComparison.Ordinal);

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

        /// <summary>
        /// Removes all ANSI codes from the specified string.
        /// </summary>
        /// <param name="buf"></param>
        public static string RemoveAnsiCodes(this string buf)
        {
            return Colorizer.RemoveAllAnsiCodes(buf);
        }

        /// <summary>
        /// Pulses the <see cref="Control.Background"/> property a specified color.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="color"></param>
        /// <param name="durationMilliseconds"></param>
        public static async Task Pulse(this Control c, Color color, int durationMilliseconds)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                if (c?.Dispatcher == null)
                {
                    return;
                }

                await c.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // The color animation of the pulse
                    var ca = new ColorAnimation
                    {
                        Duration = new Duration(TimeSpan.FromMilliseconds(durationMilliseconds)),
                        To = color,
                        AutoReverse = true,
                        FillBehavior = FillBehavior.Stop
                    };

                    c.Background = new SolidColorBrush(System.Windows.Media.Colors.White);
                    c.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

                    ca.Completed += delegate
                    {
                        tcs.SetResult(true);
                    };
                }));
            }
            catch
            {
                // Task was canceled
                if (c != null)
                {
                    c.Background = Brushes.White;
                }

                return;
            }

            await tcs.Task;
        }

        /// <summary>
        /// Adds words from an input into a HashSet if they do not already exist.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="input"></param>
        public static void AddWords(this List<string> list, string input)
        {
            var matches = Regex.Matches(input, @"\b[\w']*\b");
            var words = from m in matches
                        group m by m.Value into x
                        where !string.IsNullOrEmpty(x.Key) && x.Key.Length > 2
                        select x.Key;

            foreach (string word in words)
            {
                list.Remove(word);
                list.Add(word);
            }
        }

        /// <summary>
        /// Selects the word that the cursor is on.
        /// </summary>
        /// <param name="tb"></param>
        public static void SelectCurrentWord(this TextBox tb)
        {
            int currentPos = tb.SelectionStart;
            int startPos = 0;
            int endPos = 0;

            for (int i = currentPos - 1; i > 0; i--)
            {
                if (tb.Text[i] == ' ')
                {
                    startPos = i + 1;
                    break;
                }
            }

            endPos = currentPos;

            for (int i = currentPos; i <= tb.Text.Length; i++)
            {
                if (i >= tb.Text.Length || tb.Text[i] == ' ')
                {
                    endPos = i;
                    break;
                }
            }
            
            tb.SelectionStart = startPos;
            tb.SelectionLength = endPos - startPos;
        }

        /// <summary>
        /// Pick off one argument from a string and return a tuple
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Tuple where Item1 is the first word and Item2 is the remainder</returns>
        /// <remarks>Was formerly known as one_argument</remarks>
        public static Tuple<string, string> FirstArgument(this string value)
        {
            return new Tuple<string, string>(value.FirstWord(), value.RemoveWord(1));
        }
    }
}