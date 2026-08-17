namespace System.IO
{
    public unsafe class FileStream : Stream
    {
        public override int Length => (int)FileInfo->FileSize;

        EFI_FILE_HANDLE* Volume = null;
        EFI_FILE_HANDLE* File = null;
        EFI_FILE_INFO* FileInfo = null;

        public FileStream(string path, FileMode mode)
        {
            EFI_LOADED_IMAGE_PROTOCOL* loadedimage = null;
            if (gBS->HandleProtocol(gImageHandle, (EFI_GUID*)EFI_LOADED_IMAGE_PROTOCOL_GUID, (void**)&loadedimage) != EFI_SUCCESS || loadedimage == null)
                throw new IOException();

            EFI_SIMPLE_FILE_SYSTEM_PROTOCOL* simplefilesystem = null;
            if (gBS->HandleProtocol(loadedimage->DeviceHandle, (EFI_GUID*)EFI_SIMPLE_FILE_SYSTEM_PROTOCOL_GUID, (void**)&simplefilesystem) != EFI_SUCCESS || simplefilesystem == null)
                throw new IOException();

            {
                EFI_FILE_HANDLE* vol = null;
                if (simplefilesystem->OpenVolume(simplefilesystem, &vol) != EFI_SUCCESS || vol == null)
                    throw new IOException();
                Volume = vol;
            }
            {
                EFI_FILE_HANDLE* file = null;
                fixed (char* ptr = path)
                    if (Volume->Open(Volume, &file, ptr, mode switch
                    {
                        FileMode.CreateNew => EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE | EFI_FILE_MODE_CREATE,
                        FileMode.Create => EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE | EFI_FILE_MODE_CREATE,
                        FileMode.Open => EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE,
                        FileMode.OpenOrCreate => EFI_FILE_MODE_READ | EFI_FILE_MODE_WRITE | EFI_FILE_MODE_CREATE,
                        _ => throw new NotSupportedException()
                    }, 0) != EFI_SUCCESS || file == null)
                        throw new IOException();
                File = file;
            }
            ulong fileinfosize = 0;
            if (File->GetInfo(File, (EFI_GUID*)EFI_FILE_INFO_ID, &fileinfosize, null) != EFI_BUFFER_TOO_SMALL)
            {
                File->Close(File);
                Volume->Close(Volume);
                throw new IOException();
            }

            byte[] fileinfobuffer = new byte[fileinfosize];
            fixed (byte* pfileinfo = fileinfobuffer)
            {
                if (File->GetInfo(File, (EFI_GUID*)EFI_FILE_INFO_ID, &fileinfosize, pfileinfo) != EFI_SUCCESS)
                {
                    File->Close(File);
                    Volume->Close(Volume);
                    throw new IOException();
                }

                FileInfo = (EFI_FILE_INFO*)pfileinfo;
            }
        }

        public override int Read(byte[] buffer)
        {
            fixed (byte* pbuf = buffer)
            {
                ulong size = (ulong)buffer.Length;
                if (File->Read(File, &size, pbuf) != EFI_SUCCESS)
                {
                    throw new IOException();
                }
                return (int)size;
            }
        }

        public override int Write(byte[] buffer)
        {
            fixed (byte* pbuf = buffer)
            {
                ulong size = (ulong)buffer.Length;
                if (File->Write(File, &size, pbuf) != EFI_SUCCESS)
                {
                    throw new IOException();
                }
                return (int)size;
            }
        }

        public override void Flush()
        {
            if (File->Flush(File) != EFI_SUCCESS)
            {
                throw new IOException();
            }
        }

        public override void Close()
        {
            File->Close(File);
            Volume->Close(Volume);
        }
    }
}
