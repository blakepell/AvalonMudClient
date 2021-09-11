/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Colors;
using Avalon.Common.Colors;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using MoonSharp.Interpreter;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;
using Line = Avalon.Common.Models.Line;

namespace Avalon.Lua
{
    /// <summary>
    /// Script commands that center around creating and manipulating various provided
    /// window types that will be housed in the 'win' namespace.
    /// </summary>
    public class WindowScriptCommands
    {
        private readonly IInterpreter _interpreter;

        public WindowScriptCommands(IInterpreter interp)
        {
            _interpreter = interp;
        }

        /// <summary>
        /// Creates a new window.  This will also activate an existing window.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="windowType"></param>
        [Description("Creates a new instance of a window.  If the window already exists nothing is done.  This will also activate an existing window.")]
        public void New(string windowName, WindowType windowType = WindowType.TerminalWindow)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => this.New(windowName, windowType));
                return;
            }

            // Does the specified window already exist by name?
            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            // The window already existed, get out.
            if (win != null)
            {
                return;
            }

            switch (windowType)
            {
                case WindowType.Default:
                case WindowType.TerminalWindow:
                    win = new TerminalWindow() { Name = windowName };
                    _interpreter.Conveyor.WindowList.Add(win);

                    break;
                case WindowType.CompassWindow:
                    win = new CompassWindow() { Name = windowName };
                    _interpreter.Conveyor.WindowList.Add(win);

                    break;
                default:
                    throw new Exception($"The specified WindowType of {windowType.ToString()} was invalid.");
            }

            win.Show();
            win.Activate();
        }


        /// <summary>
        /// Shows a window that already exists and attempts to bring it to the foreground.
        /// </summary>
        /// <param name="windowName"></param>
        [Description("Shows a window that already exists and attempts to bring it to the foreground.")]
        public void Show(string windowName)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => this.New(windowName));
                return;
            }

            // Does the specified window already exist by name?
            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            // The window doesn't exist, get out.
            if (win == null)
            {
                return;
            }

            win.Show();
            win.Activate();
        }

        /// <summary>
        /// Closes the specified window if it is found.
        /// </summary>
        /// <param name="windowName"></param>
        [Description("Closes the specified window if it is found.")]
        public void Close(string windowName)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => this.Close(windowName));
                return;
            }

            // Does the specified window already exist by name?
            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            // The window doesn't exist, get out.
            if (win == null)
            {
                return;
            }

            win.Close();
        }

        /// <summary>
        /// Closes all custom windows.
        /// </summary>
        [Description("Closes all custom windows")]
        public void CloseAll()
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(this.CloseAll);
                return;
            }

            for (int i = _interpreter.Conveyor.WindowList.Count - 1; i >= 0; i--)
            {
                var win = _interpreter.Conveyor.WindowList[i];
                win?.Close();
            }
        }

        /// <summary>
        /// Echos text to a terminal window.
        /// </summary>
        /// <param name="windowName">The name of the terminal window.</param>
        /// <param name="text">The text to echo.  Mud color codes are supported.</param>
        /// <param name="includeTimeStamp">If a timestamp should be prepended to the line.</param>
        [Description("Writes text to a terminal window.")]
        public void Echo(string windowName, string text, bool includeTimeStamp = false)
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

                var sb = Argus.Memory.StringBuilderPool.Take();

                if (includeTimeStamp)
                {
                    sb.Append('[').Append(DateTime.Now.ToString(@"hh:mm:ss tt", new CultureInfo("en-US"))).Append("]: ");
                }

                sb.Append(text);

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
        /// Echos text to a terminal window.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="text">The text to echo.  Mud color codes are supported.</param>
        /// <param name="includeTimeStamp">If a timestamp should be prepended to the line.</param>
        [Description("Writes text to a terminal window.")]
        public void EchoLine(string windowName, string text, bool includeTimeStamp = false)
        {
            Echo(windowName, $"{text}\r\n", includeTimeStamp);
        }

        /// <summary>
        /// Clears the text in a terminal window.
        /// </summary>
        /// <param name="windowName">The name of the terminal window.</param>
        [Description("Clears the text in a terminal window.")]
        public void Clear(string windowName)
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

                win.Clear();
            });
        }

        /// <summary>
        /// Sets the left coordinate for the specified window.  If the window does not
        /// exist no action is performed.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="left"></param>
        [Description("Sets the left coordinate for the specified window.")]
        public void SetLeft(string windowName, int left)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetLeft(windowName, left)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            if (win == null)
            {
                return;
            }

            win.Left = left;
        }

        /// <summary>
        /// Sets the top coordinate for the specified window.  If the window does not
        /// exist no action is performed.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="top"></param>
        [Description("Sets the top coordinate for the specified window.")]
        public void SetTop(string windowName, int top)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetTop(windowName, top)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            if (win == null)
            {
                return;
            }

            win.Top = top;
        }

        /// <summary>
        /// Sets the height for the specified window.  If the window does not
        /// exist no action is performed.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="height"></param>
        [Description("Sets the top coordinate for the specified window.")]
        public void SetHeight(string windowName, int height)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetHeight(windowName, height)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            if (win == null)
            {
                return;
            }

            win.Height = height;
        }

        /// <summary>
        /// Sets the width for the specified window.  If the window does not
        /// exist no action is performed.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="width"></param>
        [Description("Sets the top coordinate for the specified window.")]
        public void SetWidth(string windowName, int width)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetWidth(windowName, width)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            if (win == null)
            {
                return;
            }

            win.Width = width;
        }

        /// <summary>
        /// Sets the opacity for the specified window.  If the window does not exist no action
        /// is performed.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="opacity"></param>
        [Description("Sets the opacity for the specified window.")]
        public void SetOpacity(string windowName, double opacity)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetOpacity(windowName, opacity)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            if (win == null)
            {
                return;
            }

            win.Opacity = opacity;
        }

        /// <summary>
        /// Sets all attributes for the coordinates of the window.  If the
        /// window does not exist no action is performed.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        [Description("Sets both the top the left coordinate for the specified window.")]
        public void SetPosition(string windowName, int left, int top, int height, int width)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => SetPosition(windowName, left, top, height, width)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            if (win == null)
            {
                return;
            }

            win.Left = left;
            win.Top = top;
            win.Height = height;
            win.Width = width;
        }

        /// <summary>
        /// Returns an object that contains metadata about the current state of a window.  Note
        /// that this is a snapshot in time.
        /// </summary>
        /// <param name="windowName"></param>
        [Description("Gets an IWindow object that contains metadata about the window.")]
        public IWindow GetWindowInfo(string windowName)
        {
            var win = new WindowInfo();

            // In order for WindowInfo to be usable by MoonSharp the type has
            // to be registered.
            if (!UserData.IsTypeRegistered(win.GetType()))
            {
                UserData.RegisterType(win.GetType());
            }

            return Application.Current.Dispatcher.Invoke(() =>
            {
                IWindow win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

                if (win == null)
                {
                    throw new Exception($"The requested window was not found: '{windowName}'");
                }

                // Because the Lua is having threading issues accessing the IWindow.  WindowInfo implements
                // IWindow and thus return it's data.
                WindowInfo wi = new()
                {
                    Left = win.Left,
                    Top = win.Top,
                    Height = win.Height,
                    Width = win.Width,
                    Title = win.Title,
                    StatusText = win.StatusText,
                    WindowType = win.WindowType,
                    Opacity = win.Opacity,
                    Name = win.Name
                };

                return win;
            });
        }

        /// <summary>
        /// Maximizes the specified window.
        /// </summary>
        /// <param name="windowName"></param>
        [Description("Maximizes the specified window.")]
        public void Maximize(string windowName)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.Maximize(windowName)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal)) as Window;

            if (win == null)
            {
                return;
            }

            win.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Minimizes the specified window.
        /// </summary>
        /// <param name="windowName"></param>
        [Description("Minimizes the specified window.")]
        public void Minimize(string windowName)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.Minimize(windowName)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal)) as Window;

            if (win == null)
            {
                return;
            }

            win.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Restores the specified window to it's normal state (not maximized or minimized).
        /// </summary>
        /// <param name="windowName"></param>
        [Description("Restores the specified window to it's normal state (not maximized or minimized).")]
        public void Restore(string windowName)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.Restore(windowName)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal)) as Window;

            if (win == null)
            {
                return;
            }

            win.WindowState = WindowState.Normal;
        }

        /// <summary>
        /// Sets the title for the specified window.  If the window does not exist
        /// no action is performed.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="title"></param>
        [Description("Sets both the title for the specified window.")]
        public void SetTitle(string windowName, string title)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.SetTitle(windowName, title)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            if (win == null)
            {
                return;
            }

            win.Title = title ?? "Untitled";
        }

        /// <summary>
        /// Sets the status bar text on the specified window.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="statusText"></param>
        [Description("Sets the status bar text on the specified window.")]
        public void SetStatusText(string windowName, string statusText)
        {
            // If it doesn't have access then execute the same function on the UI thread, otherwise just run it.
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.SetStatusText(windowName, statusText)));
                return;
            }

            var win = _interpreter.Conveyor.WindowList.FirstOrDefault(x => x.Name.Equals(windowName, StringComparison.Ordinal));

            if (win == null)
            {
                return;
            }

            win.StatusText = statusText ?? "";
        }


        /// <summary>
        /// Scrolls to the last line in the terminal.
        /// </summary>
        /// <param name="windowName">The name of the terminal window.</param>
        [Description("Scrolls to the last line in the terminal.")]
        public void ScrollToEnd(string windowName)
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

                win.ScrollToEnd();
            });
        }

        /// <summary>
        /// Scrolls to the first line in the terminal.
        /// </summary>
        /// <param name="windowName">The name of the terminal window.</param>
        [Description("Scrolls to the first line in the terminal.")]
        public void ScrollToTop(string windowName)
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

                win.ScrollToTop();
            });
        }

        /// <summary>
        /// Whether a terminal window should show line numbers or not.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="visible">Whether to show line numbers in a terminal window or not.</param>
        [Description("Restores the specified window to it's normal state (not maximized or minimized).")]
        public void ShowLineNumbers(string windowName, bool visible)
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

                win.ShowLineNumbers(visible);
            });
        }

        /// <summary>
        /// Loads a file into a terminal window.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="fileName"></param>
        [Description("Loads a file into a terminal window.")]
        public void LoadFile(string windowName, string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception($"The specified file does not exist '{fileName}'");
            }

            string buf = File.ReadAllText(fileName);

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
                win.AppendText(buf);
            });
        }

        /// <summary>
        /// Saves the text in the terminal window to the specified file.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="fileName"></param>
        [Description("Saves the text in the terminal window to the specified file.  The file will be overwritten with this overload.")]
        public void SaveFile(string windowName, string fileName)
        {
            SaveFile(windowName, fileName, false);
        }

        /// <summary>
        /// Saves the text in the terminal window to the specified file.
        /// </summary>
        /// <param name="windowName"></param>
        /// <param name="fileName"></param>
        /// <param name="append"></param>
        [Description("Saves the text in the terminal window to the specified file.")]
        public void SaveFile(string windowName, string fileName, bool append)
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

                if (append)
                {
                    File.WriteAllTextAsync(fileName, win.Text);
                }
                else
                {
                    File.AppendAllTextAsync(fileName, win.Text);
                }
            });
        }
    }
}