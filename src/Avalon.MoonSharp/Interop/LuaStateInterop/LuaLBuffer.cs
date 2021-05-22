// Disable warnings about XML documentation

#pragma warning disable 1591

using System.Text;

namespace MoonSharp.Interpreter.Interop.LuaStateInterop
{
    public class LuaLBuffer
    {
        public LuaLBuffer(LuaState l)
        {
            this.StringBuilder = new StringBuilder();
            this.LuaState = l;
        }

        public StringBuilder StringBuilder { get; }
        public LuaState LuaState { get; }
    }
}