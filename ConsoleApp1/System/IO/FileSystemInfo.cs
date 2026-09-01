namespace System.IO
{
    public abstract class FileSystemInfo : IDisposable
    {
        public abstract string FullName { get; }
        public abstract string Name { get; }
        public abstract bool Exists { get; }

        public virtual FileAttributes Attributes
        {
            get => File.GetAttributes(FullName);
            set => File.SetAttributes(FullName, value);
        }

        public virtual DateTime CreationTime
        {
            get => File.GetCreationTime(FullName);
            set => File.SetCreationTime(FullName, value);
        }

        public virtual DateTime LastAccessTime
        {
            get => File.GetLastAccessTime(FullName);
            set => File.SetLastAccessTime(FullName, value);
        }

        public virtual DateTime LastWriteTime
        {
            get => File.GetLastWriteTime(FullName);
            set => File.SetLastWriteTime(FullName, value);
        }

        public virtual DateTime CreationTimeUtc
        {
            get => CreationTime;
            set => CreationTime = value;
        }

        public virtual DateTime LastAccessTimeUtc
        {
            get => LastAccessTime;
            set => LastAccessTime = value;
        }

        public virtual DateTime LastWriteTimeUtc
        {
            get => LastWriteTime;
            set => LastWriteTime = value;
        }

        public abstract void Delete();

        public virtual void Refresh() { }
        public virtual void Dispose() { }

    }
}
