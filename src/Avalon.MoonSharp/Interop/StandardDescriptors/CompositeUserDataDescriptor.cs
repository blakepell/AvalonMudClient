﻿using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Compatibility;

namespace MoonSharp.Interpreter.Interop
{
    /// <summary>
    /// A user data descriptor which aggregates multiple descriptors and tries dispatching members
    /// on them, in order.
    /// 
    /// Used, for example, for objects implementing multiple interfaces but for which no descriptor is 
    /// specifically registered.
    /// </summary>
    public class CompositeUserDataDescriptor : IUserDataDescriptor
    {
        private List<IUserDataDescriptor> m_Descriptors;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeUserDataDescriptor"/> class.
        /// </summary>
        /// <param name="descriptors">The descriptors.</param>
        /// <param name="type">The type.</param>
        public CompositeUserDataDescriptor(List<IUserDataDescriptor> descriptors, Type type)
        {
            m_Descriptors = descriptors;
            this.Type = type;
        }

        /// <summary>
        /// Gets the descriptors aggregated by this object, allowing changes to the descriptor list
        /// </summary>
        public IList<IUserDataDescriptor> Descriptors => m_Descriptors;


        /// <summary>
        /// Gets the name of the descriptor (usually, the name of the type described).
        /// </summary>
        public string Name => "^" + this.Type.FullName;

        /// <summary>
        /// Gets the type this descriptor refers to
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Performs an "index" "get" operation.
        /// </summary>
        /// <param name="ecToken">The execution control token of the script processing thread</param>
        /// <param name="script">The script originating the request</param>
        /// <param name="obj">The object (null if a static request is done)</param>
        /// <param name="index">The index.</param>
        /// <param name="isNameIndex">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
        public DynValue Index(ExecutionControlToken ecToken, Script script, object obj, DynValue index,
            bool isNameIndex)
        {
            foreach (var dd in m_Descriptors)
            {
                var v = dd.Index(ecToken, script, obj, index, isNameIndex);

                if (v != null)
                {
                    return v;
                }
            }

            return null;
        }

        /// <summary>
        /// Performs an "index" "set" operation.
        /// </summary>
        /// <param name="ecToken">The execution control token of the script processing thread</param>
        /// <param name="script">The script originating the request</param>
        /// <param name="obj">The object (null if a static request is done)</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value to be set</param>
        /// <param name="isNameIndex">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
        public bool SetIndex(ExecutionControlToken ecToken, Script script, object obj, DynValue index, DynValue value,
            bool isNameIndex)
        {
            foreach (var dd in m_Descriptors)
            {
                if (dd.SetIndex(ecToken, script, obj, index, value, isNameIndex))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts this userdata to string
        /// </summary>
        /// <param name="obj">The object.</param>
        public string AsString(object obj)
        {
            return obj?.ToString();
        }


        /// <summary>
        /// Gets a "meta" operation on this userdata. If a descriptor does not support this functionality,
        /// it should return "null" (not a nil). 
        /// These standard metamethods can be supported (the return value should be a function accepting the
        /// classic parameters of the corresponding metamethod):
        /// __add, __sub, __mul, __div, __div, __pow, __unm, __eq, __lt, __le, __lt, __len, __concat, 
        /// __pairs, __ipairs, __iterator, __call
        /// These standard metamethods are supported through other calls for efficiency:
        /// __index, __newindex, __tostring
        /// </summary>
        /// <param name="ecToken">The execution control token of the script processing thread</param>
        /// <param name="script">The script originating the request</param>
        /// <param name="obj">The object (null if a static request is done)</param>
        /// <param name="metaname">The name of the metamember.</param>
        public DynValue MetaIndex(ExecutionControlToken ecToken, Script script, object obj, string metaname)
        {
            foreach (var dd in m_Descriptors)
            {
                var v = dd.MetaIndex(ecToken, script, obj, metaname);

                if (v != null)
                {
                    return v;
                }
            }

            return null;
        }


        /// <summary>
        /// Determines whether the specified object is compatible with the specified type.
        /// Unless a very specific behaviour is needed, the correct implementation is a 
        /// simple " return type.IsInstanceOfType(obj); "
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="obj">The object.</param>
        public bool IsTypeCompatible(Type type, object obj)
        {
            return Framework.Do.IsInstanceOfType(type, obj);
        }
    }
}