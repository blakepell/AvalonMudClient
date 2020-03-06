using Avalon.Common.Colors;
using Avalon.Extensions;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Windows;
using System.Windows.Media;
using Avalon.Colors;

namespace Avalon.Controls
{
    /// <summary>
    /// ANSI color support for the AvalonTerminal.
    /// </summary>
    /// <remarks>
    /// TODO - Background colors: http://pueblo.sourceforge.net/doc/manual/ansi_color_codes.html
    /// </remarks>
    public class AnsiColorizer : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(DocumentLine line)
        {
            // Instead of allocating a string here we're going to use a Span and work from it.
            var text = CurrentContext.Document.GetText(line).AsSpan();

            foreach (var color in Colorizer.ColorMap)
            {
                int start = 0;
                int index;

                while ((index = text.IndexOf(color.AnsiColor.AnsiCode.AsSpan(), start)) >= 0)
                {
                    // Find the end of the control sequence
                    int indexEnd = text.IndexOf("m".AsSpan(), index + 1) + 1;

                    // This should look for the index of the next color code EXCEPT when it's a style code.
                    int endMarker = text.IndexOfNextColorCode("\x1B".AsSpan(), index + 1);

                    // If the end marker isn't found on this line then it goes to the end of the line
                    if (endMarker == -1)
                    {
                        endMarker = text.Length;
                    }

                    // All of the text that needs to be colored
                    base.ChangeLinePart(
                        line.Offset + index,    // startOffset
                        line.Offset + endMarker, // endOffset
                        (VisualLineElement element) =>
                        {
                            element.TextRunProperties.SetForegroundBrush(color.Brush);
                        });

                    // Search for next occurrence, again, we'll reference the span and not the length.
                    start = index + color.AnsiColor.AnsiCode.AsSpan().Length;

                    // Hide the control sequence (the control escape is hidden via the TextArea.TextView.Options.ShowBoxForControlCharacters
                    // property, but we need to hide the rest of the ANSI sequence as well.  I haven't found a way to make these hidden and
                    // collapse their space so I'm setting them to transparent and making them really really small.  Kind of a hack but it works.
                    base.ChangeLinePart(
                        line.Offset + index,    // startOffset
                        line.Offset + indexEnd, // endOffset
                        (VisualLineElement element) =>
                        {
                            element.TextRunProperties.SetForegroundBrush(Brushes.Transparent);
                            element.TextRunProperties.SetFontRenderingEmSize(.00000001);
                        });

                }
            }

            // Styles that should be applied after the colors are applied.  Styles may reverse the text
            // underline it, make it blink, etc.  They key thing about a style is that they work on top
            // of any color codes that might have already been applied.
            foreach (var color in Colorizer.StyleMap)
            {
                int start = 0;
                int index;

                while ((index = text.IndexOf(color.AnsiColor.AnsiCode.AsSpan(), start)) >= 0)
                {
                    // Find the end of the control sequence
                    int indexEnd = index + color.AnsiColor.AnsiCode.AsSpan().Length;

                    // Find the clear color code if it exists.
                    int endMarker = text.IndexOf(AnsiColors.Clear.AnsiCode.AsSpan(), index + 1);

                    // If the end marker isn't found on this line then it goes to the end of the line
                    if (endMarker == -1)
                    {
                        endMarker = text.Length;
                    }

                    // Flip flop the colors
                    base.ChangeLinePart(
                        line.Offset + index,    // startOffset
                        line.Offset + endMarker, // endOffset
                        (VisualLineElement element) =>
                        {
                            if (color.AnsiColor is Reverse)
                            {
                                var foreground = element.TextRunProperties.ForegroundBrush;
                                var background = element.BackgroundBrush ?? Brushes.Black;
                                element.TextRunProperties.SetForegroundBrush(background);
                                element.TextRunProperties.SetBackgroundBrush(foreground);
                            }
                            else if (color.AnsiColor is Underline)
                            {
                                element.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
                            }
                        });

                    // Search for the next occurrence
                    start = index + color.AnsiColor.AnsiCode.AsSpan().Length;

                    // Hide the control sequence (the control escape is hidden via the TextArea.TextView.Options.ShowBoxForControlCharacters
                    // property, but we need to hide the rest of the ANSI sequence as well.  I haven't found a way to make these hidden and
                    // collapse their space so I'm setting them to transparent and making them really really small.  Kind of a hack but it works.
                    base.ChangeLinePart(
                        line.Offset + index,    // startOffset
                        line.Offset + indexEnd, // endOffset
                        (VisualLineElement element) =>
                        {
                            element.TextRunProperties.SetForegroundBrush(Brushes.Transparent);
                            element.TextRunProperties.SetFontRenderingEmSize(.00000001);
                        });

                }
            }
        }
    }
}