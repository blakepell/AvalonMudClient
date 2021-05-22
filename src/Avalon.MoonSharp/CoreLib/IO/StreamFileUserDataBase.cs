using System.IO;

namespace MoonSharp.Interpreter.CoreLib.IO
{
    /// <summary>
    /// Abstract class implementing a file Lua userdata. Methods are meant to be called by Lua code.
    /// </summary>
    internal abstract class StreamFileUserDataBase : FileUserDataBase
    {
        protected bool _closed;
        protected StreamReader _reader;
        protected Stream _stream;
        protected StreamWriter _writer;


        protected void Initialize(Stream stream, StreamReader reader, StreamWriter writer)
        {
            _stream = stream;
            _reader = reader;
            _writer = writer;
        }


        private void CheckFileIsNotClosed()
        {
            if (_closed)
            {
                throw new ScriptRuntimeException("attempt to use a closed file");
            }
        }


        protected override bool Eof()
        {
            this.CheckFileIsNotClosed();

            if (_reader != null)
            {
                return _reader.EndOfStream;
            }

            return false;
        }

        protected override string ReadLine()
        {
            this.CheckFileIsNotClosed();
            return _reader.ReadLine();
        }

        protected override string ReadToEnd()
        {
            this.CheckFileIsNotClosed();
            return _reader.ReadToEnd();
        }

        protected override string ReadBuffer(int p)
        {
            this.CheckFileIsNotClosed();
            var buffer = new char[p];
            int length = _reader.ReadBlock(buffer, 0, p);
            return new string(buffer, 0, length);
        }

        protected override char Peek()
        {
            this.CheckFileIsNotClosed();
            return (char) _reader.Peek();
        }

        protected override void Write(string value)
        {
            this.CheckFileIsNotClosed();
            _writer.Write(value);
        }

        protected override string Close()
        {
            this.CheckFileIsNotClosed();

            _writer?.Dispose();
            _reader?.Dispose();
            _stream.Dispose();

            _closed = true;

            return null;
        }

        public override bool flush()
        {
            this.CheckFileIsNotClosed();

            _writer?.Flush();

            return true;
        }

        public override long seek(string whence, long offset = 0)
        {
            this.CheckFileIsNotClosed();

            if (whence != null)
            {
                if (whence == "set")
                {
                    _stream.Seek(offset, SeekOrigin.Begin);
                }
                else if (whence == "cur")
                {
                    _stream.Seek(offset, SeekOrigin.Current);
                }
                else if (whence == "end")
                {
                    _stream.Seek(offset, SeekOrigin.End);
                }
                else
                {
                    throw ScriptRuntimeException.BadArgument(0, "seek", "invalid option '" + whence + "'");
                }
            }

            return _stream.Position;
        }

        public override bool setvbuf(string mode)
        {
            this.CheckFileIsNotClosed();

            if (_writer != null)
            {
                _writer.AutoFlush = (mode == "no" || mode == "line");
            }

            return true;
        }

        protected internal override bool isopen()
        {
            return !_closed;
        }
    }
}