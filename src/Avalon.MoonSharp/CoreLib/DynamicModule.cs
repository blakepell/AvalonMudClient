﻿// Disable warnings about XML documentation

#pragma warning disable 1591


namespace MoonSharp.Interpreter.CoreLib
{
    /// <summary>
    /// Class implementing dynamic expression evaluations at runtime (a MoonSharp addition).
    /// </summary>
    [MoonSharpModule(Namespace = "dynamic")]
    public class DynamicModule
    {
        public static void MoonSharpInit(Table globalTable, Table stringTable)
        {
            UserData.RegisterType<DynamicExprWrapper>(InteropAccessMode.HideMembers);
        }

        [MoonSharpModuleMethod(Description = "Evaluates an expression",
            AutoCompleteHint = "dynamic.eval(object obj)",
            ParameterCount = 1,
            ReturnTypeHint = "object")]
        public static DynValue eval(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            try
            {
                if (args[0].Type == DataType.UserData)
                {
                    var ud = args[0].UserData;
                    if (ud.Object is DynamicExprWrapper wrapper)
                    {
                        return wrapper.Expr.Evaluate(executionContext);
                    }

                    throw ScriptRuntimeException.BadArgument(0, "dynamic.eval",
                        "A userdata was passed, but was not a previously prepared expression.");
                }

                var vs = args.AsType(0, "dynamic.eval", DataType.String);
                var expr = executionContext.GetScript().CreateDynamicExpression(vs.String);
                return expr.Evaluate(executionContext);
            }
            catch (SyntaxErrorException ex)
            {
                throw new ScriptRuntimeException(ex);
            }
        }

        [MoonSharpModuleMethod(Description = "Prepares an expression for evaluation.",
            AutoCompleteHint = "dynamic.prepare(object obj)",
            ParameterCount = 1,
            ReturnTypeHint = "object")]
        public static DynValue prepare(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            try
            {
                var vs = args.AsType(0, "dynamic.prepare", DataType.String);
                var expr = executionContext.GetScript().CreateDynamicExpression(vs.String);
                return UserData.Create(new DynamicExprWrapper {Expr = expr});
            }
            catch (SyntaxErrorException ex)
            {
                throw new ScriptRuntimeException(ex);
            }
        }

        private class DynamicExprWrapper
        {
            public DynamicExpression Expr;
        }
    }
}