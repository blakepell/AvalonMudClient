using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class AssignmentStatement : Statement
    {
        private List<IVariable> _lValues = new List<IVariable>();
        private SourceRef _ref;
        private List<Expression> _rValues;

        public AssignmentStatement(ScriptLoadingContext lcontext, Token startToken) : base(lcontext)
        {
            var names = new List<string>();
            var first = startToken;

            while (true)
            {
                var name = CheckTokenType(lcontext, TokenType.Name);
                names.Add(name.Text);

                if (lcontext.Lexer.Current.Type != TokenType.Comma)
                {
                    break;
                }

                lcontext.Lexer.Next();
            }

            if (lcontext.Lexer.Current.Type == TokenType.Op_Assignment)
            {
                CheckTokenType(lcontext, TokenType.Op_Assignment);
                _rValues = Expression.ExprList(lcontext);
            }
            else
            {
                _rValues = new List<Expression>();
            }

            foreach (string name in names)
            {
                var localVar = lcontext.Scope.TryDefineLocal(name);
                var symbol = new SymbolRefExpression(lcontext, localVar);
                _lValues.Add(symbol);
            }

            var last = lcontext.Lexer.Current;
            _ref = first.GetSourceRefUpTo(last);
            lcontext.Source.Refs.Add(_ref);
        }

        public AssignmentStatement(ScriptLoadingContext lcontext, Expression firstExpression, Token first) : base(lcontext)
        {
            _lValues.Add(this.CheckVar(lcontext, firstExpression));

            while (lcontext.Lexer.Current.Type == TokenType.Comma)
            {
                lcontext.Lexer.Next();
                var e = Expression.PrimaryExp(lcontext);
                _lValues.Add(this.CheckVar(lcontext, e));
            }

            CheckTokenType(lcontext, TokenType.Op_Assignment);

            _rValues = Expression.ExprList(lcontext);

            var last = lcontext.Lexer.Current;
            _ref = first.GetSourceRefUpTo(last);
            lcontext.Source.Refs.Add(_ref);
        }

        private IVariable CheckVar(ScriptLoadingContext lcontext, Expression firstExpression)
        {
            var v = firstExpression as IVariable;

            if (v == null)
            {
                throw new SyntaxErrorException(lcontext.Lexer.Current, "unexpected symbol near '{0}' - not a l-value",
                    lcontext.Lexer.Current);
            }

            return v;
        }

        public override void Compile(ByteCode bc)
        {
            using (bc.EnterSource(_ref))
            {
                foreach (var exp in _rValues)
                {
                    exp.Compile(bc);
                }

                for (int i = 0; i < _lValues.Count; i++)
                {
                    _lValues[i].CompileAssignment(bc,
                        Math.Max(_rValues.Count - 1 - i, 0), // index of r-value
                        i - Math.Min(i, _rValues.Count - 1)); // index in last tuple
                }

                bc.Emit_Pop(_rValues.Count);
            }
        }
    }
}