using System;

namespace MoonSharp.Interpreter.Execution.VM
{
    internal sealed partial class Processor
    {
        private void ClearBlockData(Instruction I)
        {
            int from = I.NumVal;
            int to = I.NumVal2;

            var array = _executionStack.Peek().LocalScope;

            if (to >= 0 && from >= 0 && to >= from)
            {
                Array.Clear(array, from, to - from + 1);
            }
        }


        public DynValue GetGenericSymbol(SymbolRef symref)
        {
            switch (symref._type)
            {
                case SymbolRefType.DefaultEnv:
                    return DynValue.NewTable(this.GetScript().Globals);
                case SymbolRefType.Local:
                    return this.GetTopNonClrFunction().LocalScope[symref._index];
                case SymbolRefType.Upvalue:
                    return this.GetTopNonClrFunction().ClosureScope[symref._index];
                default:
                    throw new InternalErrorException("Unexpected {0} LRef at resolution: {1}", symref._type,
                        symref._name);
            }
        }

        private CallStackItem GetTopNonClrFunction()
        {
            CallStackItem stackframe = null;

            for (int i = 0; i < _executionStack.Count; i++)
            {
                stackframe = _executionStack.Peek(i);

                if (stackframe.ClrFunction == null)
                {
                    break;
                }
            }

            return stackframe;
        }


        public SymbolRef FindSymbolByName(string name)
        {
            if (_executionStack.Count > 0)
            {
                var stackframe = this.GetTopNonClrFunction();

                if (stackframe != null)
                {
                    if (stackframe.Debug_Symbols != null)
                    {
                        for (int i = stackframe.Debug_Symbols.Length - 1; i >= 0; i--)
                        {
                            var l = stackframe.Debug_Symbols[i];

                            if (l._name == name && stackframe.LocalScope[i] != null)
                            {
                                return l;
                            }
                        }
                    }

                    var closure = stackframe.ClosureScope;

                    if (closure != null)
                    {
                        for (int i = 0; i < closure.Symbols.Length; i++)
                        {
                            if (closure.Symbols[i] == name)
                            {
                                return SymbolRef.Upvalue(name, i);
                            }
                        }
                    }
                }
            }

            if (name != WellKnownSymbols.ENV)
            {
                var env = this.FindSymbolByName(WellKnownSymbols.ENV);
                return SymbolRef.Global(name, env);
            }

            return SymbolRef.DefaultEnv;
        }
    }
}