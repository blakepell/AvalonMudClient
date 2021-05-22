using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class FunctionCallStatement : Statement
    {
        private FunctionCallExpression _functionCallExpression;

        public FunctionCallStatement(ScriptLoadingContext lcontext, FunctionCallExpression functionCallExpression) : base(lcontext)
        {
            _functionCallExpression = functionCallExpression;
            lcontext.Source.Refs.Add(_functionCallExpression.SourceRef);
        }


        public override void Compile(ByteCode bc)
        {
            using (bc.EnterSource(_functionCallExpression.SourceRef))
            {
                _functionCallExpression.Compile(bc);
                this.RemoveBreakpointStop(bc.Emit_Pop());
            }
        }

        private void RemoveBreakpointStop(Instruction instruction)
        {
            instruction.SourceCodeRef = null;
        }
    }
}