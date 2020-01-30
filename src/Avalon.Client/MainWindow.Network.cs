using System;
using System.Text;
using System.Windows;
using Avalon.Common.Colors;
using Avalon.Common.Models;
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
            GameTerminal.Append($"Disconnected: {DateTime.Now.ToString()}\r\n", AnsiColors.Cyan);
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
        /// Handles text from the interpreter that needs to be echo'd out to the terminal.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InterpreterEcho(object sender, EventArgs e)
        {
            var ea = e as EchoEventArgs;

            if (ea.UseDefaultColors)
            {
                GameTerminal.Append(ea.Text);
            }
            else
            {
                GameTerminal.Append(ea.Text, ea.ForegroundColor, ea.ReverseColors);
            }
        }

    }
}
