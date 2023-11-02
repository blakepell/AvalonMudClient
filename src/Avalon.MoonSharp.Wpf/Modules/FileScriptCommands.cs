/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Argus.Extensions;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// File Script Commands
    /// </summary>
    [MoonSharpModule(Namespace = "file")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class FileScriptCommands
    {
        [MoonSharpModuleMethod(Description = "Appends the specified text to a file.",
                               ParameterCount = 1)]
        public void Append(string filename, string text)
        {
            File.AppendAllText(filename, text);
        }

        [MoonSharpModuleMethod(Description = "Writes the specified text to a file, overwriting any contents in the file.",
                               ParameterCount = 2)]
        public void Write(string filename, string text)
        {
            File.WriteAllText(filename, text);
        }

        [MoonSharpModuleMethod(Description = "Reads the entire contents of a file into a string.",
                               ParameterCount = 1)]
        public string Read(string filename)
        {
            return File.ReadAllText(filename);
        }

        [MoonSharpModuleMethod(Description = "Reads all lines and returns the contents as string array.",
                               ParameterCount = 1)]
        public string[] ReadLines(string filename)
        {
            return File.ReadAllLines(filename);
        }

        [MoonSharpModuleMethod(Description = "Deletes the specified file.",
                               ParameterCount = 1)]
        public void Delete(string filename)
        {
            File.Delete(filename);
        }

        [MoonSharpModuleMethod(Description = "Returns whether a file exists or not.",
                               ParameterCount = 1)]
        public bool Exists(string filename)
        {
            return File.Exists(filename);
        }

        [MoonSharpModuleMethod(Description = "Copies one file to another (both old and new files exist afterwards).",
                               ParameterCount = 2)]
        public void Copy(string sourceFilename, string destinationFilename)
        {
            File.Copy(sourceFilename, destinationFilename);
        }

        [MoonSharpModuleMethod(Description = "Moves one file to another (the old file no longer exists afterwards).",
                               ParameterCount = 1)]
        public void Move(string sourceFilename, string destinationFilename)
        {
            File.Copy(sourceFilename, destinationFilename);
        }

        [MoonSharpModuleMethod(Description = "The last time the file was written to.",
                               ParameterCount = 1)]
        public DateTime LastWriteTime(string filename)
        {
            return File.GetLastWriteTime(filename);
        }

        [MoonSharpModuleMethod(Description = "The last time the file was accessed.",
                               ParameterCount = 1)]
        public DateTime LastAccessTime(string filename)
        {
            return File.GetLastWriteTime(filename);
        }

        [MoonSharpModuleMethod(Description = "The file creation date.",
                               ParameterCount = 1)]
        public DateTime CreationDate(string filename)
        {
            return File.GetCreationTime(filename);
        }

        [MoonSharpModuleMethod(Description = "The size in bytes of a file.",
                               ParameterCount = 1)]
        public long FileSize(string filename)
        {
            var fi = new FileInfo(filename);
            return fi.Length;
        }

        [MoonSharpModuleMethod(Description = "The formatted size of a file (e.g. 5KB, 2.5MB, etc.)",
                               ParameterCount = 1)]
        public string FileSizeFormatted(string filename)
        {
            var fi = new FileInfo(filename);
            return fi.FormattedFileSize();
        }

        [MoonSharpModuleMethod(Description = "If a file is currently read only.",
                               ParameterCount = 1)]
        public bool IsReadOnly(string filename)
        {
            var fi = new FileInfo(filename);
            return fi.IsReadOnly;
        }

        [MoonSharpModuleMethod(Description = "If a file is contains the case insensitive search text.  Returns false if the file does not exist.",
            ParameterCount = 2)]
        public bool Contains(string filename, string searchText)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            return File.ReadLines(filename).Any(line => line.Contains(searchText, StringComparison.OrdinalIgnoreCase));
        }

        [MoonSharpModuleMethod(Description = "If a file is contains the search text.  The final parameter is whether the search should be case sensitive or not.  Returns false if the file does not exist.",
            ParameterCount = 3)]
        public bool Contains(string filename, string searchText, bool caseSensitive)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            if (caseSensitive)
            {
                return this.Contains(filename, searchText);
            }

            return File.ReadLines(filename).Any(line => line.Contains(searchText, StringComparison.Ordinal));
        }

        [MoonSharpModuleMethod(Description = "Returns all lines in a file that match the specified case insensitive text.",
            ParameterCount = 2)]
        public IEnumerable<string> SearchFor(string filename, string text)
        {
            return this.SearchFor(filename, text, false);
        }

        [MoonSharpModuleMethod(Description = "Returns all lines in a file that match the specified text with the specified case sensitivity.",
            ParameterCount = 3)]
        public IEnumerable<string> SearchFor(string filename, string text, bool caseSensitive)
        {
            var list = new List<string>();

            foreach (var line in File.ReadLines(filename))
            {
                if (caseSensitive)
                {
                    if (line.Contains(text, StringComparison.Ordinal))
                    {
                        //list.Add(line);
                        yield return line;
                    }

                    continue;
                }

                if (line.Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    yield return line;
                    //list.Add(line);
                }
            }
        }
    }
}