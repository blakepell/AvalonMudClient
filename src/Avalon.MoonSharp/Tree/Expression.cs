using System.Collections.Generic;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree
{
    internal abstract class Expression : NodeBase
    {
        public Expression(ScriptLoadingContext lcontext) : base(lcontext)
        {
        }

        public virtual string GetFriendlyDebugName()
        {
            return null;
        }

        public abstract DynValue Eval(ScriptExecutionContext context);

        public virtual SymbolRef FindDynamic(ScriptExecutionContext context)
        {
            return null;
        }

        internal static List<Expression> ExprListAfterFirstExpr(ScriptLoadingContext lcontext, Expression expr1)
        {
            var exps = new List<Expression> { expr1 };

            while ((lcontext.Lexer.Current.Type == TokenType.Comma))
            {
                lcontext.Lexer.Next();
                exps.Add(Expr(lcontext));
            }

            return exps;
        }


        internal static List<Expression> ExprList(ScriptLoadingContext lcontext)
        {
            var exps = new List<Expression>();

            while (true)
            {
                exps.Add(Expr(lcontext));

                if (lcontext.Lexer.Current.Type != TokenType.Comma)
                {
                    break;
                }

                lcontext.Lexer.Next();
            }

            return exps;
        }

        internal static Expression Expr(ScriptLoadingContext lcontext)
        {
            return SubExpr(lcontext, true);
        }

        internal static Expression SubExpr(ScriptLoadingContext lcontext, bool isPrimary)
        {
            Expression e;

            var t = lcontext.Lexer.Current;

            if (t.IsUnaryOperator())
            {
                lcontext.Lexer.Next();
                e = SubExpr(lcontext, false);

                // Check for power operator -- it be damned forever and ever for being higher priority than unary ops
                var unaryOp = t;
                t = lcontext.Lexer.Current;

                if (isPrimary && t.Type == TokenType.Op_Pwr)
                {
                    var powerChain = new List<Expression> { e };

                    while (t.Type == TokenType.Op_Pwr)
                    {
                        lcontext.Lexer.Next();
                        powerChain.Add(SubExpr(lcontext, false));
                        t = lcontext.Lexer.Current;
                    }

                    e = powerChain[^1];

                    for (int i = powerChain.Count - 2; i >= 0; i--)
                    {
                        e = BinaryOperatorExpression.CreatePowerExpression(powerChain[i], e, lcontext);
                    }
                }

                e = new UnaryOperatorExpression(lcontext, e, unaryOp);
            }
            else
            {
                e = SimpleExp(lcontext);
            }

            t = lcontext.Lexer.Current;

            if (isPrimary && t.IsBinaryOperator())
            {
                var chain = BinaryOperatorExpression.BeginOperatorChain();

                BinaryOperatorExpression.AddExpressionToChain(chain, e);

                while (t.IsBinaryOperator())
                {
                    BinaryOperatorExpression.AddOperatorToChain(chain, t);
                    lcontext.Lexer.Next();
                    var right = SubExpr(lcontext, false);
                    BinaryOperatorExpression.AddExpressionToChain(chain, right);
                    t = lcontext.Lexer.Current;
                }

                e = BinaryOperatorExpression.CommitOperatorChain(chain, lcontext);
            }

            return e;
        }

        internal static Expression SimpleExp(ScriptLoadingContext lcontext)
        {
            var t = lcontext.Lexer.Current;

            switch (t.Type)
            {
                case TokenType.Number:
                case TokenType.Number_Hex:
                case TokenType.Number_HexFloat:
                case TokenType.String:
                case TokenType.String_Long:
                case TokenType.Nil:
                case TokenType.True:
                case TokenType.False:
                    return new LiteralExpression(lcontext, t);
                case TokenType.VarArgs:
                    return new SymbolRefExpression(t, lcontext);
                case TokenType.Brk_Open_Curly:
                case TokenType.Brk_Open_Curly_Shared:
                    return new TableConstructor(lcontext, t.Type == TokenType.Brk_Open_Curly_Shared);
                case TokenType.Function:
                    lcontext.Lexer.Next();
                    return new FunctionDefinitionExpression(lcontext, false, false);
                case TokenType.Lambda:
                    return new FunctionDefinitionExpression(lcontext, false, true);
                default:
                    return PrimaryExp(lcontext);
            }
        }

        /// <summary>
        /// Primaries the exp.
        /// </summary>
        /// <param name="lcontext">The lcontext.</param>
        internal static Expression PrimaryExp(ScriptLoadingContext lcontext)
        {
            var e = PrefixExp(lcontext);

            while (true)
            {
                var t = lcontext.Lexer.Current;
                Token thisCallName = null;

                switch (t.Type)
                {
                    case TokenType.Dot:
                    {
                        lcontext.Lexer.Next();
                        var name = CheckTokenType(lcontext, TokenType.Name);
                        e = new IndexExpression(e, name.Text, lcontext);
                    }
                        break;
                    case TokenType.Brk_Open_Square:
                    {
                        var openBrk = lcontext.Lexer.Current;
                        lcontext.Lexer.Next(); // skip bracket
                        var index = Expr(lcontext);

                        // support moonsharp multiple indexers for userdata
                        if (lcontext.Lexer.Current.Type == TokenType.Comma)
                        {
                            var explist = ExprListAfterFirstExpr(lcontext, index);
                            index = new ExprListExpression(explist, lcontext);
                        }

                        CheckMatch(lcontext, openBrk, TokenType.Brk_Close_Square, "]");
                        e = new IndexExpression(e, index, lcontext);
                    }
                        break;
                    case TokenType.Colon:
                        lcontext.Lexer.Next();
                        thisCallName = CheckTokenType(lcontext, TokenType.Name);
                        goto case TokenType.Brk_Open_Round;
                    case TokenType.Brk_Open_Round:
                    case TokenType.String:
                    case TokenType.String_Long:
                    case TokenType.Brk_Open_Curly:
                    case TokenType.Brk_Open_Curly_Shared:
                        e = new FunctionCallExpression(lcontext, e, thisCallName);
                        break;
                    default:
                        return e;
                }
            }
        }


        private static Expression PrefixExp(ScriptLoadingContext lcontext)
        {
            var T = lcontext.Lexer.Current;
            switch (T.Type)
            {
                case TokenType.Brk_Open_Round:
                    lcontext.Lexer.Next();
                    var e = Expr(lcontext);
                    e = new AdjustmentExpression(lcontext, e);
                    CheckMatch(lcontext, T, TokenType.Brk_Close_Round, ")");
                    return e;
                case TokenType.Name:
                    return new SymbolRefExpression(T, lcontext);
                default:
                    throw new SyntaxErrorException(T, "unexpected symbol near '{0}'", T.Text)
                    {
                        IsPrematureStreamTermination = (T.Type == TokenType.Eof)
                    };
            }
        }
    }
}