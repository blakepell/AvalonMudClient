using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Execution.Scopes
{
    internal class BuildTimeScopeBlock
    {
        private Dictionary<string, SymbolRef> _definedNames = new Dictionary<string, SymbolRef>();
        private string _lastDefinedName;
        private Dictionary<string, LabelStatement> _localLabels;
        private List<GotoStatement> _pendingGotos;

        internal BuildTimeScopeBlock(BuildTimeScopeBlock parent)
        {
            this.Parent = parent;
            this.ChildNodes = new List<BuildTimeScopeBlock>();
            this.ScopeBlock = new RuntimeScopeBlock();
        }

        internal BuildTimeScopeBlock Parent { get; }
        internal List<BuildTimeScopeBlock> ChildNodes { get; }
        internal RuntimeScopeBlock ScopeBlock { get; }

        internal void Rename(string name)
        {
            var sref = _definedNames[name];
            _definedNames.Remove(name);
            _definedNames.Add($"@{name}_{Guid.NewGuid().ToString("N")}", sref);
        }


        internal BuildTimeScopeBlock AddChild()
        {
            var block = new BuildTimeScopeBlock(this);
            this.ChildNodes.Add(block);
            return block;
        }

        internal SymbolRef Find(string name)
        {
            return _definedNames.GetOrDefault(name);
        }

        internal SymbolRef Define(string name)
        {
            var l = SymbolRef.Local(name, -1);
            _definedNames.Add(name, l);
            _lastDefinedName = name;
            return l;
        }

        internal int ResolveLRefs(BuildTimeScopeFrame buildTimeScopeFrame)
        {
            int firstVal = -1;
            int lastVal = -1;

            foreach (var lref in _definedNames.Values)
            {
                int pos = buildTimeScopeFrame.AllocVar(lref);

                if (firstVal < 0)
                {
                    firstVal = pos;
                }

                lastVal = pos;
            }

            this.ScopeBlock.From = firstVal;
            this.ScopeBlock.ToInclusive = this.ScopeBlock.To = lastVal;

            if (firstVal < 0)
            {
                this.ScopeBlock.From = buildTimeScopeFrame.GetPosForNextVar();
            }

            foreach (var child in this.ChildNodes)
            {
                this.ScopeBlock.ToInclusive =
                    Math.Max(this.ScopeBlock.ToInclusive, child.ResolveLRefs(buildTimeScopeFrame));
            }

            if (_localLabels != null)
            {
                foreach (var label in _localLabels.Values)
                {
                    label.SetScope(this.ScopeBlock);
                }
            }

            return this.ScopeBlock.ToInclusive;
        }

        internal void DefineLabel(LabelStatement label)
        {
            if (_localLabels == null)
            {
                _localLabels = new Dictionary<string, LabelStatement>();
            }

            if (_localLabels.ContainsKey(label.Label))
            {
                throw new SyntaxErrorException(label.NameToken, "label '{0}' already defined on line {1}", label.Label,
                    _localLabels[label.Label].SourceRef.FromLine);
            }

            _localLabels.Add(label.Label, label);
            label.SetDefinedVars(_definedNames.Count, _lastDefinedName);
        }

        internal void RegisterGoto(GotoStatement gotostat)
        {
            if (_pendingGotos == null)
            {
                _pendingGotos = new List<GotoStatement>();
            }

            _pendingGotos.Add(gotostat);
            gotostat.SetDefinedVars(_definedNames.Count, _lastDefinedName);
        }

        internal void ResolveGotos()
        {
            if (_pendingGotos == null)
            {
                return;
            }

            foreach (var gotostat in _pendingGotos)
            {
                LabelStatement label;

                if (_localLabels != null && _localLabels.TryGetValue(gotostat.Label, out label))
                {
                    if (label.DefinedVarsCount > gotostat.DefinedVarsCount)
                    {
                        throw new SyntaxErrorException(gotostat.GotoToken,
                            "<goto {0}> at line {1} jumps into the scope of local '{2}'", gotostat.Label,
                            gotostat.GotoToken.FromLine,
                            label.LastDefinedVarName);
                    }

                    label.RegisterGoto(gotostat);
                }
                else
                {
                    if (this.Parent == null)
                    {
                        throw new SyntaxErrorException(gotostat.GotoToken,
                            "no visible label '{0}' for <goto> at line {1}", gotostat.Label,
                            gotostat.GotoToken.FromLine);
                    }

                    this.Parent.RegisterGoto(gotostat);
                }
            }

            _pendingGotos.Clear();
        }
    }
}