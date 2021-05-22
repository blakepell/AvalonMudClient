using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Execution.Scopes
{
    internal class BuildTimeScopeFrame
    {
        private RuntimeScopeFrame _scopeFrame = new RuntimeScopeFrame();
        private BuildTimeScopeBlock _scopeTreeHead;
        private BuildTimeScopeBlock _scopeTreeRoot;

        internal BuildTimeScopeFrame(bool hasVarArgs)
        {
            this.HasVarArgs = hasVarArgs;
            _scopeTreeHead = _scopeTreeRoot = new BuildTimeScopeBlock(null);
        }

        public bool HasVarArgs { get; }

        internal void PushBlock()
        {
            _scopeTreeHead = _scopeTreeHead.AddChild();
        }

        internal RuntimeScopeBlock PopBlock()
        {
            var tree = _scopeTreeHead;

            _scopeTreeHead.ResolveGotos();

            _scopeTreeHead = _scopeTreeHead.Parent;

            if (_scopeTreeHead == null)
            {
                throw new InternalErrorException("Can't pop block - stack underflow");
            }

            return tree.ScopeBlock;
        }

        internal RuntimeScopeFrame GetRuntimeFrameData()
        {
            if (_scopeTreeHead != _scopeTreeRoot)
            {
                throw new InternalErrorException("Misaligned scope frames/blocks!");
            }

            _scopeFrame.ToFirstBlock = _scopeTreeRoot.ScopeBlock.To;

            return _scopeFrame;
        }

        internal SymbolRef Find(string name)
        {
            for (var tree = _scopeTreeHead; tree != null; tree = tree.Parent)
            {
                var l = tree.Find(name);

                if (l != null)
                {
                    return l;
                }
            }

            return null;
        }

        internal SymbolRef DefineLocal(string name)
        {
            return _scopeTreeHead.Define(name);
        }

        internal SymbolRef TryDefineLocal(string name)
        {
            if (_scopeTreeHead.Find(name) != null)
            {
                _scopeTreeHead.Rename(name);
            }

            return _scopeTreeHead.Define(name);
        }

        internal void ResolveLRefs()
        {
            _scopeTreeRoot.ResolveGotos();

            _scopeTreeRoot.ResolveLRefs(this);
        }

        internal int AllocVar(SymbolRef var)
        {
            var._index = _scopeFrame.DebugSymbols.Count;
            _scopeFrame.DebugSymbols.Add(var);
            return var._index;
        }

        internal int GetPosForNextVar()
        {
            return _scopeFrame.DebugSymbols.Count;
        }

        internal void DefineLabel(LabelStatement label)
        {
            _scopeTreeHead.DefineLabel(label);
        }

        internal void RegisterGoto(GotoStatement gotostat)
        {
            _scopeTreeHead.RegisterGoto(gotostat);
        }
    }
}