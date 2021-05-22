using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class ForEachLoopStatement : Statement
    {
        private Statement _block;
        private IVariable[] _nameExps;
        private SymbolRef[] _names;
        private SourceRef _refFor, _refEnd;
        private Expression _rValues;
        private RuntimeScopeBlock _stackFrame;

        public ForEachLoopStatement(ScriptLoadingContext lcontext, Token firstNameToken, Token forToken) : base(lcontext)
        {
            //	for namelist in explist do block end | 		
            var names = new List<string>
            {
                firstNameToken.Text
            };

            while (lcontext.Lexer.Current.Type == TokenType.Comma)
            {
                lcontext.Lexer.Next();
                var name = CheckTokenType(lcontext, TokenType.Name);
                names.Add(name.Text);
            }

            CheckTokenType(lcontext, TokenType.In);

            _rValues = new ExprListExpression(Expression.ExprList(lcontext), lcontext);

            lcontext.Scope.PushBlock();

            _names = names
                .Select(n => lcontext.Scope.TryDefineLocal(n))
                .ToArray();

            _nameExps = _names
                .Select(s => new SymbolRefExpression(lcontext, s))
                .Cast<IVariable>()
                .ToArray();

            _refFor = forToken.GetSourceRef(CheckTokenType(lcontext, TokenType.Do));

            _block = new CompositeStatement(lcontext);

            _refEnd = CheckTokenType(lcontext, TokenType.End).GetSourceRef();

            _stackFrame = lcontext.Scope.PopBlock();

            lcontext.Source.Refs.Add(_refFor);
            lcontext.Source.Refs.Add(_refEnd);
        }


        public override void Compile(ByteCode bc)
        {
            //for var_1, ···, var_n in explist do block end
            bc.PushSourceRef(_refFor);

            var l = new Loop
            {
                Scope = _stackFrame
            };

            bc.LoopTracker.Loops.Push(l);

            // get iterator tuple
            _rValues.Compile(bc);

            // prepares iterator tuple - stack : iterator-tuple
            bc.Emit_IterPrep();

            // loop start - stack : iterator-tuple
            int start = bc.GetJumpPointForNextInstruction();
            bc.Emit_Enter(_stackFrame);

            // expand the tuple - stack : iterator-tuple, f, var, s
            bc.Emit_ExpTuple(0);

            // calls f(s, var) - stack : iterator-tuple, iteration result
            bc.Emit_Call(2, "for..in");

            // perform assignment of iteration result- stack : iterator-tuple, iteration result
            for (int i = 0; i < _nameExps.Length; i++)
            {
                _nameExps[i].CompileAssignment(bc, 0, i);
            }

            // pops  - stack : iterator-tuple
            bc.Emit_Pop();

            // repushes the main iterator var - stack : iterator-tuple, main-iterator-var
            bc.Emit_Load(_names[0]);

            // updates the iterator tuple - stack : iterator-tuple, main-iterator-var
            bc.Emit_IterUpd();

            // checks head, jumps if nil - stack : iterator-tuple, main-iterator-var
            var endjump = bc.Emit_Jump(OpCode.JNil, -1);

            // executes the stuff - stack : iterator-tuple
            _block.Compile(bc);

            bc.PopSourceRef();
            bc.PushSourceRef(_refEnd);

            // loop back again - stack : iterator-tuple
            bc.Emit_Leave(_stackFrame);
            bc.Emit_Jump(OpCode.Jump, start);

            bc.LoopTracker.Loops.Pop();

            int exitpointLoopExit = bc.GetJumpPointForNextInstruction();
            bc.Emit_Leave(_stackFrame);

            int exitpointBreaks = bc.GetJumpPointForNextInstruction();

            bc.Emit_Pop();

            foreach (var i in l.BreakJumps)
            {
                i.NumVal = exitpointBreaks;
            }

            endjump.NumVal = exitpointLoopExit;

            bc.PopSourceRef();
        }
    }
}