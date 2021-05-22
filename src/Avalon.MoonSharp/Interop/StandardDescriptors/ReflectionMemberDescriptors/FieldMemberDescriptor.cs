using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop
{
    /// <summary>
    /// Class providing easier marshalling of CLR fields
    /// </summary>
    public class FieldMemberDescriptor : IMemberDescriptor, IOptimizableDescriptor, IWireableDescriptor
    {
        private object m_ConstValue;

        private Func<object, object> m_OptimizedGetter;


        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMemberDescriptor"/> class.
        /// </summary>
        /// <param name="fi">The FieldInfo.</param>
        /// <param name="accessMode">The <see cref="InteropAccessMode" /> </param>
        public FieldMemberDescriptor(FieldInfo fi, InteropAccessMode accessMode)
        {
            if (Script.GlobalOptions.Platform.IsRunningOnAOT())
            {
                accessMode = InteropAccessMode.Reflection;
            }

            this.FieldInfo = fi;
            this.AccessMode = accessMode;
            this.Name = fi.Name;
            this.IsStatic = this.FieldInfo.IsStatic;

            if (this.FieldInfo.IsLiteral)
            {
                this.IsConst = true;
                m_ConstValue = this.FieldInfo.GetValue(null);
            }
            else
            {
                this.IsReadonly = this.FieldInfo.IsInitOnly;
            }

            if (this.AccessMode == InteropAccessMode.Preoptimized)
            {
                this.OptimizeGetter();
            }
        }

        /// <summary>
        /// Gets the FieldInfo got by reflection
        /// </summary>
        public FieldInfo FieldInfo { get; }

        /// <summary>
        /// Gets the <see cref="InteropAccessMode" />
        /// </summary>
        public InteropAccessMode AccessMode { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a constant 
        /// </summary>
        public bool IsConst { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is readonly 
        /// </summary>
        public bool IsReadonly { get; }

        /// <summary>
        /// Gets a value indicating whether the described property is static.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// Gets the name of the property
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// Gets the value of the property
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="obj">The object.</param>
        public DynValue GetValue(Script script, object obj)
        {
            this.CheckAccess(MemberDescriptorAccess.CanRead, obj);

            // optimization+workaround of Unity bug.. 
            if (this.IsConst)
            {
                return ClrToScriptConversions.ObjectToDynValue(script, m_ConstValue);
            }

            if (this.AccessMode == InteropAccessMode.LazyOptimized && m_OptimizedGetter == null)
            {
                this.OptimizeGetter();
            }

            object result;

            if (m_OptimizedGetter != null)
            {
                result = m_OptimizedGetter(obj);
            }
            else
            {
                result = this.FieldInfo.GetValue(obj);
            }

            return ClrToScriptConversions.ObjectToDynValue(script, result);
        }

        /// <summary>
        /// Sets the value of the property
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="obj">The object.</param>
        /// <param name="v">The value to set.</param>
        public void SetValue(Script script, object obj, DynValue v)
        {
            this.CheckAccess(MemberDescriptorAccess.CanWrite, obj);

            if (this.IsReadonly || this.IsConst)
            {
                throw new ScriptRuntimeException("userdata field '{0}.{1}' cannot be written to.",
                    this.FieldInfo.DeclaringType.Name, this.Name);
            }

            var value = ScriptToClrConversions.DynValueToObjectOfType(v, this.FieldInfo.FieldType, null, false);

            try
            {
                if (value is double d)
                {
                    value = NumericConversions.DoubleToType(this.FieldInfo.FieldType, d);
                }

                this.FieldInfo.SetValue(this.IsStatic ? null : obj, value);
            }
            catch (ArgumentException)
            {
                // non-optimized setters fall here
                throw ScriptRuntimeException.UserDataArgumentTypeMismatch(v.Type, this.FieldInfo.FieldType);
            }
            catch (InvalidCastException)
            {
                // optimized setters fall here
                throw ScriptRuntimeException.UserDataArgumentTypeMismatch(v.Type, this.FieldInfo.FieldType);
            }
#if !(ENABLE_DOTNET || NETFX_CORE)
            catch (FieldAccessException ex)
            {
                throw new ScriptRuntimeException(ex);
            }
#endif
        }


        /// <summary>
        /// Gets the types of access supported by this member
        /// </summary>
        public MemberDescriptorAccess MemberAccess
        {
            get
            {
                if (this.IsReadonly || this.IsConst)
                {
                    return MemberDescriptorAccess.CanRead;
                }

                return MemberDescriptorAccess.CanRead | MemberDescriptorAccess.CanWrite;
            }
        }

        void IOptimizableDescriptor.Optimize()
        {
            if (m_OptimizedGetter == null)
            {
                this.OptimizeGetter();
            }
        }

        /// <summary>
        /// Prepares the descriptor for hard-wiring.
        /// The descriptor fills the passed table with all the needed data for hardwire generators to generate the appropriate code.
        /// </summary>
        /// <param name="t">The table to be filled</param>
        public void PrepareForWiring(Table t)
        {
            t.Set("class", DynValue.NewString(this.GetType().FullName));
            t.Set("visibility", DynValue.NewString(this.FieldInfo.GetClrVisibility()));

            t.Set("name", DynValue.NewString(this.Name));
            t.Set("static", DynValue.NewBoolean(this.IsStatic));
            t.Set("const", DynValue.NewBoolean(this.IsConst));
            t.Set("readonly", DynValue.NewBoolean(this.IsReadonly));
            t.Set("decltype", DynValue.NewString(this.FieldInfo.DeclaringType.FullName));
            t.Set("declvtype", DynValue.NewBoolean(Framework.Do.IsValueType(this.FieldInfo.DeclaringType)));
            t.Set("type", DynValue.NewString(this.FieldInfo.FieldType.FullName));
            t.Set("read", DynValue.NewBoolean(true));
            t.Set("write", DynValue.NewBoolean(!(this.IsConst || this.IsReadonly)));
        }


        /// <summary>
        /// Tries to create a new StandardUserDataFieldDescriptor, returning <c>null</c> in case the field is not 
        /// visible to script code.
        /// </summary>
        /// <param name="fi">The FieldInfo.</param>
        /// <param name="accessMode">The <see cref="InteropAccessMode" /></param>
        /// <returns>A new StandardUserDataFieldDescriptor or null.</returns>
        public static FieldMemberDescriptor TryCreateIfVisible(FieldInfo fi, InteropAccessMode accessMode)
        {
            if (fi.GetVisibilityFromAttributes() ?? fi.IsPublic)
            {
                return new FieldMemberDescriptor(fi, accessMode);
            }

            return null;
        }

        internal void OptimizeGetter()
        {
            if (this.IsConst)
            {
                return;
            }

            if (this.IsStatic)
            {
                var paramExp = Expression.Parameter(typeof(object), "dummy");
                var propAccess = Expression.Field(null, this.FieldInfo);
                var castPropAccess = Expression.Convert(propAccess, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
                Interlocked.Exchange(ref m_OptimizedGetter, lambda.Compile());
            }
            else
            {
                var paramExp = Expression.Parameter(typeof(object), "obj");
                var castParamExp = Expression.Convert(paramExp, this.FieldInfo.DeclaringType);
                var propAccess = Expression.Field(castParamExp, this.FieldInfo);
                var castPropAccess = Expression.Convert(propAccess, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
                Interlocked.Exchange(ref m_OptimizedGetter, lambda.Compile());
            }
        }
    }
}