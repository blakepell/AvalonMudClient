using System.Collections.Generic;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class FunctionDefinitionStatement : Statement
    {
        private string _friendlyName;
        private FunctionDefinitionExpression _funcDef;
        private SymbolRef _funcSymbol;
        private bool _isMethodCallingConvention;
        private bool _local;
        private string _methodName;
        private SourceRef _sourceRef;
        private List<string> _tableAccessors;

        public FunctionDefinitionStatement(ScriptLoadingContext lcontext, bool local, Token localToken) : base(lcontext)
        {
            // here lexer must be at the 'function' keyword
            var funcKeyword = CheckTokenType(lcontext, TokenType.Function);
            funcKeyword = localToken ?? funcKeyword; // for debugger purposes

            _local = local;

            if (_local)
            {
                var name = CheckTokenType(lcontext, TokenType.Name);
                _funcSymbol = lcontext.Scope.TryDefineLocal(name.Text);
                _friendlyName = $"{name.Text} (local)";
                _sourceRef = funcKeyword.GetSourceRef(name);
            }
            else
            {
                var name = CheckTokenType(lcontext, TokenType.Name);
                string firstName = name.Text;

                _sourceRef = funcKeyword.GetSourceRef(name);

                _funcSymbol = lcontext.Scope.Find(firstName);
                _friendlyName = firstName;

                if (lcontext.Lexer.Current.Type != TokenType.Brk_Open_Round)
                {
                    _tableAccessors = new List<string>();

                    while (lcontext.Lexer.Current.Type != TokenType.Brk_Open_Round)
                    {
                        var separator = lcontext.Lexer.Current;

                        if (separator.Type != TokenType.Colon && separator.Type != TokenType.Dot)
                        {
                            UnexpectedTokenType(separator);
                        }

                        lcontext.Lexer.Next();

                        var field = CheckTokenType(lcontext, TokenType.Name);

                        _friendlyName += $"{separator.Text}{field.Text}";
                        _sourceRef = funcKeyword.GetSourceRef(field);

                        if (separator.Type == TokenType.Colon)
                        {
                            _methodName = field.Text;
                            _isMethodCallingConvention = true;
                            break;
                        }

                        _tableAccessors.Add(field.Text);
                    }

                    if (_methodName == null && _tableAccessors.Count > 0)
                    {
                        _methodName = _tableAccessors[_tableAccessors.Count - 1];
                        _tableAccessors.RemoveAt(_tableAccessors.Count - 1);
                    }
                }
            }

            _funcDef = new FunctionDefinitionExpression(lcontext, _isMethodCallingConvention, false);
            lcontext.Source.Refs.Add(_sourceRef);
        }

        public override void Compile(ByteCode bc)
        {
            using (bc.EnterSource(_sourceRef))
            {
                if (_local)
                {
                    bc.Emit_Literal(DynValue.Nil);
                    bc.Emit_Store(_funcSymbol, 0, 0);
                    _funcDef.Compile(bc, () => this.SetFunction(bc, 2), _friendlyName);
                }
                else if (_methodName == null)
                {
                    _funcDef.Compile(bc, () => this.SetFunction(bc, 1), _friendlyName);
                }
                else
                {
                    _funcDef.Compile(bc, () => this.SetMethod(bc), _friendlyName);
                }
            }
        }

        private int SetMethod(ByteCode bc)
        {
            int cnt = 0;

            cnt += bc.Emit_Load(_funcSymbol);

            foreach (string str in _tableAccessors)
            {
                bc.Emit_Index(DynValue.NewString(str), true);
                cnt += 1;
            }

            bc.Emit_IndexSet(0, 0, DynValue.NewString(_methodName), true);

            return 1 + cnt;
        }

        private int SetFunction(ByteCode bc, int numPop)
        {
            int num = bc.Emit_Store(_funcSymbol, 0, 0);
            bc.Emit_Pop(numPop);
            return num + 1;
        }
    }
}