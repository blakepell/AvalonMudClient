using System;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Lua;
using MoonSharp.Interpreter;

namespace Avalon.HashCommands
{
    public class LuaSync : HashCommand
    {
        public LuaSync(IInterpreter interp) : base(interp)
        {
            this.IsAsync = false;
            _random = new Random();
        }

        /// <summary>
        /// Single static Random object that will need to be locked between usages.  Calls to _random
        /// should be locked for thread safety as Random is not thread safe.
        /// </summary>
        private static Random _random;

        public override string Name { get; } = "#lua-sync";

        public override string Description { get; } = "Executes an inline Lua script synchronously.";

        public override void Execute()
        {
            var lua = new LuaCaller(this.Interpreter);
            lua.Execute(Parameters);
        }
    }
}