using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.StandardDescriptors;

namespace MoonSharp.Interpreter.Interop
{
    /// <summary>
    /// Class providing easier marshalling of CLR events. Handling is limited to a narrow range of handler signatures, which,
    /// however, covers in practice most of all available events.
    /// </summary>
    public class EventMemberDescriptor : IMemberDescriptor
    {
        /// <summary>
        /// The maximum number of arguments supported in an event handler delegate
        /// </summary>
        public const int MAX_ARGS_IN_DELEGATE = 16;

        private MethodInfo m_Add, m_Remove;

        private MultiDictionary<object, Closure> m_Callbacks =
            new MultiDictionary<object, Closure>(new DataStructs.ReferenceEqualityComparer());

        private Dictionary<object, Delegate> m_Delegates =
            new Dictionary<object, Delegate>(new DataStructs.ReferenceEqualityComparer());


        private object m_Lock = new object();


        /// <summary>
        /// Initializes a new instance of the <see cref="EventMemberDescriptor"/> class.
        /// </summary>
        /// <param name="ei">The ei.</param>
        /// <param name="accessMode">The access mode.</param>
        public EventMemberDescriptor(EventInfo ei, InteropAccessMode accessMode = InteropAccessMode.Default)
        {
            CheckEventIsCompatible(ei, true);
            this.EventInfo = ei;
            m_Add = Framework.Do.GetAddMethod(ei);
            m_Remove = Framework.Do.GetRemoveMethod(ei);
            this.IsStatic = m_Add.IsStatic;
        }


        /// <summary>
        /// Gets the EventInfo object of the event described by this descriptor
        /// </summary>
        public EventInfo EventInfo { get; }

        /// <summary>
        /// Gets a value indicating whether the event described by this descriptor is static.
        /// </summary>
        public bool IsStatic { get; }

        /// <summary>
        /// Gets a dynvalue which is a facade supporting add/remove methods which is callable from scripts
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="obj">The object for which the facade should be written.</param>
        public DynValue GetValue(Script script, object obj)
        {
            this.CheckAccess(MemberDescriptorAccess.CanRead, obj);

            if (this.IsStatic)
            {
                obj = this;
            }

            return UserData.Create(new EventFacade(this, obj));
        }


        /// <summary>
        /// Gets the name of the member
        /// </summary>
        public string Name => this.EventInfo.Name;

        /// <summary>
        /// Gets the types of access supported by this member
        /// </summary>
        public MemberDescriptorAccess MemberAccess => MemberDescriptorAccess.CanRead;

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="obj">The object.</param>
        /// <param name="v">The v.</param>
        public void SetValue(Script script, object obj, DynValue v)
        {
            this.CheckAccess(MemberDescriptorAccess.CanWrite, obj);
        }

        /// <summary>
        /// Tries to create a new StandardUserDataEventDescriptor, returning <c>null</c> in case the method is not 
        /// visible to script code.
        /// </summary>
        /// <param name="ei">The EventInfo.</param>
        /// <param name="accessMode">The <see cref="InteropAccessMode" /></param>
        /// <returns>A new StandardUserDataEventDescriptor or null.</returns>
        public static EventMemberDescriptor TryCreateIfVisible(EventInfo ei, InteropAccessMode accessMode)
        {
            if (!CheckEventIsCompatible(ei, false))
            {
                return null;
            }

            var addm = Framework.Do.GetAddMethod(ei);
            var remm = Framework.Do.GetRemoveMethod(ei);

            if (ei.GetVisibilityFromAttributes() ??
                ((remm != null && remm.IsPublic) && (addm != null && addm.IsPublic)))
            {
                return new EventMemberDescriptor(ei, accessMode);
            }

            return null;
        }


        /// <summary>
        /// Checks if the event is compatible with a standard descriptor
        /// </summary>
        /// <param name="ei">The EventInfo.</param>
        /// <param name="throwException">if set to <c>true</c> an exception with the proper error message is thrown if not compatible.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if throwException is <c>true</c> and one of this applies:
        /// The event is declared in a value type
        /// or
        /// The event does not have both add and remove methods 
        /// or
        /// The event handler type doesn't implement a public Invoke method
        /// or
        /// The event handler has a return type which is not System.Void
        /// or
        /// The event handler has more than MAX_ARGS_IN_DELEGATE parameters
        /// or
        /// The event handler has a value type parameter or a by ref parameter
        /// or
        /// The event handler signature is not a valid method according to <see cref="MethodMemberDescriptor.CheckMethodIsCompatible"/>
        /// </exception>
        public static bool CheckEventIsCompatible(EventInfo ei, bool throwException)
        {
            if (Framework.Do.IsValueType(ei.DeclaringType))
            {
                if (throwException)
                {
                    throw new ArgumentException("Events are not supported on value types");
                }

                return false;
            }

            if ((Framework.Do.GetAddMethod(ei) == null) || (Framework.Do.GetRemoveMethod(ei) == null))
            {
                if (throwException)
                {
                    throw new ArgumentException("Event must have add and remove methods");
                }

                return false;
            }

            var invoke = Framework.Do.GetMethod(ei.EventHandlerType, "Invoke");

            if (invoke == null)
            {
                if (throwException)
                {
                    throw new ArgumentException("Event handler type doesn't seem to be a delegate");
                }

                return false;
            }

            if (!MethodMemberDescriptor.CheckMethodIsCompatible(invoke, throwException))
            {
                return false;
            }

            if (invoke.ReturnType != typeof(void))
            {
                if (throwException)
                {
                    throw new ArgumentException("Event handler cannot have a return type");
                }

                return false;
            }

            var pars = invoke.GetParameters();

            if (pars.Length > MAX_ARGS_IN_DELEGATE)
            {
                if (throwException)
                {
                    throw new ArgumentException(
                        $"Event handler cannot have more than {MAX_ARGS_IN_DELEGATE} parameters");
                }

                return false;
            }

            foreach (var pi in pars)
            {
                if (Framework.Do.IsValueType(pi.ParameterType))
                {
                    if (throwException)
                    {
                        throw new ArgumentException("Event handler cannot have value type parameters");
                    }

                    return false;
                }

                if (pi.ParameterType.IsByRef)
                {
                    if (throwException)
                    {
                        throw new ArgumentException("Event handler cannot have by-ref type parameters");
                    }

                    return false;
                }
            }

            return true;
        }


        internal DynValue AddCallback(object o, ScriptExecutionContext context, CallbackArguments args)
        {
            lock (m_Lock)
            {
                var closure = args.AsType(0,
                    $"userdata<{this.EventInfo.DeclaringType}>.{this.EventInfo.Name}.add",
                    DataType.Function).Function;

                if (m_Callbacks.Add(o, closure))
                {
                    this.RegisterCallback(o);
                }

                return DynValue.Void;
            }
        }

        internal DynValue RemoveCallback(object o, ScriptExecutionContext context, CallbackArguments args)
        {
            lock (m_Lock)
            {
                var closure = args.AsType(0,
                    $"userdata<{this.EventInfo.DeclaringType}>.{this.EventInfo.Name}.remove",
                    DataType.Function).Function;

                if (m_Callbacks.RemoveValue(o, closure))
                {
                    this.UnregisterCallback(o);
                }

                return DynValue.Void;
            }
        }

        private void RegisterCallback(object o)
        {
            m_Delegates.GetOrCreate(o, () =>
            {
                var d = this.CreateDelegate(o);
#if NETFX_CORE
					Delegate handler = d.GetMethodInfo().CreateDelegate(EventInfo.EventHandlerType, d.Target);
#else
                var handler = Delegate.CreateDelegate(this.EventInfo.EventHandlerType, d.Target, d.Method);
#endif
                m_Add.Invoke(o, new object[] {handler});
                return handler;
            });
        }

        private void UnregisterCallback(object o)
        {
            var handler = m_Delegates.GetOrDefault(o);

            if (handler == null)
            {
                throw new InternalErrorException("can't unregister null delegate");
            }

            m_Delegates.Remove(o);
            m_Remove.Invoke(o, new object[] {handler});
        }


        private Delegate CreateDelegate(object sender)
        {
            switch (Framework.Do.GetMethod(this.EventInfo.EventHandlerType, "Invoke").GetParameters().Length)
            {
                case 0:
                    return (EventWrapper00) (() => this.DispatchEvent(sender));
                case 1:
                    return (EventWrapper01) (o1 => this.DispatchEvent(sender, o1));
                case 2:
                    return (EventWrapper02) ((o1, o2) => this.DispatchEvent(sender, o1, o2));
                case 3:
                    return (EventWrapper03) ((o1, o2, o3) => this.DispatchEvent(sender, o1, o2, o3));
                case 4:
                    return (EventWrapper04) ((o1, o2, o3, o4) => this.DispatchEvent(sender, o1, o2, o3, o4));
                case 5:
                    return (EventWrapper05) ((o1, o2, o3, o4, o5) => this.DispatchEvent(sender, o1, o2, o3, o4, o5));
                case 6:
                    return (EventWrapper06) ((o1, o2, o3, o4, o5, o6) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6));
                case 7:
                    return (EventWrapper07) ((o1, o2, o3, o4, o5, o6, o7) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7));
                case 8:
                    return (EventWrapper08) ((o1, o2, o3, o4, o5, o6, o7, o8) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7, o8));
                case 9:
                    return (EventWrapper09) ((o1, o2, o3, o4, o5, o6, o7, o8, o9) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7, o8, o9));
                case 10:
                    return (EventWrapper10) ((o1, o2, o3, o4, o5, o6, o7, o8, o9, o10) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10));
                case 11:
                    return (EventWrapper11) ((o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11));
                case 12:
                    return (EventWrapper12) ((o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12));
                case 13:
                    return (EventWrapper13) ((o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13));
                case 14:
                    return (EventWrapper14) ((o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13, o14) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13, o14));
                case 15:
                    return (EventWrapper15) ((o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13, o14, o15) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13, o14, o15));
                case 16:
                    return (EventWrapper16) ((o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13, o14, o15, o16) =>
                        this.DispatchEvent(sender, o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13, o14, o15,
                            o16));
                default:
                    throw new InternalErrorException("too many args in delegate type");
            }
        }

        private void DispatchEvent(object sender,
            object o01 = null, object o02 = null, object o03 = null, object o04 = null,
            object o05 = null, object o06 = null, object o07 = null, object o08 = null,
            object o09 = null, object o10 = null, object o11 = null, object o12 = null,
            object o13 = null, object o14 = null, object o15 = null, object o16 = null)
        {
            Closure[] closures;
            lock (m_Lock)
            {
                closures = m_Callbacks.Find(sender).ToArray();
            }

            foreach (var c in closures)
            {
                c.Call(o01, o02, o03, o04, o05, o06, o07, o08, o09, o10, o11, o12, o13, o14, o15, o16);
            }
        }

        private delegate void EventWrapper00();

        private delegate void EventWrapper01(object o1);

        private delegate void EventWrapper02(object o1, object o2);

        private delegate void EventWrapper03(object o1, object o2, object o3);

        private delegate void EventWrapper04(object o1, object o2, object o3, object o4);

        private delegate void EventWrapper05(object o1, object o2, object o3, object o4, object o5);

        private delegate void EventWrapper06(object o1, object o2, object o3, object o4, object o5, object o6);

        private delegate void EventWrapper07(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7);

        private delegate void EventWrapper08(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7, object o8);

        private delegate void EventWrapper09(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7, object o8, object o9);

        private delegate void EventWrapper10(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7, object o8, object o9, object o10);

        private delegate void EventWrapper11(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7, object o8, object o9, object o10, object o11);

        private delegate void EventWrapper12(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7, object o8, object o9, object o10, object o11, object o12);

        private delegate void EventWrapper13(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7, object o8, object o9, object o10, object o11, object o12, object o13);

        private delegate void EventWrapper14(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7, object o8, object o9, object o10, object o11, object o12, object o13, object o14);

        private delegate void EventWrapper15(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7, object o8, object o9, object o10, object o11, object o12, object o13, object o14, object o15);

        private delegate void EventWrapper16(object o1, object o2, object o3, object o4, object o5, object o6,
            object o7, object o8, object o9, object o10, object o11, object o12, object o13, object o14, object o15,
            object o16);
    }
}