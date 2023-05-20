/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    public class LuaReset : HashCommand
    {
        public LuaReset(IInterpreter interp) : base(interp)
        {
            this.IsAsync = false;
        }

        public override string Name { get; } = "#lua-reset";

        public override string Description { get; } = "Resets the Lua environment and flushes the memory pool.";

        public override void Execute()
        {
            var interp = this.Interpreter as Interpreter;
            interp?.ScriptHost?.Reset();
            interp?.ScriptHost?.RefreshScripts();
            this.Interpreter.Conveyor.EchoSuccess("Lua environment reset and scripts refreshed.");
        }
    }
}