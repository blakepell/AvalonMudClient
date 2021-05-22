/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Colors;
using System;

namespace Avalon.Common.Models
{
    /// <summary>
    /// Event arguments for echo events that come from the command interpreter.
    /// </summary>
    public class EchoEventArgs : EventArgs
    {

        /// <summary>
        /// The text to echo to the terminal.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The foreground color of the text.
        /// </summary>
        public AnsiColor ForegroundColor { get; set; }

        /// <summary>
        /// Whether the text should appear ANSI reversed with the background.
        /// </summary>
        public bool ReverseColors { get; set; }

        /// <summary>
        /// Whether the text should be the default color.
        /// </summary>
        public bool UseDefaultColors { get; set; } = true;

        /// <summary>
        /// The terminal that the target should echo to if it's not the main terminal
        /// which is the default.
        /// </summary>
        public TerminalTarget Terminal { get; set; } = TerminalTarget.Main;

    }
}
