/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Argus.Memory;
using Cysharp.Text;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalon.Common.Scripting
{
    /// <summary>
    /// A Lua script engine provided through MoonSharp's implementation of Lua.
    /// </summary>
    public class MoonSharpEngine : IScriptEngine
    {
        /// <summary>
        /// A memory pool of idle Script objects that can be re-used.
        /// </summary>
        public ObjectPool<Script> MemoryPool { get; set; }

        /// <summary>
        /// Global variables available to Lua that are shared across all of our Lua sessions.
        /// </summary>
        public MoonSharpGlobalVariables GlobalVariables { get; private set; }

        /// <inheritdoc cref="SharedObjects"/>
        public Dictionary<string, object> SharedObjects { get; set; } = new();

        /// <inheritdoc cref="ExceptionHandler"/>
        public Action<Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// Event for before a script executes.
        /// </summary>
        public EventHandler<EventArgs> PreScriptExecute { get; set; }

        /// <summary>
        /// Event for after a script has executed.
        /// </summary>
        public EventHandler<EventArgs> PostScriptExecute { get; set; }

        /// <summary>
        /// The list of functions and code which have been loaded into this environment.
        /// </summary>
        public Dictionary<string, SourceCode> Functions { get; set; } = new();

        /// <summary>
        /// <inheritdoc cref="ScriptHost"/>
        /// </summary>
        public ScriptHost ScriptHost { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MoonSharpEngine(ScriptHost host)
        {
            this.ScriptHost = host;

            // The global variables will be created once and registered.  These will be shared
            // between all script instances.
            UserData.RegisterType<MoonSharpGlobalVariables>();
            this.GlobalVariables = new MoonSharpGlobalVariables();

            MemoryPool = new ObjectPool<Script>
            {
                InitAction = this.InitializeScript,
                ReturnAction = script =>
                {
                }
            };

            Script.WarmUp();
        }

        /// <summary>
        /// The default options and references to set on our <see cref="Script"/> objects.
        /// </summary>
        /// <param name="script"></param>
        private void InitializeScript(Script script)
        {
            // Setup Lua
            script.Options.CheckThreadAccess = false;

            // Dynamic types from plugins.  These are created when they are registered and only need to be
            // added into globals here for use.
            foreach (var item in this.SharedObjects)
            {
                if (script.Globals.Get(item.Key).IsNil())
                {
                    script.Globals.Set(item.Key, (DynValue)item.Value);
                }
            }

            // Set the global variables that are specifically only available in Lua.
            script.Globals["global"] = this.GlobalVariables;

            // Try to load all the functions we have stored.
            foreach (var func in this.Functions)
            {
                try
                {
                    using (var sb = ZString.CreateStringBuilder())
                    {
                        sb.AppendFormat("function {0}(...)\n", func.Key);
                        sb.Append(func.Value.Code);
                        sb.Append("\nend");

                        _ = script.DoString(sb.ToString(), codeFriendlyName: func.Key);
                    }
                }
                catch (Exception ex)
                {
                    this?.ExceptionHandler(ex);
                }
            }
        }

        /// <summary>
        /// Event for before a script executes.
        /// </summary>
        private void OnPreScriptExecuted()
        {
            this.PreScriptExecute?.Invoke(this, null);
        }

        /// <summary>
        /// Event for after a script executes.
        /// </summary>
        private void OnPostScriptExecuted()
        {
            this.PostScriptExecute?.Invoke(this, null);
        }

        /// <inheritdoc cref="RegisterObject{T}"/>
        public void RegisterObject<T>(Type t, object item, string prefix)
        {
            // Register the type if it's not already registered.
            if (!UserData.IsTypeRegistered(t))
            {
                UserData.RegisterType(t);
            }

            // Only add the type in if it hasn't been added previously.  This must occur
            // after the type is registered.
            if (item != null && !this.SharedObjects.ContainsKey(prefix))
            {
                this.SharedObjects.Add(prefix, UserData.Create(item));
            }

            // Dynamic types from plugins that need to be added into anything currently
            // in the MemoryPool.  They will be added when new MemoryPool items are
            // initialized.
            this.MemoryPool.InvokeAll((script) =>
            {
                foreach (var item in this.SharedObjects)
                {
                    if (script.Globals.Get(item.Key).IsNil())
                    {
                        script.Globals.Set(item.Key, (DynValue)item.Value);
                    }
                }
            });
        }

        /// <inheritdoc cref="Reset"/>
        public void Reset()
        {
            this.Functions.Clear();
            MemoryPool.Clear();
            this.GlobalVariables = new MoonSharpGlobalVariables();
        }

        /// <summary>
        /// MoonSharp uses the CLR garbage collector.  We'll just run the same collect call that it
        /// would run if collectgarbage was called from Lua code.
        /// </summary>
        public void GarbageCollect()
        {
            GC.Collect(2, GCCollectionMode.Forced);
        }

        /// <summary>
        /// Loads a function into all available script objects in the <see cref="MemoryPool"/>.
        /// </summary>
        /// <param name="functionName">The name of the function to call.</param>
        /// <param name="code">The Lua code to load.</param>
        public void LoadFunction(string functionName, string code)
        {
            bool update;

            if (string.IsNullOrWhiteSpace(functionName) || string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            // Check if the function has already been loaded and if it's the same copy.  If it is
            // then we can ditch out of this early.
            if (this.Functions.ContainsKey(functionName))
            {
                string md5 = Argus.Cryptography.HashUtilities.MD5Hash(code);

                // If the function name exists in the list and MD5 matches then get out, it's good.
                if (string.Equals(md5, this.Functions[functionName].Md5Hash))
                {
                    return;
                }

                // The code was changed so we're going to need to update it.
                update = true;
            }
            else
            {
                // The code didn't exist at all, we're going to need to load it.
                update = false;
            }

            try
            {
                // Init one new script so the memory pool has something to load the function on.
                if (this.MemoryPool.Count() == 0)
                {
                    var lua = this.MemoryPool.Get();
                    this.MemoryPool.Return(lua);
                }

                // Insert or update the script against all existing items in the memory pool.  There
                // is a chance a script object could be in use and in that case it will need to be
                // loaded later when it's returned.
                this.MemoryPool.InvokeAll((script) =>
                {
                    if (update)
                    {
                        script.Globals.Remove(functionName);
                    }

                    using (var sb = ZString.CreateStringBuilder())
                    {
                        sb.AppendFormat("function {0}(...)\n", functionName);
                        sb.Append(code);
                        sb.Append("\nend");

                        _ = script.DoString(sb.ToString(), codeFriendlyName: functionName);
                    }
                });

                // When these are loaded from the get go there maybe nothing in the memory pool to run
                // this against yet.  We will save this, but it could have errors associated with it.  If
                // a script has an error, it won't get to here.  It has to have been loaded successfully
                // for us to save it.
                this.Functions[functionName] = new SourceCode(code);
            }
            catch (Exception ex)
            {
                this?.ExceptionHandler(ex);
                throw;
            }
        }

        /// <inheritdoc cref="Execute{T}"/>
        public T Execute<T>(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return DynValue.Nil.ToObject<T>();
            }

            // Gets a new or used but ready instance of the a Lua object to use.
            var lua = MemoryPool.Get();
            DynValue ret;

            try
            {
                this.ScriptHost.Statistics.ScriptsActive++;
                this.ScriptHost.Statistics.RunCount++;
                this.OnPreScriptExecuted();
                ret = lua.DoString(code);
            }
            catch (Exception ex)
            {
                this?.ExceptionHandler(ex);
                throw;
            }
            finally
            {
                this.ScriptHost.Statistics.ScriptsActive--;
                this.OnPostScriptExecuted();
                MemoryPool.Return(lua);
            }

            return ret.ToObject<T>();
        }

        /// <inheritdoc cref="ExecuteAsync{T}"/>
        public async Task<T> ExecuteAsync<T>(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return DynValue.Nil.ToObject<T>();
            }

            // Gets a new or used but ready instance of the a Lua object to use.
            var lua = MemoryPool.Get();
            DynValue ret;
            var executionControlToken = new ExecutionControlToken();

            try
            {
                this.ScriptHost.Statistics.ScriptsActive++;
                this.ScriptHost.Statistics.RunCount++;
                this.OnPreScriptExecuted();
                ret = await lua.DoStringAsync(executionControlToken, code);
            }
            catch (Exception ex)
            {
                this?.ExceptionHandler(ex);
                throw;
            }
            finally
            {
                this.ScriptHost.Statistics.ScriptsActive--;
                this.OnPostScriptExecuted();
                MemoryPool.Return(lua);
            }

            return ret.ToObject<T>();
        }

        /// <summary>
        /// Executes a function.  If the function isn't stored a copy will be loaded.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functionName">The name of the function to call.</param>
        /// <param name="args">Any param arguments to pass to the function.</param>
        public Task<T> ExecuteFunctionAsync<T>(string functionName, params string[] args)
        {
            return this.ExecuteFunctionAsync<T>(functionName, "", args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functionName"></param>
        /// <param name="code"></param>
        /// <param name="args"></param>
        public async Task<T> ExecuteFunctionAsync<T>(string functionName, string code, params string[] args)
        {
            functionName = ScriptHost.GetFunctionName(functionName);
            this.LoadFunction(functionName, code);

            // Gets a new or used but ready instance of the a Lua object to use.
            var lua = MemoryPool.Get();

            // Get the function reference.
            var fnc = lua.Globals.Get(functionName);

            // If the function doesn't exist load it with the code provided.
            if (fnc.IsNil())
            {
                var notFoundException = new Exception($"Function '{functionName}' was not loaded.");
                this?.ExceptionHandler(notFoundException);
                MemoryPool.Return(lua);

                throw notFoundException;
            }

            DynValue ret;

            try
            {
                var executionControlToken = new ExecutionControlToken();
                this.ScriptHost.Statistics.ScriptsActive++;
                this.ScriptHost.Statistics.RunCount++;
                this.OnPreScriptExecuted();
                ret = await lua.CallAsync(executionControlToken, fnc, args);
            }
            catch (Exception ex)
            {
                this?.ExceptionHandler(ex);
                throw;
            }
            finally
            {
                this.ScriptHost.Statistics.ScriptsActive--;
                this.OnPostScriptExecuted();
                MemoryPool.Return(lua);
            }

            return ret.ToObject<T>();
        }

        /// <summary>
        /// Executes a function.  If the function isn't stored a copy will be loaded.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functionName">The name of the function to call.</param>
        /// <param name="args">Any param arguments to pass to the function.</param>
        public T ExecuteFunction<T>(string functionName, params string[] args)
        {
            return ExecuteFunction<T>(functionName, "", args);
        }

        /// <summary>
        /// Executes a function.  If the function isn't stored a copy will be loaded.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functionName">The name of the function to call.</param>
        /// <param name="code">The Lua code to load if the function hasn't already been loaded.</param>
        /// <param name="args">Any param arguments to pass to the function.</param>
        public T ExecuteFunction<T>(string functionName, string code, params string[] args)
        {
            functionName = ScriptHost.GetFunctionName(functionName);
            this.LoadFunction(functionName, code);

            // Gets a new or used but ready instance of the a Lua object to use.
            var lua = MemoryPool.Get();

            // Get the function reference.
            DynValue fnc = lua.Globals.Get(functionName);

            // If the function doesn't exist load it with the code provided.
            if (fnc.IsNil())
            {
                var notFoundException = new Exception($"Function '{functionName}' was not loaded.");
                this?.ExceptionHandler(notFoundException);
                MemoryPool.Return(lua);

                throw notFoundException;
            }

            DynValue ret;

            try
            {
                this.ScriptHost.Statistics.ScriptsActive++;
                this.ScriptHost.Statistics.RunCount++;
                this.OnPreScriptExecuted();
                ret = lua.Call(fnc, args);
            }
            catch (Exception ex)
            {
                this?.ExceptionHandler(ex);
                throw;
            }
            finally
            {
                this.ScriptHost.Statistics.ScriptsActive--;
                this.OnPostScriptExecuted();
                MemoryPool.Return(lua);
            }

            return ret.ToObject<T>();
        }

        /// <inheritdoc cref="ExecuteStatic{T}"/>
        public T ExecuteStatic<T>(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return DynValue.Nil.ToObject<T>();
            }

            // Gets a new instance of a Script that will be discarded after use.
            var lua = new Script();
            DynValue ret;

            this.InitializeScript(lua);

            try
            {
                this.ScriptHost.Statistics.ScriptsActive++;
                this.ScriptHost.Statistics.RunCount++;
                this.OnPreScriptExecuted();
                ret = lua.DoString(code);
            }
            catch (Exception ex)
            {
                this?.ExceptionHandler(ex);
                throw;
            }
            finally
            {
                this.ScriptHost.Statistics.ScriptsActive--;
                this.OnPostScriptExecuted();
            }

            return ret.ToObject<T>();
        }

        /// <inheritdoc cref="ExecuteStaticAsync{T}"/>
        public async Task<T> ExecuteStaticAsync<T>(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return DynValue.Nil.ToObject<T>();
            }

            // Gets a new or used but ready instance of the a Lua object to use.
            var lua = new Script();
            DynValue ret;
            var executionControlToken = new ExecutionControlToken();

            this.InitializeScript(lua);

            try
            {
                this.ScriptHost.Statistics.ScriptsActive++;
                this.ScriptHost.Statistics.RunCount++;
                this.OnPreScriptExecuted();
                ret = await lua.DoStringAsync(executionControlToken, code);
            }
            catch (Exception ex)
            {
                this?.ExceptionHandler(ex);
                throw;
            }
            finally
            {
                this.ScriptHost.Statistics.ScriptsActive--;
                this.OnPostScriptExecuted();
            }

            return ret.ToObject<T>();
        }

        /// <summary>
        /// Validates Lua code for potential syntax errors but does not execute it.
        /// </summary>
        /// <param name="luaCode"></param>
        public ValidationResult Validate(string luaCode)
        {
            if (string.IsNullOrWhiteSpace(luaCode))
            {
                return new ValidationResult
                {
                    Success = true
                };
            }

            try
            {
                // Setup Lua
                var lua = new Script
                {
                    Options = { CheckThreadAccess = false }
                };

                // Dynamic types from plugins.  These are created when they are registered and only need to be
                // added into globals here for use.
                foreach (var item in this.SharedObjects)
                {
                    lua.Globals.Set(item.Key, (DynValue)item.Value);
                }

                // Set the global variables that are specifically only available in Lua.
                lua.Globals["global"] = this.GlobalVariables;

                lua.LoadString(luaCode);

                return new ValidationResult
                {
                    Success = true
                };
            }
            catch (SyntaxErrorException ex)
            {
                return new ValidationResult
                {
                    Success = false,
                    Exception = ex
                };
            }
        }

        /// <summary>
        /// Validates Lua code for potential syntax errors but does not execute it.
        /// </summary>
        /// <param name="luaCode"></param>
        public async Task<ValidationResult> ValidateAsync(string luaCode)
        {
            if (string.IsNullOrWhiteSpace(luaCode))
            {
                return new ValidationResult
                {
                    Success = true
                };
            }

            try
            {
                // Setup Lua
                var lua = new Script
                {
                    Options = { CheckThreadAccess = false }
                };

                // Dynamic types from plugins.  These are created when they are registered and only need to be
                // added into globals here for use.
                foreach (var item in this.SharedObjects)
                {
                    lua.Globals.Set(item.Key, (DynValue)item.Value);
                }

                // Set the global variables that are specifically only available in Lua.
                lua.Globals["global"] = this.GlobalVariables;

                await lua.LoadStringAsync(luaCode);

                return new ValidationResult
                {
                    Success = true
                };
            }
            catch (SyntaxErrorException ex)
            {
                return new ValidationResult
                {
                    Success = false,
                    Exception = ex
                };
            }
        }
    }
}