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

        public virtual DateTime CreationTime => ToDateTime(GetMetadata().CreateTime);
        public virtual DateTime LastAccessTime => ToDateTime(GetMetadata().LastAccessTime);
        public virtual DateTime LastWriteTime => ToDateTime(GetMetadata().ModificationTime);

        public abstract void Delete();

        public virtual void Refresh() { }
        public virtual void Dispose() { }

        private File.FileMetadata GetMetadata()
        {
            if (!File.TryGetInfo(FullName, out File.FileMetadata metadata))
                throw new IOException("The file metadata could not be read.");
            return metadata;
        }

        private static DateTime ToDateTime(EFI_TIME time)
            => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute,
                time.Second, (int)(time.Nanosecond / 1000000));
    }
}
