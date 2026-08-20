using System.Collections.Generic;


internal unsafe struct UefiFileMetadata
{
    internal ulong FileSize;
    internal ulong Attribute;
    internal EFI_TIME CreateTime;
    internal EFI_TIME LastAccessTime;
    internal EFI_TIME ModificationTime;
}

internal static unsafe class UefiFileSystem
{
    internal static bool OpenVolume(out EFI_FILE_HANDLE* volume)
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

    internal static bool TryGetInfo(string path, out UefiFileMetadata metadata)
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

    internal static bool FileExists(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (!TryGetInfo(path, out UefiFileMetadata metadata))
            return false;

        return (metadata.Attribute & EFI_FILE_DIRECTORY) == 0;
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
                    EFI_FILE_INFO* info = (EFI_FILE_INFO*)data;
                    bool isDirectory = (info->Attribute & EFI_FILE_DIRECTORY) != 0;
                    if (directoriesOnly ? isDirectory : !isDirectory)
                    {
                        char* name = (char*)((byte*)info + sizeof(EFI_FILE_INFO) - sizeof(char));
                        int nameLength = 0;
                        while (nameLength < 2048 && name[nameLength] != '\0')
                            nameLength++;
                        if (nameLength > 0 && name[0] != '.' && !(name[0] == '.' && name[1] == '\0'))
                        {
                            char[] nameBuffer = new char[nameLength];
                            for (int i = 0; i < nameLength; i++)
                                nameBuffer[i] = name[i];
                            result.Add(new string(nameBuffer));
                        }
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

    internal static bool CreateDirectory(string path)
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

    internal static bool Delete(string path)
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
}
