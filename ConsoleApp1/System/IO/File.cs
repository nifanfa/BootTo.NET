namespace System.IO
{
    public static unsafe class File
    {
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
            {
                status = vol->Open(vol, &file, ptr, EFI_FILE_MODE_READ, 0);
            }

            bool exists = ((ulong)status == EFI_SUCCESS && file != null);

            if (file != null)
            {
                file->Close(file);
            }

            vol->Close(vol);

            return exists;
        }

        public static void WriteAllBytes(string path, byte[] buffer)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Write(buffer);
            fs.Close();
        }

        public static void Delete(string path)
        {
            EFI_LOADED_IMAGE_PROTOCOL* loadedimage = null;
            EFI_STATUS status = gBS->HandleProtocol(gImageHandle, (EFI_GUID*)EFI_LOADED_IMAGE_DEVICE_PATH_PROTOCOL_GUID, (void**)&loadedimage);
            if ((ulong)status != EFI_SUCCESS || loadedimage == null)
                return;

            EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* simplefilesystem = null;
            status = gBS->HandleProtocol(loadedimage->DeviceHandle, (EFI_GUID*)EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&simplefilesystem);
            if ((ulong)status != EFI_SUCCESS || simplefilesystem == null)
                return;

            EFI_FILE_HANDLE* vol = null;
            status = simplefilesystem->OpenVolume(simplefilesystem, &vol);
            if ((ulong)status != EFI_SUCCESS || vol == null)
                return;

            EFI_FILE_HANDLE* file = null;
            fixed (char* ptr = path)
            {
                status = vol->Open(vol, &file, ptr, EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE | EFI_FILE_MODE_CREATE, 0);
            }

            if ((ulong)status == EFI_SUCCESS && file != null)
            {
                file->Delete(file);
            }

            if (vol != null)
            {
                vol->Close(vol);
            }
        }
    }
}
