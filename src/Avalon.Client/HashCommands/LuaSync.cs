using Avalon.Common.Interfaces;

namespace Avalon.HashCommands
{
    public class LuaSync : HashCommand
    {
        public LuaSync(IInterpreter interp) : base(interp)
        {
            this.IsAsync = false;
        }

        public override string Name { get; } = "#lua-sync";

        public override string Description { get; } = "Executes an inline Lua script synchronously.";

        public override void Execute()
        {
            var lua = ((Interpreter)this.Interpreter).LuaCaller;
            lua.Execute(Parameters);
        }
    }
}