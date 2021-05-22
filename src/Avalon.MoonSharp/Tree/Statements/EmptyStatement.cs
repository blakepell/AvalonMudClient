using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
    internal class EmptyStatement : Statement
    {
        public EmptyStatement(ScriptLoadingContext lcontext) : base(lcontext)
        {
        }

        public override void Compile(ByteCode bc)
        {
        }
    }
}