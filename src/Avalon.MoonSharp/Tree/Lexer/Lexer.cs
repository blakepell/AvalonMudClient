using System.Text;
using Cysharp.Text;

namespace MoonSharp.Interpreter.Tree
{
    internal class Lexer
    {
        private bool _autoSkipComments;
        private string _code;
        private int _col;
        private Token _current;
        private int _cursor;
        private int _line = 1;
        private int _prevColTo = 1;
        private int _prevLineTo;

        public Lexer(string scriptContent, bool autoSkipComments)
        {
            _code = scriptContent;

            // remove unicode BOM if any
            if (_code.Length > 0 && _code[0] == 0xFEFF)
            {
                _code = _code.Substring(1);
            }

            _autoSkipComments = autoSkipComments;
        }

        public Token Current
        {
            get
            {
                if (_current == null)
                {
                    this.Next();
                }

                return _current;
            }
        }

        private Token FetchNewToken()
        {
            while (true)
            {
                var t = this.ReadToken();

                if ((t.Type != TokenType.Comment && t.Type != TokenType.HashBang) || (!_autoSkipComments))
                {
                    return t;
                }
            }
        }

        public void Next()
        {
            _current = this.FetchNewToken();
        }

        public Token PeekNext()
        {
            int snapshot = _cursor;
            var current = _current;
            int line = _line;
            int col = _col;

            this.Next();
            var t = this.Current;

            _cursor = snapshot;
            _current = current;
            _line = line;
            _col = col;

            return t;
        }


        private void CursorNext()
        {
            if (this.CursorNotEof())
            {
                if (this.CursorChar() == '\n')
                {
                    _col = 0;
                    _line++;
                }
                else
                {
                    _col++;
                }

                _cursor++;
            }
        }

        private char CursorChar()
        {
            if (_cursor < _code.Length)
            {
                return _code[_cursor];
            }

            return '\0'; //  sentinel
        }

        private char CursorCharNext()
        {
            this.CursorNext();
            return this.CursorChar();
        }

        private bool CursorMatches(string pattern)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                int j = _cursor + i;

                if (j >= _code.Length)
                {
                    return false;
                }

                if (_code[j] != pattern[i])
                {
                    return false;
                }
            }

            return true;
        }

        private bool CursorNotEof()
        {
            return _cursor < _code.Length;
        }

        private bool IsWhiteSpace(char c)
        {
            return char.IsWhiteSpace(c);
        }

        private void SkipWhiteSpace()
        {
            for (; this.CursorNotEof() && this.IsWhiteSpace(this.CursorChar()); this.CursorNext())
            {
            }
        }

        private Token ReadToken()
        {
            this.SkipWhiteSpace();

            int fromLine = _line;
            int fromCol = _col;

            if (!this.CursorNotEof())
            {
                return this.CreateToken(TokenType.Eof, fromLine, fromCol, "<eof>");
            }

            char c = this.CursorChar();

            switch (c)
            {
                case '|':
                    this.CursorCharNext();
                    return this.CreateToken(TokenType.Lambda, fromLine, fromCol, "|");
                case ';':
                    this.CursorCharNext();
                    return this.CreateToken(TokenType.SemiColon, fromLine, fromCol, ";");
                case '=':
                    return this.PotentiallyDoubleCharOperator('=', TokenType.Op_Assignment, TokenType.Op_Equal, fromLine, fromCol);
                case '<':
                    return this.PotentiallyDoubleCharOperator('=', TokenType.Op_LessThan, TokenType.Op_LessThanEqual, fromLine, fromCol);
                case '>':
                    return this.PotentiallyDoubleCharOperator('=', TokenType.Op_GreaterThan, TokenType.Op_GreaterThanEqual, fromLine, fromCol);
                case '~':
                case '!':
                    if (this.CursorCharNext() != '=')
                    {
                        throw new SyntaxErrorException(this.CreateToken(TokenType.Invalid, fromLine, fromCol), "unexpected symbol near '{0}'", c);
                    }

                    this.CursorCharNext();

                    return this.CreateToken(TokenType.Op_NotEqual, fromLine, fromCol, "~=");
                case '.':
                    {
                        char next = this.CursorCharNext();

                        if (next == '.')
                        {
                            return this.PotentiallyDoubleCharOperator('.', TokenType.Op_Concat, TokenType.VarArgs, fromLine, fromCol);
                        }

                        if (LexerUtils.CharIsDigit(next))
                        {
                            return this.ReadNumberToken(fromLine, fromCol, true);
                        }

                        return this.CreateToken(TokenType.Dot, fromLine, fromCol, ".");
                    }
                case '+':
                    return this.CreateSingleCharToken(TokenType.Op_Add, fromLine, fromCol);
                case '-':
                    {
                        char next = this.CursorCharNext();

                        if (next == '-')
                        {
                            return this.ReadComment(fromLine, fromCol);
                        }

                        return this.CreateToken(TokenType.Op_MinusOrSub, fromLine, fromCol, "-");
                    }
                case '*':
                    return this.CreateSingleCharToken(TokenType.Op_Mul, fromLine, fromCol);
                case '/':
                    return this.CreateSingleCharToken(TokenType.Op_Div, fromLine, fromCol);
                case '%':
                    return this.CreateSingleCharToken(TokenType.Op_Mod, fromLine, fromCol);
                case '^':
                    return this.CreateSingleCharToken(TokenType.Op_Pwr, fromLine, fromCol);
                case '$':
                    return this.PotentiallyDoubleCharOperator('{', TokenType.Op_Dollar, TokenType.Brk_Open_Curly_Shared,
                        fromLine, fromCol);
                case '#':
                    if (_cursor == 0 && _code.Length > 1 && _code[1] == '!')
                    {
                        return this.ReadHashBang(fromLine, fromCol);
                    }

                    return this.CreateSingleCharToken(TokenType.Op_Len, fromLine, fromCol);
                case '[':
                    {
                        char next = this.CursorCharNext();

                        if (next == '=' || next == '[')
                        {
                            string str = this.ReadLongString(fromLine, fromCol, null, "string");
                            return this.CreateToken(TokenType.String_Long, fromLine, fromCol, str);
                        }

                        return this.CreateToken(TokenType.Brk_Open_Square, fromLine, fromCol, "[");
                    }
                case ']':
                    return this.CreateSingleCharToken(TokenType.Brk_Close_Square, fromLine, fromCol);
                case '(':
                    return this.CreateSingleCharToken(TokenType.Brk_Open_Round, fromLine, fromCol);
                case ')':
                    return this.CreateSingleCharToken(TokenType.Brk_Close_Round, fromLine, fromCol);
                case '{':
                    return this.CreateSingleCharToken(TokenType.Brk_Open_Curly, fromLine, fromCol);
                case '}':
                    return this.CreateSingleCharToken(TokenType.Brk_Close_Curly, fromLine, fromCol);
                case ',':
                    return this.CreateSingleCharToken(TokenType.Comma, fromLine, fromCol);
                case ':':
                    return this.PotentiallyDoubleCharOperator(':', TokenType.Colon, TokenType.DoubleColon, fromLine, fromCol);
                case '"':
                case '\'':
                    return this.ReadSimpleStringToken(fromLine, fromCol);
                case '\0':
                    throw new SyntaxErrorException(this.CreateToken(TokenType.Invalid, fromLine, fromCol), "unexpected symbol near '{0}'", this.CursorChar())
                    {
                        IsPrematureStreamTermination = true
                    };
                default:
                    {
                        if (char.IsLetter(c) || c == '_')
                        {
                            string name = this.ReadNameToken();
                            return this.CreateNameToken(name, fromLine, fromCol);
                        }

                        if (LexerUtils.CharIsDigit(c))
                        {
                            return this.ReadNumberToken(fromLine, fromCol, false);
                        }
                    }

                    throw new SyntaxErrorException(this.CreateToken(TokenType.Invalid, fromLine, fromCol), "unexpected symbol near '{0}'", this.CursorChar());
            }
        }

        private string ReadLongString(int fromLine, int fromCol, string startpattern, string subtypeforerrors)
        {
            // here we are at the first '=' or second '['
            var text = new StringBuilder(1024);
            string endPattern = "]";

            if (startpattern == null)
            {
                for (char c = this.CursorChar(); ; c = this.CursorCharNext())
                {
                    if (c == '\0' || !this.CursorNotEof())
                    {
                        throw new SyntaxErrorException(this.CreateToken(TokenType.Invalid, fromLine, fromCol),
                                "unfinished long {0} near '<eof>'", subtypeforerrors)
                        { IsPrematureStreamTermination = true };
                    }

                    if (c == '=')
                    {
                        endPattern += "=";
                    }
                    else if (c == '[')
                    {
                        endPattern += "]";
                        break;
                    }
                    else
                    {
                        throw new SyntaxErrorException(this.CreateToken(TokenType.Invalid, fromLine, fromCol),
                                "invalid long {0} delimiter near '{1}'", subtypeforerrors, c)
                        { IsPrematureStreamTermination = true };
                    }
                }
            }
            else
            {
                endPattern = startpattern.Replace('[', ']');
            }


            for (char c = this.CursorCharNext(); ; c = this.CursorCharNext())
            {
                if (c == '\r'
                ) // XXI century and we still debate on how a newline is made. throw new DeveloperExtremelyAngryException.
                {
                    continue;
                }

                if (c == '\0' || !this.CursorNotEof())
                {
                    throw new SyntaxErrorException(this.CreateToken(TokenType.Invalid, fromLine, fromCol),
                            "unfinished long {0} near '{1}'", subtypeforerrors, text.ToString())
                    { IsPrematureStreamTermination = true };
                }

                if (c == ']' && this.CursorMatches(endPattern))
                {
                    for (int i = 0; i < endPattern.Length; i++)
                    {
                        this.CursorCharNext();
                    }

                    return LexerUtils.AdjustLuaLongString(text.ToString());
                }

                text.Append(c);
            }
        }

        private Token ReadNumberToken(int fromLine, int fromCol, bool leadingDot)
        {
            //INT : Digit+
            //HEX : '0' [xX] HexDigit+
            //FLOAT : Digit+ '.' Digit* ExponentPart?
            //		| '.' Digit+ ExponentPart?
            //		| Digit+ ExponentPart
            //HEX_FLOAT : '0' [xX] HexDigit+ '.' HexDigit* HexExponentPart?
            //			| '0' [xX] '.' HexDigit+ HexExponentPart?
            //			| '0' [xX] HexDigit+ HexExponentPart
            //
            // ExponentPart : [eE] [+-]? Digit+
            // HexExponentPart : [pP] [+-]? Digit+

            using (var sb = ZString.CreateStringBuilder())
            {
                bool isHex = false;
                bool dotAdded = false;
                bool exponentPart = false;
                bool exponentSignAllowed = false;

                if (leadingDot)
                {
                    sb.Append("0.");
                }
                else if (this.CursorChar() == '0')
                {
                    sb.Append(this.CursorChar());
                    char secondChar = this.CursorCharNext();

                    if (secondChar == 'x' || secondChar == 'X')
                    {
                        isHex = true;
                        sb.Append(this.CursorChar());
                        this.CursorCharNext();
                    }
                }

                for (char c = this.CursorChar(); this.CursorNotEof(); c = this.CursorCharNext())
                {
                    if (exponentSignAllowed && (c == '+' || c == '-'))
                    {
                        exponentSignAllowed = false;
                        sb.Append(c);
                    }
                    else if (LexerUtils.CharIsDigit(c))
                    {
                        sb.Append(c);
                    }
                    else if (c == '.' && !dotAdded)
                    {
                        dotAdded = true;
                        sb.Append(c);
                    }
                    else if (LexerUtils.CharIsHexDigit(c) && isHex && !exponentPart)
                    {
                        sb.Append(c);
                    }
                    else if (c == 'e' || c == 'E' || (isHex && (c == 'p' || c == 'P')))
                    {
                        sb.Append(c);
                        exponentPart = true;
                        exponentSignAllowed = true;
                        dotAdded = true;
                    }
                    else
                    {
                        break;
                    }
                }

                var numberType = TokenType.Number;

                if (isHex && (dotAdded || exponentPart))
                {
                    numberType = TokenType.Number_HexFloat;
                }
                else if (isHex)
                {
                    numberType = TokenType.Number_Hex;
                }

                return this.CreateToken(numberType, fromLine, fromCol, sb.ToString());
            }
        }

        private Token CreateSingleCharToken(TokenType tokenType, int fromLine, int fromCol)
        {
            char c = this.CursorChar();
            this.CursorCharNext();
            return this.CreateToken(tokenType, fromLine, fromCol, c.ToString());
        }

        private Token ReadHashBang(int fromLine, int fromCol)
        {
            var text = new StringBuilder(32);

            for (char c = this.CursorChar(); this.CursorNotEof(); c = this.CursorCharNext())
            {
                if (c == '\n')
                {
                    this.CursorCharNext();
                    return this.CreateToken(TokenType.HashBang, fromLine, fromCol, text.ToString());
                }

                if (c != '\r')
                {
                    text.Append(c);
                }
            }

            return this.CreateToken(TokenType.HashBang, fromLine, fromCol, text.ToString());
        }


        private Token ReadComment(int fromLine, int fromCol)
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                bool extraneousFound = false;

                for (char c = this.CursorCharNext(); this.CursorNotEof(); c = this.CursorCharNext())
                {
                    if (c == '[' && !extraneousFound && sb.Length > 0)
                    {
                        sb.Append('[');
                        string comment = this.ReadLongString(fromLine, fromCol, sb.ToString(), "comment");
                        return this.CreateToken(TokenType.Comment, fromLine, fromCol, comment);
                    }

                    if (c == '\n')
                    {
                        this.CursorCharNext();
                        return this.CreateToken(TokenType.Comment, fromLine, fromCol, sb.ToString());
                    }

                    if (c != '\r')
                    {
                        if (c != '[' && c != '=')
                        {
                            extraneousFound = true;
                        }

                        sb.Append(c);
                    }
                }

                return this.CreateToken(TokenType.Comment, fromLine, fromCol, sb.ToString());
            }
        }

        private Token ReadSimpleStringToken(int fromLine, int fromCol)
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                char separator = this.CursorChar();

                for (char c = this.CursorCharNext(); this.CursorNotEof(); c = this.CursorCharNext())
                {
                    redo_Loop:

                    if (c == '\\')
                    {
                        sb.Append(c);
                        c = this.CursorCharNext();
                        sb.Append(c);

                        if (c == '\r')
                        {
                            c = this.CursorCharNext();
                            if (c == '\n')
                            {
                                sb.Append(c);
                            }
                            else
                            {
                                goto redo_Loop;
                            }
                        }
                        else if (c == 'z')
                        {
                            c = this.CursorCharNext();

                            if (char.IsWhiteSpace(c))
                            {
                                this.SkipWhiteSpace();
                            }

                            c = this.CursorChar();

                            goto redo_Loop;
                        }
                    }
                    else if (c == '\n' || c == '\r')
                    {
                        throw new SyntaxErrorException(this.CreateToken(TokenType.Invalid, fromLine, fromCol),
                            "unfinished string near '{0}'", sb.ToString());
                    }
                    else if (c == separator)
                    {
                        this.CursorCharNext();
                        var t = this.CreateToken(TokenType.String, fromLine, fromCol);
                        t.Text = LexerUtils.UnescapeLuaString(t, sb.ToString());
                        return t;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                throw new SyntaxErrorException(this.CreateToken(TokenType.Invalid, fromLine, fromCol),
                        "unfinished string near '{0}'", sb.ToString())
                { IsPrematureStreamTermination = true };
            }
        }


        private Token PotentiallyDoubleCharOperator(char expectedSecondChar, TokenType singleCharToken, TokenType doubleCharToken, int fromLine, int fromCol)
        {
            string op = this.CursorChar().ToString();

            this.CursorCharNext();

            if (this.CursorChar() == expectedSecondChar)
            {
                this.CursorCharNext();
                return this.CreateToken(doubleCharToken, fromLine, fromCol, op + expectedSecondChar);
            }

            return this.CreateToken(singleCharToken, fromLine, fromCol, op);
        }


        private Token CreateNameToken(string name, int fromLine, int fromCol)
        {
            var reservedType = Token.GetReservedTokenType(name);

            if (reservedType.HasValue)
            {
                return this.CreateToken(reservedType.Value, fromLine, fromCol, name);
            }

            return this.CreateToken(TokenType.Name, fromLine, fromCol, name);
        }


        private Token CreateToken(TokenType tokenType, int fromLine, int fromCol, string text = null)
        {
            var t = new Token(tokenType, fromLine, fromCol, _line, _col, _prevLineTo, _prevColTo)
            {
                Text = text
            };
            _prevLineTo = _line;
            _prevColTo = _col;
            return t;
        }

        private string ReadNameToken()
        {
            using (var sb = ZString.CreateStringBuilder())
            {
                for (char c = this.CursorChar(); this.CursorNotEof(); c = this.CursorCharNext())
                {
                    if (char.IsLetterOrDigit(c) || c == '_')
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        break;
                    }
                }

                return sb.ToString();
            }
        }
    }
}