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
           await Application.Current.Dispatcher.InvokeAsync(new Action(async () =>
               {
                   try
                   {
                       // Setup Lua
                       var lua = new Script();
                       lua.Options.CheckThreadAccess = false;
                       UserData.RegisterType<LuaCommands>();

                       // Create a userdata, again, explicitly.
                       var luaCmd = UserData.Create(new LuaCommands(this.Interpreter, _random));
                       lua.Globals.Set("Cmd", luaCmd);
                       var executionControlToken = new ExecutionControlToken();

                       await lua.DoStringAsync(executionControlToken, Parameters);
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
               }), DispatcherPriority.Normal);
        }
    }
}
