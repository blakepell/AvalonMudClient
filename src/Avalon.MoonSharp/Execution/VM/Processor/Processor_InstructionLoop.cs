using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Interop;

namespace MoonSharp.Interpreter.Execution.VM
{
    internal sealed partial class Processor
    {
        private const int YIELD_SPECIAL_TRAP = -99;

        internal long AutoYieldCounter = 0;

        /// <summary>
        /// This is the main loop of the processor, has a weird control flow and needs to be as fast as possible.
        /// This sentence is just a convoluted way to say "don't complain about gotos".
        /// </summary>
        /// <param name="ecToken"></param>
        /// <param name="instructionPtr"></param>
        private DynValue Processing_Loop(ExecutionControlToken ecToken, int instructionPtr)
        {
            long executedInstructions = 0;
            bool canAutoYield = (AutoYieldCounter > 0) && _canYield && (this.State != CoroutineState.Main);

        repeat_execution:

            try
            {
                while (true)
                {
                    var i = _rootChunk.Code[instructionPtr];

                    ++executedInstructions;

                    if (canAutoYield && executedInstructions > AutoYieldCounter)
                    {
                        _savedInstructionPtr = instructionPtr;
                        return DynValue.NewForcedYieldReq();
                    }

                    if (ecToken.IsAbortRequested)
                    {
                        throw new ScriptTerminationRequestedException();
                    }

                    ++instructionPtr;

                    switch (i.OpCode)
                    {
                        case OpCode.Nop:
                        case OpCode.Debug:
                        case OpCode.Meta:
                            break;
                        case OpCode.Pop:
                            _valueStack.RemoveLast(i.NumVal);
                            break;
                        case OpCode.Copy:
                            _valueStack.Push(_valueStack.Peek(i.NumVal));
                            break;
                        case OpCode.Swap:
                            this.ExecSwap(i);
                            break;
                        case OpCode.Literal:
                            _valueStack.Push(i.Value);
                            break;
                        case OpCode.Add:
                            instructionPtr = this.ExecAdd(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Concat:
                            instructionPtr = this.ExecConcat(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Neg:
                            instructionPtr = this.ExecNeg(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Sub:
                            instructionPtr = this.ExecSub(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Mul:
                            instructionPtr = this.ExecMul(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Div:
                            instructionPtr = this.ExecDiv(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Mod:
                            instructionPtr = this.ExecMod(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Power:
                            instructionPtr = this.ExecPower(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Eq:
                            instructionPtr = this.ExecEq(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.LessEq:
                            instructionPtr = this.ExecLessEq(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Less:
                            instructionPtr = this.ExecLess(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Len:
                            instructionPtr = this.ExecLen(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Call:
                        case OpCode.ThisCall:
                            instructionPtr = this.Internal_ExecCall(ecToken, i.NumVal, instructionPtr, null, null,
                                i.OpCode == OpCode.ThisCall, i.Name);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Scalar:
                            _valueStack.Push(_valueStack.Pop().ToScalar());
                            break;
                        case OpCode.Not:
                            this.ExecNot(i);
                            break;
                        case OpCode.CNot:
                            this.ExecCNot(i);
                            break;
                        case OpCode.JfOrPop:
                        case OpCode.JtOrPop:
                            instructionPtr = this.ExecShortCircuitingOperator(i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.JNil:
                            {
                                var v = _valueStack.Pop().ToScalar();

                                if (v.Type == DataType.Nil || v.Type == DataType.Void)
                                {
                                    instructionPtr = i.NumVal;
                                }
                            }
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Jf:
                            instructionPtr = this.JumpBool(i, false, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Jump:
                            instructionPtr = i.NumVal;
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.MkTuple:
                            this.ExecMkTuple(i);
                            break;
                        case OpCode.Clean:
                            this.ClearBlockData(i);
                            break;
                        case OpCode.Closure:
                            this.ExecClosure(i);
                            break;
                        case OpCode.BeginFn:
                            this.ExecBeginFn(i);
                            break;
                        case OpCode.ToBool:
                            _valueStack.Push(DynValue.NewBoolean(_valueStack.Pop().ToScalar().CastToBool()));
                            break;
                        case OpCode.Args:
                            this.ExecArgs(i);
                            break;
                        case OpCode.Ret:
                            instructionPtr = this.ExecRet(ecToken, i);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            if (instructionPtr < 0)
                            {
                                goto return_to_native_code;
                            }

                            break;
                        case OpCode.Incr:
                            this.ExecIncr(i);
                            break;
                        case OpCode.ToNum:
                            this.ExecToNum(i);
                            break;
                        case OpCode.JFor:
                            instructionPtr = this.ExecJFor(i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.NewTable:
                            if (i.NumVal == 0)
                            {
                                _valueStack.Push(DynValue.NewTable(_script));
                            }
                            else
                            {
                                _valueStack.Push(DynValue.NewPrimeTable());
                            }

                            break;
                        case OpCode.IterPrep:
                            this.ExecIterPrep(ecToken, i);
                            break;
                        case OpCode.IterUpd:
                            this.ExecIterUpd(i);
                            break;
                        case OpCode.ExpTuple:
                            this.ExecExpTuple(i);
                            break;
                        case OpCode.Local:
                            var scope = _executionStack.Peek().LocalScope;
                            int index = i.Symbol._index;
                            _valueStack.Push(scope[index].AsReadOnly());
                            break;
                        case OpCode.Upvalue:
                            _valueStack.Push(_executionStack.Peek().ClosureScope[i.Symbol._index].AsReadOnly());
                            break;
                        case OpCode.StoreUpv:
                            this.ExecStoreUpv(i);
                            break;
                        case OpCode.StoreLcl:
                            this.ExecStoreLcl(i);
                            break;
                        case OpCode.TblInitN:
                            this.ExecTblInitN(i);
                            break;
                        case OpCode.TblInitI:
                            this.ExecTblInitI(i);
                            break;
                        case OpCode.Index:
                        case OpCode.IndexN:
                        case OpCode.IndexL:
                            instructionPtr = this.ExecIndex(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.IndexSet:
                        case OpCode.IndexSetN:
                        case OpCode.IndexSetL:
                            instructionPtr = this.ExecIndexSet(ecToken, i, instructionPtr);
                            if (instructionPtr == YIELD_SPECIAL_TRAP)
                            {
                                goto yield_to_calling_coroutine;
                            }

                            break;
                        case OpCode.Invalid:
                            throw new NotImplementedException($"Invalid opcode : {i.Name}");
                        default:
                            throw new NotImplementedException($"Execution for {i.OpCode} not implemented yet!");
                    }
                }

            yield_to_calling_coroutine:

                var yieldRequest = _valueStack.Pop().ToScalar();

                if (_canYield)
                {
                    return yieldRequest;
                }

                if (this.State == CoroutineState.Main)
                {
                    throw ScriptRuntimeException.CannotYieldMain();
                }

                throw ScriptRuntimeException.CannotYield();
            }
            catch (InterpreterException ex)
            {
                this.FillDebugData(ex, instructionPtr);

                if (!(ex is ScriptRuntimeException))
                {
                    ex.Rethrow();
                    throw;
                }

                for (int i = 0; i < _executionStack.Count; i++)
                {
                    var c = _executionStack.Peek(i);

                    if (c.ErrorHandlerBeforeUnwind != null)
                    {
                        ex.DecoratedMessage = this.PerformMessageDecorationBeforeUnwind(ecToken,
                            c.ErrorHandlerBeforeUnwind, ex.DecoratedMessage, this.GetCurrentSourceRef(instructionPtr));
                    }
                }


                while (_executionStack.Count > 0)
                {
                    var csi = this.PopToBasePointer();

                    if (csi.ErrorHandler != null)
                    {
                        instructionPtr = csi.ReturnAddress;

                        if (csi.ClrFunction == null)
                        {
                            int argscnt = (int)(_valueStack.Pop().Number);
                            _valueStack.RemoveLast(argscnt + 1);
                        }

                        var cbargs = new[] { DynValue.NewString(ex.DecoratedMessage) };

                        var handled = csi.ErrorHandler.Invoke(
                            new ScriptExecutionContext(ecToken, this, csi.ErrorHandler,
                                this.GetCurrentSourceRef(instructionPtr)), cbargs);

                        _valueStack.Push(handled);

                        goto repeat_execution;
                    }

                    if ((csi.Flags & CallStackItemFlags.EntryPoint) != 0)
                    {
                        ex.Rethrow();
                        throw;
                    }
                }

                ex.Rethrow();
                throw;
            }

        return_to_native_code:
            return _valueStack.Pop();
        }


        internal string PerformMessageDecorationBeforeUnwind(ExecutionControlToken ecToken, DynValue messageHandler, string decoratedMessage, SourceRef sourceRef)
        {
            try
            {
                DynValue[] args = { DynValue.NewString(decoratedMessage) };
                DynValue ret;

                if (messageHandler.Type == DataType.Function)
                {
                    ret = this.Call(ecToken, messageHandler, args);
                }
                else if (messageHandler.Type == DataType.ClrFunction)
                {
                    var ctx = new ScriptExecutionContext(ecToken, this, messageHandler.Callback, sourceRef);
                    ret = messageHandler.Callback.Invoke(ctx, args);
                }
                else
                {
                    throw new ScriptRuntimeException("error handler not set to a function");
                }

                string newMsg = ret.ToPrintString();

                if (newMsg != null)
                {
                    return newMsg;
                }
            }
            catch (ScriptRuntimeException innerEx)
            {
                return $"{innerEx.Message}\n{decoratedMessage}";
            }

            return decoratedMessage;
        }


        private void AssignLocal(SymbolRef symref, DynValue value)
        {
            var stackframe = _executionStack.Peek();

            var v = stackframe.LocalScope[symref._index];
            if (v == null)
            {
                stackframe.LocalScope[symref._index] = v = DynValue.NewNil();
            }

            v.Assign(value);
        }

        private void ExecStoreLcl(Instruction i)
        {
            var value = this.GetStoreValue(i);
            var symref = i.Symbol;

            this.AssignLocal(symref, value);
        }

        private void ExecStoreUpv(Instruction i)
        {
            var value = this.GetStoreValue(i);
            var symref = i.Symbol;

            var stackframe = _executionStack.Peek();

            var v = stackframe.ClosureScope[symref._index];
            if (v == null)
            {
                stackframe.ClosureScope[symref._index] = v = DynValue.NewNil();
            }

            v.Assign(value);
        }

        private void ExecSwap(Instruction i)
        {
            var v1 = _valueStack.Peek(i.NumVal);
            var v2 = _valueStack.Peek(i.NumVal2);

            _valueStack.Set(i.NumVal, v2);
            _valueStack.Set(i.NumVal2, v1);
        }


        private DynValue GetStoreValue(Instruction i)
        {
            int stackofs = i.NumVal;
            int tupleidx = i.NumVal2;

            var v = _valueStack.Peek(stackofs);

            if (v.Type == DataType.Tuple)
            {
                return (tupleidx < v.Tuple.Length) ? v.Tuple[tupleidx] : DynValue.NewNil();
            }

            return (tupleidx == 0) ? v : DynValue.NewNil();
        }

        private void ExecClosure(Instruction i)
        {
            var c = new Closure(_script, i.NumVal, i.SymbolList,
                i.SymbolList.Select(s => this.GetUpvalueSymbol(s)).ToList());

            _valueStack.Push(DynValue.NewClosure(c));
        }

        private DynValue GetUpvalueSymbol(SymbolRef s)
        {
            if (s.Type == SymbolRefType.Local)
            {
                return _executionStack.Peek().LocalScope[s._index];
            }

            if (s.Type == SymbolRefType.Upvalue)
            {
                return _executionStack.Peek().ClosureScope[s._index];
            }

            throw new Exception("unsupported symbol type");
        }

        private void ExecMkTuple(Instruction i)
        {
            var slice = new Slice<DynValue>(_valueStack, _valueStack.Count - i.NumVal, i.NumVal, false);

            var v = this.Internal_AdjustTuple(slice);

            _valueStack.RemoveLast(i.NumVal);

            _valueStack.Push(DynValue.NewTuple(v));
        }

        private void ExecToNum(Instruction i)
        {
            var v = _valueStack.Pop().ToScalar().CastToNumber();
            if (v.HasValue)
            {
                _valueStack.Push(DynValue.NewNumber(v.Value));
            }
            else
            {
                throw ScriptRuntimeException.ConvertToNumberFailed(i.NumVal);
            }
        }


        private void ExecIterUpd(Instruction i)
        {
            var v = _valueStack.Peek();
            var t = _valueStack.Peek(1);
            t.Tuple[2] = v;
        }

        private void ExecExpTuple(Instruction i)
        {
            var t = _valueStack.Peek(i.NumVal);

            if (t.Type == DataType.Tuple)
            {
                for (int idx = 0; idx < t.Tuple.Length; idx++)
                {
                    _valueStack.Push(t.Tuple[idx]);
                }
            }
            else
            {
                _valueStack.Push(t);
            }
        }

        private void ExecIterPrep(ExecutionControlToken ecToken, Instruction i)
        {
            var v = _valueStack.Pop();

            if (v.Type != DataType.Tuple)
            {
                v = DynValue.NewTuple(v, DynValue.Nil, DynValue.Nil);
            }

            var f = v.Tuple.Length >= 1 ? v.Tuple[0] : DynValue.Nil;
            var s = v.Tuple.Length >= 2 ? v.Tuple[1] : DynValue.Nil;
            var var = v.Tuple.Length >= 3 ? v.Tuple[2] : DynValue.Nil;

            // MoonSharp additions - given f, s, var
            // 1) if f is not a function and has a __iterator metamethod, call __iterator to get the triplet
            // 2) if f is a table with no __call metamethod, use a default table iterator

            if (f.Type != DataType.Function && f.Type != DataType.ClrFunction)
            {
                var meta = this.GetMetamethod(ecToken, f, "__iterator");

                if (meta != null && !meta.IsNil())
                {
                    if (meta.Type != DataType.Tuple)
                    {
                        v = this.GetScript().Call(meta, f, s, var);
                    }
                    else
                    {
                        v = meta;
                    }

                    f = v.Tuple.Length >= 1 ? v.Tuple[0] : DynValue.Nil;
                    s = v.Tuple.Length >= 2 ? v.Tuple[1] : DynValue.Nil;
                    var = v.Tuple.Length >= 3 ? v.Tuple[2] : DynValue.Nil;

                    _valueStack.Push(DynValue.NewTuple(f, s, var));
                    return;
                }

                if (f.Type == DataType.Table)
                {
                    var callmeta = this.GetMetamethod(ecToken, f, "__call");

                    if (callmeta == null || callmeta.IsNil())
                    {
                        _valueStack.Push(EnumerableWrapper.ConvertTable(f.Table));
                        return;
                    }
                }
            }

            _valueStack.Push(DynValue.NewTuple(f, s, var));
        }


        private int ExecJFor(Instruction i, int instructionPtr)
        {
            double val = _valueStack.Peek().Number;
            double step = _valueStack.Peek(1).Number;
            double stop = _valueStack.Peek(2).Number;

            bool whileCond = (step > 0) ? val <= stop : val >= stop;

            if (!whileCond)
            {
                return i.NumVal;
            }

            return instructionPtr;
        }


        private void ExecIncr(Instruction i)
        {
            var top = _valueStack.Peek();
            var btm = _valueStack.Peek(i.NumVal);

            if (top.ReadOnly)
            {
                _valueStack.Pop();

                if (top.ReadOnly)
                {
                    top = top.CloneAsWritable();
                }

                _valueStack.Push(top);
            }

            top.AssignNumber(top.Number + btm.Number);
        }


        private void ExecCNot(Instruction i)
        {
            var v = _valueStack.Pop().ToScalar();
            var not = _valueStack.Pop().ToScalar();

            if (not.Type != DataType.Boolean)
            {
                throw new InternalErrorException("CNOT had non-bool arg");
            }

            if (not.CastToBool())
            {
                _valueStack.Push(DynValue.NewBoolean(!(v.CastToBool())));
            }
            else
            {
                _valueStack.Push(DynValue.NewBoolean(v.CastToBool()));
            }
        }

        private void ExecNot(Instruction i)
        {
            var v = _valueStack.Pop().ToScalar();
            _valueStack.Push(DynValue.NewBoolean(!(v.CastToBool())));
        }

        private void ExecBeginFn(Instruction i)
        {
            var cur = _executionStack.Peek();

            cur.Debug_Symbols = i.SymbolList;
            cur.LocalScope = new DynValue[i.NumVal];

            this.ClearBlockData(i);
        }

        private CallStackItem PopToBasePointer()
        {
            var csi = _executionStack.Pop();
            if (csi.BasePointer >= 0)
            {
                _valueStack.CropAtCount(csi.BasePointer);
            }

            return csi;
        }

        private int PopExecStackAndCheckVStack(int vstackguard)
        {
            var xs = _executionStack.Pop();
            if (vstackguard != xs.BasePointer)
            {
                throw new InternalErrorException("StackGuard violation");
            }

            return xs.ReturnAddress;
        }

        private IList<DynValue> CreateArgsListForFunctionCall(int numargs, int offsFromTop)
        {
            if (numargs == 0)
            {
                return new DynValue[0];
            }

            var lastParam = _valueStack.Peek(offsFromTop);

            if (lastParam.Type == DataType.Tuple && lastParam.Tuple.Length > 1)
            {
                var values = new List<DynValue>();

                for (int idx = 0; idx < numargs - 1; idx++)
                {
                    values.Add(_valueStack.Peek(numargs - idx - 1 + offsFromTop));
                }

                for (int idx = 0; idx < lastParam.Tuple.Length; idx++)
                {
                    values.Add(lastParam.Tuple[idx]);
                }

                return values;
            }

            return new Slice<DynValue>(_valueStack, _valueStack.Count - numargs - offsFromTop, numargs, false);
        }


        private void ExecArgs(Instruction I)
        {
            int numargs = (int)_valueStack.Peek().Number;

            // unpacks last tuple arguments to simplify a lot of code down under
            var argsList = this.CreateArgsListForFunctionCall(numargs, 1);

            for (int i = 0; i < I.SymbolList.Length; i++)
            {
                if (i >= argsList.Count)
                {
                    this.AssignLocal(I.SymbolList[i], DynValue.NewNil());
                }
                else if ((i == I.SymbolList.Length - 1) && (I.SymbolList[i]._name == WellKnownSymbols.VARARGS))
                {
                    int len = argsList.Count - i;
                    var varargs = new DynValue[len];

                    for (int ii = 0; ii < len; ii++, i++)
                    {
                        varargs[ii] = argsList[i].ToScalar().CloneAsWritable();
                    }

                    this.AssignLocal(I.SymbolList[I.SymbolList.Length - 1],
                        DynValue.NewTuple(this.Internal_AdjustTuple(varargs)));
                }
                else
                {
                    this.AssignLocal(I.SymbolList[i], argsList[i].ToScalar().CloneAsWritable());
                }
            }
        }


        private int Internal_ExecCall(ExecutionControlToken ecToken, int argsCount, int instructionPtr,
            CallbackFunction handler = null,
            CallbackFunction continuation = null, bool thisCall = false, string debugText = null,
            DynValue unwindHandler = null)
        {
            var fn = _valueStack.Peek(argsCount);
            var flags = (thisCall ? CallStackItemFlags.MethodCall : CallStackItemFlags.None);

            // if TCO threshold reached
            if ((_executionStack.Count > _script.Options.TailCallOptimizationThreshold && _executionStack.Count > 1)
                || (_valueStack.Count > _script.Options.TailCallOptimizationThreshold && _valueStack.Count > 1))
            {
                // and the "will-be" return address is valid (we don't want to crash here)
                if (instructionPtr >= 0 && instructionPtr < _rootChunk.Code.Count)
                {
                    var I = _rootChunk.Code[instructionPtr];

                    // and we are followed *exactly* by a RET 1
                    if (I.OpCode == OpCode.Ret && I.NumVal == 1)
                    {
                        var csi = _executionStack.Peek();

                        // if the current stack item has no "odd" things pending and neither has the new coming one..
                        if (csi.ClrFunction == null && csi.Continuation == null && csi.ErrorHandler == null
                            && csi.ErrorHandlerBeforeUnwind == null && continuation == null && unwindHandler == null &&
                            handler == null)
                        {
                            instructionPtr = this.PerformTCO(instructionPtr, argsCount);
                            flags |= CallStackItemFlags.TailCall;
                        }
                    }
                }
            }


            if (fn.Type == DataType.ClrFunction)
            {
                var args = this.CreateArgsListForFunctionCall(argsCount, 0);
                // we expand tuples before callbacks
                // instructionPtr - 1: instructionPtr already points to the next instruction at this moment
                // but we need the current instruction here
                var sref = GetCurrentSourceRef(instructionPtr - 1);

                _executionStack.Push(new CallStackItem
                {
                    ClrFunction = fn.Callback,
                    ReturnAddress = instructionPtr,
                    CallingSourceRef = sref,
                    BasePointer = -1,
                    ErrorHandler = handler,
                    Continuation = continuation,
                    ErrorHandlerBeforeUnwind = unwindHandler,
                    Flags = flags
                });


                var ret = fn.Callback.Invoke(new ScriptExecutionContext(ecToken, this, fn.Callback, sref), args,
                    thisCall);
                _valueStack.RemoveLast(argsCount + 1);
                _valueStack.Push(ret);

                _executionStack.Pop();

                return this.Internal_CheckForTailRequests(ecToken, null, instructionPtr);
            }

            if (fn.Type == DataType.Function)
            {
                _valueStack.Push(DynValue.NewNumber(argsCount));
                _executionStack.Push(new CallStackItem
                {
                    BasePointer = _valueStack.Count,
                    ReturnAddress = instructionPtr,
                    Debug_EntryPoint = fn.Function.EntryPointByteCodeLocation,
                    CallingSourceRef = GetCurrentSourceRef(instructionPtr - 1),
                    ClosureScope = fn.Function.ClosureContext,
                    ErrorHandler = handler,
                    Continuation = continuation,
                    ErrorHandlerBeforeUnwind = unwindHandler,
                    Flags = flags
                });
                return fn.Function.EntryPointByteCodeLocation;
            }

            // fallback to __call metamethod
            var m = this.GetMetamethod(ecToken, fn, "__call");

            if (m != null && m.IsNotNil())
            {
                var tmp = new DynValue[argsCount + 1];
                for (int i = 0; i < argsCount + 1; i++)
                {
                    tmp[i] = _valueStack.Pop();
                }

                _valueStack.Push(m);

                for (int i = argsCount; i >= 0; i--)
                {
                    _valueStack.Push(tmp[i]);
                }

                return this.Internal_ExecCall(ecToken, argsCount + 1, instructionPtr, handler, continuation);
            }

            throw ScriptRuntimeException.AttemptToCallNonFunc(fn.Type, debugText);
        }

        private int PerformTCO(int instructionPtr, int argsCount)
        {
            var args = new DynValue[argsCount + 1];

            // Remove all cur args and func ptr
            for (int i = 0; i <= argsCount; i++)
            {
                args[i] = _valueStack.Pop();
            }

            // perform a fake RET
            var csi = this.PopToBasePointer();
            int retpoint = csi.ReturnAddress;
            int argscnt = (int)(_valueStack.Pop().Number);
            _valueStack.RemoveLast(argscnt + 1);

            // Re-push all cur args and func ptr
            for (int i = argsCount; i >= 0; i--)
            {
                _valueStack.Push(args[i]);
            }

            return retpoint;
        }


        private int ExecRet(ExecutionControlToken ecToken, Instruction i)
        {
            CallStackItem csi;
            int retpoint;

            if (i.NumVal == 0)
            {
                csi = this.PopToBasePointer();
                retpoint = csi.ReturnAddress;
                int argscnt = (int)(_valueStack.Pop().Number);
                _valueStack.RemoveLast(argscnt + 1);
                _valueStack.Push(DynValue.Void);
            }
            else if (i.NumVal == 1)
            {
                var retval = _valueStack.Pop();
                csi = this.PopToBasePointer();
                retpoint = csi.ReturnAddress;
                int argscnt = (int)(_valueStack.Pop().Number);
                _valueStack.RemoveLast(argscnt + 1);
                _valueStack.Push(retval);
                retpoint = this.Internal_CheckForTailRequests(ecToken, i, retpoint);
            }
            else
            {
                throw new InternalErrorException("RET supports only 0 and 1 ret val scenarios");
            }

            if (csi.Continuation != null)
            {
                _valueStack.Push(csi.Continuation.Invoke(
                    new ScriptExecutionContext(ecToken, this, csi.Continuation, i.SourceCodeRef),
                    new DynValue[1] { _valueStack.Pop() }));
            }

            return retpoint;
        }


        private int Internal_CheckForTailRequests(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var tail = _valueStack.Peek();

            if (tail.Type == DataType.TailCallRequest)
            {
                _valueStack.Pop(); // discard tail call request

                var tcd = tail.TailCallData;

                _valueStack.Push(tcd.Function);

                for (int ii = 0; ii < tcd.Args.Length; ii++)
                {
                    _valueStack.Push(tcd.Args[ii]);
                }

                return this.Internal_ExecCall(ecToken, tcd.Args.Length, instructionPtr, tcd.ErrorHandler,
                    tcd.Continuation, false, null, tcd.ErrorHandlerBeforeUnwind);
            }

            if (tail.Type == DataType.YieldRequest)
            {
                _savedInstructionPtr = instructionPtr;
                return YIELD_SPECIAL_TRAP;
            }


            return instructionPtr;
        }


        private int JumpBool(Instruction i, bool expectedValueForJump, int instructionPtr)
        {
            var op = _valueStack.Pop().ToScalar();

            if (op.CastToBool() == expectedValueForJump)
            {
                return i.NumVal;
            }

            return instructionPtr;
        }

        private int ExecShortCircuitingOperator(Instruction i, int instructionPtr)
        {
            bool expectedValToShortCircuit = i.OpCode == OpCode.JtOrPop;

            var op = _valueStack.Peek().ToScalar();

            if (op.CastToBool() == expectedValToShortCircuit)
            {
                return i.NumVal;
            }

            _valueStack.Pop();
            return instructionPtr;
        }


        private int ExecAdd(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            var rn = r.CastToNumber();
            var ln = l.CastToNumber();

            if (ln.HasValue && rn.HasValue)
            {
                _valueStack.Push(DynValue.NewNumber(ln.Value + rn.Value));
                return instructionPtr;
            }

            int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__add", instructionPtr);
            if (ip >= 0)
            {
                return ip;
            }

            throw ScriptRuntimeException.ArithmeticOnNonNumber(l, r);
        }

        private int ExecSub(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            var rn = r.CastToNumber();
            var ln = l.CastToNumber();

            if (ln.HasValue && rn.HasValue)
            {
                _valueStack.Push(DynValue.NewNumber(ln.Value - rn.Value));
                return instructionPtr;
            }

            int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__sub", instructionPtr);
            if (ip >= 0)
            {
                return ip;
            }

            throw ScriptRuntimeException.ArithmeticOnNonNumber(l, r);
        }


        private int ExecMul(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            var rn = r.CastToNumber();
            var ln = l.CastToNumber();

            if (ln.HasValue && rn.HasValue)
            {
                _valueStack.Push(DynValue.NewNumber(ln.Value * rn.Value));
                return instructionPtr;
            }

            int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__mul", instructionPtr);
            if (ip >= 0)
            {
                return ip;
            }

            throw ScriptRuntimeException.ArithmeticOnNonNumber(l, r);
        }

        private int ExecMod(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            var rn = r.CastToNumber();
            var ln = l.CastToNumber();

            if (ln.HasValue && rn.HasValue)
            {
                double mod = Math.IEEERemainder(ln.Value, rn.Value);
                if (mod < 0)
                {
                    mod += rn.Value;
                }

                _valueStack.Push(DynValue.NewNumber(mod));
                return instructionPtr;
            }

            int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__mod", instructionPtr);
            if (ip >= 0)
            {
                return ip;
            }

            throw ScriptRuntimeException.ArithmeticOnNonNumber(l, r);
        }

        private int ExecDiv(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            var rn = r.CastToNumber();
            var ln = l.CastToNumber();

            if (ln.HasValue && rn.HasValue)
            {
                _valueStack.Push(DynValue.NewNumber(ln.Value / rn.Value));
                return instructionPtr;
            }

            int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__div", instructionPtr);
            if (ip >= 0)
            {
                return ip;
            }

            throw ScriptRuntimeException.ArithmeticOnNonNumber(l, r);
        }

        private int ExecPower(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            var rn = r.CastToNumber();
            var ln = l.CastToNumber();

            if (ln.HasValue && rn.HasValue)
            {
                _valueStack.Push(DynValue.NewNumber(Math.Pow(ln.Value, rn.Value)));
                return instructionPtr;
            }

            int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__pow", instructionPtr);
            if (ip >= 0)
            {
                return ip;
            }

            throw ScriptRuntimeException.ArithmeticOnNonNumber(l, r);
        }

        private int ExecNeg(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var rn = r.CastToNumber();

            if (rn.HasValue)
            {
                _valueStack.Push(DynValue.NewNumber(-rn.Value));
                return instructionPtr;
            }

            int ip = this.Internal_InvokeUnaryMetaMethod(ecToken, r, "__unm", instructionPtr);
            if (ip >= 0)
            {
                return ip;
            }

            throw ScriptRuntimeException.ArithmeticOnNonNumber(r);
        }


        private int ExecEq(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            // first we do a brute force equals over the references
            if (ReferenceEquals(r, l))
            {
                _valueStack.Push(DynValue.True);
                return instructionPtr;
            }

            // then if they are userdatas, attempt meta
            if (l.Type == DataType.UserData || r.Type == DataType.UserData)
            {
                int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__eq", instructionPtr);
                if (ip >= 0)
                {
                    return ip;
                }
            }

            // then if types are different, ret false
            if (r.Type != l.Type)
            {
                if ((l.Type == DataType.Nil && r.Type == DataType.Void) ||
                    (l.Type == DataType.Void && r.Type == DataType.Nil))
                {
                    _valueStack.Push(DynValue.True);
                }
                else
                {
                    _valueStack.Push(DynValue.False);
                }

                return instructionPtr;
            }

            // then attempt metatables for tables
            if ((l.Type == DataType.Table) && (this.GetMetatable(l) != null) &&
                (this.GetMetatable(l) == this.GetMetatable(r)))
            {
                int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__eq", instructionPtr);
                if (ip >= 0)
                {
                    return ip;
                }
            }

            // else perform standard comparison
            _valueStack.Push(DynValue.NewBoolean(r.Equals(l)));
            return instructionPtr;
        }

        private int ExecLess(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            if (l.Type == DataType.Number && r.Type == DataType.Number)
            {
                _valueStack.Push(DynValue.NewBoolean(l.Number < r.Number));
            }
            else if (l.Type == DataType.String && r.Type == DataType.String)
            {
                _valueStack.Push(DynValue.NewBoolean(l.String.CompareTo(r.String) < 0));
            }
            else
            {
                int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__lt", instructionPtr);
                if (ip < 0)
                {
                    throw ScriptRuntimeException.CompareInvalidType(l, r);
                }

                return ip;
            }

            return instructionPtr;
        }


        private int ExecLessEq(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            if (l.Type == DataType.Number && r.Type == DataType.Number)
            {
                _valueStack.Push(DynValue.False);
                _valueStack.Push(DynValue.NewBoolean(l.Number <= r.Number));
            }
            else if (l.Type == DataType.String && r.Type == DataType.String)
            {
                _valueStack.Push(DynValue.False);
                _valueStack.Push(DynValue.NewBoolean(l.String.CompareTo(r.String) <= 0));
            }
            else
            {
                int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__le", instructionPtr, DynValue.False);
                if (ip < 0)
                {
                    ip = this.Internal_InvokeBinaryMetaMethod(ecToken, r, l, "__lt", instructionPtr, DynValue.True);

                    if (ip < 0)
                    {
                        throw ScriptRuntimeException.CompareInvalidType(l, r);
                    }

                    return ip;
                }

                return ip;
            }

            return instructionPtr;
        }

        private int ExecLen(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();

            if (r.Type == DataType.String)
            {
                _valueStack.Push(DynValue.NewNumber(r.String.Length));
            }
            else
            {
                int ip = this.Internal_InvokeUnaryMetaMethod(ecToken, r, "__len", instructionPtr);
                if (ip >= 0)
                {
                    return ip;
                }

                if (r.Type == DataType.Table)
                {
                    _valueStack.Push(DynValue.NewNumber(r.Table.Length));
                }

                else
                {
                    throw ScriptRuntimeException.LenOnInvalidType(r);
                }
            }

            return instructionPtr;
        }

        /// <summary>
        /// Concatenates two values.
        /// </summary>
        private int ExecConcat(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            var r = _valueStack.Pop().ToScalar();
            var l = _valueStack.Pop().ToScalar();

            string rs = r.CastToString();
            string ls = l.CastToString();

            if (rs != null && ls != null)
            {
                _valueStack.Push(DynValue.NewString(string.Concat(ls, rs)));
                return instructionPtr;
            }

            int ip = this.Internal_InvokeBinaryMetaMethod(ecToken, l, r, "__concat", instructionPtr);

            if (ip >= 0)
            {
                return ip;
            }

            throw ScriptRuntimeException.ConcatOnNonString(l, r);
        }


        private void ExecTblInitI(Instruction i)
        {
            // stack: tbl - val
            var val = _valueStack.Pop();
            var tbl = _valueStack.Peek();

            if (tbl.Type != DataType.Table)
            {
                throw new InternalErrorException("Unexpected type in table ctor : {0}", tbl);
            }

            tbl.Table.InitNextArrayKeys(val, i.NumVal != 0);
        }

        private void ExecTblInitN(Instruction i)
        {
            // stack: tbl - key - val
            var val = _valueStack.Pop();
            var key = _valueStack.Pop();
            var tbl = _valueStack.Peek();

            if (tbl.Type != DataType.Table)
            {
                throw new InternalErrorException("Unexpected type in table ctor : {0}", tbl);
            }

            tbl.Table.Set(key, val.ToScalar());
        }

        private int ExecIndexSet(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            int nestedMetaOps = 100; // sanity check, to avoid potential infinite loop here

            // stack: vals.. - base - index
            bool isNameIndex = i.OpCode == OpCode.IndexSetN;
            bool isMultiIndex = (i.OpCode == OpCode.IndexSetL);

            var originalIdx = i.Value ?? _valueStack.Pop();
            var idx = originalIdx.ToScalar();
            var obj = _valueStack.Pop().ToScalar();
            var value = this.GetStoreValue(i);

            while (nestedMetaOps > 0)
            {
                --nestedMetaOps;

                DynValue h;

                if (obj.Type == DataType.Table)
                {
                    if (!isMultiIndex)
                    {
                        if (!obj.Table.Get(idx).IsNil())
                        {
                            obj.Table.Set(idx, value);
                            return instructionPtr;
                        }
                    }

                    h = this.GetMetamethodRaw(obj, "__newindex");

                    if (h == null || h.IsNil())
                    {
                        if (isMultiIndex)
                        {
                            throw new ScriptRuntimeException("cannot multi-index a table. userdata expected");
                        }

                        obj.Table.Set(idx, value);
                        return instructionPtr;
                    }
                }
                else if (obj.Type == DataType.UserData)
                {
                    var ud = obj.UserData;

                    if (!ud.Descriptor.SetIndex(ecToken, this.GetScript(), ud.Object, originalIdx, value, isNameIndex))
                    {
                        throw ScriptRuntimeException.UserDataMissingField(ud.Descriptor.Name, idx.String);
                    }

                    return instructionPtr;
                }
                else
                {
                    h = this.GetMetamethodRaw(obj, "__newindex");

                    if (h == null || h.IsNil())
                    {
                        throw ScriptRuntimeException.IndexType(obj);
                    }
                }

                if (h.Type == DataType.Function || h.Type == DataType.ClrFunction)
                {
                    if (isMultiIndex)
                    {
                        throw new ScriptRuntimeException("cannot multi-index through metamethods. userdata expected");
                    }

                    _valueStack.Pop(); // burn extra value ?

                    _valueStack.Push(h);
                    _valueStack.Push(obj);
                    _valueStack.Push(idx);
                    _valueStack.Push(value);
                    return this.Internal_ExecCall(ecToken, 3, instructionPtr);
                }

                obj = h;
                h = null;
            }

            throw ScriptRuntimeException.LoopInNewIndex();
        }

        private int ExecIndex(ExecutionControlToken ecToken, Instruction i, int instructionPtr)
        {
            int nestedMetaOps = 100; // sanity check, to avoid potential infinite loop here

            // stack: base - index
            bool isNameIndex = i.OpCode == OpCode.IndexN;

            bool isMultiIndex = (i.OpCode == OpCode.IndexL);

            var originalIdx = i.Value ?? _valueStack.Pop();
            var idx = originalIdx.ToScalar();
            var obj = _valueStack.Pop().ToScalar();

            while (nestedMetaOps > 0)
            {
                --nestedMetaOps;

                DynValue h;

                if (obj.Type == DataType.Table)
                {
                    if (!isMultiIndex)
                    {
                        var v = obj.Table.Get(idx);

                        if (!v.IsNil())
                        {
                            _valueStack.Push(v.AsReadOnly());
                            return instructionPtr;
                        }
                    }

                    h = this.GetMetamethodRaw(obj, "__index");

                    if (h == null || h.IsNil())
                    {
                        if (isMultiIndex)
                        {
                            throw new ScriptRuntimeException("cannot multi-index a table. userdata expected");
                        }

                        _valueStack.Push(DynValue.Nil);
                        return instructionPtr;
                    }
                }
                else if (obj.Type == DataType.UserData)
                {
                    var ud = obj.UserData;

                    var v = ud.Descriptor.Index(ecToken, this.GetScript(), ud.Object, originalIdx, isNameIndex);

                    if (v == null)
                    {
                        throw ScriptRuntimeException.UserDataMissingField(ud.Descriptor.Name, idx.String);
                    }

                    _valueStack.Push(v.AsReadOnly());
                    return instructionPtr;
                }
                else
                {
                    h = this.GetMetamethodRaw(obj, "__index");

                    if (h == null || h.IsNil())
                    {
                        throw ScriptRuntimeException.IndexType(obj);
                    }
                }

                if (h.Type == DataType.Function || h.Type == DataType.ClrFunction)
                {
                    if (isMultiIndex)
                    {
                        throw new ScriptRuntimeException("cannot multi-index through metamethods. userdata expected");
                    }

                    _valueStack.Push(h);
                    _valueStack.Push(obj);
                    _valueStack.Push(idx);
                    return this.Internal_ExecCall(ecToken, 2, instructionPtr);
                }

                obj = h;
                h = null;
            }

            throw ScriptRuntimeException.LoopInIndex();
        }
    }
}