using System.Collections.Generic;

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
            if (Exists(path))
                return;

            string parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent) && !Exists(parent))
                CreateDirectory(parent);
            if (!File.TryCreateDirectory(path))
                throw new IOException("The directory could not be created.");
        }

        public static void Delete(string path)
            => Delete(path, false);

        public static void Delete(string path, bool recursive)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The directory path cannot be null or empty.");
            if (recursive)
            {
                string[] files = File.ReadDirectory(path, false);
                for (int i = 0; i < files.Length; i++)
                    File.Delete(Path.Combine(path, files[i]));

                string[] directories = File.ReadDirectory(path, true);
                for (int i = 0; i < directories.Length; i++)
                    Delete(Path.Combine(path, directories[i]), true);
            }
            if (!File.TryDelete(path))
                throw new IOException("The directory could not be deleted.");
        }

        public static string[] GetFiles(string path)
            => GetFiles(path, "*", SearchOption.TopDirectoryOnly);

        public static string[] GetFiles(string path, string searchPattern)
            => GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
            => GetEntries(path, searchPattern, searchOption, false);

        public static string[] GetDirectories(string path)
            => GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

        public static string[] GetDirectories(string path, string searchPattern)
            => GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);

        public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
            => GetEntries(path, searchPattern, searchOption, true);

        public static string[] GetFileSystemEntries(string path)
            => GetFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly);

        public static string[] GetFileSystemEntries(string path, string searchPattern)
            => GetFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly);

        public static string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
        {
            ValidatePathAndSearch(path, searchPattern, searchOption);
            List<string> result = new List<string>();
            AddEntries(path, searchPattern, searchOption, result, false, true);
            return result.ToArray();
        }

        public static IEnumerable<string> EnumerateFiles(string path)
            => GetFiles(path);

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
            => GetFiles(path, searchPattern);

        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
            => GetFiles(path, searchPattern, searchOption);

        public static IEnumerable<string> EnumerateDirectories(string path)
            => GetDirectories(path);

        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
            => GetDirectories(path, searchPattern);

        public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
            => GetDirectories(path, searchPattern, searchOption);

        public static IEnumerable<string> EnumerateFileSystemEntries(string path)
            => GetFileSystemEntries(path);

        public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
            => GetFileSystemEntries(path, searchPattern);

        public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
            => GetFileSystemEntries(path, searchPattern, searchOption);

        public static string GetCurrentDirectory() => ".";

        private static string[] GetEntries(string path, string searchPattern, SearchOption searchOption, bool directoriesOnly)
        {
            ValidatePathAndSearch(path, searchPattern, searchOption);
            List<string> result = new List<string>();
            AddEntries(path, searchPattern, searchOption, result, directoriesOnly, false);
            return result.ToArray();
        }

        private static void AddEntries(string path, string searchPattern, SearchOption searchOption,
            List<string> result, bool directoriesOnly, bool includeFilesAndDirectories)
        {
            string[] names = File.ReadDirectory(path, false);
            if (!directoriesOnly || includeFilesAndDirectories)
            {
                for (int i = 0; i < names.Length; i++)
                    if (Matches(names[i], searchPattern))
                        result.Add(Path.Combine(path, names[i]));
            }

            string[] directories = File.ReadDirectory(path, true);
            if (directoriesOnly || includeFilesAndDirectories)
            {
                for (int i = 0; i < directories.Length; i++)
                    if (Matches(directories[i], searchPattern))
                        result.Add(Path.Combine(path, directories[i]));
            }

            if (searchOption != SearchOption.AllDirectories)
                return;
            for (int i = 0; i < directories.Length; i++)
                AddEntries(Path.Combine(path, directories[i]), searchPattern, searchOption,
                    result, directoriesOnly, includeFilesAndDirectories);
        }

        private static void ValidatePathAndSearch(string path, string searchPattern, SearchOption searchOption)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The directory path cannot be null or empty.");
            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentException("The search pattern cannot be null or empty.");
            if (searchOption != SearchOption.TopDirectoryOnly && searchOption != SearchOption.AllDirectories)
                throw new ArgumentOutOfRangeException("The search option is invalid.");
        }

        private static bool Matches(string value, string pattern)
        {
            if (pattern == "*.*")
                return true;

            int valueIndex = 0;
            int patternIndex = 0;
            int starIndex = -1;
            int retryIndex = 0;
            while (valueIndex < value.Length)
            {
                if (patternIndex < pattern.Length &&
                    (pattern[patternIndex] == '?' || SameCharacter(value[valueIndex], pattern[patternIndex])))
                {
                    valueIndex++;
                    patternIndex++;
                }
                else if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    starIndex = patternIndex++;
                    retryIndex = valueIndex;
                }
                else if (starIndex >= 0)
                {
                    patternIndex = starIndex + 1;
                    valueIndex = ++retryIndex;
                }
                else
                {
                    return false;
                }
            }

            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                patternIndex++;
            return patternIndex == pattern.Length;
        }

        private static bool SameCharacter(char left, char right)
        {
            if (left == right)
                return true;
            if (left >= 'A' && left <= 'Z')
                left = (char)(left + ('a' - 'A'));
            if (right >= 'A' && right <= 'Z')
                right = (char)(right + ('a' - 'A'));
            return left == right;
        }
    }
}
