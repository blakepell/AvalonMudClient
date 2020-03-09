using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using Avalon.Lua;
using MoonSharp.Interpreter;

namespace Avalon.HashCommands
{
    public class Lua : HashCommand
    {
        public Lua(IInterpreter interp) : base(interp)
        {
            this.IsAsync = true;
            _random = new Random();
        }

        /// <summary>
        /// Single static Random object that will need to be locked between usages.  Calls to _random
        /// should be locked for thread safety as Random is not thread safe.
        /// </summary>
        private static Random _random;

        public override string Name { get; } = "#lua";

        public override string Description { get; } = "Executes an inline Lua script.";

        public override async Task ExecuteAsync()
        {
            // Call our single point of Lua entry.
            var lua = new LuaCaller(this.Interpreter);
            await lua.ExecuteAsync(Parameters);
        }
    }
}
