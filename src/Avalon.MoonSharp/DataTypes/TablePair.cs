namespace MoonSharp.Interpreter
{
    /// <summary>
    /// A class representing a key/value pair for Table use
    /// </summary>
    public struct TablePair
    {
        private DynValue _value;

        /// <summary>
        /// Gets the key.
        /// </summary>
        public DynValue Key { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public DynValue Value
        {
            get => _value;
            set
            {
                if (this.Key.IsNotNil())
                {
                    this.Value = value;
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TablePair"/> struct.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        public TablePair(DynValue key, DynValue val)
        {
            this.Key = key;
            _value = val;
        }

        /// <summary>
        /// Gets the nil pair
        /// </summary>
        public static TablePair Nil { get; } = new TablePair(DynValue.Nil, DynValue.Nil);
    }
}