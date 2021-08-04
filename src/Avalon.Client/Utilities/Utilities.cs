/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Avalon.Common.Models;
using Avalon.Common.Scripting;

namespace Avalon.Utilities
{
    /// <summary>
    /// General utilities that don't currently fit other places.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Sets all triggers up with the Conveyor and ScriptHost from the MainWindow if they haven't been wired up already.
        /// </summary>
        public static void SetupTriggers()
        {
            if (App.Settings?.ProfileSettings?.TriggerList != null)
            {
                foreach (var trigger in App.Settings.ProfileSettings.TriggerList)
                {
                    // If it's got the old IsLua bit and it's set as a Command (the default) set it to MoonSharp.
                    if (trigger.IsLua && trigger.ExecuteAs == ExecuteType.Command)
                    {
                        trigger.ExecuteAs = ExecuteType.LuaMoonsharp;
                    }

                    // In case any of the function names are null or blank, set them up based off of the ID.
                    if (string.IsNullOrWhiteSpace(trigger.FunctionName))
                    {
                        trigger.FunctionName = ScriptHost.GetFunctionName(trigger.Identifier, "tr");
                    }

                    // Load the scripts into the scripting environment.
                    if (trigger.ExecuteAs == ExecuteType.LuaMoonsharp)
                    {
                        App.MainWindow.Interp.ScriptHost.AddFunction(new SourceCode(trigger.Command, trigger.FunctionName, ScriptType.MoonSharpLua));
                    }
                }
            }
        }

        /// <summary>
        /// Sets up any aliases with logic that should be executed on them.
        /// </summary>
        public static void SetupAliases()
        {
            if (App.Settings?.ProfileSettings?.AliasList != null)
            {
                foreach (var alias in App.Settings.ProfileSettings.AliasList)
                {
                    // If it's got the old IsLua bit and it's set as a Command (the default) set it to MoonSharp.
                    if (alias.IsLua && alias.ExecuteAs == ExecuteType.Command)
                    {
                        alias.ExecuteAs = ExecuteType.LuaMoonsharp;
                    }

                    // In case any of the function names are null or blank, set them up based off of the ID.
                    if (string.IsNullOrWhiteSpace(alias.FunctionName))
                    {
                        alias.FunctionName = ScriptHost.GetFunctionName(alias.Id, "a");
                    }

                    // Load the scripts into the scripting environment.
                    if (alias.ExecuteAs == ExecuteType.LuaMoonsharp)
                    {
                        App.MainWindow.Interp.ScriptHost.AddFunction(new SourceCode(alias.Command, alias.FunctionName, ScriptType.MoonSharpLua));
                    }
                }
            }
        }

        /// <summary>
        /// Puts common variables that might be expensive to populate into the common variable list
        /// as a static variable.
        /// </summary>
        public static void UpdateCommonVariables()
        {
            App.Conveyor.SetVariable("Username", Environment.UserName);
            App.Conveyor.SetVariable("Date", DateTime.Now.ToFileNameFriendlyFormat(false));
        }

        /// <summary>
        /// Characters that will be removed from incoming data via <see cref="RemoveUnsupportedCharacters"/>.
        /// </summary>
        private static HashSet<char> _unsupportedChars = new HashSet<char> { (char)1, (char)249, (char)251, (char)252, (char)255, (char)65533 };

        /// <summary>
        /// Removes unsupported characters or other sets of sequences we don't want parsed.
        /// </summary>
        /// <param name="sb"></param>
        public static void RemoveUnsupportedCharacters(this StringBuilder sb)
        {
            // Go through the StringBuilder backwards and remove any characters in our HashSet.
            for (int i = sb.Length - 1; i >= 0; i--)
            {
                if (_unsupportedChars.Contains(sb[i]))
                {
                    sb.Remove(i, 1);
                }
            }

            // Remove the up, down, left, right, blink, reverse and underline.  We're not supporting
            // these at this time although we will support come of them in the future.
            sb.Replace("\x1B[1A", ""); // Up
            sb.Replace("\x1B[1B", ""); // Down
            sb.Replace("\x1B[1C", ""); // Right
            sb.Replace("\x1B[1D", ""); // Left
            sb.Replace("\x1B[5m", ""); // Blink
            sb.Replace("\x1b[2J", ""); // Clear Screen
        }

        /// <summary>
        /// Returns the danger color for a given percent.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxValue"></param>
        public static SolidColorBrush ColorPercent(int value, int maxValue)
        {
            if (value < (maxValue / 2))
            {
                return Brushes.Red;
            }
            else if (value < ((maxValue * 3) / 4))
            {
                return Brushes.Yellow;
            }
            else
            {
                return Brushes.White;
            }
        }

        /// <summary>
        /// Processes a speedwalk command into a set of commands.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="leaveParens">Whether or not to leave parenthesis around commands.  The default value is false.</param>/// 
        public static string Speedwalk(string input, bool leaveParens = false)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "";
            }

            var sb = Argus.Memory.StringBuilderPool.Take();

            try
            {
                // This will be each individual step (or a number in the same direction)
                foreach (string step in input.Split(' '))
                {
                    if (step.ContainsNumber())
                    {
                        string stepsStr = "";
                        string direction = "";

                        // Pluck the number off the front (e.g. 4n)
                        foreach (char c in step)
                        {
                            if (char.IsNumber(c))
                            {
                                stepsStr += c;
                            }
                            else
                            {
                                direction += c;
                            }
                        }

                        // The number of steps to repeat this specific step
                        int steps = int.Parse(stepsStr);

                        for (int i = 1; i <= steps; i++)
                        {
                            sb.Append(direction);
                            sb.Append(App.Settings.AvalonSettings.CommandSplitCharacter);
                        }

                    }
                    else
                    {
                        // No number, put it in verbatim.
                        sb.Append(step);
                        sb.Append(App.Settings.AvalonSettings.CommandSplitCharacter);
                    }

                }

                sb.TrimEnd(App.Settings.AvalonSettings.CommandSplitCharacter);

                // Finally, look for parens and turn semi-colons in between there into spaces.  This might be hacky but should
                // allow for commands in the middle of our directions as long as they're surrounded by ().
                bool on = false;

                for (int i = 0; i < sb.Length; i++)
                {
                    if (sb[i] == '(')
                    {
                        on = true;
                    }
                    else if (sb[i] == ')')
                    {
                        on = false;
                    }

                    if (on && sb[i] == App.Settings.AvalonSettings.CommandSplitCharacter)
                    {
                        sb[i] = ' ';
                    }
                }

                // Now that the command has been properly placed, remove any parents.
                if (!leaveParens)
                {
                    sb.Replace("(", "").Replace(")", "");
                }

                return sb.ToString();
            }
            finally
            {
                Argus.Memory.StringBuilderPool.Return(sb);
            }
        }

        /// <summary>
        /// Reverses a speedwalk path.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="leaveParens">Whether or not to leave parenthesis around commands.  The default value is false.</param>
        public static string SpeedwalkReverse(string input, bool leaveParens = false)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "";
            }

            var forwardList = new List<string>()
            {
                "n", "e", "s", "w", "u", "d", "nw", "ne", "sw", "se"
            };

            var reverseList = new List<string>()
            {
                "s", "w", "n", "e", "d", "u", "se", "sw", "ne", "nw"
            };

            var sb = Argus.Memory.StringBuilderPool.Take();

            // Make it so they're all busted out.
            string forwardPath = Speedwalk(input, true);

            // Now, reverse that.
            var path = forwardPath.Split(App.Settings.AvalonSettings.CommandSplitCharacter).Reverse().ToList();

            for (int i = 0; i < path.Count; i++)
            {
                // Swap the reverse direction in
                int pos = forwardList.IndexOf(path[i]);

                if (pos == -1)
                {
                    continue;
                }

                path[i] = reverseList[pos];
            }

            // This will be each individual step (or a number in the same direction)
            foreach (string step in path)
            {
                if (step.ContainsNumber())
                {
                    string stepsStr = "";
                    string direction = "";

                    // Pluck the number off the front (e.g. 4n)
                    foreach (char c in step)
                    {
                        if (char.IsNumber(c))
                        {
                            stepsStr += c;
                        }
                        else
                        {
                            direction += c;
                        }
                    }

                    // The number of steps to repeat this specific step
                    int steps = int.Parse(stepsStr);

                    for (int i = 1; i <= steps; i++)
                    {
                        sb.Append(direction);
                        sb.Append(App.Settings.AvalonSettings.CommandSplitCharacter);
                    }

                }
                else
                {
                    // No number, put it in verbatim.
                    sb.Append(step);
                    sb.Append(App.Settings.AvalonSettings.CommandSplitCharacter);
                }

            }

            sb.TrimEnd(App.Settings.AvalonSettings.CommandSplitCharacter);

            if (!leaveParens)
            {
                // Finally, look for parens and turn semi-colons in between there into spaces.  This might be hacky but should
                // allow for commands in the middle of our directions as long as they're surrounded by ().
                bool on = false;

                for (int i = 0; i < sb.Length; i++)
                {
                    if (sb[i] == '(')
                    {
                        on = true;
                    }
                    else if (sb[i] == ')')
                    {
                        on = false;
                    }

                    if (on && sb[i] == App.Settings.AvalonSettings.CommandSplitCharacter)
                    {
                        sb[i] = ' ';
                    }
                }

                // Now that the command has been properly placed, remove any parens.
                sb.Replace("(", "").Replace(")", "");
            }
            else
            {
                sb.Replace(App.Settings.AvalonSettings.CommandSplitCharacter, ' ');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Copies any plugins that were updated to the plugins folder.
        /// </summary>
        public static int UpdatePlugins()
        {
            var files = Directory.GetFiles(App.Settings.UpdateDirectory);
            int count = 0;

            // First, move any plugin DLL's
            foreach (string file in files)
            {
                string pluginFileName = Argus.IO.FileSystemUtilities.ExtractFileName(file);

                if (pluginFileName.StartsWith("Avalon.Plugin", StringComparison.OrdinalIgnoreCase) && pluginFileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    string outputFile = Path.Combine(App.Settings.PluginDirectory, pluginFileName);
                    File.Copy(file, outputFile, true);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Try to cleanup all files in the update folder, not point in them hanging around.
        /// </summary>
        public static int CleanupUpdatesFolder()
        {
            var files = Directory.GetFiles(App.Settings.UpdateDirectory);
            int count = 0;

            foreach (string file in files)
            {
                File.Delete(file);
                count++;
            }

            return count;
        }

        /// <summary>
        /// Shells a link via System.Diagnostics.Process.
        /// </summary>
        /// <param name="url"></param>
        public static void ShellLink(string url)
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
        /// Shells an executable.  Caller should check the AllowShell setting.
        /// </summary>
        /// <param name="path"></param>
        public static void Shell(string path)
        {
            Process.Start(path);
        }

        /// <summary>
        /// Shells an executable.  Caller should check the AllowShell setting.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="arguments"></param>
        public static void Shell(string path, string arguments)
        {
            Process.Start(path, arguments);
        }

        /// <summary>
        /// Returns a text time stamp in the format set in the user settings.
        /// </summary>
        public static string Timestamp()
        {
            switch (App.Settings.AvalonSettings.TimestampFormat)
            {
                case Common.Settings.AvalonSettings.TimestampFormats.HoursMinutes:
                    return DateTime.Now.ToString("hh:mm tt");
                case Common.Settings.AvalonSettings.TimestampFormats.HoursMinutesSeconds:
                    return DateTime.Now.ToString("hh:mm:ss tt");
                case Common.Settings.AvalonSettings.TimestampFormats.OSDefault:
                    return DateTime.Now.ToString("g");
                case Common.Settings.AvalonSettings.TimestampFormats.TwentyFourHour:
                    return DateTime.Now.ToString("HH:mm:ss");
                default:
                    return DateTime.Now.ToString("hh:mm:ss tt");
            }
        }

        /// <summary>
        /// Sets up a PropertyChanged binding.
        /// </summary>
        /// <param name="o">The source object with the property that should be bound.</param>
        /// <param name="propertyName">The name of the property on the source object.</param>
        /// <param name="depObj">The name of the DependencyObject, usually the control.</param>
        /// <param name="depProp">The DependencyProperty on the control.</param>
        /// <param name="converter"></param>
        public static void SetBinding(object o, string propertyName, DependencyObject depObj, DependencyProperty depProp, IValueConverter converter = null)
        {
            var binding = new Binding
            {
                Source = o,
                Path = new PropertyPath(propertyName),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            if (converter != null)
            {
                binding.Converter = converter;
            }

            // Clear the binding object for just this object and property.
            BindingOperations.ClearBinding(depObj, depProp);

            // Set the binding anew.
            BindingOperations.SetBinding(depObj, depProp, binding);
        }
    }
}