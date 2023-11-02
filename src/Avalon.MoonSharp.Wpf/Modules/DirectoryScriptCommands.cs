/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// Directory Script Commands
    /// </summary>
    [MoonSharpModule(Namespace = "directory")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class DirectoryScriptCommands
    {
        [MoonSharpModuleMethod(Description = "If a directory exists or not.",
                               ParameterCount = 1)]
        public bool Exists(string directory)
        {
            return Directory.Exists(directory);
        }

        [MoonSharpModuleMethod(Description = "Creates a directory.",
                               ParameterCount = 1)]
        public void CreateDirectory(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
        }

        [MoonSharpModuleMethod(Description = "Deletes a directory.",
                               ParameterCount = 1)]
        public void DeleteDirectory(string directoryPath)
        {
            Directory.Delete(directoryPath);
        }

        [MoonSharpModuleMethod(Description = "Returns the directories in a directory.",
                               ParameterCount = 1)]
        public string[] GetDirectories(string directoryPath)
        {
            return Directory.GetDirectories(directoryPath);
        }

        [MoonSharpModuleMethod(Description = "Returns the directories in a directory and filters by the provided search pattern.",
                               ParameterCount = 2)]
        public string[] GetDirectories(string directoryPath, string searchPattern)
        {
            return Directory.GetDirectories(directoryPath, searchPattern);
        }

        [MoonSharpModuleMethod(Description = "Returns the files in a directory.",
                               ParameterCount = 1)]
        public string[] GetFiles(string directoryPath)
        {
            return Directory.GetFiles(directoryPath);
        }

        [MoonSharpModuleMethod(Description = "Returns the files in a directory and filters by the provided search pattern.",
                               ParameterCount = 2)]
        public string[] GetFiles(string directoryPath, string searchPattern)
        {
            return Directory.GetFiles(directoryPath, searchPattern);
        }

        [MoonSharpModuleMethod(Description = "Gets the number of files in a specified directory.",
                               ParameterCount = 1)]
        public int FileCount(string directoryPath)
        {
            return Directory.GetFiles(directoryPath).Length;
        }
    }
}