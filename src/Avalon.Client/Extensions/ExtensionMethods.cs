using Avalon.Common.Colors;
using System;
using Avalon.Colors;
using Avalon.Common.Models;
using Argus.Extensions;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Reflection;

namespace Avalon.Extensions
{
    /// <summary>
    /// Extension methods for internal types.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Sets a property's value via reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void Set<T>(this T @this, string propertyName, object value)
        {
            var t = @this.GetType();
            var prop = t.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            prop.SetValue(@this, value, null);
        }

        /// <summary>
        /// Removes all line endings from a string using a char array for performance vs.
        /// a string replace.
        /// </summary>
        /// <param name="s"></param>
        public static string RemoveLineEndings(this string s)
        {
            int len = s.Length;
            char[] output = new char[len];
            int i2 = 0;

            for (int i = 0; i < len; i++)
            {
                char c = s[i];

                if (c != '\r' && c != '\n')
                {
                    output[i2++] = c;
                }
            }

            return new string(output, 0, i2);
        }

        /// <summary>
        /// Returns visibility to visible or collapsed (does not reserves space) if not visible.
        /// </summary>
        /// <param name="value"></param>
        public static Visibility ToVisibleOrCollapse(this bool value)
        {
            if (value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Returns visibility to visible or hidden (reserves space) if not visible.
        /// </summary>
        /// <param name="value"></param>
        public static Visibility ToVisibleOrHidden(this bool value)
        {
            if (value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
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
        /// <returns></returns>
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
                c.Background = Brushes.White;
                return;
            }

            await tcs.Task;
        }
    }

}