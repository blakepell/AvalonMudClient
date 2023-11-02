/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Rect = MoonSharp.Interpreter.Wpf.Common.Windows.Rect;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// Hash Script Commands
    /// </summary>
    [MoonSharpModule(Namespace = "process")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ProcessScriptCommands
    {

        [MoonSharpModuleMethod(Description = "Starts a process.",
            ParameterCount = 1)]
        public void Start(string fileName)
        {
            Process.Start(fileName);
        }

        [MoonSharpModuleMethod(Description = "Starts a process with the provided arguments.",
            ParameterCount = 1)]
        public void Start(string fileName, string arguments)
        {
            Process.Start(fileName, arguments);
        }

        [MoonSharpModuleMethod(Description = "Starts a process with the provided arguments.",
            ParameterCount = 1)]
        public void Start(string fileName, string[] arguments)
        {
            Process.Start(fileName, arguments);
        }

        [MoonSharpModuleMethod(Description = "Returns an array of all running processes.",
            ParameterCount = 0)]
        public Process[] GetProcesses()
        {
            return Process.GetProcesses();
        }

        [MoonSharpModuleMethod(Description = "Gets the names of all running processes.",
            ParameterCount = 0)]
        public string[] GetProcessNames()
        {
            return Process.GetProcesses().Select(x => x.ProcessName).ToArray();
        }

        [MoonSharpModuleMethod(Description = "Gets the names of all running processes.",
                              ParameterCount = 0)]
        public int[] GetProcessIds()
        {
            return Process.GetProcesses().Select(x => x.Id).ToArray();
        }

        [MoonSharpModuleMethod(Description = "Gets the process ID of a program provided it's name.  A return of -1 indicates the process was not found.",
                               ParameterCount = 1)]
        public int GetProcessIdByName(string name, bool exactMatch)
        {
            if (exactMatch)
            {
                return Process.GetProcesses().FirstOrDefault(x => x.ProcessName.Equals(name, StringComparison.Ordinal))?.Id ?? -1;
            }

            return Process.GetProcesses().FirstOrDefault(x => x.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase))?.Id ?? -1;
        }

        [MoonSharpModuleMethod(Description = "Gets the names of all running processes.",
                               ParameterCount = 0)]
        public bool IsProcessRunning(string processName, bool exactMatch)
        {
            if (exactMatch)
            {
                return Process.GetProcesses().Any(x => x.ProcessName.Equals(processName, StringComparison.Ordinal));
            }

            return Process.GetProcesses().Any(x => x.ProcessName.Contains(processName, StringComparison.OrdinalIgnoreCase));
        }


        [MoonSharpModuleMethod(Description = "Sets the focus to the specified process if it has a visible window.",
            ParameterCount = 1)]
        public void SetFocus(int processId)
        {
            var proc = Process.GetProcessById(processId);
            _ = Argus.Windows.Window.SetForegroundWindow(proc.MainWindowHandle);
        }

        [MoonSharpModuleMethod(Description = "Sets the focus to the specified process if it has a visible window.",
                               ParameterCount = 1)]
        public void SetFocus(string processName)
        {
            var proc = Process.GetProcessesByName(processName).FirstOrDefault();

            if (proc == null)
            {
                return;
            }

            _ = Argus.Windows.Window.SetForegroundWindow(proc.MainWindowHandle);
        }

        [MoonSharpModuleMethod(Description = "Gets the active window's title.  If return parent is true the parent window title is returned, otherwise the focused child window of the app will be returned.",
                               ParameterCount = 1)]
        public string WindowTitle(bool returnParent)
        {
            return Argus.Windows.Window.GetActiveWindowTitle(returnParent);
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [MoonSharpModuleMethod(Description = "Returns a Rect for the specified process if it's found.",
            ParameterCount = 1)]
        public Rect GetPosition(int id)
        {
            var rect = new Rect();
            var teamsProc = Process.GetProcessById(id);
            _ = GetWindowRect(teamsProc.MainWindowHandle, ref rect);
            return rect;
        }

        [MoonSharpModuleMethod(Description = "Returns a Rect for the specified process if it's found.",
            ParameterCount = 1)]
        public Rect GetPosition(string processName)
        {
            var rect = new Rect();
            var teamsProc = Process.GetProcessesByName(processName).FirstOrDefault();

            if (teamsProc == null)
            {
                return rect;
            }

            _ = GetWindowRect(teamsProc.MainWindowHandle, ref rect);
            return rect;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern bool SetWindowPos(System.IntPtr hwnd, int hWndInsertAfter, int x, int Y, int width, int height, int wFlags);

        [MoonSharpModuleMethod(Description = "Sets the window position for a process.",
            ParameterCount = 5)]
        public static void SetPosition(int processId, int x, int y, int width = 0, int height = 0)
        {
            var proc = Process.GetProcessById(processId);
            SetWindowPos(proc.MainWindowHandle, 0, x, y, width, height, width * height == 0 ? 1 : 0);
        }
    }
}