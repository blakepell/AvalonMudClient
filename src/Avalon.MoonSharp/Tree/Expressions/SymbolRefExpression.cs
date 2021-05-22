using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Expressions
{
    internal class SymbolRefExpression : Expression, IVariable
    {
        private SymbolRef _ref;
        private string _varName;

        public SymbolRefExpression(Token T, ScriptLoadingContext lcontext) : base(lcontext)
        {
            _varName = T.Text;

            if (T.Type == TokenType.VarArgs)
            {
                _ref = lcontext.Scope.Find(WellKnownSymbols.VARARGS);

                if (!lcontext.Scope.CurrentFunctionHasVarArgs())
                {
                    throw new SyntaxErrorException(T, "cannot use '...' outside a vararg function");
                }

                if (lcontext.IsDynamicExpression)
                {
                    throw new DynamicExpressionException("cannot use '...' in a dynamic expression.");
                }
            }
            else
            {
                if (!lcontext.IsDynamicExpression)
                {
                    _ref = lcontext.Scope.Find(_varName);
                }
            }

            lcontext.Lexer.Next();
        }

        public SymbolRefExpression(ScriptLoadingContext lcontext, SymbolRef refr) : base(lcontext)
        {
            _ref = refr;

            if (lcontext.IsDynamicExpression)
            {
                throw new DynamicExpressionException("Unsupported symbol reference expression detected.");
            }
        }


        public void CompileAssignment(ByteCode bc, int stackofs, int tupleidx)
        {
            bc.Emit_Store(_ref, stackofs, tupleidx);
        }

        public override void Compile(ByteCode bc)
        {
            bc.Emit_Load(_ref);
        }

        public override DynValue Eval(ScriptExecutionContext context)
        {
            return context.EvaluateSymbolByName(_varName);
        }

        public override SymbolRef FindDynamic(ScriptExecutionContext context)
        {
            return context.FindSymbolByName(_varName);
        }
    }
}