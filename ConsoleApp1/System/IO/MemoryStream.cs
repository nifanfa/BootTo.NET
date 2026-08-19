using System.Threading.Tasks;

namespace System.IO
{
    public class MemoryStream : Stream
    {
        private byte[] _buffer;
        private int _position;
        private int _length;
        private readonly bool _expandable;
        private readonly bool _writable;
        private bool _open;

        public MemoryStream()
        {
            _buffer = new byte[0];
            _expandable = true;
            _writable = true;
            _open = true;
        }

        public MemoryStream(byte[] buffer)
            : this(buffer, true)
        {
        }

        public MemoryStream(byte[] buffer, bool writable)
        {
            if (buffer == null)
                throw new ArgumentNullException();

            _buffer = buffer;
            _length = buffer.Length;
            _expandable = false;
            _writable = writable;
            _open = true;
        }

        public MemoryStream(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException();

            _buffer = new byte[capacity];
            _expandable = true;
            _writable = true;
            _open = true;
        }

        public override bool CanRead => _open;
        public override bool CanSeek => _open;
        public override bool CanWrite => _open && _writable;
        public override int Length
        {
            get
            {
                EnsureOpen();
                return _length;
            }
        }

        public int Position
        {
            get
            {
                EnsureOpen();
                return _position;
            }
            set
            {
                EnsureOpen();
                if (value < 0)
                    throw new ArgumentException();

                _position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            EnsureOpen();
            ValidateRange(buffer, offset, count);

            if (_position >= _length || count == 0)
                return 0;

            int available = _length - _position;
            if (count > available)
                count = available;

            for (int i = 0; i < count; i++)
                buffer[offset + i] = _buffer[_position + i];

            _position += count;
            return count;
        }

        public override Task<int> ReadAsync(byte[] buffer)
            => Task.FromResult(Read(buffer, 0, buffer == null ? 0 : buffer.Length));

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count)
            => Task.FromResult(Read(buffer, offset, count));

        public override int Write(byte[] buffer, int offset, int count)
        {
            EnsureOpen();
            EnsureWritable();
            ValidateRange(buffer, offset, count);

            if (count == 0)
                return 0;

            int required = _position + count;
            if (required < _position)
                throw new ArgumentException();

            EnsureCapacity(required);

            if (_position > _length)
            {
                for (int i = _length; i < _position; i++)
                    _buffer[i] = 0;
            }

            for (int i = 0; i < count; i++)
                _buffer[_position + i] = buffer[offset + i];

            _position = required;
            if (_position > _length)
                _length = _position;

            return count;
        }

        public override Task<int> WriteAsync(byte[] buffer)
            => Task.FromResult(Write(buffer, 0, buffer == null ? 0 : buffer.Length));

        public override Task<int> WriteAsync(byte[] buffer, int offset, int count)
            => Task.FromResult(Write(buffer, offset, count));

        public override void Flush()
        {
            EnsureOpen();
        }

        public override Task FlushAsync()
        {
            Flush();
            return Task.CompletedTask;
        }

        public byte[] ToArray()
        {
            EnsureOpen();
            byte[] result = new byte[_length];
            for (int i = 0; i < _length; i++)
                result[i] = _buffer[i];

            return result;
        }

        public override void Close()
        {
            _open = false;
        }

        private void EnsureCapacity(int required)
        {
            if (required <= _buffer.Length)
                return;

            if (!_expandable)
                throw new NotSupportedException();

            int capacity = _buffer.Length == 0 ? 256 : _buffer.Length * 2;
            if (capacity < required)
                capacity = required;

            byte[] resized = new byte[capacity];
            for (int i = 0; i < _length; i++)
                resized[i] = _buffer[i];

            _buffer = resized;
        }

        private void EnsureOpen()
        {
            if (!_open)
                throw new IOException();
        }

        private void EnsureWritable()
        {
            if (!_writable)
                throw new NotSupportedException();
        }

        private static void ValidateRange(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (offset < 0 || count < 0 || offset > buffer.Length - count)
                throw new ArgumentException();
        }
    }
}
