using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class ScopeBlockStatement : Statement
    {
        private Statement _block;
        private SourceRef _do, _end;
        private RuntimeScopeBlock _stackFrame;

        public ScopeBlockStatement(ScriptLoadingContext lcontext) : base(lcontext)
        {
            lcontext.Scope.PushBlock();

            _do = CheckTokenType(lcontext, TokenType.Do).GetSourceRef();

            _block = new CompositeStatement(lcontext);

            _end = CheckTokenType(lcontext, TokenType.End).GetSourceRef();

            _stackFrame = lcontext.Scope.PopBlock();
            lcontext.Source.Refs.Add(_do);
            lcontext.Source.Refs.Add(_end);
        }

        public override void Compile(ByteCode bc)
        {
            using (bc.EnterSource(_do))
            {
                bc.Emit_Enter(_stackFrame);
            }

            _block.Compile(bc);

            using (bc.EnterSource(_end))
            {
                bc.Emit_Leave(_stackFrame);
            }
        }
    }
}