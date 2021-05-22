using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class ReturnStatement : Statement
    {
        private Expression _expression;
        private SourceRef _ref;

        public ReturnStatement(ScriptLoadingContext lcontext, Expression e, SourceRef sref) : base(lcontext)
        {
            _expression = e;
            _ref = sref;
            lcontext.Source.Refs.Add(sref);
        }

        public ReturnStatement(ScriptLoadingContext lcontext) : base(lcontext)
        {
            var ret = lcontext.Lexer.Current;

            lcontext.Lexer.Next();

            var cur = lcontext.Lexer.Current;

            if (cur.IsEndOfBlock() || cur.Type == TokenType.SemiColon)
            {
                _expression = null;
                _ref = ret.GetSourceRef();
            }
            else
            {
                _expression = new ExprListExpression(Expression.ExprList(lcontext), lcontext);
                _ref = ret.GetSourceRefUpTo(lcontext.Lexer.Current);
            }

            lcontext.Source.Refs.Add(_ref);
        }

        public override void Compile(ByteCode bc)
        {
            using (bc.EnterSource(_ref))
            {
                if (_expression != null)
                {
                    _expression.Compile(bc);
                    bc.Emit_Ret(1);
                }
                else
                {
                    bc.Emit_Ret(0);
                }
            }
        }
    }
}