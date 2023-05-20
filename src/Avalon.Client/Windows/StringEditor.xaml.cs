/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Lua;
using Avalon.Utilities;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Windows;
using System.Windows.Input;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class StringEditor
    {
        /// <summary>
        /// The value of the Lua text editor.
        /// </summary>
        public string Text
        {
            get => AvalonLuaEditor.Text;
            set => AvalonLuaEditor.Text = value;
        }

        /// <summary>
        /// The text for the status bar.
        /// </summary>
        public string StatusText
        {
            get => TextBlockStatus.Text;
            set => TextBlockStatus.Text = value;
        }

        private EditorType _editorMode;

        public EditorType EditorMode
        {
            get => _editorMode;
            set
            {
                _editorMode = value;

                switch (_editorMode)
                {
                    case EditorType.Text:
                        this.Title = "Text Editor";
                        break;
                    case EditorType.Lua:
                        this.Title = "Lua Editor";
                        Avalon.Controls.AvalonLuaEditor.LoadSyntaxHighlighting(AvalonLuaEditor);
                        this.StatusText = "Press [F1] for code snippets or type 'lua.' to see custom functions.";
                        break;
                }
            }
        }

        public enum EditorType
        {
            Text,
            Lua
        }

        /// <summary>
        /// Used for auto completion with Lua.
        /// </summary>
        CompletionWindow _completionWindow;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StringEditor()
        {
            InitializeComponent();

            AvalonLuaEditor.TextArea.TextEntering += AvalonLuaEditor_TextEntering;
            AvalonLuaEditor.TextArea.TextEntered += AvalonLuaEditor_TextEntered;
            AvalonLuaEditor.Options.ConvertTabsToSpaces = true;
        }

        /// <summary>
        /// Gets or sets the text of the action button.
        /// </summary>
        public string ActionButtonText
        {
            get => ButtonSave.Content.ToString();
            set => ButtonSave.Content = value;
        }

        /// <summary>
        /// Fires when the Window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StringEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AvalonLuaEditor.Focus();
        }

        /// <summary>
        /// Fired when the Window is unloaded.  Cleanup any resources including removing EventHandlers
        /// so this memory can be properly disposed of.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StringEditor_OnUnloaded(object sender, RoutedEventArgs e)
        {
            AvalonLuaEditor.TextArea.TextEntering -= AvalonLuaEditor_TextEntering;
            AvalonLuaEditor.TextArea.TextEntered -= AvalonLuaEditor_TextEntered;
        }

        private void AvalonLuaEditor_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Text colon or dot, find the word before it.
            if (e.Text == "." || e.Text == ":")
            {
                string word = GetWordBefore(AvalonLuaEditor);

                if (word == "lua")
                {
                    // Open code completion after the user has pressed dot:
                    _completionWindow = new CompletionWindow(AvalonLuaEditor.TextArea);
                    var data = _completionWindow.CompletionList.CompletionData;
                    LuaCompletion.LoadCompletionData(data, word);
                }
                else if (word == "win")
                {
                    // Open code completion after the user has pressed dot
                    _completionWindow = new CompletionWindow(AvalonLuaEditor.TextArea);
                    var data = _completionWindow.CompletionList.CompletionData;
                    LuaCompletion.LoadCompletionData(data, word);
                }

                if (_completionWindow != null)
                {
                    _completionWindow.Show();
                    _completionWindow.Closed += (sender, args) => _completionWindow = null;
                }
            }
        }

        /// <summary>
        /// Escapes either the selected text or the entire text box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemEscapeString_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // If a selection, just do that, otherwise, do the whole thing.
                if (AvalonLuaEditor.SelectionLength > 0)
                {
                    string str = AvalonLuaEditor.SelectedText;
                    int startPos = AvalonLuaEditor.SelectionStart;
                    int len = AvalonLuaEditor.SelectionLength;
                    string escapedText = JsonConvert.ToString(AvalonLuaEditor.Text.Substring(startPos, len));

                    AvalonLuaEditor.Delete();
                    AvalonLuaEditor.Document.Insert(startPos, escapedText);
                }
                else
                {
                    AvalonLuaEditor.Text = JsonConvert.ToString(AvalonLuaEditor.Text);
                }

                this.StatusText = "";
            }
            catch (System.Exception ex)
            {
                this.StatusText = $"Error escaping string: {ex.Message}";
            }
        }

        /// <summary>
        /// Unescapes the selectect text or the entire box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemUnescapeString_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // If a selection, just do that, otherwise, do the whole thing.
                if (AvalonLuaEditor.SelectionLength > 0)
                {
                    string str = AvalonLuaEditor.SelectedText;
                    int startPos = AvalonLuaEditor.SelectionStart;
                    int len = AvalonLuaEditor.SelectionLength;
                    string unescapedText = JsonConvert.DeserializeObject<string>(AvalonLuaEditor.Text.Substring(startPos, len));

                    AvalonLuaEditor.Delete();
                    AvalonLuaEditor.Document.Insert(startPos, unescapedText);
                }
                else
                {
                    AvalonLuaEditor.Text = JsonConvert.DeserializeObject<string>(AvalonLuaEditor.Text);
                }

                this.StatusText = "";
            }
            catch (System.Exception ex)
            {
                this.StatusText = $"Error unescaping string: {ex.Message}";
            }
        }

        private void AvalonLuaEditor_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void AvalonLuaEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                // Open code completion after the user has pressed dot:
                _completionWindow = new CompletionWindow(AvalonLuaEditor.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;
                LuaCompletion.LoadCompletionDatasnippets(data);

                _completionWindow.Show();
                _completionWindow.Closed += (sender, args) => _completionWindow = null;

            }
        }

        /// <summary>
        /// Code that is executed for the Cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Code that is executed for the Save button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            // Allow Lua validating to be turned off, but if it's on attempt to use LoadString to check
            // for any blatant syntax errors.
            if (this.EditorMode == EditorType.Lua && App.Settings.AvalonSettings.ValidateLua)
            {
                var luaResult = await App.MainWindow.Interp.ScriptHost.MoonSharp.ValidateAsync(this.Text);

                if (!luaResult.Success && luaResult.Exception != null)
                {
                    string buf = $"An error occurred on line {luaResult.Exception.ToLineNumber.ToString()}\r\nMessage: {luaResult?.Exception?.Message ?? "N/A"}\r\n\r\nWould you still like to save?";

                    var result = await WindowManager.InputBox(buf, "Syntax Error");

                    if (!result)
                    {
                        return;
                    }
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Gets the word before the caret.  This seems to work accidentally.  Go through this when
        /// new use cases come up if wonky behavior occurs.
        /// </summary>
        /// <param name="textEditor"></param>
        public static string GetWordBefore(TextEditor textEditor)
        {
            var wordBeforeDot = string.Empty;
            var caretPosition = textEditor.CaretOffset - 2;

            if (caretPosition < 0)
            {
                return "";
            }

            var lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(caretPosition));
            string text = textEditor.Document.GetText(lineOffset, 1);

            while (true)
            {
                if (text == null && text.CompareTo(' ') > 0)
                {
                    break;
                }
                if (Regex.IsMatch(text, @".*[^A-Za-z\. ]"))
                {
                    break;
                }

                if (text != "." && text != ":" && text != " ")
                {
                    wordBeforeDot = text + wordBeforeDot;
                }

                if (text == " ")
                {
                    break;
                }

                if (caretPosition == 0)
                {
                    break;
                }

                lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(--caretPosition));

                text = textEditor.Document.GetText(lineOffset, 1);
            }

            return wordBeforeDot;
        }
    }
}