/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Models
{
    /// <summary>
    /// The types of Windows that can be spawned from scripting metaphors.
    /// </summary>
    public enum WindowType
    {
        /// <summary>
        /// A default window which represents a normal window for the application (e.g. a WPF window if
        /// running WPF on Windows).
        /// </summary>
        Default = 0,
        /// <summary>
        /// A window that inherits from ITerminalWindow
        /// </summary>
        TerminalWindow = 1,
        /// <summary>
        /// A window that implements ICompassWindow
        /// </summary>
        CompassWindow = 2
    }
}
