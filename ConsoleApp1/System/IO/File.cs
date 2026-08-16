namespace System.IO
{
    public static unsafe class File
    {
        public static byte[] ReadAllBytes(string path)
        {
            EFI_LOADED_IMAGE_PROTOCOL* loadedimage = null;
            EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* simplefilesystem = null;
            gBS->HandleProtocol(gImageHandle, (EFI_GUID*)EFI_LOADED_IMAGE_PROTOCOL_GUID, (void**)&loadedimage);
            gBS->HandleProtocol(loadedimage->DeviceHandle, (EFI_GUID*)EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&simplefilesystem);
            EFI_FILE_HANDLE* vol = null;
            simplefilesystem->OpenVolume(simplefilesystem, &vol);
            EFI_FILE_HANDLE* file = null;
            fixed (char* ptr = path)
                vol->Open(vol, &file, ptr, EFI_FILE_MODE_READ, 0);
            ulong fileinfosize = 0;
            EFI_STATUS status = file->GetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID, &fileinfosize, null);
            if ((ulong)status != EFI_BUFFER_TOO_SMALL)
            {
                file->Close(file);
                vol->Close(vol);
                return new byte[0];
            }

            byte[] fileinfobuffer = new byte[fileinfosize];
            ulong filesize;
            fixed (byte* pfileinfo = fileinfobuffer)
            {
                status = file->GetInfo(file, (EFI_GUID*)EFI_FILE_INFO_ID, &fileinfosize, pfileinfo);
                if ((ulong)status != EFI_SUCCESS)
                {
                    file->Close(file);
                    vol->Close(vol);
                    return new byte[0];
                }

                filesize = ((EFI_FILE_INFO*)pfileinfo)->FileSize;
            }

            byte[] buffer = new byte[filesize];
            fixed (byte* pbuf = buffer)
                status = file->Read(file, &filesize, pbuf);
            file->Close(file);
            vol->Close(vol);
            return (ulong)status == EFI_SUCCESS ? buffer : new byte[0];
        }

        public static bool Exists(string path)
        {
            EFI_LOADED_IMAGE_PROTOCOL* loadedimage = null;
            EFI_STATUS status = gBS->HandleProtocol(gImageHandle, (EFI_GUID*)EFI_LOADED_IMAGE_PROTOCOL_GUID, (void**)&loadedimage);
            if ((ulong)status != EFI_SUCCESS || loadedimage == null)
                return false;

            EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* simplefilesystem = null;
            status = gBS->HandleProtocol(loadedimage->DeviceHandle, (EFI_GUID*)EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&simplefilesystem);
            if ((ulong)status != EFI_SUCCESS || simplefilesystem == null)
                return false;

            EFI_FILE_HANDLE* vol = null;
            status = simplefilesystem->OpenVolume(simplefilesystem, &vol);
            if ((ulong)status != EFI_SUCCESS || vol == null)
                return false;

            EFI_FILE_HANDLE* file = null;
            fixed (char* ptr = path)
                status = vol->Open(vol, &file, ptr, EFI_FILE_MODE_READ, 0);

            if (file != null)
                file->Close(file);
            vol->Close(vol);
            return (ulong)status == EFI_SUCCESS && file != null;
        }

        public static void WriteAllBytes(string path, byte[] buffer)
        {
            File.Delete(path);
            EFI_LOADED_IMAGE_PROTOCOL* loadedimage = null;
            EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* simplefilesystem = null;
            gBS->HandleProtocol(gImageHandle, (EFI_GUID*)EFI_LOADED_IMAGE_PROTOCOL_GUID, (void**)&loadedimage);
            gBS->HandleProtocol(loadedimage->DeviceHandle, (EFI_GUID*)EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&simplefilesystem);
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
            gBS->HandleProtocol(gImageHandle, (EFI_GUID*)EFI_LOADED_IMAGE_DEVICE_PATH_PROTOCOL_GUID, (void**)&loadedimage);
            gBS->HandleProtocol(loadedimage->DeviceHandle, (EFI_GUID*)EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&simplefilesystem);
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
