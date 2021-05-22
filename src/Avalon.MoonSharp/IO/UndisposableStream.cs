using System;
using System.IO;

namespace MoonSharp.Interpreter.IO
{
    /// <summary>
    /// An adapter over Stream which bypasses the Dispose and Close methods.
    /// Used to work around the pesky wrappers .NET has over Stream (BinaryReader, StreamWriter, etc.) which think they
    /// own the Stream and close them when they shouldn't. Damn.
    /// </summary>
    public class UndisposableStream : Stream
    {
        private Stream _stream;

        public UndisposableStream(Stream stream)
        {
            _stream = stream;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override bool CanTimeout => _stream.CanTimeout;

        public override int ReadTimeout
        {
            get => _stream.ReadTimeout;
            set => _stream.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => _stream.WriteTimeout;
            set => _stream.WriteTimeout = value;
        }

        protected override void Dispose(bool disposing)
        {
        }

#if !(ENABLE_DOTNET || NETFX_CORE)
        public override void Close()
        {
        }
#endif

        public override void Flush()
        {
            _stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }


        public override bool Equals(object obj)
        {
            return _stream.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _stream.GetHashCode();
        }


        public override int ReadByte()
        {
            return _stream.ReadByte();
        }

        public override string ToString()
        {
            return _stream.ToString();
        }

        public override void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

#if (!(NETFX_CORE))
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            return _stream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback,
            object state)
        {
            return _stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _stream.EndWrite(asyncResult);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return _stream.EndRead(asyncResult);
        }
#endif
    }
}