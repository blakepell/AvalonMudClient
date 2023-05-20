/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Models;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Represents a basic window from which other window types can implement/inherit from.
    /// </summary>
    public interface IWindow
    {
        /// <summary>
        /// Shows the window.
        /// </summary>
        void Show();

        /// <summary>
        /// Closes down the window.
        /// </summary>
        void Close();

        /// <summary>
        /// Attempts to Activate the Window and bring it to the foreground.
        /// </summary>
        void Activate();

        /// <summary>
        /// The name of the window.
        /// </summary>
        string Name { get; set; }


        /// <summary>
        /// The title of the window.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// The topmost screen pixel the form should be placed at.
        /// </summary>
        double Top { get; set; }

        /// <summary>
        /// The leftmost screen pixel the form should be placed at.
        /// </summary>
        double Left { get; set; }

        /// <summary>
        /// The width in pixels of the form.
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// The height in pixels of the form.
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// The text for the status bar of the terminal window.
        /// </summary>
        string StatusText { get; set; }

        /// <summary>
        /// The opacity of the window.
        /// </summary>
        double Opacity { get; set; }

        /// <summary>
        /// The type of window that this represents.
        /// </summary>
        WindowType WindowType { get; set; }
    }
}