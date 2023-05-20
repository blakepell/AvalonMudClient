/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Avalon
{
    /// <summary>
    /// An immortal utility for restrings.
    /// </summary>
    public partial class OlcVnumBatchCommandsWindow : Window
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
        /// Constructor.
        /// </summary>
        public OlcVnumBatchCommandsWindow(IInterpreter interp)
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
            TextBoxVnumStart.Focus();
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
        private async void ButtonSendToDsl_OnClick(object sender, RoutedEventArgs e)
        {
            if (!TextBoxVnumStart.Text.IsNumeric())
            {
                SetError("Starting VNUM must be a number.");
                TextBoxVnumStart.Focus();
                return;
            }

            if (!TextBoxVnumEnd.Text.IsNumeric())
            {
                SetError("Ending VNUM must be a number.");
                TextBoxVnumEnd.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxCommandsToExecute.Text))
            {
                SetError("Please enter a list of commands to execute at each VNUM.");
                TextBoxCommandsToExecute.Focus();
                return;
            }

            ClearError();

            string commands = Preview();

            // Split the prog up into lines
            var lines = commands.Split(Environment.NewLine);

            // Send each line with a slight delay as to not get disconnected from the game.
            foreach (string line in lines)
            {
                await _interp.Send(line, false, false);
                await Task.Delay(250);
            }

            this.Close();
        }

        /// <summary>
        /// Calls the Preview event to see the items without color codes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxOriginalKeyword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxPreview != null)
            {
                TextBoxPreview.Text = Preview();
            }
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
            ButtonSendToDsl.IsEnabled = false;
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
            ButtonSendToDsl.IsEnabled = true;
        }

        /// <summary>
        /// Generates the preview from what was entered into the text boxes.
        /// </summary>
        public string Preview()
        {        
            var sb = Argus.Memory.StringBuilderPool.Take();
            
            try
            {
                if (!TextBoxVnumStart.Text.IsNumeric() || !TextBoxVnumEnd.Text.IsNumeric())
                {
                    return "No preview available.";
                }

                int startVnum = int.Parse(TextBoxVnumStart.Text);
                int endVnum = int.Parse(TextBoxVnumEnd.Text);

                if ((endVnum - startVnum) > 1000)
                {
                    SetError("Sanity check: Not for ranges larger than 1,000 vnums.");
                    return "No preview available";
                }

                bool endingCrLf = TextBoxCommandsToExecute.Text.EndsWith("\r\n");

                for (int i = startVnum; i <= endVnum; i++)
                {
                    sb.Append(TextBoxCommandsToExecute.Text.Replace("@vnum", i.ToString()));

                    if (!endingCrLf)
                    {
                        sb.Append("\r\n");
                    }

                }

                ClearError();
                this.StatusText = $"{sb.ToString().Count(x => x == '\n')} commands have been prepared.";
                return sb.ToString();
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
                return "No preview available.";
            }
            finally
            {
                Argus.Memory.StringBuilderPool.Return(sb);
            }
        }

        /// <summary>
        /// Updates the preview whenever a TextBox loses focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxPreview.Text = Preview();
        }

    }
}
