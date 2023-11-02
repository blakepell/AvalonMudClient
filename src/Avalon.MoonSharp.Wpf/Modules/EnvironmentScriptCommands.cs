/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// General script commands.
    /// </summary>
    [MoonSharpModule(Namespace = "environment")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class EnvironmentScriptCommands
    {
        [MoonSharpModuleMethod(Description = "Returns the location of the currently logged in users desktop folder.",
                               ParameterCount = 0)]
        public string DesktopFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        [MoonSharpModuleMethod(Description = "Returns the location of the currently logged in users document folder.",
            ParameterCount = 0)]
        public string DocumentsFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        [MoonSharpModuleMethod(Description = "Returns the location of the application data folder.",
            ParameterCount = 0)]
        public string ApplicationDataFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        [MoonSharpModuleMethod(Description = "Returns the location of the local application data folder.",
            ParameterCount = 0)]
        public string LocalApplicationDataFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        [MoonSharpModuleMethod(Description = "Returns the location of the common application data folder.",
            ParameterCount = 0)]
        public string CommonApplicationDataFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        }

        [MoonSharpModuleMethod(Description = "Returns the directory the current application is scoped to.",
            ParameterCount = 0)]
        public string CurrentDirectory()
        {
            return Environment.CurrentDirectory;
        }

        [MoonSharpModuleMethod(Description = "The current computer/servers name.",
            ParameterCount = 0)]
        public string MachineName()
        {
            return Environment.MachineName;
        }

        [MoonSharpModuleMethod(Description = "The username of the account currently logged into the system.",
            ParameterCount = 0)]
        public string Username()
        {
            return Environment.UserName;
        }

        [MoonSharpModuleMethod(Description = "The domain name of the account that is currently logged in is associated with.",
            ParameterCount = 0)]
        public string UserDomainName()
        {
            return Environment.UserDomainName;
        }

        [MoonSharpModuleMethod(Description = "Returns the environment variable for the specified name.  If no value exists a blank string is returned.",
            ParameterCount = 1)]
        public string GetEnvironmentVariable(string variableName)
        {
            return Environment.GetEnvironmentVariable(variableName) ?? "";
        }

        [MoonSharpModuleMethod(Description = "Sets the value of a specified environment variable.",
            ParameterCount = 2)]
        public void SetEnvironmentVariable(string variableName, string value)
        {
            Environment.SetEnvironmentVariable(variableName, value);
        }

        [MoonSharpModuleMethod(Description = "Returns an array of the logical drives attached to a computer.",
            ParameterCount = 0)]
        public string[] LogicalDriveList()
        {
            return Environment.GetLogicalDrives();
        }

        [MoonSharpModuleMethod(Description = "Returns the number of ticks since the system started.",
            ParameterCount = 0)]
        public int TickCount()
        {
            return Environment.TickCount;
        }

        [MoonSharpModuleMethod(Description = "Whether a system shutdown has started or not.",
            ParameterCount = 0)]
        public bool HasShutdownStarted()
        {
            return Environment.HasShutdownStarted;
        }

        [MoonSharpModuleMethod(Description = "The process ID or PID for the current instance of this application.",
            ParameterCount = 0)]
        public int ProcessId()
        {
            return Environment.ProcessId;
        }

        [MoonSharpModuleMethod(Description = "Combines two paths or path and a file.",
            ParameterCount = 2)]
        public string PathCombine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        [MoonSharpModuleMethod(Description = "Combines two paths or path and a file.",
            ParameterCount = 2)]
        public string PathCombine(string path1, string path2, string path3)
        {
            return Path.Combine(path1, path2, path3);
        }

    }
}