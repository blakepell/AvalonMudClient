/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System;
using System.Windows.Controls;
using System.Windows.Media;
using Argus.Memory;
using Cysharp.Text;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using MahApps.Metro.IconPacks;
using MoonSharp.Interpreter.Wpf.Controls;

namespace MoonSharp.Interpreter.Wpf.Editor
{
    /// <summary>
    /// Autocomplete data for Lua.
    /// </summary>
    public class LuaCompletionData : ICompletionData
    {
        public LuaCompletionData(string text)
        {
            Text = text;
            CodeMetadataControl = new();
        }

        public LuaCompletionData(string text, string description)
        {
            Text = text;
            Description = description;
            CodeMetadataControl = new();
            CodeMetadataControl.Description = description;
        }

        public LuaCompletionData(string text, string description, string contentPrefix)
        {
            Text = text;
            Description = description;
            ContentPrefix = contentPrefix;
            CodeMetadataControl = new();
            CodeMetadataControl.Description = description;
        }

        public LuaCompletionData(string text, string description, string contentPrefix, double priority)
        {
            Text = text;
            Description = description;
            ContentPrefix = contentPrefix;
            Priority = priority;
            CodeMetadataControl = new();
            CodeMetadataControl.Description = description;
        }

        public PackIconCodiconsKind Icon { get; set; }

        public SolidColorBrush IconForeground { get; set; } = Brushes.White;

        public ImageSource Image { get; set; } = null;

        /// <summary>
        /// Actual text to insert.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// A prefix that displays before the Content display.
        /// </summary>
        public string ContentPrefix { get; set; } = "";

        /// <summary>
        /// The number of arguments that exist for this if it is a method.
        /// </summary>
        public int Arguments { get; set; } = 0;

        /// <summary>
        /// If the AutoComplete is for a method.
        /// </summary>
        public bool IsMethod { get; set; } = false;

        /// <summary>
        /// If the AutoComplete is for a property.
        /// </summary>
        public bool IsProperty { get; set; } = false;

        /// <summary>
        /// Use this property if you want to show a fancy UIElement in the list that displays.
        /// </summary>
        public object Content => new TextBlock()
        {
            Text = Text,
        };

        public object Description { get; set; }

        public double Priority { get; set; } = 1.0;

        public CodeMetadataControl CodeMetadataControl { get; set; }

        /// <summary>
        /// Code that executes when the auto complete has been accepted is going to insert the text.
        /// </summary>
        /// <param name="textArea"></param>
        /// <param name="completionSegment"></param>
        /// <param name="insertionRequestEventArgs"></param>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            if (Text == "For Loop")
            {
                var sb = StringBuilderPool.Take();
                sb.AppendLine("for i = 1, 10 do");
                sb.AppendLine("   -- i");
                sb.AppendLine("end");

                textArea.Document.Replace(completionSegment, sb.ToString());
                StringBuilderPool.Return(sb);
            }
            else if (Text == "For Loop Pairs")
            {
                var sb = StringBuilderPool.Take();
                sb.AppendLine("for k, v in pairs(arr) do");
                sb.AppendLine("   --(k, v[1], v[2], v[3])");
                sb.AppendLine("end");

                textArea.Document.Replace(completionSegment, sb.ToString());
                StringBuilderPool.Return(sb);
            }
            else if (Text == "Multiline String")
            {
                var sb = StringBuilderPool.Take();
                sb.AppendLine("local buf = [[");
                sb.AppendLine("");
                sb.AppendLine("]]");

                textArea.Document.Replace(completionSegment, sb.ToString());
                StringBuilderPool.Return(sb);
            }
            else if (Text == "While Loop")
            {
                var sb = StringBuilderPool.Take();

                sb.Append(@"local condition = true
while(condition)
do
    -- Pause for 10 seconds
    ui.PauseAsync(10000)
end");

                textArea.Document.Replace(completionSegment, sb.ToString());
                StringBuilderPool.Return(sb);
            }
            else
            {
                using (var sb = ZString.CreateStringBuilder())
                {
                    if (IsMethod && Arguments == 0)
                    {
                        sb.Append(Text);
                        sb.Append("()");

                        textArea.Document.Replace(completionSegment, sb.ToString());
                    }
                    else if (IsMethod && Arguments >= 1)
                    {
                        sb.Append(Text);
                        sb.Append("()");

                        textArea.Document.Replace(completionSegment, sb.ToString());
                        textArea.Caret.Column--;
                    }
                    else
                    {
                        textArea.Document.Replace(completionSegment, Text);
                    }
                }
            }
        }
    }
}