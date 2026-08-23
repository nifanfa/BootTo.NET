using System.Threading.Tasks;

namespace System.IO
{
    public abstract class Stream : IDisposable
    {
        public virtual bool CanRead => false;
        public virtual bool CanSeek => false;
        public virtual bool CanWrite => false;
        public virtual int Length => throw new NotSupportedException();
        public virtual long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public virtual long Seek(long offset, SeekOrigin origin)
            => throw new NotSupportedException();

        public virtual void SetLength(long value)
            => throw new NotSupportedException();

        public virtual int ReadByte()
        {
            byte[] buffer = new byte[1];
            return Read(buffer, 0, 1) == 0 ? -1 : buffer[0];
        }

        public virtual void WriteByte(byte value)
        {
            byte[] buffer = new byte[] { value };
            Write(buffer, 0, 1);
        }
        public virtual int Read(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException();

            return Read(buffer, 0, buffer.Length);
        }

        public virtual int Read(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public virtual Task<int> ReadAsync(byte[] buffer) => Task.FromException<int>(new NotSupportedException());
        public virtual Task<int> ReadAsync(byte[] buffer, int offset, int count)
            => Task.FromException<int>(new NotSupportedException());

        public virtual int Write(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException();

            return Write(buffer, 0, buffer.Length);
        }

        public virtual int Write(byte[] buffer, int offset, int count)
            => throw new NotSupportedException();

        public virtual Task<int> WriteAsync(byte[] buffer) => Task.FromException<int>(new NotSupportedException());
        public virtual Task<int> WriteAsync(byte[] buffer, int offset, int count)
            => Task.FromException<int>(new NotSupportedException());

        public virtual void Flush() => throw new NotSupportedException();
        public virtual Task FlushAsync() => Task.FromException(new NotSupportedException());
        public virtual void Close() { }
        public virtual void Dispose() => Close();
    }
}
