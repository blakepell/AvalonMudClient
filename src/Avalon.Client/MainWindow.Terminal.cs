using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Avalon.Colors;
using Avalon.Controls;
using Avalon.Extensions;

namespace Avalon
{
    /// <summary>
    /// Partial class for terminal related events and code of the MainWindow.
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Scrolls all the visible terminal windows to the bottom.
        /// </summary>
        public void ScrollAllToBottom()
        {
            GameTerminal.ScrollToLastLine();
            Terminal1.ScrollToLastLine();
            Terminal2.ScrollToLastLine();
            Terminal3.ScrollToLastLine();
        }

        /// <summary>
        /// Copies any text from the terminal and removes all ANSI escape codes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvalonTerminalContextMenuCopy_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = sender as MenuItem;
                var cm = (ContextMenu)item?.Parent;
                var popup = (Popup)cm?.Parent;

                var terminal = popup?.PlacementTarget as AvalonTerminal;

                if (terminal == null)
                {
                    return;
                }

                // If the SetText crashes more often than not, change SetText to SetDataObject.

                // Remove any ANSI codes from the selected text.
                var sb = new StringBuilder(terminal.SelectedText);
                Colorizer.RemoveAllAnsiCodes(sb);
                Clipboard.SetText(sb.ToString(), TextDataFormat.Text);
            }
            catch (Exception ex)
            {
                this.Interp.Conveyor.EchoLog(ex.Message, Common.Models.LogType.Error);

                if (!string.IsNullOrWhiteSpace(ex.StackTrace))
                {
                    this.Interp.Conveyor.EchoLog(ex.StackTrace, Common.Models.LogType.Error);
                }
            }
        }

        /// <summary>
        /// Copies any text from the terminal and replaces all ANSI escape codes with their known
        /// mud color codes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvalonTerminalContextMenuCopyWithMudColors_OnClick(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                this.Interp.Conveyor.EchoLog(ex.Message, Common.Models.LogType.Error);

                if (!string.IsNullOrWhiteSpace(ex.StackTrace))
                {
                    this.Interp.Conveyor.EchoLog(ex.StackTrace, Common.Models.LogType.Error);
                }
            }
        }

        /// <summary>
        /// Clears all of the text from the terminal window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvalonTerminalContextMenuClear_OnClick(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                this.Interp.Conveyor.EchoLog(ex.Message, Common.Models.LogType.Error);

                if (!string.IsNullOrWhiteSpace(ex.StackTrace))
                {
                    this.Interp.Conveyor.EchoLog(ex.StackTrace, Common.Models.LogType.Error);
                }
            }
        }

        /// <summary>
        /// The ability to send the selected text in the terminal to the regular expression tester.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemTestSelectionRegEx_Click(object sender, RoutedEventArgs e)
        {
            try
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
                // Startup position of the dialog should be in the center of the parent window.  The
                // owner has to be set for this to work.
                var win = new RegexTestWindow
                {
                    Owner = App.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    CancelButtonText = "Close",
                    SaveButtonVisible = false
                };

                // Remove any ANSI codes from the selected text.
                var sb = new StringBuilder(terminal.SelectedText);
                Colorizer.RemoveAllAnsiCodes(sb);
                win.Pattern = sb.ToString();
                win.EscapePattern();
                win.TextBoxTest1.SetText(sb.ToString());

                // Show the Lua dialog.
                win.ShowDialog();
            }
            catch (Exception ex)
            {
                this.Interp.Conveyor.EchoLog(ex.Message, Common.Models.LogType.Error);

                if (!string.IsNullOrWhiteSpace(ex.StackTrace))
                {
                    this.Interp.Conveyor.EchoLog(ex.StackTrace, Common.Models.LogType.Error);
                }
            }
        }

    }
}