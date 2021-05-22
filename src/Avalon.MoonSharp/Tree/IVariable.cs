using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree
{
    internal interface IVariable
    {
        void CompileAssignment(ByteCode bc, int stackofs, int tupleidx);
    }
}