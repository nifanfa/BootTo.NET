using System.Threading.Tasks;

namespace System.IO
{
    public abstract class Stream
    {
        public virtual int Length => throw new NotSupportedException();
        public virtual int Read(byte[] buffer) => throw new NotSupportedException();
        public virtual Task<int> ReadAsync(byte[] buffer) => Task.FromException<int>(new NotSupportedException());
        public virtual int Write(byte[] buffer) => throw new NotSupportedException();
        public virtual Task<int> WriteAsync(byte[] buffer) => Task.FromException<int>(new NotSupportedException());
        public virtual void Flush() => throw new NotSupportedException();
        public virtual Task FlushAsync() => Task.FromException(new NotSupportedException());
        public virtual void Close() => throw new NotSupportedException();
    }
}
