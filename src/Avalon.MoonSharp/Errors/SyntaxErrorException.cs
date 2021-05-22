using System;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Tree;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// Exception for all parsing/lexing errors. 
    /// </summary>
    [Serializable]
    public class SyntaxErrorException : InterpreterException
    {
        internal SyntaxErrorException(Token t, string format, params object[] args)
            : base(format, args)
        {
            this.Token = t;
        }

        internal SyntaxErrorException(Token t, string message)
            : base(message)
        {
            this.Token = t;
        }

        internal SyntaxErrorException(Script script, SourceRef sref, string format, params object[] args)
            : base(format, args)
        {
            this.DecorateMessage(script, sref);
        }

        internal SyntaxErrorException(Script script, SourceRef sref, string message)
            : base(message)
        {
            this.DecorateMessage(script, sref);
        }

        private SyntaxErrorException(SyntaxErrorException syntaxErrorException)
            : base(syntaxErrorException, syntaxErrorException.DecoratedMessage)
        {
            this.Token = syntaxErrorException.Token;
            this.DecoratedMessage = this.Message;
        }

        internal Token Token { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this exception was caused by premature stream termination (that is, unexpected EOF).
        /// This can be used in REPL interfaces to tell between unrecoverable errors and those which can be recovered by extra input.
        /// </summary>
        public bool IsPrematureStreamTermination { get; set; }

        /// <summary>
        /// The first line of any errors.
        /// </summary>
        public int FromLineNumber => this.Token?.FromLine ?? -1;

        /// <summary>
        /// The last line of any errors.
        /// </summary>
        public int ToLineNumber => this.Token?.ToLine ?? -1;

        internal void DecorateMessage(Script script)
        {
            if (this.Token != null)
            {
                this.DecorateMessage(script, this.Token.GetSourceRef(false));
            }
        }

        /// <summary>
        /// Rethrows this instance if 
        /// </summary>
        public override void Rethrow()
        {
            if (Script.GlobalOptions.RethrowExceptionNested)
            {
                throw new SyntaxErrorException(this);
            }
        }
    }
}