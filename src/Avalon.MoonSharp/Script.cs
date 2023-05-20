using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.IO;
using MoonSharp.Interpreter.Platforms;
using MoonSharp.Interpreter.Tree.Fast_Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// This class implements a MoonSharp scripting session. Multiple Script objects can coexist in the same program but cannot share
    /// data among themselves unless some mechanism is put in place.
    /// </summary>
    public class Script : IScriptPrivateResource
    {
        /// <summary>
        /// The version of the MoonSharp engine
        /// </summary>
        public const string VERSION = "3.0.0.0";

        /// <summary>
        /// The Lua version being supported
        /// </summary>
        public const string LUA_VERSION = "5.2";

        private ByteCode _byteCode;
        private Processor _mainProcessor;
        private Table[] _typeMetaTables = new Table[(int)LuaTypeExtensions.MaxMetaTypes];

        /// <summary>
        /// A list of MD5 hashes for each function that has been loaded.  Each <see cref="Script"/> will have
        /// it's own list so the caller will know whether a new version of a function needs to be loaded since
        /// all <see cref="Script"/> objects in a memory pool might be in a different state if change are frequently
        /// made to scripts.
        /// </summary>
        public Dictionary<string, string> SourceCodeHashIndex { get; set; } = new();

        /// <summary>
        /// A metadata field to tallied by the caller to track the number of registered plugin
        /// types.  This can then be used to check if a Script object is out of sync with the
        /// number of types it should have.
        /// </summary>
        public int PluginTypeCount { get; set; } = 0;

        /// <summary>
        /// A unique identifier for this specific <see cref="Script"/> instance.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Returns the number of instructions held by this <see cref="ByteCode"/> class.
        /// </summary>
        public int InstructionCount => _byteCode.InstructionCount;

        /// <summary>
        /// Initializes the <see cref="Script"/> class.
        /// </summary>
        static Script()
        {
            GlobalOptions = new ScriptGlobalOptions();

            DefaultOptions = new ScriptOptions
            {
                DebugPrint = GlobalOptions.Platform.DefaultPrint,
                DebugInput = GlobalOptions.Platform.DefaultInput,
                CheckThreadAccess = true,
                ScriptLoader = PlatformAutoDetector.GetDefaultScriptLoader(),
                TailCallOptimizationThreshold = 65536
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Script"/> class.
        /// </summary>
        public Script() : this(CoreModules.Preset_Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Script"/> class.
        /// </summary>
        /// <param name="coreModules">The core modules to be pre-registered in the default global table.</param>
        public Script(CoreModules coreModules)
        {
            this.Id = Guid.NewGuid().ToString();

            this.Options = new ScriptOptions(DefaultOptions);
            this.Registry = new Table(this);
            
            _byteCode = new ByteCode(this);
            _mainProcessor = new Processor(this, this.Globals, _byteCode);
            this.Globals = new Table(this).RegisterCoreModules(coreModules);
        }


        /// <summary>
        /// Gets or sets the script loader which will be used as the value of the
        /// ScriptLoader property for all newly created scripts.
        /// </summary>
        public static ScriptOptions DefaultOptions { get; }

        /// <summary>
        /// Gets access to the script options. 
        /// </summary>
        public ScriptOptions Options { get; }

        /// <summary>
        /// Gets the global options, that is options which cannot be customized per-script.
        /// </summary>
        public static ScriptGlobalOptions GlobalOptions { get; }

        /// <summary>
        /// Gets the default global table for this script. Unless a different table is intentionally passed (or setfenv has been used)
        /// execution uses this table.
        /// </summary>
        public Table Globals { get; }

        /// <summary>
        /// MoonSharp (like Lua itself) provides a registry, a predefined table that can be used by any CLR code to 
        /// store whatever Lua values it needs to store. 
        /// Any CLR code can store data into this table, but it should take care to choose keys 
        /// that are different from those used by other libraries, to avoid collisions. 
        /// Typically, you should use as key a string GUID, a string containing your library name, or a 
        /// userdata with the address of a CLR object in your code.
        /// </summary>
        public Table Registry { get; }

        Script IScriptPrivateResource.OwnerScript => this;

        /// <summary>
        /// Loads a string containing a Lua/MoonSharp function.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="globalTable">The global table to bind to this chunk.</param>
        /// <param name="funcFriendlyName">Name of the function used to report errors, etc.</param>
        /// <returns>
        /// A DynValue containing a function which will execute the loaded code.
        /// </returns>
        public DynValue LoadFunction(string code, Table globalTable = null, string funcFriendlyName = null)
        {
            this.CheckScriptOwnership(globalTable);

            string chunkName = $"libfunc_{funcFriendlyName ?? "x"}";

            var source = new SourceCode(chunkName, code, this);

            int address = Loader_Fast.LoadFunction(this, source, _byteCode, globalTable != null || this.Globals != null);

            return this.MakeClosure(address, globalTable ?? this.Globals);
        }

        /// <summary>
        /// Loads a string containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="globalTable">The global table to bind to this chunk.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc. Also used by debuggers to locate the original source file.</param>
        /// <returns>
        /// A DynValue containing a function which will execute the loaded code.
        /// </returns>
        public DynValue LoadString(string code, Table globalTable = null, string codeFriendlyName = null)
        {
            this.CheckScriptOwnership(globalTable);
            var source = new SourceCode(codeFriendlyName ?? "chunk_x", code, this);
            int address = Loader_Fast.LoadChunk(this, source, _byteCode);

            return this.MakeClosure(address, globalTable ?? this.Globals);
        }

        /// <summary>
        /// Loads a Lua/MoonSharp script from a System.IO.Stream. NOTE: This will *NOT* close the stream!
        /// </summary>
        /// <param name="stream">The stream containing code.</param>
        /// <param name="globalTable">The global table to bind to this chunk.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc.</param>
        /// <returns>
        /// A DynValue containing a function which will execute the loaded code.
        /// </returns>
        public DynValue LoadStream(Stream stream, Table globalTable = null, string codeFriendlyName = null)
        {
            this.CheckScriptOwnership(globalTable);

            Stream codeStream = new UndisposableStream(stream);

            using (var sr = new StreamReader(codeStream))
            {
                string scriptCode = sr.ReadToEnd();
                return this.LoadString(scriptCode, globalTable, codeFriendlyName);
            }
        }

        /// <summary>
        /// Loads a string containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="filename">The code.</param>
        /// <param name="globalContext">The global table to bind to this chunk.</param>
        /// <param name="friendlyFilename">The filename to be used in error messages.</param>
        /// <returns>
        /// A DynValue containing a function which will execute the loaded code.
        /// </returns>
        public DynValue LoadFile(string filename, Table globalContext = null, string friendlyFilename = null)
        {
            this.CheckScriptOwnership(globalContext);

#pragma warning disable 618
            filename = this.Options.ScriptLoader.ResolveFileName(filename, globalContext ?? this.Globals);
#pragma warning restore 618

            var code = this.Options.ScriptLoader.LoadFile(filename, globalContext ?? this.Globals);

            if (code is string s)
            {
                return this.LoadString(s, globalContext, friendlyFilename ?? filename);
            }

            if (code is byte[] b)
            {
                using (var ms = new MemoryStream(b))
                {
                    return this.LoadStream(ms, globalContext, friendlyFilename ?? filename);
                }
            }

            if (code is Stream st)
            {
                try
                {
                    return this.LoadStream(st, globalContext, friendlyFilename ?? filename);
                }
                finally
                {
                    st.Dispose();
                }
            }
            
            if (code == null)
            {
                throw new InvalidCastException("Unexpected null from IScriptLoader.LoadFile");
            }

            throw new InvalidCastException($"Unsupported return type from IScriptLoader.LoadFile : {code.GetType()}");
        }


        /// <summary>
        /// Loads and executes a string containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="globalContext">The global context.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc. Also used by debuggers to locate the original source file.</param>
        /// <returns>
        /// A DynValue containing the result of the processing of the loaded chunk.
        /// </returns>
        public DynValue DoString(string code, Table globalContext = null, string codeFriendlyName = null)
        {
            var func = this.LoadString(code, globalContext, codeFriendlyName);
            return this.Call(func);
        }


        /// <summary>
        /// Loads and executes a stream containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="globalContext">The global context.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc. Also used by debuggers to locate the original source file.</param>
        /// <returns>
        /// A DynValue containing the result of the processing of the loaded chunk.
        /// </returns>
        public DynValue DoStream(Stream stream, Table globalContext = null, string codeFriendlyName = null)
        {
            var func = this.LoadStream(stream, globalContext, codeFriendlyName);
            return this.Call(func);
        }

        /// <summary>
        /// Loads and executes a file containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="globalContext">The global context.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc. Also used by debuggers to locate the original source file.</param>
        /// <returns>
        /// A DynValue containing the result of the processing of the loaded chunk.
        /// </returns>
        public DynValue DoFile(string filename, Table globalContext = null, string codeFriendlyName = null)
        {
            var func = this.LoadFile(filename, globalContext, codeFriendlyName);
            return this.Call(func);
        }

        /// <summary>
        /// Runs the specified file with all possible defaults for quick experimenting.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// A DynValue containing the result of the processing of the executed script.
        public static DynValue RunFile(string filename)
        {
            var s = new Script();
            return s.DoFile(filename);
        }

        /// <summary>
        /// Runs the specified code with all possible defaults for quick experimenting.
        /// </summary>
        /// <param name="code">The Lua/MoonSharp code.</param>
        /// A DynValue containing the result of the processing of the executed script.
        public static DynValue RunString(string code)
        {
            var s = new Script();
            return s.DoString(code);
        }

        /// <summary>
        /// Creates a closure from a bytecode address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="envTable">The env table to create a 0-upvalue</param>
        private DynValue MakeClosure(int address, Table envTable = null)
        {
            this.CheckScriptOwnership(envTable);
            Closure c;

            if (envTable == null)
            {
                var meta = _mainProcessor.FindMeta(ref address);

                // if we find the meta for a new chunk, we use the value in the meta for the _ENV upvalue
                if ((meta?.NumVal2 == (int)OpCodeMetadataType.ChunkEntrypoint))
                {
                    c = new Closure(this, address,
                        new[] {SymbolRef.Upvalue(WellKnownSymbols.ENV, 0)},
                        new[] {meta.Value});
                }
                else
                {
                    c = new Closure(this, address, Array.Empty<SymbolRef>(), Array.Empty<DynValue>());
                }
            }
            else
            {
                var syms = new[]
                {
                    new SymbolRef
                        {_env = null, _index = 0, _name = WellKnownSymbols.ENV, _type = SymbolRefType.DefaultEnv}
                };

                var vals = new[]
                {
                    DynValue.NewTable(envTable)
                };

                c = new Closure(this, address, syms, vals);
            }

            return DynValue.NewClosure(c);
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <returns>
        /// The return value(s) of the function call.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public DynValue Call(DynValue function)
        {
            return this.Call(function, Array.Empty<DynValue>());
        }

        private DynValue Internal_Call(ExecutionControlToken ecToken, DynValue function, params DynValue[] args)
        {
            this.CheckScriptOwnership(function);
            this.CheckScriptOwnership(args);

            if (function.Type != DataType.Function && function.Type != DataType.ClrFunction)
            {
                var metaFunction = _mainProcessor.GetMetamethod(ecToken, function, "__call");

                if (metaFunction != null)
                {
                    var metaArgs = new DynValue[args.Length + 1];
                    metaArgs[0] = function;

                    for (int i = 0; i < args.Length; i++)
                    {
                        metaArgs[i + 1] = args[i];
                    }

                    function = metaFunction;
                    args = metaArgs;
                }
                else
                {
                    throw new ArgumentException("function is not a function and has no __call metamethod.");
                }
            }
            else if (function.Type == DataType.ClrFunction)
            {
                return function.Callback.ClrCallback(this.CreateDynamicExecutionContext(ecToken, function.Callback),
                    new CallbackArguments(args, false));
            }

            return _mainProcessor.Call(ecToken, function, args);
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <returns>
        /// The return value(s) of the function call.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public DynValue Call(DynValue function, params DynValue[] args)
        {
            return this.Internal_Call(ExecutionControlToken.Dummy, function, args);
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <returns>
        /// The return value(s) of the function call.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public DynValue Call(DynValue function, params object[] args)
        {
            var dargs = new DynValue[args.Length];

            for (int i = 0; i < dargs.Length; i++)
            {
                dargs[i] = DynValue.FromObject(this, args[i]);
            }

            return this.Call(function, dargs);
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <param name="args">The string arguments to pass to the function.</param>
        /// <returns>
        /// The return value(s) of the function call.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public DynValue Call(DynValue function, params string[] args)
        {
            var dargs = new DynValue[args.Length];

            for (int i = 0; i < dargs.Length; i++)
            {
                dargs[i] = DynValue.NewString(args[i]);
            }

            return this.Call(function, dargs);
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public DynValue Call(object function)
        {
            return this.Call(DynValue.FromObject(this, function));
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The Lua/MoonSharp function to be called </param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public DynValue Call(object function, params object[] args)
        {
            return this.Call(DynValue.FromObject(this, function), args);
        }


        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="function">The Lua/MoonSharp function to be called </param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public DynValue Call(object function, params DynValue[] args)
        {
            return this.Call(DynValue.FromObject(this, function), args);
        }

        /// <summary>
        /// Asynchronously loads and executes a string containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="ecToken">The execution control token to be associated with the execution of this function</param>
        /// <param name="code">The code.</param>
        /// <param name="globalContext">The global context.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc. Also used by debuggers to locate the original source file.</param>
        /// <returns>
        /// A DynValue containing the result of the processing of the loaded chunk.
        /// </returns>
        public async Task<DynValue> DoStringAsync(ExecutionControlToken ecToken, string code, Table globalContext = null,
            string codeFriendlyName = null)
        {
            return await this.LoadStringAsync(code, globalContext, codeFriendlyName)
                // ReSharper disable once AsyncConverter.AsyncWait
                .ContinueWith(prevTask => this.CallAsync(ecToken, prevTask.Result).Result);
        }

        /// <summary>
        /// Asynchronously loads and executes a stream containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="ecToken">The execution control token to be associated with the execution of this function</param>
        /// <param name="stream">The stream.</param>
        /// <param name="globalContext">The global context.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc. Also used by debuggers to locate the original source file.</param>
        /// <returns>
        /// A DynValue containing the result of the processing of the loaded chunk.
        /// </returns>
        public Task<DynValue> DoStreamAsync(ExecutionControlToken ecToken, Stream stream, Table globalContext = null,
            string codeFriendlyName = null)
        {
            return this.LoadStreamAsync(stream, globalContext, codeFriendlyName)
                // ReSharper disable once AsyncConverter.AsyncWait
                .ContinueWith(prevTask => this.CallAsync(ecToken, prevTask.Result).Result);
        }


        /// <summary>
        /// Asynchronously loads and executes a file containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="ecToken">The execution control token to be associated with the execution of this function</param>
        /// <param name="filename">The filename.</param>
        /// <param name="globalContext">The global context.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc. Also used by debuggers to locate the original source file.</param>
        /// <returns>
        /// A DynValue containing the result of the processing of the loaded chunk.
        /// </returns>
        public Task<DynValue> DoFileAsync(ExecutionControlToken ecToken, string filename, Table globalContext = null,
            string codeFriendlyName = null)
        {
            return this.LoadFileAsync(filename, globalContext, codeFriendlyName)
                // ReSharper disable once AsyncConverter.AsyncWait
                .ContinueWith(prevTask => this.CallAsync(ecToken, prevTask.Result).Result);
        }


        /// <summary>
        /// Asynchronously loads a string containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="globalTable">The global table to bind to this chunk.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc.</param>
        /// <returns>
        /// A DynValue containing a function which will execute the loaded code.
        /// </returns>
        public Task<DynValue> LoadStringAsync(string code, Table globalTable = null, string codeFriendlyName = null)
        {
            return Task.Factory.StartNew(() => this.LoadString(code, globalTable, codeFriendlyName));
        }


        /// <summary>
        /// Asynchronously loads a Lua/MoonSharp script from a System.IO.Stream. NOTE: This will *NOT* close the stream!
        /// </summary>
        /// <param name="stream">The stream containing code.</param>
        /// <param name="globalTable">The global table to bind to this chunk.</param>
        /// <param name="codeFriendlyName">Name of the code - used to report errors, etc.</param>
        /// <returns>
        /// A DynValue containing a function which will execute the loaded code.
        /// </returns>
        public Task<DynValue> LoadStreamAsync(Stream stream, Table globalTable = null, string codeFriendlyName = null)
        {
            return Task.Factory.StartNew(() => this.LoadStream(stream, globalTable, codeFriendlyName));
        }


        /// <summary>
        /// Asynchronously loads a string containing a Lua/MoonSharp script.
        /// </summary>
        /// <param name="filename">The code.</param>
        /// <param name="globalContext">The global table to bind to this chunk.</param>
        /// <param name="friendlyFilename">The filename to be used in error messages.</param>
        /// <returns>
        /// A DynValue containing a function which will execute the loaded code.
        /// </returns>
        public Task<DynValue> LoadFileAsync(string filename, Table globalContext = null, string friendlyFilename = null)
        {
            return Task.Factory.StartNew(() => this.LoadFile(filename, globalContext, friendlyFilename));
        }


        /// <summary>
        /// Asynchronously loads a string containing a Lua/MoonSharp function.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="globalTable">The global table to bind to this chunk.</param>
        /// <param name="funcFriendlyName">Name of the function used to report errors, etc.</param>
        /// <returns>
        /// A DynValue containing a function which will execute the loaded code.
        /// </returns>
        public Task<DynValue> LoadFunctionAsync(string code, Table globalTable = null, string funcFriendlyName = null)
        {
            return Task.Factory.StartNew(() => this.LoadFunction(code, globalTable, funcFriendlyName));
        }

        /// <summary>
        /// Calls the specified function.
        /// </summary>
        /// <param name="ecToken">The execution control token to be associated with the execution of this function</param>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <returns>
        /// The return value(s) of the function call.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public Task<DynValue> CallAsync(ExecutionControlToken ecToken, DynValue function)
        {
            return this.CallAsync(ecToken, function, Array.Empty<DynValue>());
        }


        /// <summary>
        /// Asynchronously calls the specified function.
        /// </summary>
        /// <param name="ecToken">The execution control token to be associated with the execution of this function</param>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <returns>
        /// The return value(s) of the function call.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public Task<DynValue> CallAsync(ExecutionControlToken ecToken, DynValue function, params DynValue[] args)
        {
            return Task.Factory.StartNew(() => this.Internal_Call(ecToken, function, args));
        }

        /// <summary>
        /// Asynchronously calls the specified function.
        /// </summary>
        /// <param name="ecToken">The execution control token to be associated with the execution of this function</param>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <returns>
        /// The return value(s) of the function call.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public Task<DynValue> CallAsync(ExecutionControlToken ecToken, DynValue function, params object[] args)
        {
            var dargs = new DynValue[args.Length];

            for (int i = 0; i < dargs.Length; i++)
            {
                dargs[i] = DynValue.FromObject(this, args[i]);
            }

            return this.CallAsync(ecToken, function, dargs);
        }

        /// <summary>
        /// Asynchronously calls the specified function.
        /// </summary>
        /// <param name="ecToken">The execution control token to be associated with the execution of this function</param>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <returns>
        /// The return value(s) of the function call.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public Task<DynValue> CallAsync(ExecutionControlToken ecToken, DynValue function, params string[] args)
        {
            var dargs = new DynValue[args.Length];

            for (int i = 0; i < dargs.Length; i++)
            {
                dargs[i] = DynValue.NewString(args[i]);
            }

            return this.CallAsync(ecToken, function, dargs);
        }

        /// <summary>
        /// Asynchronously calls the specified function.
        /// </summary>
        /// <param name="ecToken">The execution control token to be associated with the execution of this function</param>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public Task<DynValue> CallAsync(ExecutionControlToken ecToken, object function)
        {
            return this.CallAsync(ecToken, DynValue.FromObject(this, function));
        }


        /// <summary>
        /// Asynchronously calls the specified function.
        /// </summary>
        /// <param name="ecToken">The execution control token to be associated with the execution of this function</param>
        /// <param name="function">The Lua/MoonSharp function to be called</param>
        /// <param name="args">The arguments to pass to the function.</param>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function</exception>
        public Task<DynValue> CallAsync(ExecutionControlToken ecToken, object function, params object[] args)
        {
            return this.CallAsync(ecToken, DynValue.FromObject(this, function), args);
        }

        /// <summary>
        /// Asynchronously creates a new dynamic expression.
        /// </summary>
        /// <param name="code">The code of the expression.</param>
        public Task<DynamicExpression> CreateDynamicExpressionAsync(string code)
        {
            return Task.Factory.StartNew(() => this.CreateDynamicExpression(code));
        }


        /// <summary>
        /// Creates a coroutine pointing at the specified function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>
        /// The coroutine handle.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function or DataType.ClrFunction</exception>
        public DynValue CreateCoroutine(DynValue function)
        {
            this.CheckScriptOwnership(function);

            if (function.Type == DataType.Function)
            {
                return _mainProcessor.Coroutine_Create(function.Function);
            }

            if (function.Type == DataType.ClrFunction)
            {
                return DynValue.NewCoroutine(new Coroutine(function.Callback));
            }

            throw new ArgumentException("function is not of DataType.Function or DataType.ClrFunction");
        }

        /// <summary>
        /// Creates a coroutine pointing at the specified function.
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>
        /// The coroutine handle.
        /// </returns>
        /// <exception cref="System.ArgumentException">Thrown if function is not of DataType.Function or DataType.ClrFunction</exception>
        public DynValue CreateCoroutine(object function)
        {
            return this.CreateCoroutine(DynValue.FromObject(this, function));
        }

        /// <summary>
        /// Loads a module as per the "require" Lua function. http://www.lua.org/pil/8.1.html
        /// </summary>
        /// <param name="modname">The module name</param>
        /// <param name="globalContext">The global context.</param>
        /// <exception cref="ScriptRuntimeException">Raised if module is not found</exception>
        public DynValue RequireModule(string modname, Table globalContext = null)
        {
            this.CheckScriptOwnership(globalContext);

            var globals = globalContext ?? this.Globals;
            string filename = this.Options.ScriptLoader.ResolveModuleName(modname, globals);

            if (filename == null)
            {
                throw new ScriptRuntimeException("module '{0}' not found", modname);
            }

            var func = this.LoadFile(filename, globalContext, filename);
            return func;
        }


        /// <summary>
        /// Gets a type metatable.
        /// </summary>
        /// <param name="type">The type.</param>
        public Table GetTypeMetatable(DataType type)
        {
            int t = (int) type;

            if (t >= 0 && t < _typeMetaTables.Length)
            {
                return _typeMetaTables[t];
            }

            return null;
        }

        /// <summary>
        /// Sets a type metatable.
        /// </summary>
        /// <param name="type">The type. Must be Nil, Boolean, Number, String or Function</param>
        /// <param name="metatable">The metatable.</param>
        /// <exception cref="System.ArgumentException">Specified type not supported :  + type.ToString()</exception>
        public void SetTypeMetatable(DataType type, Table metatable)
        {
            this.CheckScriptOwnership(metatable);

            int t = (int) type;

            if (t >= 0 && t < _typeMetaTables.Length)
            {
                _typeMetaTables[t] = metatable;
            }
            else
            {
                throw new ArgumentException($"Specified type not supported : {type}");
            }
        }


        /// <summary>
        /// Warms up the parser/lexer structures so that MoonSharp operations start faster.
        /// </summary>
        public static void WarmUp()
        {
            var s = new Script(CoreModules.Basic);
            s.LoadString("return 1;");
        }

        /// <summary>
        /// Resets and clears the code that's been loaded into this <see cref="Script"/> object.
        /// </summary>
        public void Reset()
        {
            _byteCode.Reset();
        }

        /// <summary>
        /// Creates a new dynamic expression.
        /// </summary>
        /// <param name="code">The code of the expression.</param>
        public DynamicExpression CreateDynamicExpression(string code)
        {
            var dee = Loader_Fast.LoadDynamicExpr(this, new SourceCode("__dynamic", code, this));
            return new DynamicExpression(this, code, dee);
        }

        /// <summary>
        /// Creates a new dynamic expression which is actually quite static, returning always the same constant value.
        /// </summary>
        /// <param name="code">The code of the not-so-dynamic expression.</param>
        /// <param name="constant">The constant to return.</param>
        public DynamicExpression CreateConstantDynamicExpression(string code, DynValue constant)
        {
            this.CheckScriptOwnership(constant);

            return new DynamicExpression(this, code, constant);
        }

        /// <summary>
        /// Gets an execution context exposing only partial functionality, which should be used for
        /// those cases where the execution engine is not really running - for example for dynamic expression
        /// or calls from CLR to CLR callbacks
        /// </summary>
        internal ScriptExecutionContext CreateDynamicExecutionContext(ExecutionControlToken ecToken, CallbackFunction func = null)
        {
            return new ScriptExecutionContext(ecToken, _mainProcessor, func, null, true);
        }
    }
}