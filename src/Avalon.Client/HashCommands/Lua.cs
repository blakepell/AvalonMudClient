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
    public class Lua : HashCommand
    {
        public Lua(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
        }

        public override string Name { get; } = "#lua";

        public override string Description { get; } = "Executes an inline Lua script asynchronously.";

        public override async Task ExecuteAsync()
        {
            // Call our single point of Lua entry.
            var lua = ((Interpreter)this.Interpreter).ScriptHost.MoonSharp;
            _ = await lua.ExecuteAsync<object>(Parameters);
        }
    }
}
