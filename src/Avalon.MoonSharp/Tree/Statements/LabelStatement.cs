using System.Collections.Generic;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class LabelStatement : Statement
    {
        private List<GotoStatement> _gotos = new List<GotoStatement>();
        private RuntimeScopeBlock _stackFrame;

        public LabelStatement(ScriptLoadingContext lcontext)
            : base(lcontext)
        {
            CheckTokenType(lcontext, TokenType.DoubleColon);
            this.NameToken = CheckTokenType(lcontext, TokenType.Name);
            CheckTokenType(lcontext, TokenType.DoubleColon);

            this.SourceRef = this.NameToken.GetSourceRef();
            this.Label = this.NameToken.Text;

            lcontext.Scope.DefineLabel(this);
        }

        public string Label { get; }
        public int Address { get; private set; }
        public SourceRef SourceRef { get; }
        public Token NameToken { get; }

        internal int DefinedVarsCount { get; private set; }
        internal string LastDefinedVarName { get; private set; }

        internal void SetDefinedVars(int definedVarsCount, string lastDefinedVarsName)
        {
            this.DefinedVarsCount = definedVarsCount;
            this.LastDefinedVarName = lastDefinedVarsName;
        }

        internal void RegisterGoto(GotoStatement gotostat)
        {
            _gotos.Add(gotostat);
        }


        public override void Compile(ByteCode bc)
        {
            bc.Emit_Clean(_stackFrame);

            this.Address = bc.GetJumpPointForLastInstruction();

            foreach (var gotostat in _gotos)
            {
                gotostat.SetAddress(this.Address);
            }
        }

        internal void SetScope(RuntimeScopeBlock runtimeScopeBlock)
        {
            _stackFrame = runtimeScopeBlock;
        }
    }
}