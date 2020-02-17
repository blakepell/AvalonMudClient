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
        }

        public override string Name { get; } = "#lua-sync";

        public override string Description { get; } = "Executes an inline Lua script synchronously.";

        public override void Execute()
        {
            try
            {
                // Setup Lua
                var lua = new Script();
                lua.Options.CheckThreadAccess = false;
                UserData.RegisterType<LuaCommands>();

                // Create a userdata, again, explicitly.
                var luaCmd = UserData.Create(new LuaCommands(this.Interpreter));
                lua.Globals.Set("Cmd", luaCmd);
                var executionControlToken = new ExecutionControlToken();

                lua.DoString(Parameters);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message.Contains("abort"))
                    {
                        // TODO - Make this a setting so that it can be tailored (the command that is sent, e.g. the ~).
                        // Cancel pending sends with the mud in case something went haywire
                        Interpreter.Send("~", true, false);
                        Interpreter.Conveyor.EchoLog("All active Lua scripts have been terminated.", LogType.Error);
                    }
                    else
                    {
                        Interpreter.Send("~", true, false);
                        Interpreter.Conveyor.EchoLog($"--> {ex.InnerException.Message}", LogType.Error);
                    }
                }
                else
                {
                    Interpreter.Send("~", true, false);
                    Interpreter.Conveyor.EchoLog(ex.Message, LogType.Error);
                }
            }
        }

    }
}
