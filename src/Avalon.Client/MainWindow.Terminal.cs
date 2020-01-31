using System.Text;
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
    }
}