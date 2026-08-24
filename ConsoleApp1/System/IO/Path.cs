namespace System.IO
{
    public static class Path
    {
        public const char DirectorySeparatorChar = '\\';
        public const char AltDirectorySeparatorChar = '/';
        public const char VolumeSeparatorChar = ':';
        public const char PathSeparator = ';';

        public static string ChangeExtension(string path, string extension)
        {
            if (path == null)
                throw new ArgumentNullException("The path cannot be null.");

            int end = TrimTrailingSeparators(path);
            int separator = LastSeparator(path, end);
            int dot = LastIndexOf(path, '.', end);
            if (dot <= separator)
                dot = end;

            string prefix = Slice(path, 0, dot);
            if (extension == null || extension.Length == 0)
                return prefix;
            if (extension[0] == '.')
                return string.Concat(prefix, extension);
            return string.Concat(prefix, ".", extension);
        }

        public static string Combine(string path1, string path2)
        {
            if (path1 == null || path2 == null)
                throw new ArgumentNullException("Path components cannot be null.");
            if (path1.Length == 0)
                return path2;
            if (path2.Length == 0)
                return path1;
            if (IsPathRooted(path2))
                return path2;

            char last = path1[path1.Length - 1];
            if (IsSeparator(last))
                return string.Concat(path1, path2);
            return string.Concat(path1, DirectorySeparatorChar.ToString(), path2);
        }

        public static string Combine(string path1, string path2, string path3)
            => Combine(Combine(path1, path2), path3);

        public static string GetDirectoryName(string path)
        {
            if (path == null)
                throw new ArgumentNullException("The path cannot be null.");
            int end = TrimTrailingSeparators(path);
            if (end == 0)
                return path.Length == 0 ? null : Slice(path, 0, 1);

            int separator = LastSeparator(path, end);
            if (separator < 0)
                return null;
            if (separator == 0)
                return Slice(path, 0, 1);
            return Slice(path, 0, separator);
        }

        public static string GetFileName(string path)
        {
            if (path == null)
                throw new ArgumentNullException("The path cannot be null.");
            int end = TrimTrailingSeparators(path);
            int separator = LastSeparator(path, end);
            return Slice(path, separator + 1, end - separator - 1);
        }

        public static string GetFileNameWithoutExtension(string path)
            => RemoveExtension(GetFileName(path));

        public static string GetExtension(string path)
        {
            if (path == null)
                throw new ArgumentNullException("The path cannot be null.");
            int end = TrimTrailingSeparators(path);
            int separator = LastSeparator(path, end);
            int dot = LastIndexOf(path, '.', end);
            if (dot <= separator || dot == end - 1)
                return string.Empty;
            return Slice(path, dot, end - dot);
        }

        public static string GetPathRoot(string path)
        {
            if (path == null)
                throw new ArgumentNullException("The path cannot be null.");
            if (path.Length == 0)
                return null;
            if (IsSeparator(path[0]))
                return Slice(path, 0, 1);
            if (path.Length >= 2 && path[1] == VolumeSeparatorChar)
            {
                if (path.Length >= 3 && IsSeparator(path[2]))
                    return Slice(path, 0, 3);
                return Slice(path, 0, 2);
            }
            return null;
        }

        public static bool HasExtension(string path)
            => GetExtension(path).Length != 0;

        public static bool IsPathRooted(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            return IsSeparator(path[0]) || (path.Length > 1 && path[1] == VolumeSeparatorChar);
        }

        public static string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The path cannot be null or empty.");
            return path;
        }

        private static string RemoveExtension(string value)
        {
            int dot = LastIndexOf(value, '.', value.Length);
            return dot <= 0 ? value : Slice(value, 0, dot);
        }

        private static int TrimTrailingSeparators(string value)
        {
            int end = value.Length;
            while (end > 0 && IsSeparator(value[end - 1]))
                end--;
            return end;
        }

        private static int LastSeparator(string value, int end)
        {
            for (int i = end - 1; i >= 0; i--)
                if (IsSeparator(value[i]))
                    return i;
            return -1;
        }

        private static int LastIndexOf(string value, char character, int end)
        {
            for (int i = end - 1; i >= 0; i--)
                if (value[i] == character)
                    return i;
            return -1;
        }

        private static bool IsSeparator(char value)
            => value == DirectorySeparatorChar || value == AltDirectorySeparatorChar;

        private static string Slice(string value, int start, int length)
        {
            if (length <= 0)
                return string.Empty;
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = value[start + i];
            return new string(result);
        }
    }
}
