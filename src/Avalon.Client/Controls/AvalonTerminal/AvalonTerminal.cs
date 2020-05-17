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
        /// Regular expression to find the last ANSI color code used.
        /// </summary>
        private readonly Regex _lastColorRegEx = new Regex(@"\x1B\[[^@-~]*[@-~]", RegexOptions.RightToLeft | RegexOptions.Compiled);

        private readonly GagElementGenerator _gagElementGenerator;

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
            this.TextArea.TextView.LineTransformers.Add(new AnsiColorizer());

            _gagElementGenerator = new GagElementGenerator();
            this.TextArea.TextView.ElementGenerators.Add(_gagElementGenerator);

            // For custom key handling like trapping copy and paste.
            this.PreviewKeyDown += OnPreviewKeyDown;
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
                // optimizations if we can mark the line as having no color (and then the colorizer can simply
                // skip that line.
                if (!string.IsNullOrWhiteSpace(_lastColorCode) && _lastColorCode != AnsiColors.Clear)
                {
                    line.FormattedText = line.FormattedText.Insert(0, _lastColorCode);
                }

                // Save the last color code if it's a new line, that way we can continue the color on the
                // next line.. however, if it was only a partial line clear the color code because that
                // line will already have the last color code on it.
                if (line.FormattedText.EndsWith('\n'))
                {
                    _lastColorCode = _lastColorRegEx.Match(line.FormattedText).Value;
                }
                else
                {
                    _lastColorCode = "";
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
        /// Similar functionality to ScrollToEnd but considerably more efficient.  This scrolls with ScrollTo.
        /// ScrollToLastLine with the vertical offset overload is even more efficient but will not scroll all
        /// the way down in cases where there is word wrapping occuring.
        /// </summary>
        public void ScrollToLastLine()
        {
            //this.ScrollToVerticalOffset(this.TextArea.TextView.GetVisualTopByDocumentLine(this.LineCount));
            this.ScrollTo(this.LineCount, 0);
        }

        /// <summary>
        /// Scrolls to the end of the terminal by the vertical offset.  This is the most efficient way to scroll
        /// but sometimes does not scroll all the way to the bottom when wrapping has occured.
        /// </summary>
        public void ScrollToLastLine(bool useVerticalOffset = false)
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
            _gagElementGenerator.UncollapseAfter(lineNumber);
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
            _gagElementGenerator.UncollapseAfter(startLineNumber);

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
            _gagElementGenerator.UncollapseAll();
            this.Text = "";
        }

        /// <summary>
        /// Goes through the terminal backwards looking for a single instance to replace.
        /// </summary>
        /// <param name="searchFor"></param>
        /// <param name="replaceWith"></param>
        public void ReplaceLastInstance(string searchFor, string replaceWith)
        {
            int start = this.Document.LastIndexOf(searchFor, 0, this.Document.TextLength, StringComparison.Ordinal);

            if (start == -1)
            {
                return;
            }

            // If they start removing lines we have uncollapse and let things recollapse otherwise
            // their will eventually be a crash because the collapsed lines are out of sync.
            if (searchFor.Contains('\n'))
            {
                _gagElementGenerator.UncollapseAll();
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
                    _gagElementGenerator.UncollapseAll();
                }

                this.Document.Replace(index, searchFor.Length, replaceWith);
                //this.Select(index, replacement.Length);
            }
        }

        /// <summary>
        /// Replaces all instancese of a string with another.
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
                    _gagElementGenerator.UncollapseAll();
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
        /// Finds the next occurance of a string
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
        /// <returns></returns>
        public string GetText(int offset, int length)
        {
            return this.Document.GetText(offset, length);
        }

        /// <summary>
        /// Returns metadata about the current state of a line.  Note that if this is held
        /// onto that it is not updated (it is a snapshot in time).
        /// </summary>
        /// <param name="lineNumber"></param>
        /// <returns></returns>
        public LineData LineData(int lineNumber)
        {
            var line = this.Document.GetLineByNumber(lineNumber);
            
            var lineData = new LineData
            {
                LineNumber = lineNumber,
                Text = this.Document.GetText(line.Offset, line.Length),
                IsGagged = _gagElementGenerator.CollapsedLineSections.ContainsKey(lineNumber),
                Offset = line.Offset,
                EndOffset = line.EndOffset,
                IsDeleted = line.IsDeleted
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
        /// Whether or not the gags are enabled on this instance of the terminal.
        /// </summary>
        public bool GagEnabled
        {
            get => _gagElementGenerator.Enabled;
            set => _gagElementGenerator.Enabled = value;
        }

    }
}
