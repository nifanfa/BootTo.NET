namespace System.IO
{
    public sealed class FileInfo : FileSystemInfo
    {
        private readonly string _fullName;

        public FileInfo(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("The file name cannot be null or empty.");
            _fullName = fileName;
        }

        public override string FullName => _fullName;

        public string DirectoryName => Path.GetDirectoryName(_fullName);

        public DirectoryInfo Directory
        {
            get
            {
                string directoryName = DirectoryName;
                return directoryName == null ? null : new DirectoryInfo(directoryName);
            }
        }

        public string Extension => Path.GetExtension(_fullName);

        public override string Name
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

        public override bool Exists
        {
            get
            {
                if (!File.TryGetInfo(_fullName, out File.FileMetadata metadata))
                    return false;
                return (metadata.Attribute & EFI_FILE_DIRECTORY) == 0;
            }
        }

        public long Length
        {
            get
            {
                if (!File.TryGetInfo(_fullName, out File.FileMetadata metadata))
                    throw new IOException("The file metadata could not be read.");
                return (long)metadata.FileSize;
            }
        }

        public bool IsReadOnly
        {
            get => (Attributes & FileAttributes.ReadOnly) != 0;
            set
            {
                FileAttributes attributes = Attributes;
                Attributes = value
                    ? attributes | FileAttributes.ReadOnly
                    : attributes & ~FileAttributes.ReadOnly;
            }
        }

        private File.FileMetadata GetMetadata()
        {
            if (!File.TryGetInfo(_fullName, out File.FileMetadata metadata))
                throw new IOException("The file metadata could not be read.");
            return metadata;
        }

        public override void Delete()
        {
            if (!File.TryDelete(_fullName))
                throw new IOException("The file could not be deleted.");
        }

        public FileStream OpenRead()
            => new FileStream(_fullName, FileMode.Open, FileAccess.Read, FileShare.Read);

        public FileStream OpenWrite()
            => new FileStream(_fullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

        public FileStream Create()
            => new FileStream(_fullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

        public FileStream Open(FileMode mode, FileAccess access, FileShare share)
            => new FileStream(_fullName, mode, access, share);

        public StreamReader OpenText()
            => new StreamReader(_fullName);

        public StreamWriter CreateText()
            => new StreamWriter(_fullName, false);

        public StreamWriter AppendText()
            => new StreamWriter(_fullName, true);

        public FileInfo CopyTo(string destFileName)
            => CopyTo(destFileName, false);

        public FileInfo CopyTo(string destFileName, bool overwrite)
        {
            File.Copy(_fullName, destFileName, overwrite);
            return new FileInfo(destFileName);
        }

        public void MoveTo(string destFileName)
            => File.Move(_fullName, destFileName);

        public void MoveTo(string destFileName, bool overwrite)
            => File.Move(_fullName, destFileName, overwrite);

        private static string Substring(string value, int start)
        {
            char[] chars = new char[value.Length - start];
            for (int i = 0; i < chars.Length; i++)
                chars[i] = value[start + i];
            return new string(chars);
        }
    }
}
