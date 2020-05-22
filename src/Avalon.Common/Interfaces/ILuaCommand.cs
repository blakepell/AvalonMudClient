using System;
using System.Collections.Generic;
using System.Text;

namespace Avalon.Common.Interfaces
{
    /// <summary>
    /// Property and methods that Lua command classes must contain.
    /// </summary>
    public interface ILuaCommand
    {
        /// <summary>
        /// The unique namespace that will be prefixed to the Lua commands.
        /// </summary>
        string Namespace { get; set; }

        /// <summary>
        /// A reference to the Interpreter.
        /// </summary>
        IInterpreter Interpreter { get; set; }
    }
}
