using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Avalon.Lua
{
    /// <summary>
	/// Shared global variables for the Lua environment.
	/// </summary>
	public class LuaGlobalVariables : IUserDataType
	{
		/// <summary>
		/// Global variables dictionary.
		/// </summary>
		Dictionary<string, DynValue> _values = new Dictionary<string, DynValue>();

		/// <summary>
		/// Lock to ensure thread safety.
		/// </summary>
		private readonly object _lock = new object();

		public object this[string property]
		{
			get
            {
                lock (_lock)
                {
                    return _values[property].ToObject();
                }
            }
			set
            {
                lock (_lock)
                {
                    _values[property] = DynValue.FromObject(null, value);
                }
            }
		}

		public DynValue Index(ExecutionControlToken ecToken, Script script, DynValue index, bool isDirectIndexing)
		{
            if (index.Type != DataType.String)
            {
                throw new ScriptRuntimeException("string property was expected");
            }

            lock (_lock)
            {
                if (_values.ContainsKey(index.String))
                {
                    return _values[index.String].Clone();
                }

                return DynValue.Nil;
            }
		}

		public bool SetIndex(ExecutionControlToken ecToken, Script script, DynValue index, DynValue value, bool isDirectIndexing)
		{
            if (index.Type != DataType.String)
            {
                throw new ScriptRuntimeException("string property was expected");
            }

            lock (_lock)
			{
				switch (value.Type)
				{
					case DataType.Void:
					case DataType.Nil:
						_values.Remove(index.String);
						return true;
					case DataType.UserData:
						// HERE YOU CAN CHOOSE A DIFFERENT POLICY.. AND TRY TO SHARE IF NEEDED. DANGEROUS, THOUGH.
						throw new ScriptRuntimeException("Cannot share a value of type {0}", value.Type.ToErrorTypeString());
					case DataType.ClrFunction:
						// HERE YOU CAN CHOOSE A DIFFERENT POLICY.. AND TRY TO SHARE IF NEEDED. DANGEROUS, THOUGH.
						throw new ScriptRuntimeException("Cannot share a value of type {0}", value.Type.ToErrorTypeString());
					case DataType.Boolean:
					case DataType.Number:
					case DataType.String:
						_values[index.String] = value.Clone();
						return true;
					case DataType.Table:
						_values[index.String] = value.Clone();
						return true;
					default:
						throw new ScriptRuntimeException("Cannot share a value of type {0}", value.Type.ToErrorTypeString());
				}
			}
		}

		public DynValue MetaIndex(ExecutionControlToken ecToken, Script script, string metaname)
		{
			return null;
		}
    }
}