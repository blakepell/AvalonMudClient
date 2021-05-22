using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Expressions
{
    internal class AdjustmentExpression : Expression
    {
        private Expression _expression;

        public AdjustmentExpression(ScriptLoadingContext lcontext, Expression exp) : base(lcontext)
        {
            _expression = exp;
        }

        public override void Compile(ByteCode bc)
        {
            _expression.Compile(bc);
            bc.Emit_Scalar();
        }

        public override DynValue Eval(ScriptExecutionContext context)
        {
            return _expression.Eval(context).ToScalar();
        }
    }
}