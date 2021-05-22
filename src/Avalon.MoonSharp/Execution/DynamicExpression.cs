using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// Represents a dynamic expression in the script
    /// </summary>
    public class DynamicExpression : IScriptPrivateResource
    {
        /// <summary>
        /// The code which generated this expression
        /// </summary>
        public readonly string ExpressionCode;

        private DynValue _constant;
        private DynamicExprExpression _exp;

        internal DynamicExpression(Script s, string strExpr, DynamicExprExpression expr)
        {
            ExpressionCode = strExpr;
            this.OwnerScript = s;
            _exp = expr;
        }

        internal DynamicExpression(Script s, string strExpr, DynValue constant)
        {
            ExpressionCode = strExpr;
            this.OwnerScript = s;
            _constant = constant;
        }

        /// <summary>
        /// Gets the script owning this resource.
        /// </summary>
        /// <value>
        /// The script owning this resource.
        /// </value>
        public Script OwnerScript { get; }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">The context.</param>
        public DynValue Evaluate(ScriptExecutionContext context = null)
        {
            context ??= this.OwnerScript.CreateDynamicExecutionContext(ExecutionControlToken.Dummy);

            this.CheckScriptOwnership(context.GetScript());

            if (_constant != null)
            {
                return _constant;
            }

            return _exp.Eval(context);
        }

        /// <summary>
        /// Finds a symbol in the expression
        /// </summary>
        /// <param name="context">The context.</param>
        public SymbolRef FindSymbol(ScriptExecutionContext context)
        {
            this.CheckScriptOwnership(context.GetScript());

            return _exp?.FindDynamic(context);
        }

        /// <summary>
        /// Determines whether this instance is a constant expression
        /// </summary>
        public bool IsConstant()
        {
            return _constant != null;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return ExpressionCode.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var o = obj as DynamicExpression;

            if (o == null)
            {
                return false;
            }

            return o.ExpressionCode == ExpressionCode;
        }
    }
}