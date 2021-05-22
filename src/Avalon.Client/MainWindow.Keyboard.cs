/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Avalon.Controls;
using ModernWpf;
using System.Linq;
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
            // Second, look for whether this key was a Macro, if a Macro is found, execute it,
            // set the focus to the text input box then get out.
            foreach (var item in App.Settings.ProfileSettings.MacroList)
            {
                if ((int)e.Key == item.Key)
                {
                    // If they're recording commands then we are going to add it to the input history, because
                    // those commands might be directions.
                    if (!Interp.IsRecordingCommands)
                    {
                        Interp.Send(item.Command, false, false);
                    }
                    else
                    {
                        Interp.Send(item.Command, false, true);
                    }

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
                    TextInput.Editor.Text = TextInput.Editor.Text.RemoveLineEndings();
                    TextInput.Editor.SelectAll();
                    Interp.Send(TextInput.Editor.Text);

                    // If the user wants the input box to clear after a command, make it so.
                    if (App.Settings.AvalonSettings.InputBoxClearAfterCommand)
                    {
                        TextInput.Editor.Text = "";
                    }

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
                    e.Handled = true;

                    // Back buffer is collapsed, show it, scroll to the bottom of it.
                    if (GameBackBufferTerminal.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        GameBackBufferTerminal.Visibility = System.Windows.Visibility.Visible;

                        // Scroll via the vertical offset, if not done for some reason the first time the window is shown
                        // it will be at the top.
                        GameBackBufferTerminal.ScrollToLastLine(true);

                        // Since the height of this changed scroll it to the bottom.
                        GameTerminal.ScrollToLastLine();
                        break;
                    }

                    // If it was already visible, then we PageUp()
                    GameBackBufferTerminal.PageUp();

                    break;
                case Key.PageDown:
                    e.Handled = true;

                    // Back buffer is visible then execute a PageDown()
                    if (GameBackBufferTerminal.Visibility == System.Windows.Visibility.Visible)
                    {
                        GameBackBufferTerminal.PageDown();
                    }

                    // Now, if the last line in the back buffer is visible then we can just collapse the
                    // back buffer because the main terminal shows everything at the end.
                    if (GameBackBufferTerminal.IsLastLineVisible())
                    {
                        GameBackBufferTerminal.Visibility = System.Windows.Visibility.Collapsed;
                        TextInput.Editor.Focus();
                    }

                    break;
                case Key.Oem5:
                case Key.OemBackslash:
                    TextInput.Editor.SelectAll();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    // Collapse the back buffer so it hides it and reclaims the space for the main terminal.
                    GameBackBufferTerminal.Visibility = System.Windows.Visibility.Collapsed;

                    // Setting to see if the comm windows should scroll to the bottom on escape.
                    if (App.Settings.AvalonSettings.EscapeScrollsAllTerminalsToBottom)
                    {
                        Terminal1.ScrollToLastLine();
                        Terminal2.ScrollToLastLine();
                        Terminal3.ScrollToLastLine();
                        GameTerminal.ScrollToLastLine();
                    }

                    // Reset the input history to the default position and clear the text in the input box.
                    Interp.InputHistoryPosition = -1;
                    TextInput.Editor.Text = "";
                    break;
                case Key.Tab:
                    // Auto Complete from command history and/or aliases.
                    e.Handled = true;
                    string command = null;
                    bool ctrl = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

                    // If control is down search aliases, if it's not search the history
                    if (ctrl && !string.IsNullOrWhiteSpace(TextInput.Editor.Text))
                    {
                        // Aliases
                        var alias = App.Settings.ProfileSettings.AliasList.FirstOrDefault(x => x.AliasExpression.StartsWith(TextInput.Editor.Text, System.StringComparison.OrdinalIgnoreCase));

                        if (alias != null)
                        {
                            command = alias.AliasExpression;
                        }

                        // If the alias isn't null, put it in the text editor and leave, we're done.  If it is, we'll go ahead and
                        // continue on to the history as a fall back.
                        if (!string.IsNullOrWhiteSpace(command))
                        {
                            TextInput.Editor.Text = command;
                            TextInput.Editor.SelectionStart = TextInput.Editor.Text.Length;
                            return;
                        }
                    }

                    // If there is no input in the text editor, get the last entered command otherwise search the input
                    // history for the command (searching the latest entries backwards).
                    if (string.IsNullOrWhiteSpace(TextInput.Editor.Text))
                    {
                        command = Interp.InputHistory.Last();
                    }
                    else
                    {
                        command = Interp.InputHistory.FindLast(x => x.StartsWith(TextInput.Editor.Text, System.StringComparison.OrdinalIgnoreCase));
                    }

                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        TextInput.Editor.Text = command;
                        TextInput.Editor.SelectionStart = TextInput.Editor.Text.Length;
                    }

                    break;
            }
        }

        /// <summary>
        /// Shared preview key down logic between the game terminal and it's back buffer.  If they have focus this
        /// will implement page up and page down so that the back buffer will show all of the paging, once it gets
        /// to the bottom it will disappear.  The escape key will send the focus back to the input box hiding the
        /// back buffer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameTerminal_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.PageUp:
                    e.Handled = true;

                    // Back buffer is collapsed, show it, scroll to the bottom of it.
                    if (GameBackBufferTerminal.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        GameBackBufferTerminal.Visibility = System.Windows.Visibility.Visible;

                        // Scroll via the vertical offset, if not done for some reason the first time the window is shown
                        // it will be at the top.
                        GameBackBufferTerminal.ScrollToLastLine(true);

                        // Since the height of this changed scroll it to the bottom.
                        GameTerminal.ScrollToLastLine();
                        break;
                    }

                    // If it was already visible, then we PageUp()
                    GameBackBufferTerminal.PageUp();

                    break;
                case Key.PageDown:
                    e.Handled = true;

                    // Back buffer is visible then execute a PageDown()
                    if (GameBackBufferTerminal.Visibility == System.Windows.Visibility.Visible)
                    {
                        GameBackBufferTerminal.PageDown();
                    }

                    // Now, if the last line in the back buffer is visible then we can just collapse the
                    // back buffer because the main terminal shows everything at the end.  Set the focus
                    // back to the input box if it's not already there.
                    if (GameBackBufferTerminal.IsLastLineVisible())
                    {
                        GameBackBufferTerminal.Visibility = System.Windows.Visibility.Collapsed;
                        TextInput.Editor.Focus();
                    }

                    break;
                case Key.Escape:
                    // Collapse the back buffer so it hides it and reclaims the space for the main terminal.
                    GameBackBufferTerminal.Visibility = System.Windows.Visibility.Collapsed;

                    // Setting to see if the comm windows should scroll to the bottom on escape.
                    if (App.Settings.AvalonSettings.EscapeScrollsAllTerminalsToBottom)
                    {
                        GameTerminal.ScrollToLastLine();
                        Terminal1.ScrollToLastLine();
                        Terminal2.ScrollToLastLine();
                        Terminal3.ScrollToLastLine();
                    }

                    // Reset the input history to the default position and clear the text in the input box.
                    Interp.InputHistoryPosition = -1;
                    TextInput.Editor.Text = "";
                    TextInput.Editor.Focus();

                    break;
            }
        }

        /// <summary>
        /// Common OnKeyDown event used in all of the ANSI terminals.  Since they are read only anytime someone
        /// clicks into one of them and types it will go straight to the input box.  This specific functionality
        /// is a shared event between all of the terminals on the main game tab.
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

        /// <summary>
        /// For handling executing the load plugin menu option via a hot-key.
        /// </summary>
        public static readonly RoutedUICommand LoadPlugin = new RoutedUICommand("LoadPlugin", "LoadPlugin", typeof(MainWindow));

        /// <summary>
        /// For handling executing the select element option via a hot-key.
        /// </summary>
        public static readonly RoutedUICommand SelectElement = new RoutedUICommand("SelectElement", "SelectElement", typeof(MainWindow));

        /// <summary>
        /// For handling executing the edit last item via a hot-key.
        /// </summary>
        public static readonly RoutedUICommand EditLastItem = new RoutedUICommand("EditLastItem", "EditLastItem", typeof(MainWindow));

        /// <summary>
        /// For handling making the terminal font sizes larger and smaller via a hot-key.
        /// </summary>
        public static readonly RoutedUICommand FontSizeChange = new RoutedUICommand("FontSizeChange", "FontSizeChange", typeof(MainWindow));

        /// <summary>
        /// For handling find requests, will behave different depending on the tab selected.
        /// </summary>
        public static readonly RoutedUICommand Find = new RoutedUICommand("Find", "Find", typeof(MainWindow));

        /// <summary>
        /// For handling to focus elements from hot keys.
        /// </summary>
        public static readonly RoutedUICommand FocusByName = new RoutedUICommand("FocusByName", "FocusByName", typeof(MainWindow));

        /// <summary>
        /// For handling hot keys to shell various known windows.
        /// </summary>
        public static readonly RoutedUICommand ShellWindow = new RoutedUICommand("ShellWindow", "ShellWindow", typeof(MainWindow));

        /// <summary>
        /// Event used from menus to shell any number of Shell based UserControls into windows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ShellWindowInternal(object sender, ExecutedRoutedEventArgs e)
        {
            var param = (string)e.Parameter;
            await Utilities.WindowManager.ShellWindowAsync(param);
        }

        /// <summary>
        /// Focuses an known element by name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FocusByNameInternal(object sender, ExecutedRoutedEventArgs e)
        {
            string param = e.Parameter as string;

            if (string.IsNullOrWhiteSpace(param))
            {
                return;
            }

            var element = this.FindDescendantByName(param);
            element?.Focus();
        }

        /// <summary>
        /// Handler to select an element.
        /// </summary>
        private void SelectElementInternal(object sender, ExecutedRoutedEventArgs e)
        {
            var element = this.FindDescendantByName(e.Parameter as string);

            if (element is TabItemEx tabItem)
            {
                tabItem.IsSelected = true;
            }
        }

        /// <summary>
        /// Changes the terminal font size making it one larger or one smaller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeTerminalFontSize(object sender, ExecutedRoutedEventArgs e)
        {
            string param = e.Parameter as string;

            if (string.IsNullOrWhiteSpace(param))
            {
                return;
            }

            if (param.Equals("+", System.StringComparison.OrdinalIgnoreCase))
            {
                if (App.Settings.AvalonSettings.TerminalFontSize < 72)
                {
                    App.Settings.AvalonSettings.TerminalFontSize++;
                }
            }
            else if (param.Equals("-", System.StringComparison.OrdinalIgnoreCase))
            {
                if (App.Settings.AvalonSettings.TerminalFontSize > 8)
                {
                    App.Settings.AvalonSettings.TerminalFontSize--;
                }
            }

            UpdateUISettings();
        }

        /// <summary>
        /// Executes the default code for Finding depending on what context has the focus.
        /// </summary>
        private void FindInternal(object sender, ExecutedRoutedEventArgs e)
        {
            App.MainWindow.TitleBar.SearchBox.Focus();
        }

    }
}