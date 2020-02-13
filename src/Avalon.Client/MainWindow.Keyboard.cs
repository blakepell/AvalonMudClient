using System.Windows.Input;

namespace Avalon
{
    /// <summary>
    /// Partial class for keyboard related events and code of the MainWindow.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Going to use the OnPreviewKeyDown for the entire window to handle macros.  We will not fire these
        /// however unless the main game tab is open.
        /// </summary>
        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Make sure that we are on the game tab which will always be the first tab.
            if (!TabGame.IsSelected)
            {
                return;
            }

            // Second, look for whether this key was a Macro, if a Macro is found, execute it,
            // set the focus to the text input box then get out.
            foreach (var item in App.Settings.ProfileSettings.MacroList)
            {
                if ((int)e.Key == item.Key)
                {
                    Interp.Send(item.Command, false, false);
                    TextInput.Editor.Focus();
                    e.Handled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// The PreviewKeyDown event for the input text box used to setup special behavior from that box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Editor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // So they key's for macros aren't entered into the text box.
            foreach (var item in App.Settings.ProfileSettings.MacroList)
            {
                if ((int)e.Key == item.Key)
                {
                    e.Handled = true;
                    return;
                }
            }

            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;

                    // When a command is entered into the input box.
                    // Make sure the newline didn't make it into the text input, then select all in the box so it can be cleared quickly.
                    TextInput.Editor.Text = TextInput.Editor.Text.Replace("\r", "").Replace("\n", "");
                    TextInput.Editor.SelectAll();
                    Interp.Send(TextInput.Editor.Text);

                    // Set the history count to the end
                    Interp.InputHistoryPosition = -1;

                    break;
                case Key.Up:
                    e.Handled = false;

                    // If the drop down is open allow the up and down keys to work for it, not history.
                    if (TextInput.IsDropDownOpen)
                    {
                        return;
                    }

                    //  Go to the previous item in the history.
                    TextInput.Editor.Text = Interp.InputHistoryNext();
                    TextInput.Editor.SelectionStart = (TextInput.Editor.Text.Length);
                    TextInput.Editor.SelectionLength = 0;

                    break;
                case Key.Down:
                    e.Handled = false;

                    // If the drop down is open allow the up and down keys to work for it, not history.
                    if (TextInput.IsDropDownOpen)
                    {
                        return;
                    }

                    //  Go to the next item in the history.
                    TextInput.Editor.Text = Interp.InputHistoryPrevious();
                    TextInput.Editor.SelectionStart = (TextInput.Editor.Text.Length);
                    TextInput.Editor.SelectionLength = 0;

                    break;
                case Key.PageUp:
                    // Scroll down a page in the terminal.
                    e.Handled = true;
                    GameTerminal.PageUp();
                    break;
                case Key.PageDown:
                    // Scroll down a page in the terminal.
                    e.Handled = true;
                    GameTerminal.PageDown();
                    break;
                case Key.Oem5:
                case Key.OemBackslash:
                    TextInput.Editor.SelectAll();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    Interp.InputHistoryPosition = -1;
                    TextInput.Editor.Text = "";
                    break;
            }
        }

        /// <summary>
        /// Common OnKeyDown event used in all of the ANSI terminals.  Since they are read only anytime someone
        /// clicks into one of them and types it will go straight to the input box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Terminal_OnKeyDown(object sender, KeyEventArgs e)
        {
            // Ignore if either of the control keys are down, this will allow control-c to copy text
            // that's selected out of the given terminal.  If this doesn't handle the control keys
            // the function in the actual control that overrides the copy to strip ANSI codes won't fire.
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                return;
            }

            // Set the focus of the keyboard to the input text box.
            TextInput.Editor.CaretIndex = TextInput.Editor.Text.Length;
            TextInput.Editor.Focus();
        }

    }
}
