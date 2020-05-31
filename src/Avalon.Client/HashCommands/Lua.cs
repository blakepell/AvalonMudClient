using Avalon.Common.Interfaces;
using System.Threading.Tasks;

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
            var lua = ((Interpreter)this.Interpreter).LuaCaller;
            await lua.ExecuteAsync(Parameters);
        }
    }
}
