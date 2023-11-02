/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter.Wpf.Types;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// General script commands.
    /// </summary>
    [MoonSharpModule(Namespace = "factory")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class FactoryScriptCommands
    {
        [MoonSharpModuleMethod(Description = "Creates a memory efficient string builder.",
                               ParameterCount = 0,
                               ReturnTypeHint = "LuaStringBuilder")]
        public LuaStringBuilder StringBuilder()
        {
            return new LuaStringBuilder();
        }

        [MoonSharpModuleMethod(Description = "Creates a new bitmap.",
            ParameterCount = 2,
            ReturnTypeHint = "LuaBitmap")]
        public LuaBitmap Bitmap(int height, int width)
        {
            return new LuaBitmap(height, width);
        }

        [MoonSharpModuleMethod(Description = "Creates a new bitmap.",
            ParameterCount = 1,
            ReturnTypeHint = "LuaBitmap")]
        public LuaBitmap Bitmap(string filename)
        {
            return new LuaBitmap(filename);
        }

        [MoonSharpModuleMethod(Description = "Creates stopwatch object.  Common properties and functions are ElapsedMilliseconds, IsRunning, Start(), Stop(), Restart(), Reset()",
            ParameterCount = 0,
            ReturnTypeHint = "Stopwatch")]
        public Stopwatch Stopwatch()
        {
            return new Stopwatch();
        }

        [MoonSharpModuleMethod(Description = "Creates a list of objects that can be any type.  Supported methods/properties are: Count, Add(), Clear(), Remove(), RemoveAt(), RemoveAll(), Reverse(), AddRange(), Contains(), Exists(), Find(), FindAll(), IndexOf(), Insert(), LastIndexOf(), Distinct()",
            ParameterCount = 0,
            ReturnTypeHint = "List<object>")]
        public List<object> List()
        {
            return new List<object>();
        }

        [MoonSharpModuleMethod(Description = "Creates a list of strings.  Supported methods/properties are: Count, Add(), Clear(), Remove(), RemoveAt(), RemoveAll(), Reverse(), AddRange(), Contains(), Exists(), Find(), FindAll(), IndexOf(), Insert(), LastIndexOf(), Distinct()",
            ParameterCount = 0,
            ReturnTypeHint = "List<string>")]
        public List<string> ListString()
        {
            return new List<string>();
        }

        [MoonSharpModuleMethod(Description = "Creates a list of numbers.  Supported methods/properties are: Count, Add(), Clear(), Remove(), RemoveAt(), RemoveAll(), Reverse(), AddRange(), Contains(), Exists(), Find(), FindAll(), IndexOf(), Insert(), LastIndexOf(), Distinct()",
            ParameterCount = 0,
            ReturnTypeHint = "List<double>")]
        public List<double> ListNumber()
        {
            return new List<double>();
        }
    }
}