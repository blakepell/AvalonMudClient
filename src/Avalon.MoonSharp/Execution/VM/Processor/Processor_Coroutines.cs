using System;

namespace MoonSharp.Interpreter.Execution.VM
{
    // This part is practically written procedural style - it looks more like C than C#.
    // This is intentional so to avoid this-calls and virtual-calls as much as possible.
    // Same reason for the "sealed" declaration.
    internal sealed partial class Processor
    {
        public CoroutineState State { get; private set; }

        public Coroutine AssociatedCoroutine { get; set; }

        public DynValue Coroutine_Create(Closure closure)
        {
            // Create a processor instance
            var p = new Processor(this);

            // Put the closure as first value on the stack, for future reference
            p._valueStack.Push(DynValue.NewClosure(closure));

            // Return the coroutine handle
            return DynValue.NewCoroutine(new Coroutine(p));
        }

        public DynValue Coroutine_Resume(DynValue[] args)
        {
            this.EnterProcessor();

            try
            {
                int entryPoint = 0;

                if (this.State != CoroutineState.NotStarted && this.State != CoroutineState.Suspended &&
                    this.State != CoroutineState.ForceSuspended)
                {
                    throw ScriptRuntimeException.CannotResumeNotSuspended(this.State);
                }

                if (this.State == CoroutineState.NotStarted)
                {
                    entryPoint = this.PushClrToScriptStackFrame(CallStackItemFlags.ResumeEntryPoint, null, args);
                }
                else if (this.State == CoroutineState.Suspended)
                {
                    _valueStack.Push(DynValue.NewTuple(args));
                    entryPoint = _savedInstructionPtr;
                }
                else if (this.State == CoroutineState.ForceSuspended)
                {
                    if (args != null && args.Length > 0)
                    {
                        throw new ArgumentException("When resuming a force-suspended coroutine, args must be empty.");
                    }

                    entryPoint = _savedInstructionPtr;
                }

                this.State = CoroutineState.Running;
                var retVal = this.Processing_Loop(ExecutionControlToken.Dummy, entryPoint);

                if (retVal.Type == DataType.YieldRequest)
                {
                    if (retVal.YieldRequest.Forced)
                    {
                        this.State = CoroutineState.ForceSuspended;
                        return retVal;
                    }
                    else
                    {
                        this.State = CoroutineState.Suspended;
                        return DynValue.NewTuple(retVal.YieldRequest.ReturnValues);
                    }
                }
                else
                {
                    this.State = CoroutineState.Dead;
                    return retVal;
                }
            }
            catch (Exception)
            {
                // Unhandled exception - move to dead
                this.State = CoroutineState.Dead;
                throw;
            }
            finally
            {
                this.LeaveProcessor();
            }
        }
    }
}