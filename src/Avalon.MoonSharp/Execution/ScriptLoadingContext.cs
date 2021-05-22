using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Tree;

namespace MoonSharp.Interpreter.Execution
{
    internal class ScriptLoadingContext
    {
        public ScriptLoadingContext(Script s)
        {
            this.Script = s;
        }

        public Script Script { get; }
        public BuildTimeScope Scope { get; set; }
        public SourceCode Source { get; set; }
        public bool Anonymous { get; set; }
        public bool IsDynamicExpression { get; set; }
        public Lexer Lexer { get; set; }
    }
}