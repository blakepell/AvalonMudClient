using Argus.Extensions;
using Avalon.Common.Interfaces;
using Avalon.Common.Settings;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using Avalon.Common.Colors;
using Avalon.Extensions;
using Avalon.Common.Models;
using System.Threading.Tasks;
using Avalon.Colors;

namespace Avalon
{

    /// <summary>
    /// WPF Implementation of the IConveyor for UI interactions and specific platform implementations.  This will
    /// allow everything from LUA scripts to plugins to have access to parts of the program that they need access
    /// to function.
    /// </summary>
    public class Conveyor : IConveyor
    {

        /// <summary>
        /// Gets a variable from the settings (TODO: character)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetVariable(string key)
        {
            var variable = App.Settings.ProfileSettings.Variables.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

            if (variable != null)
            {
                return variable.Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Sets a variable in the settings (TODO: character)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetVariable(string key, string value)
        {
            var variable = App.Settings.ProfileSettings.Variables.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

            if (variable != null)
            {
                variable.Value = value;
            }
            else
            {
                App.Settings.ProfileSettings.Variables.Add(new Variable(key, value));
            }
        }

        /// <summary>
        /// Replaces any variables in the provided string with the variable literal value.
        /// </summary>
        /// <param name="text"></param>
        public string ReplaceVariablesWithValue(string text)
        {
            if (text == null)
            {
                return "";
            }

            if (text.Contains("@"))
            {
                var sb = Argus.Memory.StringBuilderPool.Take();

                try
                {
                    sb.Append(text);

                    foreach (var item in App.Settings.ProfileSettings.Variables)
                    {
                        sb.Replace($"@{item.Key}", item.Value);
                    }

                    return sb.ToString();
                }
                finally
                {
                    Argus.Memory.StringBuilderPool.Return(sb);
                }
            }

            return text;
        }

        /// <summary>
        /// Removes a variable from the settings (TODO: character)
        /// </summary>
        /// <param name="key"></param>
        public void RemoveVariable(string key)
        {
            for (int i = App.Settings.ProfileSettings.Variables.Count - 1; i >= 0; i--)
            {
                if (App.Settings.ProfileSettings.Variables[i].Key == key)
                {
                    App.Settings.ProfileSettings.Variables.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// The title of the main window.
        /// </summary>
        public string Title
        {
            get
            {
                string buf = "";

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    buf = App.MainWindow.Title;
                }));

                return buf;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    App.MainWindow.Title = value;
                }));
            }
        }

        /// <summary>
        /// Writes output to the main terminal window.
        /// </summary>
        /// <param name="text"></param>
        public void EchoText(string text)
        {
            EchoText(text.ToLine(), TerminalTarget.Main);
        }

        /// <summary>
        /// Writes output to the main terminal window.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="terminal"></param>
        public void EchoText(string text, AnsiColor foregroundColor, TerminalTarget terminal)
        {
            var line = text.ToLine();
            line.ForegroundColor = foregroundColor;
            line.IgnoreLastColor = true;
            EchoText(line, terminal);
        }

        /// <summary>
        /// Writes output to the specified terminal window.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="target"></param>
        public void EchoText(string text, TerminalTarget target)
        {
            EchoText(text.ToLine(), target);
        }

        /// <summary>
        /// Writes output to the specified terminal window.  This procedure writes directly to the
        /// terminal and does not go through processing as when other data comes in through the OnEcho
        /// event (that gets processed for triggers).  We don't want triggers processed from here.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="target"></param>
        public void EchoText(Line line, TerminalTarget target)
        {
            switch (target)
            {
                case TerminalTarget.None:
                case TerminalTarget.Main:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.GameTerminal.Append(line);
                        
                        // If the back buffer setting is enabled put the data also in there.
                        if (App.Settings.AvalonSettings.BackBufferEnabled)
                        {
                            line.ScrollToLastLine = false;
                            App.MainWindow.GameBackBufferTerminal.Append(line);
                        }

                    }));

                    break;
                case TerminalTarget.Communication:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.CommunicationTerminal.Append(line);
                    }));

                    break;
                case TerminalTarget.OutOfCharacterCommunication:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.OocCommunicationTerminal.Append(line);
                    }));

                    break;
            }
        }

        /// <summary>
        /// Echos a log line to the terminal.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public void EchoLog(string text, LogType type)
        {
            var line = new Line
            {
                IgnoreLastColor = true,
                ForegroundColor = AnsiColors.LightGray
            };

            switch (type)
            {
                case LogType.Information:
                    line.FormattedText = $"[  {AnsiColors.Green}Info   {AnsiColors.LightGray}] {text}\r\n";
                    break;
                case LogType.Success:
                    line.FormattedText = $"[ {AnsiColors.Green}Success {AnsiColors.LightGray}] {text}\r\n";
                    break;
                case LogType.Warning:
                    line.FormattedText = $"[ {AnsiColors.Yellow}Warning {AnsiColors.LightGray}] {text}\r\n";
                    break;
                case LogType.Error:
                    line.FormattedText = $"[  {AnsiColors.Red}Error  {AnsiColors.LightGray}] {text}\r\n";
                    break;
                case LogType.Debug:
                    line.FormattedText = $"[  {AnsiColors.Blue}Debug  {AnsiColors.LightGray}] {text}\r\n";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            EchoText(line, TerminalTarget.Main);
        }

        /// <summary>
        /// Gets all of the text from the requested window.
        /// </summary>
        /// <param name="target"></param>
        public string GetText(TerminalTarget target, bool removeColors)
        {
            var sb = new StringBuilder();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                switch (target)
                {
                    case TerminalTarget.Main:
                        sb.Append(App.MainWindow.GameTerminal.Text);                       
                        break;
                    case TerminalTarget.Communication:
                        sb.Append(App.MainWindow.CommunicationTerminal.Text);
                        break;
                    case TerminalTarget.OutOfCharacterCommunication:
                        sb.Append(App.MainWindow.OocCommunicationTerminal.Text);
                        break;
                }
            }));

            if (removeColors)
            {
                Colorizer.RemoveAllAnsiCodes(sb);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the selected text from the requested window.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public string GetSelectedText(TerminalTarget target, bool removeColors)
        {
            var sb = new StringBuilder();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                switch (target)
                {
                    case TerminalTarget.Main:
                        sb.Append(App.MainWindow.GameTerminal.SelectedText);
                        break;
                    case TerminalTarget.Communication:
                        sb.Append(App.MainWindow.CommunicationTerminal.SelectedText);
                        break;
                    case TerminalTarget.OutOfCharacterCommunication:
                        sb.Append(App.MainWindow.OocCommunicationTerminal.SelectedText);
                        break;
                }
            }));

            if (removeColors)
            {
                Colorizer.RemoveAllAnsiCodes(sb);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Sets the seconds left on the tick timer on the main InfoBar.
        /// </summary>
        /// <param name="value"></param>
        public void SetTickTime(int value)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                App.MainWindow.InfoBar.TickTimer = value;
            }));
        }

        /// <summary>
        /// Gets the seconds left on the tick timer on the main InfoBar.
        /// </summary>
        /// <returns></returns>
        public int GetTickTime()
        {
            return App.MainWindow.InfoBar.TickTimer;
        }

        /// <summary>
        /// Gets the game time from the main InfoBar.
        /// </summary>
        /// <returns></returns>
        public string GetGameTime()
        {
            return App.MainWindow.InfoBar.Time;
        }

        /// <summary>
        /// Clears the contents of the specified terminal.
        /// </summary>
        /// <param name="target"></param>
        public void ClearTerminal(TerminalTarget target)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                switch (target)
                {
                    case TerminalTarget.None:
                        break;
                    case TerminalTarget.Main:
                        App.MainWindow.GameTerminal.Text = "";
                        break;
                    case TerminalTarget.Communication:
                        App.MainWindow.CommunicationTerminal.Text = "";
                        break;
                    case TerminalTarget.OutOfCharacterCommunication:
                        App.MainWindow.OocCommunicationTerminal.Text = "";
                        break;
                    case TerminalTarget.BackBuffer:
                        App.MainWindow.GameBackBufferTerminal.Text = "";
                        break;
                }
            }));
        }

        /// <summary>
        /// The number of lines in a given terminal.
        /// </summary>
        /// <param name="target"></param>
        public int LineCount(TerminalTarget target)
        {
            switch (target)
            {
                case TerminalTarget.None:
                    return 0;
                case TerminalTarget.Main:
                    return App.MainWindow.GameTerminal.LineCount;
                case TerminalTarget.Communication:
                    return App.MainWindow.CommunicationTerminal.LineCount;
                case TerminalTarget.OutOfCharacterCommunication:
                    return App.MainWindow.OocCommunicationTerminal.LineCount;
                case TerminalTarget.BackBuffer:
                    return App.MainWindow.GameBackBufferTerminal.LineCount;
            }

            return 0;
        }

        /// <summary>
        /// Enables all aliases and triggers in a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>Returns true if the group was found, false if it was not.</returns>
        public bool EnableGroup(string groupName)
        {
            bool found = false;

            foreach (var item in App.Settings.ProfileSettings.TriggerList)
            {
                if (string.Equals(item.Group, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    item.Enabled = true;
                }
            }

            foreach (var item in App.Settings.ProfileSettings.AliasList)
            {
                if (string.Equals(item.Group, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    item.Enabled = true;
                }
            }

            return found;
        }

        /// <summary>
        /// Disables all aliases and triggers in a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>Returns true if the group was found, false if it was not.</returns>
        public bool DisableGroup(string groupName)
        {
            bool found = false;

            foreach (var item in App.Settings.ProfileSettings.TriggerList)
            {
                if (string.Equals(item.Group, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    item.Enabled = false;
                }
            }

            foreach (var item in App.Settings.ProfileSettings.AliasList)
            {
                if (string.Equals(item.Group, groupName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    item.Enabled = false;
                }
            }

            return found;
        }

        /// <summary>
        /// Imports a JSON package into the currently loaded profile.
        /// </summary>
        /// <param name="json"></param>
        public void ImportPackageFromJson(string json)
        {
            App.Settings.ImportPackageFromJson(json);
        }

        /// <summary>
        /// Prompts the user with an input box and returns the string content.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public async Task<string> InputBox(string caption, string title)
        {
            var win = new InputBoxDialog
            {
                Title = title,
                Caption = caption
            };

            var result = await win.ShowAsync();

            return win.Text;
        }

        /// <summary>
        /// Prompts a dialog box and sets a variable with the result when it's complete.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="title"></param>
        /// <param name="variable"></param>
        public async void InputBoxToVariable(string caption, string title, string variable)
        {
            var win = new InputBoxDialog
            {
                Title = title,
                Caption = caption
            };

            var result = await win.ShowAsync();
            
            if (result == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                // Cancel
                return;
            }
            else if (result == ModernWpf.Controls.ContentDialogResult.Secondary)
            {
                // OK
                this.SetVariable(variable, win.Text);
            }
        }

        /// <summary>
        /// Sets the focus to the given UI element.
        /// </summary>
        /// <param name="target"></param>
        public void Focus(FocusTarget target)
        {
            switch (target)
            {
                case FocusTarget.Input:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.TextInput.Editor.Focus();
                    }));

                    break;
            }
        }

        /// <summary>
        /// Returns information about the current WindowPosition.
        /// </summary>
        public WindowPosition GetWindowPosition
        {
            get
            {
                var win = new WindowPosition
                {
                    Left = App.MainWindow.Left,
                    Top = App.MainWindow.Top,
                    Height = App.MainWindow.Height,
                    Width = App.MainWindow.Width
                };

                return win;
            }
        }

        /// <summary>
        /// A StringBuilder for holding scraped data.
        /// </summary>
        public StringBuilder Scrape { get; set; } = new StringBuilder();

        /// <summary>
        /// Whether the main terminal should be scraping data into the Scrape StringBuilder for later use.
        /// </summary>
        public bool ScrapeEnabled { get; set; } = false;

        /// <summary>
        /// The settings for the current profile that is loaded.
        /// </summary>
        public ProfileSettings ProfileSettings => App.Settings.ProfileSettings;
    }
}
