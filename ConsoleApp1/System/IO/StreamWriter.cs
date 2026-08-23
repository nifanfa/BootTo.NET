using System.Text;

namespace System.IO
{
    public sealed class StreamWriter : TextWriter
    {
        private readonly Stream _stream;
        private readonly bool _leaveOpen;
        private readonly Encoding _encoding;
        private bool _closed;

        public StreamWriter(string path)
            : this(path, false, Encoding.UTF8)
        {
        }

        public StreamWriter(string path, bool append)
            : this(path, append, Encoding.UTF8)
        {
        }

        public StreamWriter(string path, bool append, Encoding encoding)
            : this(new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read), encoding, 1024, false)
        {
        }

        public StreamWriter(string path, Encoding encoding)
            : this(path, false, encoding)
        {
        }

        public StreamWriter(string path, bool append, Encoding encoding, int bufferSize)
            : this(new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read), encoding, bufferSize, false)
        {
        }

        public StreamWriter(Stream stream)
            : this(stream, Encoding.UTF8, 1024, false)
        {
        }

        public StreamWriter(Stream stream, Encoding encoding)
            : this(stream, encoding, 1024, false)
        {
        }

        public StreamWriter(Stream stream, Encoding encoding, bool leaveOpen)
            : this(stream, encoding, 1024, leaveOpen)
        {
        }

        public StreamWriter(Stream stream, Encoding encoding, int bufferSize)
            : this(stream, encoding, bufferSize, false)
        {
        }

        public StreamWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen)
        {
            if (stream == null || encoding == null)
                throw new ArgumentNullException();
            if (bufferSize <= 0)
                throw new ArgumentException();

            _stream = stream;
            _encoding = encoding;
            _leaveOpen = leaveOpen;
        }

        public override Encoding Encoding => _encoding;

        public override void Write(char value)
            => Write(new string(new char[] { value }));

        public override void Write(string value)
        {
            EnsureOpen();
            if (value == null)
                return;
            byte[] bytes = _encoding.GetBytes(value);
            _stream.Write(bytes, 0, bytes.Length);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (index < 0 || count < 0 || index > buffer.Length - count)
                throw new ArgumentException();
            char[] value = new char[count];
            for (int i = 0; i < count; i++)
                value[i] = buffer[index + i];
            Write(new string(value));
        }

        public override void Flush()
        {
            EnsureOpen();
            _stream.Flush();
        }

        public override void Close()
        {
            if (_closed)
                return;
            try
            {
                Flush();
            }
            finally
            {
                _closed = true;
                if (!_leaveOpen)
                    _stream.Close();
            }
        }

        private void EnsureOpen()
        {
            if (_closed)
                throw new IOException();
        }
    }
}
