/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Avalon.Windows.MobProgEditor
{
    /// <summary>
    /// CompletionData for use with MobProgs.
    /// </summary>
    public class MobProgCompletionData : ICompletionData
    {
        public MobProgCompletionData(string text)
        {
            this.Text = text;
        }

        public MobProgCompletionData(string text, string description)
        {
            this.Text = text;
            this.Description = description;
        }

        public MobProgCompletionData(string text, string description, string contentPrefix)
        {
            this.Text = text;
            this.Description = description;
            this.ContentPrefix = contentPrefix;
        }

        public MobProgCompletionData(string text, string description, string contentPrefix, double priority)
        {
            this.Text = text;
            this.Description = description;
            this.ContentPrefix = contentPrefix;
            this.Priority = priority;
        }

        public System.Windows.Media.ImageSource Image { get; set; } = null;

        /// <summary>
        /// Actual text to insert.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// A prefix that displays before the Content display.
        /// </summary>
        public string ContentPrefix { get; set; } = "";

        /// <summary>
        /// Use this property if you want to show a fancy UIElement in the list that displays.
        /// </summary>
        public object Content => $"{this.ContentPrefix}{this.Text}";

        public object Description { get; set; }

        public double Priority { get; set; } = 1.0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
