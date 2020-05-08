﻿using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Avalon.Colors;
using Avalon.Controls;

namespace Avalon
{
    /// <summary>
    /// Partial class for terminal related events and code of the MainWindow.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Copies any text from the terminal and removes all ANSI escape codes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvalonTerminalContextMenuCopy_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var cm = (ContextMenu)item?.Parent;
            var popup = (Popup)cm?.Parent;

            var terminal = popup?.PlacementTarget as AvalonTerminal;

            if (terminal == null)
            {
                return;
            }

            // Remove any ANSI codes from the selected text.
            var sb = new StringBuilder(terminal.SelectedText);
            Colorizer.RemoveAllAnsiCodes(sb);
            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        /// Copies any text from the terminal and replaces all ANSI escape codes with their known
        /// mud color codes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvalonTerminalContextMenuCopyWithMudColors_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var cm = (ContextMenu)item?.Parent;
            var popup = (Popup)cm?.Parent;

            var terminal = popup?.PlacementTarget as AvalonTerminal;

            if (terminal == null)
            {
                return;
            }

            // Remove any ANSI codes from the selected text.
            var sb = new StringBuilder(terminal.SelectedText);
            Colorizer.AnsiToMudColorCodes(sb);
            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        /// Clears all of the text from the terminal window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvalonTerminalContextMenuClear_OnClick(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var cm = (ContextMenu)item?.Parent;
            var popup = (Popup)cm?.Parent;

            var terminal = popup?.PlacementTarget as AvalonTerminal;

            if (terminal == null)
            {
                return;
            }

            terminal.ClearText();
        }

        /// <summary>
        /// The ability to send the selected text in the terminal to the regular expression tester.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemTestSelectionRegEx_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            var cm = (ContextMenu)item?.Parent;
            var popup = (Popup)cm?.Parent;

            var terminal = popup?.PlacementTarget as AvalonTerminal;

            if (terminal == null)
            {
                return;
            }

            // Set the initial text for the editor.
            var win = new RegexTestWindow();

            // Startup position of the dialog should be in the center of the parent window.  The
            // owner has to be set for this to work.
            win.Owner = App.MainWindow;
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.CancelButtonText = "Close";
            win.SaveButtonVisible = false;

            // Remove any ANSI codes from the selected text.
            var sb = new StringBuilder(terminal.SelectedText);
            Colorizer.RemoveAllAnsiCodes(sb);
            win.Pattern = sb.ToString();

            // Show the Lua dialog.
            win.ShowDialog();
        }

    }
}