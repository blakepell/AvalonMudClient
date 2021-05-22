using System;
using System.Linq;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.Interop.BasicDescriptors;

namespace MoonSharp.Interpreter.Interop
{
    /// <summary>
    /// Standard descriptor for Enum values
    /// </summary>
    public class StandardEnumUserDataDescriptor : DispatchingUserDataDescriptor
    {
        private Func<object, long> m_EnumToLong;

        private Func<object, ulong> m_EnumToULong;
        private Func<long, object> m_LongToEnum;
        private Func<ulong, object> m_ULongToEnum;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardEnumUserDataDescriptor"/> class.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <exception cref="System.ArgumentException">enumType must be an enum!</exception>
        public StandardEnumUserDataDescriptor(Type enumType, string friendlyName = null,
            string[] names = null, object[] values = null, Type underlyingType = null)
            : base(enumType, friendlyName)
        {
            if (!Framework.Do.IsEnum(enumType))
            {
                throw new ArgumentException("enumType must be an enum!");
            }

            this.UnderlyingType = underlyingType ?? Enum.GetUnderlyingType(enumType);
            this.IsUnsigned = ((this.UnderlyingType == typeof(byte)) || (this.UnderlyingType == typeof(ushort)) ||
                               (this.UnderlyingType == typeof(uint)) || (this.UnderlyingType == typeof(ulong)));

            names = names ?? Enum.GetNames(this.Type);
            values = values ?? Enum.GetValues(this.Type).OfType<object>().ToArray();

            this.FillMemberList(names, values);
        }

        /// <summary>
        /// Gets the underlying type of the enum.
        /// </summary>
        public Type UnderlyingType { get; }

        /// <summary>
        /// Gets a value indicating whether underlying type of the enum is unsigned.
        /// </summary>
        public bool IsUnsigned { get; }

        /// <summary>
        /// Gets a value indicating whether this instance describes a flags enumeration.
        /// </summary>
        public bool IsFlags { get; private set; }

        /// <summary>
        /// Fills the member list.
        /// </summary>
        private void FillMemberList(string[] names, object[] values)
        {
            for (int i = 0; i < names.Length; i++)
            {
                string name = names[i];
                var value = values.GetValue(i);
                var cvalue = UserData.Create(value, this);

                this.AddDynValue(name, cvalue);
            }

            var attrs = Framework.Do.GetCustomAttributes(this.Type, typeof(FlagsAttribute), true);

            if (attrs != null && attrs.Length > 0)
            {
                this.IsFlags = true;

                this.AddEnumMethod("flagsAnd", DynValue.NewCallback(this.Callback_And));
                this.AddEnumMethod("flagsOr", DynValue.NewCallback(this.Callback_Or));
                this.AddEnumMethod("flagsXor", DynValue.NewCallback(this.Callback_Xor));
                this.AddEnumMethod("flagsNot", DynValue.NewCallback(this.Callback_BwNot));
                this.AddEnumMethod("hasAll", DynValue.NewCallback(this.Callback_HasAll));
                this.AddEnumMethod("hasAny", DynValue.NewCallback(this.Callback_HasAny));
            }
        }


        /// <summary>
        /// Adds an enum method to the object
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="dynValue">The dyn value.</param>
        private void AddEnumMethod(string name, DynValue dynValue)
        {
            if (!this.HasMember(name))
            {
                this.AddDynValue(name, dynValue);
            }

            if (!this.HasMember("__" + name))
            {
                this.AddDynValue("__" + name, dynValue);
            }
        }


        /// <summary>
        /// Gets the value of the enum as a long
        /// </summary>
        private long GetValueSigned(DynValue dv)
        {
            this.CreateSignedConversionFunctions();

            if (dv.Type == DataType.Number)
            {
                return (long) dv.Number;
            }

            if ((dv.Type != DataType.UserData) || (dv.UserData.Descriptor != this) || (dv.UserData.Object == null))
            {
                throw new ScriptRuntimeException(
                    "Enum userdata or number expected, or enum is not of the correct type.");
            }

            return m_EnumToLong(dv.UserData.Object);
        }

        /// <summary>
        /// Gets the value of the enum as a ulong
        /// </summary>
        private ulong GetValueUnsigned(DynValue dv)
        {
            this.CreateUnsignedConversionFunctions();

            if (dv.Type == DataType.Number)
            {
                return (ulong) dv.Number;
            }

            if ((dv.Type != DataType.UserData) || (dv.UserData.Descriptor != this) || (dv.UserData.Object == null))
            {
                throw new ScriptRuntimeException(
                    "Enum userdata or number expected, or enum is not of the correct type.");
            }

            return m_EnumToULong(dv.UserData.Object);
        }

        /// <summary>
        /// Creates an enum value from a long
        /// </summary>
        private DynValue CreateValueSigned(long value)
        {
            this.CreateSignedConversionFunctions();
            return UserData.Create(m_LongToEnum(value), this);
        }

        /// <summary>
        /// Creates an enum value from a ulong
        /// </summary>
        private DynValue CreateValueUnsigned(ulong value)
        {
            this.CreateUnsignedConversionFunctions();
            return UserData.Create(m_ULongToEnum(value), this);
        }

        /// <summary>
        /// Creates conversion functions for signed enums
        /// </summary>
        private void CreateSignedConversionFunctions()
        {
            if (m_EnumToLong == null || m_LongToEnum == null)
            {
                if (this.UnderlyingType == typeof(sbyte))
                {
                    m_EnumToLong = o => (long) ((sbyte) o);
                    m_LongToEnum = o => (sbyte) (o);
                }
                else if (this.UnderlyingType == typeof(short))
                {
                    m_EnumToLong = o => (long) ((short) o);
                    m_LongToEnum = o => (short) (o);
                }
                else if (this.UnderlyingType == typeof(int))
                {
                    m_EnumToLong = o => (long) ((int) o);
                    m_LongToEnum = o => (int) (o);
                }
                else if (this.UnderlyingType == typeof(long))
                {
                    m_EnumToLong = o => (long) (o);
                    m_LongToEnum = o => o;
                }
                else
                {
                    throw new ScriptRuntimeException("Unexpected enum underlying type : {0}",
                        this.UnderlyingType.FullName);
                }
            }
        }

        /// <summary>
        /// Creates conversion functions for unsigned enums
        /// </summary>
        private void CreateUnsignedConversionFunctions()
        {
            if (m_EnumToULong == null || m_ULongToEnum == null)
            {
                if (this.UnderlyingType == typeof(byte))
                {
                    m_EnumToULong = o => (ulong) ((byte) o);
                    m_ULongToEnum = o => (byte) (o);
                }
                else if (this.UnderlyingType == typeof(ushort))
                {
                    m_EnumToULong = o => (ulong) ((ushort) o);
                    m_ULongToEnum = o => (ushort) (o);
                }
                else if (this.UnderlyingType == typeof(uint))
                {
                    m_EnumToULong = o => (ulong) ((uint) o);
                    m_ULongToEnum = o => (uint) (o);
                }
                else if (this.UnderlyingType == typeof(ulong))
                {
                    m_EnumToULong = o => (ulong) (o);
                    m_ULongToEnum = o => o;
                }
                else
                {
                    throw new ScriptRuntimeException("Unexpected enum underlying type : {0}",
                        this.UnderlyingType.FullName);
                }
            }
        }

        private DynValue PerformBinaryOperationS(string funcName, ScriptExecutionContext ctx, CallbackArguments args,
            Func<long, long, DynValue> operation)
        {
            if (args.Count != 2)
            {
                throw new ScriptRuntimeException("Enum.{0} expects two arguments", funcName);
            }

            long v1 = this.GetValueSigned(args[0]);
            long v2 = this.GetValueSigned(args[1]);
            return operation(v1, v2);
        }

        private DynValue PerformBinaryOperationU(string funcName, ScriptExecutionContext ctx, CallbackArguments args,
            Func<ulong, ulong, DynValue> operation)
        {
            if (args.Count != 2)
            {
                throw new ScriptRuntimeException("Enum.{0} expects two arguments", funcName);
            }

            ulong v1 = this.GetValueUnsigned(args[0]);
            ulong v2 = this.GetValueUnsigned(args[1]);
            return operation(v1, v2);
        }

        private DynValue PerformBinaryOperationS(string funcName, ScriptExecutionContext ctx, CallbackArguments args,
            Func<long, long, long> operation)
        {
            return this.PerformBinaryOperationS(funcName, ctx, args,
                (v1, v2) => this.CreateValueSigned(operation(v1, v2)));
        }

        private DynValue PerformBinaryOperationU(string funcName, ScriptExecutionContext ctx, CallbackArguments args,
            Func<ulong, ulong, ulong> operation)
        {
            return this.PerformBinaryOperationU(funcName, ctx, args,
                (v1, v2) => this.CreateValueUnsigned(operation(v1, v2)));
        }

        private DynValue PerformUnaryOperationS(string funcName, ScriptExecutionContext ctx, CallbackArguments args,
            Func<long, long> operation)
        {
            if (args.Count != 1)
            {
                throw new ScriptRuntimeException("Enum.{0} expects one argument.", funcName);
            }

            long v1 = this.GetValueSigned(args[0]);
            long r = operation(v1);
            return this.CreateValueSigned(r);
        }

        private DynValue PerformUnaryOperationU(string funcName, ScriptExecutionContext ctx, CallbackArguments args,
            Func<ulong, ulong> operation)
        {
            if (args.Count != 1)
            {
                throw new ScriptRuntimeException("Enum.{0} expects one argument.", funcName);
            }

            ulong v1 = this.GetValueUnsigned(args[0]);
            ulong r = operation(v1);
            return this.CreateValueUnsigned(r);
        }

        internal DynValue Callback_Or(ScriptExecutionContext ctx, CallbackArguments args)
        {
            if (this.IsUnsigned)
            {
                return this.PerformBinaryOperationU("or", ctx, args, (v1, v2) => v1 | v2);
            }

            return this.PerformBinaryOperationS("or", ctx, args, (v1, v2) => v1 | v2);
        }

        internal DynValue Callback_And(ScriptExecutionContext ctx, CallbackArguments args)
        {
            if (this.IsUnsigned)
            {
                return this.PerformBinaryOperationU("and", ctx, args, (v1, v2) => v1 & v2);
            }

            return this.PerformBinaryOperationS("and", ctx, args, (v1, v2) => v1 & v2);
        }

        internal DynValue Callback_Xor(ScriptExecutionContext ctx, CallbackArguments args)
        {
            if (this.IsUnsigned)
            {
                return this.PerformBinaryOperationU("xor", ctx, args, (v1, v2) => v1 ^ v2);
            }

            return this.PerformBinaryOperationS("xor", ctx, args, (v1, v2) => v1 ^ v2);
        }

        internal DynValue Callback_BwNot(ScriptExecutionContext ctx, CallbackArguments args)
        {
            if (this.IsUnsigned)
            {
                return this.PerformUnaryOperationU("not", ctx, args, v1 => ~v1);
            }

            return this.PerformUnaryOperationS("not", ctx, args, v1 => ~v1);
        }

        internal DynValue Callback_HasAll(ScriptExecutionContext ctx, CallbackArguments args)
        {
            if (this.IsUnsigned)
            {
                return this.PerformBinaryOperationU("hasAll", ctx, args,
                    (v1, v2) => DynValue.NewBoolean((v1 & v2) == v2));
            }

            return this.PerformBinaryOperationS("hasAll", ctx, args, (v1, v2) => DynValue.NewBoolean((v1 & v2) == v2));
        }

        internal DynValue Callback_HasAny(ScriptExecutionContext ctx, CallbackArguments args)
        {
            if (this.IsUnsigned)
            {
                return this.PerformBinaryOperationU("hasAny", ctx, args,
                    (v1, v2) => DynValue.NewBoolean((v1 & v2) != 0));
            }

            return this.PerformBinaryOperationS("hasAny", ctx, args, (v1, v2) => DynValue.NewBoolean((v1 & v2) != 0));
        }

        /// <summary>
        /// Determines whether the specified object is compatible with the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="obj">The object.</param>
        public override bool IsTypeCompatible(Type type, object obj)
        {
            if (obj != null)
            {
                return (this.Type == type);
            }

            return base.IsTypeCompatible(type, obj);
        }

        /// <summary>
        /// Gets a "meta" operation on this userdata. 
        /// In this specific case, only the concat operator is supported, only on flags enums and it implements the
        /// 'or' operator.
        /// </summary>
        /// <param name="ecToken">The execution control token of the script processing thread</param>
        /// <param name="script"></param>
        /// <param name="obj"></param>
        /// <param name="metaname"></param>
        public override DynValue MetaIndex(ExecutionControlToken ecToken, Script script, object obj, string metaname)
        {
            if (metaname == "__concat" && this.IsFlags)
            {
                return DynValue.NewCallback(this.Callback_Or);
            }

            return null;
        }
    }
}