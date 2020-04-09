using System.Reflection;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class StringEditor : Window
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

                switch(_editorMode)
                {
                    case EditorType.Text:
                        this.Title = "Text Editor";
                        break;
                    case EditorType.Lua:
                        this.Title = "Lua Editor";

                        var asm = Assembly.GetExecutingAssembly();
                        string resourceName = $"{asm.GetName().Name}.Resources.LuaDarkTheme.xshd";

                        using (var s = asm.GetManifestResourceStream(resourceName))
                        {
                            using (var reader = new XmlTextReader(s))
                            {
                                AvalonLuaEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                            }
                        }

                        break;
                }
            }
        }
        
        public enum EditorType
        {
            Text,
            Lua,
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StringEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the text of the action button.
        /// </summary>
        public string ActionButtonText
        {
            get
            {
                return ButtonSave.Content.ToString();
            }
            set
            {
                ButtonSave.Content = value;
            }
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
        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

    }
}
