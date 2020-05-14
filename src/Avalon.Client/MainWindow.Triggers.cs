using Avalon.Controls;
using System;
using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Models;

namespace Avalon
{
    /// <summary>
    /// Partial class for trigger related events and code of the MainWindow.
    /// </summary>
    public partial class MainWindow
    {

        /// <summary>
        /// Checks a line to see if any Triggers should fire and if so executes those triggers.
        /// </summary>
        /// <param name="line"></param>
        public async void CheckTriggers(Line line)
        {
            // Don't process if the user has disabled triggers.
            if (!App.Settings.ProfileSettings.TriggersEnabled)
            {
                return;
            }

            // Don't process if the line is blank.
            if (string.IsNullOrWhiteSpace(line.Text))
            {
                return;
            }

            // Go through the immutable system triggers, system triggers are silent in that
            // they won't echo to the terminal window, they also don't adhere to attributes like
            // character or enabled.  These can and will have CLR implementations and can be loaded
            // from other DLL's as plugins.  System triggers are also unique in that they are designed
            // to be loaded from a plugin and they don't save their state in the profile.
            foreach (var item in App.SystemTriggers)
            {
                // Skip it if it's not enabled.
                if (!item.Enabled)
                {
                    continue;
                }
                
                if (item.IsMatch(line.Text))
                {
                    // Run any CLR that might exist.
                    item.Execute();

                    if (!string.IsNullOrEmpty(item.Command) && item.IsLua == false)
                    {
                        // If it has text but it's not lua, send it to the interpreter.
                        await Interp.Send(item.Command, item.IsSilent, false);
                    }
                    else if (!string.IsNullOrEmpty(item.Command) && item.IsLua == true)
                    {
                        // If it has text and it IS lua, send it to the LUA engine.
                        await Interp.LuaCaller.ExecuteAsync(item.Command);
                    }

                    // Check if we're supposed to move this line somewhere else.
                    if (item.MoveTo != TerminalTarget.None)
                    {
                        string timeStamp;

                        switch (App.Settings.AvalonSettings.TimestampFormat)
                        {
                            case Common.Settings.AvalonSettings.TimestampFormats.HoursMinutes:
                                timeStamp = DateTime.Now.ToString("hh:mm tt");
                                break;
                            case Common.Settings.AvalonSettings.TimestampFormats.HoursMinutesSeconds:
                                timeStamp = DateTime.Now.ToString("hh:mm:ss tt");
                                break;
                            case Common.Settings.AvalonSettings.TimestampFormats.OSDefault:
                                timeStamp = DateTime.Now.ToString("g");
                                break;
                            case Common.Settings.AvalonSettings.TimestampFormats.TwentyFourHour:
                                timeStamp = DateTime.Now.ToString("HH:mm:ss");
                                break;
                            default:
                                timeStamp = DateTime.Now.ToString("hh:mm:ss tt");
                                break;
                        }

                        // Create a brand new line (not a shared reference) where this can be shown in the communication window.
                        var commLine = new Line
                        {
                            FormattedText = $"[{timeStamp}]: {line.FormattedText}\r\n"
                        };

                        if (item.MoveTo == TerminalTarget.Terminal1)
                        {
                            Terminal1.Append(commLine);

                            if (!App.MainWindow.CustomTab1.IsSelected)
                            {
                                App.MainWindow.CustomTab1Badge.Value += 1;
                            }
                            else if (App.MainWindow.CustomTab1.IsSelected && App.MainWindow.CustomTab1Badge.Value != 0)
                            {
                                // Only setting this if the value isn't 0 so it doesn't trigger UI processing.
                                App.MainWindow.CustomTab1Badge.Value = 0;
                            }
                        }
                        else if (item.MoveTo == TerminalTarget.Terminal2)
                        {
                            Terminal2.Append(commLine);

                            if (!App.MainWindow.CustomTab2.IsSelected)
                            {
                                App.MainWindow.CustomTab2Badge.Value += 1;
                            }
                            else if (App.MainWindow.CustomTab2.IsSelected && App.MainWindow.CustomTab2Badge.Value != 0)
                            {
                                // Only setting this if the value isn't 0 so it doesn't trigger UI processing.
                                App.MainWindow.CustomTab2Badge.Value = 0;
                            }
                        }
                        else if (item.MoveTo == TerminalTarget.Terminal3)
                        {
                            Terminal3.Append(commLine);

                            if (!App.MainWindow.CustomTab3.IsSelected)
                            {
                                App.MainWindow.CustomTab3Badge.Value += 1;
                            }
                            else if (App.MainWindow.CustomTab3.IsSelected && App.MainWindow.CustomTab3Badge.Value != 0)
                            {
                                // Only setting this if the value isn't 0 so it doesn't trigger UI processing.
                                App.MainWindow.CustomTab3Badge.Value = 0;
                            }
                        }

                    }
                }
            }

            // Go through the TriggerList which are user defined triggers
            foreach (var item in App.Settings.ProfileSettings.TriggerList)
            {
                // Skip it if it's not enabled.
                if (!item.Enabled)
                {
                    continue;
                }

                // Skip it if it's not global or for this character.
                if (!string.IsNullOrWhiteSpace(item.Character) && item.Character != App.Conveyor.GetVariable("Character"))
                {
                    continue;
                }

                // If there is no pattern skip it, we don't want to send thousands of commands on empty patterns.
                if (string.IsNullOrWhiteSpace(item.Pattern))
                {
                    continue;
                }

                if (item.IsMatch(line.Text))
                {
                    // Run any CLR that might exist.
                    item.Execute();

                    // Increment the counter.
                    item.Count++;

                    // Line Highlighting if the trigger is supposed to.
                    if (item.HighlightLine)
                    {
                        // TODO - Allow the highlighted color to be set for each trigger.
                        int start = GameTerminal.Document.Text.LastIndexOf(line.FormattedText, StringComparison.Ordinal);
                        GameTerminal.Document.Replace(start, line.FormattedText.Length, $"{AnsiColors.DarkCyan}{line.Text}");
                    }

                    // Only send if it has something in it.  Use the processed command.
                    if (!string.IsNullOrEmpty(item.ProcessedCommand) && item.IsLua == false)
                    {
                        // If it has text but it's not lua, send it to the interpreter.
                        await Interp.Send(item.ProcessedCommand, false, false);
                    }
                    else if (!string.IsNullOrEmpty(item.ProcessedCommand) && item.IsLua == true)
                    {
                        // If it has text and it IS lua, send it to the LUA engine.
                        await Interp.LuaCaller.ExecuteAsync(item.ProcessedCommand);
                    }

                    // Check if we're supposed to move this line somewhere else.
                    if (item.MoveTo != TerminalTarget.None)
                    {
                        string timeStamp;

                        switch (App.Settings.AvalonSettings.TimestampFormat)
                        {
                            case Common.Settings.AvalonSettings.TimestampFormats.HoursMinutes:
                                timeStamp = DateTime.Now.ToString("hh:mm tt");
                                break;
                            case Common.Settings.AvalonSettings.TimestampFormats.HoursMinutesSeconds:
                                timeStamp = DateTime.Now.ToString("hh:mm:ss tt");
                                break;
                            case Common.Settings.AvalonSettings.TimestampFormats.OSDefault:
                                timeStamp = DateTime.Now.ToString("g");
                                break;
                            case Common.Settings.AvalonSettings.TimestampFormats.TwentyFourHour:
                                timeStamp = DateTime.Now.ToString("HH:mm:ss");
                                break;
                            default:
                                timeStamp = DateTime.Now.ToString("hh:mm:ss tt");
                                break;
                        }

                        // Create a brand new line (not a shared reference) where this can be shown in the communication window.
                        var commLine = new Line
                        {
                            FormattedText = $"[{timeStamp}]: {line.FormattedText}\r\n"
                        };

                        if (item.MoveTo == TerminalTarget.Terminal1)
                        {
                            Terminal1.Append(commLine);

                            if (!App.MainWindow.CustomTab1.IsSelected)
                            {
                                App.MainWindow.CustomTab1Badge.Value += 1;
                            }
                            else if (App.MainWindow.CustomTab1.IsSelected && App.MainWindow.CustomTab1Badge.Value != 0)
                            {
                                // Only setting this if the value isn't 0 so it doesn't trigger UI processing.
                                App.MainWindow.CustomTab1Badge.Value = 0;
                            }
                        }
                        else if (item.MoveTo == TerminalTarget.Terminal2)
                        {
                            Terminal2.Append(commLine);

                            if (!App.MainWindow.CustomTab2.IsSelected)
                            {
                                App.MainWindow.CustomTab2Badge.Value += 1;
                            }
                            else if (App.MainWindow.CustomTab2.IsSelected && App.MainWindow.CustomTab2Badge.Value != 0)
                            {
                                // Only setting this if the value isn't 0 so it doesn't trigger UI processing.
                                App.MainWindow.CustomTab2Badge.Value = 0;
                            }
                        }
                        else if (item.MoveTo == TerminalTarget.Terminal3)
                        {
                            Terminal3.Append(commLine);

                            if (!App.MainWindow.CustomTab3.IsSelected)
                            {
                                App.MainWindow.CustomTab3Badge.Value += 1;
                            }
                            else if (App.MainWindow.CustomTab3.IsSelected && App.MainWindow.CustomTab3Badge.Value != 0)
                            {
                                // Only setting this if the value isn't 0 so it doesn't trigger UI processing.
                                App.MainWindow.CustomTab3Badge.Value = 0;
                            }
                        }

                    }
                }
            }
        }
    }
}
