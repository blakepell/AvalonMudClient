using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Expressions;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Tree.Fast_Interface
{
    internal static class Loader_Fast
    {
        internal static DynamicExprExpression LoadDynamicExpr(Script script, SourceCode source)
        {
            var lcontext = CreateLoadingContext(script, source);

            try
            {
                lcontext.IsDynamicExpression = true;
                lcontext.Anonymous = true;

                Expression exp;
                exp = Expression.Expr(lcontext);

                return new DynamicExprExpression(exp, lcontext);
            }
            catch (SyntaxErrorException ex)
            {
                ex.DecorateMessage(script);
                ex.Rethrow();
                throw;
            }
        }

        private static ScriptLoadingContext CreateLoadingContext(Script script, SourceCode source)
        {
            return new ScriptLoadingContext(script)
            {
                Scope = new BuildTimeScope(),
                Source = source,
                Lexer = new Lexer(source.Code, true)
            };
        }

        internal static int LoadChunk(Script script, SourceCode source, ByteCode bytecode)
        {
            var lcontext = CreateLoadingContext(script, source);

            try
            {
                Statement stat = new ChunkStatement(lcontext);

                int beginIp;

                using (bytecode.EnterSource(null))
                {
                    bytecode.Emit_Nop($"Begin chunk {source.Name}");
                    beginIp = bytecode.GetJumpPointForLastInstruction();
                    stat.Compile(bytecode);
                    bytecode.Emit_Nop($"End chunk {source.Name}");
                }

                return beginIp;
            }
            catch (SyntaxErrorException ex)
            {
                ex.DecorateMessage(script);
                ex.Rethrow();
                throw;
            }
        }

        internal static int LoadFunction(Script script, SourceCode source, ByteCode bytecode, bool usesGlobalEnv)
        {
            var lcontext = CreateLoadingContext(script, source);

            try
            {
                FunctionDefinitionExpression fnx;

                fnx = new FunctionDefinitionExpression(lcontext, usesGlobalEnv);

                int beginIp;

                using (bytecode.EnterSource(null))
                {
                    bytecode.Emit_Nop($"Begin function {source.Name}");
                    beginIp = fnx.CompileBody(bytecode, source.Name);
                    bytecode.Emit_Nop($"End function {source.Name}");
                }

                return beginIp;
            }
            catch (SyntaxErrorException ex)
            {
                ex.DecorateMessage(script);
                ex.Rethrow();
                throw;
            }
        }
    }
}