/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Controls
{
    /// <summary>
    /// The types a NavMenuItem can manifest as.
    /// </summary>
    public enum NavType
    {
        /// <summary>
        /// Executes the provided void method.
        /// </summary>
        Default,

        /// <summary>
        /// Shells an application.
        /// </summary>
        Shell,
        /// <summary>
        /// Shells a known window.
        /// </summary>
        ShellWindow,

        /// <summary>
        /// Executes an alias.
        /// </summary>
        Alias
    }

}
