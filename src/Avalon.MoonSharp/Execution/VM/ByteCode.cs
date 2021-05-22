using MoonSharp.Interpreter.Debugging;
using System;
using System.Collections.Generic;

namespace MoonSharp.Interpreter.Execution.VM
{
    internal class ByteCode : RefIdObject
    {
        public List<Instruction> Code = new();
        internal LoopTracker LoopTracker = new();
        private SourceRef _currentSourceRef;
        private List<SourceRef> _sourceRefStack = new();

        public ByteCode(Script script)
        {
            this.Script = script;
        }

        /// <summary>
        /// Resets the list of instructions that have been loaded into this instance of MoonSharp.  This
        /// will in effect negate being able to call loaded functions (e.g. they will have to be reloaded)
        /// but if lots of unused code has been loaded it will allow the program to reclaim the memory
        /// instead of effectively leaking it.  Use caution with this method.
        /// </summary>
        public void Reset()
        {
            this.Code.Clear();
        }

        public Script Script { get; }

        /// <summary>
        /// Returns the number of instructions held by this <see cref="ByteCode"/> class.
        /// </summary>
        public int InstructionCount => this.Code.Count;

        public IDisposable EnterSource(SourceRef sref)
        {
            return new SourceCodeStackGuard(sref, this);
        }


        public void PushSourceRef(SourceRef sref)
        {
            _sourceRefStack.Add(sref);
            _currentSourceRef = sref;
        }

        public void PopSourceRef()
        {
            _sourceRefStack.RemoveAt(_sourceRefStack.Count - 1);
            _currentSourceRef = (_sourceRefStack.Count > 0) ? _sourceRefStack[^1] : null;
        }

        public int GetJumpPointForNextInstruction()
        {
            return Code.Count;
        }

        public int GetJumpPointForLastInstruction()
        {
            return Code.Count - 1;
        }

        public Instruction GetLastInstruction()
        {
            return Code[^1];
        }

        private Instruction AppendInstruction(Instruction c)
        {
            Code.Add(c);
            return c;
        }

        public Instruction Emit_Nop(string comment)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Nop, Name = comment});
        }

        public Instruction Emit_Invalid(string type)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Invalid, Name = type});
        }

        public Instruction Emit_Pop(int num = 1)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Pop, NumVal = num});
        }

        public void Emit_Call(int argCount, string debugName)
        {
            this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.Call, NumVal = argCount, Name = debugName});
        }

        public void Emit_ThisCall(int argCount, string debugName)
        {
            this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.ThisCall, NumVal = argCount, Name = debugName});
        }

        public Instruction Emit_Literal(DynValue value)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Literal, Value = value});
        }

        public Instruction Emit_Jump(OpCode jumpOpCode, int idx, int optPar = 0)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = jumpOpCode, NumVal = idx, NumVal2 = optPar});
        }

        public Instruction Emit_MkTuple(int cnt)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.MkTuple, NumVal = cnt});
        }

        public Instruction Emit_Operator(OpCode opcode)
        {
            var i = this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = opcode});

            if (opcode == OpCode.LessEq)
            {
                this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.CNot});
            }

            if (opcode == OpCode.Eq || opcode == OpCode.Less)
            {
                this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.ToBool});
            }

            return i;
        }

        public Instruction Emit_Enter(RuntimeScopeBlock runtimeScopeBlock)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.Clean, NumVal = runtimeScopeBlock.From, NumVal2 = runtimeScopeBlock.ToInclusive});
        }

        public Instruction Emit_Leave(RuntimeScopeBlock runtimeScopeBlock)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.Clean, NumVal = runtimeScopeBlock.From, NumVal2 = runtimeScopeBlock.To});
        }

        public Instruction Emit_Exit(RuntimeScopeBlock runtimeScopeBlock)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.Clean, NumVal = runtimeScopeBlock.From, NumVal2 = runtimeScopeBlock.ToInclusive});
        }

        public Instruction Emit_Clean(RuntimeScopeBlock runtimeScopeBlock)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.Clean, NumVal = runtimeScopeBlock.To + 1, NumVal2 = runtimeScopeBlock.ToInclusive});
        }

        public Instruction Emit_Closure(SymbolRef[] symbols, int jmpnum)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.Closure, SymbolList = symbols, NumVal = jmpnum});
        }

        public Instruction Emit_Args(params SymbolRef[] symbols)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.Args, SymbolList = symbols});
        }

        public Instruction Emit_Ret(int retvals)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Ret, NumVal = retvals});
        }

        public Instruction Emit_ToNum(int stage = 0)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.ToNum, NumVal = stage});
        }

        public Instruction Emit_Incr(int i)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Incr, NumVal = i});
        }

        public Instruction Emit_NewTable(bool shared)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.NewTable, NumVal = shared ? 1 : 0});
        }

        public Instruction Emit_IterPrep()
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.IterPrep});
        }

        public Instruction Emit_ExpTuple(int stackOffset)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.ExpTuple, NumVal = stackOffset});
        }

        public Instruction Emit_IterUpd()
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.IterUpd});
        }

        public Instruction Emit_Meta(string funcName, OpCodeMetadataType metaType, DynValue value = null)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
            {
                OpCode = OpCode.Meta,
                Name = funcName,
                NumVal2 = (int) metaType,
                Value = value
            });
        }


        public Instruction Emit_BeginFn(RuntimeScopeFrame stackFrame)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
            {
                OpCode = OpCode.BeginFn,
                SymbolList = stackFrame.DebugSymbols.ToArray(),
                NumVal = stackFrame.Count,
                NumVal2 = stackFrame.ToFirstBlock
            });
        }

        public Instruction Emit_Scalar()
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Scalar});
        }

        public int Emit_Load(SymbolRef sym)
        {
            switch (sym.Type)
            {
                case SymbolRefType.Global:
                    this.Emit_Load(sym._env);
                    this.AppendInstruction(new Instruction(_currentSourceRef)
                        {OpCode = OpCode.Index, Value = DynValue.NewString(sym._name)});
                    return 2;
                case SymbolRefType.Local:
                    this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Local, Symbol = sym});
                    return 1;
                case SymbolRefType.Upvalue:
                    this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Upvalue, Symbol = sym});
                    return 1;
                default:
                    throw new InternalErrorException("Unexpected symbol type : {0}", sym);
            }
        }

        public int Emit_Store(SymbolRef sym, int stackofs, int tupleidx)
        {
            switch (sym.Type)
            {
                case SymbolRefType.Global:
                    this.Emit_Load(sym._env);
                    this.AppendInstruction(new Instruction(_currentSourceRef)
                    {
                        OpCode = OpCode.IndexSet, Symbol = sym, NumVal = stackofs, NumVal2 = tupleidx,
                        Value = DynValue.NewString(sym._name)
                    });
                    return 2;
                case SymbolRefType.Local:
                    this.AppendInstruction(new Instruction(_currentSourceRef)
                        {OpCode = OpCode.StoreLcl, Symbol = sym, NumVal = stackofs, NumVal2 = tupleidx});
                    return 1;
                case SymbolRefType.Upvalue:
                    this.AppendInstruction(new Instruction(_currentSourceRef)
                        {OpCode = OpCode.StoreUpv, Symbol = sym, NumVal = stackofs, NumVal2 = tupleidx});
                    return 1;
                default:
                    throw new InternalErrorException("Unexpected symbol type : {0}", sym);
            }
        }

        public Instruction Emit_TblInitN()
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.TblInitN});
        }

        public Instruction Emit_TblInitI(bool lastpos)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.TblInitI, NumVal = lastpos ? 1 : 0});
        }

        public Instruction Emit_Index(DynValue index = null, bool isNameIndex = false, bool isExpList = false)
        {
            OpCode o;
            if (isNameIndex)
            {
                o = OpCode.IndexN;
            }
            else if (isExpList)
            {
                o = OpCode.IndexL;
            }
            else
            {
                o = OpCode.Index;
            }

            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = o, Value = index});
        }

        public Instruction Emit_IndexSet(int stackofs, int tupleidx, DynValue index = null, bool isNameIndex = false,
            bool isExpList = false)
        {
            OpCode o;
            if (isNameIndex)
            {
                o = OpCode.IndexSetN;
            }
            else if (isExpList)
            {
                o = OpCode.IndexSetL;
            }
            else
            {
                o = OpCode.IndexSet;
            }

            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = o, NumVal = stackofs, NumVal2 = tupleidx, Value = index});
        }

        public Instruction Emit_Copy(int numval)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef) {OpCode = OpCode.Copy, NumVal = numval});
        }

        public Instruction Emit_Swap(int p1, int p2)
        {
            return this.AppendInstruction(new Instruction(_currentSourceRef)
                {OpCode = OpCode.Swap, NumVal = p1, NumVal2 = p2});
        }


        private class SourceCodeStackGuard : IDisposable
        {
            private ByteCode _bc;

            public SourceCodeStackGuard(SourceRef sref, ByteCode bc)
            {
                _bc = bc;
                _bc.PushSourceRef(sref);
            }

            public void Dispose()
            {
                _bc.PopSourceRef();
            }
        }
    }
}