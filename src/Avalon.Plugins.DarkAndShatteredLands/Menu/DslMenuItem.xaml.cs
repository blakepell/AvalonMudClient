/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;
using System.Windows.Controls;

namespace Avalon.Plugins.DarkAndShatteredLands
{
    public partial class DslMenuItem : IPluginMenu
    {

        /// <summary>
        /// Gets a casted copy of the interpreter
        /// </summary>
        private IInterpreter GetInterpreter()
        {
            try
            {
                return (IInterpreter)((MenuItem)this["PluginMenu"]).Tag;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Opens the DSL website.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDslWebite_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShellLink("http://www.dsl-mud.org");
        }

        /// <summary>
        /// Opens the DSL forum.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemForum_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShellLink("http://www.dsl-mud.org/forum/default.asp");
        }

        /// <summary>
        /// Opens the DSL Wiki.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemWiki_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShellLink("https://dslmud.fandom.com/wiki/Dslmud_Wiki");
        }

        /// <summary>
        /// Opens the community Google Docs spreadsheet with directions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDirectionsSpreadsheet_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ShellLink("https://docs.google.com/spreadsheets/d/1XRuFO6WLwIgNRoH5XX70frnETlX_ceUDlrpo-NrX12M/edit#gid=0");
        }

        /// <summary>
        /// Shells a link via System.Diagnostics.Process.
        /// </summary>
        /// <param name="url"></param>
        public void ShellLink(string url)
        {
            var link = new Uri(url);

            var psi = new ProcessStartInfo
            {
                FileName = "cmd",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"/c start {link.AbsoluteUri}"
            };

            Process.Start(psi);
        }

        /// <summary>
        /// Checks the status of a con card.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemCheckConCard_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var interp = GetInterpreter();

            if (interp == null)
            {
                return;
            }

            string value = await interp.Conveyor.InputBox("What is the con card code you would like to check the status of?", "Check Con Card");

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            await interp.Send($"#con-card {value}");
        }

        /// <summary>
        /// Show who is online according to the web-site.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItemPlayersOnline_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var interp = GetInterpreter();

            if (interp == null)
            {
                return;
            }

            await interp.Send($"#online");
        }

        /// <summary>
        /// A helper to allow an immortal to create restrings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemRestring_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var interp = GetInterpreter();

            if (interp == null)
            {
                return;
            }
            
            var win = new RestringWindow(interp);
            win.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            win.Show();
        }

        /// <summary>
        /// Shows the OLC VNUM Batch Commands helper window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemOlcVnumBatchCommands_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var interp = GetInterpreter();

            if (interp == null)
            {
                return;
            }

            var win = new OlcVnumBatchCommandsWindow(interp);
            win.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            win.Show();
        }

        /// <summary>
        /// A helper to allow an immortal to create restrings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemLogCreator_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var interp = GetInterpreter();

            if (interp == null)
            {
                return;
            }

            string text = interp.Conveyor.GetSelectedText(Common.Models.TerminalTarget.Main, true);

            // If the selection was null, get all the text in the main window.
            if (string.IsNullOrWhiteSpace(text))
            {
                text = interp.Conveyor.GetText(Common.Models.TerminalTarget.Main, true);
            }

            var win = new LogCreatorWindow(interp, text);
            win.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            win.Show();
        }

    }
}
