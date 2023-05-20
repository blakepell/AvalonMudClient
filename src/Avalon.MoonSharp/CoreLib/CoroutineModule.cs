// Disable warnings about XML documentation

#pragma warning disable 1591

using System.Collections.Generic;

namespace MoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing coroutine Lua functions 
    /// </summary>
    [MoonSharpModule(Namespace = "coroutine")]
    public class CoroutineModule
    {
        [MoonSharpModuleMethod(Description = "Creates a new coroutine",
            AutoCompleteHint = "coroutine.create(function func)",
            ParameterCount = 1)]
        public static DynValue create(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            if (args[0].Type != DataType.Function && args[0].Type != DataType.ClrFunction)
            {
                args.AsType(0, "create", DataType.Function); // this throws
            }

            return executionContext.GetScript().CreateCoroutine(args[0]);
        }

        [MoonSharpModuleMethod(Description = "Returns a function that resumes the coroutine each time it is called.",
            AutoCompleteHint = "coroutine.wrap(function func)",
            ParameterCount = 1)]
        public static DynValue wrap(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            if (args[0].Type != DataType.Function && args[0].Type != DataType.ClrFunction)
            {
                args.AsType(0, "wrap", DataType.Function); // this throws
            }

            var v = create(executionContext, args);
            var c = DynValue.NewCallback(__wrap_wrapper);
            c.Callback.AdditionalData = v;
            return c;
        }

        public static DynValue __wrap_wrapper(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var handle = (DynValue) executionContext.AdditionalData;
            return handle.Coroutine.Resume(args.GetArray());
        }

        [MoonSharpModuleMethod(Description = "Starts of continues the execution of a coroutine.",
            AutoCompleteHint = "coroutine.create(thread t)",
            ParameterCount = 1,
            ReturnTypeHint = "tuple<object...>")]
        public static DynValue resume(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var handle = args.AsType(0, "resume", DataType.Thread);

            try
            {
                var ret = handle.Coroutine.Resume(args.GetArray(1));

                var retval = new List<DynValue> { DynValue.True };

                if (ret.Type == DataType.Tuple)
                {
                    for (int i = 0; i < ret.Tuple.Length; i++)
                    {
                        var v = ret.Tuple[i];

                        if ((i == ret.Tuple.Length - 1) && (v.Type == DataType.Tuple))
                        {
                            retval.AddRange(v.Tuple);
                        }
                        else
                        {
                            retval.Add(v);
                        }
                    }
                }
                else
                {
                    retval.Add(ret);
                }

                return DynValue.NewTuple(retval.ToArray());
            }
            catch (ScriptRuntimeException ex)
            {
                return DynValue.NewTuple(
                    DynValue.False,
                    DynValue.NewString(ex.Message));
            }
        }

        [MoonSharpModuleMethod(Description = "Yields a thread (or thread pool) of the current coroutine dispatcher to other coroutines to run.",
            AutoCompleteHint = "coroutine.yield()",
            ParameterCount = 0)]
        public static DynValue yield(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return DynValue.NewYieldReq(args.GetArray());
        }


        [MoonSharpModuleMethod(Description = "Returns the running coroutine.",
            AutoCompleteHint = "coroutine.create(function func)",
            ParameterCount = 0,
            ReturnTypeHint = "tuple<coroutine, bool>")]
        public static DynValue running(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var C = executionContext.GetCallingCoroutine();
            return DynValue.NewTuple(DynValue.NewCoroutine(C), DynValue.NewBoolean(C.State == CoroutineState.Main));
        }

        [MoonSharpModuleMethod(Description = "Returns the status of a coroutine.",
            AutoCompleteHint = "coroutine.status(thread t)",
            ParameterCount = 1,
            ReturnTypeHint = "string")]
        public static DynValue status(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var handle = args.AsType(0, "status", DataType.Thread);
            var running = executionContext.GetCallingCoroutine();
            var cs = handle.Coroutine.State;

            switch (cs)
            {
                case CoroutineState.Main:
                case CoroutineState.Running:
                    return (handle.Coroutine == running) ? DynValue.NewString("running") : DynValue.NewString("normal");
                case CoroutineState.NotStarted:
                case CoroutineState.Suspended:
                case CoroutineState.ForceSuspended:
                    return DynValue.NewString("suspended");
                case CoroutineState.Dead:
                    return DynValue.NewString("dead");
                default:
                    throw new InternalErrorException("Unexpected coroutine state {0}", cs);
            }
        }
    }
}