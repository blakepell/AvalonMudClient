using System;
using System.Linq;
using Avalon.Extensions;
using Avalon.Lua;
using Argus.Extensions;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Avalon.Colors;

namespace Avalon.Controls
{
    /// <summary>
    /// Interaction logic for AvalonLuaEditor.xaml
    /// </summary>
    public partial class AvalonLuaEditor
    {
        /// <summary>
        /// Used for auto completion with Lua.
        /// </summary>
        CompletionWindow _completionWindow;

        public static readonly DependencyProperty InfoButtonVisibilityProperty = DependencyProperty.Register(
            nameof(InfoButtonVisibility), typeof(Visibility), typeof(AvalonLuaEditor), new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Whether the Lua Info button is visible.
        /// </summary>
        public Visibility InfoButtonVisibility
        {
            get => (Visibility) GetValue(InfoButtonVisibilityProperty);
            set => SetValue(InfoButtonVisibilityProperty, value);
        }


        public static readonly DependencyProperty SaveButtonVisibilityProperty = DependencyProperty.Register(
            nameof(SaveButtonVisibility), typeof(Visibility), typeof(AvalonLuaEditor), new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Whether the Lua Save button is visible.
        /// </summary>
        public Visibility SaveButtonVisibility
        {
            get => (Visibility)GetValue(SaveButtonVisibilityProperty);
            set => SetValue(SaveButtonVisibilityProperty, value);
        }

        /// <summary>
        /// A reference to the object that the Lua editor should save to when save
        /// is clicked.
        /// </summary>
        public object SaveObject { get; set; }

        /// <summary>
        /// The name of the property that the Lua editor should save to when save
        /// is clicked.
        /// </summary>
        public string SaveProperty { get; set; }

        /// <summary>
        /// Whether or not the syntax highlighting has been loaded, for use if Loaded is called multiple
        /// times which seems to be happening when the control is made visible.
        /// </summary>
        private bool _isSyntaxLoaded = false;

        public AvalonLuaEditor()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void AvalonLuaEditor_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.SetupLuaEditor();
            
            Editor.TextArea.TextEntering += AvalonLuaEditor_TextEntering;
            Editor.TextArea.TextEntered += AvalonLuaEditor_TextEntered;
        }

        /// <summary>
        /// Runs a Lua script in the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRunLua_OnClick(object sender, RoutedEventArgs e)
        {
            // Call our single point of Lua entry.
            try
            {
                var lua = App.MainWindow.Interp.LuaCaller;
                _ = lua.ExecuteAsync(Editor.Text);
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoError(ex.Message);

                if (ex.InnerException != null)
                {
                    App.Conveyor.EchoError(ex.InnerException.Message);
                }
            }
        }

        /// <summary>
        /// Sets up the syntax highlighting for the interactive Lua editor.
        /// </summary>
        public void SetupLuaEditor()
        {
            // If the syntax has already been loaded, get out.
            if (_isSyntaxLoaded)
            {
                return;
            }

            try
            {
                LoadSyntaxHighlighting(Editor);
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoError($"Error setting up Lua syntax highlighting: {ex.Message}");
            }

            _isSyntaxLoaded = true;
        }

        /// <summary>
        /// Loads the syntax highlighting rules, both from the stored resource and dynamically.
        /// </summary>
        /// <param name="te"></param>
        /// <remarks>
        /// This was made a static method so that other instances of that require our Lua syntax highlighting
        /// but might not be using this control can reference this code.
        /// </remarks>
        public static void LoadSyntaxHighlighting(TextEditor te)
        {
            var asm = Assembly.GetExecutingAssembly();

            using (var s = asm.GetManifestResourceStream($"{asm.GetName().Name}.Resources.LuaDarkTheme.xshd"))
            {
                using (var reader = new XmlTextReader(s))
                {
                    te.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

                    // Dynamic syntax highlighting for your own purpose
                    var rules = te.SyntaxHighlighting.MainRuleSet.Rules;

                    var rule = new HighlightingRule
                    {
                        Color = new HighlightingColor()
                        {
                            Foreground = new CustomizedBrush((Color) ColorConverter.ConvertFromString(("#DCDCAA")))
                        }
                    };

                    // Construct our custom highlighting rule via reflection.
                    var t = typeof(LuaCommands);
                    var sb = new StringBuilder();
                    sb.Append(@"\b(");

                    // This should get all of our methods but exclude ones that are defined on
                    // object like ToString, GetHashCode, Equals, etc.
                    foreach (var method in t.GetMethods().Where(m => m.DeclaringType != typeof(object)))
                    {
                        sb.AppendFormat("{0}|", method.Name);
                    }

                    sb.TrimEnd('|');
                    sb.Append(@")\w*\b");

                    rule.Regex = new Regex(sb.ToString());
                    rules.Add(rule);
                }
            }
        }

        /// <summary>
        /// Fired when the UserControl is unloaded.  Cleanup any resources including removing EventHandlers
        /// so this memory can be properly disposed of.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvalonLuaEditor_OnUnloaded(object sender, RoutedEventArgs e)
        {
            Editor.TextArea.TextEntering -= AvalonLuaEditor_TextEntering;
            Editor.TextArea.TextEntered -= AvalonLuaEditor_TextEntered;
        }

        private void AvalonLuaEditor_TextEntered(object sender, TextCompositionEventArgs e)
        {
            // Text colon or dot, find the word before it.
            if (e.Text != "." && e.Text != ":")
            {
                return;
            }

            // If the auto complete window is already showing don't show it again.
            if (_completionWindow != null)
            {
                return;
            }

            string word = GetWordBefore(Editor);

            if (word == "lua")
            {
                // Open code completion after the user has pressed dot
                _completionWindow = new CompletionWindow(Editor.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;                
                LuaCompletion.LoadCompletionData(data, word);
            }

            if (_completionWindow != null)
            {
                _completionWindow.Show();
                _completionWindow.Closed += (sender, args) => _completionWindow = null;
            }
        }

        private void AvalonLuaEditor_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                // Open code completion after the user has pressed dot:
                _completionWindow = new CompletionWindow(Editor.TextArea);
                var data = _completionWindow.CompletionList.CompletionData;
                LuaCompletion.LoadCompletionDatasnippets(data);

                _completionWindow.Show();
                _completionWindow.Closed += (sender, args) => _completionWindow = null;

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

            // NOTE: Do not set "e.Handled=true", we still want to insert the character that was typed.
        }

        /// <summary>
        /// Gets the word before the caret.  This seems to work accidentally.  Go through this when
        /// new use cases come up if wonky behavior occurs.
        /// </summary>
        /// <param name="textEditor"></param>
        public static string GetWordBefore(TextEditor textEditor)
        {
            var wordBeforeDot = string.Empty;
            int caretPosition = textEditor.CaretOffset - 2;

            if (caretPosition < 0)
            {
                return wordBeforeDot;
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

        /// <summary>
        /// Displays information about the current Lua environment in the main game terminal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonInfo_OnClick(object sender, RoutedEventArgs e)
        {
            _ = App.MainWindow.Interp.Send("#lua-debug", true, false);
        }

        private void ButtonSaveLua_Click(object sender, RoutedEventArgs e)
        {
            if (this.SaveObject == null || string.IsNullOrWhiteSpace(this.SaveProperty))
            {
                return;
            }

            try
            {
                this.SaveObject.Set(this.SaveProperty, this.Editor.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}