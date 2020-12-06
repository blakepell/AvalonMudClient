using Argus.Extensions;
using Avalon.Colors;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Avalon.Lua
{
    /// <summary>
    /// C# methods that are exposed to LUA.
    /// </summary>
    public class LuaCommands
    {
        public LuaCommands(IInterpreter interp, Random rnd)
        {
            _interpreter = interp;
            _random = rnd;
        }

        /// <summary>
        /// Single static Random object that will need to be locked between usages.
        /// </summary>
        private static Random _random;

        /// <summary>
        /// Locking object for the random number generator
        /// </summary>
        private static object _randomLock = new object();

        /// <summary>
        /// Sends text to the server.
        /// </summary>
        /// <param name="cmd"></param>
        public async void Send(string cmd)
        {
            if (cmd == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                await _interpreter.Send(cmd, false, false);
            }));
        }

        /// <summary>
        /// Gets a <see cref="Variable"/> from the profile's global variable list.
        /// </summary>
        /// <param name="key"></param>
        public string GetVariable(string key)
        {
            if (key == null)
            {
                return "";
            }
            
            // Invoke requested so that the call waits for the result of the function before returning.
            return Application.Current.Dispatcher.Invoke(new Func<string>(() =>
            {
                return _interpreter.Conveyor.GetVariable(key);
            }));
        }

        /// <summary>
        /// Sets a <see cref="Variable"/> in the profile's global variable list.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetVariable(string key, string value)
        {
            if (key == null)
            {
                return;
            }

            if (value == null)
            {
                value = "";
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.Conveyor.SetVariable(key, value);
            }));
        }

        /// <summary>
        /// Removes a <see cref="Variable"/> from the global variable list.
        /// </summary>
        /// <param name="key"></param>
        public void RemoveVariable(string key)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.Conveyor.RemoveVariable(key);
            }));
        }

        /// <summary>
        /// Echos text to the main terminal.
        /// </summary>
        /// <param name="msg"></param>
        public void Echo(string msg)
        {
            if (msg == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var line = new Line
                {
                    FormattedText = $"{msg}\r\n",
                    ForegroundColor = AnsiColors.Cyan,
                    ReverseColors = false,
                    IgnoreLastColor = true
                };

                _interpreter.Conveyor.EchoText(line, TerminalTarget.Main);
            }));
        }

        /// <summary>
        /// Echos text to the main terminal.
        /// </summary>
        /// <param name="msg"></param>
        public void Echo(string msg, string color, bool reverse)
        {
            if (msg == null)
            {
                return;
            }

            if (color == null)
            {
                color = "Cyan";
            }

            var foreground = Colorizer.ColorMapByName(color)?.AnsiColor ?? AnsiColors.Cyan;

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var line = new Line
                {
                    FormattedText = $"{msg}\r\n",
                    ForegroundColor = foreground,
                    ReverseColors = reverse,
                    IgnoreLastColor = true
                };

                _interpreter.Conveyor.EchoText(line, TerminalTarget.Main);
            }));
        }

        /// <summary>
        /// Echos an event to the main terminal.
        /// </summary>
        /// <param name="msg"></param>
        public void EchoEvent(string msg)
        {
            if (msg == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var line = new Line
                {
                    FormattedText = $"{msg}\r\n",
                    ForegroundColor = AnsiColors.Cyan,
                    ReverseColors = true,
                    IgnoreLastColor = true
                };

                _interpreter.Conveyor.EchoText(line, TerminalTarget.Main);
            }));
        }

        /// <summary>
        /// Echos text to a custom window.  The append parameter is true by default but if made
        /// false this will clear the text in the window first.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="text"></param>
        public void EchoWindow(string windowName, string text)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                // This case is if they specified a window that might exist, we'll find it, edit that.
                var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.WindowType == WindowType.TerminalWindow && x.Name.Equals(windowName, StringComparison.Ordinal)) as TerminalWindow;

                if (win == null)
                {
                    return;
                }
                else
                {
                    var sb = Argus.Memory.StringBuilderPool.Take(text);
                    Colorizer.MudToAnsiColorCodes(sb);

                    var line = new Line
                    {
                        FormattedText = sb.ToString(),
                        ForegroundColor = AnsiColors.Default,
                        ReverseColors = false
                    };

                    win.AppendText(line);

                    Argus.Memory.StringBuilderPool.Return(sb);
                }
            }));
        }

        /// <summary>
        /// Clears the text in a terminal of a specified window name.
        /// </summary>
        /// <param name="windowName"></param>
        public void ClearWindow(string windowName)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                // This case is if they specified a window that might exist, we'll find it, edit that.
                var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.WindowType == WindowType.TerminalWindow && x.Name.Equals(windowName, StringComparison.Ordinal)) as TerminalWindow;

                if (win == null)
                {
                    return;
                }

                win.Text = "";
            }));

        }

        /// <summary>
        /// Returns the first non null and non empty value.  If none are found a blank
        /// string will be returned.
        /// </summary>
        public string Coalesce(string valueOne, string valueTwo)
        {
            if (!string.IsNullOrWhiteSpace(valueOne))
            {
                return valueOne;
            }

            if (!string.IsNullOrWhiteSpace(valueTwo))
            {
                return valueTwo;
            }

            return "";
        }

        /// <summary>
        /// Returns the current time formatted as either 12-hour or 24-hour.
        /// </summary>
        /// <param name="meridiemTime">Whether or not to return the time in AM/PM format.</param>
        public string GetTime(bool meridiemTime = false)
        {
            if (meridiemTime)
            {
                return DateTime.Now.ToString(@"hh:mm:ss tt", new CultureInfo("en-US"));
            }

            return $"{DateTime.Now.Hour}:{DateTime.Now.Minute}:{DateTime.Now.Second}";
        }

        /// <summary>
        /// Returns the current hour.
        /// </summary>
        /// <returns></returns>
        public int GetHour()
        {
            return DateTime.Now.Hour;
        }

        /// <summary>
        /// Returns the current minute.
        /// </summary>
        public int GetMinute()
        {
            return DateTime.Now.Minute;
        }

        /// <summary>
        /// Returns the current second.
        /// </summary>
        public int GetSecond()
        {
            return DateTime.Now.Second;
        }

        /// <summary>
        /// Returns the current millisecond.
        /// </summary>
        public int GetMillisecond()
        {
            return DateTime.Now.Millisecond;
        }

        /// <summary>
        /// The minutes elapsed since the start of the day.
        /// </summary>
        public int DailyMinutesElapsed()
        {
            var dt = DateTime.Now;
            return (dt.Hour * 60) + dt.Minute;
        }

        /// <summary>
        /// The seconds elapsed since the start of the day.
        /// </summary>
        public int DailySecondsElapsed()
        {
            var dt = DateTime.Now;
            int minutes = (dt.Hour * 60) + dt.Minute;
            return (minutes * 60) + dt.Second;
        }

        /// <summary>
        /// The milliseconds elapsed since the start of the day.
        /// </summary>
        public int DailyMillisecondsElapsed()
        {
            var dt = DateTime.Now;
            int minutes = (dt.Hour * 60) + dt.Minute;
            int seconds = (minutes * 60) + dt.Second;
            return (seconds * 1000) + dt.Millisecond;
        }

        /// <summary>
        /// Will pause the Lua script for the designated amount of milliseconds.  This is not async
        /// so it will block the Lua (but since Lua is called async the rest of the program continues
        /// to work).  This will be an incredibly useful and powerful command for those crafting Lua scripts.
        /// </summary>
        /// <param name="milliseconds"></param>
        public void Sleep(int milliseconds)
        {
            Task.Delay(milliseconds).Wait();
        }

        /// <summary>
        /// Returns a random number.
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        public int RandomNumber(int low, int high)
        {
            lock (_randomLock)
            {
                // Randoms lower bound is inclusive.. but the upper bound is exclusive, so +1
                return _random.Next(low, high + 1);
            }
        }

        /// <summary>
        /// Returns a random element from the string array provided.
        /// </summary>
        /// <param name="choices"></param>
        public string RandomChoice(string[] choices)
        {
            if (choices == null)
            {
                return "";
            }

            int upperBound = choices.GetUpperBound(0);

            lock (_randomLock)
            {
                // Randoms lower bound is inclusive.. but the upper bound is exclusive, so +1
                return choices[_random.Next(0, upperBound + 1)];
            }
        }

        /// <summary>
        /// Returns a random element from the string provided that string and a delimiter.  The delimiter
        /// is used to split the string into the choices.
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="delimiter"></param>
        public string RandomChoice(string choices, string delimiter)
        {
            if (choices == null || delimiter == null)
            {
                return "";
            }

            var items = choices.Split(delimiter);
            return RandomChoice(items);
        }

        /// <summary>
        /// Returns a new GUID.
        /// </summary>
        public string Guid()
        {
            return System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Sets the mud client's title.
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title)
        {
            if (title == null)
            {
                return;
            }

            // TODO - Threading done in the property, probably the wrong way to do this, should move here probably.
            _interpreter.Conveyor.Title = title;
        }

        /// <summary>
        /// The text that is currently in the scrape buffer.
        /// </summary>
        public string GetScrapedText()
        {
            return _interpreter.Conveyor.Scrape.ToString();
        }

        /// <summary>
        /// Checks if a string exists in another string (Case Sensitive).
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="contains"></param>
        public bool Contains(string buf, string contains)
        {
            return Contains(buf, contains, false);
        }

        /// <summary>
        /// Checks if a string exists in another string.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="contains"></param>
        /// <param name="ignoreCase"></param>
        public bool Contains(string buf, string contains, bool ignoreCase)
        {
            if (ignoreCase)
            {
                return buf.Contains(contains, StringComparison.OrdinalIgnoreCase);
            }

            return buf.Contains(contains, StringComparison.Ordinal);
        }

        /// <summary>
        /// Trims whitespace off of the front and end of a string.
        /// </summary>
        /// <param name="buf"></param>
        public string Trim(string buf)
        {
            return buf?.Trim() ?? "";
        }

        /// <summary>
        /// Trims whitespace off of the front and end of a string.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="trimOff"></param>
        public string Trim(string buf, string trimOff)
        {
            return buf?.Trim(trimOff) ?? "";
        }

        /// <summary>
        /// Trims whitespace off of the start of a string.
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public string TrimStart(string buf)
        {
            return buf?.TrimStart() ?? "";
        }

        /// <summary>
        /// Trims whitespace off of the start of a string.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="trimOff"></param>
        /// <returns></returns>
        public string TrimStart(string buf, string trimOff)
        {
            return buf?.TrimStart(trimOff) ?? "";
        }

        /// <summary>
        /// Trims whitespace off the end of a string.
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public string TrimEnd(string buf)
        {
            return buf?.TrimEnd() ?? "";
        }

        /// <summary>
        /// Trims whitespace off the end of a string.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="trimOff"></param>
        /// <returns></returns>
        public string TrimEnd(string buf, string trimOff)
        {
            return buf?.TrimEnd(trimOff) ?? "";
        }

        /// <summary>
        /// Splits a string into a string array using a specified delimiter.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="delimiter"></param>
        public string[] Split(string buf, string delimiter)
        {
            return buf?.Split(delimiter);
        }

        /// <summary>
        /// Searches an array for whether a specified value exists within it.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="searchValue"></param>
        public bool ArrayContains(string[] array, string searchValue)
        {
            foreach (var s in array)
            {
                if (s == searchValue)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds an item to a list.  Duplicate items are acceptable.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        public string ListAdd(string sourceList, string value, char delimiter = '|')
        {
            return $"{sourceList}|{value}".Trim('|');
        }

        /// <summary>
        /// Adds an item to a list at the start.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        public string ListAddStart(string sourceList, string value, char delimiter = '|')
        {
            return $"{value}|{sourceList}".Trim('|');
        }

        /// <summary>
        /// Adds an item to a list only if it doesn't exist.  Duplicates
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        public string ListAddIfNotExist(string sourceList, string value, char delimiter = '|')
        {
            if (ListExists(sourceList, value, delimiter))
            {
                return sourceList;
            }

            return ListAdd(sourceList, value, delimiter);
        }

        /// <summary>
        /// Removes an item from a list.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        public string ListRemove(string sourceList, string value, char delimiter = '|')
        {
            var list = sourceList.Split(delimiter);
            var sb = new StringBuilder();

            foreach (string item in list)
            {
                if (!item.Equals(value, StringComparison.Ordinal))
                {
                    sb.AppendFormat("|{0}", value);
                }
            }

            return sb.ToString().Trim('|');
        }

        /// <summary>
        /// Removes 1 to n items from the end of a list.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        public string ListRemove(string sourceList, int items, char delimiter = '|')
        {
            var list = sourceList.Split(delimiter);
            var sb = new StringBuilder();
            int keep = list.Count() - items;

            for (int i = 0; i < keep; i++)
            {
                sb.AppendFormat("|{0}", list[i]);
            }

            return sb.ToString().Trim('|');
        }

        /// <summary>
        /// If an item exists in a list.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        public bool ListExists(string sourceList, string value, char delimiter = '|')
        {
            var list = sourceList.Split(delimiter);
            return list.Any(x => x.Equals(value, StringComparison.Ordinal));
        }

        /// <summary>
        /// Returns a new string with all occurrences of a string replaced with another string.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="searchValue"></param>
        /// <param name="replaceValue"></param>
        /// <returns></returns>
        public string Replace(string buf, string searchValue, string replaceValue)
        {
            if (buf == null)
            {
                return "";
            }

            return buf.Replace(searchValue, replaceValue);
        }

        /// <summary>
        /// Enables all aliases and triggers in a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>Returns true if the group was found, false if it was not.</returns>
        public bool EnableGroup(string groupName)
        {
            if (groupName == null)
            {
                return false;
            }

            return _interpreter.Conveyor.EnableGroup(groupName);
        }

        /// <summary>
        /// Disables all aliases and triggers in a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>Returns true if the group was found, false if it was not.</returns>
        public bool DisableGroup(string groupName)
        {
            if (groupName == null)
            {
                return false;
            }

            return _interpreter.Conveyor.DisableGroup(groupName);
        }

        /// <summary>
        /// Adds a scheduled task (command or Lua) to be executed after a designated time.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isLua"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public void AddScheduledTask(string command, bool isLua, int seconds)
        {
            App.MainWindow.ScheduledTasks.AddTask(command, isLua, DateTime.Now.AddSeconds(seconds));
        }

        /// <summary>
        /// Adds a batch task (command or Lua) to be executed in order when the batch is run.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isLua"></param>
        public void AddBatchTask(string command, bool isLua)
        {
            App.MainWindow.BatchTasks.AddTask(command, isLua);
        }

        /// <summary>
        /// Starts the current batch processing.
        /// </summary>
        /// <param name="secondsInBetweenCommands"></param>
        public void StartBatch(int secondsInBetweenCommands)
        {
            App.MainWindow.BatchTasks.StartBatch(secondsInBetweenCommands);
        }

        /// <summary>
        /// Clears all tasks from the scheduled tasks queue.
        /// </summary>
        public void ClearTasks()
        {
            App.MainWindow.ScheduledTasks.ClearTasks();
        }

        /// <summary>
        /// Formats a number as string with commas and no decimal places.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string FormatNumber(string value)
        {
            if (value == null)
            {
                return "";
            }

            return value.FormatIfNumber();
        }

        /// <summary>
        /// Formats a number as string with commas with the specified number of decimal places.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public string FormatNumber(string value, int decimalPlaces)
        {
            if (value == null)
            {
                return "";
            }

            return value.FormatIfNumber(decimalPlaces);
        }

        /// <summary>
        /// Returns the last non-empty line in the game terminal.
        /// </summary>
        public string LastNonEmptyLine()
        {
            string buf = "";

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                buf = App.MainWindow.GameTerminal.LastNonEmptyLine;
            }));

            return buf ?? "";
        }

        /// <summary>
        /// Returns a string array of the requested number of last lines from the game terminal.
        /// </summary>
        /// <param name="numberToTake"></param>
        public string[] LastLines(int numberToTake)
        {
            return LastLines(numberToTake, true);
        }

        /// <summary>
        /// Returns a string array of the requested number of last lines from the game terminal.
        /// </summary>
        /// <param name="numberToTake"></param>
        /// <param name="reverseOrder">Whether the order of the array should be reversed.  True will return oldest line to newest, False will be newest to oldest.</param>
        public string[] LastLines(int numberToTake, bool reverseOrder)
        {
            var list = new List<string>();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                string text = "";
                int i = App.MainWindow.GameTerminal.Document.LineCount;
                int taken = 0;

                while (string.IsNullOrEmpty(text) && i > 0)
                {
                    var line = App.MainWindow.GameTerminal.Document.GetLineByNumber(i);
                    list.Add(App.MainWindow.GameTerminal.Document.GetText(line.Offset, line.Length));

                    i--;
                    taken++;

                    // Once we've satisfied the number of lines to get, exit the loop.
                    if (taken == numberToTake)
                    {
                        break;
                    }
                }
            }));

            if (reverseOrder)
            {
                list.Reverse();
            }

            return list.ToArray();
        }

        /// <summary>
        /// Returns the MD5 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        public string MD5(string value)
        {
            return Argus.Cryptography.HashUtilities.MD5Hash(value);
        }

        /// <summary>
        /// Returns the SHA256 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        public string SHA256(string value)
        {
            return Argus.Cryptography.HashUtilities.Sha256Hash(value);
        }

        /// <summary>
        /// Returns the SHA512 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        public string SHA512(string value)
        {
            return Argus.Cryptography.HashUtilities.Sha512Hash(value);
        }

        /// <summary>
        /// The current location of the profile save directory.
        /// </summary>
        public string ProfileDirectory()
        {
            return App.Settings.AvalonSettings.SaveDirectory;
        }

        /// <summary>
        /// Where the avalon setting file is stored that among other things has where the profile
        /// save directory is (in case that is in Dropbox, OneDrive, etc.).
        /// </summary>
        public string AppDataDirectory()
        {
            return App.Settings.AppDataDirectory;
        }

        private readonly IInterpreter _interpreter;

    }
}