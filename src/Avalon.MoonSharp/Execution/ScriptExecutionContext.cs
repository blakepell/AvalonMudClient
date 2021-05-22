using System;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Interop.LuaStateInterop;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// Class giving access to details of the environment where the script is executing
    /// </summary>
    public class ScriptExecutionContext : IScriptPrivateResource
    {
        private CallbackFunction _callback;
        internal ExecutionControlToken _ecToken;
        private Processor _processor;

        internal ScriptExecutionContext(ExecutionControlToken ecToken, Processor p, CallbackFunction callBackFunction,
            SourceRef sourceRef, bool isDynamic = false)
        {
            this.IsDynamicExecution = isDynamic;
            _processor = p;
            _callback = callBackFunction;
            _ecToken = ecToken;
            this.CallingLocation = sourceRef;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running a dynamic execution.
        /// Under a dynamic execution, most methods of ScriptExecutionContext are not reliable as the
        /// processing engine of the script is not "really" running or is not available.
        /// </summary>
        public bool IsDynamicExecution { get; }

        /// <summary>
        /// Gets the location of the code calling back 
        /// </summary>
        public SourceRef CallingLocation { get; }

        /// <summary>
        /// Gets or sets the additional data associated to this CLR function call.
        /// </summary>
        public object AdditionalData
        {
            get => _callback?.AdditionalData;
            set
            {
                if (_callback == null)
                {
                    throw new InvalidOperationException(
                        "Cannot set additional data on a context which has no callback");
                }

                _callback.AdditionalData = value;
            }
        }

        /// <summary>
        /// Gets the current global env, or null if not found.
        /// </summary>
        public Table CurrentGlobalEnv
        {
            get
            {
                var env = this.EvaluateSymbolByName(WellKnownSymbols.ENV);

                if (env == null || env.Type != DataType.Table)
                {
                    return null;
                }

                return env.Table;
            }
        }


        /// <summary>
        /// Gets the script owning this resource.
        /// </summary>
        /// <value>
        /// The script owning this resource.
        /// </value>
        public Script OwnerScript => this.GetScript();


        /// <summary>
        /// Gets the metatable associated with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        public Table GetMetatable(DynValue value)
        {
            return _processor.GetMetatable(value);
        }


        /// <summary>
        /// Gets the specified metamethod associated with the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="metamethod">The metamethod name.</param>
        public DynValue GetMetamethod(DynValue value, string metamethod)
        {
            return _processor.GetMetamethod(_ecToken, value, metamethod);
        }

        /// <summary>
        /// prepares a tail call request for the specified metamethod, or null if no metamethod is found.
        /// </summary>
        public DynValue GetMetamethodTailCall(DynValue value, string metamethod, params DynValue[] args)
        {
            var meta = this.GetMetamethod(value, metamethod);
            if (meta == null)
            {
                return null;
            }

            return DynValue.NewTailCallReq(meta, args);
        }

        /// <summary>
        /// Gets the metamethod to be used for a binary operation using op1 and op2.
        /// </summary>
        public DynValue GetBinaryMetamethod(DynValue op1, DynValue op2, string eventName)
        {
            return _processor.GetBinaryMetamethod(_ecToken, op1, op2, eventName);
        }

        /// <summary>
        /// Gets the script object associated with this request
        /// </summary>
        public Script GetScript()
        {
            return _processor.GetScript();
        }

        /// <summary>
        /// Gets the coroutine which is performing the call
        /// </summary>
        public Coroutine GetCallingCoroutine()
        {
            return _processor.AssociatedCoroutine;
        }

        /// <summary>
        /// Calls a callback function implemented in "classic way". 
        /// Useful to port C code from Lua, or C# code from UniLua and KopiLua.
        /// Lua : http://www.lua.org/
        /// UniLua : http://github.com/xebecnan/UniLua
        /// KopiLua : http://github.com/NLua/KopiLua
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="functionName">Name of the function - for error messages.</param>
        /// <param name="callback">The callback.</param>
        public DynValue EmulateClassicCall(CallbackArguments args, string functionName, Func<LuaState, int> callback)
        {
            var L = new LuaState(this, args, functionName);
            int retvals = callback(L);
            return L.GetReturnValue(retvals);
        }

        /// <summary>
        /// Calls the specified function, supporting most cases. The called function must not yield.
        /// </summary>
        /// <param name="func">The function; it must be a Function or ClrFunction or have a call metamethod defined.</param>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ScriptRuntimeException">If the function yields, returns a tail call request with continuations/handlers or, of course, if it encounters errors.</exception>
        public DynValue Call(DynValue func, params DynValue[] args)
        {
            if (func.Type == DataType.Function)
            {
                return this.GetScript().Call(func, args);
            }

            if (func.Type == DataType.ClrFunction)
            {
                while (true)
                {
                    var ret = func.Callback.Invoke(this, args);

                    if (ret.Type == DataType.YieldRequest)
                    {
                        throw ScriptRuntimeException.CannotYield();
                    }

                    if (ret.Type == DataType.TailCallRequest)
                    {
                        var tail = ret.TailCallData;

                        if (tail.Continuation != null || tail.ErrorHandler != null)
                        {
                            throw new ScriptRuntimeException(
                                "the function passed cannot be called directly. wrap in a script function instead.");
                        }

                        args = tail.Args;
                        func = tail.Function;
                    }
                    else
                    {
                        return ret;
                    }
                }
            }

            // TODO while(true)? or make this actually work.
            int maxloops = 10;

            while (maxloops > 0)
            {
                var v = this.GetMetamethod(func, "__call");

                if (v == null || v.IsNil())
                {
                    throw ScriptRuntimeException.AttemptToCallNonFunc(func.Type);
                }
                
                func = v;

                if (func.Type == DataType.Function || func.Type == DataType.ClrFunction)
                {
                    return this.Call(func, args);
                }
            }

            throw ScriptRuntimeException.LoopInCall();
        }

        /// <summary>
        /// Tries to get the reference of a symbol in the current execution state
        /// </summary>
        public DynValue EvaluateSymbol(SymbolRef symref)
        {
            if (symref == null)
            {
                return DynValue.Nil;
            }

            return _processor.GetGenericSymbol(symref);
        }

        /// <summary>
        /// Tries to get the value of a symbol in the current execution state
        /// </summary>
        public DynValue EvaluateSymbolByName(string symbol)
        {
            return this.EvaluateSymbol(this.FindSymbolByName(symbol));
        }

        /// <summary>
        /// Finds a symbol by name in the current execution state
        /// </summary>
        public SymbolRef FindSymbolByName(string symbol)
        {
            return _processor.FindSymbolByName(symbol);
        }

        /// <summary>
        /// Performs a message decoration before unwinding after an error. To be used in the implementation of xpcall like functions.
        /// </summary>
        /// <param name="messageHandler">The message handler.</param>
        /// <param name="exception">The exception.</param>
        public void PerformMessageDecorationBeforeUnwind(DynValue messageHandler, ScriptRuntimeException exception)
        {
            if (messageHandler != null)
            {
                exception.DecoratedMessage = _processor.PerformMessageDecorationBeforeUnwind(_ecToken, messageHandler,
                    exception.Message, this.CallingLocation);
            }
            else
            {
                exception.DecoratedMessage = exception.Message;
            }
        }


        /// <summary>
        /// Pauses the script thread for the specified amount of time. 
        /// 
        /// </summary>
        /// <param name="timeout">Timeout.</param>
        public void PauseExecution(TimeSpan timeout)
        {
            _ecToken.Wait(timeout);

            // This is not strictly required, but why allow the code
            // to go back to Processor::Processing_Loop if we can check right here if
            // we should stop or not?
            if (_ecToken.IsAbortRequested)
            {
                throw new ScriptTerminationRequestedException();
            }
        }
    }
}