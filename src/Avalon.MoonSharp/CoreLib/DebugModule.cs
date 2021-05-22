// Disable warnings about XML documentation

#pragma warning disable 1591

namespace MoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing debug Lua functions. Support for the debug module is partial. 
    /// </summary>
    [MoonSharpModule(Namespace = "debug")]
    public class DebugModule
    {
        [MoonSharpModuleMethod]
        public static DynValue getuservalue(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var v = args[0];

            if (v.Type != DataType.UserData)
            {
                return DynValue.Nil;
            }

            return v.UserData.UserValue ?? DynValue.Nil;
        }

        [MoonSharpModuleMethod]
        public static DynValue setuservalue(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var v = args.AsType(0, "setuservalue", DataType.UserData);
            var t = args.AsType(0, "setuservalue", DataType.Table, true);

            return v.UserData.UserValue = t;
        }

        [MoonSharpModuleMethod]
        public static DynValue getregistry(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return DynValue.NewTable(executionContext.GetScript().Registry);
        }

        [MoonSharpModuleMethod]
        public static DynValue getmetatable(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var v = args[0];
            var S = executionContext.GetScript();

            if (v.Type.CanHaveTypeMetatables())
            {
                return DynValue.NewTable(S.GetTypeMetatable(v.Type));
            }

            if (v.Type == DataType.Table && v.Table.MetaTable != null)
            {
                return DynValue.NewTable(v.Table.MetaTable);
            }

            return DynValue.Nil;
        }

        [MoonSharpModuleMethod]
        public static DynValue setmetatable(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var v = args[0];
            var t = args.AsType(1, "setmetatable", DataType.Table, true);
            var m = (t.IsNil()) ? null : t.Table;
            var S = executionContext.GetScript();

            if (v.Type.CanHaveTypeMetatables())
            {
                S.SetTypeMetatable(v.Type, m);
            }
            else if (v.Type == DataType.Table)
            {
                v.Table.MetaTable = m;
            }
            else
            {
                throw new ScriptRuntimeException("cannot debug.setmetatable on type {0}", v.Type.ToErrorTypeString());
            }

            return v;
        }

        [MoonSharpModuleMethod]
        public static DynValue getupvalue(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            int index = (int) args.AsType(1, "getupvalue", DataType.Number).Number - 1;

            if (args[0].Type == DataType.ClrFunction)
            {
                return DynValue.Nil;
            }

            var fn = args.AsType(0, "getupvalue", DataType.Function).Function;

            var closure = fn.ClosureContext;

            if (index < 0 || index >= closure.Count)
            {
                return DynValue.Nil;
            }

            return DynValue.NewTuple(
                DynValue.NewString(closure.Symbols[index]),
                closure[index]);
        }


        [MoonSharpModuleMethod]
        public static DynValue upvalueid(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            int index = (int) args.AsType(1, "getupvalue", DataType.Number).Number - 1;

            if (args[0].Type == DataType.ClrFunction)
            {
                return DynValue.Nil;
            }

            var fn = args.AsType(0, "getupvalue", DataType.Function).Function;

            var closure = fn.ClosureContext;

            if (index < 0 || index >= closure.Count)
            {
                return DynValue.Nil;
            }

            return DynValue.NewNumber(closure[index].ReferenceID);
        }


        [MoonSharpModuleMethod]
        public static DynValue setupvalue(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            int index = (int) args.AsType(1, "setupvalue", DataType.Number).Number - 1;

            if (args[0].Type == DataType.ClrFunction)
            {
                return DynValue.Nil;
            }

            var fn = args.AsType(0, "setupvalue", DataType.Function).Function;

            var closure = fn.ClosureContext;

            if (index < 0 || index >= closure.Count)
            {
                return DynValue.Nil;
            }

            closure[index].Assign(args[2]);

            return DynValue.NewString(closure.Symbols[index]);
        }


        [MoonSharpModuleMethod]
        public static DynValue upvaluejoin(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var f1 = args.AsType(0, "upvaluejoin", DataType.Function);
            var f2 = args.AsType(2, "upvaluejoin", DataType.Function);
            int n1 = args.AsInt(1, "upvaluejoin") - 1;
            int n2 = args.AsInt(3, "upvaluejoin") - 1;

            var c1 = f1.Function;
            var c2 = f2.Function;

            if (n1 < 0 || n1 >= c1.ClosureContext.Count)
            {
                throw ScriptRuntimeException.BadArgument(1, "upvaluejoin", "invalid upvalue index");
            }

            if (n2 < 0 || n2 >= c2.ClosureContext.Count)
            {
                throw ScriptRuntimeException.BadArgument(3, "upvaluejoin", "invalid upvalue index");
            }

            c2.ClosureContext[n2] = c1.ClosureContext[n1];

            return DynValue.Void;
        }


        [MoonSharpModuleMethod]
        public static DynValue traceback(ScriptExecutionContext executionContext, CallbackArguments args)
        {
                var vMessage = args[0];
                var vLevel = args[1];

                double defaultSkip = 1.0;

                var cor = executionContext.GetCallingCoroutine();

                if (vMessage.Type == DataType.Thread)
                {
                    cor = vMessage.Coroutine;
                    vMessage = args[1];
                    vLevel = args[2];
                    defaultSkip = 0.0;
                }

                if (vMessage.IsNotNil() && vMessage.Type != DataType.String && vMessage.Type != DataType.Number)
                {
                    return vMessage;
                }

                string message = vMessage.CastToString();

                int skip = (int)((vLevel.CastToNumber()) ?? defaultSkip);

                return DynValue.NewString(message ?? "");
        }
    }
}