/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2023 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Argus.Memory;
using Cysharp.Text;
using MoonSharp.Interpreter;

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
        public Action<ScriptExceptionData> ExceptionHandler { get; set; }

        /// <summary>
        /// Event for before a script executes.
        /// </summary>
        public EventHandler<EventArgs> PreScriptExecute { get; set; }

        /// <summary>
        /// Event for after a script has executed.
        /// </summary>
        public EventHandler<EventArgs> PostScriptExecute { get; set; }

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
                GetAction = this.InitializeScript,
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
        /// <param name="init"></param>
        private void InitializeScript(Script script, bool init)
        {
            if (!init && script.PluginTypeCount == this.SharedObjects.Count)
            {
                return;
            }

            // Setup Lua
            script.Options.CheckThreadAccess = false;

            // Dynamic types from plugins.  These are created when they are registered and only need to be
            // added into globals here for use.
            foreach (var item in this.SharedObjects)
            {
                if (script.Globals.Get(item.Key).IsNil())
                {
                    script.Globals.Set(item.Key, (DynValue)item.Value);
                    script.PluginTypeCount++;
                }
            }

            // Set the global variables that are specifically only available in Lua.
            script.Globals["global"] = this.GlobalVariables;
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

        /// <summary>
        /// Registers an instantiated object with the script engine.  This object will be passed into
        /// the engine for use by scripts, including it's state if any.  An object if thread safe
        /// can be shared between many script environments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="item"></param>
        /// <param name="prefix"></param>
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
        }

        /// <inheritdoc cref="Reset"/>
        public void Reset()
        {
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
                var exd = new ScriptExceptionData
                {
                    Exception = ex,
                    FunctionName = "",
                    Description = "Execute<T>"
                };

                this?.ExceptionHandler(exd);
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
                var exd = new ScriptExceptionData
                {
                    Exception = ex,
                    FunctionName = "",
                    Description = "ExecuteAsync<T>"
                };

                this?.ExceptionHandler(exd);
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

        public async Task<T> ExecuteFunctionAsync<T>(string functionName, params string[] args)
        {
            // Gets a new or used but ready instance of the a Lua object to use.
            var lua = MemoryPool.Get();

            // Get the function reference.
            var fnc = lua.Globals.Get(functionName);

            // If the function doesn't exist load it with the code provided.
            if (fnc.IsNil())
            {
                // The script also doesn't exist in the source index, error
                if (!this.ScriptHost.SourceCodeIndex.ContainsKey(functionName))
                {
                    var exd = new ScriptExceptionData
                    {
                        FunctionName = functionName,
                        Description = $"ExecuteFunctionAsync<T>: (Lua) {functionName} not found in the source code index."
                    };

                    this?.ExceptionHandler(exd);
                    throw new Exception($"ExecuteFunctionAsync<T>: (Lua) {functionName} not found.");
                }

                // Load the function
                try
                {
                    _ = await lua.DoStringAsync(new ExecutionControlToken(), this.ScriptHost.SourceCodeIndex[functionName].AsFunctionString, codeFriendlyName: functionName);
                }
                catch (Exception ex)
                {
                    var exd = new ScriptExceptionData
                    {
                        Exception = ex,
                        FunctionName = functionName,
                        Description = $"ExecuteFunctionAsync<T>: (Lua) {functionName} failed to load."
                    };

                    this?.ExceptionHandler(exd);
                    throw new Exception($"ExecuteFunctionAsync<T>: (Lua) {functionName} failed to load.");
                }

                // Track the hash for this function in this specific script instance.
                lua.SourceCodeHashIndex[functionName] = this.ScriptHost.SourceCodeIndex[functionName].Md5Hash;

                // Do the get again
                fnc = lua.Globals.Get(functionName);

                if (fnc.IsNil())
                {
                    var exd = new ScriptExceptionData
                    {
                        FunctionName = functionName,
                        Description = $"ExecuteFunctionAsync<T>: (Lua) {functionName} not found after load."
                    };

                    this?.ExceptionHandler(exd);
                    throw new Exception($"ExecuteFunctionAsync<T>: (Lua) {functionName} not found after load.");
                }
            }
            else
            {
                // The script existed, let's see if it needs to be updated.
                if (this.ScriptHost.SourceCodeIndex[functionName].Md5Hash != lua.SourceCodeHashIndex[functionName])
                {
                    // The MD5 hashes didn't match, out with the old and in with the new.
                    lua.Globals.Remove(functionName);

                    try
                    {
                        _ = await lua.DoStringAsync(new ExecutionControlToken(), this.ScriptHost.SourceCodeIndex[functionName].AsFunctionString, codeFriendlyName: functionName);
                    }
                    catch (Exception ex)
                    {
                        var exd = new ScriptExceptionData
                        {
                            Exception = ex,
                            FunctionName = functionName,
                            Description = $"ExecuteFunctionAsync<T>: (Lua) {functionName} failed to load."
                        };

                        this?.ExceptionHandler(exd);
                        throw new Exception($"ExecuteFunctionAsync<T>: (Lua) {functionName} failed to load.");
                    }

                    // Track the hash for this function in this specific script instance.
                    lua.SourceCodeHashIndex[functionName] = this.ScriptHost.SourceCodeIndex[functionName].Md5Hash;

                    // Do the get again
                    fnc = lua.Globals.Get(functionName);
                }
            }

            // If we got here, we should have a valid function loaded and now all we have to do is execute it.
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
                string formattedArgs = this.FormatArgs(args);

                var exd = new ScriptExceptionData
                {
                    Exception = ex,
                    FunctionName = functionName,
                    Description = $"ExecuteFunctionAsync<T>: Lua Error running function {functionName}{formattedArgs}"
                };

                this?.ExceptionHandler(exd);
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

        public T ExecuteFunction<T>(string functionName, params string[] args)
        {
            // Gets a new or used but ready instance of the a Lua object to use.
            var lua = MemoryPool.Get();

            // Get the function reference.
            var fnc = lua.Globals.Get(functionName);

            // If the function doesn't exist load it with the code provided.
            if (fnc.IsNil())
            {
                // The script also doesn't exist in the source index, error
                if (!this.ScriptHost.SourceCodeIndex.ContainsKey(functionName))
                {
                    var exd = new ScriptExceptionData
                    {
                        FunctionName = functionName,
                        Description = $"ExecuteFunction<T>: (Lua) {functionName} not found in the source code index."
                    };

                    this?.ExceptionHandler(exd);
                    throw new Exception($"ExecuteFunction<T>: (Lua) {functionName} not found.");
                }

                // Load the function
                try
                {
                    _ = lua.DoString(this.ScriptHost.SourceCodeIndex[functionName].AsFunctionString, codeFriendlyName: functionName);
                }
                catch (Exception ex)
                {
                    var exd = new ScriptExceptionData
                    {
                        Exception = ex,
                        FunctionName = functionName,
                        Description = $"ExecuteFunction<T>: (Lua) {functionName} failed to load."
                    };

                    this?.ExceptionHandler(exd);
                    throw new Exception($"ExecuteFunction<T>: (Lua) {functionName} failed to load.");
                }

                // Track the hash for this function in this specific script instance.
                lua.SourceCodeHashIndex[functionName] = this.ScriptHost.SourceCodeIndex[functionName].Md5Hash;

                // Do the get again
                fnc = lua.Globals.Get(functionName);

                if (fnc.IsNil())
                {
                    var exd = new ScriptExceptionData
                    {
                        FunctionName = functionName,
                        Description = $"ExecuteFunction<T>: (Lua) {functionName} not found after load."
                    };

                    this?.ExceptionHandler(exd);
                    throw new Exception($"ExecuteFunction<T>: (Lua) {functionName} not found after load.");
                }
            }
            else
            {
                // The script existed, let's see if it needs to be updated.
                if (this.ScriptHost.SourceCodeIndex[functionName].Md5Hash != lua.SourceCodeHashIndex[functionName])
                {
                    // The MD5 hashes didn't match, out with the old and in with the new.
                    lua.Globals.Remove(functionName);

                    try
                    {
                        _ = lua.DoString(this.ScriptHost.SourceCodeIndex[functionName].AsFunctionString, codeFriendlyName: functionName);
                    }
                    catch (Exception ex)
                    {
                        var exd = new ScriptExceptionData
                        {
                            Exception = ex,
                            FunctionName = functionName,
                            Description = $"ExecuteFunction<T>: (Lua) {functionName} failed to load."
                        };

                        this?.ExceptionHandler(exd);
                        throw new Exception($"ExecuteFunction<T>: (Lua) {functionName} failed to load.");
                    }

                    // Track the hash for this function in this specific script instance.
                    lua.SourceCodeHashIndex[functionName] = this.ScriptHost.SourceCodeIndex[functionName].Md5Hash;

                    // Do the get again
                    fnc = lua.Globals.Get(functionName);
                }
            }

            // If we got here, we should have a valid function loaded and now all we have to do is execute it.
            DynValue ret;

            try
            {
                var executionControlToken = new ExecutionControlToken();
                this.ScriptHost.Statistics.ScriptsActive++;
                this.ScriptHost.Statistics.RunCount++;
                this.OnPreScriptExecuted();
                ret = lua.Call(fnc, args);
            }
            catch (Exception ex)
            {
                string formattedArgs = this.FormatArgs(args);

                var exd = new ScriptExceptionData
                {
                    Exception = ex,
                    FunctionName = functionName,
                    Description = $"ExecuteFunction<T>: Lua Error running function {functionName}{formattedArgs}"
                };

                this?.ExceptionHandler(exd);
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

            this.InitializeScript(lua, true);

            try
            {
                this.ScriptHost.Statistics.ScriptsActive++;
                this.ScriptHost.Statistics.RunCount++;
                this.OnPreScriptExecuted();
                ret = lua.DoString(code);
            }
            catch (Exception ex)
            {
                var exd = new ScriptExceptionData
                {
                    Exception = ex,
                    FunctionName = "",
                    Description = "ExecuteStatic<T>"
                };

                this?.ExceptionHandler(exd);
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

            this.InitializeScript(lua, true);

            try
            {
                this.ScriptHost.Statistics.ScriptsActive++;
                this.ScriptHost.Statistics.RunCount++;
                this.OnPreScriptExecuted();
                ret = await lua.DoStringAsync(executionControlToken, code);
            }
            catch (Exception ex)
            {
                var exd = new ScriptExceptionData
                {
                    Exception = ex,
                    FunctionName = "",
                    Description = "ExecuteStaticAsync<T>"
                };

                this?.ExceptionHandler(exd);
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

        /// <summary>
        /// Returns arguments for display.
        /// </summary>
        /// <param name="args"></param>
        private string FormatArgs(string[] args)
        {
            string buf;

            if (args == null || args.Length > 0)
            {
                try
                {
                    using (var sb = ZString.CreateStringBuilder())
                    {
                        sb.Append('(');

                        foreach (string arg in args)
                        {
                            sb.Append("\"");
                            sb.Append(arg);
                            sb.Append("\", ");
                        }

                        sb.Remove(sb.Length - 2, 2);
                        sb.Append(')');

                        buf = sb.ToString();
                    }
                }
                catch (Exception ex)
                {
                    return $"Error Formatting Arguments: {ex.Message}";
                }
            }
            else
            {
                buf = "()";
            }

            return buf;
        }
    }
}