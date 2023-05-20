namespace MoonSharp.Interpreter
{
    /// <summary>
    /// This class stores a possible l-value (that is a potential target of an assignment)
    /// </summary>
    public class SymbolRef
    {
        internal SymbolRef _env;
        internal int _index;
        internal string _name;

        // Fields are internal - direct access by the executor was a 10% improvement at profiling here!
        internal SymbolRefType _type;

        /// <summary>
        /// Gets the type of this symbol reference
        /// </summary>
        public SymbolRefType Type => _type;

        /// <summary>
        /// Gets the index of this symbol in its scope context
        /// </summary>
        public int Index => _index;

        /// <summary>
        /// Gets the name of this symbol
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets the environment this symbol refers to (for global symbols only)
        /// </summary>
        public SymbolRef Environment => _env;

        /// <summary>
        /// Gets the default _ENV.
        /// </summary>
        public static SymbolRef DefaultEnv { get; } = new SymbolRef {_type = SymbolRefType.DefaultEnv};

        /// <summary>
        /// Creates a new symbol reference pointing to a global var
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="envSymbol">The _ENV symbol.</param>
        public static SymbolRef Global(string name, SymbolRef envSymbol)
        {
            return new SymbolRef {_index = -1, _type = SymbolRefType.Global, _env = envSymbol, _name = name};
        }

        /// <summary>
        /// Creates a new symbol reference pointing to a local var
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="index">The index of the var in local scope.</param>
        internal static SymbolRef Local(string name, int index)
        {
            //Debug.Assert(index >= 0, "Symbol Index < 0");
            return new SymbolRef {_index = index, _type = SymbolRefType.Local, _name = name};
        }

        /// <summary>
        /// Creates a new symbol reference pointing to an upvalue var
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="index">The index of the var in closure scope.</param>
        internal static SymbolRef Upvalue(string name, int index)
        {
            //Debug.Assert(index >= 0, "Symbol Index < 0");
            return new SymbolRef {_index = index, _type = SymbolRefType.Upvalue, _name = name};
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (_type == SymbolRefType.DefaultEnv)
            {
                return "(default _ENV)";
            }

            if (_type == SymbolRefType.Global)
            {
                return string.Format("{2} : {0} / {1}", _type, _env, _name);
            }

            return string.Format("{2} : {0}[{1}]", _type, _index.ToString(), _name);
        }
    }
}