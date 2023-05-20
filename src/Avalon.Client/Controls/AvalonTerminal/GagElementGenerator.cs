/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Colors;
using Cysharp.Text;
using ICSharpCode.AvalonEdit.Rendering;

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
        /// allocated once when the Gag is created I've chosen to keep this here instead of using the StringBuilderPool because
        /// it's always going to be needed so I've just made it dedicated.
        /// </summary>
        private readonly StringBuilder _sb = new();

        /// <summary>
        /// Tracks all of the collapsed sections so that we can call UnCollapse() if needed.
        /// </summary>
        public Dictionary<int, CollapsedLineSection> CollapsedLineSections = new();

        /// <summary>
        /// Whether or not the gag is currently enabled.  It is important to reference the property for this
        /// value when setting it unless you're looking to specifically not UncollapseAll() which is probably
        /// a bad idea.
        /// </summary>
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

        /// <summary>
        /// Uncollapses a single line.  Note, if a line was removed from the document all subsequent lines need to
        /// be Uncollapsed (as they have moved up and are no longer in sync).
        /// </summary>
        /// <param name="lineNumber"></param>
        public void UncollapseLine(int lineNumber)
        {
            if (CollapsedLineSections.TryGetValue(lineNumber, out var section))
            {
                section.Uncollapse();
                CollapsedLineSections.Remove(lineNumber);
            }
        }

        /// <summary>
        /// Uncollapses all lines after and including the line specified.
        /// </summary>
        /// <param name="lineNumber"></param>
        public void UncollapseAfter(int lineNumber)
        {
            foreach (int key in CollapsedLineSections.Keys)
            {
                if (key >= lineNumber)
                {
                    CollapsedLineSections[key].Uncollapse();
                    CollapsedLineSections.Remove(key);
                }
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
                if (CollapsedLineSections.Count > 0)
                {
                    this.UncollapseAll();
                }

                return -1;
            }
            
            var endLine = this.CurrentContext.VisualLine.LastDocumentLine;
            var segment = this.CurrentContext.GetText(startOffset, endLine.EndOffset - startOffset);
            
            // If the line has already been collapsed, ignore it.  However, if a trigger changes this potentially would need
            // to be called again because it would have been collapsed by the old version.  But, this might be desirable.
            if (endLine?.NextLine != null && CollapsedLineSections.ContainsKey(endLine.LineNumber))
            {
                return startOffset;
            }

            // Clear the StringBuilder and re-remove the ANSI codes so the match can run again.
            _sb.Clear();
            _sb.Append(segment.Text);
            Colorizer.RemoveAllAnsiCodes(_sb);

            // Create only one string that will pass multiple times into the trigger's IsMatch function.
            string text = _sb.ToString();

            // Once a trigger is found that this thing basically gets out.  It might behoove us here to run the system triggers
            // first and maybe have a priority sequence so they can be run in a certain order.  The example being, the prompt
            // will be gagged the most, it should run first and if it is, nothing else has to run here.
            // Also of note, the documentation for CollapseLines indicates that "if you want a VisualLineElement to span from line N to line M, then you 
            // need to collapse only the lines N+1 to M. Do not collapse line N itself (cause collapsing the next line looks weird at first read).

            // TODO: This try catch is not ideal, but will catch Collection was modified errors.  The collection needs to be concurrent or isolated.
            // System Triggers moved to be first.
            try
            {
                foreach (var trigger in App.InstanceGlobals.SystemTriggers.Where(x => x.Gag && x.Enabled))
                {
                    // These triggers match for the gag but do NOT execute the trigger's command (VERY important because it would cause the triggers
                    // to get fired multiple times as the line is re-rendered on the screen.. that is -bad-).
                    if (endLine?.NextLine != null && trigger.Regex?.IsMatch(text) == true)
                    {
                        CollapsedLineSections.Add(endLine.LineNumber, this.CurrentContext.TextView.CollapseLines(endLine.NextLine, endLine.NextLine));
                        return startOffset;
                    }
                }

                // Regular triggers, same comments as above.
                foreach (var trigger in App.Settings.ProfileSettings.TriggerList.GagEnumerable())
                {
                    if (endLine?.NextLine != null && trigger.Regex?.IsMatch(text) == true)
                    {
                        CollapsedLineSections.Add(endLine.LineNumber, this.CurrentContext.TextView.CollapseLines(endLine.NextLine, endLine.NextLine));
                        return startOffset;
                    }
                }
            }
            catch
            {
                // TODO: Error Logging
            }

            return -1;
        }

        public override VisualLineElement ConstructElement(int offset)
        {
            return new HiddenTextElement(this.CurrentContext.Document.GetLineByOffset(offset).TotalLength);
        }

    }
}