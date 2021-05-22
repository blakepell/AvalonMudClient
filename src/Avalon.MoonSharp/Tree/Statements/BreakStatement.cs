using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class BreakStatement : Statement
    {
        private SourceRef _ref;

        public BreakStatement(ScriptLoadingContext lcontext) : base(lcontext)
        {
            _ref = CheckTokenType(lcontext, TokenType.Break).GetSourceRef();
            lcontext.Source.Refs.Add(_ref);
        }

        public override void Compile(ByteCode bc)
        {
            using (bc.EnterSource(_ref))
            {
                if (bc.LoopTracker.Loops.Count == 0)
                {
                    throw new SyntaxErrorException(this.Script, _ref, "<break> at line {0} not inside a loop",
                        _ref.FromLine);
                }

                var loop = bc.LoopTracker.Loops.Peek();

                if (loop.IsBoundary())
                {
                    throw new SyntaxErrorException(this.Script, _ref, "<break> at line {0} not inside a loop",
                        _ref.FromLine);
                }

                loop.CompileBreak(bc);
            }
        }
    }
}