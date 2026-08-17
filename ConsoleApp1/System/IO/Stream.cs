namespace System.IO
{
    public abstract class Stream
    {
        public virtual int Length => throw new NotSupportedException();
        public virtual int Read(byte[] buffer) => throw new NotSupportedException();
        public virtual int Write(byte[] buffer) => throw new NotSupportedException();
        public virtual void Flush() => throw new NotSupportedException();
        public virtual void Close() => throw new NotSupportedException();
    }
}
