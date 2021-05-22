using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree
{
    internal abstract class NodeBase
    {
        public NodeBase(ScriptLoadingContext lcontext)
        {
            this.Script = lcontext.Script;
        }

        public Script Script { get; }
        protected ScriptLoadingContext LoadingContext { get; private set; }

        public abstract void Compile(ByteCode bc);

        protected static Token UnexpectedTokenType(Token t)
        {
            throw new SyntaxErrorException(t, "unexpected symbol near '{0}'", t.Text)
            {
                IsPrematureStreamTermination = (t.Type == TokenType.Eof)
            };
        }

        protected static Token CheckTokenType(ScriptLoadingContext lcontext, TokenType tokenType)
        {
            var t = lcontext.Lexer.Current;

            if (t.Type != tokenType)
            {
                return UnexpectedTokenType(t);
            }

            lcontext.Lexer.Next();

            return t;
        }


        protected static Token CheckTokenType(ScriptLoadingContext lcontext, TokenType tokenType1, TokenType tokenType2)
        {
            var t = lcontext.Lexer.Current;

            if (t.Type != tokenType1 && t.Type != tokenType2)
            {
                return UnexpectedTokenType(t);
            }

            lcontext.Lexer.Next();

            return t;
        }

        protected static Token CheckTokenType(ScriptLoadingContext lcontext, TokenType tokenType1, TokenType tokenType2, TokenType tokenType3)
        {
            var t = lcontext.Lexer.Current;

            if (t.Type != tokenType1 && t.Type != tokenType2 && t.Type != tokenType3)
            {
                return UnexpectedTokenType(t);
            }

            lcontext.Lexer.Next();

            return t;
        }

        protected static void CheckTokenTypeNotNext(ScriptLoadingContext lcontext, TokenType tokenType)
        {
            var t = lcontext.Lexer.Current;

            if (t.Type != tokenType)
            {
                UnexpectedTokenType(t);
            }
        }

        protected static Token CheckMatch(ScriptLoadingContext lcontext, Token originalToken, TokenType expectedTokenType, string expectedTokenText)
        {
            var t = lcontext.Lexer.Current;
            if (t.Type != expectedTokenType)
            {
                throw new SyntaxErrorException(lcontext.Lexer.Current,
                    "'{0}' expected (to close '{1}' at line {2}) near '{3}'",
                    expectedTokenText, originalToken.Text, originalToken.FromLine, t.Text)
                {
                    IsPrematureStreamTermination = (t.Type == TokenType.Eof)
                };
            }

            lcontext.Lexer.Next();

            return t;
        }
    }
}