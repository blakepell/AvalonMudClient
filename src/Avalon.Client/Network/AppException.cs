/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Network
{
    /// <summary>
    /// Exception Information that can be serialized for API consumption.
    /// </summary>
    public class AppException
    {
        /// <summary>
        /// Exception information that can be serialized for API consumption.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the stack trace of an exception.
        /// </summary>
        public string? StackTrace { get; set; }
    }
}
