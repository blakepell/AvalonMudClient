using Avalon.Common.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Avalon
{
    /// <summary>
    /// A simple Lua highlighted text editor for use with editing Lua scripts.
    /// </summary>
    public partial class MacroEditWindow : Window
    {
        /// <summary>
        /// Editor type: Add or Edit.
        /// </summary>
        public enum EditTypeCode
        {
            Add,
            Edit
        }

        /// <summary>
        /// Whether this is an add or edit dialog.
        /// </summary>
        public EditTypeCode EditType { get; set; } = EditTypeCode.Add;


        /// <summary>
        /// The values of the original macro so we can back it out if the use cancels.
        /// </summary>
        private Macro _originalMacro = new Macro();

        /// <summary>
        /// The macro passed into the dialog.
        /// </summary>
        private Macro _macro;

        /// <summary>
        /// A reference to the macro that is being edited.
        /// </summary>
        public Macro Macro
        {
            get
            {
                return _macro;
            }
            set
            {
                _originalMacro.Command = value.Command;
                _originalMacro.Key = value.Key;
                _originalMacro.KeyDescription = value.KeyDescription;

                _macro = value;
                TextBoxCommand.Text = _macro.Command;

                if (string.IsNullOrWhiteSpace(_macro.KeyDescription))
                {
                    var convert = new KeyConverter();

                    if (_macro.Key == 156)
                    {
                        _macro.KeyDescription = "F10";
                    }
                    else
                    {
                        _macro.KeyDescription = convert.ConvertToString(_macro.Key);
                    }
                }
                
                TextBoxMacroKey.Text = _macro.KeyDescription;

                SetStatus(_macro);
            }
        }

        /// <summary>
        /// The default status bar color.
        /// </summary>
        private SolidColorBrush _defaultStatusColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#007ACC"));

        /// <summary>
        /// Constructor.
        /// </summary>
        public MacroEditWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Code that should run when the window is first loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StringEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxMacroKey.Focus();
        }

        /// <summary>
        /// Sets the status with the contents of a selected key.
        /// </summary>
        /// <param name="key"></param>
        public void SetStatus(Key key)
        {
            var convert = new KeyConverter();            
            TextBlockStatus.Text = $"Macro for {convert.ConvertToString(key)} (Key Code {(int)key})";
        }

        /// <summary>
        /// Sets the status with the contents of a selected key from a macro.
        /// </summary>
        /// <param name="macro"></param>
        public void SetStatus(Macro macro)
        {
            TextBlockStatus.Text = $"Macro for {macro.KeyDescription} (Key Code {_macro.Key})";
        }

        /// <summary>
        /// Sets the status.
        /// </summary>
        /// <param name="msg"></param>
        public void SetStatus(string msg)
        {
            TextBlockStatus.Text = msg;
        }

        /// <summary>
        /// Code that is executed for the Cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.EditType == EditTypeCode.Add)
            {
                // If they're cancelling from an add new, remove it from the list.
                App.Settings.ProfileSettings.MacroList.Remove(_macro);
            }
            else if (this.EditType == EditTypeCode.Edit)
            {
                // Reset the values to the original values.
                _macro.Key = _originalMacro.Key;
                _macro.Command = _originalMacro.Command;
                _macro.KeyDescription = _originalMacro.KeyDescription;
            }

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
            this.Macro.Command = TextBoxCommand.Text;
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// PreviewKeyDown is required to handle some keys that don't get some to KeyDown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxMacroKey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                case Key.Insert:
                case Key.End:
                case Key.Home:
                case Key.PageDown:
                case Key.PageUp:
                case Key.NumLock:
                case Key.CapsLock:
                case Key.Tab:
                case Key.Back:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LWin:
                case Key.RWin:
                    e.Handled = true;
                    SetStatus($"{e.Key.ToString()} is not valid");
                    break;
            }
        }

        /// <summary>
        /// Handles the key down event to get the key for the specific key.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Key codes for english keyboards: https://lists.w3.org/Archives/Public/www-dom/2010JulSep/att-0182/keyCode-spec.html
        /// </remarks>
        private void TextBoxMacroKey_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            TextBoxMacroKey.Text = "";

            var convert = new KeyConverter();
            _macro.Key = (int)e.Key;

            // For whatever reason, F10 is special and considered a system key.
            if (e.Key == Key.System && e.SystemKey == Key.F10)
            {
                _macro.KeyDescription = "F10";
            }
            else
            {
                // And everything else.
                _macro.KeyDescription = convert.ConvertToString(e.Key);
            }

            SetStatus(_macro);

            TextBoxMacroKey.Text = _macro.KeyDescription;
        }

    }
}
