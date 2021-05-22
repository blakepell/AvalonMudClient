namespace MoonSharp.Interpreter.Execution.VM
{
    internal sealed partial class Processor
    {
        internal Table GetMetatable(DynValue value)
        {
            if (value.Type == DataType.Table)
            {
                return value.Table.MetaTable;
            }

            if (value.Type.CanHaveTypeMetatables())
            {
                return _script.GetTypeMetatable(value.Type);
            }

            return null;
        }

        internal DynValue GetBinaryMetamethod(ExecutionControlToken ecToken, DynValue op1, DynValue op2,
            string eventName)
        {
            var op1_MetaTable = this.GetMetatable(op1);
            if (op1_MetaTable != null)
            {
                var meta1 = op1_MetaTable.RawGet(eventName);
                if (meta1 != null && meta1.IsNotNil())
                {
                    return meta1;
                }
            }

            var op2_MetaTable = this.GetMetatable(op2);
            if (op2_MetaTable != null)
            {
                var meta2 = op2_MetaTable.RawGet(eventName);
                if (meta2 != null && meta2.IsNotNil())
                {
                    return meta2;
                }
            }

            if (op1.Type == DataType.UserData)
            {
                var meta = op1.UserData.Descriptor.MetaIndex(ecToken, _script,
                    op1.UserData.Object, eventName);

                if (meta != null)
                {
                    return meta;
                }
            }

            if (op2.Type == DataType.UserData)
            {
                var meta = op2.UserData.Descriptor.MetaIndex(ecToken, _script,
                    op2.UserData.Object, eventName);

                if (meta != null)
                {
                    return meta;
                }
            }

            return null;
        }

        internal DynValue GetMetamethod(ExecutionControlToken ecToken, DynValue value, string metamethod)
        {
            if (value.Type == DataType.UserData)
            {
                var v = value.UserData.Descriptor.MetaIndex(ecToken, _script, value.UserData.Object, metamethod);
                if (v != null)
                {
                    return v;
                }
            }

            return this.GetMetamethodRaw(value, metamethod);
        }


        internal DynValue GetMetamethodRaw(DynValue value, string metamethod)
        {
            var metatable = this.GetMetatable(value);

            if (metatable == null)
            {
                return null;
            }

            var metameth = metatable.RawGet(metamethod);

            if (metameth == null || metameth.IsNil())
            {
                return null;
            }

            return metameth;
        }

        internal Script GetScript()
        {
            return _script;
        }
    }
}