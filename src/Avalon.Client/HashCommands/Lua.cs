using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Avalon.Common.Colors;
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

        public override string Description { get; } = "Executes an inline Lua script.";

        public override async Task ExecuteAsync()
        {
           await Application.Current.Dispatcher.InvokeAsync(new Action(async () =>
               {
                   try
                   {
                       await App.MainWindow.Lua.DoStringAsync(App.MainWindow.LuaControl, Parameters);
                   }
                   catch (Exception ex)
                   {
                       if (ex.InnerException != null)
                       {
                           if (ex.InnerException.Message.Contains("abort"))
                           {
                               // Cancel pending sends with the mud in case something went haywire
                               Interpreter.Send("~", true, false);
                               Interpreter.EchoText("--> All active Lua scripts have been terminated.", AnsiColors.Red);
                           }
                           else
                           {
                               Interpreter.Send("~", true, false);
                               Interpreter.EchoText($"--> {ex.InnerException.Message}", AnsiColors.Red);
                           }
                       }
                       else
                       {
                           Interpreter.Send("~", true, false);
                           Interpreter.EchoText($"--> {ex.Message}", AnsiColors.Red);
                       }
                   }
               }), DispatcherPriority.Normal);
        }
    }
}
