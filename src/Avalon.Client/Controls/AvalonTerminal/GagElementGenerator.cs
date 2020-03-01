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
        /// while rendering the impact was enough that this made sense.  Since this is heavily used in the Gag and it's only
        /// allocated once when the Gag is created I've choosen to keep this here instead of using the StringBuilderPool because
        /// it's always going to be needed so I've just made it dedicated.
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
                    this.UncollapseAll();
                }

                _enabled = value;
            }
        }

        /// <summary>
        /// Uncollapses all of the lines.  Generally this is done when the control's gagging is disabled but
        /// it also needs to be done if the triggers that are enforcing gagging have changed to be off.  It basically
        /// needs a reset so it can re-calculate what should be gagged at that point.
        /// </summary>
        public void UncollapseAll()
        {
            foreach (int key in CollapsedLineSections.Keys)
            {
                CollapsedLineSections[key].Uncollapse();
                CollapsedLineSections.Remove(key);
            }
        }

        public override int GetFirstInterestedOffset(int startOffset)
        {
            // Don't process if the AppSettings are null or triggers are disabled.
            if (App.Settings.ProfileSettings.TriggersEnabled == false || this.Enabled == false)
            {
                // If triggers were globally shut off some how and we're tracking collapsed lines uncollapse all of them.
                // If we don't do this AvalonEdit will crash.  Whenever triggers are re-enabled all of the gagging will
                // be re-applied (the content of what was moved from this terminal stays in the other terminals which probably
                // will have gagging disabled nearly always since the main point of gagging is to cleanup the main terminal
                // and organize what's coming in).
                if (this.CollapsedLineSections.Count > 0)
                {
                    this.UncollapseAll();
                }

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
            // Once a trigger is found that this thing basically gets out.  It might behoof us here to run the system triggers
            // first and maybe have a priority sequence so they can be run in a certain order.  The example being, the prompt
            // will be gagged the most, it should run first and if it is, nothing else has to run here.

            // System Triggers moved to be first.
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

            // Regular triggers
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
