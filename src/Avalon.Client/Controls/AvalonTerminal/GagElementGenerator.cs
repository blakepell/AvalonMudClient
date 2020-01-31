using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Rendering;
using System.Linq;
using System.Text;
using System.Windows.Media.TextFormatting;
using Avalon.Colors;

namespace Avalon.Controls
{
    /// <summary>
    /// Gags text based on whether it meets one of the regex requirements.
    /// </summary>
    /// <remarks>
    /// https://web.archive.org/web/20190316163741/http://community.sharpdevelop.net/forums/t/15881.aspx
    /// </remarks>
    public class GagElementGenerator : VisualLineElementGenerator
    {
        /// <summary>
        /// This might seem like a micro optimization but the StringBuilder used in GetFirstInterestedOffset had a performance
        /// hit allocated that is avoided if we reuse the StringBuilder and simply clear it.  Since this is called A LOT it
        /// while rendering the impact was enough that this made sense.
        /// </summary>
        private readonly StringBuilder _sb = new StringBuilder();

        /// <summary>
        /// Tracks all of the collapsed sections so that we can call UnCollapse() if needed.
        /// </summary>
        public Dictionary<int, CollapsedLineSection> CollapsedLineSections = new Dictionary<int, CollapsedLineSection>();

        private bool _enabled = true;

        /// <summary>
        /// Whether or not the gag is currently enabled.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                // Uncollapse all of the lines if this is false.. do it before you set the _enabled in case
                // other processing is going on.  After the line has had Uncollapse called on it then remove
                // it from the collection that's tracking the collapsed lines, we don't need to track it anymore.
                if (value == false)
                {

                    foreach (int key in CollapsedLineSections.Keys)
                    {
                        CollapsedLineSections[key].Uncollapse();
                        CollapsedLineSections.Remove(key);
                    }
                }

                _enabled = value;
            }
        }

        public override int GetFirstInterestedOffset(int startOffset)
        {
            // Don't process if the AppSettings are null or triggers are disabled.
            if (App.Settings.ProfileSettings.TriggersEnabled == false || this.Enabled == false)
            {
                return -1;
            }

            var endLine = CurrentContext.VisualLine.LastDocumentLine;
            var segment = CurrentContext.GetText(startOffset, endLine.EndOffset - startOffset);
            
            // Clear the StringBuilder and re-remove the ANSI codes so the match can run again.
            _sb.Clear();
            _sb.Append(segment.Text);
            Colorizer.RemoveAllAnsiCodes(_sb);

            // TODO - Performance (only Uncollapse() when needed)... perhaps put this in else below but the add would need a check then.
            // If the triggers change, it has to have Uncollapse() called on it.
            if (endLine?.NextLine != null && CollapsedLineSections.ContainsKey(endLine.NextLine.LineNumber))
            {
                CollapsedLineSections[endLine.NextLine.LineNumber].Uncollapse();
                CollapsedLineSections.Remove(endLine.NextLine.LineNumber);
            }

            // TODO - Performance Will running this linq query every time get slow?  Do we need to manually add the gag triggers in only when updated?
            foreach (var trigger in App.Settings.ProfileSettings.TriggerList.Where(x => x.Gag == true && x.Enabled == true))
            { 
                // These triggers match for the gag but do NOT execute the trigger's command (VERY important because it would cause the triggers
                // to get fired multiple times as the line is re-rendered on the screen.. that is -bad-).
                if (trigger.IsMatch(_sb.ToString(), true))
                {
                    CollapsedLineSections.Add(endLine.NextLine.LineNumber, CurrentContext.TextView.CollapseLines(endLine.NextLine, endLine.NextLine));
                    return startOffset;
                }
            }

            foreach (var trigger in App.SystemTriggers.Where(x => x.Gag == true && x.Enabled == true))
            {
                // These triggers match for the gag but do NOT execute the trigger's command (VERY important because it would cause the triggers
                // to get fired multiple times as the line is re-rendered on the screen.. that is -bad-).
                if (trigger.IsMatch(_sb.ToString(), true))
                {
                    CollapsedLineSections.Add(endLine.NextLine.LineNumber, CurrentContext.TextView.CollapseLines(endLine.NextLine, endLine.NextLine));
                    return startOffset;
                }
            }

            return -1;
        }

        public override VisualLineElement ConstructElement(int offset)
        {
            return new HiddenTextElement(CurrentContext.Document.GetLineByOffset(offset).TotalLength);
        }

    }

    /// <summary>
    /// Represents a hidden element in the text editor.
    /// </summary>
    public class HiddenTextElement : VisualLineElement
    {
        public HiddenTextElement(int documentLength) : base(1, documentLength)
        {
        }

        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            return new TextHidden(1);
        }
    }

}
