/*
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
    /// Information about the window that we might want to restore at a later time.
    /// </summary>
    public class WindowPosition
    {
        /// <summary>
        /// The height of the window.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// The width of the window.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The left position of the window.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// The top position of the window.
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// The window state which should correspond to WinForms/WPF window state (which we are not referencing
        /// here from the common library).
        /// </summary>
        public int WindowState { get; set; }
    }
}
