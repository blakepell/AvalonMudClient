/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using MoonSharp.Interpreter;

namespace Avalon.Common.Scripting
{
    /// <summary>
    /// A result from script code that was validated.
    /// </summary>
    public class ValidationResult
    {

        public bool Success { get; set; }

        public SyntaxErrorException Exception { get; set; }

    }
}
