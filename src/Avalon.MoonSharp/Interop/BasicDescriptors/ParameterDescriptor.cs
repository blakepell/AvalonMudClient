using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter.Compatibility;

namespace MoonSharp.Interpreter.Interop.BasicDescriptors
{
    /// <summary>
    /// Descriptor of parameters used in <see cref="IOverloadableMemberDescriptor"/> implementations.
    /// </summary>
    public sealed class ParameterDescriptor : IWireableDescriptor
    {
        /// <summary>
        /// If the type got restricted, the original type before the restriction.
        /// </summary>
        private Type m_OriginalType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDescriptor" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="hasDefaultValue">if set to <c>true</c> the parameter has default value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="isOut">if set to <c>true</c>, is an out param.</param>
        /// <param name="isRef">if set to <c>true</c> is a ref param.</param>
        /// <param name="isVarArgs">if set to <c>true</c> is variable arguments param.</param>
        public ParameterDescriptor(string name, Type type, bool hasDefaultValue = false, object defaultValue = null,
            bool isOut = false,
            bool isRef = false, bool isVarArgs = false)
        {
            this.Name = name;
            this.Type = type;
            this.HasDefaultValue = hasDefaultValue;
            this.DefaultValue = defaultValue;
            this.IsOut = isOut;
            this.IsRef = isRef;
            this.IsVarArgs = isVarArgs;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDescriptor" /> class. 
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="hasDefaultValue">if set to <c>true</c> the parameter has default value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="isOut">if set to <c>true</c>, is an out param.</param>
        /// <param name="isRef">if set to <c>true</c> is a ref param.</param>
        /// <param name="isVarArgs">if set to <c>true</c> is variable arguments param.</param>
        /// <param name="typeRestriction">The type restriction, or nll.</param>
        public ParameterDescriptor(string name, Type type, bool hasDefaultValue, object defaultValue, bool isOut,
            bool isRef, bool isVarArgs, Type typeRestriction)
        {
            this.Name = name;
            this.Type = type;
            this.HasDefaultValue = hasDefaultValue;
            this.DefaultValue = defaultValue;
            this.IsOut = isOut;
            this.IsRef = isRef;
            this.IsVarArgs = isVarArgs;

            if (typeRestriction != null)
            {
                this.RestrictType(typeRestriction);
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDescriptor"/> class.
        /// </summary>
        /// <param name="pi">A ParameterInfo taken from reflection.</param>
        public ParameterDescriptor(ParameterInfo pi)
        {
            this.Name = pi.Name;
            this.Type = pi.ParameterType;
            this.HasDefaultValue = !(Framework.Do.IsDbNull(pi.DefaultValue));
            this.DefaultValue = pi.DefaultValue;
            this.IsOut = pi.IsOut;
            this.IsRef = pi.ParameterType.IsByRef;
            this.IsVarArgs = (pi.ParameterType.IsArray &&
                              pi.GetCustomAttributes(typeof(ParamArrayAttribute), true).Any());
        }

        /// <summary>
        /// Gets the name of the parameter
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of the parameter
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance has a default value.
        /// </summary>
        public bool HasDefaultValue { get; }

        /// <summary>
        /// Gets the default value
        /// </summary>
        public object DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is an out parameter
        /// </summary>
        public bool IsOut { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a "ref" parameter
        /// </summary>
        public bool IsRef { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a variable arguments param
        /// </summary>
        public bool IsVarArgs { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has been restricted.
        /// </summary>
        public bool HasBeenRestricted => m_OriginalType != null;

        /// <summary>
        /// Gets the original type of the parameter before any restriction has been applied.
        /// </summary>
        public Type OriginalType => m_OriginalType ?? this.Type;


        /// <summary>
        /// Prepares the descriptor for hard-wiring.
        /// The descriptor fills the passed table with all the needed data for hard-wire generators to generate the appropriate code.
        /// </summary>
        /// <param name="table">The table to be filled</param>
        public void PrepareForWiring(Table table)
        {
            table.Set("name", DynValue.NewString(this.Name));

            if (this.Type.IsByRef)
            {
                table.Set("type", DynValue.NewString(this.Type.GetElementType()?.FullName ?? "unknown"));
            }
            else
            {
                table.Set("type", DynValue.NewString(this.Type.FullName));
            }

            if (this.OriginalType.IsByRef)
            {
                table.Set("origtype", DynValue.NewString(this.OriginalType.GetElementType()?.FullName ?? "unknown"));
            }
            else
            {
                table.Set("origtype", DynValue.NewString(this.OriginalType.FullName));
            }

            table.Set("default", DynValue.NewBoolean(this.HasDefaultValue));
            table.Set("out", DynValue.NewBoolean(this.IsOut));
            table.Set("ref", DynValue.NewBoolean(this.IsRef));
            table.Set("varargs", DynValue.NewBoolean(this.IsVarArgs));
            table.Set("restricted", DynValue.NewBoolean(this.HasBeenRestricted));
        }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{this.Type.Name} {this.Name}{(this.HasDefaultValue ? " = ..." : "")}";
        }

        /// <summary>
        /// Restricts the type of this parameter to a tighter constraint.
        /// Restrictions must be applied before the <see cref="IOverloadableMemberDescriptor"/> containing this
        /// parameter is used in any way.
        /// </summary>
        /// <param name="type">The new type.</param>
        /// <exception cref="System.InvalidOperationException">
        /// Cannot restrict a ref/out or varargs param
        /// or
        /// Specified operation is not a restriction
        /// </exception>
        public void RestrictType(Type type)
        {
            if (this.IsOut || this.IsRef || this.IsVarArgs)
            {
                throw new InvalidOperationException("Cannot restrict a ref/out or varargs param");
            }

            if (!Framework.Do.IsAssignableFrom(this.Type, type))
            {
                throw new InvalidOperationException("Specified operation is not a restriction");
            }

            m_OriginalType = this.Type;
            this.Type = type;
        }
    }
}