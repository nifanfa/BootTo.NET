using System.Threading.Tasks;

namespace System.IO
{
    public class BufferedStream : Stream
    {
        private readonly Stream _stream;
        private readonly byte[] _buffer;
        private int _readPosition;
        private int _readLength;
        private int _writePosition;
        private bool _closed;

        public BufferedStream(Stream stream)
            : this(stream, 4096)
        {
        }

        public BufferedStream(Stream stream, int bufferSize)
        {
            if (stream == null)
                throw new ArgumentNullException("The stream cannot be null.");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("The buffer size must be positive.");
            _stream = stream;
            _buffer = new byte[bufferSize];
        }

        public override bool CanRead => !_closed && _stream.CanRead;
        public override bool CanSeek => !_closed && _stream.CanSeek;
        public override bool CanWrite => !_closed && _stream.CanWrite;
        public override int Length
        {
            get
            {
                EnsureOpen();
                return _stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                EnsureOpen();
                EnsureSeekable();
                return _stream.Position - (_readLength - _readPosition) + _writePosition;
            }
            set
            {
                EnsureOpen();
                EnsureSeekable();
                Flush();
                _readPosition = 0;
                _readLength = 0;
                _stream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureOpen();
            ValidateRange(buffer, offset, count);
            if (!CanRead)
                throw new IOException("The buffered stream is not readable.");
            if (count == 0)
                return 0;

            if (_writePosition != 0)
                Flush();

            if (count >= _buffer.Length)
            {
                DiscardReadBuffer();
                return _stream.Read(buffer, offset, count);
            }

            int total = 0;
            while (total < count)
            {
                if (_readPosition == _readLength)
                {
                    _readLength = _stream.Read(_buffer, 0, _buffer.Length);
                    _readPosition = 0;
                    if (_readLength == 0)
                        break;
                }

                int available = _readLength - _readPosition;
                int copy = count - total < available ? count - total : available;
                for (int i = 0; i < copy; i++)
                    buffer[offset + total + i] = _buffer[_readPosition + i];
                _readPosition += copy;
                total += copy;
            }
            return total;
        }

        public override int Write(byte[] buffer, int offset, int count)
        {
            EnsureOpen();
            ValidateRange(buffer, offset, count);
            if (!CanWrite)
                throw new IOException("The buffered stream is not writable.");
            if (count == 0)
                return 0;

            DiscardReadBuffer();
            int total = 0;
            while (total < count)
            {
                int available = _buffer.Length - _writePosition;
                if (_writePosition == 0 && count - total >= _buffer.Length)
                {
                    int written = _stream.Write(buffer, offset + total, count - total);
                    if (written <= 0)
                        throw new IOException("The underlying stream did not accept the write.");
                    total += written;
                    continue;
                }

                int copy = count - total < available ? count - total : available;
                for (int i = 0; i < copy; i++)
                    _buffer[_writePosition + i] = buffer[offset + total + i];
                _writePosition += copy;
                total += copy;
                if (_writePosition == _buffer.Length)
                    FlushWriteBuffer();
            }
            return count;
        }

        public override int ReadByte()
        {
            byte[] value = new byte[1];
            return Read(value, 0, 1) == 0 ? -1 : value[0];
        }

        public override void WriteByte(byte value)
        {
            byte[] buffer = new byte[] { value };
            Write(buffer, 0, 1);
        }

        public override void Flush()
        {
            EnsureOpen();
            FlushWriteBuffer();
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureOpen();
            EnsureSeekable();
            if (origin == SeekOrigin.Current)
                offset -= _readLength - _readPosition;
            Flush();
            _readPosition = 0;
            _readLength = 0;
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            EnsureOpen();
            EnsureSeekable();
            Flush();
            _readPosition = 0;
            _readLength = 0;
            _stream.SetLength(value);
        }

        public override Task<int> ReadAsync(byte[] buffer)
            => Task.FromResult(Read(buffer, 0, buffer == null ? 0 : buffer.Length));

        public override Task<int> WriteAsync(byte[] buffer)
            => Task.FromResult(Write(buffer, 0, buffer == null ? 0 : buffer.Length));

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
                _stream.Close();
            }
        }

        private void FlushWriteBuffer()
        {
            if (_writePosition == 0)
                return;
            int total = 0;
            while (total < _writePosition)
            {
                int written = _stream.Write(_buffer, total, _writePosition - total);
                if (written <= 0)
                    throw new IOException("The underlying stream did not accept the write.");
                total += written;
            }
            _writePosition = 0;
        }

        private void DiscardReadBuffer()
        {
            if (_readPosition == _readLength)
            {
                _readPosition = 0;
                _readLength = 0;
                return;
            }
            EnsureSeekable();
            _stream.Position -= _readLength - _readPosition;
            _readPosition = 0;
            _readLength = 0;
        }

        private void EnsureOpen()
        {
            if (_closed)
                throw new IOException("The buffered stream is closed.");
        }

        private void EnsureSeekable()
        {
            if (!_stream.CanSeek)
                throw new NotSupportedException("The underlying stream does not support seeking.");
        }

        private static void ValidateRange(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("The buffer cannot be null.");
            if (offset < 0 || count < 0 || offset > buffer.Length - count)
                throw new ArgumentException("The buffer range is invalid.");
        }
    }
}
