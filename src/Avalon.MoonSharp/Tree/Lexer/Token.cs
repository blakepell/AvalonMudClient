using System;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Tree
{
    internal class Token
    {
        public readonly int FromCol, ToCol, FromLine, ToLine, PrevCol, PrevLine;
        public readonly TokenType Type;

        public Token(TokenType type, int fromLine, int fromCol, int toLine, int toCol, int prevLine, int prevCol)
        {
            Type = type;
            FromLine = fromLine;
            FromCol = fromCol;
            ToCol = toCol;
            ToLine = toLine;
            PrevCol = prevCol;
            PrevLine = prevLine;
        }

        public string Text { get; set; }

        public override string ToString()
        {
            string tokenTypeString = (Type + "                                                      ").Substring(0, 16);
            string location = $"{FromLine.ToString()}:{FromCol.ToString()}-{ToLine.ToString()}:{ToCol.ToString()}";

            location = (location + "                                                      ").Substring(0, 10);

            return $"{tokenTypeString}  - {location} - '{this.Text ?? ""}'";
        }

        public static TokenType? GetReservedTokenType(string reservedWord)
        {
            switch (reservedWord)
            {
                case "and":
                    return TokenType.And;
                case "break":
                    return TokenType.Break;
                case "do":
                    return TokenType.Do;
                case "else":
                    return TokenType.Else;
                case "elseif":
                    return TokenType.ElseIf;
                case "end":
                    return TokenType.End;
                case "false":
                    return TokenType.False;
                case "for":
                    return TokenType.For;
                case "function":
                    return TokenType.Function;
                case "goto":
                    return TokenType.Goto;
                case "if":
                    return TokenType.If;
                case "in":
                    return TokenType.In;
                case "local":
                    return TokenType.Local;
                case "nil":
                    return TokenType.Nil;
                case "not":
                    return TokenType.Not;
                case "or":
                    return TokenType.Or;
                case "repeat":
                    return TokenType.Repeat;
                case "return":
                    return TokenType.Return;
                case "then":
                    return TokenType.Then;
                case "true":
                    return TokenType.True;
                case "until":
                    return TokenType.Until;
                case "while":
                    return TokenType.While;
                default:
                    return null;
            }
        }

        public double GetNumberValue()
        {
            if (Type == TokenType.Number)
            {
                return LexerUtils.ParseNumber(this);
            }

            if (Type == TokenType.Number_Hex)
            {
                return LexerUtils.ParseHexInteger(this);
            }

            if (Type == TokenType.Number_HexFloat)
            {
                return LexerUtils.ParseHexFloat(this);
            }

            throw new NotSupportedException("GetNumberValue is supported only on numeric tokens");
        }


        public bool IsEndOfBlock()
        {
            switch (Type)
            {
                case TokenType.Else:
                case TokenType.ElseIf:
                case TokenType.End:
                case TokenType.Until:
                case TokenType.Eof:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsUnaryOperator()
        {
            return Type == TokenType.Op_MinusOrSub || Type == TokenType.Not || Type == TokenType.Op_Len;
        }

        public bool IsBinaryOperator()
        {
            switch (Type)
            {
                case TokenType.And:
                case TokenType.Or:
                case TokenType.Op_Equal:
                case TokenType.Op_LessThan:
                case TokenType.Op_LessThanEqual:
                case TokenType.Op_GreaterThanEqual:
                case TokenType.Op_GreaterThan:
                case TokenType.Op_NotEqual:
                case TokenType.Op_Concat:
                case TokenType.Op_Pwr:
                case TokenType.Op_Mod:
                case TokenType.Op_Div:
                case TokenType.Op_Mul:
                case TokenType.Op_MinusOrSub:
                case TokenType.Op_Add:
                    return true;
                default:
                    return false;
            }
        }


        internal SourceRef GetSourceRef(bool isStepStop = true)
        {
            return new SourceRef(FromCol, ToCol, FromLine, ToLine, isStepStop);
        }

        internal SourceRef GetSourceRef(Token to, bool isStepStop = true)
        {
            return new SourceRef(FromCol, to.ToCol, FromLine, to.ToLine, isStepStop);
        }

        internal SourceRef GetSourceRefUpTo(Token to, bool isStepStop = true)
        {
            return new SourceRef(FromCol, to.PrevCol, FromLine, to.PrevLine, isStepStop);
        }
    }
}