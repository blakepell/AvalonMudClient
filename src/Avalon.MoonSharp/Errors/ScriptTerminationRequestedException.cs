using System;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// Exception thrown when an async script is requested to abort
    /// </summary>
    [Serializable]
    public class ScriptTerminationRequestedException : InterpreterException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptTerminationRequestedException"/> class.
        /// </summary>
        internal ScriptTerminationRequestedException()
            : base("script has been requested to abort")
        {
            this.DecoratedMessage = this.Message;
        }
    }
}