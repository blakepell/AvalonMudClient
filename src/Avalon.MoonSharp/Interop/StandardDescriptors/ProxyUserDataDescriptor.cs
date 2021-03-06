﻿using System;
using MoonSharp.Interpreter.Compatibility;

namespace MoonSharp.Interpreter.Interop
{
    /// <summary>
    /// Data descriptor used for proxy objects
    /// </summary>
    public sealed class ProxyUserDataDescriptor : IUserDataDescriptor
    {
        private IProxyFactory m_ProxyFactory;

        internal ProxyUserDataDescriptor(IProxyFactory proxyFactory, IUserDataDescriptor proxyDescriptor,
            string friendlyName = null)
        {
            m_ProxyFactory = proxyFactory;
            this.Name = friendlyName ?? (proxyFactory.TargetType.Name + "::proxy");
            this.InnerDescriptor = proxyDescriptor;
        }

        /// <summary>
        /// Gets the descriptor which describes the proxy object
        /// </summary>
        public IUserDataDescriptor InnerDescriptor { get; }

        /// <summary>
        /// Gets the name of the descriptor (usually, the name of the type described).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type this descriptor refers to
        /// </summary>
        public Type Type => m_ProxyFactory.TargetType;

        /// <summary>
        /// Performs an "index" "get" operation.
        /// </summary>
        /// <param name="ecToken">The execution control token of the script processing thread</param>
        /// <param name="script">The script originating the request</param>
        /// <param name="obj">The object (null if a static request is done)</param>
        /// <param name="index">The index.</param>
        /// <param name="isDirectIndexing">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
        public DynValue Index(ExecutionControlToken ecToken, Script script, object obj, DynValue index,
            bool isDirectIndexing)
        {
            return this.InnerDescriptor.Index(ecToken, script, this.Proxy(obj), index, isDirectIndexing);
        }

        /// <summary>
        /// Performs an "index" "set" operation.
        /// </summary>
        /// <param name="ecToken">The execution control token of the script processing thread</param>
        /// <param name="script">The script originating the request</param>
        /// <param name="obj">The object (null if a static request is done)</param>
        /// <param name="index">The index.</param>
        /// <param name="value">The value to be set</param>
        /// <param name="isDirectIndexing">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
        public bool SetIndex(ExecutionControlToken ecToken, Script script, object obj, DynValue index, DynValue value,
            bool isDirectIndexing)
        {
            return this.InnerDescriptor.SetIndex(ecToken, script, this.Proxy(obj), index, value, isDirectIndexing);
        }

        /// <summary>
        /// Converts this userdata to string
        /// </summary>
        /// <param name="obj">The object.</param>
        public string AsString(object obj)
        {
            return this.InnerDescriptor.AsString(this.Proxy(obj));
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
            return this.InnerDescriptor.MetaIndex(ecToken, script, this.Proxy(obj), metaname);
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

        /// <summary>
        /// Proxies the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        private object Proxy(object obj)
        {
            return obj != null ? m_ProxyFactory.CreateProxyObject(obj) : null;
        }
    }
}