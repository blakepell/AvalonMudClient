using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// Base type of all exceptions thrown in MoonSharp
    /// </summary>
    [Serializable]
    public class InterpreterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterpreterException"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="message"></param>
        protected InterpreterException(Exception ex, string message)
            : base(message, ex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterpreterException"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        protected InterpreterException(Exception ex)
            : base(ex.Message, ex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterpreterException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        protected InterpreterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterpreterException"/> class.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        protected InterpreterException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }

        /// <summary>
        /// Gets the instruction pointer of the execution (if it makes sense)
        /// </summary>
        public int InstructionPtr { get; internal set; }

        /// <summary>
        /// Gets the decorated message (error message plus error location in script) if possible.
        /// </summary>
        public string DecoratedMessage { get; internal set; }


        /// <summary>
        /// Gets or sets a value indicating whether the message should not be decorated
        /// </summary>
        public bool DoNotDecorateMessage { get; set; }


        internal void DecorateMessage(Script script, SourceRef sRef, int ip = -1)
        {
            if (string.IsNullOrEmpty(this.DecoratedMessage))
            {
                if (this.DoNotDecorateMessage)
                {
                    this.DecoratedMessage = this.Message;
                }
                else if (sRef != null)
                {
                    this.DecoratedMessage = $"{sRef.FormatLocation(script)}: {this.Message}";
                }
                else
                {
                    this.DecoratedMessage = $"bytecode:{ip.ToString()}: {this.Message}";
                }
            }
        }

        /// <summary>
        /// Rethrows this instance if 
        /// </summary>
        public virtual void Rethrow()
        {
        }
    }
}