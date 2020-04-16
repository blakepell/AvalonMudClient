using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Avalon
{
    /// <summary>
    /// An immortal utility for restrings.
    /// </summary>
    public partial class RestringWindow : Window
    {

        /// <summary>
        /// The text for the status bar.
        /// </summary>
        public string StatusText
        {
            get => TextBlockStatus.Text;
            set => TextBlockStatus.Text = value;
        }

        /// <summary>
        /// The default status bar color.
        /// </summary>
        private SolidColorBrush _defaultStatusColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#007ACC"));

        /// <summary>
        /// A reference to the clients interpreter.
        /// </summary>
        private IInterpreter _interp;

        /// <summary>
        /// A list of the AnsiColors so that we can remove them from the strings for easy viewing.
        /// </summary>
        private List<AnsiColor> _colors = AnsiColors.ToList();

        /// <summary>
        /// Constructor.
        /// </summary>
        public RestringWindow(IInterpreter interp)
        {
            InitializeComponent();
            _interp = interp;
        }

        /// <summary>
        /// Event that executes when the window is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StringEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxOriginalKeyword.Focus();
        }


        /// <summary>
        /// Code that is executed for the Cancel button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }

        /// <summary>
        /// Code that is executed for the Save button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonExecuteRestring_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TextBoxOriginalKeyword.Text))
            {
                SetError("Please enter the original keyword to use.");
                TextBoxOriginalKeyword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxShortDescription.Text))
            {
                SetError("Please enter a short description.");
                TextBoxShortDescription.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxLongDescription.Text))
            {
                SetError("Please enter a long description.");
                TextBoxLongDescription.Focus();
                return;
            }

            if (TextBoxShortDescription.Text.StartsWith("short", StringComparison.CurrentCultureIgnoreCase))
            {
                SetError("Your short description begings with the word 'short', copy/paste mistake?");
                TextBoxShortDescription.Focus();
                return;
            }

            if (TextBoxLongDescription.Text.StartsWith("long", StringComparison.CurrentCultureIgnoreCase))
            {
                SetError("Your long description begings with the word 'long', copy/paste mistake?");
                TextBoxShortDescription.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxNewKeywords.Text))
            {
                SetError("Please enter the new set of keywords for this item.");
                TextBoxNewKeywords.Focus();
                return;
            }

            ClearError();
            ExecuteCommands();
            this.Close();
        }

        /// <summary>
        /// Calls the Preview event to see the items without color codes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxOriginalKeyword_TextChanged(object sender, TextChangedEventArgs e)
        {
            Preview();
        }

        /// <summary>
        /// Sets the form to error colors.
        /// </summary>
        /// <param name="text"></param>
        public void SetError(string text)
        {
            StatusText = text;
            this.BorderBrush = Brushes.Red;
            TextBlockStatus.Background = Brushes.Red;
            StatusBarWindow.Background = Brushes.Red;
        }

        /// <summary>
        /// Resets the color of the form and the text on the status bar to the default.
        /// </summary>
        public void ClearError()
        {
            StatusText = "";
            this.BorderBrush = _defaultStatusColor;
            TextBlockStatus.Background = _defaultStatusColor;
            StatusBarWindow.Background = _defaultStatusColor;
        }

        /// <summary>
        /// Generates the preview from what was entered into the text boxes.
        /// </summary>
        public void Preview()
        {
            var sbShort = new StringBuilder(TextBoxShortDescription.Text);
            var sbLong = new StringBuilder(TextBoxLongDescription.Text);

            foreach (var color in _colors.Where(x => !string.IsNullOrWhiteSpace(x.MudColorCode)))
            {
                sbShort.Replace(color.MudColorCode, "");
                sbLong.Replace(color.MudColorCode, "");
            }

            TextBoxPreview.Text = $"{sbShort.ToString()}\r\n{sbLong.ToString()}";
        }

        /// <summary>
        /// Executes the restring commands with a slight delay between each command.
        /// </summary>
        public async void ExecuteCommands()
        {
            string guid = Guid.NewGuid().ToString();

            await _interp.Send($"string obj {TextBoxOriginalKeyword.Text} name {guid};");
            await Task.Delay(100);

            await _interp.Send($"string obj {guid} short {TextBoxShortDescription.Text};");
            await Task.Delay(100);

            await _interp.Send($"string obj {guid} long {TextBoxLongDescription.Text};");
            await Task.Delay(100);

            await _interp.Send($"string obj {guid} name {TextBoxNewKeywords.Text};");
        }

    }
}
