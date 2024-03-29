﻿/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Settings
{
    /// <summary>
    /// The available startup positions for windows
    /// </summary>
    public enum WindowStartupPosition
    {
        /// <summary>
        /// Windows will place the window where Windows places the window.
        /// </summary>
        OperatingSystemDefault,
        /// <summary>
        /// The window will start maximized on the primary monitor.
        /// </summary>
        Maximized,
        /// <summary>
        /// Attempts to place the window in the position where it was when the user last used the program.
        /// </summary>
        LastUsed
    }

}
