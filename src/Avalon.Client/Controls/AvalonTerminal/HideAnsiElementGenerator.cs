/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using ICSharpCode.AvalonEdit.Rendering;

namespace Avalon.Controls
{
    /// <summary>
    /// Hides ANSI color codes and control sequences.
    /// </summary>
    public class HideAnsiElementGenerator : VisualLineElementGenerator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">
        /// A reference to the parent <see cref="AvalonTerminal"/> required in order to
        /// see if a line has already been Gagged via it's <see cref="GagElementGenerator"/>.
        /// </param>
        public HideAnsiElementGenerator(AvalonTerminal parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Whether this should hide the smaller or larger set of ANSI sequences.  The smaller set might fill the need for
        /// most muds and have a modest performance boost.  The extended set doesn't cost much more processing and covers
        /// a wider variety of cases, especially for muds that implement some other standards like VT100.
        /// </summary>
        public bool ParseExtended { get; set; } = true;

        /// <summary>
        /// A reference to the parent terminal for this element generator.
        /// </summary>
        private readonly AvalonTerminal _parent;

        /// <summary>
        /// A basic list of ANSI control sequences.
        /// </summary>
        private static readonly char[] _endMarkers = { 'm', 'A', 'B', 'C', 'D', 'J' };

        /// <summary>
        /// A more extensive list of the ANSI control sequences.
        /// </summary>
        /// <remarks>
        /// The char 'm' is listed first since it will be the most used code that comes down from any game since it
        /// ends all of the color and style sequences. 
        /// </remarks>
        private static readonly char[] _endMarkersExtended = { 'm', 'D', 'A', 'B', 'C', 'J', 'E', 'F', 'G', 'H', 'K', 'S', 'T' };

        /// <summary>
        /// A match of a control sequence containing only the match's offset and the length
        /// to the end of the sequence.
        /// </summary>
        private struct Match
        {
            public int MatchOffset;
            public int Length;
        }

        /// <summary>
        /// Finds the position and length of the next ANSI sequence starting at the specified offset.
        /// </summary>
        /// <param name="startOffset"></param>
        private Match FindPosition(int startOffset)
        {
            var endLine = CurrentContext.VisualLine.LastDocumentLine;

            // If the line has already been collapsed, ignore it.  If we don't ignore it then the client will crash
            // or throw an exception on the second thing that is gagged (either here or in the GagElementGenerator).
            if (_parent?.Gag?.CollapsedLineSections.ContainsKey(endLine.LineNumber) ?? false)
            {
                return new Match()
                {
                    MatchOffset = startOffset
                };
            }

            var relevantText = CurrentContext.GetText(startOffset, endLine.EndOffset - startOffset);
            int index = relevantText.Text.IndexOf('\x1B', relevantText.Offset, relevantText.Count);
            
            var m = new Match
            {
                MatchOffset = index > -1 ? index - relevantText.Offset + startOffset : -1
            };

            if (index == -1)
            {
                return m;
            }

            int endIndex = relevantText.Text.IndexOfAny(this.ParseExtended ? _endMarkersExtended : _endMarkers, index);

            if (endIndex > -1)
            {
                endIndex++;
                m.Length = endIndex - index;
            }
            else
            {
                // There was no end to this sequence which might be a partial sequence.
                m.MatchOffset = -1;
            }

            return m;
        }

        /// <inheritdoc />
        public override int GetFirstInterestedOffset(int startOffset)
        {
            // This will return either the offset or a -1 if it's not found as per the AvalonEdit docs.
            return this.FindPosition(startOffset).MatchOffset;
        }

        /// <inheritdoc/>
        public override VisualLineElement ConstructElement(int offset)
        {
            var m = FindPosition(offset);

            // The offset must have found a match and be equal to the offset specified
            // in order to have the HiddenTextElement created, otherwise we ignore it
            // and return a null from here.
            if (m.MatchOffset > -1 && m.MatchOffset == offset)
            {
                return new HiddenTextElement(m.Length);
            }

            return null;
        }
    }
}