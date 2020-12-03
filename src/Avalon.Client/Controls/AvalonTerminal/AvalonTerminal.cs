using System.Text;
using Avalon.Common.Colors;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Avalon.Colors;
using Avalon.Common.Models;
using System.Windows.Controls;
using System;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Utils;
using System.Linq;

namespace Avalon.Controls
{
    /// <summary>
    /// Text editor based off of AvalonEdit which is setup to render a limited number of ANSI
    /// control sequences.
    /// </summary>
    /// <remarks>
    /// TODO - Copy override: https://stackoverflow.com/questions/47541080/capture-modify-paste-event-in-avalonedit
    /// https://web.archive.org/web/20190316163741/http://community.sharpdevelop.net/forums/t/15881.aspx
    /// https://web.archive.org/web/20160410073639/http://community.sharpdevelop.net/forums/p/13903/37261.aspx#37261
    /// (Search Panel) myEditor.TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(myEditor.TextArea));
    /// SearchPanel.Install(XTBAvalonEditor);
    /// </remarks>
    public class AvalonTerminal : ICSharpCode.AvalonEdit.TextEditor
    {
        /// <summary>
        /// Whether or not the terminal will attempt to AutoScroll if the line inserted
        /// requests for auto scroll.  This being set to false negates all auto scrolling
        /// even if requested by the line.
        /// </summary>
        public bool IsAutoScrollEnabled { get; set; } = true;

        /// <summary>
        /// The last ANSI color code that was used so that it can start the next set of text
        /// where color isn't ignored.
        /// </summary>
        private string _lastColorCode = "";

        /// <summary>
        /// Whether the last color code was set and should be used.
        /// </summary>
        /// <remarks>
        /// We previously set the last color code to a blank string and used that as an indicator however
        /// if we use a bool we can avoid a string allocation per line that's rendered which adds up.
        /// </remarks>
        private bool _useLastColorCode = false;

        /// <summary>
        /// Regular expression to find the last ANSI color code used.
        /// </summary>
        private readonly Regex _lastColorRegEx = new Regex(@"\x1B\[[^@-~]*[@-~]", RegexOptions.RightToLeft | RegexOptions.Compiled);

        /// <summary>
        /// Gag element generator.
        /// </summary>
        public GagElementGenerator Gag { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AvalonTerminal() : base()
        {
            // Disable the undo stack, we don't need it.
            this.Document.UndoStack.SizeLimit = 0;

            // Set our custom properties
            this.TextArea.TextView.Options.EnableEmailHyperlinks = false;
            this.TextArea.TextView.Options.EnableHyperlinks = false;
            this.TextArea.TextView.Options.ShowBoxForControlCharacters = false;
            this.TextArea.Caret.CaretBrush = Brushes.Transparent;

            // Setup our custom line and element transformers.
            this.TextArea.TextView.ElementGenerators.Add(new HideAnsiElementGenerator(this) { ParseExtended = false });
            this.TextArea.TextView.LineTransformers.Add(new AnsiColorizer());

            this.Gag = new GagElementGenerator();
            this.TextArea.TextView.ElementGenerators.Add(this.Gag);

            // For custom key handling like trapping copy and paste.
            this.PreviewKeyDown += OnPreviewKeyDown;

            // To handle code for when the size of the control changes (namingly, the width).
            this.SizeChanged += this.AvalonTerminal_SizeChanged;

            // Find out if we're wrapped.
            this.TextArea.TextView.VisualLineConstructionStarting += this.TextView_VisualLineConstructionStarting;            
        }

        /// <summary>
        /// Code to execute when the visual lines begin construction (I think this happens just before).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextView_VisualLineConstructionStarting(object sender, ICSharpCode.AvalonEdit.Rendering.VisualLineConstructionStartEventArgs e)
        {
            if (this.TextArea.TextView.VisualLinesValid && this.TextArea.TextView.VisualLines.Any())
            {
                foreach (var item in this.TextArea.TextView.VisualLines)
                {
                    // If it's more than one line AND it's not gagged (very important).
                    if (item.TextLines.Count > 1 && !this.Gag.CollapsedLineSections.ContainsKey(item.FirstDocumentLine.LineNumber))
                    {
                        this.HasVisibleWrappedLines = true;
                        return;
                    }
                }
            }
            else
            {
                return;
            }

            this.HasVisibleWrappedLines = false;
        }

        /// <summary>
        /// Code that needs to execute when the size of this terminal changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvalonTerminal_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Since width is what affects word wrapping we're only concerned with it in terms
            // of telling the terminal gags they need to recalculate.  We have to tell the calling
            // control to uncollapse all the lines.
            if (e.WidthChanged)
            {
                this.Gag.UncollapseAll();
                this.TextArea.TextView.Redraw();
            }
        }

        /// <summary>
        /// Used to trap key combinations (like Control + C) to execute custom code on actions like
        /// when text is copied.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Override copy so that we can strip out the ANSI color codes if they exist.
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.C)
                {
                    // Remove any ANSI codes from the selected text.
                    var sb = new StringBuilder(this.SelectedText);
                    Colorizer.RemoveAllAnsiCodes(sb);
                    Clipboard.SetText(sb.ToString());

                    // Set the handled to true
                    e.Handled = true;

                    return;
                }
            }

            e.Handled = false;
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="text"></param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(string text)
        {
            var line = new Line
            {
                FormattedText = text,
                IgnoreLastColor = false,
                ForegroundColor = AnsiColors.Default
            };

            Append(line);
        }


        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="sb"></param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(StringBuilder sb)
        {
            var line = new Line
            {
                FormattedText = sb.ToString(),
                IgnoreLastColor = false,
                ForegroundColor = AnsiColors.Default
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="text">The text to append to the terminal.</param>
        /// <param name="scrollToLastLine">Whether or not the line should cause the terminal to scroll to the last line.</param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(string text, bool scrollToLastLine)
        {
            var line = new Line
            {
                FormattedText = text,
                IgnoreLastColor = false,
                ForegroundColor = AnsiColors.Default,
                ScrollToLastLine = scrollToLastLine
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="sb">The text to append to the terminal.</param>
        /// <param name="scrollToLastLine">Whether or not the line should cause the terminal to scroll to the last line.</param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(StringBuilder sb, bool scrollToLastLine)
        {
            var line = new Line
            {
                FormattedText = sb.ToString(),
                IgnoreLastColor = false,
                ForegroundColor = AnsiColors.Default,
                ScrollToLastLine = scrollToLastLine
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(string text, AnsiColor foregroundColor)
        {
            var line = new Line
            {
                FormattedText = text,
                IgnoreLastColor = true,
                ForegroundColor = foregroundColor
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="foregroundColor"></param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(StringBuilder sb, AnsiColor foregroundColor)
        {
            var line = new Line
            {
                FormattedText = sb.ToString(),
                IgnoreLastColor = true,
                ForegroundColor = foregroundColor
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="reverseColors"></param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(string text, AnsiColor foregroundColor, bool reverseColors)
        {
            var line = new Line
            {
                FormattedText = text,
                IgnoreLastColor = true,
                ForegroundColor = foregroundColor,
                ReverseColors = reverseColors
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="reverseColors"></param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(StringBuilder sb, AnsiColor foregroundColor, bool reverseColors)
        {
            var line = new Line
            {
                FormattedText = sb.ToString(),
                IgnoreLastColor = true,
                ForegroundColor = foregroundColor,
                ReverseColors = reverseColors
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="reverseColors"></param>
        /// <param name="scrollToLastLine">Whether or not the line should cause the terminal to scroll to the last line.</param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(string text, AnsiColor foregroundColor, bool reverseColors, bool scrollToLastLine)
        {
            var line = new Line
            {
                FormattedText = text,
                IgnoreLastColor = true,
                ForegroundColor = foregroundColor,
                ReverseColors = reverseColors,
                ScrollToLastLine = scrollToLastLine
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="reverseColors"></param>
        /// <param name="scrollToLastLine">Whether or not the line should cause the terminal to scroll to the last line.</param>
        /// <remarks>
        /// As the Text property isn't used in this context we are specificity not allocating an
        /// entry for it.  In the past we used to save a collection of the lines received but we
        /// are no longer doing that for now.
        /// </remarks>
        public void Append(StringBuilder sb, AnsiColor foregroundColor, bool reverseColors, bool scrollToLastLine)
        {
            var line = new Line
            {
                FormattedText = sb.ToString(),
                IgnoreLastColor = true,
                ForegroundColor = foregroundColor,
                ReverseColors = reverseColors,
                ScrollToLastLine = scrollToLastLine
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line to the line list and also appends it to the terminal, additionally will convert
        /// ANSI codes into the mud color codes the EscapeSequenceElementGenerator class uses.
        /// </summary>
        /// <param name="line"></param>
        public void Append(Line line)
        {
            if (!line.IgnoreLastColor)
            {
                // We'll insert the last color code UNLESS it's the clear color code.  No point in putting one
                // of those at the start of every line.  Having a line with no color might provide us some
                // optimizations if we can mark the line as having no color (and then the AnsiColorizer can simply
                // skip that line.
                if (_useLastColorCode && _lastColorCode != AnsiColors.Clear)
                {
                    line.FormattedText = line.FormattedText.Insert(0, _lastColorCode);
                }

                // Save the last color code if it's a new line, that way we can continue the color on the
                // next line.. however, if it was only a partial line clear the color code because that
                // line will already have the last color code on it.  Instead of setting the last color
                // to a blank if it's not a new line we'll use the _useLastColorCode bool as to not
                // allocate a lot of unneeded strings.
                if (line.FormattedText.EndsWith('\n'))
                {
                    _lastColorCode = _lastColorRegEx.Match(line.FormattedText).Value;
                    _useLastColorCode = true;
                }
                else
                {
                    _useLastColorCode = false;
                }
            }
            else
            {
                line.FormattedText = line.FormattedText.Insert(0, line.ForegroundColor);

                if (line.ReverseColors)
                {
                    line.FormattedText = line.FormattedText.Insert(0, AnsiColors.Reverse);
                }
            }

            this.AppendText(line.FormattedText);

            // Check to see if we should scroll to the last line after appending.  This will be true for probably
            // all terminal windows except the back buffer.  The AutoScroll property can override this behavior in
            // terms of being able to turn it off.
            if (this.IsAutoScrollEnabled && line.ScrollToLastLine)
            {                
                this.ScrollToLastLine();
            }
        }

        /// <summary>
        /// Appends ANSI color coded text to the terminal (via the mud color codes).
        /// </summary>
        /// <param name="text"></param>
        public void AppendAnsi(string text)
        {
            var line = new Line
            {
                IgnoreLastColor = false,
                ForegroundColor = AnsiColors.Default
            };

            var sb = Argus.Memory.StringBuilderPool.Take(text);
            Colorizer.MudToAnsiColorCodes(sb);
            line.FormattedText = sb.ToString();
            Argus.Memory.StringBuilderPool.Return(sb);

            Append(line);
        }

        /// <summary>
        /// Appends ANSI color coded text to the terminal (via the mud color codes).
        /// </summary>
        /// <param name="sb"></param>
        public void AppendAnsi(StringBuilder sb)
        {
            var line = new Line
            {
                IgnoreLastColor = false,
                ForegroundColor = AnsiColors.Default
            };

            Colorizer.MudToAnsiColorCodes(sb);
            line.FormattedText = sb.ToString();

            Append(line);
        }

        /// <summary>
        /// Uncollapses all of the gagged lines and redraws the visual lines on the screen.  This
        /// might be required when certain events are triggered (word wrap changing, width changing
        /// or properties on the this control).
        /// </summary>
        public void RedrawLines()
        {
            this.Gag.UncollapseAll();
            this.TextArea.TextView.Redraw();
        }

        /// <summary>
        /// Scrolls to the last line.  If there are any wrapped lines on the screen this will do a less efficient
        /// but certain scroll to the bottom, if there are no wrapped lines it will use the more efficient
        /// ScrollToVerticalOffset (that only works right when there aren't wrapped lines.).  The determination
        /// on whether something is wrapped happens at the time text is added to the document (which is fine
        /// for us because we're concerned with what will be at the bottom of the terminal).
        /// </summary>
        public void ScrollToLastLine()
        {
            if (this.HasVisibleWrappedLines)
            {
                this.ScrollTo(this.LineCount, 0);
            }
            else
            {
                this.ScrollToVerticalOffset(this.TextArea.TextView.GetVisualTopByDocumentLine(this.LineCount));
            }
        }

        /// <summary>
        /// Scrolls to the end of the terminal by the vertical offset.  This is the most efficient way to scroll
        /// but sometimes does not scroll all the way to the bottom when wrapping has occurred.
        /// </summary>
        /// <param name="useVerticalOffset">Use the vertical offset is much faster but doesn't always work when there are lines that are wrapped.</param>
        public void ScrollToLastLine(bool useVerticalOffset)
        {
            if (useVerticalOffset)
            {
                this.ScrollToVerticalOffset(this.TextArea.TextView.GetVisualTopByDocumentLine(this.LineCount));
            }
            else
            {
                this.ScrollTo(this.LineCount, 0);
            }
        }

        /// <summary>
        /// Scrolls to the specified index in the document if that index exists.
        /// </summary>
        /// <param name="index"></param>
        public void ScrollToIndex(int index)
        {
            if (index >=0 && index < this.Document.TextLength)
            {
                this.ScrollToLine(this.Document.GetLocation(index).Line);
            }
        }

        /// <summary>
        /// Whether or not the last line is visible on the screen.
        /// </summary>
        public bool IsLastLineVisible()
        {
            var lastLine = this.TextArea.TextView.GetVisualLine(this.LineCount);

            if (lastLine == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Whether or not the first line is visible on the screen.
        /// </summary>
        public bool IsFirstLineVisible()
        {
            var firstLine = this.TextArea.TextView.GetVisualLine(0);

            if (firstLine == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Whether or not the requested line number is visible.
        /// </summary>
        public bool IsLineVisible(int lineNumber)
        {
            var line = this.TextArea.TextView.GetVisualLine(lineNumber);

            if (line == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Removes the specified line number if it exists.
        /// </summary>
        /// <param name="lineNumber"></param>
        public void RemoveLine(int lineNumber)
        {
            if (lineNumber < 1 || lineNumber > this.LineCount)
            {
                return;
            }
            
            var line = this.Document.GetLineByNumber(lineNumber);

            if (line == null)
            {
                return;
            }

            // Remove the line from the collapsed line section if it exists (before deleting it).
            this.Gag.UncollapseAfter(lineNumber);
            this.Document.Remove(line.Offset, line.TotalLength);
        }

        /// <summary>
        /// Removes lines between the start and end line number.
        /// </summary>
        /// <param name="startLineNumber">The starting line number with 1 being the lowest.</param>
        /// <param name="endLineNumber">The ending line number.</param>
        /// <remarks>
        /// If the start or ending line number exceeds the bounds of lines in the editor then the
        /// upper or lower bound will be defaulted to for that parameter.
        /// </remarks>
        public void RemoveLine(int startLineNumber, int endLineNumber)
        {
            int length = 0;
            int startOffset = 0;

            if (startLineNumber < 1)
            {
                startLineNumber = 1;
            }

            if (endLineNumber > this.LineCount)
            {
                endLineNumber = this.LineCount;
            }

            // Uncollapse all lines after the start line number.
            this.Gag.UncollapseAfter(startLineNumber);

            // Find the starting position/offset and the length to remove for the specified
            // set of lines.
            for (int i = startLineNumber; i < endLineNumber; i++)
            {
                var line = this.Document.GetLineByNumber(i);

                if (i == startLineNumber)
                {
                    startOffset = line.Offset;
                }

                length += line.TotalLength;
            }

            this.Document.Remove(startOffset, length);
        }

        /// <summary>
        /// Clears all of the text and uncollapses all of the lines.  Must be used instead of setting the text
        /// or crashes will eventually occur if gagging existed.
        /// </summary>
        public void ClearText()
        {
            this.Gag.UncollapseAll();
            this.Text = "";
        }

        /// <summary>
        /// Goes through the terminal backwards looking for a single instance to replace.
        /// </summary>
        /// <param name="searchFor"></param>
        /// <param name="replaceWith"></param>
        public void ReplaceLastInstance(string searchFor, string replaceWith)
        {
            // First, search for any instance so we know if we should continue with the
            // extra code we need to run.
            int start = this.Document.LastIndexOf(searchFor, 0, this.Document.TextLength, StringComparison.Ordinal);

            if (start == -1)
            {
                return;
            }

            // If they start removing lines we have uncollapse and let things recollapse otherwise
            // their will eventually be a crash because the collapsed lines are out of sync.
            if (searchFor.Contains('\n'))
            {
                this.Gag.UncollapseAll();
            }
            
            this.Document.Replace(start, searchFor.Length, replaceWith);
        }

        /// <summary>
        /// Replaces the first instance of one string with another.
        /// </summary>
        /// <param name="searchFor">The string to search for.</param>
        /// <param name="replaceWith">The string to replace the search string with.</param>
        /// <param name="selectedOnly">Selected only will search only the selected text.</param>
        public void Replace(string searchFor, string replaceWith, bool selectedOnly)
        {
            int index;

            if (selectedOnly)
            {
                index = this.Document.IndexOf(searchFor, this.SelectionStart, this.SelectionLength, StringComparison.Ordinal);
            }
            else
            {
                index = this.Document.IndexOf(searchFor, 0, this.Document.TextLength, StringComparison.Ordinal);
            }

            if (index != -1)
            {
                // If they start removing lines we have uncollapse and let things recollapse otherwise
                // their will eventually be a crash because the collapsed lines are out of sync.
                if (searchFor.Contains('\n'))
                {
                    this.Gag.UncollapseAll();
                }

                this.Document.Replace(index, searchFor.Length, replaceWith);
                //this.Select(index, replacement.Length);
            }
        }

        /// <summary>
        /// Replaces all instances of a string with another.
        /// </summary>
        /// <param name="searchFor">The string to search for.</param>
        /// <param name="replacement">The string to replace the search string with.</param>
        /// <param name="selectedOnly">Selected only will search only the selected text.</param>
        public void ReplaceAll(string searchFor, string replacement, bool selectedOnly)
        {
            int index;

            if (selectedOnly)
            {
                index = this.Document.IndexOf(searchFor, this.SelectionStart, this.SelectionLength, StringComparison.Ordinal);
            }
            else
            {
                index = this.Document.IndexOf(searchFor, 0, this.Document.TextLength, StringComparison.Ordinal);
            }

            // Uncollapse only once before the looping replacements.
            if (index > -1)
            {
                // If they start removing lines we have uncollapse and let things recollapse otherwise
                // their will eventually be a crash because the collapsed lines are out of sync.
                if (searchFor.Contains('\n'))
                {
                    this.Gag.UncollapseAll();
                }
            }

            while (index > -1)
            {
                if (selectedOnly)
                {
                    index = this.Document.IndexOf(searchFor, this.SelectionStart, this.SelectionLength, StringComparison.Ordinal);
                }
                else
                {
                    index = this.Document.IndexOf(searchFor, 0, this.Document.TextLength, StringComparison.Ordinal);
                }

                if (index != -1)
                {
                    this.Document.Replace(index, searchFor.Length, replacement);
                }
            }
        }

        /// <summary>
        /// Used to track the last find index.
        /// </summary>
        private int _lastFindIndex = 0;

        /// <summary>
        /// Finds the next occurrence of a string
        /// </summary>
        /// <param name="searchFor"></param>
        /// <param name="reset">Whether or not to reset the starting index.</param>
        public void Find(string searchFor, bool reset = false)
        {
            if (reset)
            {
                _lastFindIndex = 0;
            }
            
            if (string.IsNullOrEmpty(searchFor) || this.Document.TextLength == 0)
            {
                _lastFindIndex = 0;
                return;
            }

            int index = this.Document.Text.IndexOf(searchFor, _lastFindIndex);

            if (index != -1)
            {                
                // Select the text that we found
                this.Select(index, searchFor.Length);

                // Set the last found so we can start from there next time.
                _lastFindIndex = index + searchFor.Length;

                // Scroll the line where that exists into view.
                this.ScrollToLine(this.Document.GetLocation(index).Line);
            }
            else
            {
                _lastFindIndex = 0;
            }
        }

        /// <summary>
        /// Returns the text of the requested line.
        /// </summary>
        /// <param name="lineNumber"></param>
        public string GetText(int lineNumber)
        {                        
            var line = this.Document.GetLineByNumber(lineNumber);            
            return this.Document.GetText(line.Offset, line.Length);
        }

        /// <summary>
        /// Gets the text at the requested offset for the requested length.  Shortcut for
        /// Document.GetText.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public string GetText(int offset, int length)
        {
            return this.Document.GetText(offset, length);
        }

        /// <summary>
        /// Returns metadata about the current state of a line.  Note that if this is held
        /// onto that it is not updated (it is a snapshot in time).
        /// </summary>
        /// <param name="lineNumber"></param>
        public LineData LineData(int lineNumber)
        {
            var line = this.Document.GetLineByNumber(lineNumber);
            int linesWithWrap = this.TextArea.TextView.VisualLines.FirstOrDefault(x => x.FirstDocumentLine.LineNumber == lineNumber)?.TextLines.Count ?? 1;

            var lineData = new LineData
            {
                LineNumber = lineNumber,
                Text = this.Document.GetText(line.Offset, line.Length),
                IsGagged = this.Gag.CollapsedLineSections.ContainsKey(lineNumber),
                Offset = line.Offset,
                EndOffset = line.EndOffset,
                IsDeleted = line.IsDeleted,
                LinesWithWrap = linesWithWrap
            };
            
            lineData.IsEmptyOrWhitespace = string.IsNullOrWhiteSpace(lineData.Text);
            
            return lineData;
        }

        /// <summary>
        /// Returns the text of the last not empty line in the document.
        /// </summary>
        public string LastNonEmptyLine
        {
            get
            {                                
                string text = "";
                int i = this.Document.LineCount;

                while (string.IsNullOrEmpty(text) && i > 0)
                {                    
                    var line = this.Document.GetLineByNumber(i);                                        
                    text = this.Document.GetText(line.Offset, line.Length);
                    i--;
                }

                return text;
            }
        }

        /// <summary>
        /// Whether the currently visible lines have any wrapped lines.
        /// </summary>
        public bool HasVisibleWrappedLines { get; set; } = false;

        /// <summary>
        /// Whether or not the gags are enabled on this instance of the terminal.
        /// </summary>
        public bool GagEnabled
        {
            get => this.Gag.Enabled;
            set => this.Gag.Enabled = value;
        }

    }
}
