using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Tree.Expressions
{
    internal class FunctionDefinitionExpression : Expression, IClosureBuilder
    {
        private SourceRef _begin, _end;
        private List<SymbolRef> _closure = new List<SymbolRef>();
        private Instruction _closureInstruction;
        private SymbolRef _env;
        private bool _hasVarArgs;
        private SymbolRef[] _paramNames;
        private RuntimeScopeFrame _stackFrame;
        private Statement _statement;
        private bool _usesGlobalEnv;

        public FunctionDefinitionExpression(ScriptLoadingContext lcontext, bool usesGlobalEnv) : this(lcontext, false, usesGlobalEnv, false)
        {
        }

        public FunctionDefinitionExpression(ScriptLoadingContext lcontext, bool pushSelfParam, bool isLambda) : this(lcontext, pushSelfParam, false, isLambda)
        {
        }

        private FunctionDefinitionExpression(ScriptLoadingContext lcontext, bool pushSelfParam, bool usesGlobalEnv, bool isLambda) : base(lcontext)
        {
            if (_usesGlobalEnv = usesGlobalEnv)
            {
                CheckTokenType(lcontext, TokenType.Function);
            }

            // here lexer should be at the '(' or at the '|'
            var openRound = CheckTokenType(lcontext, isLambda ? TokenType.Lambda : TokenType.Brk_Open_Round);
            var paramnames = this.BuildParamList(lcontext, pushSelfParam, openRound, isLambda);

            // here lexer is at first token of body
            _begin = openRound.GetSourceRefUpTo(lcontext.Lexer.Current);

            // create scope
            lcontext.Scope.PushFunction(this, _hasVarArgs);

            if (_usesGlobalEnv)
            {
                _env = lcontext.Scope.DefineLocal(WellKnownSymbols.ENV);
            }
            else
            {
                lcontext.Scope.ForceEnvUpValue();
            }

            _paramNames = this.DefineArguments(paramnames, lcontext);

            if (isLambda)
            {
                _statement = this.CreateLambdaBody(lcontext);
            }
            else
            {
                _statement = this.CreateBody(lcontext);
            }

            _stackFrame = lcontext.Scope.PopFunction();

            lcontext.Source.Refs.Add(_begin);
            lcontext.Source.Refs.Add(_end);
        }

        public SymbolRef CreateUpvalue(BuildTimeScope scope, SymbolRef symbol)
        {
            for (int i = 0; i < _closure.Count; i++)
            {
                if (_closure[i]._name == symbol._name)
                {
                    return SymbolRef.Upvalue(symbol._name, i);
                }
            }

            _closure.Add(symbol);

            if (_closureInstruction != null)
            {
                _closureInstruction.SymbolList = _closure.ToArray();
            }

            return SymbolRef.Upvalue(symbol._name, _closure.Count - 1);
        }


        private Statement CreateLambdaBody(ScriptLoadingContext lcontext)
        {
            var start = lcontext.Lexer.Current;
            var e = Expr(lcontext);
            var end = lcontext.Lexer.Current;
            var sref = start.GetSourceRefUpTo(end);
            return new ReturnStatement(lcontext, e, sref);
        }


        private Statement CreateBody(ScriptLoadingContext lcontext)
        {
            Statement s = new CompositeStatement(lcontext);

            if (lcontext.Lexer.Current.Type != TokenType.End)
            {
                throw new SyntaxErrorException(lcontext.Lexer.Current, "'end' expected near '{0}'",
                    lcontext.Lexer.Current.Text)
                {
                    IsPrematureStreamTermination = (lcontext.Lexer.Current.Type == TokenType.Eof)
                };
            }

            _end = lcontext.Lexer.Current.GetSourceRef();

            lcontext.Lexer.Next();
            return s;
        }

        private List<string> BuildParamList(ScriptLoadingContext lcontext, bool pushSelfParam, Token openBracketToken, bool isLambda)
        {
            var closeToken = isLambda ? TokenType.Lambda : TokenType.Brk_Close_Round;

            var paramnames = new List<string>();

            // method decls with ':' must push an implicit 'self' param
            if (pushSelfParam)
            {
                paramnames.Add("self");
            }

            while (lcontext.Lexer.Current.Type != closeToken)
            {
                var t = lcontext.Lexer.Current;

                if (t.Type == TokenType.Name)
                {
                    paramnames.Add(t.Text);
                }
                else if (t.Type == TokenType.VarArgs)
                {
                    _hasVarArgs = true;
                    paramnames.Add(WellKnownSymbols.VARARGS);
                }
                else
                {
                    UnexpectedTokenType(t);
                }

                lcontext.Lexer.Next();

                t = lcontext.Lexer.Current;

                if (t.Type == TokenType.Comma)
                {
                    lcontext.Lexer.Next();
                }
                else
                {
                    CheckMatch(lcontext, openBracketToken, closeToken, isLambda ? "|" : ")");
                    break;
                }
            }

            if (lcontext.Lexer.Current.Type == closeToken)
            {
                lcontext.Lexer.Next();
            }

            return paramnames;
        }

        private SymbolRef[] DefineArguments(List<string> paramnames, ScriptLoadingContext lcontext)
        {
            var names = new HashSet<string>();

            var ret = new SymbolRef[paramnames.Count];

            for (int i = paramnames.Count - 1; i >= 0; i--)
            {
                if (!names.Add(paramnames[i]))
                {
                    paramnames[i] = $"{paramnames[i]}@{i.ToString()}";
                }

                ret[i] = lcontext.Scope.DefineLocal(paramnames[i]);
            }

            return ret;
        }

        public override DynValue Eval(ScriptExecutionContext context)
        {
            throw new DynamicExpressionException("Dynamic Expressions cannot define new functions.");
        }

        public int CompileBody(ByteCode bc, string friendlyName)
        {
            string funcName = friendlyName ?? $"<{_begin.FormatLocation(bc.Script)}>";

            bc.PushSourceRef(_begin);

            var I = bc.Emit_Jump(OpCode.Jump, -1);

            var meta = bc.Emit_Meta(funcName, OpCodeMetadataType.FunctionEntrypoint);
            int metaip = bc.GetJumpPointForLastInstruction();

            bc.Emit_BeginFn(_stackFrame);

            bc.LoopTracker.Loops.Push(new LoopBoundary());

            int entryPoint = bc.GetJumpPointForLastInstruction();

            if (_usesGlobalEnv)
            {
                bc.Emit_Load(SymbolRef.Upvalue(WellKnownSymbols.ENV, 0));
                bc.Emit_Store(_env, 0, 0);
                bc.Emit_Pop();
            }

            if (_paramNames.Length > 0)
            {
                bc.Emit_Args(_paramNames);
            }

            _statement.Compile(bc);

            bc.PopSourceRef();
            bc.PushSourceRef(_end);

            bc.Emit_Ret(0);

            bc.LoopTracker.Loops.Pop();

            I.NumVal = bc.GetJumpPointForNextInstruction();
            meta.NumVal = bc.GetJumpPointForLastInstruction() - metaip;

            bc.PopSourceRef();

            return entryPoint;
        }

        public int Compile(ByteCode bc, Func<int> afterDecl, string friendlyName)
        {
            using (bc.EnterSource(_begin))
            {
                var symbs = _closure.ToArray();

                _closureInstruction = bc.Emit_Closure(symbs, bc.GetJumpPointForNextInstruction());
                int ops = afterDecl();

                _closureInstruction.NumVal += 2 + ops;
            }

            return this.CompileBody(bc, friendlyName);
        }


        public override void Compile(ByteCode bc)
        {
            this.Compile(bc, () => 0, null);
        }
    }
}