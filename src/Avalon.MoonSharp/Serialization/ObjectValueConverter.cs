using System;
using System.Collections;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Serialization
{
    public static class ObjectValueConverter
    {
        public static DynValue SerializeObjectToDynValue(Script script, object o, DynValue valueForNulls = null)
        {
            if (o == null)
            {
                return valueForNulls ?? DynValue.Nil;
            }

            var v = ClrToScriptConversions.TryObjectToTrivialDynValue(script, o);

            if (v != null)
            {
                return v;
            }

            if (o is Enum)
            {
                return DynValue.NewNumber(NumericConversions.TypeToDouble(Enum.GetUnderlyingType(o.GetType()), o));
            }

            var t = new Table(script);

            if (o is IEnumerable ienum)
            {
                foreach (var obj in ienum)
                {
                    t.Append(SerializeObjectToDynValue(script, obj, valueForNulls));
                }
            }
            else
            {
                var type = o.GetType();

                foreach (var pi in Framework.Do.GetProperties(type))
                {
                    var getter = Framework.Do.GetGetMethod(pi);
                    bool isStatic = getter.IsStatic;
                    var obj = getter.Invoke(isStatic ? null : o,
                        null); // convoluted workaround for --full-aot Mono execution

                    t.Set(pi.Name, SerializeObjectToDynValue(script, obj, valueForNulls));
                }
            }

            return DynValue.NewTable(t);
        }
    }
}