using System;
using System.IO;
using System.Runtime;

/// <summary>
/// NativeFileIO for Doom and Quake file operations.
/// </summary>
internal static unsafe class NativeFileIO
{
    private const int MaximumOpenFiles = 64;
    private static readonly OpenFile[] s_openFiles = new OpenFile[MaximumOpenFiles];

    private sealed class OpenFile
    {
        internal readonly string Path;
        internal byte[] Buffer;
        internal int Length;
        internal int Position;
        internal readonly bool CanRead;
        internal readonly bool CanWrite;
        internal bool Dirty;

        internal OpenFile(string path, byte[] buffer, bool canRead, bool canWrite, bool append, bool dirty)
        {
            Path = path;
            Buffer = buffer;
            Length = buffer.Length;
            Position = append ? Length : 0;
            CanRead = canRead;
            CanWrite = canWrite;
            Dirty = dirty;
        }

        internal void EnsureCapacity(int required)
        {
            if (required <= Buffer.Length)
                return;

            int capacity = Buffer.Length == 0 ? 4096 : Buffer.Length;
            while (capacity < required)
            {
                int next = capacity <= int.MaxValue / 2 ? capacity * 2 : int.MaxValue;
                if (next == capacity)
                    throw new IOException("The file is too large.");
                capacity = next;
            }

            byte[] resized = new byte[capacity];
            for (int i = 0; i < Length; i++)
                resized[i] = Buffer[i];
            Buffer = resized;
        }

        internal byte[] GetContents()
        {
            byte[] contents = new byte[Length];
            for (int i = 0; i < Length; i++)
                contents[i] = Buffer[i];
            return contents;
        }
    }

    [RuntimeExport("BTDN_FileOpen")]
    public static int FileOpen(byte* path, byte* mode)
    {
        string filePath = NormalizePath(DecodeAscii(path));
        string openMode = DecodeAscii(mode);
        if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(openMode))
            return 0;

        int slot = FindFreeFileSlot();
        if (slot < 0)
            return 0;

        try
        {
            char operation = openMode[0];
            if (operation != 'r' && operation != 'w' && operation != 'a')
                return 0;

            bool update = Contains(openMode, '+');
            bool exists = File.Exists(filePath);
            if (operation == 'r' && !exists)
                return 0;

            bool canRead = operation == 'r' || update;
            bool canWrite = operation != 'r' || update;
            bool append = operation == 'a';
            bool truncate = operation == 'w';
            byte[] contents = !truncate && exists ? File.ReadAllBytes(filePath) : new byte[0];

            if (canWrite)
                EnsureParentDirectory(filePath);

            s_openFiles[slot] = new OpenFile(
                filePath,
                contents,
                canRead,
                canWrite,
                append,
                truncate || (append && !exists));
            return slot + 1;
        }
        catch
        {
            return 0;
        }
    }

    [RuntimeExport("BTDN_FileRead")]
    public static int FileRead(int handle, byte* destination, int length)
    {
        OpenFile file = GetFile(handle);
        if (file == null || !file.CanRead || length < 0 || (destination == null && length != 0))
            return -1;
        if (length == 0 || file.Position >= file.Length)
            return 0;

        int count = file.Length - file.Position;
        if (count > length)
            count = length;
        for (int i = 0; i < count; i++)
            destination[i] = file.Buffer[file.Position + i];
        file.Position += count;
        return count;
    }

    [RuntimeExport("BTDN_FileWrite")]
    public static int FileWrite(int handle, byte* source, int length)
    {
        OpenFile file = GetFile(handle);
        if (file == null || !file.CanWrite || length < 0 || (source == null && length != 0))
            return -1;
        if (length == 0)
            return 0;

        try
        {
            int required = checked(file.Position + length);
            file.EnsureCapacity(required);
            for (int i = file.Length; i < file.Position; i++)
                file.Buffer[i] = 0;
            for (int i = 0; i < length; i++)
                file.Buffer[file.Position + i] = source[i];

            file.Position = required;
            if (file.Length < required)
                file.Length = required;
            file.Dirty = true;
            return length;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("BTDN_FileSeek")]
    public static int FileSeek(int handle, int offset, int origin)
    {
        OpenFile file = GetFile(handle);
        if (file == null)
            return -1;

        long basis;
        if (origin == 0)
            basis = 0;
        else if (origin == 1)
            basis = file.Position;
        else if (origin == 2)
            basis = file.Length;
        else
            return -1;

        long position = basis + offset;
        if (position < 0 || position > int.MaxValue)
            return -1;
        file.Position = (int)position;
        return (int)position;
    }

    [RuntimeExport("BTDN_FileTell")]
    public static int FileTell(int handle)
    {
        OpenFile file = GetFile(handle);
        return file == null ? -1 : file.Position;
    }

    [RuntimeExport("BTDN_FileClose")]
    public static int FileClose(int handle)
    {
        int index = handle - 1;
        if ((uint)index >= MaximumOpenFiles || s_openFiles[index] == null)
            return -1;

        OpenFile file = s_openFiles[index];
        s_openFiles[index] = null;
        try
        {
            if (file.CanWrite && file.Dirty)
                File.WriteAllBytes(file.Path, file.GetContents());
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("BTDN_FileFlush")]
    public static int FileFlush(int handle)
    {
        // Data is already present in the shared memory buffer. It is committed
        // once on close so Quake's per-edict fflush calls do not rewrite a file.
        return GetFile(handle) == null ? -1 : 0;
    }

    [RuntimeExport("BTDN_FileExists")]
    public static int FileExists(byte* path)
    {
        string filePath = NormalizePath(DecodeAscii(path));
        return !string.IsNullOrEmpty(filePath) && File.Exists(filePath) ? 1 : 0;
    }

    [RuntimeExport("BTDN_FileCreateDirectory")]
    public static int FileCreateDirectory(byte* path)
    {
        try
        {
            string directory = NormalizePath(DecodeAscii(path));
            if (!string.IsNullOrEmpty(directory))
                CreateDirectories(directory);
            return 0;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("BTDN_FileRemove")]
    public static int FileRemove(byte* path)
    {
        try
        {
            string filePath = NormalizePath(DecodeAscii(path));
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return -1;
            File.Delete(filePath);
            return File.Exists(filePath) ? -1 : 0;
        }
        catch
        {
            return -1;
        }
    }

    [RuntimeExport("BTDN_FileRename")]
    public static int FileRename(byte* oldPath, byte* newPath)
    {
        try
        {
            string sourcePath = NormalizePath(DecodeAscii(oldPath));
            string destinationPath = NormalizePath(DecodeAscii(newPath));
            if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath) ||
                !File.Exists(sourcePath))
                return -1;

            EnsureParentDirectory(destinationPath);
            if (File.Exists(destinationPath))
                File.Delete(destinationPath);
            File.Move(sourcePath, destinationPath);
            return File.Exists(destinationPath) && !File.Exists(sourcePath) ? 0 : -1;
        }
        catch
        {
            return -1;
        }
    }

    private static OpenFile GetFile(int handle)
    {
        int index = handle - 1;
        return (uint)index < MaximumOpenFiles ? s_openFiles[index] : null;
    }

    private static int FindFreeFileSlot()
    {
        for (int i = 0; i < MaximumOpenFiles; i++)
        {
            if (s_openFiles[i] == null)
                return i;
        }
        return -1;
    }

    private static bool Contains(string value, char character)
    {
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == character)
                return true;
        }
        return false;
    }

    private static string DecodeAscii(byte* value)
    {
        if (value == null)
            return null;

        int length = 0;
        while (value[length] != 0)
            length++;

        char[] characters = new char[length];
        for (int i = 0; i < length; i++)
            characters[i] = (char)value[i];
        return new string(characters);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        int start = 0;
        while (start + 1 < path.Length && path[start] == '.' &&
               (path[start + 1] == '\\' || path[start + 1] == '/'))
            start += 2;

        int end = path.Length;
        while (end > start && (path[end - 1] == '\\' || path[end - 1] == '/'))
            end--;
        if (end - start == 1 && path[start] == '.')
            return string.Empty;

        char[] normalized = new char[end - start];
        for (int i = 0; i < normalized.Length; i++)
        {
            char value = path[start + i];
            normalized[i] = value == '/' ? '\\' : value;
        }
        return new string(normalized);
    }

    private static void EnsureParentDirectory(string path)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            CreateDirectories(directory);
    }

    private static void CreateDirectories(string path)
    {
        if (Directory.Exists(path))
            return;

        int start = path.Length > 0 && path[0] == '\\' ? 1 : 0;
        for (int i = start; i <= path.Length; i++)
        {
            if (i != path.Length && path[i] != '\\')
                continue;
            if (i == 0)
                continue;

            string part = path.Substring(0, i);
            if (!string.IsNullOrEmpty(part) && !Directory.Exists(part))
                Directory.CreateDirectory(part);
        }
    }
}
