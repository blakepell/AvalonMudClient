using MoonSharp.Interpreter.Serialization.Json;

namespace MoonSharp.Interpreter.CoreLib
{
    [MoonSharpModule(Namespace = "json")]
    public class JsonModule
    {
        [MoonSharpModuleMethod]
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

        [MoonSharpModuleMethod]
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

        [MoonSharpModuleMethod]
        public static DynValue isnull(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var vs = args[0];
            return DynValue.NewBoolean((JsonNull.IsJsonNull(vs)) || (vs.IsNil()));
        }

        [MoonSharpModuleMethod]
        public static DynValue @null(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            return JsonNull.Create();
        }
    }
}