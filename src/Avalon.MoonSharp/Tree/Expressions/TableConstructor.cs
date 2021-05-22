using System.Collections.Generic;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Expressions
{
    internal class TableConstructor : Expression
    {
        private List<KeyValuePair<Expression, Expression>> _ctorArgs = new List<KeyValuePair<Expression, Expression>>();

        private List<Expression> _positionalValues = new List<Expression>();
        private bool _shared;

        public TableConstructor(ScriptLoadingContext lcontext, bool shared) : base(lcontext)
        {
            _shared = shared;

            // here lexer is at the '{', go on
            CheckTokenType(lcontext, TokenType.Brk_Open_Curly, TokenType.Brk_Open_Curly_Shared);

            while (lcontext.Lexer.Current.Type != TokenType.Brk_Close_Curly)
            {
                switch (lcontext.Lexer.Current.Type)
                {
                    case TokenType.Name:
                    {
                        var assign = lcontext.Lexer.PeekNext();

                        if (assign.Type == TokenType.Op_Assignment)
                        {
                            this.StructField(lcontext);
                        }
                        else
                        {
                            this.ArrayField(lcontext);
                        }
                    }
                        break;
                    case TokenType.Brk_Open_Square:
                        this.MapField(lcontext);
                        break;
                    default:
                        this.ArrayField(lcontext);
                        break;
                }

                var curr = lcontext.Lexer.Current;

                if (curr.Type == TokenType.Comma || curr.Type == TokenType.SemiColon)
                {
                    lcontext.Lexer.Next();
                }
                else
                {
                    break;
                }
            }

            CheckTokenType(lcontext, TokenType.Brk_Close_Curly);
        }

        private void MapField(ScriptLoadingContext lcontext)
        {
            lcontext.Lexer.Next(); // skip '['

            var key = Expr(lcontext);

            CheckTokenType(lcontext, TokenType.Brk_Close_Square);

            CheckTokenType(lcontext, TokenType.Op_Assignment);

            var value = Expr(lcontext);

            _ctorArgs.Add(new KeyValuePair<Expression, Expression>(key, value));
        }

        private void StructField(ScriptLoadingContext lcontext)
        {
            Expression key = new LiteralExpression(lcontext, DynValue.NewString(lcontext.Lexer.Current.Text));
            lcontext.Lexer.Next();

            CheckTokenType(lcontext, TokenType.Op_Assignment);

            var value = Expr(lcontext);

            _ctorArgs.Add(new KeyValuePair<Expression, Expression>(key, value));
        }


        private void ArrayField(ScriptLoadingContext lcontext)
        {
            var e = Expr(lcontext);
            _positionalValues.Add(e);
        }


        public override void Compile(ByteCode bc)
        {
            bc.Emit_NewTable(_shared);

            foreach (var kvp in _ctorArgs)
            {
                kvp.Key.Compile(bc);
                kvp.Value.Compile(bc);
                bc.Emit_TblInitN();
            }

            for (int i = 0; i < _positionalValues.Count; i++)
            {
                _positionalValues[i].Compile(bc);
                bc.Emit_TblInitI(i == _positionalValues.Count - 1);
            }
        }


        public override DynValue Eval(ScriptExecutionContext context)
        {
            if (!_shared)
            {
                throw new DynamicExpressionException("Dynamic Expressions cannot define new non-prime tables.");
            }

            var tval = DynValue.NewPrimeTable();
            var t = tval.Table;

            int idx = 0;
            foreach (var e in _positionalValues)
            {
                t.Set(++idx, e.Eval(context));
            }

            foreach (var kvp in _ctorArgs)
            {
                t.Set(kvp.Key.Eval(context), kvp.Value.Eval(context));
            }

            return tval;
        }
    }
}