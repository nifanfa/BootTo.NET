using System.Threading.Tasks;

namespace System.IO
{
    public abstract class Stream
    {
        public virtual bool CanRead => false;
        public virtual bool CanSeek => false;
        public virtual bool CanWrite => false;
        public virtual int Length => throw new NotSupportedException();
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
        public virtual void Close() => throw new NotSupportedException();
    }
}
