/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;

namespace Avalon.Common.Scripting
{
    /// <summary>
    /// A class that contains information about an Exception that was thrown including
    /// state information to help track down the source of the error.
    /// </summary>
    public class ScriptExceptionData
    {
        /// <summary>
        /// The <see cref="Exception"/> that caused the error.
        /// </summary>
        public Exception Exception;

        /// <summary>
        /// The name of the function if the source of the <see cref="Exception"/> was a function call.
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Details about the ScriptExceptionData error.
        /// </summary>
        public string Description { get; set; }
    }
}
