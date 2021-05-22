using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
    internal class CallStackItem
    {
        public int BasePointer;
        public SourceRef CallingSourceRef;
        public ClosureContext ClosureScope;
        public CallbackFunction ClrFunction;
        public CallbackFunction Continuation;
        public int Debug_EntryPoint;
        public SymbolRef[] Debug_Symbols;
        public CallbackFunction ErrorHandler;
        public DynValue ErrorHandlerBeforeUnwind;
        public CallStackItemFlags Flags;
        public DynValue[] LocalScope;
        public int ReturnAddress;
    }
}