using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter.Execution.Scopes;
using MoonSharp.Interpreter.Tree.Statements;

namespace MoonSharp.Interpreter.Execution
{
    internal class BuildTimeScope
    {
        private List<IClosureBuilder> _closureBuilders = new List<IClosureBuilder>();
        private List<BuildTimeScopeFrame> _frames = new List<BuildTimeScopeFrame>();


        public void PushFunction(IClosureBuilder closureBuilder, bool hasVarArgs)
        {
            _closureBuilders.Add(closureBuilder);
            _frames.Add(new BuildTimeScopeFrame(hasVarArgs));
        }

        public void PushBlock()
        {
            _frames.Last().PushBlock();
        }

        public RuntimeScopeBlock PopBlock()
        {
            return _frames.Last().PopBlock();
        }

        public RuntimeScopeFrame PopFunction()
        {
            var last = _frames.Last();
            last.ResolveLRefs();
            _frames.RemoveAt(_frames.Count - 1);

            _closureBuilders.RemoveAt(_closureBuilders.Count - 1);

            return last.GetRuntimeFrameData();
        }


        public SymbolRef Find(string name)
        {
            var local = _frames.Last().Find(name);

            if (local != null)
            {
                return local;
            }

            for (int i = _frames.Count - 2; i >= 0; i--)
            {
                var symb = _frames[i].Find(name);

                if (symb != null)
                {
                    symb = this.CreateUpValue(this, symb, i, _frames.Count - 2);

                    if (symb != null)
                    {
                        return symb;
                    }
                }
            }

            return this.CreateGlobalReference(name);
        }

        public SymbolRef CreateGlobalReference(string name)
        {
            if (name == WellKnownSymbols.ENV)
            {
                throw new InternalErrorException("_ENV passed in CreateGlobalReference");
            }

            var env = this.Find(WellKnownSymbols.ENV);
            return SymbolRef.Global(name, env);
        }


        public void ForceEnvUpValue()
        {
            this.Find(WellKnownSymbols.ENV);
        }

        private SymbolRef CreateUpValue(BuildTimeScope buildTimeScope, SymbolRef symb, int closuredFrame,
            int currentFrame)
        {
            // it's a 0-level upvalue. Just create it and we're done.
            if (closuredFrame == currentFrame)
            {
                return _closureBuilders[currentFrame + 1].CreateUpvalue(this, symb);
            }

            var upvalue = this.CreateUpValue(buildTimeScope, symb, closuredFrame, currentFrame - 1);

            return _closureBuilders[currentFrame + 1].CreateUpvalue(this, upvalue);
        }

        public SymbolRef DefineLocal(string name)
        {
            return _frames.Last().DefineLocal(name);
        }

        public SymbolRef TryDefineLocal(string name)
        {
            return _frames.Last().TryDefineLocal(name);
        }

        public bool CurrentFunctionHasVarArgs()
        {
            return _frames.Last().HasVarArgs;
        }

        internal void DefineLabel(LabelStatement label)
        {
            _frames.Last().DefineLabel(label);
        }

        internal void RegisterGoto(GotoStatement gotostat)
        {
            _frames.Last().RegisterGoto(gotostat);
        }
    }
}