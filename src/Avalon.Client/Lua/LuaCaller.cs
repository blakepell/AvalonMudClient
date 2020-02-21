using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Avalon.Common.Interfaces;
using Avalon.Common.Models;
using MoonSharp.Interpreter;

namespace Avalon.Lua
{
    /// <summary>
    /// A class to handle the code to execute Lua from various locations in the client.
    /// </summary>
    public class LuaCaller
    {
        /// <summary>
        /// A reference to the mud client's current interpreter.
        /// </summary>
        private IInterpreter _interpreter;

        /// <summary>
        /// Global variables available to Lua that are shared across all of our Lua sessions.
        /// </summary>
        private LuaGlobalVariables _luaGlobalVariables;

        /// <summary>
        /// Single static Random object that will need to be locked between usages.  Calls to _random
        /// should be locked for thread safety as Random is not thread safe.
        /// </summary>
        private static Random _random;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interp"></param>
        public LuaCaller(IInterpreter interp)
        {
            _interpreter = interp;
            _luaGlobalVariables = new LuaGlobalVariables();
            _random = new Random();
        }

        /// <summary>
        /// Executes a Lua script synchronously.
        /// </summary>
        /// <param name="luaCode"></param>
        public void Execute(string luaCode)
        {
            if (string.IsNullOrWhiteSpace(luaCode))
            {
                return;
            }

            try
            {
                // Setup Lua
                var lua = new Script();
                lua.Options.CheckThreadAccess = false;
                UserData.RegisterType<LuaCommands>();
                UserData.RegisterType<LuaGlobalVariables>();

                // Create a UserData, again, explicitly.
                var luaCmd = UserData.Create(new LuaCommands(_interpreter, _random));
                lua.Globals.Set("lua", luaCmd);

                // Set the global variables that are specifically only available in Lua.
                lua.Globals["global"] = _luaGlobalVariables;

                // Execute the lua code.
                lua.DoString(luaCode);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message.Contains("abort"))
                    {
                        // TODO - Make this a setting so that it can be tailored (the command that is sent, e.g. the ~).
                        // Cancel pending sends with the mud in case something went haywire
                        _interpreter.Send("~", true, false);
                        _interpreter.Conveyor.EchoLog("All active Lua scripts have been terminated.", LogType.Error);
                    }
                    else
                    {
                        _interpreter.Send("~", true, false);
                        _interpreter.Conveyor.EchoLog($"--> {ex.InnerException.Message}", LogType.Error);
                    }
                }
                else
                {
                    _interpreter.Send("~", true, false);
                    _interpreter.Conveyor.EchoLog(ex.Message, LogType.Error);
                }
            }
        }

        /// <summary>
        /// Executes a Lua script asynchronously.
        /// </summary>
        /// <param name="luaCode"></param>
        public async Task ExecuteAsync(string luaCode)
        {
            if (string.IsNullOrWhiteSpace(luaCode))
            {
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(new Action(async () =>
            {
                try
                {
                    // Setup Lua
                    var lua = new Script();
                    lua.Options.CheckThreadAccess = false;
                    UserData.RegisterType<LuaCommands>();
                    UserData.RegisterType<LuaGlobalVariables>();

                    // Custom Lua Commands
                    var luaCmd = UserData.Create(new LuaCommands(_interpreter, _random));
                    lua.Globals.Set("lua", luaCmd);

                    // Set the global variables that are specifically only available in Lua.
                    lua.Globals["global"] = _luaGlobalVariables;
                    var executionControlToken = new ExecutionControlToken();
                    await lua.DoStringAsync(executionControlToken, luaCode);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.Message.Contains("abort"))
                        {
                            // TODO - Make this a setting so that it can be tailored (the command that is sent, e.g. the ~).
                            // Cancel pending sends with the mud in case something went haywire
                            _interpreter.Send("~", true, false);
                            _interpreter.Conveyor.EchoLog("All active Lua scripts have been terminated.", LogType.Error);
                        }
                        else
                        {
                            var exInner = ((InterpreterException) ex.InnerException);
                            _interpreter.Send("~", true, false);
                            _interpreter.Conveyor.EchoLog($"--> {exInner.DecoratedMessage}", LogType.Error);
                        }
                    }
                    else
                    {
                        _interpreter.Send("~", true, false);
                        _interpreter.Conveyor.EchoLog(ex.Message, LogType.Error);
                    }
                }
            }), DispatcherPriority.Normal);
        }
    }
}