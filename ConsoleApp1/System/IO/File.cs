using System.Collections.Generic;
using System.Text;
namespace System.IO
{
    public static unsafe class File
    {
        internal struct FileMetadata
        {
            internal ulong FileSize;
            internal ulong Attribute;
            internal EFI_TIME CreateTime;
            internal EFI_TIME LastAccessTime;
            internal EFI_TIME ModificationTime;
        }

        public static byte[] ReadAllBytes(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer);
            fs.Close();
            return buffer;
        }

        public static bool Exists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return TryGetInfo(path, out FileMetadata metadata) &&
                (metadata.Attribute & EFI_FILE_DIRECTORY) == 0;
        }

        public static void WriteAllBytes(string path, byte[] buffer)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Write(buffer);
            fs.Flush();
            fs.Close();
        }

        public static void Delete(string path) => TryDelete(path);

        public static FileStream Open(string path, FileMode mode)
            => new FileStream(path, mode);

        public static FileStream Open(string path, FileMode mode, FileAccess access)
            => new FileStream(path, mode, access);

        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
            => new FileStream(path, mode, access, share);

        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
            => Open(path, mode, access, share);

        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share,
            int bufferSize, FileOptions options)
            => Open(path, mode, access, share);

        public static FileStream OpenRead(string path)
            => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

        public static FileStream OpenWrite(string path)
            => new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

        public static FileStream Create(string path)
            => new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);

        public static string ReadAllText(string path)
            => ReadAllText(path, Encoding.UTF8);

        public static string ReadAllText(string path, Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("The encoding cannot be null.");
            string result = encoding.GetString(ReadAllBytes(path));
            return result.Length > 0 && result[0] == '\uFEFF'
                ? result.Substring(1)
                : result;
        }

        public static void WriteAllText(string path, string contents)
            => WriteAllText(path, contents, Encoding.UTF8);

        public static void WriteAllText(string path, string contents, Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("The encoding cannot be null.");
            WriteAllBytes(path, encoding.GetBytes(contents ?? string.Empty));
        }

        public static void AppendAllText(string path, string contents)
            => AppendAllText(path, contents, Encoding.UTF8);

        public static void AppendAllText(string path, string contents, Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("The encoding cannot be null.");
            FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            try
            {
                byte[] data = encoding.GetBytes(contents ?? string.Empty);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            finally
            {
                stream.Close();
            }
        }

        public static string[] ReadAllLines(string path)
            => ReadAllLines(path, Encoding.UTF8);

        public static string[] ReadAllLines(string path, Encoding encoding)
        {
            string text = ReadAllText(path, encoding);
            List<string> lines = new List<string>();
            int start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != '\r' && text[i] != '\n')
                    continue;

                lines.Add(text.Substring(start, i - start));
                if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                start = i + 1;
            }
            if (start < text.Length)
                lines.Add(text.Substring(start));
            return lines.ToArray();
        }

        public static void WriteAllLines(string path, string[] contents)
            => WriteAllLines(path, contents, Encoding.UTF8);

        public static void WriteAllLines(string path, IEnumerable<string> contents)
            => WriteAllLines(path, contents, Encoding.UTF8);

        public static void WriteAllLines(string path, string[] contents, Encoding encoding)
            => WriteAllLines(path, (IEnumerable<string>)contents, encoding);

        public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            if (contents == null)
                throw new ArgumentNullException("The line collection cannot be null.");
            if (encoding == null)
                throw new ArgumentNullException("The encoding cannot be null.");

            Text.StringBuilder text = new Text.StringBuilder();
            int index = 0;
            foreach (string line in contents)
            {
                if (index++ != 0)
                    text.Append("\r\n");
                text.Append(line ?? string.Empty);
            }
            WriteAllText(path, text.ToString(), encoding);
        }

        public static void Copy(string sourceFileName, string destFileName)
            => Copy(sourceFileName, destFileName, false);

        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if (string.IsNullOrEmpty(sourceFileName) || string.IsNullOrEmpty(destFileName))
                throw new ArgumentException("The source and destination paths cannot be empty.");
            if (!Exists(sourceFileName))
                throw new IOException("The source file does not exist.");
            if (!overwrite && Exists(destFileName))
                throw new IOException("The destination file already exists.");
            WriteAllBytes(destFileName, ReadAllBytes(sourceFileName));
        }

        public static void Move(string sourceFileName, string destFileName, bool overwrite)
        {
            if (overwrite && Exists(destFileName))
                Delete(destFileName);
            Move(sourceFileName, destFileName);
        }

        public static FileAttributes GetAttributes(string path)
        {
            if (!TryGetInfo(path, out FileMetadata metadata))
                throw new IOException("The file metadata could not be read.");
            return ToFileAttributes(metadata.Attribute);
        }

        public static void SetAttributes(string path, FileAttributes attributes)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("The file path cannot be null or empty.");
            if (!OpenVolume(out EFI_FILE_HANDLE* volume))
                throw new IOException("The file system volume could not be opened.");

            EFI_FILE_HANDLE* file = null;
            try
            {
                fixed (char* pathPointer = path)
                {
                    if ((ulong)volume->Open(volume, &file, pathPointer,
                        EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE, 0) != EFI_SUCCESS || file == null)
                        throw new IOException("The file could not be opened.");
                }

                ulong infoSize = 0;
                if ((ulong)file->GetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID, &infoSize, null) != EFI_BUFFER_TOO_SMALL)
                    throw new IOException("The file metadata size could not be queried.");
                byte[] buffer = new byte[infoSize];
                fixed (byte* infoBuffer = buffer)
                {
                    if ((ulong)file->GetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID, &infoSize, infoBuffer) != EFI_SUCCESS)
                        throw new IOException("The file metadata could not be read.");
                    ((EFI_FILE_INFO*)infoBuffer)->Attribute = ToEfiAttributes(attributes);
                    if ((ulong)file->SetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID, infoSize, infoBuffer) != EFI_SUCCESS)
                        throw new IOException("The file attributes could not be updated.");
                }
            }
            finally
            {
                if (file != null)
                    file->Close(file);
                volume->Close(volume);
            }
        }

        public static void Move(string sourceFileName, string destFileName)
        {
            if (Exists(destFileName))
                throw new IOException("The destination file already exists.");
            if (!TryMove(sourceFileName, destFileName))
                throw new IOException("The file could not be moved.");
        }

        private static bool OpenVolume(out EFI_FILE_HANDLE* volume)
        {
            volume = null;
            EFI_LOADED_IMAGE_PROTOCOL* loadedImage = null;
            EFI_STATUS status = gBS->HandleProtocol(gImageHandle, (EFI_GUID*)EFI_LOADED_IMAGE_PROTOCOL_GUID, (void**)&loadedImage);
            if ((ulong)status != EFI_SUCCESS || loadedImage == null)
                return false;

            EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* fileSystem = null;
            status = gBS->HandleProtocol(loadedImage->DeviceHandle, (EFI_GUID*)EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&fileSystem);
            if ((ulong)status != EFI_SUCCESS || fileSystem == null)
                return false;

            EFI_FILE_HANDLE* openedVolume = null;
            status = fileSystem->OpenVolume(fileSystem, &openedVolume);
            volume = openedVolume;
            if ((ulong)status != EFI_SUCCESS || volume == null)
            {
                volume = null;
                return false;
            }
            return true;
        }

        internal static bool TryGetInfo(string path, out FileMetadata metadata)
        {
            metadata = default;
            if (!OpenVolume(out EFI_FILE_HANDLE* volume))
                return false;

            EFI_FILE_HANDLE* file = null;
            try
            {
                fixed (char* pathPointer = path)
                {
                    if ((ulong)volume->Open(volume, &file, pathPointer, EFI_FILE_MODE_READ, 0) != EFI_SUCCESS || file == null)
                        return false;
                }

                ulong infoSize = 0;
                if ((ulong)file->GetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID, &infoSize, null) != EFI_BUFFER_TOO_SMALL || infoSize == 0)
                    return false;

                byte[] buffer = new byte[infoSize];
                fixed (byte* infoBuffer = buffer)
                {
                    if ((ulong)file->GetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID, &infoSize, infoBuffer) != EFI_SUCCESS)
                        return false;
                    EFI_FILE_INFO* info = (EFI_FILE_INFO*)infoBuffer;
                    metadata.FileSize = info->FileSize;
                    metadata.Attribute = info->Attribute;
                    metadata.CreateTime = info->CreateTime;
                    metadata.LastAccessTime = info->LastAccessTime;
                    metadata.ModificationTime = info->ModificationTime;
                }
                return true;
            }
            finally
            {
                if (file != null)
                    file->Close(file);
                volume->Close(volume);
            }
        }

        internal static string[] ReadDirectory(string path, bool directoriesOnly)
        {
            if (!OpenVolume(out EFI_FILE_HANDLE* volume))
                return new string[0];

            EFI_FILE_HANDLE* directory = null;
            try
            {
                fixed (char* pathPointer = path)
                {
                    if ((ulong)volume->Open(volume, &directory, pathPointer, EFI_FILE_MODE_READ, 0) != EFI_SUCCESS || directory == null)
                        return new string[0];
                }

                List<string> result = new List<string>();
                byte[] buffer = new byte[4096];
                while (true)
                {
                    ulong size = (ulong)buffer.Length;
                    EFI_STATUS status;
                    fixed (byte* data = buffer)
                        status = directory->Read(directory, &size, data);

                    if ((ulong)status != EFI_SUCCESS || size == 0)
                        break;

                    fixed (byte* data = buffer)
                    {
                        byte* current = data;
                        ulong remaining = size;
                        while (remaining >= (ulong)sizeof(ulong))
                        {
                            EFI_FILE_INFO* info = (EFI_FILE_INFO*)current;
                            ulong recordSize = info->Size;
                            char* name = info->FileName;
                            ulong nameOffset = (ulong)((byte*)name - current);
                            if (recordSize < nameOffset + (ulong)sizeof(char) || recordSize > remaining)
                                break;

                            bool isDirectory = (info->Attribute & EFI_FILE_DIRECTORY) != 0;
                            if (directoriesOnly ? isDirectory : !isDirectory)
                            {
                                ulong nameCapacity = (recordSize - nameOffset) / (ulong)sizeof(char);
                                int nameLength = 0;
                                while ((ulong)nameLength < nameCapacity && name[nameLength] != '\0')
                                    nameLength++;
                                if (nameLength > 0 && name[0] != '.' && !(nameLength > 1 && name[1] == '\0'))
                                {
                                    char[] nameBuffer = new char[nameLength];
                                    for (int i = 0; i < nameLength; i++)
                                        nameBuffer[i] = name[i];
                                    result.Add(new string(nameBuffer));
                                }
                            }

                            current += recordSize;
                            remaining -= recordSize;
                        }
                    }
                }

                return result.ToArray();
            }
            finally
            {
                if (directory != null)
                    directory->Close(directory);
                volume->Close(volume);
            }
        }

        internal static bool TryCreateDirectory(string path)
        {
            if (!OpenVolume(out EFI_FILE_HANDLE* volume))
                return false;
            EFI_FILE_HANDLE* directory = null;
            try
            {
                fixed (char* pathPointer = path)
                {
                    EFI_STATUS status = volume->Open(volume, &directory, pathPointer,
                        EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE | EFI_FILE_MODE_CREATE, EFI_FILE_DIRECTORY);
                    return (ulong)status == EFI_SUCCESS && directory != null;
                }
            }
            finally
            {
                if (directory != null)
                    directory->Close(directory);
                volume->Close(volume);
            }
        }

        internal static bool TryDelete(string path)
        {
            if (!OpenVolume(out EFI_FILE_HANDLE* volume))
                return false;
            EFI_FILE_HANDLE* file = null;
            try
            {
                fixed (char* pathPointer = path)
                {
                    if ((ulong)volume->Open(volume, &file, pathPointer, EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE, 0) != EFI_SUCCESS || file == null)
                        return false;
                }

                EFI_STATUS status = file->Delete(file);
                file = null;
                return (ulong)status == EFI_SUCCESS;
            }
            finally
            {
                if (file != null)
                    file->Close(file);
                volume->Close(volume);
            }
        }

        internal static bool TryMove(string sourceFileName, string destFileName)
        {
            if (string.IsNullOrEmpty(sourceFileName) || string.IsNullOrEmpty(destFileName))
                return false;

            if (Path.GetDirectoryName(sourceFileName) != Path.GetDirectoryName(destFileName))
                return false;

            string destinationName = Path.GetFileName(destFileName);
            if (string.IsNullOrEmpty(destinationName) || !OpenVolume(out EFI_FILE_HANDLE* volume))
                return false;

            EFI_FILE_HANDLE* file = null;
            try
            {
                fixed (char* sourcePath = sourceFileName)
                {
                    EFI_STATUS status = volume->Open(volume, &file, sourcePath,
                        EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE, 0);
                    if ((ulong)status != EFI_SUCCESS || file == null)
                        return false;
                }

                ulong currentInfoSize = 0;
                if ((ulong)file->GetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID,
                    &currentInfoSize, null) != EFI_BUFFER_TOO_SMALL || currentInfoSize == 0)
                    return false;

                ulong destinationNameSize = ((ulong)destinationName.Length + 1UL) * sizeof(char);
                byte[] buffer = new byte[currentInfoSize + destinationNameSize];
                fixed (byte* infoBuffer = buffer)
                {
                    ulong infoSize = (ulong)buffer.Length;
                    if ((ulong)file->GetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID,
                        &infoSize, infoBuffer) != EFI_SUCCESS)
                        return false;

                    EFI_FILE_INFO* info = (EFI_FILE_INFO*)infoBuffer;
                    char* fileName = info->FileName;
                    ulong fileNameOffset = (ulong)((byte*)fileName - infoBuffer);
                    ulong renameInfoSize = fileNameOffset + destinationNameSize;
                    if (renameInfoSize > (ulong)buffer.Length)
                        return false;

                    for (int i = 0; i < destinationName.Length; i++)
                        fileName[i] = destinationName[i];
                    fileName[destinationName.Length] = '\0';
                    info->Size = renameInfoSize;

                    return (ulong)file->SetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID,
                        renameInfoSize, infoBuffer) == EFI_SUCCESS;
                }
            }
            finally
            {
                if (file != null)
                    file->Close(file);
                volume->Close(volume);
            }
        }

        private static FileAttributes ToFileAttributes(ulong attributes)
        {
            FileAttributes result = FileAttributes.Normal;
            if ((attributes & EFI_FILE_READ_ONLY) != 0) result |= FileAttributes.ReadOnly;
            if ((attributes & EFI_FILE_HIDDEN) != 0) result |= FileAttributes.Hidden;
            if ((attributes & EFI_FILE_SYSTEM) != 0) result |= FileAttributes.System;
            if ((attributes & EFI_FILE_DIRECTORY) != 0) result |= FileAttributes.Directory;
            if ((attributes & EFI_FILE_ARCHIVE) != 0) result |= FileAttributes.Archive;
            return result;
        }

        private static ulong ToEfiAttributes(FileAttributes attributes)
        {
            ulong result = 0;
            if ((attributes & FileAttributes.ReadOnly) != 0) result |= EFI_FILE_READ_ONLY;
            if ((attributes & FileAttributes.Hidden) != 0) result |= EFI_FILE_HIDDEN;
            if ((attributes & FileAttributes.System) != 0) result |= EFI_FILE_SYSTEM;
            if ((attributes & FileAttributes.Directory) != 0) result |= EFI_FILE_DIRECTORY;
            if ((attributes & FileAttributes.Archive) != 0) result |= EFI_FILE_ARCHIVE;
            return result;
        }
    }
}
