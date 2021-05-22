using MoonSharp.Interpreter.Interop.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// A class representing a value in a Lua/MoonSharp script.
    /// </summary>
    public sealed class DynValue
    {
        private static int _refIDCounter;
        private int _hashCode = -1;
        private object _object;

        static DynValue()
        {
            Nil = new DynValue {Type = DataType.Nil}.AsReadOnly();
            Void = new DynValue {Type = DataType.Void}.AsReadOnly();
            True = NewBoolean(true).AsReadOnly();
            False = NewBoolean(false).AsReadOnly();
        }


        /// <summary>
        /// Gets a unique reference identifier. This is guaranteed to be unique only for dynvalues created in a single thread as it's not thread-safe.
        /// </summary>
        public int ReferenceID { get; } = ++_refIDCounter;

        /// <summary>
        /// Gets the type of the value.
        /// </summary>
        public DataType Type { get; private set; }

        /// <summary>
        /// Gets the function (valid only if the <see cref="Type"/> is <see cref="DataType.Function"/>)
        /// </summary>
        public Closure Function => _object as Closure;

        /// <summary>
        /// Gets the numeric value (valid only if the <see cref="Type"/> is <see cref="DataType.Number"/>)
        /// </summary>
        public double Number { get; private set; }

        /// <summary>
        /// Gets the values in the tuple (valid only if the <see cref="Type"/> is Tuple).
        /// This field is currently also used to hold arguments in values whose <see cref="Type"/> is <see cref="DataType.TailCallRequest"/>.
        /// </summary>
        public DynValue[] Tuple => _object as DynValue[];

        /// <summary>
        /// Gets the coroutine handle. (valid only if the <see cref="Type"/> is Thread).
        /// </summary>
        public Coroutine Coroutine => _object as Coroutine;

        /// <summary>
        /// Gets the table (valid only if the <see cref="Type"/> is <see cref="DataType.Table"/>)
        /// </summary>
        public Table Table => _object as Table;

        /// <summary>
        /// Gets the boolean value (valid only if the <see cref="Type"/> is <see cref="DataType.Boolean"/>)
        /// </summary>
        public bool Boolean => this.Number != 0;

        /// <summary>
        /// Gets the string value (valid only if the <see cref="Type"/> is <see cref="DataType.String"/>)
        /// </summary>
        public string String => _object as string;

        /// <summary>
        /// Gets the CLR callback (valid only if the <see cref="Type"/> is <see cref="DataType.ClrFunction"/>)
        /// </summary>
        public CallbackFunction Callback => _object as CallbackFunction;

        /// <summary>
        /// Gets the tail call data.
        /// </summary>
        public TailCallData TailCallData => _object as TailCallData;

        /// <summary>
        /// Gets the yield request data.
        /// </summary>
        public YieldRequest YieldRequest => _object as YieldRequest;

        /// <summary>
        /// Gets the tail call data.
        /// </summary>
        public UserData UserData => _object as UserData;

        /// <summary>
        /// Returns true if this instance is write protected.
        /// </summary>
        public bool ReadOnly { get; private set; }


        /// <summary>
        /// A pre-initialized, readonly instance, equaling Void
        /// </summary>
        public static DynValue Void { get; }

        /// <summary>
        /// A pre-initialized, readonly instance, equaling Nil
        /// </summary>
        public static DynValue Nil { get; }

        /// <summary>
        /// A pre-initialized, readonly instance, equaling True
        /// </summary>
        public static DynValue True { get; }

        /// <summary>
        /// A pre-initialized, readonly instance, equaling False
        /// </summary>
        public static DynValue False { get; }


        /// <summary>
        /// Creates a new writable value initialized to Nil.
        /// </summary>
        public static DynValue NewNil()
        {
            return new DynValue();
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified boolean.
        /// </summary>
        public static DynValue NewBoolean(bool v)
        {
            return new DynValue
            {
                Number = v ? 1 : 0,
                Type = DataType.Boolean
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified number.
        /// </summary>
        public static DynValue NewNumber(double num)
        {
            return new DynValue
            {
                Number = num,
                Type = DataType.Number,
                _hashCode = -1
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified string.
        /// </summary>
        public static DynValue NewString(string str)
        {
            return new DynValue
            {
                _object = str,
                Type = DataType.String
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified StringBuilder.
        /// </summary>
        public static DynValue NewString(StringBuilder sb)
        {
            return new DynValue
            {
                _object = sb.ToString(),
                Type = DataType.String
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified ReadOnlySpan&lt;char&gt;.
        /// </summary>
        /// <param name="span"></param>
        public static DynValue NewString(ReadOnlySpan<char> span)
        {
            return new()
            {
                _object = new string(span),
                Type = DataType.String
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified char[].
        /// </summary>
        /// <param name="buf"></param>
        public static DynValue NewString(char[] buf)
        {
            return new()
            {
                _object = new string(buf, 0, buf.Length),
                Type = DataType.String
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified string using String.Format like syntax
        /// </summary>
        public static DynValue NewString(string format, params object[] args)
        {
            return new DynValue
            {
                _object = string.Format(format, args),
                Type = DataType.String
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified coroutine.
        /// Internal use only, for external use, see Script.CoroutineCreate
        /// </summary>
        /// <param name="coroutine">The coroutine object.</param>
        public static DynValue NewCoroutine(Coroutine coroutine)
        {
            return new DynValue
            {
                _object = coroutine,
                Type = DataType.Thread
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified closure (function).
        /// </summary>
        public static DynValue NewClosure(Closure function)
        {
            return new DynValue
            {
                _object = function,
                Type = DataType.Function
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified CLR callback.
        /// </summary>
        public static DynValue NewCallback(Func<ScriptExecutionContext, CallbackArguments, DynValue> callBack,
            string name = null)
        {
            return new DynValue
            {
                _object = new CallbackFunction(callBack, name),
                Type = DataType.ClrFunction
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified CLR callback.
        /// See also CallbackFunction.FromDelegate and CallbackFunction.FromMethodInfo factory methods.
        /// </summary>
        public static DynValue NewCallback(CallbackFunction function)
        {
            return new DynValue
            {
                _object = function,
                Type = DataType.ClrFunction
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to the specified table.
        /// </summary>
        public static DynValue NewTable(Table table)
        {
            return new DynValue
            {
                _object = table,
                Type = DataType.Table
            };
        }

        /// <summary>
        /// Creates a new writable value initialized to an empty prime table (a 
        /// prime table is a table made only of numbers, strings, booleans and other
        /// prime tables).
        /// </summary>
        public static DynValue NewPrimeTable()
        {
            return NewTable(new Table(null));
        }

        /// <summary>
        /// Creates a new writable value initialized to an empty table.
        /// </summary>
        public static DynValue NewTable(Script script)
        {
            return NewTable(new Table(script));
        }

        /// <summary>
        /// Creates a new writable value initialized to with array contents.
        /// </summary>
        public static DynValue NewTable(Script script, params DynValue[] arrayValues)
        {
            return NewTable(new Table(script, arrayValues));
        }

        /// <summary>
        /// Creates a new request for a tail call. This is the preferred way to execute Lua/MoonSharp code from a callback,
        /// although it's not always possible to use it. When a function (callback or script closure) returns a
        /// TailCallRequest, the bytecode processor immediately executes the function contained in the request.
        /// By executing script in this way, a callback function ensures it's not on the stack anymore and thus a number
        /// of functionality (state savings, coroutines, etc) keeps working at full power.
        /// </summary>
        /// <param name="tailFn">The function to be called.</param>
        /// <param name="args">The arguments.</param>
        public static DynValue NewTailCallReq(DynValue tailFn, params DynValue[] args)
        {
            return new DynValue
            {
                _object = new TailCallData
                {
                    Args = args,
                    Function = tailFn
                },
                Type = DataType.TailCallRequest
            };
        }

        /// <summary>
        /// Creates a new request for a tail call. This is the preferred way to execute Lua/MoonSharp code from a callback,
        /// although it's not always possible to use it. When a function (callback or script closure) returns a
        /// TailCallRequest, the bytecode processor immediately executes the function contained in the request.
        /// By executing script in this way, a callback function ensures it's not on the stack anymore and thus a number
        /// of functionality (state savings, coroutines, etc) keeps working at full power.
        /// </summary>
        /// <param name="tailCallData">The data for the tail call.</param>
        public static DynValue NewTailCallReq(TailCallData tailCallData)
        {
            return new DynValue
            {
                _object = tailCallData,
                Type = DataType.TailCallRequest
            };
        }


        /// <summary>
        /// Creates a new request for a yield of the current coroutine.
        /// </summary>
        /// <param name="args">The yield argumenst.</param>
        public static DynValue NewYieldReq(DynValue[] args)
        {
            return new DynValue
            {
                _object = new YieldRequest {ReturnValues = args},
                Type = DataType.YieldRequest
            };
        }

        /// <summary>
        /// Creates a new request for a yield of the current coroutine.
        /// </summary>
        internal static DynValue NewForcedYieldReq()
        {
            return new DynValue
            {
                _object = new YieldRequest {Forced = true},
                Type = DataType.YieldRequest
            };
        }

        /// <summary>
        /// Creates a new tuple initialized to the specified values.
        /// </summary>
        public static DynValue NewTuple(params DynValue[] values)
        {
            if (values.Length == 0)
            {
                return NewNil();
            }

            if (values.Length == 1)
            {
                return values[0];
            }

            return new DynValue
            {
                _object = values,
                Type = DataType.Tuple
            };
        }

        /// <summary>
        /// Creates a new tuple initialized to the specified values - which can be potentially other tuples
        /// </summary>
        public static DynValue NewTupleNested(params DynValue[] values)
        {
            if (!values.Any(v => v.Type == DataType.Tuple))
            {
                return NewTuple(values);
            }

            if (values.Length == 1)
            {
                return values[0];
            }

            var vals = new List<DynValue>();

            foreach (var v in values)
            {
                if (v.Type == DataType.Tuple)
                {
                    vals.AddRange(v.Tuple);
                }
                else
                {
                    vals.Add(v);
                }
            }

            return new DynValue
            {
                _object = vals.ToArray(),
                Type = DataType.Tuple
            };
        }


        /// <summary>
        /// Creates a new userdata value
        /// </summary>
        public static DynValue NewUserData(UserData userData)
        {
            return new DynValue
            {
                _object = userData,
                Type = DataType.UserData
            };
        }

        /// <summary>
        /// Returns this value as readonly - eventually cloning it in the process if it isn't readonly to start with.
        /// </summary>
        public DynValue AsReadOnly()
        {
            if (this.ReadOnly)
            {
                return this;
            }

            return this.Clone(true);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        public DynValue Clone()
        {
            return this.Clone(this.ReadOnly);
        }

        /// <summary>
        /// Clones this instance, overriding the "readonly" status.
        /// </summary>
        /// <param name="readOnly">if set to <c>true</c> the new instance is set as readonly, or writeable otherwise.</param>
        public DynValue Clone(bool readOnly)
        {
            var v = new DynValue();
            v._object = _object;
            v.Number = this.Number;
            v._hashCode = _hashCode;
            v.Type = this.Type;
            v.ReadOnly = readOnly;
            return v;
        }

        /// <summary>
        /// Clones this instance, returning a writable copy.
        /// </summary>
        /// <exception cref="System.ArgumentException">Can't clone Symbol values</exception>
        public DynValue CloneAsWritable()
        {
            return this.Clone(false);
        }


        /// <summary>
        /// Returns a string which is what it's expected to be output by the print function applied to this value.
        /// </summary>
        public string ToPrintString()
        {
            if (_object is RefIdObject refid)
            {
                string typeString = this.Type.ToLuaTypeString();

                if (refid is UserData ud)
                {
                    string str = ud.Descriptor.AsString(ud.Object);
                    if (str != null)
                    {
                        return str;
                    }
                }

                return refid.FormatTypeString(typeString);
            }

            switch (this.Type)
            {
                case DataType.String:
                    return this.String;
                case DataType.Tuple:
                    return string.Join("\t", this.Tuple.Select(t => t.ToPrintString()).ToArray());
                case DataType.TailCallRequest:
                    return "(TailCallRequest -- INTERNAL!)";
                case DataType.YieldRequest:
                    return "(YieldRequest -- INTERNAL!)";
                default:
                    return this.ToString();
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            switch (this.Type)
            {
                case DataType.Void:
                    return "void";
                case DataType.Nil:
                    return "nil";
                case DataType.Boolean:
                    return this.Boolean.ToString().ToLower();
                case DataType.Number:
                    return this.Number.ToString(CultureInfo.InvariantCulture);
                case DataType.String:
                    return "\"" + this.String + "\"";
                case DataType.Function:
                    return $"(Function {this.Function.EntryPointByteCodeLocation:X8})";
                case DataType.ClrFunction:
                    return "(Function CLR)";
                case DataType.Table:
                    return "(Table)";
                case DataType.Tuple:
                    return string.Join(", ", this.Tuple.Select(t => t.ToString()).ToArray());
                case DataType.TailCallRequest:
                    return "Tail:(" + string.Join(", ", this.Tuple.Select(t => t.ToString()).ToArray()) + ")";
                case DataType.UserData:
                    return "(UserData)";
                case DataType.Thread:
                    return $"(Coroutine {this.Coroutine.ReferenceID:X8})";
                default:
                    return "(???)";
            }
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (_hashCode != -1)
            {
                return _hashCode;
            }

            int baseValue = ((int) (this.Type)) << 27;

            switch (this.Type)
            {
                case DataType.Void:
                case DataType.Nil:
                    _hashCode = 0;
                    break;
                case DataType.Boolean:
                    _hashCode = this.Boolean ? 1 : 2;
                    break;
                case DataType.Number:
                    _hashCode = baseValue ^ this.Number.GetHashCode();
                    break;
                case DataType.String:
                    _hashCode = baseValue ^ this.String.GetHashCode();
                    break;
                case DataType.Function:
                    _hashCode = baseValue ^ this.Function.GetHashCode();
                    break;
                case DataType.ClrFunction:
                    _hashCode = baseValue ^ this.Callback.GetHashCode();
                    break;
                case DataType.Table:
                    _hashCode = baseValue ^ this.Table.GetHashCode();
                    break;
                case DataType.Tuple:
                case DataType.TailCallRequest:
                    _hashCode = baseValue ^ this.Tuple.GetHashCode();
                    break;
                case DataType.UserData:
                case DataType.Thread:
                default:
                    _hashCode = 999;
                    break;
            }

            return _hashCode;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as DynValue;

            if (other == null)
            {
                return false;
            }

            if ((other.Type == DataType.Nil && this.Type == DataType.Void)
                || (other.Type == DataType.Void && this.Type == DataType.Nil))
            {
                return true;
            }

            if (other.Type != this.Type)
            {
                return false;
            }


            switch (this.Type)
            {
                case DataType.Void:
                case DataType.Nil:
                    return true;
                case DataType.Boolean:
                    return this.Boolean == other.Boolean;
                case DataType.Number:
                    return this.Number == other.Number;
                case DataType.String:
                    return this.String == other.String;
                case DataType.Function:
                    return this.Function == other.Function;
                case DataType.ClrFunction:
                    return this.Callback == other.Callback;
                case DataType.Table:
                    return this.Table == other.Table;
                case DataType.Tuple:
                case DataType.TailCallRequest:
                    return this.Tuple == other.Tuple;
                case DataType.Thread:
                    return this.Coroutine == other.Coroutine;
                case DataType.UserData:
                {
                    var ud1 = this.UserData;
                    var ud2 = other.UserData;

                    if (ud1 == null || ud2 == null)
                    {
                        return false;
                    }

                    if (ud1.Descriptor != ud2.Descriptor)
                    {
                        return false;
                    }

                    if (ud1.Object == null && ud2.Object == null)
                    {
                        return true;
                    }

                    if (ud1.Object != null && ud2.Object != null)
                    {
                        return ud1.Object.Equals(ud2.Object);
                    }

                    return false;
                }
                default:
                    return ReferenceEquals(this, other);
            }
        }


        /// <summary>
        /// Casts this DynValue to string, using coercion if the type is number.
        /// </summary>
        /// <returns>The string representation, or null if not number, not string.</returns>
        public string CastToString()
        {
            var rv = this.ToScalar();
            if (rv.Type == DataType.Number)
            {
                return rv.Number.ToString(CultureInfo.InvariantCulture);
            }

            if (rv.Type == DataType.String)
            {
                return rv.String;
            }

            return null;
        }

        /// <summary>
        /// Casts this DynValue to a double, using coercion if the type is string.
        /// </summary>
        /// <returns>The string representation, or null if not number, not string or non-convertible-string.</returns>
        public double? CastToNumber()
        {
            var rv = this.ToScalar();
            if (rv.Type == DataType.Number)
            {
                return rv.Number;
            }

            if (rv.Type == DataType.String)
            {
                if (double.TryParse(rv.String, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                {
                    return num;
                }
            }

            return null;
        }


        /// <summary>
        /// Casts this DynValue to a bool
        /// </summary>
        /// <returns>False if value is false or nil, true otherwise.</returns>
        public bool CastToBool()
        {
            var rv = this.ToScalar();
            if (rv.Type == DataType.Boolean)
            {
                return rv.Boolean;
            }

            return (rv.Type != DataType.Nil && rv.Type != DataType.Void);
        }

        /// <summary>
        /// Returns this DynValue as an instance of <see cref="IScriptPrivateResource"/>, if possible,
        /// null otherwise
        /// </summary>
        /// <returns>False if value is false or nil, true otherwise.</returns>
        public IScriptPrivateResource GetAsPrivateResource()
        {
            return _object as IScriptPrivateResource;
        }


        /// <summary>
        /// Converts a tuple to a scalar value. If it's already a scalar value, this function returns "this".
        /// </summary>
        public DynValue ToScalar()
        {
            if (this.Type != DataType.Tuple)
            {
                return this;
            }

            if (this.Tuple.Length == 0)
            {
                return Void;
            }

            return this.Tuple[0].ToScalar();
        }

        /// <summary>
        /// Performs an assignment, overwriting the value with the specified one.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="ScriptRuntimeException">If the value is readonly.</exception>
        public void Assign(DynValue value)
        {
            if (this.ReadOnly)
            {
                throw new ScriptRuntimeException("Assigning on r-value");
            }

            this.Number = value.Number;
            _object = value._object;
            this.Type = value.Type;
            _hashCode = -1;
        }


        /// <summary>
        /// Gets the length of a string or table value.
        /// </summary>
        /// <exception cref="ScriptRuntimeException">Value is not a table or string.</exception>
        public DynValue GetLength()
        {
            if (this.Type == DataType.Table)
            {
                return NewNumber(this.Table.Length);
            }

            if (this.Type == DataType.String)
            {
                return NewNumber(this.String.Length);
            }

            throw new ScriptRuntimeException("Can't get length of type {0}", this.Type);
        }

        /// <summary>
        /// Determines whether this instance is nil or void
        /// </summary>
        public bool IsNil()
        {
            return this.Type == DataType.Nil || this.Type == DataType.Void;
        }

        /// <summary>
        /// Determines whether this instance is not nil or void
        /// </summary>
        public bool IsNotNil()
        {
            return this.Type != DataType.Nil && this.Type != DataType.Void;
        }

        /// <summary>
        /// Determines whether this instance is void
        /// </summary>
        public bool IsVoid()
        {
            return this.Type == DataType.Void;
        }

        /// <summary>
        /// Determines whether this instance is not void
        /// </summary>
        public bool IsNotVoid()
        {
            return this.Type != DataType.Void;
        }

        /// <summary>
        /// Determines whether is nil, void or NaN (and thus unsuitable for using as a table key).
        /// </summary>
        public bool IsNilOrNan()
        {
            return (this.Type == DataType.Nil) || (this.Type == DataType.Void) ||
                   (this.Type == DataType.Number && double.IsNaN(this.Number));
        }

        /// <summary>
        /// Changes the numeric value of a number DynValue.
        /// </summary>
        internal void AssignNumber(double num)
        {
            if (this.ReadOnly)
            {
                throw new InternalErrorException(null, "Writing on r-value");
            }

            if (this.Type != DataType.Number)
            {
                throw new InternalErrorException("Can't assign number to type {0}", this.Type);
            }

            this.Number = num;
        }

        /// <summary>
        /// Creates a new DynValue from a CLR object
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="obj">The object.</param>
        public static DynValue FromObject(Script script, object obj)
        {
            return ClrToScriptConversions.ObjectToDynValue(script, obj);
        }

        /// <summary>
        /// Converts this MoonSharp DynValue to a CLR object.
        /// </summary>
        public object ToObject()
        {
            return ScriptToClrConversions.DynValueToObject(this);
        }

        /// <summary>
        /// Converts this MoonSharp DynValue to a CLR object of the specified type.
        /// </summary>
        public object ToObject(Type desiredType)
        {
            //Contract.Requires(desiredType != null);
            return ScriptToClrConversions.DynValueToObjectOfType(this, desiredType, null, false);
        }

        /// <summary>
        /// Converts this MoonSharp DynValue to a CLR object of the specified type.
        /// </summary>
        public T ToObject<T>()
        {
            var obj = (T) this.ToObject(typeof(T));
            return obj ?? default;
        }

        /// <summary>
        /// Converts this MoonSharp DynValue to a CLR object, marked as dynamic
        /// </summary>
        public dynamic ToDynamic()
        {
            return ScriptToClrConversions.DynValueToObject(this);
        }

        /// <summary>
        /// Checks the type of this value corresponds to the desired type. A property ScriptRuntimeException is thrown
        /// if the value is not of the specified type or - considering the TypeValidationFlags - is not convertible
        /// to the specified type.
        /// </summary>
        /// <param name="funcName">Name of the function requesting the value, for error message purposes.</param>
        /// <param name="desiredType">The desired data type.</param>
        /// <param name="argNum">The argument number, for error message purposes.</param>
        /// <param name="flags">The TypeValidationFlags.</param>
        /// <exception cref="ScriptRuntimeException">Thrown
        /// if the value is not of the specified type or - considering the TypeValidationFlags - is not convertible
        /// to the specified type.</exception>
        public DynValue CheckType(string funcName, DataType desiredType, int argNum = -1,
            TypeValidationFlags flags = TypeValidationFlags.Default)
        {
            if (this.Type == desiredType)
            {
                return this;
            }

            bool allowNil = ((int) (flags & TypeValidationFlags.AllowNil) != 0);

            if (allowNil && this.IsNil())
            {
                return this;
            }

            bool autoConvert = ((int) (flags & TypeValidationFlags.AutoConvert) != 0);

            if (autoConvert)
            {
                if (desiredType == DataType.Boolean)
                {
                    return NewBoolean(this.CastToBool());
                }

                if (desiredType == DataType.Number)
                {
                    var v = this.CastToNumber();
                    if (v.HasValue)
                    {
                        return NewNumber(v.Value);
                    }
                }

                if (desiredType == DataType.String)
                {
                    string v = this.CastToString();
                    if (v != null)
                    {
                        return NewString(v);
                    }
                }
            }

            if (this.IsVoid())
            {
                throw ScriptRuntimeException.BadArgumentNoValue(argNum, funcName, desiredType);
            }

            throw ScriptRuntimeException.BadArgument(argNum, funcName, desiredType, this.Type, allowNil);
        }

        /// <summary>
        /// Checks if the type is a specific userdata type, and returns it or throws.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="funcName">Name of the function.</param>
        /// <param name="argNum">The argument number.</param>
        /// <param name="flags">The flags.</param>
        public T CheckUserDataType<T>(string funcName, int argNum = -1,
            TypeValidationFlags flags = TypeValidationFlags.Default)
        {
            var v = this.CheckType(funcName, DataType.UserData, argNum, flags);
            bool allowNil = ((int) (flags & TypeValidationFlags.AllowNil) != 0);

            if (v.IsNil())
            {
                return default;
            }

            var o = v.UserData.Object;
            if (o is T t)
            {
                return t;
            }

            throw ScriptRuntimeException.BadArgumentUserData(argNum, funcName, typeof(T), o, allowNil);
        }
    }
}