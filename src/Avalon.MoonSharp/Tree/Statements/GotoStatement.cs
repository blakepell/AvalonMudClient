using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class GotoStatement : Statement
    {
        private Instruction _jump;
        private int _labelAddress = -1;

        public GotoStatement(ScriptLoadingContext lcontext) : base(lcontext)
        {
            this.GotoToken = CheckTokenType(lcontext, TokenType.Goto);
            var name = CheckTokenType(lcontext, TokenType.Name);

            this.SourceRef = this.GotoToken.GetSourceRef(name);

            this.Label = name.Text;

            lcontext.Scope.RegisterGoto(this);
        }

        internal SourceRef SourceRef { get; }
        internal Token GotoToken { get; }
        public string Label { get; }

        internal int DefinedVarsCount { get; private set; }
        internal string LastDefinedVarName { get; private set; }

        public override void Compile(ByteCode bc)
        {
            _jump = bc.Emit_Jump(OpCode.Jump, _labelAddress);
        }

        internal void SetDefinedVars(int definedVarsCount, string lastDefinedVarsName)
        {
            this.DefinedVarsCount = definedVarsCount;
            this.LastDefinedVarName = lastDefinedVarsName;
        }

        internal void SetAddress(int labelAddress)
        {
            _labelAddress = labelAddress;

            if (_jump != null)
            {
                _jump.NumVal = labelAddress;
            }
        }
    }
}