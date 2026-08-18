using System.Threading.Tasks;

namespace System.Net.Sockets
{
    public unsafe class Socket
    {
        private sealed class SocketPoller : TaskPoller
        {
            private readonly Socket _socket;

            internal SocketPoller(Socket socket) => _socket = socket;

            internal override void Poll() => _socket.Poll();
        }

        private EFI_TCP4* tcp;
        private EFI_SERVICE_BINDING* serviceBinding;
        private EFI_HANDLE serviceHandle;
        private EFI_HANDLE tcpHandle;

        private EFI_TCP4_RECEIVE_DATA receiveData;
        private EFI_TCP4_TRANSMIT_DATA transmitData;
        private EFI_TCP4_IO_TOKEN receiveToken;
        private EFI_TCP4_IO_TOKEN transmitToken;
        private EFI_TCP4_CONNECTION_TOKEN connectionToken;
        private EFI_TCP4_CLOSE_TOKEN closeToken;
        private EFI_TCP4_CONFIG_DATA configuration;

        private TaskCompletionSource connectCompletion;
        private TaskCompletionSource<int> receiveCompletion;
        private TaskCompletionSource<int> transmitCompletion;
        private TaskCompletionSource closeCompletion;

        private byte[] receiveBuffer;
        private byte[] transmitBuffer;
        private bool waitingForAddress;
        private bool connected;
        private readonly SocketPoller poller;

        public Socket(SocketType socketType, ProtocolType protocolType)
        {
            if (socketType != SocketType.Stream || protocolType != ProtocolType.Tcp)
                throw new SocketException(EFI_UNSUPPORTED);

            poller = new SocketPoller(this);
        }

        public void Connect(EFI_IPv4_ADDRESS address, ushort port)
            => ConnectAsync(address, port).GetAwaiter().GetResult();

        public Task ConnectAsync(EFI_IPv4_ADDRESS address, ushort port)
        {
            if (connected)
                return Task.CompletedTask;
            if (connectCompletion != null)
                return Task.FromException(new SocketException(EFI_ALREADY_STARTED));

            TaskCompletionSource completion = new TaskCompletionSource();
            connectCompletion = completion;

            EFI_STATUS status = InitializeTcp();
            if ((ulong)status != EFI_SUCCESS)
            {
                CompleteConnect(status);
                return completion.Task;
            }

            configuration = new EFI_TCP4_CONFIG_DATA();
            configuration.TimeToLive = 188;
            configuration.AccessPoint.UseDefaultAddress = true;
            configuration.AccessPoint.ActiveFlag = true;
            configuration.AccessPoint.RemotePort = port;
            configuration.AccessPoint.RemoteAddress = address;

            fixed (EFI_TCP4_CONFIG_DATA* config = &configuration)
                status = tcp->Configure(tcp, config);

            if ((ulong)status == EFI_NO_MAPPING)
            {
                waitingForAddress = true;
                UpdatePollingRegistration();
            }
            else if ((ulong)status == EFI_SUCCESS)
            {
                SubmitConnect();
            }
            else
            {
                CompleteConnect(status);
            }

            return completion.Task;
        }

        public void Send(byte[] buffer)
            => SendAsync(buffer).GetAwaiter().GetResult();

        public Task<int> SendAsync(byte[] buffer)
        {
            if (buffer == null)
                return Task.FromException<int>(new Exception("The send buffer cannot be null."));
            if (!connected)
                return Task.FromException<int>(new SocketException(EFI_NOT_STARTED));
            if (transmitCompletion != null)
                return Task.FromException<int>(new SocketException(EFI_ACCESS_DENIED));

            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
            transmitCompletion = completion;
            transmitBuffer = buffer;

            transmitData.Push = true;
            transmitData.Urgent = false;
            transmitData.DataLength = (uint)buffer.Length;
            transmitData.FragmentCount = 1;
            transmitData.FragmentTable.FragmentLength = (uint)buffer.Length;
            fixed (byte* data = buffer)
                transmitData.FragmentTable.FragmentBuffer = data;

            transmitToken.CompletionToken.Status = EFI_NOT_READY;
            EFI_STATUS status;
            fixed (EFI_TCP4_IO_TOKEN* token = &transmitToken)
                status = tcp->Transmit(tcp, token);

            if ((ulong)status == EFI_SUCCESS)
                UpdatePollingRegistration();
            else
                CompleteTransmit(status);

            return completion.Task;
        }

        public int Receive(byte[] buffer)
            => ReceiveAsync(buffer).GetAwaiter().GetResult();

        public Task<int> ReceiveAsync(byte[] buffer)
        {
            if (buffer == null)
                return Task.FromException<int>(new Exception("The receive buffer cannot be null."));
            if (!connected)
                return Task.FromException<int>(new SocketException(EFI_NOT_STARTED));
            if (receiveCompletion != null)
                return Task.FromException<int>(new SocketException(EFI_ACCESS_DENIED));

            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
            receiveCompletion = completion;
            receiveBuffer = buffer;

            receiveData.UrgentFlag = false;
            receiveData.DataLength = (uint)buffer.Length;
            receiveData.FragmentCount = 1;
            receiveData.FragmentTable.FragmentLength = (uint)buffer.Length;
            fixed (byte* data = buffer)
                receiveData.FragmentTable.FragmentBuffer = data;

            receiveToken.CompletionToken.Status = EFI_NOT_READY;
            EFI_STATUS status;
            fixed (EFI_TCP4_IO_TOKEN* token = &receiveToken)
                status = tcp->Receive(tcp, token);

            if ((ulong)status == EFI_SUCCESS)
                UpdatePollingRegistration();
            else
                CompleteReceive(status);

            return completion.Task;
        }

        public Task CloseAsync()
        {
            if (tcp == null)
                return Task.CompletedTask;
            if (closeCompletion != null)
                return closeCompletion.Task;

            TaskCompletionSource completion = new TaskCompletionSource();
            closeCompletion = completion;
            closeToken.AbortOnClose = false;
            closeToken.CompletionToken.Status = EFI_NOT_READY;

            EFI_STATUS status;
            fixed (EFI_TCP4_CLOSE_TOKEN* token = &closeToken)
                status = tcp->Close(tcp, token);

            if ((ulong)status == EFI_SUCCESS)
                UpdatePollingRegistration();
            else
                CompleteClose(status);

            return completion.Task;
        }

        public EFI_STATUS Poll()
        {
            if (tcp == null)
                return EFI_NOT_STARTED;

            EFI_STATUS status = tcp->Poll(tcp);

            if (waitingForAddress)
                PollAddressConfiguration();

            if (connectCompletion != null && !waitingForAddress && IsSignaled(connectionToken.CompletionToken.Event))
                CompleteConnect(connectionToken.CompletionToken.Status);

            if (transmitCompletion != null && IsSignaled(transmitToken.CompletionToken.Event))
                CompleteTransmit(transmitToken.CompletionToken.Status);

            if (receiveCompletion != null && IsSignaled(receiveToken.CompletionToken.Event))
                CompleteReceive(receiveToken.CompletionToken.Status);

            if (closeCompletion != null && IsSignaled(closeToken.CompletionToken.Event))
                CompleteClose(closeToken.CompletionToken.Status);

            return status;
        }

        internal void Close()
            => CloseAsync().GetAwaiter().GetResult();

        private EFI_STATUS InitializeTcp()
        {
            if (tcp != null)
                return EFI_SUCCESS;

            ulong deviceCount = 0;
            EFI_HANDLE* devices = null;
            EFI_STATUS status = gBS->LocateHandleBuffer(
                EFI_LOCATE_SEARCH_TYPE.ByProtocol,
                (EFI_GUID*)EFI_TCP4_SERVICE_BINDING_PROTOCOL,
                null,
                &deviceCount,
                &devices);

            if ((ulong)status != EFI_SUCCESS)
                return status;
            if (deviceCount == 0)
            {
                gBS->FreePool(devices);
                return EFI_NOT_FOUND;
            }

            serviceHandle = devices[0];
            EFI_SERVICE_BINDING* binding = null;
            status = gBS->OpenProtocol(
                serviceHandle,
                (EFI_GUID*)EFI_TCP4_SERVICE_BINDING_PROTOCOL,
                (void**)&binding,
                gImageHandle,
                default,
                EFI_OPEN_PROTOCOL_GET_PROTOCOL);
            gBS->FreePool(devices);

            if ((ulong)status != EFI_SUCCESS)
                return status;
            serviceBinding = binding;

            EFI_HANDLE childHandle = default;
            status = serviceBinding->CreateChild(serviceBinding, &childHandle);
            if ((ulong)status != EFI_SUCCESS)
                return status;
            tcpHandle = childHandle;

            fixed (EFI_TCP4** protocol = &tcp)
                status = gBS->OpenProtocol(
                    tcpHandle,
                    (EFI_GUID*)EFI_TCP4_PROTOCOL,
                    (void**)protocol,
                    gImageHandle,
                    default,
                    EFI_OPEN_PROTOCOL_GET_PROTOCOL);

            if ((ulong)status != EFI_SUCCESS)
            {
                serviceBinding->DestroyChild(serviceBinding, tcpHandle);
                tcpHandle = default;
                return status;
            }

            receiveData = new EFI_TCP4_RECEIVE_DATA();
            transmitData = new EFI_TCP4_TRANSMIT_DATA();
            receiveToken = new EFI_TCP4_IO_TOKEN();
            transmitToken = new EFI_TCP4_IO_TOKEN();
            connectionToken = new EFI_TCP4_CONNECTION_TOKEN();
            closeToken = new EFI_TCP4_CLOSE_TOKEN();

            receiveData.FragmentCount = 1;
            transmitData.FragmentCount = 1;
            transmitData.Push = true;

            fixed (EFI_TCP4_RECEIVE_DATA* data = &receiveData)
                receiveToken.Packet_RxData = data;
            fixed (EFI_TCP4_TRANSMIT_DATA* data = &transmitData)
                transmitToken.Packet_TxData = data;

            status = CreateCompletionEvents();
            if ((ulong)status != EFI_SUCCESS)
                ReleaseTcp();

            return status;
        }

        private EFI_STATUS CreateCompletionEvents()
        {
            EFI_STATUS status;
            fixed (EFI_TCP4_CONNECTION_TOKEN* token = &connectionToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->CompletionToken.Event);
            if ((ulong)status != EFI_SUCCESS)
                return status;

            fixed (EFI_TCP4_IO_TOKEN* token = &receiveToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->CompletionToken.Event);
            if ((ulong)status != EFI_SUCCESS)
                return status;

            fixed (EFI_TCP4_IO_TOKEN* token = &transmitToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->CompletionToken.Event);
            if ((ulong)status != EFI_SUCCESS)
                return status;

            fixed (EFI_TCP4_CLOSE_TOKEN* token = &closeToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->CompletionToken.Event);
            return status;
        }

        private void SubmitConnect()
        {
            connectionToken.CompletionToken.Status = EFI_NOT_READY;
            EFI_STATUS status;
            fixed (EFI_TCP4_CONNECTION_TOKEN* token = &connectionToken)
                status = tcp->Connect(tcp, token);

            if ((ulong)status == EFI_SUCCESS)
                UpdatePollingRegistration();
            else
                CompleteConnect(status);
        }

        private void PollAddressConfiguration()
        {
            EFI_IP4_MODE_DATA mode = new EFI_IP4_MODE_DATA();
            EFI_STATUS status = tcp->GetModeData(tcp, null, null, &mode, null, null);
            if ((ulong)status != EFI_SUCCESS && (ulong)status != EFI_NO_MAPPING)
            {
                CompleteConnect(status);
                return;
            }

            if (!mode.IsConfigured)
                return;

            fixed (EFI_TCP4_CONFIG_DATA* config = &configuration)
                status = tcp->Configure(tcp, config);

            if ((ulong)status == EFI_NO_MAPPING)
                return;
            if ((ulong)status != EFI_SUCCESS)
            {
                CompleteConnect(status);
                return;
            }

            waitingForAddress = false;
            SubmitConnect();
        }

        private bool IsSignaled(EFI_EVENT e)
            => (void*)e != null && (ulong)gBS->CheckEvent(e) == EFI_SUCCESS;

        private void CompleteConnect(EFI_STATUS status)
        {
            TaskCompletionSource completion = connectCompletion;
            connectCompletion = null;
            waitingForAddress = false;
            connected = (ulong)status == EFI_SUCCESS;
            UpdatePollingRegistration();

            if (completion == null)
                return;
            if ((ulong)status == EFI_SUCCESS)
                completion.TrySetResult();
            else
                completion.TrySetException(new SocketException(status));
        }

        private void CompleteTransmit(EFI_STATUS status)
        {
            TaskCompletionSource<int> completion = transmitCompletion;
            int bytesTransferred = (int)transmitData.FragmentTable.FragmentLength;
            transmitCompletion = null;
            transmitBuffer = null;
            transmitData.FragmentTable.FragmentBuffer = null;
            UpdatePollingRegistration();

            if (completion == null)
                return;
            if ((ulong)status == EFI_SUCCESS)
                completion.TrySetResult(bytesTransferred);
            else
                completion.TrySetException(new SocketException(status));
        }

        private void CompleteReceive(EFI_STATUS status)
        {
            TaskCompletionSource<int> completion = receiveCompletion;
            int bytesTransferred = (int)receiveData.FragmentTable.FragmentLength;
            receiveCompletion = null;
            receiveBuffer = null;
            receiveData.FragmentTable.FragmentBuffer = null;
            UpdatePollingRegistration();

            if (completion == null)
                return;
            if ((ulong)status == EFI_SUCCESS)
                completion.TrySetResult(bytesTransferred);
            else if ((ulong)status == EFI_CONNECTION_FIN)
                completion.TrySetResult(0);
            else
                completion.TrySetException(new SocketException(status));
        }

        private void CompleteClose(EFI_STATUS status)
        {
            TaskCompletionSource completion = closeCompletion;
            closeCompletion = null;
            if ((ulong)status == EFI_SUCCESS)
            {
                connected = false;
                ReleaseTcp();
            }
            UpdatePollingRegistration();

            if (completion == null)
                return;
            if ((ulong)status == EFI_SUCCESS)
                completion.TrySetResult();
            else
                completion.TrySetException(new SocketException(status));
        }

        private void UpdatePollingRegistration()
        {
            if (waitingForAddress || connectCompletion != null || receiveCompletion != null || transmitCompletion != null || closeCompletion != null)
                TaskScheduler.Register(poller);
            else
                TaskScheduler.Unregister(poller);
        }

        private void ReleaseTcp()
        {
            TaskScheduler.Unregister(poller);

            CloseEvent(ref connectionToken.CompletionToken.Event);
            CloseEvent(ref receiveToken.CompletionToken.Event);
            CloseEvent(ref transmitToken.CompletionToken.Event);
            CloseEvent(ref closeToken.CompletionToken.Event);

            if (tcp != null)
                gBS->CloseProtocol(tcpHandle, (EFI_GUID*)EFI_TCP4_PROTOCOL, gImageHandle, default);
            tcp = null;

            if (serviceBinding != null && (void*)tcpHandle != null)
                serviceBinding->DestroyChild(serviceBinding, tcpHandle);
            tcpHandle = default;

            if (serviceBinding != null && (void*)serviceHandle != null)
                gBS->CloseProtocol(serviceHandle, (EFI_GUID*)EFI_TCP4_SERVICE_BINDING_PROTOCOL, gImageHandle, default);
            serviceBinding = null;
            serviceHandle = default;
        }

        private static void CloseEvent(ref EFI_EVENT e)
        {
            if ((void*)e == null)
                return;

            gBS->CloseEvent(e);
            e = default;
        }
    }

    public sealed class SocketException : Exception
    {
        public SocketException(EFI_STATUS status) : base("The UEFI TCP operation failed.")
            => Status = status;

        public EFI_STATUS Status { get; }
    }
}
