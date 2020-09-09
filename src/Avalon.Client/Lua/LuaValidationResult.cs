using MoonSharp.Interpreter;
using System;

namespace Avalon.Lua
{
    /// <summary>
    /// A result from Lua code that was validated.
    /// </summary>
    public class LuaValidationResult
    {

        public bool Success { get; set; }

        public SyntaxErrorException Exception { get; set; }

    }
}
