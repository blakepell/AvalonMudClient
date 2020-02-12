using System;
using System.Text;
using System.Windows;
using Avalon.Colors;
using Avalon.Common.Colors;
using Avalon.Common.Models;
using Avalon.Controls;
using Avalon.Utilities;

namespace Avalon
{
    /// <summary>
    /// Partial class for network related events and code of the MainWindow.
    /// </summary>
    public partial class MainWindow
    {

        /// <summary>
        /// Event for when the network button is clicked.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        private void NetworkButton_Click(object o, RoutedEventArgs args)
        {
            try
            {
                if (TabMain.IsConnected == false)
                {
                    Connect();
                }
                else
                {
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                GameTerminal.Append($"ERROR: {ex.Message}", AnsiColors.Red);
            }

        }

        /// <summary>
        /// Connects to the server for the profile that is currently loaded.
        /// </summary>
        public void Connect()
        {
            // Load any plugins into the System Triggers.
            LoadPlugins();

            // Connect, then put the focus into the input text box.
            Interp.Connect(HandleLineReceived, this.HandleDataReceived, HandleConnectionClosed);
            TabMain.IsConnected = true;
            TextInput.Focus();
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            Interp.Disconnect();
            TabMain.IsConnected = false;
        }

        /// <summary>
        /// Handles when a connection is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleConnectionClosed(object sender, EventArgs e)
        {
            TabMain.IsConnected = false;
            App.Conveyor.EchoLog($"Disconnected: {DateTime.Now}", LogType.Warning);
            Interp.Telnet = null;
        }

        /// <summary>
        /// This fires when a complete line has been sent from the mud.  It has already been rendered
        /// to the terminal window at this point so we want to do processing such as triggers at this
        /// point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleLineReceived(object sender, string e)
        {
            var line = new Line
            {
                FormattedText = e,
                Text = Colorizer.RemoveAllAnsiCodes(e),
            };

            CheckTriggers(line);

            // Check to see if we're scraping and if so append to the StringBuilder that holds the scraped text.
            // Scraping will only occur when full lines come through.
            if (App.Conveyor.ScrapeEnabled)
            {
                App.Conveyor.Scrape.AppendLine(line.Text);
            }
        }

        /// <summary>
        /// This is fired anytime new data is received whether it is a newline or not.  This will allow us to
        /// start putting that data into the window so the user can see it (even if the trigger can't fire until
        /// the full line is available).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleDataReceived(object sender, string e)
        {
            try
            {
                // Remove unwanted characters
                var sb = new StringBuilder(e);
                sb.RemoveUnsupportedCharacters();

                // Append the data to the terminal as it comes in.
                GameTerminal.Append(sb.ToString());
            }
            catch (Exception ex)
            {
                GameTerminal.Append($"ERROR: {ex.Message}", AnsiColors.Red);
            }
        }

        /// <summary>
        /// Handles text from the interpreter that needs to be echo'd out to the terminal.  This text
        /// does not get checked for triggers, it's an echo that's coming from the interpreter or
        /// the possibly the Conveyor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InterpreterEcho(object sender, EventArgs e)
        {
            var ea = e as EchoEventArgs;
            AvalonTerminal term;

            if (ea == null)
            {
                GameTerminal.Append("--> Error: Null EchoEventArgs in InterpreterEcho", AnsiColors.Red);
                return;
            }

            switch (ea.Terminal)
            {
                case TerminalTarget.None:
                case TerminalTarget.Main:
                    term = GameTerminal;
                    break;
                case TerminalTarget.Communication:
                    term = CommunicationTerminal;
                    break;
                case TerminalTarget.OutOfCharacterCommunication:
                    term = OocCommunicationTerminal;
                    break;
                default:
                    term = GameTerminal;
                    break;
            }

            if (ea.UseDefaultColors)
            {
                term.Append(ea.Text);
            }
            else
            {
                term.Append(ea.Text, ea.ForegroundColor, ea.ReverseColors);
            }
        }

    }
}
