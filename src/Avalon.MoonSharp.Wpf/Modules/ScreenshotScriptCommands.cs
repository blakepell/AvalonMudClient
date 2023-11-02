/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using MoonSharp.Interpreter.Wpf.Common.Windows;
using MoonSharp.Interpreter.Wpf.Types;

namespace MoonSharp.Interpreter.Wpf.Modules
{
    /// <summary>
    /// Screenshot Script Commands
    /// </summary>
    [MoonSharpModule(Namespace = "screenshot")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ScreenshotScriptCommands
    {

        /// <summary>
        /// Returns a screenshot of the current active window.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Returns a screenshot of the current active window.",
                               ParameterCount = 0)]
        public LuaBitmap CurrentWindow()
        {
            var bmp = Screenshot.GetScreenshotCurrentWindow();

            if (bmp != null)
            {
                var luaBmp = new LuaBitmap(bmp);
                return luaBmp;
            }

            return new LuaBitmap(1, 1);
        }

        /// <summary>
        /// Returns a screenshot of the primary screen.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Returns a screenshot of the primary screen.",
            ParameterCount = 0)]
        public LuaBitmap PrimaryScreen()
        {
            var bmp = Screenshot.GetScreenshotPrimaryScreen();

            if (bmp != null)
            {
                var luaBmp = new LuaBitmap(bmp);
                return luaBmp;
            }
            
            return new LuaBitmap(1, 1);
        }

        /// <summary>
        /// Takes a screenshot of the specified rectangle.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        [MoonSharpModuleMethod(Description = "Returns a screenshot of the primary screen.",
            ParameterCount = 4)]
        public LuaBitmap ByLocation(int x, int y, int width, int height)
        {
            var rect = new Rectangle(x, y, width, height);
            var bmp = Screenshot.GetScreenshotByLocation(rect);

            if (bmp != null)
            {
                var luaBmp = new LuaBitmap(bmp);
                return luaBmp;
            }

            return new LuaBitmap(1, 1);
        }

    }
}