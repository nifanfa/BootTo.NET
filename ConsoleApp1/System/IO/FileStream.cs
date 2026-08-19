using System.Threading.Tasks;

namespace System.IO
{
    public unsafe class FileStream : Stream
    {
        private sealed class FileStreamPoller : TaskPoller
        {
            private readonly FileStream _stream;

            internal FileStreamPoller(FileStream stream) => _stream = stream;

            internal override void Poll() => _stream.Poll();
        }

        public override int Length => (int)_fileSize;
        public override bool CanRead => File != null;
        public override bool CanSeek => File != null;
        public override bool CanWrite => File != null;

        EFI_FILE_HANDLE* Volume = null;
        EFI_FILE_HANDLE* File = null;

        private readonly FileStreamPoller _poller;
        private EFI_FILE_IO_TOKEN _readToken;
        private EFI_FILE_IO_TOKEN _writeToken;
        private EFI_FILE_IO_TOKEN _flushToken;
        private TaskCompletionSource<int> _readCompletion;
        private TaskCompletionSource<int> _writeCompletion;
        private TaskCompletionSource _flushCompletion;
        private byte[] _readBuffer;
        private byte[] _writeBuffer;
        private ulong _fileSize;
        private bool _asyncSupported;

        public FileStream(string path, FileMode mode)
        {
            _poller = new FileStreamPoller(this);

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
                File = null;
                Volume = null;
                throw new IOException();
            }

            byte[] fileinfobuffer = new byte[fileinfosize];
            fixed (byte* pfileinfo = fileinfobuffer)
            {
                if (File->GetInfo(File, (EFI_GUID*)EFI_FILE_INFO_ID, &fileinfosize, pfileinfo) != EFI_SUCCESS)
                {
                    File->Close(File);
                    Volume->Close(Volume);
                    File = null;
                    Volume = null;
                    throw new IOException();
                }

                _fileSize = ((EFI_FILE_INFO*)pfileinfo)->FileSize;
            }

            InitializeAsyncIO();
        }

        public override int Read(byte[] buffer)
            => ReadAsync(buffer).GetAwaiter().GetResult();

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateRange(buffer, offset, count);
            if (offset == 0 && count == buffer.Length)
                return Read(buffer);

            byte[] data = new byte[count];
            int bytesRead = Read(data);
            for (int i = 0; i < bytesRead; i++)
                buffer[offset + i] = data[i];

            return bytesRead;
        }

        public override Task<int> ReadAsync(byte[] buffer)
        {
            if (buffer == null)
                return Task.FromException<int>(new ArgumentNullException());
            if (File == null)
                return Task.FromException<int>(new IOException());
            if (_readCompletion != null)
                return Task.FromException<int>(new IOException());
            if (!_asyncSupported)
                return ReadSynchronously(buffer);
            if (buffer.Length == 0)
                return Task.FromResult(0);

            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
            _readCompletion = completion;
            _readBuffer = buffer;

            _readToken.Status = EFI_NOT_READY;
            _readToken.BufferSize = (ulong)buffer.Length;
            fixed (byte* data = buffer)
                _readToken.Buffer = data;

            EFI_STATUS status;
            fixed (EFI_FILE_IO_TOKEN* token = &_readToken)
                status = File->ReadEx(File, token);

            if ((ulong)status == EFI_SUCCESS)
                UpdatePollingRegistration();
            else
                CompleteRead(status);

            return completion.Task;
        }

        public override int Write(byte[] buffer)
            => WriteAsync(buffer).GetAwaiter().GetResult();

        public override int Write(byte[] buffer, int offset, int count)
        {
            ValidateRange(buffer, offset, count);
            if (offset == 0 && count == buffer.Length)
                return Write(buffer);

            byte[] data = new byte[count];
            for (int i = 0; i < count; i++)
                data[i] = buffer[offset + i];

            return Write(data);
        }

        public override Task<int> WriteAsync(byte[] buffer)
        {
            if (buffer == null)
                return Task.FromException<int>(new ArgumentNullException());
            if (File == null)
                return Task.FromException<int>(new IOException());
            if (_writeCompletion != null)
                return Task.FromException<int>(new IOException());
            if (!_asyncSupported)
                return WriteSynchronously(buffer);
            if (buffer.Length == 0)
                return Task.FromResult(0);

            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
            _writeCompletion = completion;
            _writeBuffer = buffer;

            _writeToken.Status = EFI_NOT_READY;
            _writeToken.BufferSize = (ulong)buffer.Length;
            fixed (byte* data = buffer)
                _writeToken.Buffer = data;

            EFI_STATUS status;
            fixed (EFI_FILE_IO_TOKEN* token = &_writeToken)
                status = File->WriteEx(File, token);

            if ((ulong)status == EFI_SUCCESS)
                UpdatePollingRegistration();
            else
                CompleteWrite(status);

            return completion.Task;
        }

        public override void Flush()
            => FlushAsync().GetAwaiter().GetResult();

        public override Task FlushAsync()
        {
            if (File == null)
                return Task.FromException(new IOException());
            if (_flushCompletion != null)
                return Task.FromException(new IOException());
            if (!_asyncSupported)
                return FlushSynchronously();

            TaskCompletionSource completion = new TaskCompletionSource();
            _flushCompletion = completion;
            _flushToken.Status = EFI_NOT_READY;

            EFI_STATUS status;
            fixed (EFI_FILE_IO_TOKEN* token = &_flushToken)
                status = File->FlushEx(File, token);

            if ((ulong)status == EFI_SUCCESS)
                UpdatePollingRegistration();
            else
                CompleteFlush(status);

            return completion.Task;
        }

        public override void Close()
        {
            Exception failure = null;
            try
            {
                if (_readCompletion != null)
                {
                    try { _readCompletion.Task.GetAwaiter().GetResult(); }
                    catch (Exception exception) { failure = exception; }
                }
                if (_writeCompletion != null)
                {
                    try { _writeCompletion.Task.GetAwaiter().GetResult(); }
                    catch (Exception exception) { if (failure == null) failure = exception; }
                }
                if (_flushCompletion != null)
                {
                    try { _flushCompletion.Task.GetAwaiter().GetResult(); }
                    catch (Exception exception) { if (failure == null) failure = exception; }
                }
            }
            finally
            {
                TaskScheduler.Unregister(_poller);
                CloseEvent(ref _readToken.Event);
                CloseEvent(ref _writeToken.Event);
                CloseEvent(ref _flushToken.Event);

                if (File != null)
                {
                    File->Close(File);
                    File = null;
                }

                if (Volume != null)
                {
                    Volume->Close(Volume);
                    Volume = null;
                }

                _asyncSupported = false;
            }

            if (failure != null)
                throw failure;
        }

        private void InitializeAsyncIO()
        {
            _asyncSupported = File->Revision >= EFI_FILE_PROTOCOL_REVISION2
                && File->ReadEx != null
                && File->WriteEx != null
                && File->FlushEx != null;

            if (!_asyncSupported)
                return;

            EFI_STATUS status;
            fixed (EFI_FILE_IO_TOKEN* token = &_readToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->Event);
            if ((ulong)status != EFI_SUCCESS)
            {
                DisableAsyncIO();
                return;
            }

            fixed (EFI_FILE_IO_TOKEN* token = &_writeToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->Event);
            if ((ulong)status != EFI_SUCCESS)
            {
                DisableAsyncIO();
                return;
            }

            fixed (EFI_FILE_IO_TOKEN* token = &_flushToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->Event);
            if ((ulong)status != EFI_SUCCESS)
                DisableAsyncIO();
        }

        private Task<int> ReadSynchronously(byte[] buffer)
        {
            fixed (byte* data = buffer)
            {
                ulong size = (ulong)buffer.Length;
                EFI_STATUS status = File->Read(File, &size, data);
                if ((ulong)status != EFI_SUCCESS)
                    return Task.FromException<int>(new IOException());
                return Task.FromResult((int)size);
            }
        }

        private Task<int> WriteSynchronously(byte[] buffer)
        {
            fixed (byte* data = buffer)
            {
                ulong size = (ulong)buffer.Length;
                EFI_STATUS status = File->Write(File, &size, data);
                if ((ulong)status != EFI_SUCCESS)
                    return Task.FromException<int>(new IOException());

                UpdateFileSize();
                return Task.FromResult((int)size);
            }
        }

        private Task FlushSynchronously()
        {
            EFI_STATUS status = File->Flush(File);
            return (ulong)status == EFI_SUCCESS
                ? Task.CompletedTask
                : Task.FromException(new IOException());
        }

        private void Poll()
        {
            if (_readCompletion != null && IsSignaled(_readToken.Event))
                CompleteRead(_readToken.Status);

            if (_writeCompletion != null && IsSignaled(_writeToken.Event))
                CompleteWrite(_writeToken.Status);

            if (_flushCompletion != null && IsSignaled(_flushToken.Event))
                CompleteFlush(_flushToken.Status);
        }

        private void CompleteRead(EFI_STATUS status)
        {
            TaskCompletionSource<int> completion = _readCompletion;
            int bytesRead = (int)_readToken.BufferSize;
            _readCompletion = null;
            _readBuffer = null;
            _readToken.Buffer = null;
            UpdatePollingRegistration();

            if (completion == null)
                return;
            if ((ulong)status == EFI_SUCCESS)
                completion.TrySetResult(bytesRead);
            else
                completion.TrySetException(new IOException());
        }

        private void CompleteWrite(EFI_STATUS status)
        {
            TaskCompletionSource<int> completion = _writeCompletion;
            int bytesWritten = (int)_writeToken.BufferSize;
            _writeCompletion = null;
            _writeBuffer = null;
            _writeToken.Buffer = null;
            UpdatePollingRegistration();

            if (completion == null)
                return;
            if ((ulong)status == EFI_SUCCESS)
            {
                UpdateFileSize();
                completion.TrySetResult(bytesWritten);
            }
            else
            {
                completion.TrySetException(new IOException());
            }
        }

        private void CompleteFlush(EFI_STATUS status)
        {
            TaskCompletionSource completion = _flushCompletion;
            _flushCompletion = null;
            UpdatePollingRegistration();

            if (completion == null)
                return;
            if ((ulong)status == EFI_SUCCESS)
                completion.TrySetResult();
            else
                completion.TrySetException(new IOException());
        }

        private void UpdateFileSize()
        {
            ulong position = 0;
            if ((ulong)File->GetPosition(File, &position) == EFI_SUCCESS && position > _fileSize)
                _fileSize = position;
        }

        private bool IsSignaled(EFI_EVENT e)
            => (void*)e != null && (ulong)gBS->CheckEvent(e) == EFI_SUCCESS;

        private void UpdatePollingRegistration()
        {
            if (_readCompletion != null || _writeCompletion != null || _flushCompletion != null)
                TaskScheduler.Register(_poller);
            else
                TaskScheduler.Unregister(_poller);
        }

        private void DisableAsyncIO()
        {
            CloseEvent(ref _readToken.Event);
            CloseEvent(ref _writeToken.Event);
            CloseEvent(ref _flushToken.Event);
            _asyncSupported = false;
        }

        private static void CloseEvent(ref EFI_EVENT e)
        {
            if ((void*)e == null)
                return;

            gBS->CloseEvent(e);
            e = default;
        }

        private static void ValidateRange(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            if (offset < 0 || count < 0 || offset > buffer.Length - count)
                throw new ArgumentException();
        }
    }
}
