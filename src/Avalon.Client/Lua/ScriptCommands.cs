/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Avalon.Colors;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Extensions;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Avalon.Lua
{
    /// <summary>
    /// C# methods that are exposed to LUA.
    /// </summary>
    public class ScriptCommands
    {
        public ScriptCommands(IInterpreter interp)
        {
            _interpreter = interp;
            _random = new Random();
        }

        /// <summary>
        /// Single static Random object that will need to be locked between usages.
        /// </summary>
        private static Random _random;

        /// <summary>
        /// Locking object for the random number generator
        /// </summary>
        private static readonly object _randomLock = new object();

        /// <summary>
        /// Sends text to the server.
        /// </summary>
        /// <param name="cmd"></param>
        [Description("Sends text to the server.")]
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
        /// Sends a raw unprocessed command to the server.
        /// </summary>
        /// <param name="cmd"></param>
        [Description("Sends raw unprocessed text to the server.")]
        public async void SendRaw(string cmd)
        {
            if (cmd == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(new Action(async () =>
            {
                await _interpreter.SendRaw(cmd, false);
            }));
        }

        /// <summary>
        /// Gets a <see cref="Variable"/> from the profile's global variable list.
        /// </summary>
        /// <param name="key"></param>
        [Description("Gets a variable from the profile's global variable list.")]
        public string GetVariable(string key)
        {
            if (key == null)
            {
                return "";
            }

            // Invoke requested so that the call waits for the result of the function before returning.
            return Application.Current.Dispatcher.Invoke(() => _interpreter.Conveyor.GetVariable(key));
        }

        /// <summary>
        /// Sets a <see cref="Variable"/> in the profile's global variable list.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        [Description("Sets the value of a variable in the profile's global variable list.")]
        public void SetVariable(string key, string value)
        {
            if (key == null)
            {
                return;
            }

            value ??= "";

            Application.Current.Dispatcher.Invoke(() => _interpreter.Conveyor.SetVariable(key, value));
        }

        /// <summary>
        /// Sets a <see cref="Variable"/> in the profile's global variable list.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="color">A known color</param>
        [Description("Sets the value of a variable in the profile's global variable list.")]
        public void SetVariable(string key, string value, string color)
        {
            if (key == null)
            {
                return;
            }

            value ??= "";

            Application.Current.Dispatcher.Invoke(() => _interpreter.Conveyor.SetVariable(key, value, color));
        }

        /// <summary>
        /// Shows a variable in the variable repeater if the key is found.
        /// </summary>
        /// <param name="key"></param>
        [Description("Shows a variable in the main terminal's variable repeater.")]
        public void ShowVariable(string key)
        {
            Application.Current.Dispatcher.Invoke(() => _interpreter.Conveyor.ShowVariable(key));
        }

        /// <summary>
        /// Hides a variable in the variable repeater if the key is found.
        /// </summary>
        /// <param name="key"></param>
        [Description("Hides a variable in the main terminal's variable repeater.")]
        public void HideVariable(string key)
        {
            Application.Current.Dispatcher.Invoke(() => _interpreter.Conveyor.HideVariable(key));
        }

        /// <summary>
        /// Removes a <see cref="Variable"/> from the global variable list.
        /// </summary>
        /// <param name="key"></param>
        [Description("Removes a variable from the global variable list.")]
        public void RemoveVariable(string key)
        {
            Application.Current.Dispatcher.Invoke(() => _interpreter.Conveyor.RemoveVariable(key));
        }

        /// <summary>
        /// Echos text to the main terminal.
        /// </summary>
        /// <param name="msg"></param>
        [Description("Writes text to the main terminal.")]
        public void Echo(string msg)
        {
            if (msg == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var line = new Line
                {
                    FormattedText = $"{msg}\r\n",
                    ForegroundColor = AnsiColors.Cyan,
                    ReverseColors = false,
                    IgnoreLastColor = true
                };

                _interpreter.Conveyor.EchoText(line, TerminalTarget.Main);
            });
        }

        /// <summary>
        /// Echos text to the specified terminal.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="term"></param>
        [Description("Writes text to the specified terminal. The TerminalTarget an enum but accepts the numeric value.")]
        public void Echo(string msg, TerminalTarget term)
        {
            if (msg == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var line = new Line
                {
                    FormattedText = $"{msg}\r\n",
                    ForegroundColor = AnsiColors.Default,
                    IgnoreLastColor = true
                };

                _interpreter.Conveyor.EchoText(line, term);
            });
        }

        /// <summary>
        /// Echos text to the main terminal.
        /// </summary>
        /// <param name="msg">The text to echo to the terminal.</param>
        /// <param name="color">The known color name for the foreground color.</param>
        /// <param name="reverse">Whether foreground color should be reversed with the background color.</param>
        [Description("Writes text to the main terminal.")]
        public void Echo(string msg, string color, bool reverse)
        {
            if (msg == null)
            {
                return;
            }

            color ??= "Cyan";

            var foreground = Colorizer.ColorMapByName(color)?.AnsiColor ?? AnsiColors.Cyan;

            Application.Current.Dispatcher.Invoke(() =>
            {
                var line = new Line
                {
                    FormattedText = $"{msg}\r\n",
                    ForegroundColor = foreground,
                    ReverseColors = reverse,
                    IgnoreLastColor = true
                };

                _interpreter.Conveyor.EchoText(line, TerminalTarget.Main);
            });
        }

        /// <summary>
        /// Echos an event to the main terminal.
        /// </summary>
        /// <param name="msg">The text to echo to the client.</param>
        [Description("Writes a text event to the main terminal.")]
        public void EchoEvent(string msg)
        {
            if (msg == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var line = new Line
                {
                    FormattedText = $"{msg}\r\n",
                    ForegroundColor = AnsiColors.Cyan,
                    ReverseColors = true,
                    IgnoreLastColor = true
                };

                _interpreter.Conveyor.EchoText(line, TerminalTarget.Main);
            });
        }

        /// <summary>
        /// Echos text to a custom window.  The append parameter is true by default but if made
        /// false this will clear the text in the window first.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="text"></param>
        [Description("Writes text to a custom default terminal.")]
        public void EchoWindow(string windowName, string text)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // This case is if they specified a window that might exist, we'll find it, edit that.
                var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x =>
                        x.WindowType == WindowType.TerminalWindow &&
                        x.Name.Equals(windowName, StringComparison.Ordinal)) as
                    TerminalWindow;

                if (win == null)
                {
                    return;
                }

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
            });
        }

        /// <summary>
        /// Makes an info echo.
        /// </summary>
        /// <param name="msg"></param>
        [Description("Writes an info log message to the main terminal.  Parameter arguments are supported but not required.")]
        public void LogInfo(string msg, params object[] args)
        {
            if (msg == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _interpreter.Conveyor.EchoInfo(args.Length > 0 ? string.Format(msg, args) : msg);
            });
        }

        /// <summary>
        /// Makes an warning echo.
        /// </summary>
        /// <param name="msg"></param>
        [Description("Writes a warning log message to the main terminal.  Parameter arguments are supported but not required.")]
        public void LogWarning(string msg, params object[] args)
        {
            if (msg == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _interpreter.Conveyor.EchoWarning(args.Length > 0 ? string.Format(msg, args) : msg);
            });
        }

        /// <summary>
        /// Makes an error echo.
        /// </summary>
        /// <param name="msg"></param>
        [Description("Writes an error log entry to the main terminal.  Parameter arguments are supported but not required.")]
        public void LogError(string msg, params object[] args)
        {
            if (msg == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _interpreter.Conveyor.EchoError(args.Length > 0 ? string.Format(msg, args) : msg);
            });
        }

        /// <summary>
        /// Makes a success log echo.
        /// </summary>
        /// <param name="msg"></param>
        [Description("Writes a success log entry to the main terminal.  Parameter arguments are supported but not required.")]
        public void LogSuccess(string msg, params object[] args)
        {
            if (msg == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _interpreter.Conveyor.EchoSuccess(args.Length > 0 ? string.Format(msg, args) : msg);
            });
        }

        /// <summary>
        /// Clears the text in a terminal of a specified window name.
        /// </summary>
        /// <param name="windowName"></param>
        [Description("Clears the default terminal in the specified window.")]
        public void ClearWindow(string windowName)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // This case is if they specified a window that might exist, we'll find it, edit that.
                var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x =>
                        x.WindowType == WindowType.TerminalWindow &&
                        x.Name.Equals(windowName, StringComparison.Ordinal)) as
                    TerminalWindow;

                if (win == null)
                {
                    return;
                }

                win.Text = "";
            });

        }

        /// <summary>
        /// Returns the first non null and non empty value.  If none are found a blank
        /// string will be returned.
        /// </summary>
        [Description("Returns the first non null and non empty value.  If none are found a blank is returned.")]
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
        /// Returns the specified number of characters from the left side of the string.  If more
        /// characters were requested than exist the full string is returned.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        [Description("Returns the specified number of characters from the left side of the string.")]
        public string Left(string str, int length)
        {
            return str.SafeLeft(length);
        }

        /// <summary>
        /// Returns the specified number of characters from the right side of the string.  If more
        /// characters were requested than exist the full string is returned.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        [Description("Returns the specified number of characters from the right side of the string.")]
        public string Right(string str, int length)
        {
            return str.SafeRight(length);
        }

        /// <summary>
        /// Returns the substring starting at the specified index.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        [Description("Returns the substring start for the specified parameters.")]
        public string Substring(string str, int startIndex)
        {
            return str.SafeSubstring(startIndex);
        }

        /// <summary>
        /// Returns the substring starting at the specified index for the specified length.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        [Description("Returns the substring start for the specified parameters.")]
        public string Substring(string str, int startIndex, int length)
        {
            return str.SafeSubstring(startIndex, length);
        }

        /// <summary>
        /// Returns the zero based index of the first occurrence of a string in another string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        [Description("Returns the zero based index of the first occurrence of a string in another string.")]
        public int IndexOf(string str, string search)
        {
            return str.IndexOf(search, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the zero based index of the first occurrence of a string in another string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        /// <param name="start"></param>
        [Description("Returns the zero based index of the first occurrence of a string in another string.")]
        public int IndexOf(string str, string search, int start)
        {
            return str.IndexOf(search, start, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the zero based index of the first occurrence of a string in another string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        [Description("Returns the zero based index of the first occurrence of a string in another string.")]
        public int IndexOf(string str, string search, int start, int length)
        {
            return str.IndexOf(search, start, length, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the zero based index of the last occurrence of a string in another string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        [Description("Returns the zero based index of the last occurrence of a string in another string.")]
        public int LastIndexOf(string str, string search)
        {
            return str.LastIndexOf(search, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the zero based index of the last occurrence of a string in another string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        /// <param name="start"></param>
        [Description("Returns the zero based index of the last occurrence of a string in another string.")]
        public int LastIndexOf(string str, string search, int start)
        {
            return str.LastIndexOf(search, start, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the zero based index of the last occurrence of a string in another string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="search"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        [Description("Returns the zero based index of the last occurrence of a string in another string.")]
        public int LastIndexOf(string str, string search, int start, int length)
        {
            return str.LastIndexOf(search, start, length, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns the current time formatted as either 12-hour or 24-hour.
        /// </summary>
        /// <param name="meridiemTime">Whether or not to return the time in AM/PM format.</param>
        [Description("Returns the current time formatted as either 12-hour or 24-hour.")]
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
        /// </summary>]
        [Description("Returns the current hour.")]
        public int GetHour()
        {
            return DateTime.Now.Hour;
        }

        /// <summary>
        /// Returns the current minute.
        /// </summary>
        [Description("Returns the current minute.")]
        public int GetMinute()
        {
            return DateTime.Now.Minute;
        }

        /// <summary>
        /// Returns the current second.
        /// </summary>
        [Description("Returns the current second.")]
        public int GetSecond()
        {
            return DateTime.Now.Second;
        }

        /// <summary>
        /// Returns the current millisecond.
        /// </summary>
        [Description("Returns the current millisecond.")]
        public int GetMillisecond()
        {
            return DateTime.Now.Millisecond;
        }

        /// <summary>
        /// The minutes elapsed since the start of the day.
        /// </summary>
        [Description("Returns the number of minutes that have elapsed since the start of the day.")]
        public int DailyMinutesElapsed()
        {
            var dt = DateTime.Now;
            return (dt.Hour * 60) + dt.Minute;
        }

        /// <summary>
        /// The seconds elapsed since the start of the day.
        /// </summary>
        [Description("Returns the number of seconds that have elapsed since the start of the day.")]
        public int DailySecondsElapsed()
        {
            var dt = DateTime.Now;
            int minutes = (dt.Hour * 60) + dt.Minute;
            return (minutes * 60) + dt.Second;
        }

        /// <summary>
        /// The milliseconds elapsed since the start of the day.
        /// </summary>
        [Description("Returns the number of milliseconds that have elapsed since the start of the day.")]
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
        [Description("Pauses a Lua script for the designated amount of milliseconds.")]
        public void Sleep(int milliseconds)
        {
            // ReSharper disable once AsyncConverter.AsyncWait
            Task.Delay(milliseconds).Wait();
        }

        /// <summary>
        /// Pauses the lua script for a designated amount of milliseconds.  This should work with both
        /// sync and not sync Lua calls.
        /// </summary>
        /// <param name="milliseconds"></param>
        [Description("Pauses a Lua script for the designated amount of milliseconds.")]
        public void Pause(int milliseconds)
        {
            DispatcherFrame df = new DispatcherFrame();

            new Thread(() =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(milliseconds));
                df.Continue = false;

            }).Start();

            Dispatcher.PushFrame(df);
        }

        /// <summary>
        /// Returns a random number.
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        [Description("Returns a random number between the lower and upper bound.")]
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
        [Description("Returns a random entry from the provided string array.")]
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
        [Description("Returns a random entry from the string provided the string and the delimiter.")]
        public string RandomChoice(string choices, string delimiter)
        {
            if (choices == null || delimiter == null)
            {
                return "";
            }

            return RandomChoice(choices.Split(delimiter));
        }

        /// <summary>
        /// Returns a new GUID.
        /// </summary>
        [Description("Creates a new GUID (Global unique identifier).")]
        public string Guid()
        {
            return System.Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Sets the mud client's title.
        /// </summary>
        /// <param name="title"></param>
        [Description("Sets the mud client's title.")]
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
        [Description("Gets any text that is currently in the scrape buffer.")]
        public string GetScrapedText()
        {
            string buf = "";

            Application.Current.Dispatcher.Invoke(() =>
            {
                buf = _interpreter.Conveyor.Scrape.ToString();
            });

            return buf;
        }

        /// <summary>
        /// Turns text capturing on.
        /// </summary>
        [Description("Turns text scraping on.")]
        public void CaptureOn()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _interpreter.Conveyor.ScrapeEnabled = true;
            }, DispatcherPriority.Send);
        }

        /// <summary>
        /// Turns text capturing off.
        /// </summary>
        [Description("Turns text scraping off.")]
        public void CaptureOff()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _interpreter.Conveyor.ScrapeEnabled = false;
            }, DispatcherPriority.Send);
        }

        /// <summary>
        /// Clears the text capturing buffer.
        /// </summary>
        [Description("Clears the scrape/capture buffer.")]
        public void CaptureClear()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _interpreter.Conveyor.ScrapeEnabled = false;
                _interpreter.Conveyor.Scrape.Clear();
            });
        }

        /// <summary>
        /// Checks if a string exists in another string (Case Sensitive).
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="contains"></param>
        [Description("Checks if a string exists in another string.")]
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
        [Description("Checks if a string exists in another string.")]
        public bool Contains(string buf, string contains, bool ignoreCase)
        {
            if (ignoreCase)
            {
                return buf.Contains(contains, StringComparison.OrdinalIgnoreCase);
            }

            return buf.Contains(contains, StringComparison.Ordinal);
        }

        /// <summary>
        /// Removes non alpha characters but allows for an exceptions list of chars to be provided that
        /// should be included.
        /// </summary>
        /// <param name="buf">The string to remove all non Alpha characters from.</param>
        /// <param name="includeAlso">A string treated like a char array, if any individual characters exist in
        /// the base string then those characters will be allowed through.  This will allow for exceptions with
        /// punctuation, white space, etc.</param>
        [Description("Removes non alpha characters but allows for an exceptions list of chars to be provieed that should be left.")]
        public string RemoveNonAlpha(string buf, string includeAlso = "")
        {
            if (buf.IsNullOrEmptyOrWhiteSpace())
            {
                return "";
            }

            var sb = new StringBuilder();

            foreach (var c in buf)
            {
                if (char.IsLetter(c) || includeAlso.Contains(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Trims whitespace off of the front and end of a string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Trims whitespace off the front and end of the string.")]
        public string Trim(string buf)
        {
            return buf?.Trim() ?? "";
        }

        /// <summary>
        /// Trims whitespace off of the front and end of a string.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="trimOff"></param>
        [Description("Trims whitespace off the front and end of the string.")]
        public string Trim(string buf, string trimOff)
        {
            return buf?.Trim(trimOff) ?? "";
        }

        /// <summary>
        /// Trims whitespace off of the start of a string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Trims whitespace off the start of a string.")]
        public string TrimStart(string buf)
        {
            return buf?.TrimStart() ?? "";
        }

        /// <summary>
        /// Trims whitespace off of the start of a string.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="trimOff"></param>
        [Description("Trims whitespace off the start of a string.")]
        public string TrimStart(string buf, string trimOff)
        {
            return buf?.TrimStart(trimOff) ?? "";
        }

        /// <summary>
        /// Trims whitespace off the end of a string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Trims whitespace off the end of a string.")]
        public string TrimEnd(string buf)
        {
            return buf?.TrimEnd() ?? "";
        }

        /// <summary>
        /// Trims whitespace off the end of a string.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="trimOff"></param>
        [Description("Trims whitespace off the end of a string.")]
        public string TrimEnd(string buf, string trimOff)
        {
            return buf?.TrimEnd(trimOff) ?? "";
        }

        /// <summary>
        /// Splits a string into a string array using a specified delimiter.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="delimiter"></param>
        [Description("Splits a string into a string array using the specified delimiter")]
        public string[] Split(string buf, string delimiter)
        {
            return buf?.Split(delimiter);
        }

        /// <summary>
        /// Searches an array for whether a specified value exists within it.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="searchValue"></param>
        [Description("Searches an array for whether a specified value exists within it.")]
        public bool ArrayContains(string[] array, string searchValue)
        {
            if (array == null)
            {
                return false;
            }

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
        /// Removes all ANSI control sequences.
        /// </summary>
        /// <param name="str"></param>
        [Description("Removes all ANSI control sequences.")]
        public string RemoveAnsiCodes(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return "";
            }

            return Colorizer.RemoveAllAnsiCodes(str);
        }

        /// <summary>
        /// Removes all ANSI control sequences.
        /// </summary>
        /// <param name="array"></param>
        [Description("Removes all ANSI control sequences.")]
        public string[] RemoveAnsiCodes(string[] array)
        {
            if (array == null)
            {
                return null;
            }

            var list = new List<string>();

            foreach (string line in array)
            {
                list.Add(Colorizer.RemoveAllAnsiCodes(line));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Removes empty elements from an array.
        /// </summary>
        /// <param name="array"></param>
        [Description("Removes empty elements from an array.")]
        public string[] RemoveElementsEmpty(string[] array)
        {
            return array?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        }

        /// <summary>
        /// Removes elements from an array starting with a specified string.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="str"></param>
        [Description("Removes elements from an array starting with a specified string.")]
        public string[] RemoveElementsStartsWith(string[] array, string str)
        {
            return array?.Where(x => !x.StartsWith(str)).ToArray();
        }

        /// <summary>
        /// Removes elements from an array ending with a specified string.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="str"></param>
        [Description("Remove elements from an array ending with a specified string.")]
        public string[] RemoveElementsEndingWith(string[] array, string str)
        {
            return array?.Where(x => !x.EndsWith(str)).ToArray();
        }

        /// <summary>
        /// Removes elements from an array containing a specified string.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="str"></param>
        [Description("Removes elements from an array containing a specified string.")]
        public string[] RemoveElementsContains(string[] array, string str)
        {
            return array?.Where(x => !x.Contains(str)).ToArray();
        }

        /// <summary>
        /// Adds an item to a list.  Duplicate items are acceptable.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        [Description("Adds an item to a string list.")]
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
        [Description("Adds an item to a string list at the start.")]
        public string ListAddStart(string sourceList, string value, char delimiter = '|')
        {            
            return $"{value}|{sourceList}".Trim('|');
        }

        /// <summary>
        /// Adds an item to a list only if it doesn't exist.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        [Description("Adds an item to a list only if it doesn't already exist.")]
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
        [Description("Removes an item from a string list.")]
        public string ListRemove(string sourceList, string value, char delimiter = '|')
        {
            var list = sourceList.Split(delimiter);
            var sb = Argus.Memory.StringBuilderPool.Take();

            try
            {
                foreach (string item in list)
                {
                    if (!item.Equals(value, StringComparison.Ordinal))
                    {
                        sb.AppendFormat("|{0}", value);
                    }
                }

                return sb.ToString().Trim('|');
            }
            finally
            {
                Argus.Memory.StringBuilderPool.Return(sb);
            }
        }

        /// <summary>
        /// Removes 1 to n items from the end of a list.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="items"></param>
        /// <param name="delimiter"></param>
        [Description("Removes 1 to n items from the end of a list.")]
        public string ListRemove(string sourceList, int items, char delimiter = '|')
        {
            var list = sourceList.Split(delimiter);
            var sb = Argus.Memory.StringBuilderPool.Take();

            try
            {
                int keep = list.Count() - items;

                for (int i = 0; i < keep; i++)
                {
                    sb.AppendFormat("|{0}", list[i]);
                }

                return sb.ToString().Trim('|');
            }
            finally
            {
                Argus.Memory.StringBuilderPool.Return(sb);
            }
        }

        /// <summary>
        /// If an item exists in a list.
        /// </summary>
        /// <param name="sourceList"></param>
        /// <param name="value"></param>
        /// <param name="delimiter"></param>
        [Description("If an item exists in a string list.")]
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
        [Description("Returns a new string with all occurrences of a string replaced with another string.")]
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
        [Description("Enables all aliases and triggers in a group.")]
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
        [Description("Disables all aliases and triggers in a group.")]
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
        [Description("Adds a scheduled task (command or Lua) to be executed after a designated time.")]
        public void AddScheduledTask(string command, bool isLua, int seconds)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => AddScheduledTask(command, isLua, seconds)));
                return;
            }

            App.MainWindow.ScheduledTasks.AddTask(command, isLua, DateTime.Now.AddSeconds(seconds));
        }

        /// <summary>
        /// Adds a batch task (command or Lua) to be executed in order when the batch is run.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="isLua"></param>
        [Description("Adds a batch task (command or Lua) to be executed in order when the batch is run.")]
        public void AddBatchTask(string command, bool isLua)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => AddBatchTask(command, isLua)));
                return;
            }

            App.MainWindow.BatchTasks.AddTask(command, isLua);
        }

        /// <summary>
        /// Starts the current batch processing.
        /// </summary>
        /// <param name="secondsInBetweenCommands"></param>
        [Description("Starts the current batch processing.")]
        public void StartBatch(int secondsInBetweenCommands)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => StartBatch(secondsInBetweenCommands)));
                return;
            }

            App.MainWindow.BatchTasks.StartBatch(secondsInBetweenCommands);
        }

        /// <summary>
        /// Clears all tasks from the scheduled tasks queue.
        /// </summary>
        [Description("Clears all tasks from the scheduled tasks queue.")]
        public void ClearTasks()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(ClearTasks));
                return;
            }

            App.MainWindow.ScheduledTasks.ClearTasks();
        }

        /// <summary>
        /// Formats a number as string with commas and no decimal places.
        /// </summary>
        /// <param name="value"></param>
        [Description("Formats a number as a string.")]
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
        [Description("Formats a number as a string.")]
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
        [Description("Returns the last non-empty line in the game terminal.\r\nNote: The last non-empty line might not be the line that fired a trigger.")]
        public string LastNonEmptyLine()
        {
            string buf = "";

            Application.Current.Dispatcher.Invoke(() =>
            {
                buf = App.MainWindow.GameTerminal.LastNonEmptyLine;
            });

            return buf ?? "";
        }

        /// <summary>
        /// Returns a string array of the requested number of last lines from the game terminal.
        /// </summary>
        /// <param name="numberToTake"></param>
        [Description("Returns a string array of the requested number of last lines from the game terminal.")]
        public string[] LastLines(int numberToTake)
        {
            return LastLines(numberToTake, true);
        }

        /// <summary>
        /// Returns a string array of the requested number of last lines from the game terminal.
        /// </summary>
        /// <param name="numberToTake"></param>
        /// <param name="reverseOrder">Whether the order of the array should be reversed.  True will return oldest line to newest, False will be newest to oldest.</param>
        [Description("Returns a string array of the requested number of last lines from the game terminal.")]
        public string[] LastLines(int numberToTake, bool reverseOrder)
        {
            var list = new List<string>();

            Application.Current.Dispatcher.Invoke(() =>
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
            });

            if (reverseOrder)
            {
                list.Reverse();
            }

            return list.ToArray();
        }

        /// <summary>
        /// Returns the last lines oldest to newest where the start line contains the search pattern.
        /// </summary>
        /// <param name="startLineContains"></param>
        [Description("Returns the last lines oldest to newest where the search pattern matches.")]
        public string[] LastLinesBetweenContains(string startLineContains)
        {
            var list = new List<string>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                string text = "";
                int i = App.MainWindow.GameTerminal.Document.LineCount;

                while (string.IsNullOrEmpty(text) && i > 0)
                {
                    var line = App.MainWindow.GameTerminal.Document.GetLineByNumber(i);
                    list.Add(App.MainWindow.GameTerminal.Document.GetText(line.Offset, line.Length).RemoveAnsiCodes());

                    i--;

                    // Break if the line contains the search string.
                    if (i < 0 || list.Last().Contains(startLineContains))
                    {
                        break;
                    }
                }
            });

            list.Reverse();

            return list.ToArray();
        }

        /// <summary>
        /// Returns the last lines oldest to newest where the start line contains the search pattern and
        /// the end line contains it's search pattern.  Both patterns must be found or the list will be
        /// empty.
        /// </summary>
        /// <param name="startLineContains"></param>
        /// <param name="endLineContains"></param>
        [Description("Returns the last lines oldest to newest where the search pattern matches.")]
        public string[] LastLinesBetweenContains(string startLineContains, string endLineContains)
        {
            bool found = false;
            var list = new List<string>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                string text = "";
                int i = App.MainWindow.GameTerminal.Document.LineCount;

                while (string.IsNullOrEmpty(text) && i > 0)
                {
                    var line = App.MainWindow.GameTerminal.Document.GetLineByNumber(i);
                    list.Add(App.MainWindow.GameTerminal.Document.GetText(line.Offset, line.Length).RemoveAnsiCodes());

                    i--;

                    // Break if the line contains the search string.
                    if (i < 0 || list.Last().Contains(startLineContains))
                    {
                        found = true;
                        break;
                    }
                }
            });

            // If there is no starting pattern nothing should be returned.
            if (!found)
            {
                return Array.Empty<string>();
            }

            list.Reverse();

            // Find the line where the next occurrence is found, then remove all the lines after that line.
            int lastLine = 0;

            for (int x = 0; x < list.Count; x++)
            {
                // Break if the line contains the end line search string so we don't remove anymore.
                if (list[x].Contains(endLineContains))
                {
                    lastLine = x;
                    break;
                }
            }

            // Remove everything after the last line.
            for (int x = list.Count - 1; x >= 0; x--)
            {
                if (x > lastLine)
                {
                    list.RemoveAt(x);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Returns the last lines oldest to newest where the start line contains the search pattern.
        /// </summary>
        /// <param name="startLineStartsWith"></param>
        [Description("Returns the last lines oldest to newest where the search pattern matches.")]
        public string[] LastLinesBetweenStartsWith(string startLineStartsWith)
        {
            var list = new List<string>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                string text = "";
                int i = App.MainWindow.GameTerminal.Document.LineCount;

                while (string.IsNullOrEmpty(text) && i > 0)
                {
                    var line = App.MainWindow.GameTerminal.Document.GetLineByNumber(i);
                    list.Add(App.MainWindow.GameTerminal.Document.GetText(line.Offset, line.Length).RemoveAnsiCodes());

                    i--;

                    // Break if the line contains the search string.
                    if (i < 0 || list.Last().StartsWith(startLineStartsWith))
                    {
                        break;
                    }
                }
            });

            list.Reverse();

            return list.ToArray();
        }


        /// <summary>
        /// Returns the last lines oldest to newest where the start line contains the search pattern and
        /// the end line contains it's search pattern.  Both patterns must be found or the list will be
        /// empty.
        /// </summary>
        /// <param name="startLineStartsWith"></param>
        /// <param name="endLineStartsWith"></param>
        [Description("Returns the last lines oldest to newest where the search pattern matches.")]
        public string[] LastLinesBetweenStartsWith(string startLineStartsWith, string endLineStartsWith)
        {
            bool found = false;
            var list = new List<string>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                string text = "";
                int i = App.MainWindow.GameTerminal.Document.LineCount;

                while (string.IsNullOrEmpty(text) && i > 0)
                {
                    var line = App.MainWindow.GameTerminal.Document.GetLineByNumber(i);
                    list.Add(App.MainWindow.GameTerminal.Document.GetText(line.Offset, line.Length).RemoveAnsiCodes());

                    i--;

                    // Break if the line contains the search string.
                    if (i < 0 || list.Last().StartsWith(startLineStartsWith))
                    {
                        found = true;
                        break;
                    }
                }
            });

            // If there is no starting pattern nothing should be returned.
            if (!found)
            {
                return Array.Empty<string>();
            }

            list.Reverse();

            // Find the line where the next occurrence is found, then remove all the lines after that line.
            int lastLine = 0;

            for (int x = 0; x < list.Count; x++)
            {
                // Break if the line contains the end line search string so we don't remove anymore.
                if (list[x].StartsWith(endLineStartsWith))
                {
                    lastLine = x;
                    break;
                }
            }

            // Remove everything after the last line.
            for (int x = list.Count - 1; x >= 0; x--)
            {
                if (x > lastLine)
                {
                    list.RemoveAt(x);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Returns whether the string is a number.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Whether the string provided is a number or not.")]
        public bool IsNumber(string buf)        
        {
            return buf.IsNumeric();
        }

        /// <summary>
        /// If the number is even.
        /// </summary>
        /// <param name="value"></param>
        [Description("Whether the number provided is an even number.")]
        public bool IsEven(int value)
        {
            return value.IsEven();
        }

        /// <summary>
        /// If the number is odd.
        /// </summary>
        /// <param name="value"></param>
        [Description("Whether the number provided is an odd number.")]
        public bool IsOdd(int value)
        {            
            return value.IsOdd();
        }

        /// <summary>
        /// If the number is of the specified interval.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="interval"></param>
        [Description("Whether the number provided is of the specified interval.")]
        public bool IsInterval(int value, int interval)
        {           
            return value.IsInterval(interval);
        }

        /// <summary>
        /// Returns the value if it falls in the range of the max and min.  Otherwise it returns
        /// the upper or lower boundary depending on which one the value passed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        [Description("Returns the value if it falls in the range of the max and min.")]
        public int Clamp(int value, int min, int max)
        {            
            return value.Clamp(min, max);
        }

        /// <summary>
        /// Deletes the specified number of characters off the start of the string.  If the length
        /// is greater than the length of the string an empty string is returned.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="length"></param>
        [Description("Deletes the specified number of characters off the start of the string.")]
        public string DeleteLeft(string buf, int length)
        {
            return buf.DeleteLeft(length);
        }

        /// <summary>
        /// Deletes the specified number of characters off the end of the string.  If the length
        /// is greater than the length of the string an empty string is returned.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="length"></param>
        [Description("Deletes the specified number of characters off the end of the string.")]
        public string DeleteRight(string buf, int length)
        {
            return buf.DeleteRight(length);
        }

        /// <summary>
        /// Returns the first word in the specified string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Returns the first word in the specified string.")]
        public string FirstWord(string buf)
        {
            return buf.FirstWord();
        }

        /// <summary>
        /// Returns the second word in the specified string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Returns the second word in the specified string.")]
        public string SecondWord(string buf)
        {
            return buf.SecondWord();
        }

        /// <summary>
        /// Returns the third word in the specified string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Returns the third word in the specified string.")]
        public string ThirdWord(string buf)
        {
            return buf.ThirdWord();
        }

        /// <summary>
        /// Returns the word by index from the provided string as delimited by spaces.  The delimiter
        /// can also be provided to specify a different split character.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="wordNumber"></param>
        /// <param name="delimiter"></param>
        [Description("Returns the word by index in the specified string.")]
        public string ParseWord(string buf, int wordNumber, string delimiter = " ")
        {
            return buf.ParseWord(wordNumber, delimiter);
        }

        /// <summary>
        /// Returns a string with the specified word removed by index.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="wordIndex"></param>
        [Description("Returns word by index from the provided string.")]
        public string RemoveWord(string buf, int wordIndex)
        {
            return buf.RemoveWord(wordIndex);
        }

        /// <summary>
        /// Returns the string between the start marker and the end marker.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="beginMarker"></param>
        /// <param name="endMarker"></param>
        [Description("Returns the string between the start and the end marker.")]
        public string Between(string buf, string beginMarker, string endMarker)
        {            
            return buf.Between(beginMarker, endMarker);
        }

        /// <summary>
        /// Converts a string to Base64.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Converts the string to Base64.")]
        public string ToBase64(string buf)
        {
            return buf.ToBase64();
        }

        /// <summary>
        /// Converts a Base64 string back to it's original state.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Converts the string from Base64.")]
        public string FromBase64(string buf)
        {
            
            return buf.FromBase64();
        }

        /// <summary>
        /// HTML Encodes a string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("HTML Encodes the string.")]
        public string HtmlEncode(string buf)
        {
            return buf.HtmlEncode();
        }

        /// <summary>
        /// HTML decodes a string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("HTML Decodes the string.")]
        public string HtmlDecode(string buf)
        {            
            return buf.HtmlDecode();
        }

        /// <summary>
        /// URL Encodes a string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("URL encodes the string.")]
        public string UrlEncode(string buf)
        {
            return buf.UrlEncode();
        }

        /// <summary>
        /// URL Decodes a string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("URL decodes the string.")]
        public string UrlDecode(string buf)
        {
            return buf.UrlDecode();
        }

        /// <summary>
        /// Returns the word count in the specified string.
        /// </summary>
        /// <param name="buf"></param>
        [Description("Counts the number of words in the specified string.")]
        public int WordCount(string buf)
        {
            return buf.WordCount();
        }

        /// <summary>
        /// Returns a string that right aligns the instance by padding characters onto the left
        /// until the total width is attained.  If the total width is less than the provided string
        /// the provided string is returned.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="totalWidth"></param>
        [Description("Pads the left side of a string until the string is the specified number of characters.")]
        public string PadLeft(string buf, int totalWidth)
        {
            return buf.PadLeft(totalWidth);
        }

        /// <summary>
        /// Returns a string that left aligns the instance by padding characters onto the the right
        /// until the total width is attained.  If the total width is less than the provided string
        /// the provided string is returned.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="totalWidth"></param>
        [Description("Pads the right side of a string until the string is the specified number of characters.")]
        public string PadRight(string buf, int totalWidth)
        {            
            return buf.PadRight(totalWidth);
        }

        /// <summary>
        /// Returns the MD5 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        [Description("Returns the MD5 hash for the provided string.")]
        public string MD5(string value)
        {
            return Argus.Cryptography.HashUtilities.MD5Hash(value);
        }

        /// <summary>
        /// Returns the SHA256 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        [Description("Provides the SHA256 hash for the given string.")]
        public string SHA256(string value)
        {
            return Argus.Cryptography.HashUtilities.Sha256Hash(value);
        }

        /// <summary>
        /// Returns the SHA512 hash for the given string.
        /// </summary>
        /// <param name="value"></param>
        [Description("Provides the SHA512 hash for the given string.")]
        public string SHA512(string value)
        {
            return Argus.Cryptography.HashUtilities.Sha512Hash(value);
        }

        /// <summary>
        /// Removes all lines from a string that start with the specified text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="searchText"></param>
        /// <remarks>Either parameter being null returns either the text if it's not null or a blank string if it was null.</remarks>
        [Description("Removes all lines from a string that start with the specified text.")]
        public string RemoveLinesStartingWith(string text, string searchText)
        {
            if (text.IsNullOrEmptyOrWhiteSpace() || searchText.IsNullOrEmptyOrWhiteSpace())
            {
                return text ?? "";
            }

            var sb = Argus.Memory.StringBuilderPool.Take();

            try
            {
                foreach (var item in (string[])text.Split('\n'))
                {
                    if (!item.StartsWith(searchText))
                    {
                        sb.AppendLine(item.TrimEnd('\r', '\n'));
                    }
                }

                return sb.ToString().TrimEnd('\r', '\n');
            }
            finally
            {
                Argus.Memory.StringBuilderPool.Return(sb);
            }
        }

        /// <summary>
        /// Removes all lines from a string that end with the specified text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="searchText"></param>
        /// <remarks>Either parameter being null returns either the text if it's not null or a blank string if it was null.</remarks>
        [Description("Removes all lines from a string that end with the specified text5.")]
        public string RemoveLinesEndingWith(string text, string searchText)
        {
            if (text.IsNullOrEmptyOrWhiteSpace() || searchText.IsNullOrEmptyOrWhiteSpace())
            {
                return text ?? "";
            }

            var sb = Argus.Memory.StringBuilderPool.Take();

            try
            {
                foreach (var item in (string[])text.Split('\n'))
                {
                    if (!item.EndsWith(searchText))
                    {
                        sb.AppendLine(item.TrimEnd('\r', '\n'));
                    }
                }

                return sb.ToString().TrimEnd('\r', '\n');
            }
            finally
            {
                Argus.Memory.StringBuilderPool.Return(sb);
            }
        }

        /// <summary>
        /// If a string starts with another string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="searchText"></param>
        [Description("If a string starts with another string.")]
        public bool StartsWith(string text, string searchText)
        {
            if (text == null || searchText == null)
            {
                return false;
            }

            return text.StartsWith(searchText);
        }

        /// <summary>
        /// If a string ends with another string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="searchText"></param>
        [Description("If a string ends with another string.")]
        public bool EndsWith(string text, string searchText)
        {
            if (text == null || searchText == null)
            {
                return false;
            }

            return text.EndsWith(searchText);
        }

        /// <summary>
        /// The number of Lua scripts that are actively running.
        /// </summary>
        [Description("The number of Lua scripts that are currently running.")]
        public int LuaScriptsActive()
        {
            // TODO: Scripting
            return -1;
            //return ((Interpreter)_interpreter).LuaCaller.ActiveLuaScripts;
        }

        /// <summary>
        /// The current location of the profile save directory.
        /// </summary>
        [Description("The profile directory for the mud client.")]
        public string ProfileDirectory()
        {
            return App.Settings.AvalonSettings.SaveDirectory;
        }

        /// <summary>
        /// Where the avalon setting file is stored that among other things has where the profile
        /// save directory is (in case that is in Dropbox, OneDrive, etc.).
        /// </summary>
        [Description("The app data directory for the mud client.")]
        public string AppDataDirectory()
        {
            return App.Settings.AppDataDirectory;
        }

        /// <summary>
        /// Adds a SQL command to the SqlTasks queue.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        [Description("Executes a parameterized SQL statement via the buffered execute queue.")]
        public void DbExecute(string sql, params string[] parameters)
        {
            Application.Current.Dispatcher.Invoke(() => App.MainWindow.SqlTasks.Add(sql, parameters));
        }

        /// <summary>
        /// Executes a SQL command immediately outside of a transaction.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        [Description("Executes a parameterized SQL statement immediately and outside of a transaction.")]
        public void DbExecuteImmediate(string sql, params string[] parameters)
        {
            Application.Current.Dispatcher.Invoke(() => App.MainWindow.SqlTasks.ExecuteNonQueryAsync(sql, parameters));
        }

        /// <summary>
        /// Selects one value from the database.  If an error occurs it is written to the terminal.
        /// and an empty string is returned.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        [Description("Selects one value from the database via a parameterized query.")]
        public object DbSelectValue(string sql, params string[] parameters)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    return App.MainWindow.SqlTasks.SelectValue(sql, parameters);
                }
                catch (Exception ex)
                {
                    App.Conveyor.EchoError(ex.Message);
                    return "";
                }
            });
        }

        /// <summary>
        /// Selects a record set that can be iterated over in Lua as a table.  If an error occurs an it is
        /// written to the terminal and an empty result is returned.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        [Description("Selects a record set from the database via a paramaterized query.")]
        public IEnumerable<Dictionary<string, object>> DbSelect(string sql, params string[] parameters)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    return App.MainWindow.SqlTasks.Select(sql, parameters);
                }
                catch (Exception ex)
                {
                    App.Conveyor.EchoError(ex.Message);
                    return Enumerable.Empty<Dictionary<string, object>>();
                }
            });
        }

        /// <summary>
        /// Forces all pending database operations to be committed.
        /// </summary>
        [Description("Flushes all pending database operations that are in the buffered queue to the database.")]
        public void DbFlush()
        {
            Application.Current.Dispatcher.Invoke(() => App.MainWindow.SqlTasks.Flush());
        }

        /// <summary>
        /// Downloads a string from a URL using the GET method.
        /// </summary>
        /// <param name="url"></param>
        [Description("Downloads a string from a URL using the GET method.")]
        public string HttpGet(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }

        /// <summary>
        /// Downloads a string from a URL using the POST method.  Data is a formatted string
        /// posted as a form in the format: "Time = 12:00am temperature = 50";
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        [Description("Downloads a string from a URL using the POST method.")]
        public string HttpPost(string url, string data)
        {
            using (var client = new WebClient())
            {
                return client.UploadString(url, data);
            }
        }

        /// <summary>
        /// Sets the main status bar text with an optional icon lookup by name.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="iconName"></param>
        [Description("Sets the status bar text.")]
        public void SetText(string buf, string iconName)
        {
            this.SetText(buf, TextTarget.StatusBarText, iconName);
        }

        /// <summary>
        /// Sets the text on a specified status bar an optional Enum value for the icon.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="target"></param>
        /// <param name="icon"></param>
        [Description("Sets the status bar text.")]
        public void SetText(string buf, TextTarget target = TextTarget.StatusBarText, PackIconMaterialKind icon = PackIconMaterialKind.None)
        {
            App.Conveyor.SetText(buf, target, icon);
        }

        /// <summary>
        /// Sets the text on a specified status bar an optional text value look for the icon.
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="target"></param>
        /// <param name="iconName"></param>
        [Description("Sets the status bar text.")]
        public void SetText(string buf, TextTarget target = TextTarget.StatusBarText, string iconName = "None")
        {
            PackIconMaterialKind icon = PackIconMaterialKind.None;

            try
            {
                icon = (PackIconMaterialKind)Enum.Parse(typeof(PackIconMaterialKind), iconName, true);
            }
            catch (Exception ex)
            {
                App.Conveyor.EchoLog(string.IsNullOrWhiteSpace(iconName)
                                        ? $"A valid icon name is required: {ex.Message}"
                                        : $"Icon name '{iconName}' was not found: {ex.Message}", LogType.Error);
            }

            App.Conveyor.SetText(buf, target, icon);
        }

        /// <summary>
        /// Replaces the last occurrence of a string with another string in the main game terminal.
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="replacementText"></param>
        [Description("Replaces the last occurrence of a string with another string in the main game terminal.")]
        public void TerminalReplaceLastInstance(string searchText, string replacementText)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var sb = Argus.Memory.StringBuilderPool.Take(replacementText);
                Colorizer.MudToAnsiColorCodes(sb);
                App.MainWindow.GameTerminal.ReplaceLastInstance(searchText, sb.ToString());
                Argus.Memory.StringBuilderPool.Return(sb);
            });
        }

        /// <summary>
        /// Replaces all instances of one string with another string in the main game terminal.
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="replacementText"></param>
        [Description("Replaces all instances of one string with another string in the main game terminal.")]
        public void TerminalReplaceAll(string searchText, string replacementText)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var sb = Argus.Memory.StringBuilderPool.Take(replacementText);
                Colorizer.MudToAnsiColorCodes(sb);
                App.MainWindow.GameTerminal.ReplaceAll(searchText, sb.ToString(), false);
                Argus.Memory.StringBuilderPool.Return(sb);
            });
        }

        /// <summary>
        /// Removes a line from the game terminal.
        /// </summary>
        /// <param name="lineNumber"></param>
        [Description("Removes the specified line from the main game terminal.")]
        public void TerminalRemoveLine(int lineNumber)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                App.MainWindow.GameTerminal.RemoveLine(lineNumber);
            });
        }

        /// <summary>
        /// Scrolls the main game terminal to the last line.
        /// </summary>
        /// <param name="lineNumber"></param>
        [Description("Scrolls the main game terminal to the last line.")]
        public void TerminalScrollToLastLine(int lineNumber)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                App.MainWindow.GameTerminal.ScrollToEnd();
            });
        }

        /// <summary>
        /// Adds or replaces a replacement trigger.  A replacement is first identified by ID if provided and falls
        /// back to pattern if not.
        /// </summary>
        /// <param name="replace"></param>
        /// <param name="replaceWith"></param>
        /// <param name="id"></param>
        /// <param name="temp"></param>
        [Description("Adds or replaces a replacement trigger.  If a replacement the procedure will first search by ID if specified fall back to pattern if not.")]
        public void AddLineTransformer(string replace, string replaceWith, string id, bool temp = false)
        {
            // TODO, needs script host (get new trigger from conveyor, that way it can inject the proper stuff).
            // TODO, escape input
            Common.Triggers.Trigger t;
            string luaCode = $"return \"{replaceWith}\"";

            if (id == null)
            {
                t = App.Settings.ProfileSettings.TriggerList.Find(x => x.Pattern.Equals(replace, StringComparison.Ordinal));
            }
            else
            {
                t = App.Settings.ProfileSettings.TriggerList.Find(x => x.Identifier.Equals(id, StringComparison.Ordinal));
            }

            if (t == null)
            {
                var trigger = new Common.Triggers.Trigger();

                // ID must be set before the code so the property function name is set.
                if (!string.IsNullOrWhiteSpace(id))
                {
                    trigger.Identifier = id;
                }

                trigger.Pattern = replace;
                trigger.Command = luaCode;
                trigger.Temp = temp;
                trigger.LineTransformer = true;
                trigger.IsLua = true;
                trigger.ExecuteAs = ExecuteType.LuaMoonsharp;

                App.Settings.ProfileSettings.TriggerList.Add(trigger);
            }
            else
            {
                t.Pattern = replace;
                t.Command = luaCode;
                t.Temp = temp;
                t.LineTransformer = true;
                t.IsLua = true;
                t.ExecuteAs = ExecuteType.LuaMoonsharp;
            }            
        }

        /// <summary>
        /// Deletes a replacement trigger by ID.
        /// </summary>
        /// <param name="id"></param>
        [Description("Deletes replacement trigger by ID.")]
        public void RemoveLineTransformer(string id)
        {
            var t = App.Settings.ProfileSettings.TriggerList.Find(x => x.Identifier.Equals(id, StringComparison.Ordinal));
            App.Settings.ProfileSettings.TriggerList.Remove(t);
        }

        private readonly IInterpreter _interpreter;
    }
}