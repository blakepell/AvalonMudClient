using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Interop.Converters
{
    internal static class ClrToScriptConversions
    {
        /// <summary>
        /// Tries to convert a CLR object to a MoonSharp value, using "trivial" logic.
        /// Skips on custom conversions, etc.
        /// Does NOT throw on failure.
        /// </summary>
        internal static DynValue TryObjectToTrivialDynValue(Script script, object obj)
        {
            if (obj == null)
            {
                return DynValue.Nil;
            }

            if (obj is DynValue value)
            {
                return value;
            }

            var t = obj.GetType();

            if (obj is bool b)
            {
                return DynValue.NewBoolean(b);
            }

            if (obj is string || obj is StringBuilder || obj is char)
            {
                return DynValue.NewString(obj.ToString());
            }

            if (NumericConversions.NumericTypes.Contains(t))
            {
                return DynValue.NewNumber(NumericConversions.TypeToDouble(t, obj));
            }

            if (obj is Table table)
            {
                return DynValue.NewTable(table);
            }

            return null;
        }


        /// <summary>
        /// Tries to convert a CLR object to a MoonSharp value, using "simple" logic.
        /// Does NOT throw on failure.
        /// </summary>
        internal static DynValue TryObjectToSimpleDynValue(Script script, object obj)
        {
            if (obj == null)
            {
                return DynValue.Nil;
            }

            if (obj is DynValue value)
            {
                return value;
            }


            var converter = Script.GlobalOptions.CustomConverters.GetClrToScriptCustomConversion(obj.GetType());
            if (converter != null)
            {
                var v = converter(script, obj);
                if (v != null)
                {
                    return v;
                }
            }

            var t = obj.GetType();

            if (obj is bool b)
            {
                return DynValue.NewBoolean(b);
            }

            if (obj is string || obj is StringBuilder || obj is char)
            {
                return DynValue.NewString(obj.ToString());
            }

            if (obj is Closure closure)
            {
                return DynValue.NewClosure(closure);
            }

            if (NumericConversions.NumericTypes.Contains(t))
            {
                return DynValue.NewNumber(NumericConversions.TypeToDouble(t, obj));
            }

            if (obj is Table table)
            {
                return DynValue.NewTable(table);
            }

            if (obj is CallbackFunction function)
            {
                return DynValue.NewCallback(function);
            }

            if (obj is Delegate d)
            {
                var mi = d.Method;

                if (CallbackFunction.CheckCallbackSignature(mi, false))
                {
                    return DynValue.NewCallback((Func<ScriptExecutionContext, CallbackArguments, DynValue>) d);
                }
            }

            return null;
        }


        /// <summary>
        /// Tries to convert a CLR object to a MoonSharp value, using more in-depth analysis
        /// </summary>
        internal static DynValue ObjectToDynValue(Script script, object obj)
        {
            var v = TryObjectToSimpleDynValue(script, obj);

            if (v != null)
            {
                return v;
            }

            v = UserData.Create(obj);
            if (v != null)
            {
                return v;
            }

            if (obj is Type type)
            {
                v = UserData.CreateStatic(type);
            }

            // unregistered enums go as integers
            if (obj is Enum)
            {
                return DynValue.NewNumber(NumericConversions.TypeToDouble(Enum.GetUnderlyingType(obj.GetType()), obj));
            }

            if (v != null)
            {
                return v;
            }

            if (obj is Delegate d)
            {
                return DynValue.NewCallback(CallbackFunction.FromDelegate(script, d));
            }

            if (obj is MethodInfo { IsStatic: true } mi)
            {
                return DynValue.NewCallback(CallbackFunction.FromMethodInfo(script, mi));
            }

            if (obj is IList list)
            {
                var t = TableConversions.ConvertIListToTable(script, list);
                return DynValue.NewTable(t);
            }

            if (obj is IDictionary dict)
            {
                var t = TableConversions.ConvertIDictionaryToTable(script, dict);
                return DynValue.NewTable(t);
            }

            var enumerator = EnumerationToDynValue(script, obj);
            if (enumerator != null)
            {
                return enumerator;
            }


            throw ScriptRuntimeException.ConvertObjectFailed(obj);
        }

        /// <summary>
        /// Converts an IEnumerable or IEnumerator to a DynValue
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="obj">The object.</param>
        public static DynValue EnumerationToDynValue(Script script, object obj)
        {
            if (obj is IEnumerable ie)
            {
                return EnumerableWrapper.ConvertIterator(script, ie.GetEnumerator());
            }

            if (obj is IEnumerator ienum)
            {
                return EnumerableWrapper.ConvertIterator(script, ienum);
            }

            return null;
        }
    }
}