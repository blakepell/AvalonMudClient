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
        [MoonSharpModuleMethod(Description = "Returns a value for the specified object.",
            AutoCompleteHint = "debug.getuservalue(object obj)",
            ParameterCount = 1,
            ReturnTypeHint = "object")]
        public static DynValue getuservalue(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var v = args[0];

            if (v.Type != DataType.UserData)
            {
                return DynValue.Nil;
            }

            return v.UserData.UserValue ?? DynValue.Nil;
        }

        [MoonSharpModuleMethod(Description = "Sets a user value.",
            AutoCompleteHint = "debug.setuservalue(object value)",
            ParameterCount = 1,
            ReturnTypeHint = "bool")]
        public static DynValue setuservalue(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var v = args.AsType(0, "setuservalue", DataType.UserData);
            var t = args.AsType(0, "setuservalue", DataType.Table, true);

            return v.UserData.UserValue = t;
        }

        [MoonSharpModuleMethod(Description = "Gets a the registry for a given script.",
            AutoCompleteHint = "debug.getregistry()",
            ParameterCount = 0,
            ReturnTypeHint = "table")]
        public static DynValue getregistry(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return DynValue.NewTable(executionContext.GetScript().Registry);
        }

        [MoonSharpModuleMethod(Description = "Returns a table for the specified value.",
            AutoCompleteHint = "debug.getmetatable(object value)",
            ParameterCount = 1,
            ReturnTypeHint = "table")]
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

        [MoonSharpModuleMethod(Description = "Sets the value of a metatable.",
            AutoCompleteHint = "debug.setmetatable(object obj, object value)",
            ParameterCount = 2,
            ReturnTypeHint = "table")]
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

        [MoonSharpModuleMethod(Description = "Gets an up value.",
            AutoCompleteHint = "debug.getupvalue(int value, function func)",
            ParameterCount = 2,
            ReturnTypeHint = "tuple<string, closure>")]
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


        [MoonSharpModuleMethod(Description = "Returns the ID of an up value.",
            AutoCompleteHint = "debug.upvaludid(function func, int value)",
            ParameterCount = 2,
            ReturnTypeHint = "int")]
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


        [MoonSharpModuleMethod(Description = "Sets an up value",
            AutoCompleteHint = "debug.setupvalue(int value, function func)",
            ParameterCount = 2,
            ReturnTypeHint = "string")]
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


        [MoonSharpModuleMethod(Description = "Joins two up values.",
            AutoCompleteHint = "debug.upvaluejoin(function func1, int value, function func2, int value2)",
            ParameterCount = 4,
            ReturnTypeHint = "void")]
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


        [MoonSharpModuleMethod(Description = "No description available.",
            AutoCompleteHint = "debug.traceback(object obj1, object obj2)",
            ParameterCount = 2,
            ReturnTypeHint = "string")]
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