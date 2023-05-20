using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;

namespace MoonSharp.Interpreter.Interop.UserDataRegistries
{
    /// <summary>
    /// Registry of all type descriptors. Use UserData statics to access these.
    /// </summary>
    internal static class TypeDescriptorRegistry
    {
        private static object s_Lock = new object();

        private static Dictionary<Type, IUserDataDescriptor> s_TypeRegistry =
            new Dictionary<Type, IUserDataDescriptor>();

        private static Dictionary<Type, IUserDataDescriptor> s_TypeRegistryHistory =
            new Dictionary<Type, IUserDataDescriptor>();

        private static InteropAccessMode s_DefaultAccessMode;

        /// <summary>
        /// Gets or sets the default access mode to be used in the whole application
        /// </summary>
        /// <value>
        /// The default access mode.
        /// </value>
        /// <exception cref="System.ArgumentException">InteropAccessMode is InteropAccessMode.Default</exception>
        internal static InteropAccessMode DefaultAccessMode
        {
            get => s_DefaultAccessMode;
            set
            {
                if (value == InteropAccessMode.Default)
                {
                    throw new ArgumentException("InteropAccessMode is InteropAccessMode.Default");
                }

                s_DefaultAccessMode = value;
            }
        }

        /// <summary>
        /// Gets the list of registered types.
        /// </summary>
        /// <value>
        /// The registered types.
        /// </value>
        public static IEnumerable<KeyValuePair<Type, IUserDataDescriptor>> RegisteredTypes
        {
            get
            {
                lock (s_Lock)
                {
                    return s_TypeRegistry.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets the list of registered types, including unregistered types.
        /// </summary>
        /// <value>
        /// The registered types.
        /// </value>
        public static IEnumerable<KeyValuePair<Type, IUserDataDescriptor>> RegisteredTypesHistory
        {
            get
            {
                lock (s_Lock)
                {
                    return s_TypeRegistryHistory.ToArray();
                }
            }
        }


        /// <summary>
        /// Gets or sets the registration policy.
        /// </summary>
        internal static IRegistrationPolicy RegistrationPolicy { get; set; }

        /// <summary>
        /// Registers all types marked with a MoonSharpUserDataAttribute that ar contained in an assembly.
        /// </summary>
        /// <param name="asm">The assembly.</param>
        /// <param name="includeExtensionTypes">if set to <c>true</c> extension types are registered to the appropriate registry.</param>
        internal static void RegisterAssembly(Assembly asm = null, bool includeExtensionTypes = false)
        {
            if (asm == null)
            {
#if NETFX_CORE || DOTNET_CORE
					throw new NotSupportedException("Assembly.GetCallingAssembly is not supported on target framework.");
#else
                asm = Assembly.GetCallingAssembly();
#endif
            }

            if (includeExtensionTypes)
            {
                var extensionTypes = from t in asm.SafeGetTypes()
                                     let attributes =
                                         Framework.Do.GetCustomAttributes(t, typeof(ExtensionAttribute), true)
                                     where attributes != null && attributes.Length > 0
                                     select new { Attributes = attributes, DataType = t };

                foreach (var extType in extensionTypes)
                {
                    UserData.RegisterExtensionType(extType.DataType);
                }
            }


            var userDataTypes = from t in asm.SafeGetTypes()
                                let attributes =
                                    Framework.Do.GetCustomAttributes(t, typeof(MoonSharpUserDataAttribute), true)
                                where attributes != null && attributes.Length > 0
                                select new { Attributes = attributes, DataType = t };

            foreach (var userDataType in userDataTypes)
            {
                UserData.RegisterType(userDataType.DataType, userDataType.Attributes
                    .OfType<MoonSharpUserDataAttribute>()
                    .First()
                    .AccessMode);
            }
        }


        /// <summary>
        /// Determines whether the specified type is registered. Note that this should be used only to check if a descriptor
        /// has been registered EXACTLY. For many types a descriptor can still be created, for example through the descriptor
        /// of a base type or implemented interfaces.
        /// </summary>
        /// <param name="type">The type</param>
        internal static bool IsTypeRegistered(Type type)
        {
            lock (s_Lock)
            {
                return s_TypeRegistry.ContainsKey(type);
            }
        }


        /// <summary>
        /// Unregisters a type.
        /// WARNING: unregistering types at runtime is a dangerous practice and may cause unwanted errors.
        /// Use this only for testing purposes or to re-register the same type in a slightly different way.
        /// Additionally, it's a good practice to discard all previous loaded scripts after calling this method.
        /// </summary>
        /// <param name="t">The The type to be unregistered</param>
        internal static void UnregisterType(Type t)
        {
            lock (s_Lock)
            {
                if (s_TypeRegistry.TryGetValue(t, out var value))
                {
                    PerformRegistration(t, null, value);
                }
            }
        }

        /// <summary>
        /// Registers a proxy type.
        /// </summary>
        /// <param name="proxyFactory">The proxy factory.</param>
        /// <param name="accessMode">The access mode.</param>
        /// <param name="friendlyName">Name of the friendly.</param>
        internal static IUserDataDescriptor RegisterProxyType_Impl(IProxyFactory proxyFactory,
            InteropAccessMode accessMode, string friendlyName)
        {
            var proxyDescriptor = RegisterType_Impl(proxyFactory.ProxyType, accessMode, friendlyName, null);
            return RegisterType_Impl(proxyFactory.TargetType, accessMode, friendlyName,
                new ProxyUserDataDescriptor(proxyFactory, proxyDescriptor, friendlyName));
        }


        /// <summary>
        /// Registers a type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="accessMode">The access mode (used only if a default type descriptor is created).</param>
        /// <param name="friendlyName">Friendly name of the descriptor.</param>
        /// <param name="descriptor">The descriptor, or null to use a default one.</param>
        internal static IUserDataDescriptor RegisterType_Impl(Type type, InteropAccessMode accessMode,
            string friendlyName, IUserDataDescriptor descriptor)
        {
            accessMode = ResolveDefaultAccessModeForType(accessMode, type);

            lock (s_Lock)
            {
                s_TypeRegistry.TryGetValue(type, out var oldDescriptor);

                if (descriptor == null)
                {
                    if (IsTypeBlacklisted(type))
                    {
                        return null;
                    }

                    if (Framework.Do.GetInterfaces(type).Any(ii => ii == typeof(IUserDataType)))
                    {
                        var audd = new AutoDescribingUserDataDescriptor(type, friendlyName);
                        return PerformRegistration(type, audd, oldDescriptor);
                    }

                    if (Framework.Do.IsGenericTypeDefinition(type))
                    {
                        var typeGen = new StandardGenericsUserDataDescriptor(type, accessMode);
                        return PerformRegistration(type, typeGen, oldDescriptor);
                    }

                    if (Framework.Do.IsEnum(type))
                    {
                        var enumDescr = new StandardEnumUserDataDescriptor(type, friendlyName);
                        return PerformRegistration(type, enumDescr, oldDescriptor);
                    }

                    var udd = new StandardUserDataDescriptor(type, accessMode, friendlyName);

                    if (accessMode == InteropAccessMode.BackgroundOptimized)
                    {
#if NETFX_CORE
							System.Threading.Tasks.Task.Run(() => ((IOptimizableDescriptor)udd).Optimize());
#else
                        ThreadPool.QueueUserWorkItem(o => ((IOptimizableDescriptor)udd).Optimize());
#endif
                    }

                    return PerformRegistration(type, udd, oldDescriptor);
                }

                PerformRegistration(type, descriptor, oldDescriptor);
                return descriptor;
            }
        }

        private static IUserDataDescriptor PerformRegistration(Type type, IUserDataDescriptor newDescriptor,
            IUserDataDescriptor oldDescriptor)
        {
            var result = RegistrationPolicy.HandleRegistration(newDescriptor, oldDescriptor);

            if (result != oldDescriptor)
            {
                if (result == null)
                {
                    s_TypeRegistry.Remove(type);
                }
                else
                {
                    s_TypeRegistry[type] = result;
                    s_TypeRegistryHistory[type] = result;
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves the default type of the access mode for the given type
        /// </summary>
        /// <param name="accessMode">The access mode.</param>
        /// <param name="type">The type.</param>
        internal static InteropAccessMode ResolveDefaultAccessModeForType(InteropAccessMode accessMode, Type type)
        {
            if (accessMode == InteropAccessMode.Default)
            {
                var attr = Framework.Do.GetCustomAttributes(type, true).OfType<MoonSharpUserDataAttribute>()
                    .SingleOrDefault();

                if (attr != null)
                {
                    accessMode = attr.AccessMode;
                }
            }


            if (accessMode == InteropAccessMode.Default)
            {
                accessMode = s_DefaultAccessMode;
            }

            return accessMode;
        }


        /// <summary>
        /// Gets the best possible type descriptor for a specified CLR type.
        /// </summary>
        /// <param name="type">The CLR type for which the descriptor is desired.</param>
        /// <param name="searchInterfaces">if set to <c>true</c> interfaces are used in the search.</param>
        internal static IUserDataDescriptor GetDescriptorForType(Type type, bool searchInterfaces)
        {
            lock (s_Lock)
            {
                IUserDataDescriptor typeDescriptor = null;

                // if the type has been explicitly registered, return its descriptor as it's complete
                if (s_TypeRegistry.TryGetValue(type, out var forType))
                {
                    return forType;
                }

                if (RegistrationPolicy.AllowTypeAutoRegistration(type))
                {
                    // no autoreg of delegates
                    if (!Framework.Do.IsAssignableFrom((typeof(Delegate)), type))
                    {
                        return RegisterType_Impl(type, DefaultAccessMode, type.FullName, null);
                    }
                }

                // search for the base object descriptors
                for (var t = type; t != null; t = Framework.Do.GetBaseType(t))
                {
                    if (s_TypeRegistry.TryGetValue(t, out var u))
                    {
                        typeDescriptor = u;
                        break;
                    }

                    if (Framework.Do.IsGenericType(t))
                    {
                        if (s_TypeRegistry.TryGetValue(t.GetGenericTypeDefinition(), out u))
                        {
                            typeDescriptor = u;
                            break;
                        }
                    }
                }

                if (typeDescriptor is IGeneratorUserDataDescriptor descriptor)
                {
                    typeDescriptor = descriptor.Generate(type);
                }


                // we should not search interfaces (for example, it's just for statics..), no need to look further
                if (!searchInterfaces)
                {
                    return typeDescriptor;
                }

                var descriptors = new List<IUserDataDescriptor>();

                if (typeDescriptor != null)
                {
                    descriptors.Add(typeDescriptor);
                }


                foreach (var interfaceType in Framework.Do.GetInterfaces(type))
                {
                    if (s_TypeRegistry.TryGetValue(interfaceType, out var interfaceDescriptor))
                    {
                        if (interfaceDescriptor is IGeneratorUserDataDescriptor dataDescriptor)
                        {
                            interfaceDescriptor =
                                dataDescriptor.Generate(type);
                        }

                        if (interfaceDescriptor != null)
                        {
                            descriptors.Add(interfaceDescriptor);
                        }
                    }
                    else if (Framework.Do.IsGenericType(interfaceType))
                    {
                        if (s_TypeRegistry.TryGetValue(interfaceType.GetGenericTypeDefinition(),
                            out interfaceDescriptor))
                        {
                            if (interfaceDescriptor is IGeneratorUserDataDescriptor dataDescriptor)
                            {
                                interfaceDescriptor =
                                    dataDescriptor.Generate(type);
                            }

                            if (interfaceDescriptor != null)
                            {
                                descriptors.Add(interfaceDescriptor);
                            }
                        }
                    }
                }

                if (descriptors.Count == 1)
                {
                    return descriptors[0];
                }

                if (descriptors.Count == 0)
                {
                    return null;
                }

                return new CompositeUserDataDescriptor(descriptors, type);
            }
        }

        private static bool FrameworkIsAssignableFrom(Type type)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Determines whether the specified type is blacklisted.
        /// Blacklisted types CANNOT be registered using default descriptors but they can still be registered
        /// with custom descriptors. Forcing registration of blacklisted types in this way can introduce
        /// side effects.
        /// </summary>
        /// <param name="t">The t.</param>
        public static bool IsTypeBlacklisted(Type t)
        {
            if (Framework.Do.IsValueType(t) && Framework.Do.GetInterfaces(t).Contains(typeof(IEnumerator)))
            {
                return true;
            }

            return false;
        }
    }
}