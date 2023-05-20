using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonSharp.Interpreter.Execution
{
    /// <summary>
    /// The scope of a closure (container of upvalues)
    /// </summary>
    internal class ClosureContext : List<DynValue>
    {
        internal ClosureContext(SymbolRef[] symbols, IEnumerable<DynValue> values)
        {
            this.Symbols = symbols.Select(s => s._name).ToArray();
            this.AddRange(values);
        }

        internal ClosureContext()
        {
            this.Symbols = Array.Empty<string>();
        }

        /// <summary>
        /// Gets the symbols.
        /// </summary>
        public string[] Symbols { get; }
    }
}