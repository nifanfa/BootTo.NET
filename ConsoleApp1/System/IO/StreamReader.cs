using System.Text;

namespace System.IO
{
    public sealed class StreamReader : TextReader
    {
        private readonly Stream _stream;
        private readonly bool _leaveOpen;
        private readonly Encoding _encoding;
        private readonly string _value;
        private int _position;
        private bool _closed;

        public StreamReader(string path)
            : this(path, Encoding.UTF8)
        {
        }

        public StreamReader(string path, Encoding encoding)
            : this(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), encoding, true, 1024)
        {
        }

        public StreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : this(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), encoding, detectEncodingFromByteOrderMarks, bufferSize, false)
        {
        }

        public StreamReader(Stream stream)
            : this(stream, Encoding.UTF8, true, 1024)
        {
        }

        public StreamReader(Stream stream, Encoding encoding)
            : this(stream, encoding, true, 1024)
        {
        }

        public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : this(stream, encoding, detectEncodingFromByteOrderMarks, 1024, false)
        {
        }

        public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : this(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, false)
        {
        }

        public StreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen)
        {
            if (stream == null || encoding == null)
                throw new ArgumentNullException("The stream and encoding cannot be null.");
            if (bufferSize <= 0)
                throw new ArgumentException("The reader buffer size must be positive.");

            _stream = stream;
            _encoding = encoding;
            _leaveOpen = leaveOpen;
            _value = ReadAll(stream, encoding, detectEncodingFromByteOrderMarks);
        }

        public Encoding CurrentEncoding => _encoding;
        public bool EndOfStream => _position >= _value.Length;

        public override int Read()
        {
            EnsureOpen();
            return _position < _value.Length ? _value[_position++] : -1;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            EnsureOpen();
            if (buffer == null)
                throw new ArgumentNullException("The read buffer cannot be null.");
            if (index < 0 || count < 0 || index > buffer.Length - count)
                throw new ArgumentException("The read buffer offset and count do not describe a valid range.");

            int available = _value.Length - _position;
            int read = count < available ? count : available;
            for (int i = 0; i < read; i++)
                buffer[index + i] = _value[_position + i];
            _position += read;
            return read;
        }

        public override string ReadLine()
        {
            EnsureOpen();
            if (_position >= _value.Length)
                return null;

            int start = _position;
            while (_position < _value.Length && _value[_position] != '\r' && _value[_position] != '\n')
                _position++;
            string result = Slice(_value, start, _position - start);
            if (_position < _value.Length && _value[_position++] == '\r' && _position < _value.Length && _value[_position] == '\n')
                _position++;
            return result;
        }

        public override string ReadToEnd()
        {
            EnsureOpen();
            string result = Slice(_value, _position, _value.Length - _position);
            _position = _value.Length;
            return result;
        }

        public override void Close()
        {
            if (_closed)
                return;
            _closed = true;
            if (!_leaveOpen)
                _stream.Close();
        }

        private static string ReadAll(Stream stream, Encoding encoding, bool detectBom)
        {
            MemoryStream data = new MemoryStream();
            byte[] buffer = new byte[4096];
            int count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                data.Write(buffer, 0, count);

            string result = encoding.GetString(data.ToArray());
            if (detectBom && result.Length > 0 && result[0] == '\uFEFF')
                result = Slice(result, 1, result.Length - 1);
            data.Close();
            return result;
        }

        private void EnsureOpen()
        {
            if (_closed)
                throw new IOException("The stream reader is closed.");
        }

        private static string Slice(string value, int start, int length)
        {
            if (length == 0)
                return string.Empty;
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = value[start + i];
            return new string(result);
        }
    }
}
