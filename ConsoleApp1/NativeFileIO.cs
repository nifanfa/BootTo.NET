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
    private static readonly string[][] s_findFiles = new string[MaximumOpenFiles][];
    private static readonly int[] s_findPositions = new int[MaximumOpenFiles];

    [RuntimeExport("BTDN_FindFirst")]
    public static int FindFirst(byte* pattern, byte* name, ulong* modified)
    {
        int slot = 0;
        while (slot < MaximumOpenFiles && s_findFiles[slot] != null)
            slot++;
        if (slot == MaximumOpenFiles || name == null || modified == null)
            return 0;
        try
        {
            string path = NormalizePath(DecodeAscii(pattern));
            if (path[0] != '\\')
                path = Path.Combine(@"\Grand Theft Auto 3", path);
            string directory = Path.GetDirectoryName(path);
            string mask = Path.GetFileName(path);
            string[] files = Directory.GetFiles(string.IsNullOrEmpty(directory) ? @"\" : directory, mask);
            if (files.Length == 0)
                return 0;
            s_findFiles[slot] = files;
            s_findPositions[slot] = 0;
            CopyFindResult(files[0], name, modified);
            return slot + 1;
        }
        catch
        {
            s_findFiles[slot] = null;
            return 0;
        }
    }

    [RuntimeExport("BTDN_FindNext")]
    public static int FindNext(int handle, byte* name, ulong* modified)
    {
        int slot = handle - 1;
        if ((uint)slot >= MaximumOpenFiles || s_findFiles[slot] == null || name == null || modified == null)
            return 0;
        int position = s_findPositions[slot] + 1;
        if (position >= s_findFiles[slot].Length)
            return 0;
        s_findPositions[slot] = position;
        CopyFindResult(s_findFiles[slot][position], name, modified);
        return 1;
    }

    [RuntimeExport("BTDN_FindClose")]
    public static void FindClose(int handle)
    {
        int slot = handle - 1;
        if ((uint)slot < MaximumOpenFiles)
            s_findFiles[slot] = null;
    }

    private static void CopyFindResult(string path, byte* name, ulong* modified)
    {
        string filename = Path.GetFileName(path);
        int length = filename.Length < 259 ? filename.Length : 259;
        for (int index = 0; index < length; index++)
            name[index] = (byte)filename[index];
        name[length] = 0;
        *modified = (ulong)(File.GetLastWriteTime(path).Ticks - 504911232000000000L);
    }

    private sealed class OpenFile
    {
        internal readonly FileStream Stream;
        internal readonly bool CanRead;
        internal readonly bool CanWrite;

        internal OpenFile(FileStream stream, bool canRead, bool canWrite)
        {
            Stream = stream;
            CanRead = canRead;
            CanWrite = canWrite;
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
            bool canRead = operation == 'r' || update;
            bool canWrite = operation != 'r' || update;
            if (canWrite)
                EnsureParentDirectory(filePath);
            FileMode fileMode = operation == 'r' ? FileMode.Open :
                operation == 'w' ? FileMode.Create : FileMode.OpenOrCreate;
            FileAccess access = canRead && canWrite ? FileAccess.ReadWrite :
                canRead ? FileAccess.Read : FileAccess.Write;
            FileStream stream = new FileStream(filePath, fileMode, access, FileShare.Read);
            if (operation == 'a')
                stream.Position = stream.Length;
            s_openFiles[slot] = new OpenFile(stream, canRead, canWrite);
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
        if (length == 0)
            return 0;
        try
        {
            int total = 0;
            while (total < length)
            {
                int count = file.Stream.Read(destination + total, Math.Min(length - total, 65536));
                total += count;
                if (count == 0)
                    break;
            }
            return total;
        }
        catch { return -1; }
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
            byte[] buffer = new byte[length < 65536 ? length : 65536];
            int total = 0;
            while (total < length)
            {
                int count = length - total < buffer.Length ? length - total : buffer.Length;
                for (int i = 0; i < count; i++)
                    buffer[i] = source[total + i];
                file.Stream.Write(buffer, 0, count);
                total += count;
            }
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

        try
        {
            if (origin < 0 || origin > 2)
                return -1;
            long position = file.Stream.Seek(offset, (SeekOrigin)origin);
            return position <= int.MaxValue ? (int)position : -1;
        }
        catch { return -1; }
    }

    [RuntimeExport("BTDN_FileTell")]
    public static int FileTell(int handle)
    {
        OpenFile file = GetFile(handle);
        if (file == null) return -1;
        try { return file.Stream.Position <= int.MaxValue ? (int)file.Stream.Position : -1; }
        catch { return -1; }
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
            file.Stream.Dispose();
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
        OpenFile file = GetFile(handle);
        if (file == null) return -1;
        try { if (file.CanWrite) file.Stream.Flush(); return 0; }
        catch { return -1; }
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

    [RuntimeExport("BTDN_DirectoryExists")]
    public static int DirectoryExists(byte* path)
    {
        string directory = NormalizePath(DecodeAscii(path));
        return !string.IsNullOrEmpty(directory) && Directory.Exists(directory) ? 1 : 0;
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
