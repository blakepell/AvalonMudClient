/*
 * Avalon Mud Client
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Avalon.Common.Scripting
{
    /// <summary>
    /// Shared global variables for the Lua environment.
    /// </summary>
    public class MoonSharpGlobalVariables : IUserDataType
    {
        /// <summary>
        /// Global variables dictionary.
        /// </summary>
        readonly Dictionary<string, DynValue> _values = new();

        /// <summary>
        /// Lock to ensure thread safety.
        /// </summary>
        private readonly object _lock = new();

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

        /// <summary>
        /// Returns a list of keys stored in this instance of the global variables.
        /// </summary>
        public List<string> Keys
        {
            get
            {
                lock (_lock)
                {
                    var list = new List<string>();

                    foreach (string key in _values.Keys)
                    {
                        list.Add(key);
                    }

                    return list;
                }
            }
        }

        /// <summary>
        /// The number of global variables that are currently stored.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _values.Keys.Count;
                }

            }
        }

        public DynValue Index(ExecutionControlToken ecToken, Script script, DynValue index, bool isDirectIndexing)
        {
            if (index.Type != DataType.String)
            {
                throw new ScriptRuntimeException("String property was expected.");
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
                throw new ScriptRuntimeException("String property was expected.");
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