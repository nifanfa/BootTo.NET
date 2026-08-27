using System.Collections.Generic;
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

        public static void Move(string sourceFileName, string destFileName)
        {
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
    }
}
