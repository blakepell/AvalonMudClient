using System.Collections.Generic;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Expressions
{
    internal class ExprListExpression : Expression
    {
        private List<Expression> _expressions;

        public ExprListExpression(List<Expression> exps, ScriptLoadingContext lcontext) : base(lcontext)
        {
            _expressions = exps;
        }


        public Expression[] GetExpressions()
        {
            return _expressions.ToArray();
        }

        public override void Compile(ByteCode bc)
        {
            foreach (var exp in _expressions)
            {
                exp.Compile(bc);
            }

            if (_expressions.Count > 1)
            {
                bc.Emit_MkTuple(_expressions.Count);
            }
        }

        public override DynValue Eval(ScriptExecutionContext context)
        {
            if (_expressions.Count >= 1)
            {
                return _expressions[0].Eval(context);
            }

            return DynValue.Void;
        }
    }
}