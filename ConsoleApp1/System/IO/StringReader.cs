namespace System.IO
{
    public sealed class StringReader : TextReader
    {
        private readonly string _value;
        private int _position;
        private bool _closed;

        public StringReader(string value)
        {
            _value = value ?? throw new ArgumentNullException("The string to read cannot be null.");
        }

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

        public override void Close() => _closed = true;

        private void EnsureOpen()
        {
            if (_closed)
                throw new IOException("The string reader is closed.");
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
