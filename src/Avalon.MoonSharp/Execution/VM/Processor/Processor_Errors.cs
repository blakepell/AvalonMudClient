using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
    internal sealed partial class Processor
    {
        private SourceRef GetCurrentSourceRef(int instructionPtr)
        {
            if (instructionPtr >= 0 && instructionPtr < _rootChunk.Code.Count)
            {
                return _rootChunk.Code[instructionPtr].SourceCodeRef;
            }

            return null;
        }


        private void FillDebugData(InterpreterException ex, int ip)
        {
            // adjust IP
            if (ip == YIELD_SPECIAL_TRAP)
            {
                ip = _savedInstructionPtr;
            }
            else
            {
                ip -= 1;
            }

            ex.InstructionPtr = ip;

            var sref = this.GetCurrentSourceRef(ip);
            ex.DecorateMessage(_script, sref, ip);
        }
    }
}