namespace System.IO
{
    public static class Directory
    {
        public static bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (!File.TryGetInfo(path, out File.FileMetadata metadata))
                return false;
            return (metadata.Attribute & EFI_FILE_DIRECTORY) != 0;
        }

        public static void CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The directory path cannot be null or empty.");
            if (!File.TryCreateDirectory(path))
                throw new IOException("The directory could not be created.");
        }

        public static void Delete(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The directory path cannot be null or empty.");
            if (!File.TryDelete(path))
                throw new IOException("The directory could not be deleted.");
        }

        public static string[] GetFiles(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The directory path cannot be null or empty.");
            return AddDirectoryPrefix(path, File.ReadDirectory(path, false));
        }

        public static string[] GetDirectories(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The directory path cannot be null or empty.");
            return AddDirectoryPrefix(path, File.ReadDirectory(path, true));
        }

        private static string[] AddDirectoryPrefix(string path, string[] names)
        {
            char last = path[path.Length - 1];
            bool hasSeparator = last == '\\' || last == '/';
            string separator = hasSeparator ? string.Empty : "\\";
            string[] result = new string[names.Length];
            for (int i = 0; i < names.Length; i++)
                result[i] = string.Concat(path, separator, names[i]);
            return result;
        }
    }
}
