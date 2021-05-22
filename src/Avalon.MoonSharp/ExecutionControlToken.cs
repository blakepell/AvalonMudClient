using System;
using System.Threading;

namespace MoonSharp.Interpreter
{
    /// <summary>
    /// This class provides an interface to control execution of Lua scripts ran asynchronously.
    /// </summary>
    public class ExecutionControlToken
    {
        public static readonly ExecutionControlToken Dummy = new() {_isDummy = true};

        private CancellationTokenSource _cancellationTokenSource = new();

        private bool _isDummy;

        /// <summary>
        ///  Creates an usable execution control token.
        /// </summary>
        public ExecutionControlToken()
        {
            _isDummy = false;
        }

        internal bool IsAbortRequested => _cancellationTokenSource.IsCancellationRequested;

        /// <summary>
        ///  Aborts the execution of the script that is associated with this token.
        /// </summary>
        public void Terminate()
        {
            if (!_isDummy)
            {
                _cancellationTokenSource.Cancel(true);
            }
        }

        internal void Wait(TimeSpan timeSpan)
        {
            _cancellationTokenSource.Token.WaitHandle.WaitOne(timeSpan);
        }
    }
}