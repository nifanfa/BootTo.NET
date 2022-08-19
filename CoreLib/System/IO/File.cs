namespace System.IO
{
    public static unsafe class File
    {
        public static byte[] ReadAllBytes(string path)
        {
            EFI_LOADED_IMAGE_PROTOCOL* loadedimage = null;
            EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* simplefilesystem = null;
            gBS->HandleProtocol(gImageHandle, EFI_LOADED_IMAGE_PROTOCOL_GUID, (void**)&loadedimage);
            gBS->HandleProtocol(loadedimage->DeviceHandle, EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&simplefilesystem);
            EFI_FILE_HANDLE* vol = null;
            simplefilesystem->OpenVolume(simplefilesystem, &vol);
            EFI_FILE_HANDLE* file = null;
            fixed (char* ptr = path)
                vol->Open(vol, &file, ptr, EFI_FILE_MODE_READ, 0);
            EFI_FILE_INFO info = new EFI_FILE_INFO();
            ulong fileinfosize = (ulong)sizeof(EFI_FILE_INFO);
            file->GetInfo(file, EFI_FILE_INFO_ID, &fileinfosize, &info);
            byte[] buffer = new byte[info.FileSize];
            fixed (byte* pbuf = buffer)
                file->Read(file, &info.FileSize, pbuf);
            file->Close(file);
            vol->Close(vol);
            return buffer;
        }

        public static void WriteAllBytes(string path, byte[] buffer)
        {
            File.Delete(path);
            EFI_LOADED_IMAGE_PROTOCOL* loadedimage = null;
            EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* simplefilesystem = null;
            gBS->HandleProtocol(gImageHandle, EFI_LOADED_IMAGE_PROTOCOL_GUID, (void**)&loadedimage);
            gBS->HandleProtocol(loadedimage->DeviceHandle, EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&simplefilesystem);
            EFI_FILE_HANDLE* vol = null;
            simplefilesystem->OpenVolume(simplefilesystem, &vol);
            EFI_FILE_HANDLE* file = null;
            fixed (char* ptr = path)
                vol->Open(vol, &file, ptr, EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE | EFI_FILE_MODE_CREATE, 0);
            ulong size = (ulong)buffer.Length;
            fixed (byte* pbuf = buffer)
                file->Write(file, &size, pbuf);
            file->Flush(file);
            file->Close(file);
            vol->Flush(vol);
            vol->Close(vol);
        }

        public static void Delete(string path)
        {
            EFI_LOADED_IMAGE_PROTOCOL* loadedimage = null;
            EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* simplefilesystem = null;
            gBS->HandleProtocol(gImageHandle, EFI_LOADED_IMAGE_DEVICE_PATH_PROTOCOL_GUID, (void**)&loadedimage);
            gBS->HandleProtocol(loadedimage->DeviceHandle, EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&simplefilesystem);
            EFI_FILE_HANDLE* vol = null;
            simplefilesystem->OpenVolume(simplefilesystem, &vol);
            EFI_FILE_HANDLE* file = null;
            fixed (char* ptr = path)
                vol->Open(vol, &file, ptr, EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE | EFI_FILE_MODE_CREATE, 0);
            file->Delete(file);
            vol->Flush(vol);
            vol->Close(vol);
        }
    }
}
