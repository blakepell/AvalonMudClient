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
using MahApps.Metro.IconPacks;

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
        /// Gets a variable from the settings.  This is not thread safe, calls to this from outside
        /// threads should run through the Dispatcher.
        /// </summary>
        /// <param name="key"></param>
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
        /// Sets a variable in the settings.  This is not thread safe, calls to this from outside
        /// threads should run through the Dispatcher.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetVariable(string key, string value)
        {
            this.SetVariable(key, value, "");
        }

        /// <summary>
        /// Sets a variable in the settings.  This is not thread safe, calls to this from outside
        /// threads should run through the Dispatcher.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="color">The known color</param>
        public void SetVariable(string key, string value, string color)
        {
            var variable = App.Settings.ProfileSettings.Variables.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

            if (variable != null)
            {
                // Only change the string if the value has changed.
                if (!string.Equals(variable.Value, value, StringComparison.Ordinal))
                {
                    // Only fire the OnChanged event if the variable was changed, we will fire and
                    // forget this Lua script.  This is going to run.  %1 will be the old value and
                    // %2 will be the new value.  In order to have both the old and new value we are
                    // running before the actual update so the variable will won't have changed yet
                    // but will be provided to the script.  We're also (for now) firing and forgetting
                    // the Lua call so we can't be sure the value will consistently be set which is why
                    // we'll do the string replacement here.
                    if (!string.IsNullOrWhiteSpace(variable.OnChangeEvent))
                    {
                        // TODO - Endless loop protection if they set the value of this variable in the Lua script.
                        this.ExecuteLuaAsync(variable.OnChangeEvent.Replace("%1", variable.Value).Replace("%2", value));
                    }

                    variable.Value = value;
                }

                // If the color is specified and has changed then set the new value.  We don't want blank to set
                // the color to nothing other it would always reset user set values if not specified.
                if (!string.IsNullOrWhiteSpace(color) && !string.Equals(variable.ForegroundColor, color, StringComparison.Ordinal))
                {
                    variable.ForegroundColor = color;
                }
            }
            else
            {
                App.Settings.ProfileSettings.Variables.Add(string.IsNullOrWhiteSpace(color)
                    ? new Variable(key, value)
                    : new Variable(key, value, color));
            }
        }

        /// <summary>
        /// Shows the variable if found in the variable repeater.
        /// </summary>
        /// <param name="key"></param>
        public void ShowVariable(string key)
        {
            var variable = App.Settings.ProfileSettings.Variables.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

            if (variable != null)
            {
                variable.IsVisible = true;

                // Because the Linq query changes the ItemsSource the bind will need to be called again.
                App.MainWindow.VariableRepeater.Bind();
            }
        }

        /// <summary>
        /// Hides the variable if found in the variable repeater.
        /// </summary>
        /// <param name="key"></param>
        public void HideVariable(string key)
        {
            var variable = App.Settings.ProfileSettings.Variables.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

            if (variable != null)
            {
                variable.IsVisible = false;

                // Because the Linq query changes the ItemsSource the bind will need to be called again.
                App.MainWindow.VariableRepeater.Bind();
            }
        }

        /// <summary>
        /// Replaces any variables in the provided string with the variable literal value.  This is not thread safe, calls to this from outside
        /// threads should run through the Dispatcher.
        /// </summary>
        /// <param name="text"></param>
        public string ReplaceVariablesWithValue(string text)
        {
            if (text == null)
            {
                return "";
            }

            if (text.Contains('@'))
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
        /// Removes a variable from the settings.  This is not thread safe, calls to this from outside
        /// threads should run through the Dispatcher.
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
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    return App.MainWindow.Title;
                }

                string buf = "";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    buf = App.MainWindow.Title;
                });

                return buf;
            }
            set
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    App.MainWindow.Title = value;
                    App.MainWindow.TitleBar.Title = value;
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Set both the official Windows title of the title bar and the faux custom title bar
                    // that we created on our UI.
                    App.MainWindow.Title = value;
                    App.MainWindow.TitleBar.Title = value;
                });
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
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => EchoText(line, target)));
                return;
            }

            Colorizer.MudToAnsiColorCodes(line.FormattedText);

            switch (target)
            {
                case TerminalTarget.None:
                case TerminalTarget.Main:
                    App.MainWindow.GameTerminal.Append(line);

                    // If the back buffer setting is enabled put the data also in there.
                    if (App.Settings.AvalonSettings.BackBufferEnabled)
                    {
                        line.ScrollToLastLine = false;
                        App.MainWindow.GameBackBufferTerminal.Append(line);
                    }
                    break;
                case TerminalTarget.Terminal1:
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

                    break;
                case TerminalTarget.Terminal2:
                    App.MainWindow.Terminal2.Append(line);

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
                    App.MainWindow.Terminal3.Append(line);

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
        /// Echos text to the specified user spawned terminal window if it exists.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="windowName"></param>
        public void EchoText(string text, string windowName)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => EchoText(text, windowName)));
                return;
            }

            var win = this.WindowList.Find(x => x.WindowType == WindowType.TerminalWindow && x.Name.Equals(windowName, StringComparison.Ordinal)) as TerminalWindow;

            if (win == null)
            {
                this.EchoLog($"The window '{windowName}' was not found.", LogType.Warning);
                return;
            }

            var sb = Argus.Memory.StringBuilderPool.Take(text);
            Colorizer.MudToAnsiColorCodes(sb);
            win.AppendText(sb);
            Argus.Memory.StringBuilderPool.Return(sb);
        }

        /// <summary>
        /// Echos a line to the specified user spawned terminal window if it exists.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="windowName"></param>
        public void EchoText(Line line, string windowName)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => EchoText(line, windowName)));
                return;
            }

            var win = this.WindowList.Find(x => x.WindowType == WindowType.TerminalWindow && x.Name.Equals(windowName, StringComparison.Ordinal)) as TerminalWindow;

            if (win == null)
            {
                this.EchoLog($"The window '{windowName}' was not found.", LogType.Warning);
                return;
            }

            var sb = Argus.Memory.StringBuilderPool.Take(line.FormattedText);
            Colorizer.MudToAnsiColorCodes(sb);
            line.FormattedText.Append(sb);
            Argus.Memory.StringBuilderPool.Return(sb);

            win.AppendText(line);
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
                    line.FormattedText.AppendLine($"{{c={{C>{{x {text}");
                    break;
                case LogType.Success:
                    line.FormattedText.AppendLine($"{{g={{G>{{x {text}");
                    break;
                case LogType.Warning:
                    line.FormattedText.AppendLine($"{{y={{Y>{{x {text}");
                    break;
                case LogType.Error:
                    line.FormattedText.AppendLine($"{{r={{R>{{x {text}");
                    break;
                case LogType.Debug:
                    line.FormattedText.AppendLine($"{{b={{B>{{x {text}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            EchoText(line, TerminalTarget.Main);
        }

        /// <summary>
        /// Echos an information log line.  Shorthand for EchoLog => LogType.Information.
        /// </summary>
        /// <param name="text"></param>
        public void EchoInfo(string text)
        {
            this.EchoLog(text, LogType.Information);
        }

        /// <summary>
        /// Echos a success log line.  Shorthand for EchoLog => LogType.Success.
        /// </summary>
        /// <param name="text"></param>
        public void EchoSuccess(string text)
        {
            this.EchoLog(text, LogType.Success);
        }

        /// <summary>
        /// Echos a warning log line.  Shorthand for EchoLog => LogType.Warning.
        /// </summary>
        /// <param name="text"></param>
        public void EchoWarning(string text)
        {
            this.EchoLog(text, LogType.Warning);
        }

        /// <summary>
        /// Echos an error log line.  Shorthand for EchoLog => LogType.Error.
        /// </summary>
        /// <param name="text"></param>
        public void EchoError(string text)
        {
            this.EchoLog(text, LogType.Error);
        }

        /// <summary>
        /// Echos a debug log line.  Shorthand for EchoDebug => LogType.Debug.
        /// </summary>
        /// <param name="text"></param>
        public void EchoDebug(string text)
        {
            this.EchoLog(text, LogType.Debug);
        }

        /// <summary>
        /// Gets all of the text from the requested window.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="removeColors"></param>
        public string GetText(TerminalTarget target, bool removeColors)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                string buf = "";
                Application.Current.Dispatcher.Invoke(new Action(() => buf = GetText(target, removeColors)));
                return buf;
            }

            var sb = new StringBuilder();

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
        /// <param name="removeColors"></param>
        public string GetSelectedText(TerminalTarget target, bool removeColors)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                string buf = "";
                Application.Current.Dispatcher.Invoke(new Action(() => buf = GetSelectedText(target, removeColors)));
                return buf;
            }

            var sb = new StringBuilder();

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
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetTickTime(value)));
                return;
            }

            App.MainWindow.InfoBar.TickTimer = value;
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
            return GetVariable("GameTime");
        }

        /// <summary>
        /// Clears the contents of the specified terminal.
        /// </summary>
        /// <param name="target"></param>
        public void ClearTerminal(TerminalTarget target)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => ClearTerminal(target)));
                return;
            }

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
            ITrigger trigger = App.Settings.ProfileSettings.TriggerList.Find(x => x.Identifier.Equals(id, StringComparison.Ordinal));

            // If the trigger is null, go through the system triggers.
            if (trigger == null)
            {
                trigger = App.SystemTriggers.Find(x => x.Identifier.Equals(id, StringComparison.Ordinal));
            }

            return trigger;
        }

        /// <summary>
        /// Prompts the user with an input box and returns the string content.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="title"></param>
        public async Task<string> InputBox(string caption, string title)
        {
            var win = new InputBoxDialog
            {
                Title = title,
                Caption = caption
            };

            _ = await win.ShowAsync();

            return win.Text;
        }

        /// <summary>
        /// Prompts the user with an input box and returns the string content.
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="title"></param>
        /// <param name="prepopulateText"></param>
        public async Task<string> InputBox(string caption, string title, string prepopulateText)
        {
            var win = new InputBoxDialog
            {
                Title = title,
                Caption = caption,
                Text = prepopulateText
            };

            _ = await win.ShowAsync();

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

            string value = this.GetVariable(variable);

            if (!string.IsNullOrWhiteSpace(value))
            {
                win.Text = value;
            }

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
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Focus(target)));
                return;
            }

            switch (target)
            {
                case FocusTarget.Input:
                    App.MainWindow.TextInput.Editor.Focus();
                    break;
            }
        }

        /// <summary>
        /// Clears the default progress bar repeater.
        /// </summary>
        public void ProgressBarRepeaterClear()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(ProgressBarRepeaterClear));
                return;
            }

            App.MainWindow.BarRepeater.Clear();
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
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => ProgressBarRepeaterAdd(key, value, maximum, text)));
                return;
            }

            App.MainWindow.BarRepeater.Add(value, maximum, text, key);
        }

        public void ProgressBarRepeaterAdd(string key, int value, int maximum, string text, string command)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => ProgressBarRepeaterAdd(key, value, maximum, text, command)));
                return;
            }

            App.MainWindow.BarRepeater.Add(value, maximum, text, key, command);
        }

        /// <summary>
        /// Removes at item from the default progress bar repeater.
        /// </summary>
        /// <param name="key"></param>
        public void ProgressBarRemove(string key)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => ProgressBarRemove(key)));
                return;
            }

            App.MainWindow.BarRepeater.Remove(key);
        }

        /// <summary>
        /// Sets the visibility of the specified custom tab.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="visible"></param>
        public void SetCustomTabVisible(CustomTab tab, bool visible)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetCustomTabVisible(tab, visible)));
                return;
            }

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
        }

        /// <summary>
        /// Sets the label for the specified custom tab.
        /// </summary>
        /// <param name="tab"></param>
        /// <param name="label"></param>
        public void SetCustomTabLabel(CustomTab tab, string label)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetCustomTabLabel(tab, label)));
                return;
            }

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
        }

        /// <summary>
        /// Sets the text on a supported UI text element.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="target"></param>
        /// <param name="icon"></param>
        public void SetText(string buf, TextTarget target = TextTarget.StatusBarText, PackIconMaterialKind icon = PackIconMaterialKind.None)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetText(buf, target, icon)));
                return;
            }

            switch (target)
            {
                case TextTarget.StatusBarText:
                    App.MainWindow.ViewModel.StatusBarText = buf ?? "";
                    break;
                default:
                    App.Conveyor.EchoError($"SetText: TextTarget {(int)target} was not found.");
                    break;
            }

            this.SetIcon(icon, target);
        }

        /// <summary>
        /// Sets the associated <see cref="PackIconMaterialKind"/> to the <see cref="TextTarget"/>.
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="target"></param>
        public void SetIcon(PackIconMaterialKind icon = PackIconMaterialKind.None, TextTarget target = TextTarget.StatusBarText)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetIcon(icon, target)));
                return;
            }

            try
            {
                switch (target)
                {
                    case TextTarget.StatusBarText:
                        App.MainWindow.ViewModel.StatusBarTextIconVisibility = icon == PackIconMaterialKind.None ? Visibility.Collapsed : Visibility.Visible;
                        App.MainWindow.ViewModel.StatusBarTextIconKind = icon;
                        break;
                    default:
                        App.Conveyor.EchoError($"SetIcon: TextTarget {(int)target} was not found.");
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoError($"An error occurred setting a status bar icon: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns information about the current WindowPosition.
        /// </summary>
        public WindowPosition GetWindowPosition
        {
            get
            {
                // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    WindowPosition wp = null;
                    Application.Current.Dispatcher.Invoke(new Action(() => wp = GetWindowPosition));
                    return wp;
                }

                // I'm not sure why it didn't behave like this before, but when I changed the
                // Window chrome it became apparent that I shouldn't save the window position if
                // it was maximized.
                if (App.MainWindow.WindowState == WindowState.Maximized)
                {
                    return App.Settings.AvalonSettings.LastWindowPosition;
                }

                return new WindowPosition
                {
                    Left = App.MainWindow.Left,
                    Top = App.MainWindow.Top,
                    Height = App.MainWindow.Height,
                    Width = App.MainWindow.Width
                };
            }
        }

        /// <summary>
        /// Status bar text on the main progress bar repeater.
        /// </summary>
        public string ProgressBarRepeaterStatusText
        {
            get
            {
                // If it has access directly return it.
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    return App.MainWindow.BarRepeater.StatusText;
                }

                string text = "";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    text = App.MainWindow.BarRepeater.StatusText;
                });

                return text;
            }
            set
            {
                // If it has access directly set it.
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    App.MainWindow.BarRepeater.StatusText = value;
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.MainWindow.BarRepeater.StatusText = value;
                });
            }
        }

        /// <summary>
        /// Whether the status bar is visible on the main progress bar repeater.
        /// </summary>
        public bool ProgressBarRepeaterStatusVisible
        {
            get
            {
                // If it has access directly return it.
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    return App.MainWindow.BarRepeater.StatusBarVisible;
                }

                bool visible = false;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    visible = App.MainWindow.BarRepeater.StatusBarVisible;
                });

                return visible;
            }
            set
            {
                // If it has access directly set it
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    App.MainWindow.BarRepeater.StatusBarVisible = value;
                    return;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    App.MainWindow.BarRepeater.StatusBarVisible = value;
                });
            }
        }

        /// <summary>
        /// Sorts the triggers by priority.
        /// </summary>
        public void SortTriggersByPriority()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(SortTriggersByPriority));
                return;
            }

            // Sort the original list by the provided function.
            var sortedList = new List<Common.Triggers.Trigger>(App.Settings.ProfileSettings.TriggerList).OrderBy(x => x.Priority);

            // Clear the original list.
            App.Settings.ProfileSettings.TriggerList.Clear();

            // Repopulate the original list with the contents of the new sorted list.
            foreach (var item in sortedList)
            {
                App.Settings.ProfileSettings.TriggerList.Add(item);
            }
        }

        /// <summary>
        /// Sends a command immediately to the game and does not await the return;
        /// </summary>
        /// <param name="cmd">The command or commands to send to the game.</param>
        public void ExecuteCommand(string cmd)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => ExecuteCommand(cmd)));
                return;
            }

            App.MainWindow.Interp.Send(cmd);
        }

        /// <summary>
        /// Sends a command to the game and awaits the async call.
        /// </summary>
        /// <param name="cmd">The command or commands to send to the game.</param>
        public async Task ExecuteCommandAsync(string cmd)
        {
            // If it has access directly send it and return, otherwise we'll use the dispatcher to queue it up on the UI thread.
            if (Application.Current.Dispatcher.CheckAccess())
            {
                await App.MainWindow.Interp.Send(cmd);
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(new Action(async () =>
            {
                await App.MainWindow.Interp.Send(cmd);
            }));
        }

        /// <summary>
        /// Executes a Lua script on the UI thread.
        /// </summary>
        /// <param name="lua"></param>
        public void ExecuteLua(string lua)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => ExecuteLua(lua)));
                return;
            }

            App.MainWindow.Interp.LuaCaller.Execute(lua);
        }

        /// <summary>
        /// Executes a Lua script on the UI thread.
        /// </summary>
        /// <param name="lua"></param>
        public async Task ExecuteLuaAsync(string lua)
        {
            // If it has access directly send it and return, otherwise we'll use the dispatcher to queue it up on the UI thread.
            if (Application.Current.Dispatcher.CheckAccess())
            {
                await App.MainWindow.Interp.LuaCaller.ExecuteAsync(lua);
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(new Action(async () =>
            {
                // This is all that's going to execute as it clears the list.. we can "fire and forget".
                await App.MainWindow.Interp.LuaCaller.ExecuteAsync(lua);
            }));
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
        /// Returns the client settings that are specific to the workstation
        /// </summary>        
        public AvalonSettings ClientSettings => App.Settings.AvalonSettings;

        /// <summary>
        /// A list of user spawned windows.  These can represent any number of <see cref="WindowType"/> objects.
        /// </summary>
        public List<IWindow> WindowList { get; set; } = new List<IWindow>();

    }
}