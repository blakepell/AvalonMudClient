using System.Collections.Generic;

namespace MoonSharp.Interpreter.Execution.VM
{
    internal sealed partial class Processor
    {
        private DynValue[] Internal_AdjustTuple(IList<DynValue> values)
        {
            if (values == null || values.Count == 0)
            {
                return System.Array.Empty<DynValue>();
            }

            if (values[^1].Type == DataType.Tuple)
            {
                int baseLen = values.Count - 1 + values[^1].Tuple.Length;
                var result = new DynValue[baseLen];

                for (int i = 0; i < values.Count - 1; i++)
                {
                    result[i] = values[i].ToScalar();
                }

                for (int i = 0; i < values[^1].Tuple.Length; i++)
                {
                    result[values.Count + i - 1] = values[^1].Tuple[i];
                }

                if (result[^1].Type == DataType.Tuple)
                {
                    return this.Internal_AdjustTuple(result);
                }

                return result;
            }
            else
            {
                var result = new DynValue[values.Count];

                for (int i = 0; i < values.Count; i++)
                {
                    result[i] = values[i].ToScalar();
                }

                return result;
            }
        }


        private int Internal_InvokeUnaryMetaMethod(ExecutionControlToken ecToken, DynValue op1, string eventName,
            int instructionPtr)
        {
            DynValue m = null;

            if (op1.Type == DataType.UserData)
            {
                m = op1.UserData.Descriptor.MetaIndex(ecToken, _script, op1.UserData.Object, eventName);
            }

            if (m == null)
            {
                var op1_MetaTable = this.GetMetatable(op1);

                var meta1 = op1_MetaTable?.RawGet(eventName);
                if (meta1 != null && meta1.IsNotNil())
                {
                    m = meta1;
                }
            }

            if (m != null)
            {
                _valueStack.Push(m);
                _valueStack.Push(op1);
                return this.Internal_ExecCall(ecToken, 1, instructionPtr);
            }

            return -1;
        }

        private int Internal_InvokeBinaryMetaMethod(ExecutionControlToken ecToken, DynValue l, DynValue r,
            string eventName, int instructionPtr, DynValue extraPush = null)
        {
            var m = this.GetBinaryMetamethod(ecToken, l, r, eventName);

            if (m != null)
            {
                if (extraPush != null)
                {
                    _valueStack.Push(extraPush);
                }

                _valueStack.Push(m);
                _valueStack.Push(l);
                _valueStack.Push(r);
                return this.Internal_ExecCall(ecToken, 2, instructionPtr);
            }

            return -1;
        }
    }
}