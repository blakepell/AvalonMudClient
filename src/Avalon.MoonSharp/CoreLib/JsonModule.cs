using MoonSharp.Interpreter.Serialization.Json;

namespace MoonSharp.Interpreter.CoreLib
{
    [MoonSharpModule(Namespace = "json")]
    public class JsonModule
    {
        [MoonSharpModuleMethod(Description = "Parses a JSON string and returns a table.",
            AutoCompleteHint = "json.parse(string json)",
            ParameterCount = 1,
            ReturnTypeHint = "table")]
        public static DynValue parse(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            try
            {
                var vs = args.AsType(0, "parse", DataType.String);
                var t = JsonTableConverter.JsonToTable(vs.String, executionContext.GetScript());
                return DynValue.NewTable(t);
            }
            catch (SyntaxErrorException ex)
            {
                throw new ScriptRuntimeException(ex);
            }
        }

        [MoonSharpModuleMethod(Description = "Returns a serialized string representing a JSON object.",
            AutoCompleteHint = "json.serialize(object obj)",
            ParameterCount = 1,
            ReturnTypeHint = "string")]
        public static DynValue serialize(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            try
            {
                var vt = args.AsType(0, "serialize", DataType.Table);
                string s = vt.Table.TableToJson();
                return DynValue.NewString(s);
            }
            catch (SyntaxErrorException ex)
            {
                throw new ScriptRuntimeException(ex);
            }
        }

        [MoonSharpModuleMethod(Description = "If a provided JSON value is null.",
            AutoCompleteHint = "json.isnull(object obj)",
            ParameterCount = 1,
            ReturnTypeHint = "bool")]
        public static DynValue isnull(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var vs = args[0];
            return DynValue.NewBoolean((JsonNull.IsJsonNull(vs)) || (vs.IsNil()));
        }

        [MoonSharpModuleMethod(Description = "Returns a null JSON value.",
            AutoCompleteHint = "json.null()",
            ParameterCount = 0,
            ReturnTypeHint = "JsonNull")]
        public static DynValue @null(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return JsonNull.Create();
        }
    }
}