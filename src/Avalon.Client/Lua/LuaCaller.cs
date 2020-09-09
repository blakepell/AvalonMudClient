using System;
using System.Collections.Generic;
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
        public LuaGlobalVariables LuaGlobalVariables { get; private set; }

        /// <summary>
        /// Single static Random object that will need to be locked between usages.  Calls to _random
        /// should be locked for thread safety as Random is not thread safe.
        /// </summary>
        private static Random _random;

        /// <summary>
        /// A object to use for locking.
        /// </summary>
        private object _lockObject = new object();

        /// <summary>
        /// A counter of the number of Lua scripts that are actively executing.
        /// </summary>
        public int ActiveLuaScripts { get; set; } = 0;

        /// <summary>
        /// The number of Lua scripts that have been executed in this session.
        /// </summary>
        public int LuaScriptsRun { get; set; } = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interp"></param>
        public LuaCaller(IInterpreter interp)
        {
            _interpreter = interp;
            _random = new Random();
            this.LuaGlobalVariables = new LuaGlobalVariables();
        }

        /// <summary>
        /// The currently/dynamically loaded CLR types that can be exposed to Lua, probably
        /// through plugins.
        /// </summary>
        private Dictionary<string, Type> _clrTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Register a CLR type for use with Lua.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="prefix"></param>
        public void RegisterType(Type t, string prefix)
        {
            if (!_clrTypes.ContainsKey(prefix))
            {
                UserData.RegisterType(t);
                _clrTypes.Add(prefix, t);
            }
        }

        /// <summary>
        /// Clears the custom loaded types from LuaCaller.RegisterType.
        /// </summary>
        public void ClearTypes()
        {
            _clrTypes.Clear();
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
                lock (_lockObject)
                {
                    this.ActiveLuaScripts++;
                    this.LuaScriptsRun++;
                }

                // Setup Lua
                var lua = new Script();
                lua.Options.CheckThreadAccess = false;
                UserData.RegisterType<LuaCommands>();
                UserData.RegisterType<LuaGlobalVariables>();

                // Create a UserData, again, explicitly.
                var luaCmd = UserData.Create(new LuaCommands(_interpreter, _random));
                lua.Globals.Set("lua", luaCmd);

                // Dynamic types from plugins.
                foreach (var item in _clrTypes)
                {
                    // Set the actual class that has the Lua commands.
                    var instance = Activator.CreateInstance(item.Value) as ILuaCommand;
                    instance.Interpreter = _interpreter;

                    // Add it in.
                    var instanceLua = UserData.Create(instance);
                    lua.Globals.Set(item.Key, instanceLua);
                }

                // Set the global variables that are specifically only available in Lua.
                lua.Globals["global"] = this.LuaGlobalVariables;

                // If there is a Lua global shared set of code run it, try catch it in case there
                // is a problem with it, we don't want it to interfere with everything if there is
                // an issue with it, we DO want to show the user that though.
                try
                {
                    if (!string.IsNullOrWhiteSpace(App.Settings?.ProfileSettings?.LuaGlobalScript))
                    {
                        lua.DoString(App.Settings.ProfileSettings.LuaGlobalScript);
                    }
                }
                catch (Exception ex)
                {
                    _interpreter.Conveyor.EchoLog("There was an error in the global Lua file.", LogType.Error);
                    _interpreter.Conveyor.EchoLog($"Lua: {ex.Message}", LogType.Error);
                }

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
            finally
            {
                lock (_lockObject)
                {
                    this.ActiveLuaScripts--;
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
                    lock (_lockObject)
                    {
                        this.ActiveLuaScripts++;
                        this.LuaScriptsRun++;
                    }

                    // Setup Lua
                    var lua = new Script();
                    lua.Options.CheckThreadAccess = false;
                    UserData.RegisterType<LuaCommands>();
                    UserData.RegisterType<LuaGlobalVariables>();

                    // Custom Lua Commands
                    var luaCmd = UserData.Create(new LuaCommands(_interpreter, _random));
                    lua.Globals.Set("lua", luaCmd);

                    // Dynamic types from plugins.
                    foreach (var item in _clrTypes)
                    {
                        // Set the actual class that has the Lua commands.
                        var instance = Activator.CreateInstance(item.Value) as ILuaCommand;
                        instance.Interpreter = _interpreter;

                        // Add it in.
                        var instanceLua = UserData.Create(instance);
                        lua.Globals.Set(item.Key, instanceLua);
                    }

                    // Set the global variables that are specifically only available in Lua.
                    lua.Globals["global"] = this.LuaGlobalVariables;

                    // If there is a Lua global shared set of code run it, try catch it in case there
                    // is a problem with it, we don't want it to interfere with everything if there is
                    // an issue with it, we DO want to show the user that though.
                    var executionControlToken = new ExecutionControlToken();

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(App.Settings?.ProfileSettings?.LuaGlobalScript))
                        {
                            await lua.DoStringAsync(executionControlToken, App.Settings.ProfileSettings.LuaGlobalScript);
                        }
                    }
                    catch (Exception ex)
                    {
                        _interpreter.Conveyor.EchoLog("There was an error in the global Lua file.", LogType.Error);
                        _interpreter.Conveyor.EchoLog($"Lua: {ex.Message}", LogType.Error);
                    }

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
                            var exInner = ((InterpreterException)ex.InnerException);
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
                finally
                {
                    lock (_lockObject)
                    {
                        this.ActiveLuaScripts--;
                    }
                }
            }), DispatcherPriority.Normal);
        }

        /// <summary>
        /// Validates Lua code for potential syntax errors but does not execute it.
        /// </summary>
        /// <param name="luaCode"></param>
        public async Task<LuaValidationResult> ValidateAsync(string luaCode)
        {
            if (string.IsNullOrWhiteSpace(luaCode))
            {
                return new LuaValidationResult
                {
                    Success = true
                };
            }

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

                // Dynamic types from plugins.
                foreach (var item in _clrTypes)
                {
                    // Set the actual class that has the Lua commands.
                    var instance = Activator.CreateInstance(item.Value) as ILuaCommand;
                    instance.Interpreter = _interpreter;

                    // Add it in.
                    var instanceLua = UserData.Create(instance);
                    lua.Globals.Set(item.Key, instanceLua);
                }

                // Set the global variables that are specifically only available in Lua.
                lua.Globals["global"] = this.LuaGlobalVariables;


                if (!string.IsNullOrWhiteSpace(App.Settings?.ProfileSettings?.LuaGlobalScript))
                {
                    await lua.LoadStringAsync(App.Settings.ProfileSettings.LuaGlobalScript);
                }

                await lua.LoadStringAsync(luaCode);

                return new LuaValidationResult
                {
                    Success = true
                };
            }
            catch (SyntaxErrorException ex)
            {
                return new LuaValidationResult
                {
                    Success = false,
                    Exception = ex
                };
            }
        }
    }
}