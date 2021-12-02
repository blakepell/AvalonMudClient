/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

namespace Avalon.Common.Models
{
    /// <summary>
    /// The ways that user input can be executed, either as a command to the game or
    /// sent to any number of supported scripting engines.
    /// </summary>
    public enum ExecuteType
    {
        /// <summary>
        /// A command that is sent directly to the interpreter which will then be sent
        /// to the game.
        /// </summary>
        [Description("Command")]
        Command = 0,
        /// <summary>
        /// Lua executed through the MoonSharp interpreter.
        /// </summary>
        [Description("Lua: MoonSharp")]
        LuaMoonsharp = 1
        ///// <summary>
        ///// Lua executed through the NLua interpreter.
        ///// </summary>
        //[Description("Lua: NLua")]
        //LuaNLua = 2
    }
}
