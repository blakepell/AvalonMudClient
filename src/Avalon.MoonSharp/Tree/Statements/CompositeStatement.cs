using System.Collections.Generic;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class CompositeStatement : Statement
    {
        private List<Statement> _statements = new List<Statement>();

        public CompositeStatement(ScriptLoadingContext lcontext) : base(lcontext)
        {
            while (true)
            {
                var t = lcontext.Lexer.Current;

                if (t.IsEndOfBlock())
                {
                    break;
                }

                var s = CreateStatement(lcontext, out bool forceLast);
                _statements.Add(s);

                if (forceLast)
                {
                    break;
                }
            }

            // eat away all superfluos ';'s
            while (lcontext.Lexer.Current.Type == TokenType.SemiColon)
            {
                lcontext.Lexer.Next();
            }
        }

        public override void Compile(ByteCode bc)
        {
            if (_statements == null)
            {
                return;
            }

            foreach (var s in _statements)
            {
                s.Compile(bc);
            }
        }
    }
}