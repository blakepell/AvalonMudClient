using System.Collections;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop
{
    /// <summary>
    /// Wrappers for enumerables as return types
    /// </summary>
    internal class EnumerableWrapper : IUserDataType
    {
        private IEnumerator m_Enumerator;
        private bool m_HasTurnOnce;
        private DynValue m_Prev = DynValue.Nil;
        private Script m_Script;

        private EnumerableWrapper(Script script, IEnumerator enumerator)
        {
            m_Script = script;
            m_Enumerator = enumerator;
        }


        public DynValue Index(ExecutionControlToken ecToken, Script script, DynValue index, bool isDirectIndexing)
        {
            if (index.Type == DataType.String)
            {
                string idx = index.String;

                if (idx == "Current" || idx == "current")
                {
                    return DynValue.FromObject(script, m_Enumerator.Current);
                }

                if (idx == "MoveNext" || idx == "moveNext" || idx == "move_next")
                {
                    return DynValue.NewCallback((ctx, args) => DynValue.NewBoolean(m_Enumerator.MoveNext()));
                }

                if (idx == "Reset" || idx == "reset")
                {
                    return DynValue.NewCallback((ctx, args) =>
                    {
                        this.Reset();
                        return DynValue.Nil;
                    });
                }
            }

            return null;
        }

        public bool SetIndex(ExecutionControlToken ecToken, Script script, DynValue index, DynValue value,
            bool isDirectIndexing)
        {
            return false;
        }

        public DynValue MetaIndex(ExecutionControlToken ecToken, Script script, string metaname)
        {
            if (metaname == "__call")
            {
                return DynValue.NewCallback(this.LuaIteratorCallback);
            }

            return null;
        }

        public void Reset()
        {
            if (m_HasTurnOnce)
            {
                m_Enumerator.Reset();
            }

            m_HasTurnOnce = true;
        }

        private DynValue GetNext(DynValue prev)
        {
            if (prev.IsNil())
            {
                this.Reset();
            }

            while (m_Enumerator.MoveNext())
            {
                var v = ClrToScriptConversions.ObjectToDynValue(m_Script, m_Enumerator.Current);

                if (!v.IsNil())
                {
                    return v;
                }
            }

            return DynValue.Nil;
        }

        private DynValue LuaIteratorCallback(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            m_Prev = this.GetNext(m_Prev);
            return m_Prev;
        }

        internal static DynValue ConvertIterator(Script script, IEnumerator enumerator)
        {
            var ei = new EnumerableWrapper(script, enumerator);
            return DynValue.NewTuple(UserData.Create(ei), DynValue.Nil, DynValue.Nil);
        }

        internal static DynValue ConvertTable(Table table)
        {
            return ConvertIterator(table.OwnerScript, table.Values.GetEnumerator());
        }
    }
}