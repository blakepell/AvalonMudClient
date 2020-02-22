using Argus.Extensions;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;

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
            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                await _interpreter.Send(cmd, false, false);
            }));
        }

        /// <summary>
        /// Gets a variable from the profile's global variable list.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetVariable(string key)
        {
            return Application.Current.Dispatcher.Invoke(new Func<string>(() => _interpreter.Conveyor.GetVariable(key))
            );
        }

        /// <summary>
        /// Sets a variable in the profile's global variable list.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetVariable(string key, string value)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.Conveyor.SetVariable(key, value);
            }));
        }

        /// <summary>
        /// Echos text to the main terminal.
        /// </summary>
        /// <param name="msg"></param>
        public void Echo(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                //_interpreter.Conveyor.EchoText(msg + "\r\n", AnsiColors.Cyan, Common.Models.TerminalTarget.Main);
                _interpreter.EchoText(msg, AnsiColors.Cyan);
            }));
        }

        /// <summary>
        /// Echos an event to the main terminal.
        /// </summary>
        /// <param name="msg"></param>
        public void EchoEvent(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                _interpreter.EchoText(msg, AnsiColors.Cyan, true);
            }));
        }

        /// <summary>
        /// Returns the first non null and non empty value.
        /// </summary>
        /// <param name="checkValue"></param>
        /// <param name="returnValue"></param>
        public string Coalesce(string valueOne, string valueTwo)
        {
            if (string.IsNullOrWhiteSpace(valueOne))
            {
                return valueTwo;
            }

            return valueOne;
        }

        /// <summary>
        /// Returns the current time in HH:MM:SS format.
        /// </summary>
        /// <returns></returns>
        public string GetTime()
        {
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
        /// <returns></returns>
        public int GetMinute()
        {
            return DateTime.Now.Minute;
        }

        /// <summary>
        /// Returns the current second.
        /// </summary>
        /// <returns></returns>
        public int GetSecond()
        {
            return DateTime.Now.Second;
        }

        /// <summary>
        /// Returns the current millisecond.
        /// </summary>
        /// <returns></returns>
        public int GetMillisecond()
        {
            return DateTime.Now.Millisecond;
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
        /// <returns></returns>
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
        /// <returns></returns>
        public string RandomChoice(string[] choices)
        {
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
        /// <returns></returns>
        public string RandomChoice(string choices, string delimiter)
        {
            var items = choices.Split(delimiter);
            return RandomChoice(items);
        }

        /// <summary>
        /// Returns a new GUID.
        /// </summary>
        /// <returns></returns>
        public string Guid()
        {
            return System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Sets the mud client's title.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public void SetTitle(string title)
        {
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
        /// <returns></returns>
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
        /// <returns></returns>
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
        /// <returns></returns>
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
        /// <returns></returns>
        public string[] Split(string buf, string delimiter)
        {
            return buf.Split(delimiter);
        }

        /// <summary>
        /// Searches an array for whether a specified value exists within it.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
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
        /// Returns a new string with all occurrences of a string replaced with another string.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="searchValue"></param>
        /// <param name="replaceValue"></param>
        /// <returns></returns>
        public string Replace(string buf, string searchValue, string replaceValue)
        {
            return buf.Replace(searchValue, replaceValue);
        }

        /// <summary>
        /// Enables all aliases and triggers in a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>Returns true if the group was found, false if it was not.</returns>
        public bool EnableGroup(string groupName)
        {
            return _interpreter.Conveyor.EnableGroup(groupName);
        }

        /// <summary>
        /// Disables all aliases and triggers in a group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>Returns true if the group was found, false if it was not.</returns>
        public bool DisableGroup(string groupName)
        {
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
            return value.FormatIfNumber(decimalPlaces);
        }

        private IInterpreter _interpreter;

    }
}