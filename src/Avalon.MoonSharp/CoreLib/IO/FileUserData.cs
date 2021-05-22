using System.IO;
using System.Text;

namespace MoonSharp.Interpreter.CoreLib.IO
{
    /// <summary>
    /// Abstract class implementing a file Lua userdata. Methods are meant to be called by Lua code.
    /// </summary>
    internal class FileUserData : StreamFileUserDataBase
    {
        public FileUserData(Script script, string filename, Encoding encoding, string mode)
        {
            var stream = Script.GlobalOptions.Platform.IO_OpenFile(script, filename, encoding, mode);

            var reader = (stream.CanRead) ? new StreamReader(stream, encoding) : null;
            var writer = (stream.CanWrite) ? new StreamWriter(stream, encoding) : null;

            this.Initialize(stream, reader, writer);
        }
    }
}