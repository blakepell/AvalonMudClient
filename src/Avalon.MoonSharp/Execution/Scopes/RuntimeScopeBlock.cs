namespace MoonSharp.Interpreter.Execution
{
    internal class RuntimeScopeBlock
    {
        public int From { get; internal set; }
        public int To { get; internal set; }
        public int ToInclusive { get; internal set; }

        public override string ToString()
        {
            return $"ScopeBlock : {this.From.ToString()} -> {this.To.ToString()} --> {this.ToInclusive.ToString()}";
        }
    }
}