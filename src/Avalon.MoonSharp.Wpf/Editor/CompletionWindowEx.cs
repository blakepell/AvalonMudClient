// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MoonSharp.Interpreter.Wpf.Editor
{
    /// <summary>
    /// The code completion window.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class CompletionWindowEx : CompletionWindowBase
    {
        private readonly CompletionList? _completionList;

        private ToolTip? _toolTip;

        /// <summary>
        /// Gets the completion list used in this completion window.
        /// </summary>
        public CompletionList? CompletionList => _completionList;

        /// <summary>
		/// Creates a new code completion window.
		/// </summary>
		public CompletionWindowEx(TextArea textArea) : base(textArea)
        {
            _completionList = new();

            // keep height automatic
            CloseAutomatically = true;
            SizeToContent = SizeToContent.Height;
            MaxHeight = 300;
            Width = 200;
            Content = _completionList;
            // prevent user from resizing window to 0x0
            MinHeight = 15;
            MinWidth = 30;

            _toolTip = new()
            {
                PlacementTarget = this,
                Placement = PlacementMode.Right,
                MaxWidth = 500
            };

            _toolTip.Closed += this.ToolTip_Closed;

            AttachEvents();
        }

        #region ToolTip handling
        private void ToolTip_Closed(object sender, RoutedEventArgs e)
        {
            // Clear content after tooltip is closed.
            // We cannot clear is immediately when setting IsOpen=false
            // because the tooltip uses an animation for closing.
            if (_toolTip != null)
            {
                _toolTip.Content = null;
            }
        }

        private void CompletionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = _completionList?.SelectedItem;

            if (item == null || _toolTip == null)
            {
                return;
            }

            object description = item.Description;

            if (description != null)
            {
                if (item is LuaCompletionData data)
                {
                    _toolTip.Content = data.CodeMetadataControl;
                }

                _toolTip.IsOpen = true;
            }
            else
            {
                _toolTip.IsOpen = false;
            }
        }
        #endregion

        private void CompletionList_InsertionRequested(object sender, EventArgs e)
        {
            Close();

            // The window must close before Complete() is called.
            // If the Complete callback pushes stacked input handlers, we don't want to pop those when the CC window closes.
            var item = _completionList?.SelectedItem;
            item?.Complete(TextArea, new AnchorSegment(TextArea.Document, StartOffset, EndOffset - StartOffset), e);
        }

        private void AttachEvents()
        {
            if (_completionList != null)
            {
                _completionList.InsertionRequested += this.CompletionList_InsertionRequested;
                _completionList.SelectionChanged += this.CompletionList_SelectionChanged;
            }

            TextArea.Caret.PositionChanged += this.CaretPositionChanged;
            TextArea.MouseWheel += this.TextArea_MouseWheel;
            TextArea.PreviewTextInput += this.TextArea_PreviewTextInput;
        }

        /// <inheritdoc/>
        protected override void DetachEvents()
        {
            if (_completionList != null)
            {
                _completionList.InsertionRequested -= this.CompletionList_InsertionRequested;
                _completionList.SelectionChanged -= this.CompletionList_SelectionChanged;
            }

            TextArea.Caret.PositionChanged -= this.CaretPositionChanged;
            TextArea.MouseWheel -= this.TextArea_MouseWheel;
            TextArea.PreviewTextInput -= this.TextArea_PreviewTextInput;

            base.DetachEvents();
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_toolTip != null)
            {
                _toolTip.IsOpen = false;
                _toolTip = null;
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled)
            {
                _completionList?.HandleKey(e);
            }
        }

        private void TextArea_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = RaiseEventPair(this, PreviewTextInputEvent, TextInputEvent,
                                       new TextCompositionEventArgs(e.Device, e.TextComposition));
        }

        private void TextArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = RaiseEventPair(GetScrollEventTarget(),
                                       PreviewMouseWheelEvent, MouseWheelEvent,
                                       new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta));
        }

        private UIElement GetScrollEventTarget()
        {
            if (_completionList == null)
            {
                return this;
            }

            return _completionList.ScrollViewer ?? _completionList.ListBox ?? (UIElement)_completionList;
        }

        /// <summary>
        /// Gets/Sets whether the completion window should close automatically.
        /// The default value is true.
        /// </summary>
        public bool CloseAutomatically { get; set; }

        /// <inheritdoc/>
        protected override bool CloseOnFocusLost => CloseAutomatically;

        /// <summary>
		/// When this flag is set, code completion closes if the caret moves to the
		/// beginning of the allowed range. This is useful in Ctrl+Space and "complete when typing",
		/// but not in dot-completion.
		/// Has no effect if CloseAutomatically is false.
		/// </summary>
		public bool CloseWhenCaretAtBeginning { get; set; }

        private void CaretPositionChanged(object sender, EventArgs e)
        {
            int offset = TextArea.Caret.Offset;
            if (offset == StartOffset)
            {
                if (CloseAutomatically && CloseWhenCaretAtBeginning)
                {
                    Close();
                }
                else
                {
                    _completionList?.SelectItem(string.Empty);
                }
                return;
            }
            if (offset < StartOffset || offset > EndOffset)
            {
                if (CloseAutomatically)
                {
                    Close();
                }
            }
            else
            {
                var document = TextArea.Document;

                if (document != null)
                {
                    _completionList?.SelectItem(document.GetText(StartOffset, offset - StartOffset));
                }
            }
        }
    }
}