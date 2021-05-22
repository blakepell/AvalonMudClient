using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class RepeatStatement : Statement
    {
        private Statement _block;
        private Expression _condition;
        private SourceRef _repeat, _until;
        private RuntimeScopeBlock _stackFrame;

        public RepeatStatement(ScriptLoadingContext lcontext) : base(lcontext)
        {
            _repeat = CheckTokenType(lcontext, TokenType.Repeat).GetSourceRef();

            lcontext.Scope.PushBlock();
            _block = new CompositeStatement(lcontext);

            var until = CheckTokenType(lcontext, TokenType.Until);

            _condition = Expression.Expr(lcontext);

            _until = until.GetSourceRefUpTo(lcontext.Lexer.Current);

            _stackFrame = lcontext.Scope.PopBlock();
            lcontext.Source.Refs.Add(_repeat);
            lcontext.Source.Refs.Add(_until);
        }

        public override void Compile(ByteCode bc)
        {
            var l = new Loop
            {
                Scope = _stackFrame
            };

            bc.PushSourceRef(_repeat);

            bc.LoopTracker.Loops.Push(l);

            int start = bc.GetJumpPointForNextInstruction();

            bc.Emit_Enter(_stackFrame);
            _block.Compile(bc);

            bc.PopSourceRef();
            bc.PushSourceRef(_until);

            _condition.Compile(bc);
            bc.Emit_Leave(_stackFrame);
            bc.Emit_Jump(OpCode.Jf, start);

            bc.LoopTracker.Loops.Pop();

            int exitPoint = bc.GetJumpPointForNextInstruction();

            foreach (var i in l.BreakJumps)
            {
                i.NumVal = exitPoint;
            }

            bc.PopSourceRef();
        }
    }
}