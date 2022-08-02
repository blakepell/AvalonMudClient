using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Debugging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MoonSharp.Interpreter.Execution.VM
{
    internal sealed partial class Processor
    {
        /// <summary>
        /// The size of the stack.
        /// </summary>
        const int STACK_SIZE = 131072;

        private bool _canYield = true;
        private List<Processor> _coroutinesStack;
        private FastStack<CallStackItem> _executionStack = new(STACK_SIZE);
        private Table _globalTable;
        private int _owningThreadID = -1;
        private Processor _parent;
        private ByteCode _rootChunk;
        private int _savedInstructionPtr = -1;
        private Script _script;
        private FastStack<DynValue> _valueStack = new(STACK_SIZE);

        public Processor(Script script, Table globalContext, ByteCode byteCode)
        {
            _coroutinesStack = new List<Processor>();
            _rootChunk = byteCode;
            _globalTable = globalContext;
            _script = script;
            this.State = CoroutineState.Main;
            DynValue.NewCoroutine(new Coroutine(this)); // creates an associated coroutine for the main processor
        }

        private Processor(Processor parentProcessor)
        {
            _rootChunk = parentProcessor._rootChunk;
            _globalTable = parentProcessor._globalTable;
            _script = parentProcessor._script;
            _parent = parentProcessor;
            this.State = CoroutineState.NotStarted;
        }


        public DynValue Call(ExecutionControlToken ecToken, DynValue function, DynValue[] args)
        {
            var coroutinesStack = _parent != null ? _parent._coroutinesStack : _coroutinesStack;

            if (coroutinesStack.Count > 0 && coroutinesStack[^1] != this)
            {
                return coroutinesStack[^1].Call(ecToken, function, args);
            }

            this.EnterProcessor();

            try
            {
                _canYield = false;

                try
                {
                    int entrypoint = this.PushClrToScriptStackFrame(CallStackItemFlags.CallEntryPoint, function, args);
                    return this.Processing_Loop(ecToken, entrypoint);
                }
                finally
                {
                    _canYield = true;
                }
            }
            finally
            {
                this.LeaveProcessor();
            }
        }

        internal Instruction FindMeta(ref int baseAddress)
        {
            Instruction meta = _rootChunk.Code[baseAddress];

            // skip nops
            while (meta.OpCode == OpCode.Nop)
            {
                baseAddress++;
                meta = _rootChunk.Code[baseAddress];
            }

            if (meta.OpCode != OpCode.Meta)
                return null;

            return meta;
        }

        // pushes all what's required to perform a clr-to-script function call. function can be null if it's already
        // at vstack top.
        private int PushClrToScriptStackFrame(CallStackItemFlags flags, DynValue function, DynValue[] args)
        {
            if (function == null)
            {
                function = _valueStack.Peek();
            }
            else
            {
                _valueStack.Push(function); // func val
            }

            args = this.Internal_AdjustTuple(args);

            for (int i = 0; i < args.Length; i++)
            {
                _valueStack.Push(args[i]);
            }

            _valueStack.Push(DynValue.NewNumber(args.Length)); // func args count

            _executionStack.Push(new CallStackItem
            {
                BasePointer = _valueStack.Count,
                Debug_EntryPoint = function.Function.EntryPointByteCodeLocation,
                ReturnAddress = -1,
                ClosureScope = function.Function.ClosureContext,
                CallingSourceRef = SourceRef.GetClrLocation(),
                Flags = flags
            });

            return function.Function.EntryPointByteCodeLocation;
        }

        private void LeaveProcessor()
        {
            _owningThreadID = -1;
            _parent?._coroutinesStack.RemoveAt(_parent._coroutinesStack.Count - 1);
        }

        private int GetThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }

        private void EnterProcessor()
        {
            int threadID = this.GetThreadId();

            if (_owningThreadID >= 0 && _owningThreadID != threadID && _script.Options.CheckThreadAccess)
            {
                string msg = $"Cannot enter the same MoonSharp processor from two different threads : {_owningThreadID} and {threadID}";
                throw new InvalidOperationException(msg);
            }

            _owningThreadID = threadID;
            _parent?._coroutinesStack.Add(this);
        }

        internal SourceRef GetCoroutineSuspendedLocation()
        {
            return this.GetCurrentSourceRef(_savedInstructionPtr);
        }
    }
}