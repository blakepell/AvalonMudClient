using System.Collections.Generic;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Expressions
{
    internal class FunctionCallExpression : Expression
    {
        private List<Expression> _arguments;
        private string _debugErr;
        private Expression _function;
        private string _name;

        public FunctionCallExpression(ScriptLoadingContext lcontext, Expression function, Token thisCallName)
            : base(lcontext)
        {
            var callToken = thisCallName ?? lcontext.Lexer.Current;

            _name = thisCallName?.Text;
            _debugErr = function.GetFriendlyDebugName();
            _function = function;

            switch (lcontext.Lexer.Current.Type)
            {
                case TokenType.Brk_Open_Round:
                    var openBrk = lcontext.Lexer.Current;
                    lcontext.Lexer.Next();
                    var t = lcontext.Lexer.Current;
                    if (t.Type == TokenType.Brk_Close_Round)
                    {
                        _arguments = new List<Expression>();
                        this.SourceRef = callToken.GetSourceRef(t);
                        lcontext.Lexer.Next();
                    }
                    else
                    {
                        _arguments = ExprList(lcontext);
                        this.SourceRef =
                            callToken.GetSourceRef(CheckMatch(lcontext, openBrk, TokenType.Brk_Close_Round, ")"));
                    }

                    break;
                case TokenType.String:
                case TokenType.String_Long:
                {
                    _arguments = new List<Expression>();
                    Expression le = new LiteralExpression(lcontext, lcontext.Lexer.Current);
                    _arguments.Add(le);
                    this.SourceRef = callToken.GetSourceRef(lcontext.Lexer.Current);
                }
                    break;
                case TokenType.Brk_Open_Curly:
                case TokenType.Brk_Open_Curly_Shared:
                {
                    _arguments = new List<Expression>
                    {
                        new TableConstructor(lcontext,
                            lcontext.Lexer.Current.Type == TokenType.Brk_Open_Curly_Shared)
                    };
                    this.SourceRef = callToken.GetSourceRefUpTo(lcontext.Lexer.Current);
                }
                    break;
                default:
                    throw new SyntaxErrorException(lcontext.Lexer.Current, "function arguments expected")
                    {
                        IsPrematureStreamTermination = (lcontext.Lexer.Current.Type == TokenType.Eof)
                    };
            }
        }

        internal SourceRef SourceRef { get; }

        public override void Compile(ByteCode bc)
        {
            _function.Compile(bc);

            int argslen = _arguments.Count;

            if (!string.IsNullOrEmpty(_name))
            {
                bc.Emit_Copy(0);
                bc.Emit_Index(DynValue.NewString(_name), true);
                bc.Emit_Swap(0, 1);
                ++argslen;
            }

            for (int i = 0; i < _arguments.Count; i++)
            {
                _arguments[i].Compile(bc);
            }

            if (!string.IsNullOrEmpty(_name))
            {
                bc.Emit_ThisCall(argslen, _debugErr);
            }
            else
            {
                bc.Emit_Call(argslen, _debugErr);
            }
        }

        public override DynValue Eval(ScriptExecutionContext context)
        {
            throw new DynamicExpressionException("Dynamic Expressions cannot call functions.");
        }
    }
}