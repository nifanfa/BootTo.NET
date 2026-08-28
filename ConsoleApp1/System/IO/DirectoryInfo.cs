using System.Collections.Generic;

namespace System.IO
{
    public sealed class DirectoryInfo : FileSystemInfo
    {
        private readonly string _fullName;

        public DirectoryInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The directory path cannot be null or empty.");
            _fullName = path;
        }

        public override string FullName => _fullName;
        public override string Name => Path.GetFileName(_fullName);
        public override bool Exists => Directory.Exists(_fullName);

        public DirectoryInfo Parent
        {
            get
            {
                string parent = Path.GetDirectoryName(_fullName);
                return string.IsNullOrEmpty(parent) ? null : new DirectoryInfo(parent);
            }
        }

        public DirectoryInfo Root
        {
            get
            {
                string root = Path.GetPathRoot(_fullName);
                return string.IsNullOrEmpty(root) ? new DirectoryInfo(".") : new DirectoryInfo(root);
            }
        }

        public void Create() => Directory.CreateDirectory(_fullName);

        public DirectoryInfo CreateSubdirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The subdirectory path cannot be null or empty.");
            DirectoryInfo result = new DirectoryInfo(Path.Combine(_fullName, path));
            result.Create();
            return result;
        }

        public FileInfo[] GetFiles()
            => GetFiles("*", SearchOption.TopDirectoryOnly);

        public FileInfo[] GetFiles(string searchPattern)
            => GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

        public FileInfo[] GetFiles(string searchPattern, SearchOption searchOption)
        {
            string[] paths = Directory.GetFiles(_fullName, searchPattern, searchOption);
            FileInfo[] result = new FileInfo[paths.Length];
            for (int i = 0; i < paths.Length; i++)
                result[i] = new FileInfo(paths[i]);
            return result;
        }

        public DirectoryInfo[] GetDirectories()
            => GetDirectories("*", SearchOption.TopDirectoryOnly);

        public DirectoryInfo[] GetDirectories(string searchPattern)
            => GetDirectories(searchPattern, SearchOption.TopDirectoryOnly);

        public DirectoryInfo[] GetDirectories(string searchPattern, SearchOption searchOption)
        {
            string[] paths = Directory.GetDirectories(_fullName, searchPattern, searchOption);
            DirectoryInfo[] result = new DirectoryInfo[paths.Length];
            for (int i = 0; i < paths.Length; i++)
                result[i] = new DirectoryInfo(paths[i]);
            return result;
        }

        public IEnumerable<FileInfo> EnumerateFiles()
            => GetFiles();

        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern)
            => GetFiles(searchPattern);

        public IEnumerable<FileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
            => GetFiles(searchPattern, searchOption);

        public IEnumerable<DirectoryInfo> EnumerateDirectories()
            => GetDirectories();

        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern)
            => GetDirectories(searchPattern);

        public IEnumerable<DirectoryInfo> EnumerateDirectories(string searchPattern, SearchOption searchOption)
            => GetDirectories(searchPattern, searchOption);

        public override void Delete() => Directory.Delete(_fullName);
        public void Delete(bool recursive) => Directory.Delete(_fullName, recursive);
    }
}
