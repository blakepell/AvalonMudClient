using System;
using System.Collections.Generic;
using System.Reflection;
using MoonSharp.Interpreter.Interop;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// This class wraps a CLR function 
    /// </summary>
    public sealed class CallbackFunction : RefIdObject
    {
        private static InteropAccessMode _defaultAccessMode = InteropAccessMode.LazyOptimized;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackFunction" /> class.
        /// </summary>
        /// <param name="callBack">The callback function to be called.</param>
        /// <param name="name">The callback name, used in stacktraces, debugger, etc..</param>
        public CallbackFunction(Func<ScriptExecutionContext, CallbackArguments, DynValue> callBack, string name = null)
        {
            this.ClrCallback = callBack;
            this.Name = name;
        }

        /// <summary>
        /// Gets the name of the function
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the call back.
        /// </summary>
        /// <value>
        /// The call back.
        /// </value>
        public Func<ScriptExecutionContext, CallbackArguments, DynValue> ClrCallback { get; }

        /// <summary>
        /// Gets or sets the default access mode used when marshalling delegates
        /// </summary>
        /// <value>
        /// The default access mode. Default, HideMembers and BackgroundOptimized are NOT supported.
        /// </value>
        /// <exception cref="System.ArgumentException">Default, HideMembers and BackgroundOptimized are NOT supported.</exception>
        public static InteropAccessMode DefaultAccessMode
        {
            get => _defaultAccessMode;
            set
            {
                if (value == InteropAccessMode.Default || value == InteropAccessMode.HideMembers ||
                    value == InteropAccessMode.BackgroundOptimized)
                {
                    throw new ArgumentException("DefaultAccessMode");
                }

                _defaultAccessMode = value;
            }
        }


        /// <summary>
        /// Gets or sets an object used as additional data to the callback function (available in the execution context).
        /// </summary>
        public object AdditionalData { get; set; }

        /// <summary>
        /// Invokes the callback function
        /// </summary>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="isMethodCall">if set to <c>true</c> this is a method call.</param>
        public DynValue Invoke(ScriptExecutionContext executionContext, IList<DynValue> args, bool isMethodCall = false)
        {
            if (isMethodCall)
            {
                var colon = executionContext.GetScript().Options.ColonOperatorClrCallbackBehaviour;

                if (colon == ColonOperatorBehaviour.TreatAsColon)
                {
                    isMethodCall = false;
                }
                else if (colon == ColonOperatorBehaviour.TreatAsDotOnUserData)
                {
                    isMethodCall = (args.Count > 0 && args[0].Type == DataType.UserData);
                }
            }

            return this.ClrCallback(executionContext, new CallbackArguments(args, isMethodCall));
        }

        /// <summary>
        /// Creates a CallbackFunction from a delegate.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="del">The delegate.</param>
        /// <param name="accessMode">The access mode.</param>
        public static CallbackFunction FromDelegate(Script script, Delegate del,
            InteropAccessMode accessMode = InteropAccessMode.Default)
        {
            if (accessMode == InteropAccessMode.Default)
            {
                accessMode = _defaultAccessMode;
            }

            var descr = new MethodMemberDescriptor(del.Method, accessMode);
            return descr.GetCallbackFunction(script, del.Target);
        }


        /// <summary>
        /// Creates a CallbackFunction from a MethodInfo relative to a function.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="mi">The MethodInfo object.</param>
        /// <param name="obj">The object to which the function applies, or null for static methods.</param>
        /// <param name="accessMode">The access mode.</param>
        /// <exception cref="System.ArgumentException">The method is not static.</exception>
        public static CallbackFunction FromMethodInfo(Script script, MethodInfo mi, object obj = null,
            InteropAccessMode accessMode = InteropAccessMode.Default)
        {
            if (accessMode == InteropAccessMode.Default)
            {
                accessMode = _defaultAccessMode;
            }

            var descr = new MethodMemberDescriptor(mi, accessMode);
            return descr.GetCallbackFunction(script, obj);
        }


        /// <summary>
        /// Checks the callback signature of a method is compatible for callbacks
        /// </summary>
        public static bool CheckCallbackSignature(MethodInfo mi, bool requirePublicVisibility)
        {
            var pi = mi.GetParameters();

            return (pi.Length == 2 && pi[0].ParameterType == typeof(ScriptExecutionContext)
                                   && pi[1].ParameterType == typeof(CallbackArguments) &&
                                   mi.ReturnType == typeof(DynValue) && (requirePublicVisibility || mi.IsPublic));
        }
    }
}