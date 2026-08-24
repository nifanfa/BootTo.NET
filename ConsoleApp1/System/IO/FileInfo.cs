namespace System.IO
{
    public sealed class FileInfo
    {
        private readonly string _fullName;

        public FileInfo(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("The file name cannot be null or empty.");
            _fullName = fileName;
        }

        public string FullName => _fullName;

        public string Name
        {
            get
            {
                int slash = -1;
                for (int i = 0; i < _fullName.Length; i++)
                    if (_fullName[i] == '\\' || _fullName[i] == '/')
                        slash = i;
                return slash < 0 ? _fullName : Substring(_fullName, slash + 1);
            }
        }

        public bool Exists
        {
            get
            {
                if (!UefiFileSystem.TryGetInfo(_fullName, out UefiFileMetadata metadata))
                    return false;
                return (metadata.Attribute & EFI_FILE_DIRECTORY) == 0;
            }
        }

        public long Length
        {
            get
            {
                if (!UefiFileSystem.TryGetInfo(_fullName, out UefiFileMetadata metadata))
                    throw new IOException("The file metadata could not be read.");
                return (long)metadata.FileSize;
            }
        }

        public DateTime CreationTime
            => ToDateTime(GetMetadata().CreateTime);

        public DateTime LastWriteTime
            => ToDateTime(GetMetadata().ModificationTime);

        public DateTime LastAccessTime
            => ToDateTime(GetMetadata().LastAccessTime);

        public void Delete()
        {
            if (!UefiFileSystem.Delete(_fullName))
                throw new IOException("The file could not be deleted.");
        }

        public FileStream OpenRead()
            => new FileStream(_fullName, FileMode.Open);

        public FileStream OpenWrite()
            => new FileStream(_fullName, FileMode.OpenOrCreate);

        private UefiFileMetadata GetMetadata()
        {
            if (!UefiFileSystem.TryGetInfo(_fullName, out UefiFileMetadata metadata))
                throw new IOException("The file metadata could not be read.");
            return metadata;
        }

        private static DateTime ToDateTime(EFI_TIME time)
            => new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, (int)(time.Nanosecond / 1000000));

        private static string Substring(string value, int start)
        {
            char[] chars = new char[value.Length - start];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = value[start + i];
            return new string(chars);
        }
    }
}
