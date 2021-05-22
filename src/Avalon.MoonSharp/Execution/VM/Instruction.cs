using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
    internal class Instruction
    {
        internal string Name;
        internal int NumVal;
        internal int NumVal2;
        internal OpCode OpCode;
        internal SourceRef SourceCodeRef;
        internal SymbolRef Symbol;
        internal SymbolRef[] SymbolList;
        internal DynValue Value;

        internal Instruction(SourceRef sourceRef)
        {
            SourceCodeRef = sourceRef;
        }
    }
}