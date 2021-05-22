using System;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Expressions
{
    internal class DynamicExprExpression : Expression
    {
        private Expression _exp;

        public DynamicExprExpression(Expression exp, ScriptLoadingContext lcontext) : base(lcontext)
        {
            lcontext.Anonymous = true;
            _exp = exp;
        }


        public override DynValue Eval(ScriptExecutionContext context)
        {
            return _exp.Eval(context);
        }

        public override void Compile(ByteCode bc)
        {
            throw new InvalidOperationException();
        }

        public override SymbolRef FindDynamic(ScriptExecutionContext context)
        {
            return _exp.FindDynamic(context);
        }
    }
}