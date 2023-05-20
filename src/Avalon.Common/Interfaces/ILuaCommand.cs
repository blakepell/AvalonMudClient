/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Property and methods that Lua command classes must contain.
    /// </summary>
    public interface ILuaCommand
    {
        /// <summary>
        /// The unique namespace that will be prefixed to the Lua commands.
        /// </summary>
        string Namespace { get; set; }

        /// <summary>
        /// A reference to the Interpreter.
        /// </summary>
        IInterpreter Interpreter { get; set; }
    }
}
