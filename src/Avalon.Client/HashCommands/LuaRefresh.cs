/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    public class LuaRefresh : HashCommand
    {
        public LuaRefresh(IInterpreter interp) : base(interp)
        {
            this.IsAsync = false;
        }

        public override string Name { get; } = "#lua-refresh";

        public override string Description { get; } = "Attempts to refresh the Lua environment any of the instances in the pool are our of sync.";

        public override void Execute()
        {
            var interp = this.Interpreter as Interpreter;
            interp?.ScriptHost?.RefreshScripts();
            this.Interpreter.Conveyor.EchoSuccess("Lua environment scripts refreshed.");
        }
    }
}