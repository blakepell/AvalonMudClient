using System.Collections.Generic;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class IfStatement : Statement
    {
        private IfBlock _else;
        private SourceRef _end;

        private List<IfBlock> _ifs = new List<IfBlock>();

        public IfStatement(ScriptLoadingContext lcontext) : base(lcontext)
        {
            while (lcontext.Lexer.Current.Type != TokenType.Else && lcontext.Lexer.Current.Type != TokenType.End)
            {
                _ifs.Add(this.CreateIfBlock(lcontext));
            }

            if (lcontext.Lexer.Current.Type == TokenType.Else)
            {
                _else = this.CreateElseBlock(lcontext);
            }

            _end = CheckTokenType(lcontext, TokenType.End).GetSourceRef();
            lcontext.Source.Refs.Add(_end);
        }

        private IfBlock CreateIfBlock(ScriptLoadingContext lcontext)
        {
            var type = CheckTokenType(lcontext, TokenType.If, TokenType.ElseIf);

            lcontext.Scope.PushBlock();

            var ifblock = new IfBlock
            {
                Exp = Expression.Expr(lcontext),
                Source = type.GetSourceRef(CheckTokenType(lcontext, TokenType.Then)),
                Block = new CompositeStatement(lcontext),
                StackFrame = lcontext.Scope.PopBlock()
            };

            lcontext.Source.Refs.Add(ifblock.Source);

            return ifblock;
        }

        private IfBlock CreateElseBlock(ScriptLoadingContext lcontext)
        {
            var type = CheckTokenType(lcontext, TokenType.Else);

            lcontext.Scope.PushBlock();

            var ifblock = new IfBlock
            {
                Block = new CompositeStatement(lcontext),
                StackFrame = lcontext.Scope.PopBlock(),
                Source = type.GetSourceRef()
            };

            lcontext.Source.Refs.Add(ifblock.Source);
            return ifblock;
        }


        public override void Compile(ByteCode bc)
        {
            var endJumps = new List<Instruction>();
            Instruction lastIfJmp = null;

            foreach (var ifblock in _ifs)
            {
                using (bc.EnterSource(ifblock.Source))
                {
                    if (lastIfJmp != null)
                    {
                        lastIfJmp.NumVal = bc.GetJumpPointForNextInstruction();
                    }

                    ifblock.Exp.Compile(bc);
                    lastIfJmp = bc.Emit_Jump(OpCode.Jf, -1);
                    bc.Emit_Enter(ifblock.StackFrame);
                    ifblock.Block.Compile(bc);
                }

                using (bc.EnterSource(_end))
                {
                    bc.Emit_Leave(ifblock.StackFrame);
                }

                endJumps.Add(bc.Emit_Jump(OpCode.Jump, -1));
            }

            lastIfJmp.NumVal = bc.GetJumpPointForNextInstruction();

            if (_else != null)
            {
                using (bc.EnterSource(_else.Source))
                {
                    bc.Emit_Enter(_else.StackFrame);
                    _else.Block.Compile(bc);
                }

                using (bc.EnterSource(_end))
                {
                    bc.Emit_Leave(_else.StackFrame);
                }
            }

            foreach (var endjmp in endJumps)
            {
                endjmp.NumVal = bc.GetJumpPointForNextInstruction();
            }
        }

        private class IfBlock
        {
            public Statement Block;
            public Expression Exp;
            public SourceRef Source;
            public RuntimeScopeBlock StackFrame;
        }
    }
}