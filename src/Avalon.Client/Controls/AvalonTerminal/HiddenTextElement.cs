/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows.Media.TextFormatting;

namespace Avalon.Controls
{
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
            return StaticElements.TextHiddenElement;
        }
    }

    /// <summary>
    /// Static elements for reuse.
    /// </summary>
    public static class StaticElements
    {
        /// <summary>
        /// Since the TextHidden(1) is used a lot and it never changes, we're going to make one
        /// static copy of it to save a lot of allocations.
        /// </summary>
        public static TextHidden TextHiddenElement = new(1);
    }
}