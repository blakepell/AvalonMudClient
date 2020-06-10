using Argus.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Avalon.Utilities
{
    /// <summary>
    /// General utilities that don't currently fit other places.
    /// </summary>
    public static class Utilities
    {
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
        /// Removes unsupported characters or other sets of sequences we don't want parsed.
        /// </summary>
        /// <param name="sb"></param>
        public static void RemoveUnsupportedCharacters(this StringBuilder sb)
        {
            // Remove single characters we're not supporting, rebuild the string.
            var removeChars = new HashSet<char> { (char)1, (char)249, (char)251, (char)252, (char)255, (char)65533 };

            // Go through the StringBuilder backwards and remove any characters in our HashSet.
            for (int i = sb.Length - 1; i >= 0; i--)
            {
                char temp = sb[i];

                if (removeChars.Contains(sb[i]))
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
        /// <returns></returns>
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
        /// <returns></returns>
        public static string Speedwalk(string input)
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
                            sb.Append(';');
                        }

                    }
                    else
                    {
                        // No number, put it in verbatim.
                        sb.Append(step);
                        sb.Append(';');
                    }

                }

                sb.TrimEnd(';');

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

                    if (on == true && sb[i] == ';')
                    {
                        sb[i] = ' ';
                    }
                }

                // Now that the command has been properly placed, remove any parents.
                sb.Replace("(", "").Replace(")", "");

                return sb.ToString();
            }
            finally
            {
                Argus.Memory.StringBuilderPool.Return(sb);
            }            
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