using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class WhileStatement : Statement
    {
        private Statement _block;
        private Expression _condition;
        private RuntimeScopeBlock _stackFrame;
        private SourceRef _start, _end;

        public WhileStatement(ScriptLoadingContext lcontext)
            : base(lcontext)
        {
            var whileTk = CheckTokenType(lcontext, TokenType.While);

            _condition = Expression.Expr(lcontext);
            _start = whileTk.GetSourceRefUpTo(lcontext.Lexer.Current);

            lcontext.Scope.PushBlock();
            CheckTokenType(lcontext, TokenType.Do);
            _block = new CompositeStatement(lcontext);
            _end = CheckTokenType(lcontext, TokenType.End).GetSourceRef();
            _stackFrame = lcontext.Scope.PopBlock();

            lcontext.Source.Refs.Add(_start);
            lcontext.Source.Refs.Add(_end);
        }


        public override void Compile(ByteCode bc)
        {
            var l = new Loop
            {
                Scope = _stackFrame
            };


            bc.LoopTracker.Loops.Push(l);

            bc.PushSourceRef(_start);

            int start = bc.GetJumpPointForNextInstruction();

            _condition.Compile(bc);
            var jumpend = bc.Emit_Jump(OpCode.Jf, -1);

            bc.Emit_Enter(_stackFrame);

            _block.Compile(bc);

            bc.PopSourceRef();
            bc.PushSourceRef(_end);

            bc.Emit_Leave(_stackFrame);
            bc.Emit_Jump(OpCode.Jump, start);

            bc.LoopTracker.Loops.Pop();

            int exitPoint = bc.GetJumpPointForNextInstruction();

            foreach (var i in l.BreakJumps)
            {
                i.NumVal = exitPoint;
            }

            jumpend.NumVal = exitPoint;

            bc.PopSourceRef();
        }
    }
}