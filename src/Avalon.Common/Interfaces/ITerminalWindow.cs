/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Models;
using System.Text;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Represents an abstracted terminal window that can be called from plugins.
    /// </summary>
    public interface ITerminalWindow : IWindow
    {
        /// <summary>
        /// Appends text to the terminal.
        /// </summary>
        /// <param name="text"></param>
        void AppendText(string text);

        /// <summary>
        /// Appends a line to the terminal.
        /// </summary>
        /// <param name="line"></param>
        void AppendText(Line line);

        /// <summary>
        /// Appends a StringBuilder to the terminal.
        /// </summary>
        /// <param name="sb"></param>
        void AppendText(StringBuilder sb);

        /// <summary>
        /// The full text in the terminal window.
        /// </summary>
        string Text { get; set; }

    }

}
