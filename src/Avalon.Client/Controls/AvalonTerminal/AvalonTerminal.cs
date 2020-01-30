using System.Linq;
using System.Text;
using Avalon.Common.Colors;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Avalon.Extensions;
using Avalon.Common.Models;

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
    /// 
    /// </remarks>
    public class AvalonTerminal : ICSharpCode.AvalonEdit.TextEditor
    {
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
        /// <param name="args"></param>
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
        public void Append(string text)
        {
            var line = new Line
            {
                FormattedText = text,
                Text = Colorizer.RemoveAllAnsiCodes(text),
                IgnoreLastColor = false,
                ForegroundColor = AnsiColors.Default
            };

            Append(line);
        }

        /// <summary>
        /// Adds a line into the Lines list and appends it's content to the buffer.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        public void Append(string text, AnsiColor foregroundColor)
        {
            var line = new Line
            {
                FormattedText = text,
                Text = Colorizer.RemoveAllAnsiCodes(text),
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
        public void Append(string text, AnsiColor foregroundColor, bool reverseColors)
        {
            var line = new Line
            {
                FormattedText = text,
                Text = Colorizer.RemoveAllAnsiCodes(text),
                IgnoreLastColor = true,
                ForegroundColor = foregroundColor,
                ReverseColors = reverseColors
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
            this.ScrollToLastLine();
        }

        /// <summary>
        /// Similar functionality to ScrollToEnd but considerably more efficient.
        /// </summary>
        public void ScrollToLastLine()
        {
            this.ScrollToVerticalOffset(this.TextArea.TextView.GetVisualTopByDocumentLine(this.LineCount));
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
