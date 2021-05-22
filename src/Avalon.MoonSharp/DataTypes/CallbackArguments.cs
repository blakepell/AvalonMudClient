using System.Collections.Generic;
using MoonSharp.Interpreter.DataStructs;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// This class is a container for arguments received by a CallbackFunction
    /// </summary>
    public class CallbackArguments
    {
        private IList<DynValue> _args;
        private bool _lastIsTuple;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackArguments" /> class.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="isMethodCall">if set to <c>true</c> [is method call].</param>
        public CallbackArguments(IList<DynValue> args, bool isMethodCall)
        {
            _args = args;

            if (_args.Count > 0)
            {
                var last = _args[_args.Count - 1];

                if (last.Type == DataType.Tuple)
                {
                    this.Count = last.Tuple.Length - 1 + _args.Count;
                    _lastIsTuple = true;
                }
                else if (last.Type == DataType.Void)
                {
                    this.Count = _args.Count - 1;
                }
                else
                {
                    this.Count = _args.Count;
                }
            }
            else
            {
                this.Count = 0;
            }

            this.IsMethodCall = isMethodCall;
        }

        /// <summary>
        /// Gets the count of arguments
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a method call.
        /// </summary>
        public bool IsMethodCall { get; }


        /// <summary>
        /// Gets the <see cref="DynValue"/> at the specified index, or Void if not found 
        /// </summary>
        public DynValue this[int index] => this.RawGet(index, true) ?? DynValue.Void;

        /// <summary>
        /// Gets the <see cref="DynValue" /> at the specified index, or null.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="translateVoids">if set to <c>true</c> all voids are translated to nils.</param>
        public DynValue RawGet(int index, bool translateVoids)
        {
            DynValue v;

            if (index >= this.Count)
            {
                return null;
            }

            if (!_lastIsTuple || index < _args.Count - 1)
            {
                v = _args[index];
            }
            else
            {
                v = _args[_args.Count - 1].Tuple[index - (_args.Count - 1)];
            }

            if (v.Type == DataType.Tuple)
            {
                if (v.Tuple.Length > 0)
                {
                    v = v.Tuple[0];
                }
                else
                {
                    v = DynValue.Nil;
                }
            }

            if (translateVoids && v.Type == DataType.Void)
            {
                v = DynValue.Nil;
            }

            return v;
        }


        /// <summary>
        /// Converts the arguments to an array
        /// </summary>
        /// <param name="skip">The number of elements to skip (default= 0).</param>
        public DynValue[] GetArray(int skip = 0)
        {
            if (skip >= this.Count)
            {
                return System.Array.Empty<DynValue>();
            }

            var vals = new DynValue[this.Count - skip];

            for (int i = skip; i < this.Count; i++)
            {
                vals[i - skip] = this[i];
            }

            return vals;
        }

        /// <summary>
        /// Gets the specified argument as as an argument of the specified type. If not possible,
        /// an exception is raised.
        /// </summary>
        /// <param name="argNum">The argument number.</param>
        /// <param name="funcName">Name of the function.</param>
        /// <param name="type">The type desired.</param>
        /// <param name="allowNil">if set to <c>true</c> nil values are allowed.</param>
        public DynValue AsType(int argNum, string funcName, DataType type, bool allowNil = false)
        {
            return this[argNum].CheckType(funcName, type, argNum,
                allowNil
                    ? TypeValidationFlags.AllowNil | TypeValidationFlags.AutoConvert
                    : TypeValidationFlags.AutoConvert);
        }

        /// <summary>
        /// Gets the specified argument as as an argument of the specified user data type. If not possible,
        /// an exception is raised.
        /// </summary>
        /// <typeparam name="T">The desired userdata type</typeparam>
        /// <param name="argNum">The argument number.</param>
        /// <param name="funcName">Name of the function.</param>
        /// <param name="allowNil">if set to <c>true</c> nil values are allowed.</param>
        public T AsUserData<T>(int argNum, string funcName, bool allowNil = false)
        {
            return this[argNum].CheckUserDataType<T>(funcName, argNum,
                allowNil ? TypeValidationFlags.AllowNil : TypeValidationFlags.None);
        }

        /// <summary>
        /// Gets the specified argument as an integer
        /// </summary>
        /// <param name="argNum">The argument number.</param>
        /// <param name="funcName">Name of the function.</param>
        public int AsInt(int argNum, string funcName)
        {
            var v = this.AsType(argNum, funcName, DataType.Number);
            double d = v.Number;
            return (int) d;
        }

        /// <summary>
        /// Gets the specified argument as a long integer
        /// </summary>
        /// <param name="argNum">The argument number.</param>
        /// <param name="funcName">Name of the function.</param>
        public long AsLong(int argNum, string funcName)
        {
            var v = this.AsType(argNum, funcName, DataType.Number);
            double d = v.Number;
            return (long) d;
        }


        /// <summary>
        /// Gets the specified argument as a string, calling the __tostring metamethod if needed, in a NON
        /// yield-compatible way.
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="argNum">The argument number.</param>
        /// <param name="funcName">Name of the function.</param>
        /// <exception cref="ScriptRuntimeException">'tostring' must return a string to '{0}'</exception>
        public string AsStringUsingMeta(ScriptExecutionContext executionContext, int argNum, string funcName)
        {
            if ((this[argNum].Type == DataType.Table) && (this[argNum].Table.MetaTable != null) &&
                (this[argNum].Table.MetaTable.RawGet("__tostring") != null))
            {
                var v = executionContext.GetScript()
                    .Call(this[argNum].Table.MetaTable.RawGet("__tostring"), this[argNum]);

                if (v.Type != DataType.String)
                {
                    throw new ScriptRuntimeException("'tostring' must return a string to '{0}'", funcName);
                }

                return v.ToPrintString();
            }

            return (this[argNum].ToPrintString());
        }


        /// <summary>
        /// Returns a copy of CallbackArguments where the first ("self") argument is skipped if this was a method call,
        /// otherwise returns itself.
        /// </summary>
        public CallbackArguments SkipMethodCall()
        {
            if (this.IsMethodCall)
            {
                var slice = new Slice<DynValue>(_args, 1, _args.Count - 1, false);
                return new CallbackArguments(slice, false);
            }

            return this;
        }
    }
}