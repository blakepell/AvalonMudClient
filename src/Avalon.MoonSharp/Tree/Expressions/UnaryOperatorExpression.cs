using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Expressions
{
    internal class UnaryOperatorExpression : Expression
    {
        private Expression _exp;
        private string _opText;

        public UnaryOperatorExpression(ScriptLoadingContext lcontext, Expression subExpression, Token unaryOpToken) : base(lcontext)
        {
            _opText = unaryOpToken.Text;
            _exp = subExpression;
        }


        public override void Compile(ByteCode bc)
        {
            _exp.Compile(bc);

            switch (_opText)
            {
                case "not":
                    bc.Emit_Operator(OpCode.Not);
                    break;
                case "#":
                    bc.Emit_Operator(OpCode.Len);
                    break;
                case "-":
                    bc.Emit_Operator(OpCode.Neg);
                    break;
                default:
                    throw new InternalErrorException("Unexpected unary operator '{0}'", _opText);
            }
        }

        public override DynValue Eval(ScriptExecutionContext context)
        {
            var v = _exp.Eval(context).ToScalar();

            switch (_opText)
            {
                case "not":
                    return DynValue.NewBoolean(!v.CastToBool());
                case "#":
                    return v.GetLength();
                case "-":
                {
                    var d = v.CastToNumber();

                    if (d.HasValue)
                    {
                        return DynValue.NewNumber(-d.Value);
                    }

                    throw new DynamicExpressionException("Attempt to perform arithmetic on non-numbers.");
                }
                default:
                    throw new DynamicExpressionException("Unexpected unary operator '{0}'", _opText);
            }
        }
    }
}