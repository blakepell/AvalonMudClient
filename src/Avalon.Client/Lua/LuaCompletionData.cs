using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Text;

namespace Avalon.Lua
{
    /// <summary>
    /// Autocomplete data for Lua.
    /// </summary>
    public class LuaCompletionData : ICompletionData
    {
        public LuaCompletionData(string text)
        {
            this.Text = text;
        }

        public LuaCompletionData(string text, string description)
        {
            this.Text = text;
            this.Description = description;
        }

        public LuaCompletionData(string text, string description, string contentPrefix)
        {
            this.Text = text;
            this.Description = description;
            this.ContentPrefix = contentPrefix;
        }

        public LuaCompletionData(string text, string description, string contentPrefix, double priority)
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
        public object Content
        {
            get
            {
                return $"{this.ContentPrefix}{this.Text}";
            }
        }

        public object Description { get; set; }

        public double Priority { get; set; } = 1.0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // TODO, these need to be in external snippit files that are read dynamically.
            if (this.Text == "Scheduled Tasks")
            {
                var sb = Argus.Memory.StringBuilderPool.Take();
                sb.AppendLine("lua:Send(\"say I will now count to 10.\")");
                sb.AppendLine("lua:AddScheduledTask(\"say 1\", false, 1)");
                sb.AppendLine("lua:AddScheduledTask(\"say 2\", false, 2)");
                sb.AppendLine("lua:AddScheduledTask(\"say 3\", false, 3)");
                sb.AppendLine("lua:AddScheduledTask(\"say 4\", false, 4)");
                sb.AppendLine("lua:AddScheduledTask(\"say 5\", false, 5)");
                sb.AppendLine("lua:AddScheduledTask(\"say 6\", false, 6)");
                sb.AppendLine("lua:AddScheduledTask(\"say 7\", false, 7)");
                sb.AppendLine("lua:AddScheduledTask(\"say 8\", false, 8)");
                sb.AppendLine("lua:AddScheduledTask(\"say 9\", false, 9)");
                sb.AppendLine("lua:AddScheduledTask(\"say 10\", false, 10)");

                textArea.Document.Replace(completionSegment, sb.ToString());
                Argus.Memory.StringBuilderPool.Return(sb);
            }
            else if (this.Text == "For Loop")
            {
                var sb = Argus.Memory.StringBuilderPool.Take();
                sb.AppendLine("for i = 1, 10 do");
                sb.AppendLine("   -- i");
                sb.AppendLine("end");

                textArea.Document.Replace(completionSegment, sb.ToString());
                Argus.Memory.StringBuilderPool.Return(sb);
            }
            else if (this.Text == "For Loop Pairs")
            {
                var sb = Argus.Memory.StringBuilderPool.Take();
                sb.AppendLine("for k, v in pairs(arr) do");
                sb.AppendLine("   --(k, v[1], v[2], v[3])");
                sb.AppendLine("end");

                textArea.Document.Replace(completionSegment, sb.ToString());
                Argus.Memory.StringBuilderPool.Return(sb);
            }
            else
            {
                textArea.Document.Replace(completionSegment, this.Text);
            }
        }
    }
}