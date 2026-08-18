using System.Threading.Tasks;

namespace System.IO.Ports
{
    public enum Parity
    {
        None,
        Odd,
        Even,
        Mark,
        Space,
    }

    public enum StopBits
    {
        None,
        One,
        OnePointFive,
        Two,
    }

    public unsafe sealed class SerialPort : IDisposable
    {
        private sealed class ReadPoller : TaskPoller
        {
            private readonly SerialPort _port;

            internal ReadPoller(SerialPort port) => _port = port;

            internal override void Poll() => _port.PollRead();
        }

        private EFI_SERIAL_IO_PROTOCOL* _serial;
        private ReadPoller _readPoller;
        private TaskCompletionSource<int> _readCompletion;
        private byte[] _readBuffer;

        public SerialPort(string portName)
            : this(portName, 115200, Parity.None, 8, StopBits.One)
        {
        }

        public SerialPort(
            string portName,
            int baudRate,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One)
        {
            if (portName == null)
                throw new ArgumentNullException();
            if (baudRate <= 0 || dataBits < 5 || dataBits > 8)
                throw new ArgumentException();

            PortName = portName;
            BaudRate = baudRate;
            Parity = parity;
            DataBits = dataBits;
            StopBits = stopBits;
        }

        public string PortName { get; }
        public int BaudRate { get; private set; }
        public Parity Parity { get; private set; }
        public int DataBits { get; private set; }
        public StopBits StopBits { get; private set; }
        public bool IsOpen => _serial != null;

        public void Open()
        {
            if (IsOpen)
                return;

            EFI_SERIAL_IO_PROTOCOL* serial = null;
            EFI_STATUS status = gBS->LocateProtocol(
                (EFI_GUID*)EFI_SERIAL_IO_PROTOCOL_GUID,
                null,
                (void**)&serial);
            if ((ulong)status != EFI_SUCCESS || serial == null)
                throw new IOException();

            _serial = serial;
            _readPoller = new ReadPoller(this);

            status = serial->SetAttributes(
                serial,
                (ulong)BaudRate,
                0,
                0,
                ToEfiParity(Parity),
                (byte)DataBits,
                ToEfiStopBits(StopBits));
            if ((ulong)status != EFI_SUCCESS)
            {
                _serial = null;
                _readPoller = null;
                throw new IOException();
            }
        }

        public void Close()
        {
            if (!IsOpen)
                return;

            TaskCompletionSource<int> completion = _readCompletion;
            _readCompletion = null;
            _readBuffer = null;
            TaskScheduler.Unregister(_readPoller);
            _readPoller = null;
            _serial = null;

            if (completion != null)
                completion.TrySetException(new IOException());
        }

        public Task<int> ReadAsync(byte[] buffer)
        {
            if (buffer == null)
                return Task.FromException<int>(new ArgumentNullException());
            if (!IsOpen)
                return Task.FromException<int>(new IOException());
            if (_readCompletion != null)
                return Task.FromException<int>(new IOException());
            if (buffer.Length == 0)
                return Task.FromResult(0);

            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
            _readCompletion = completion;
            _readBuffer = buffer;
            TaskScheduler.Register(_readPoller);
            PollRead();
            return completion.Task;
        }

        public int Read(byte[] buffer)
            => ReadAsync(buffer).GetAwaiter().GetResult();

        public Task<int> WriteAsync(byte[] buffer)
        {
            if (buffer == null)
                return Task.FromException<int>(new ArgumentNullException());
            if (!IsOpen)
                return Task.FromException<int>(new IOException());
            if (buffer.Length == 0)
                return Task.FromResult(0);

            ulong size = (ulong)buffer.Length;
            EFI_STATUS status;
            fixed (byte* data = buffer)
                status = _serial->Write(_serial, &size, data);

            return (ulong)status == EFI_SUCCESS
                ? Task.FromResult((int)size)
                : Task.FromException<int>(new IOException());
        }

        public void Write(byte[] buffer)
            => WriteAsync(buffer).GetAwaiter().GetResult();

        public void Dispose() => Close();

        private void PollRead()
        {
            if (_readCompletion == null || _serial == null)
                return;

            if (_serial->GetControl != null)
            {
                uint control = 0;
                EFI_STATUS controlStatus = _serial->GetControl(_serial, &control);
                if ((ulong)controlStatus != EFI_SUCCESS)
                {
                    CompleteRead(controlStatus, 0);
                    return;
                }

                if ((control & (uint)EFI_SERIAL_INPUT_BUFFER_EMPTY) != 0)
                    return;
            }

            ulong size = (ulong)_readBuffer.Length;
            EFI_STATUS status;
            fixed (byte* data = _readBuffer)
                status = _serial->Read(_serial, &size, data);

            if ((ulong)status == EFI_SUCCESS)
                CompleteRead(status, (int)size);
            else if ((ulong)status != EFI_NOT_READY && (ulong)status != EFI_TIMEOUT)
                CompleteRead(status, 0);
        }

        private void CompleteRead(EFI_STATUS status, int count)
        {
            TaskCompletionSource<int> completion = _readCompletion;
            _readCompletion = null;
            _readBuffer = null;
            TaskScheduler.Unregister(_readPoller);

            if (completion == null)
                return;
            if ((ulong)status == EFI_SUCCESS)
                completion.TrySetResult(count);
            else
                completion.TrySetException(new IOException());
        }

        private static EFI_PARITY_TYPE ToEfiParity(Parity parity)
        {
            return parity switch
            {
                Parity.None => NoParity,
                Parity.Odd => OddParity,
                Parity.Even => EvenParity,
                Parity.Mark => MarkParity,
                Parity.Space => SpaceParity,
                _ => DefaultParity,
            };
        }

        private static EFI_STOP_BITS_TYPE ToEfiStopBits(StopBits stopBits)
        {
            return stopBits switch
            {
                StopBits.One => OneStopBit,
                StopBits.OnePointFive => OneFiveStopBits,
                StopBits.Two => TwoStopBits,
                _ => DefaultStopBits,
            };
        }
    }
}
