using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class ForLoopStatement : Statement
    {
        private Statement _innerBlock;
        private SourceRef _refFor, _refEnd;

        //for' NAME '=' exp ',' exp (',' exp)? 'do' block 'end'
        private RuntimeScopeBlock _stackFrame;
        private Expression _start, _end, _step;
        private SymbolRef _varName;

        public ForLoopStatement(ScriptLoadingContext lcontext, Token nameToken, Token forToken) : base(lcontext)
        {
            //	for Name ‘=’ exp ‘,’ exp [‘,’ exp] do block end | 

            // lexer already at the '=' ! [due to dispatching vs for-each]
            CheckTokenType(lcontext, TokenType.Op_Assignment);

            _start = Expression.Expr(lcontext);
            CheckTokenType(lcontext, TokenType.Comma);
            _end = Expression.Expr(lcontext);

            if (lcontext.Lexer.Current.Type == TokenType.Comma)
            {
                lcontext.Lexer.Next();
                _step = Expression.Expr(lcontext);
            }
            else
            {
                _step = new LiteralExpression(lcontext, DynValue.NewNumber(1));
            }

            lcontext.Scope.PushBlock();
            _varName = lcontext.Scope.DefineLocal(nameToken.Text);
            _refFor = forToken.GetSourceRef(CheckTokenType(lcontext, TokenType.Do));
            _innerBlock = new CompositeStatement(lcontext);
            _refEnd = CheckTokenType(lcontext, TokenType.End).GetSourceRef();
            _stackFrame = lcontext.Scope.PopBlock();

            lcontext.Source.Refs.Add(_refFor);
            lcontext.Source.Refs.Add(_refEnd);
        }


        public override void Compile(ByteCode bc)
        {
            bc.PushSourceRef(_refFor);

            var l = new Loop
            {
                Scope = _stackFrame
            };

            bc.LoopTracker.Loops.Push(l);

            _end.Compile(bc);
            bc.Emit_ToNum(3);
            _step.Compile(bc);
            bc.Emit_ToNum(2);
            _start.Compile(bc);
            bc.Emit_ToNum(1);

            int start = bc.GetJumpPointForNextInstruction();
            var jumpend = bc.Emit_Jump(OpCode.JFor, -1);
            bc.Emit_Enter(_stackFrame);

            bc.Emit_Store(_varName, 0, 0);

            _innerBlock.Compile(bc);

            bc.PopSourceRef();
            bc.PushSourceRef(_refEnd);

            bc.Emit_Leave(_stackFrame);
            bc.Emit_Incr(1);
            bc.Emit_Jump(OpCode.Jump, start);

            bc.LoopTracker.Loops.Pop();

            int exitpoint = bc.GetJumpPointForNextInstruction();

            foreach (var i in l.BreakJumps)
            {
                i.NumVal = exitpoint;
            }

            jumpend.NumVal = exitpoint;
            bc.Emit_Pop(3);

            bc.PopSourceRef();
        }
    }
}