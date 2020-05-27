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
using System.Collections.Generic;

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
                case TerminalTarget.Terminal1:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.Terminal1.Append(line);

                        if (!App.MainWindow.CustomTab1.IsSelected)
                        {
                            App.MainWindow.CustomTab1Badge.Value += 1;
                        }
                        else if (App.MainWindow.CustomTab1.IsSelected && App.MainWindow.CustomTab1Badge.Value != 0)
                        {
                            // Only setting this if the value isn't 0 so it doesn't trigger UI processing.
                            App.MainWindow.CustomTab1Badge.Value = 0;
                        }
                    }));

                    break;
                case TerminalTarget.Terminal2:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.Terminal2.Append(line);
                    }));

                    if (!App.MainWindow.CustomTab2.IsSelected)
                    {
                        App.MainWindow.CustomTab2Badge.Value += 1;
                    }
                    else if (App.MainWindow.CustomTab2.IsSelected && App.MainWindow.CustomTab2Badge.Value != 0)
                    {
                        // Only setting this if the value isn't 0 so it doesn't trigger UI processing.
                        App.MainWindow.CustomTab2Badge.Value = 0;
                    }

                    break;
                case TerminalTarget.Terminal3:
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        App.MainWindow.Terminal3.Append(line);
                    }));

                    if (!App.MainWindow.CustomTab3.IsSelected)
                    {
                        App.MainWindow.CustomTab3Badge.Value += 1;
                    }
                    else if (App.MainWindow.CustomTab3.IsSelected && App.MainWindow.CustomTab3Badge.Value != 0)
                    {
                        // Only setting this if the value isn't 0 so it doesn't trigger UI processing.
                        App.MainWindow.CustomTab3Badge.Value = 0;
                    }

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
                    case TerminalTarget.Terminal1:
                        sb.Append(App.MainWindow.Terminal1.Text);
                        break;
                    case TerminalTarget.Terminal2:
                        sb.Append(App.MainWindow.Terminal2.Text);
                        break;
                    case TerminalTarget.BackBuffer:
                        sb.Append(App.MainWindow.GameBackBufferTerminal.Text);
                        break;
                    case TerminalTarget.Terminal3:
                        sb.Append(App.MainWindow.Terminal3.Text);
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
                    case TerminalTarget.Terminal1:
                        sb.Append(App.MainWindow.Terminal1.SelectedText);
                        break;
                    case TerminalTarget.Terminal2:
                        sb.Append(App.MainWindow.Terminal2.SelectedText);
                        break;
                    case TerminalTarget.BackBuffer:
                        sb.Append(App.MainWindow.GameBackBufferTerminal.SelectedText);
                        break;
                    case TerminalTarget.Terminal3:
                        sb.Append(App.MainWindow.Terminal3.SelectedText);
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
                        App.MainWindow.GameTerminal.ClearText();
                        break;
                    case TerminalTarget.Terminal1:
                        App.MainWindow.Terminal1.ClearText();
                        break;
                    case TerminalTarget.Terminal2:
                        App.MainWindow.Terminal2.ClearText();
                        break;
                    case TerminalTarget.BackBuffer:
                        App.MainWindow.GameBackBufferTerminal.ClearText();
                        break;
                    case TerminalTarget.Terminal3:
                        App.MainWindow.Terminal3.ClearText();
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
                case TerminalTarget.Terminal1:
                    return App.MainWindow.Terminal1.LineCount;
                case TerminalTarget.Terminal2:
                    return App.MainWindow.Terminal2.LineCount;
                case TerminalTarget.BackBuffer:
                    return App.MainWindow.GameBackBufferTerminal.LineCount;
                case TerminalTarget.Terminal3:
                    return App.MainWindow.Terminal3.LineCount;
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
        /// Returns a trigger for the specified id.  If no trigger is found a null is returned.
        /// </summary>
        /// <param name="id"></param>
        public ITrigger FindTrigger(string id)
        {
            ITrigger trigger = App.Settings.ProfileSettings.TriggerList.FirstOrDefault(x => x.Identifier.Equals(id, StringComparison.Ordinal));

            // If the trigger is null, go through the system triggers.
            if (trigger == null)
            {
                trigger = App.SystemTriggers.FirstOrDefault(x => x.Identifier.Equals(id, StringComparison.Ordinal));
            }

            return trigger;
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
        /// Prompts the user with an input box and returns the string content.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public async Task<string> InputBox(string caption, string title, string prepopulateText)
        {
            var win = new InputBoxDialog
            {
                Title = title,
                Caption = caption,
                Text = prepopulateText
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
        /// Clears the default progress bar repeater.
        /// </summary>
        public void ProgressBarRepeaterClear()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                App.MainWindow.BarRepeater.Clear();
            }));
        }

        /// <summary>
        /// Clears a progress bar with the specified name if it's found.
        /// </summary>
        /// <param name="progressBarName"></param>
        public void ProgressBarRepeaterClear(string progressBarName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds an item to the default progress bar repeater.  If the key exists it will be updated.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="maximum"></param>
        /// <param name="text"></param>
        public void ProgressBarRepeaterAdd(string key, int value, int maximum, string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                App.MainWindow.BarRepeater.Add(value, maximum, text, key);
            }));
        }

        /// <summary>
        /// Adds an item to the default progress bar repeater if it's found.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximum"></param>
        /// <param name="text"></param>
        /// <param name="progressBarName"></param>
        public void ProgressBarRepeaterAdd(string key, int value, int maximum, string text, string progressBarName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes at item from the default progress bar repeater.
        /// </summary>
        /// <param name="key"></param>
        public void ProgressBarRemove(string key)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                App.MainWindow.BarRepeater.Remove(key);
            }));
        }

        /// <summary>
        /// Removes at item from the default progress bar repeater if it's found.
        /// </summary>
        /// <param name="key"></param>
        public void ProgressBarRemove(string key, string progressBarName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the visibility of the specified custom tab.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="visible"></param>
        public void SetCustomTabVisible(CustomTab tab, bool visible)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                switch (tab)
                {
                    case CustomTab.Tab1:
                        App.MainWindow.CustomTab1.Visibility = visible.ToVisibleOrCollapse();
                        break;
                    case CustomTab.Tab2:
                        App.MainWindow.CustomTab2.Visibility = visible.ToVisibleOrCollapse();
                        break;
                    case CustomTab.Tab3:
                        App.MainWindow.CustomTab3.Visibility = visible.ToVisibleOrCollapse();
                        break;
                }
            }));
        }

        /// <summary>
        /// Sets the label for the specified custom tab.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="label"></param>
        public void SetCustomTabLabel(CustomTab tab, string label)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                switch (tab)
                {
                    case CustomTab.Tab1:
                        App.MainWindow.CustomTab1Label.Content = label;
                        App.Settings.AvalonSettings.CustomTab1Label = label;
                        break;
                    case CustomTab.Tab2:
                        App.MainWindow.CustomTab2Label.Content = label;
                        App.Settings.AvalonSettings.CustomTab2Label = label;
                        break;
                    case CustomTab.Tab3:
                        App.MainWindow.CustomTab3Label.Content = label;
                        App.Settings.AvalonSettings.CustomTab3Label = label;
                        break;
                }
            }));
        }

        /// <summary>
        /// Echos text to the specified user spawned terminal window if it exists.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="windowName"></param>
        public void EchoText(string text, string windowName)
        {
            var win = this.TerminalWindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));
            
            if (win == null)
            {
                return;
            }

            win.AppendText(text);
        }

        /// <summary>
        /// Echos a line to the specified user spawned terminal window if it exists.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="windowName"></param>
        public void EchoText(Line line, string windowName)
        {
            var win = this.TerminalWindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            if (win == null)
            {
                return;
            }

            win.AppendText(line);
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
        /// Status bar text on the main progress bar repeater.
        /// </summary>
        public string ProgressBarRepeaterStatusText
        {
            get 
            {
                string text = "";

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    text = App.MainWindow.BarRepeater.StatusText;
                }));

                return text;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    App.MainWindow.BarRepeater.StatusText = value;
                }));
            }
        }

        /// <summary>
        /// Whether the status bar is visible on the main progress bar repeater.
        /// </summary>
        public bool ProgressBarRepeaterStatusVisible
        {
            get
            {
                bool visible = false;

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    visible = App.MainWindow.BarRepeater.StatusBarVisible;
                }));

                return visible;
            }
            set
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    App.MainWindow.BarRepeater.StatusBarVisible = value;
                }));
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

        /// <summary>
        /// Retuns the client settings that are specific to the workstation
        /// </summary>        
        public AvalonSettings ClientSettings => App.Settings.AvalonSettings;

        /// <summary>
        /// A list of user spawned terminal windows that can be written to.
        /// </summary>
        public List<ITerminalWindow> TerminalWindowList { get; set; } = new List<ITerminalWindow>();

    }
}
