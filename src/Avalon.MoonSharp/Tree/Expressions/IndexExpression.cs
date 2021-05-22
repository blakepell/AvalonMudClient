using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Expressions
{
    internal class IndexExpression : Expression, IVariable
    {
        private Expression _baseExp;
        private Expression _indexExp;
        private string _name;


        public IndexExpression(Expression baseExp, Expression indexExp, ScriptLoadingContext lcontext) : base(lcontext)
        {
            _baseExp = baseExp;
            _indexExp = indexExp;
        }

        public IndexExpression(Expression baseExp, string name, ScriptLoadingContext lcontext) : base(lcontext)
        {
            _baseExp = baseExp;
            _name = name;
        }

        public void CompileAssignment(ByteCode bc, int stackofs, int tupleidx)
        {
            _baseExp.Compile(bc);

            if (_name != null)
            {
                bc.Emit_IndexSet(stackofs, tupleidx, DynValue.NewString(_name), true);
            }
            else if (_indexExp is LiteralExpression lit)
            {
                bc.Emit_IndexSet(stackofs, tupleidx, lit.Value);
            }
            else
            {
                _indexExp.Compile(bc);
                bc.Emit_IndexSet(stackofs, tupleidx, isExpList: (_indexExp is ExprListExpression));
            }
        }


        public override void Compile(ByteCode bc)
        {
            _baseExp.Compile(bc);

            if (_name != null)
            {
                bc.Emit_Index(DynValue.NewString(_name), true);
            }
            else if (_indexExp is LiteralExpression lit)
            {
                bc.Emit_Index(lit.Value);
            }
            else
            {
                _indexExp.Compile(bc);
                bc.Emit_Index(isExpList: (_indexExp is ExprListExpression));
            }
        }

        public override DynValue Eval(ScriptExecutionContext context)
        {
            var b = _baseExp.Eval(context).ToScalar();
            var i = _indexExp != null ? _indexExp.Eval(context).ToScalar() : DynValue.NewString(_name);

            if (b.Type != DataType.Table)
            {
                throw new DynamicExpressionException("Attempt to index non-table.");
            }

            if (i.IsNilOrNan())
            {
                throw new DynamicExpressionException("Attempt to index with nil or nan key.");
            }

            return b.Table.Get(i) ?? DynValue.Nil;
        }
    }
}