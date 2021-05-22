using System.Collections.Generic;

namespace MoonSharp.Interpreter.Execution
{
    internal class RuntimeScopeFrame
    {
        public RuntimeScopeFrame()
        {
            this.DebugSymbols = new List<SymbolRef>();
        }

        public List<SymbolRef> DebugSymbols { get; }
        public int Count => this.DebugSymbols.Count;
        public int ToFirstBlock { get; internal set; }

        public override string ToString()
        {
            return $"ScopeFrame : #{this.Count.ToString()}";
        }
    }
}