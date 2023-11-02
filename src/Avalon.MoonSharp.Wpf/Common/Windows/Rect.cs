/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System.Runtime.InteropServices;

namespace MoonSharp.Interpreter.Wpf.Common.Windows
{
    /// <summary>
    /// A rectangle for use with the WinAPI.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        /// <summary>
        /// x position of upper-left corner
        /// </summary>
        public int Left;
        /// <summary>
        /// y position of upper-left corner
        /// </summary>
        public int Top;
        /// <summary>
        /// x position of lower-right corner
        /// </summary>
        public int Right;
        /// <summary>
        /// y position of lower-right corner
        /// </summary>
        public int Bottom;

        public int X
        {
            get => Left;
            set { Right -= Left - value; Left = value; }
        }

        public int Y
        {
            get => Top;
            set { Bottom -= Top - value; Top = value; }
        }

        public int Height
        {
            get => Bottom - Top;
            set => Bottom = value + Top;
        }

        public int Width
        {
            get => Right - Left;
            set => Right = value + Left;
        }
    }
}