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
        private EFI_UDP4* udp;
        private EFI_SERVICE_BINDING* serviceBinding;
        private EFI_HANDLE serviceHandle;
        private EFI_HANDLE tcpHandle;
        private EFI_HANDLE udpHandle;

        private EFI_TCP4_RECEIVE_DATA receiveData;
        private EFI_TCP4_TRANSMIT_DATA transmitData;
        private EFI_TCP4_IO_TOKEN receiveToken;
        private EFI_TCP4_IO_TOKEN transmitToken;
        private EFI_TCP4_CONNECTION_TOKEN connectionToken;
        private EFI_TCP4_CLOSE_TOKEN closeToken;
        private EFI_TCP4_LISTEN_TOKEN listenToken;
        private EFI_TCP4_CONFIG_DATA configuration;
        private EFI_TCP4_OPTION controlOption;

        private EFI_UDP4_CONFIG_DATA udpConfiguration;
        private EFI_UDP4_SESSION_DATA udpSession;
        private EFI_UDP4_TRANSMIT_DATA udpTransmitData;
        private EFI_UDP4_COMPLETION_TOKEN udpReceiveToken;
        private EFI_UDP4_COMPLETION_TOKEN udpTransmitToken;

        private TaskCompletionSource connectCompletion;
        private TaskCompletionSource<int> receiveCompletion;
        private TaskCompletionSource<int> transmitCompletion;
        private TaskCompletionSource closeCompletion;
        private TaskCompletionSource<Socket> acceptCompletion;
        private TaskCompletionSource<SocketReceiveResult> receiveFromCompletion;
        private TaskCompletionSource<int> udpReceiveCompletion;
        private TaskCompletionSource<int> udpTransmitCompletion;
        private TaskCompletionSource udpCloseCompletion;

        private byte[] receiveBuffer;
        private byte[] transmitBuffer;
        private IPAddress localAddress;
        private IPAddress remoteAddress;
        private int localPort;
        private int remotePort;
        private bool waitingForAddress;
        private bool connected;
        private bool bound;
        private bool udpConfigured;
        private bool listening;
        private uint udpAddressConfigurationAttempt;
        private readonly SocketType socketType;
        private readonly ProtocolType protocolType;
        private bool ownsServiceBinding = true;
        private readonly SocketPoller poller;

        private const uint DefaultConnectionTimeoutSeconds = 30;

        public Socket(SocketType socketType, ProtocolType protocolType)
        {
            if ((socketType != SocketType.Stream || protocolType != ProtocolType.Tcp) &&
                (socketType != SocketType.Dgram || protocolType != ProtocolType.Udp))
                throw new SocketException(EFI_UNSUPPORTED);

            this.socketType = socketType;
            this.protocolType = protocolType;
            poller = new SocketPoller(this);
        }

        private Socket(EFI_TCP4* acceptedTcp, EFI_HANDLE acceptedHandle, EFI_SERVICE_BINDING* binding,
            EFI_HANDLE bindingHandle)
        {
            socketType = SocketType.Stream;
            protocolType = ProtocolType.Tcp;
            tcp = acceptedTcp;
            tcpHandle = acceptedHandle;
            serviceBinding = binding;
            serviceHandle = bindingHandle;
            ownsServiceBinding = false;
            connected = true;
            poller = new SocketPoller(this);
            receiveData = new EFI_TCP4_RECEIVE_DATA();
            transmitData = new EFI_TCP4_TRANSMIT_DATA();
            receiveToken = new EFI_TCP4_IO_TOKEN();
            transmitToken = new EFI_TCP4_IO_TOKEN();
            closeToken = new EFI_TCP4_CLOSE_TOKEN();
            listenToken = new EFI_TCP4_LISTEN_TOKEN();
            receiveData.FragmentCount = 1;
            transmitData.FragmentCount = 1;
            transmitData.Push = true;
            fixed (EFI_TCP4_RECEIVE_DATA* data = &receiveData)
                receiveToken.Packet_RxData = data;
            fixed (EFI_TCP4_TRANSMIT_DATA* data = &transmitData)
                transmitToken.Packet_TxData = data;
            if ((ulong)CreateIoEvents() != EFI_SUCCESS)
                ReleaseTcp();
        }

        public void Connect(IPAddress address, int port)
            => ConnectAsync(address, port).GetAwaiter().GetResult();

        public Task ConnectAsync(IPAddress address, int port)
        {
            if (address == null)
                throw new ArgumentNullException("The remote address cannot be null.");
            if ((uint)port > ushort.MaxValue)
                throw new ArgumentException("The remote port must be between 0 and 65535.");

            return socketType switch
            {
                SocketType.Stream => ConnectTcpAsync(address, port),
                SocketType.Dgram => ConnectUdpAsync(address, port),
                _ => Task.FromException(new SocketException(EFI_UNSUPPORTED))
            };
        }

        private Task ConnectTcpAsync(IPAddress address, int port)
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
            configuration.AccessPoint.UseDefaultAddress = localAddress == null || localAddress.Equals(IPAddress.Any);
            configuration.AccessPoint.StationAddress = ToEfiIPv4Address(localAddress ?? IPAddress.Any);
            configuration.AccessPoint.StationPort = (ushort)localPort;
            configuration.AccessPoint.ActiveFlag = true;
            configuration.AccessPoint.RemotePort = (ushort)port;
            configuration.AccessPoint.RemoteAddress = ToEfiIPv4Address(address);

            status = ConfigureTcp();

            if ((ulong)status == EFI_NO_MAPPING)
            {
                StartConnectTimeout();
                waitingForAddress = true;
                UpdatePollingRegistration();
            }
            else if ((ulong)status == EFI_SUCCESS)
            {
                StartConnectTimeout();
                SubmitConnect();
            }
            else
            {
                CompleteConnect(status);
            }

            return completion.Task;
        }

        private static EFI_IPv4_ADDRESS ToEfiIPv4Address(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();
            EFI_IPv4_ADDRESS result = new EFI_IPv4_ADDRESS();
            result.Addr[0] = bytes[0];
            result.Addr[1] = bytes[1];
            result.Addr[2] = bytes[2];
            result.Addr[3] = bytes[3];
            return result;
        }

        public void Send(byte[] buffer)
            => SendAsync(buffer).GetAwaiter().GetResult();

        public Task<int> SendAsync(byte[] buffer)
        {
            if (buffer == null)
                return Task.FromException<int>(new Exception("The send buffer cannot be null."));

            return socketType switch
            {
                SocketType.Stream => SendTcpAsync(buffer),
                SocketType.Dgram => SendUdpAsync(buffer, remoteAddress, remotePort),
                _ => Task.FromException<int>(new SocketException(EFI_UNSUPPORTED))
            };
        }

        private Task<int> SendTcpAsync(byte[] buffer)
        {
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

            return socketType switch
            {
                SocketType.Stream => ReceiveTcpAsync(buffer),
                SocketType.Dgram => ReceiveUdpAsync(buffer),
                _ => Task.FromException<int>(new SocketException(EFI_UNSUPPORTED))
            };
        }

        private Task<int> ReceiveTcpAsync(byte[] buffer)
        {
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

        public void Bind(IPAddress address, int port)
        {
            if (address == null)
                throw new ArgumentNullException("The local address cannot be null.");
            if ((uint)port > ushort.MaxValue)
                throw new ArgumentException("The local port must be between 0 and 65535.");
            if (bound || connected || listening)
                throw new SocketException(EFI_ALREADY_STARTED);

            localAddress = address;
            localPort = port;

            switch (socketType)
            {
                case SocketType.Stream:
                    BindTcp();
                    break;
                case SocketType.Dgram:
                    BindUdp();
                    break;
                default:
                    throw new SocketException(EFI_UNSUPPORTED);
            }
        }

        private void BindTcp()
        {
            bound = true;
        }

        private void BindUdp()
        {
            EFI_STATUS status = InitializeUdp();
            if ((ulong)status != EFI_SUCCESS)
                throw new SocketException(status);
            status = ConfigureUdp(null, 0);
            if ((ulong)status != EFI_SUCCESS && (ulong)status != EFI_NO_MAPPING)
            {
                ReleaseUdp();
                throw new SocketException(status);
            }
            if ((ulong)status == EFI_NO_MAPPING)
                BeginUdpAddressConfigurationWait();
            bound = true;
        }

        public void Listen(int backlog)
        {
            if (socketType != SocketType.Stream)
                throw new SocketException(EFI_UNSUPPORTED);
            if (backlog < 0)
                throw new ArgumentException("The listen backlog cannot be negative.");
            if (listening)
                throw new SocketException(EFI_ALREADY_STARTED);
            if (!bound)
                Bind(IPAddress.Any, 0);

            EFI_STATUS status = InitializeTcp();
            if ((ulong)status != EFI_SUCCESS)
                throw new SocketException(status);

            configuration = new EFI_TCP4_CONFIG_DATA();
            configuration.TimeToLive = 188;
            configuration.AccessPoint.UseDefaultAddress = localAddress == null || localAddress.Equals(IPAddress.Any);
            configuration.AccessPoint.StationAddress = ToEfiIPv4Address(localAddress ?? IPAddress.Any);
            configuration.AccessPoint.StationPort = (ushort)localPort;
            configuration.AccessPoint.ActiveFlag = false;
            controlOption = new EFI_TCP4_OPTION();
            controlOption.MaxSynBackLog = (uint)backlog;
            status = ConfigureTcp();
            if ((ulong)status != EFI_SUCCESS)
            {
                ReleaseTcp();
                throw new SocketException(status);
            }

            listening = true;
        }

        public Socket Accept()
            => AcceptAsync().GetAwaiter().GetResult();

        public Task<Socket> AcceptAsync()
        {
            if (!listening || tcp == null)
                return Task.FromException<Socket>(new SocketException(EFI_NOT_STARTED));
            if (acceptCompletion != null)
                return Task.FromException<Socket>(new SocketException(EFI_ACCESS_DENIED));

            TaskCompletionSource<Socket> completion = new TaskCompletionSource<Socket>();
            acceptCompletion = completion;
            SubmitAccept();
            return completion.Task;
        }

        public int SendTo(byte[] buffer, IPAddress address, int port)
            => SendToAsync(buffer, address, port).GetAwaiter().GetResult();

        public Task<int> SendToAsync(byte[] buffer, IPAddress address, int port)
        {
            if (address == null)
                return Task.FromException<int>(new ArgumentNullException("The destination address cannot be null."));
            if ((uint)port > ushort.MaxValue)
                return Task.FromException<int>(new ArgumentException("The destination port must be between 0 and 65535."));
            if (socketType != SocketType.Dgram)
                return Task.FromException<int>(new SocketException(EFI_UNSUPPORTED));
            return SendUdpAsync(buffer, address, port);
        }

        public int ReceiveFrom(byte[] buffer, out IPAddress address, out int port)
        {
            SocketReceiveResult result = ReceiveFromAsync(buffer).GetAwaiter().GetResult();
            address = result.RemoteAddress;
            port = result.RemotePort;
            return result.BytesReceived;
        }

        public Task<SocketReceiveResult> ReceiveFromAsync(byte[] buffer)
        {
            if (socketType != SocketType.Dgram)
                return Task.FromException<SocketReceiveResult>(new SocketException(EFI_UNSUPPORTED));
            if (buffer == null)
                return Task.FromException<SocketReceiveResult>(new ArgumentNullException("The receive buffer cannot be null."));
            if (receiveFromCompletion != null || udpReceiveCompletion != null)
                return Task.FromException<SocketReceiveResult>(new SocketException(EFI_ACCESS_DENIED));
            if (!bound)
            {
                try
                {
                    Bind(IPAddress.Any, 0);
                }
                catch (Exception e)
                {
                    return Task.FromException<SocketReceiveResult>(e);
                }
            }
            TaskCompletionSource<SocketReceiveResult> completion = new TaskCompletionSource<SocketReceiveResult>();
            receiveFromCompletion = completion;
            receiveBuffer = buffer;
            if (waitingForAddress)
            {
                UpdatePollingRegistration();
                return completion.Task;
            }
            EFI_STATUS status = SubmitUdpReceive();
            if ((ulong)status != EFI_SUCCESS)
                CompleteUdpReceive(status);
            else
                UpdatePollingRegistration();
            return completion.Task;
        }

        public Task CloseAsync()
        {
            return socketType switch
            {
                SocketType.Stream => CloseTcpAsync(),
                SocketType.Dgram => CloseUdpAsync(),
                _ => Task.FromException(new SocketException(EFI_UNSUPPORTED))
            };
        }

        private Task CloseTcpAsync()
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
            return socketType switch
            {
                SocketType.Stream => PollTcp(),
                SocketType.Dgram => PollUdp(),
                _ => EFI_UNSUPPORTED
            };
        }

        private EFI_STATUS PollTcp()
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

            if (acceptCompletion != null && IsSignaled(listenToken.CompletionToken.Event))
                CompleteAccept(listenToken.CompletionToken.Status);

            return status;
        }

        private Task ConnectUdpAsync(IPAddress address, int port)
        {
            if (connected)
                return Task.CompletedTask;
            if (connectCompletion != null)
                return Task.FromException(new SocketException(EFI_ALREADY_STARTED));

            TaskCompletionSource completion = new TaskCompletionSource();
            connectCompletion = completion;
            remoteAddress = address;
            remotePort = port;

            EFI_STATUS status = InitializeUdp();
            if ((ulong)status == EFI_SUCCESS)
                bound = true;
            if ((ulong)status == EFI_SUCCESS)
                status = ConfigureUdp(address, port);
            if ((ulong)status == EFI_NO_MAPPING)
            {
                BeginUdpAddressConfigurationWait();
            }
            else if ((ulong)status == EFI_SUCCESS)
            {
                CompleteConnect(EFI_SUCCESS);
            }
            else
            {
                CompleteConnect(status);
            }
            return completion.Task;
        }

        private Task<int> SendUdpAsync(byte[] buffer, IPAddress address, int port)
        {
            if (buffer == null)
                return Task.FromException<int>(new ArgumentNullException("The send buffer cannot be null."));
            if (address == null || (uint)port > ushort.MaxValue)
                return Task.FromException<int>(new ArgumentException("The destination address or port is invalid."));
            if (!bound)
            {
                try
                {
                    Bind(IPAddress.Any, 0);
                }
                catch (Exception e)
                {
                    return Task.FromException<int>(e);
                }
            }
            if (udp == null)
                return Task.FromException<int>(new SocketException(EFI_NOT_STARTED));
            if (udpTransmitCompletion != null || udpCloseCompletion != null)
                return Task.FromException<int>(new SocketException(EFI_ACCESS_DENIED));

            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
            udpTransmitCompletion = completion;
            transmitBuffer = buffer;
            udpSession = new EFI_UDP4_SESSION_DATA();
            udpSession.DestinationAddress = ToEfiIPv4Address(address);
            udpSession.DestinationPort = (ushort)port;
            udpTransmitData = new EFI_UDP4_TRANSMIT_DATA();
            udpTransmitData.DataLength = (uint)buffer.Length;
            udpTransmitData.FragmentCount = 1;
            udpTransmitData.FragmentTable.FragmentLength = (uint)buffer.Length;
            if (waitingForAddress)
            {
                UpdatePollingRegistration();
                return completion.Task;
            }

            EFI_STATUS status = SubmitUdpTransmit();
            if ((ulong)status != EFI_SUCCESS)
                CompleteUdpTransmit(status);
            UpdatePollingRegistration();
            return completion.Task;
        }

        private Task<int> ReceiveUdpAsync(byte[] buffer)
        {
            if (buffer == null)
                return Task.FromException<int>(new ArgumentNullException("The receive buffer cannot be null."));
            if (udpReceiveCompletion != null || receiveFromCompletion != null || udpCloseCompletion != null)
                return Task.FromException<int>(new SocketException(EFI_ACCESS_DENIED));
            if (!bound)
            {
                try
                {
                    Bind(IPAddress.Any, 0);
                }
                catch (Exception e)
                {
                    return Task.FromException<int>(e);
                }
            }
            TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
            udpReceiveCompletion = completion;
            receiveBuffer = buffer;
            if (waitingForAddress)
            {
                UpdatePollingRegistration();
                return completion.Task;
            }
            EFI_STATUS status = SubmitUdpReceive();
            if ((ulong)status != EFI_SUCCESS)
                CompleteUdpReceive(status);
            else
                UpdatePollingRegistration();
            return completion.Task;
        }

        private Task CloseUdpAsync()
        {
            if (udp == null)
                return Task.CompletedTask;
            if (udpCloseCompletion != null)
                return udpCloseCompletion.Task;
            TaskCompletionSource completion = new TaskCompletionSource();
            udpCloseCompletion = completion;
            TaskCompletionSource pendingConnect = connectCompletion;
            connectCompletion = null;
            if (pendingConnect != null)
                pendingConnect.TrySetException(new SocketException(EFI_ABORTED));
            if (udpReceiveCompletion == null && receiveFromCompletion == null && udpTransmitCompletion == null)
            {
                ReleaseUdp();
                udpCloseCompletion = null;
                completion.TrySetResult();
            }
            else if (waitingForAddress)
            {
                waitingForAddress = false;
                udpAddressConfigurationAttempt++;
                if (udpReceiveCompletion != null || receiveFromCompletion != null)
                    CompleteUdpReceive(EFI_ABORTED);
                if (udpTransmitCompletion != null)
                    CompleteUdpTransmit(EFI_ABORTED);
                MaybeCompleteUdpClose();
            }
            else
            {
                if (udpReceiveCompletion != null || receiveFromCompletion != null)
                {
                    fixed (EFI_UDP4_COMPLETION_TOKEN* token = &udpReceiveToken)
                        udp->Cancel(udp, token);
                }
                if (udpTransmitCompletion != null)
                {
                    fixed (EFI_UDP4_COMPLETION_TOKEN* token = &udpTransmitToken)
                        udp->Cancel(udp, token);
                }
                UpdatePollingRegistration();
            }
            return completion.Task;
        }

        private EFI_STATUS PollUdp()
        {
            if (udp == null)
                return EFI_NOT_STARTED;
            EFI_STATUS status = udp->Poll(udp);
            if (waitingForAddress)
                PollUdpAddressConfiguration();
            if ((udpReceiveCompletion != null || receiveFromCompletion != null) && IsSignaled(udpReceiveToken.Event))
                CompleteUdpReceive(udpReceiveToken.Status);
            if (udpTransmitCompletion != null && IsSignaled(udpTransmitToken.Event))
                CompleteUdpTransmit(udpTransmitToken.Status);
            MaybeCompleteUdpClose();
            return status;
        }

        private void PollUdpAddressConfiguration()
        {
            EFI_IP4_MODE_DATA mode = new EFI_IP4_MODE_DATA();
            EFI_STATUS status = udp->GetModeData(udp, null, &mode, null, null);
            if ((ulong)status != EFI_SUCCESS && (ulong)status != EFI_NO_MAPPING)
            {
                CompleteUdpAddressConfiguration(status);
                return;
            }
            if (!mode.IsConfigured)
                return;
            status = ConfigureUdp(remoteAddress, remotePort);
            if ((ulong)status == EFI_NO_MAPPING)
                return;
            if ((ulong)status != EFI_SUCCESS)
            {
                CompleteUdpAddressConfiguration(status);
                return;
            }

            CompleteUdpAddressConfiguration(EFI_SUCCESS);
        }

        private void BeginUdpAddressConfigurationWait()
        {
            if (waitingForAddress)
                return;

            waitingForAddress = true;
            uint attempt = ++udpAddressConfigurationAttempt;
            Task.Delay((int)(DefaultConnectionTimeoutSeconds * 1000)).AddContinuation(
                () => UdpAddressConfigurationTimedOut(attempt));
            UpdatePollingRegistration();
        }

        private void UdpAddressConfigurationTimedOut(uint attempt)
        {
            if (!waitingForAddress || attempt != udpAddressConfigurationAttempt)
                return;

            CompleteUdpAddressConfiguration(EFI_TIMEOUT);
        }

        private void CompleteUdpAddressConfiguration(EFI_STATUS status)
        {
            waitingForAddress = false;
            udpAddressConfigurationAttempt++;

            if ((ulong)status != EFI_SUCCESS)
            {
                if (connectCompletion != null)
                    CompleteConnect(status);
                if (udpTransmitCompletion != null)
                    CompleteUdpTransmit(status);
                if (udpReceiveCompletion != null || receiveFromCompletion != null)
                    CompleteUdpReceive(status);
                if (udp != null)
                    ReleaseUdp();
                return;
            }

            if (connectCompletion != null)
                CompleteConnect(status);
            if (udpReceiveCompletion != null || receiveFromCompletion != null)
            {
                status = SubmitUdpReceive();
                if ((ulong)status != EFI_SUCCESS)
                    CompleteUdpReceive(status);
            }
            if (udpTransmitCompletion != null)
            {
                status = SubmitUdpTransmit();
                if ((ulong)status != EFI_SUCCESS)
                    CompleteUdpTransmit(status);
            }
            UpdatePollingRegistration();
        }

        private EFI_STATUS InitializeUdp()
        {
            if (udp != null)
                return EFI_SUCCESS;
            ulong deviceCount = 0;
            EFI_HANDLE* devices = null;
            EFI_STATUS status = gBS->LocateHandleBuffer(
                ByProtocol,
                (EFI_GUID*)EFI_UDP4_SERVICE_BINDING_PROTOCOL,
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
            status = gBS->OpenProtocol(serviceHandle, (EFI_GUID*)EFI_UDP4_SERVICE_BINDING_PROTOCOL,
                (void**)&binding, gImageHandle, default, EFI_OPEN_PROTOCOL_GET_PROTOCOL);
            gBS->FreePool(devices);
            if ((ulong)status != EFI_SUCCESS)
                return status;
            serviceBinding = binding;
            EFI_HANDLE childHandle = default;
            status = serviceBinding->CreateChild(serviceBinding, &childHandle);
            if ((ulong)status != EFI_SUCCESS)
                return status;
            udpHandle = childHandle;
            fixed (EFI_UDP4** protocol = &udp)
                status = gBS->OpenProtocol(udpHandle, (EFI_GUID*)EFI_UDP4_PROTOCOL,
                    (void**)protocol, gImageHandle, default, EFI_OPEN_PROTOCOL_GET_PROTOCOL);
            if ((ulong)status != EFI_SUCCESS)
            {
                serviceBinding->DestroyChild(serviceBinding, udpHandle);
                udpHandle = default;
                return status;
            }
            udpReceiveToken = new EFI_UDP4_COMPLETION_TOKEN();
            udpTransmitToken = new EFI_UDP4_COMPLETION_TOKEN();
            status = CreateUdpEvents();
            if ((ulong)status != EFI_SUCCESS)
                ReleaseUdp();
            return status;
        }

        private EFI_STATUS CreateUdpEvents()
        {
            EFI_STATUS status;
            fixed (EFI_UDP4_COMPLETION_TOKEN* token = &udpReceiveToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->Event);
            if ((ulong)status != EFI_SUCCESS)
                return status;
            fixed (EFI_UDP4_COMPLETION_TOKEN* token = &udpTransmitToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->Event);
            return status;
        }

        private EFI_STATUS ConfigureUdp(IPAddress address, int port)
        {
            if (udpConfigured)
            {
                EFI_STATUS resetStatus = udp->Configure(udp, null);
                if ((ulong)resetStatus != EFI_SUCCESS)
                    return resetStatus;
                udpConfigured = false;
            }
            udpConfiguration = new EFI_UDP4_CONFIG_DATA();
            udpConfiguration.AcceptBroadcast = true;
            udpConfiguration.AcceptAnyPort = localPort == 0;
            udpConfiguration.AllowDuplicatePort = false;
            udpConfiguration.TimeToLive = 64;
            udpConfiguration.ReceiveTimeout = 0;
            udpConfiguration.TransmitTimeout = 0;
            udpConfiguration.UseDefaultAddress = localAddress == null || localAddress.Equals(IPAddress.Any);
            udpConfiguration.StationAddress = ToEfiIPv4Address(localAddress ?? IPAddress.Any);
            udpConfiguration.StationPort = (ushort)localPort;
            if (address != null)
            {
                udpConfiguration.RemoteAddress = ToEfiIPv4Address(address);
                udpConfiguration.RemotePort = (ushort)port;
            }
            fixed (EFI_UDP4_CONFIG_DATA* config = &udpConfiguration)
            {
                EFI_STATUS status = udp->Configure(udp, config);
                if ((ulong)status == EFI_SUCCESS)
                    udpConfigured = true;
                return status;
            }
        }

        private EFI_STATUS SubmitUdpReceive()
        {
            udpReceiveToken.Packet_RxData = null;
            udpReceiveToken.Status = EFI_NOT_READY;
            fixed (EFI_UDP4_COMPLETION_TOKEN* token = &udpReceiveToken)
                return udp->Receive(udp, token);
        }

        private EFI_STATUS SubmitUdpTransmit()
        {
            fixed (byte* data = transmitBuffer)
            fixed (EFI_UDP4_SESSION_DATA* session = &udpSession)
            fixed (EFI_UDP4_TRANSMIT_DATA* transmit = &udpTransmitData)
            fixed (EFI_UDP4_COMPLETION_TOKEN* token = &udpTransmitToken)
            {
                udpTransmitData.FragmentTable.FragmentBuffer = data;
                udpTransmitData.UdpSessionData = session;
                udpTransmitToken.Packet_TxData = transmit;
                udpTransmitToken.Status = EFI_NOT_READY;
                return udp->Transmit(udp, token);
            }
        }

        private void CompleteUdpReceive(EFI_STATUS status)
        {
            TaskCompletionSource<int> completion = udpReceiveCompletion;
            TaskCompletionSource<SocketReceiveResult> fromCompletion = receiveFromCompletion;
            udpReceiveCompletion = null;
            receiveFromCompletion = null;
            int bytes = 0;
            IPAddress source = IPAddress.None;
            int sourcePort = 0;
            if ((ulong)status == EFI_SUCCESS)
            {
                EFI_UDP4_RECEIVE_DATA* packet = udpReceiveToken.Packet_RxData;
                if (packet == null)
                {
                    status = EFI_DEVICE_ERROR;
                }
                else
                {
                    bytes = CopyUdpPacket(packet, receiveBuffer);
                    source = new IPAddress((long)ToUInt32(packet->UdpSession.SourceAddress));
                    sourcePort = packet->UdpSession.SourcePort;
                    if ((void*)packet->RecycleSignal != null)
                        gBS->SignalEvent(packet->RecycleSignal);
                }
            }
            udpReceiveToken.Packet_RxData = null;
            receiveBuffer = null;
            UpdatePollingRegistration();
            if (completion != null)
            {
                if ((ulong)status == EFI_SUCCESS)
                    completion.TrySetResult(bytes);
                else
                    completion.TrySetException(new SocketException(status));
            }
            if (fromCompletion != null)
            {
                if ((ulong)status == EFI_SUCCESS)
                    fromCompletion.TrySetResult(new SocketReceiveResult(bytes, source, sourcePort));
                else
                    fromCompletion.TrySetException(new SocketException(status));
            }
            MaybeCompleteUdpClose();
        }

        private static int CopyUdpPacket(EFI_UDP4_RECEIVE_DATA* packet, byte[] destination)
        {
            if (destination == null)
                return 0;

            int remaining = (int)packet->DataLength;
            if (remaining > destination.Length)
                remaining = destination.Length;
            int copied = 0;
            EFI_UDP4_FRAGMENT_DATA* fragment = &packet->FragmentTable;
            for (uint i = 0; i < packet->FragmentCount && copied < remaining; i++)
            {
                int fragmentLength = (int)fragment[i].FragmentLength;
                if (fragmentLength > remaining - copied)
                    fragmentLength = remaining - copied;
                byte* source = (byte*)fragment[i].FragmentBuffer;
                for (int j = 0; j < fragmentLength; j++)
                    destination[copied + j] = source[j];
                copied += fragmentLength;
            }
            return copied;
        }

        private void CompleteUdpTransmit(EFI_STATUS status)
        {
            TaskCompletionSource<int> completion = udpTransmitCompletion;
            udpTransmitCompletion = null;
            int bytes = (int)udpTransmitData.FragmentTable.FragmentLength;
            transmitBuffer = null;
            UpdatePollingRegistration();
            if (completion == null)
                return;
            if ((ulong)status == EFI_SUCCESS)
                completion.TrySetResult(bytes);
            else
                completion.TrySetException(new SocketException(status));
            MaybeCompleteUdpClose();
        }

        private void MaybeCompleteUdpClose()
        {
            if (udpCloseCompletion == null || udpReceiveCompletion != null || receiveFromCompletion != null || udpTransmitCompletion != null)
                return;
            TaskCompletionSource completion = udpCloseCompletion;
            udpCloseCompletion = null;
            ReleaseUdp();
            completion.TrySetResult();
        }

        private void ReleaseUdp()
        {
            TaskScheduler.Unregister(poller);
            CloseEvent(ref udpReceiveToken.Event);
            CloseEvent(ref udpTransmitToken.Event);
            if (udp != null)
                gBS->CloseProtocol(udpHandle, (EFI_GUID*)EFI_UDP4_PROTOCOL, gImageHandle, default);
            udp = null;
            if (serviceBinding != null && (void*)udpHandle != null)
                serviceBinding->DestroyChild(serviceBinding, udpHandle);
            udpHandle = default;
            if (serviceBinding != null && (void*)serviceHandle != null)
                gBS->CloseProtocol(serviceHandle, (EFI_GUID*)EFI_UDP4_SERVICE_BINDING_PROTOCOL, gImageHandle, default);
            serviceBinding = null;
            serviceHandle = default;
            bound = false;
            connected = false;
            udpConfigured = false;
            waitingForAddress = false;
            udpAddressConfigurationAttempt++;
        }

        private static uint ToUInt32(EFI_IPv4_ADDRESS address)
            => (uint)(address.Addr[0] | ((uint)address.Addr[1] << 8) |
                ((uint)address.Addr[2] << 16) | ((uint)address.Addr[3] << 24));

        internal void Close()
            => CloseAsync().GetAwaiter().GetResult();

        private EFI_STATUS InitializeTcp()
        {
            if (tcp != null)
                return EFI_SUCCESS;

            ulong deviceCount = 0;
            EFI_HANDLE* devices = null;
            EFI_STATUS status = gBS->LocateHandleBuffer(
                ByProtocol,
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
            if ((ulong)status != EFI_SUCCESS)
                return status;

            fixed (EFI_TCP4_LISTEN_TOKEN* token = &listenToken)
                status = gBS->CreateEvent(0, TPL_APPLICATION, null, null, &token->CompletionToken.Event);
            return status;
        }

        private EFI_STATUS CreateIoEvents()
        {
            EFI_STATUS status;
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

        private void SubmitAccept()
        {
            if (acceptCompletion == null || tcp == null)
                return;
            listenToken.CompletionToken.Status = EFI_NOT_READY;
            listenToken.NewChildHandle = default;
            EFI_STATUS status;
            fixed (EFI_TCP4_LISTEN_TOKEN* token = &listenToken)
                status = tcp->Accept(tcp, token);
            if ((ulong)status == EFI_SUCCESS)
                UpdatePollingRegistration();
            else
                CompleteAccept(status);
        }

        private void CompleteAccept(EFI_STATUS status)
        {
            TaskCompletionSource<Socket> completion = acceptCompletion;
            acceptCompletion = null;
            if (completion == null)
                return;

            if ((ulong)status != EFI_SUCCESS)
            {
                completion.TrySetException(new SocketException(status));
                UpdatePollingRegistration();
                return;
            }

            EFI_HANDLE childHandle = listenToken.NewChildHandle;
            EFI_TCP4* child = null;
            EFI_STATUS openStatus;
            openStatus = gBS->OpenProtocol(
                childHandle,
                (EFI_GUID*)EFI_TCP4_PROTOCOL,
                (void**)&child,
                gImageHandle,
                default,
                EFI_OPEN_PROTOCOL_GET_PROTOCOL);
            if ((ulong)openStatus != EFI_SUCCESS)
            {
                if ((void*)serviceBinding != null)
                    serviceBinding->DestroyChild(serviceBinding, childHandle);
                completion.TrySetException(new SocketException(openStatus));
                UpdatePollingRegistration();
                return;
            }

            Socket accepted = new Socket(child, childHandle, serviceBinding, serviceHandle);
            if (accepted.tcp == null)
            {
                completion.TrySetException(new SocketException(EFI_OUT_OF_RESOURCES));
                UpdatePollingRegistration();
                return;
            }
            completion.TrySetResult(accepted);
            UpdatePollingRegistration();
        }

        private void StartConnectTimeout()
            => Task.Delay((int)(DefaultConnectionTimeoutSeconds * 1000)).AddContinuation(ConnectTimedOut);

        private void ConnectTimedOut()
        {
            if (connectCompletion == null)
                return;

            if (tcp != null && !waitingForAddress)
            {
                fixed (EFI_TCP4_CONNECTION_TOKEN* token = &connectionToken)
                    tcp->Cancel(tcp, &token->CompletionToken);
            }

            CompleteConnect(EFI_TIMEOUT);
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

            status = ConfigureTcp();

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

        private EFI_STATUS ConfigureTcp()
        {
            uint maxSynBackLog = controlOption.MaxSynBackLog;
            controlOption = new EFI_TCP4_OPTION();
            controlOption.ConnectionTimeout = DefaultConnectionTimeoutSeconds;
            controlOption.MaxSynBackLog = maxSynBackLog;

            EFI_STATUS status;
            fixed (EFI_TCP4_OPTION* options = &controlOption)
            fixed (EFI_TCP4_CONFIG_DATA* config = &configuration)
            {
                config->ControlOption = options;
                status = tcp->Configure(tcp, config);
                config->ControlOption = null;
            }

            return status;
        }

        private bool IsSignaled(EFI_EVENT e)
            => (void*)e != null && (ulong)gBS->CheckEvent(e) == EFI_SUCCESS;

        private void CompleteConnect(EFI_STATUS status)
        {
            TaskCompletionSource completion = connectCompletion;
            connectCompletion = null;
            waitingForAddress = false;
            connected = (ulong)status == EFI_SUCCESS;
            if (!connected && (ulong)status == EFI_TIMEOUT)
            {
                switch (socketType)
                {
                    case SocketType.Stream:
                        ReleaseTcp();
                        break;
                    case SocketType.Dgram:
                        ReleaseUdp();
                        break;
                }
            }
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
                listening = false;
                ReleaseTcp();
            }
            TaskCompletionSource pendingConnect = connectCompletion;
            connectCompletion = null;
            if (pendingConnect != null)
                pendingConnect.TrySetException(new SocketException(status));
            if (completion == null)
            {
                TaskCompletionSource<Socket> pendingAccept = acceptCompletion;
                acceptCompletion = null;
                if (pendingAccept != null)
                    pendingAccept.TrySetException(new SocketException(status));
                UpdatePollingRegistration();
                return;
            }
            if ((ulong)status == EFI_SUCCESS)
                completion.TrySetResult();
            else
                completion.TrySetException(new SocketException(status));

            TaskCompletionSource<Socket> accept = acceptCompletion;
            acceptCompletion = null;
            if (accept != null)
                accept.TrySetException(new SocketException(status));
            UpdatePollingRegistration();
        }

        private void UpdatePollingRegistration()
        {
            if (waitingForAddress || connectCompletion != null || receiveCompletion != null || transmitCompletion != null ||
                closeCompletion != null || acceptCompletion != null || receiveFromCompletion != null ||
                udpReceiveCompletion != null || udpTransmitCompletion != null || udpCloseCompletion != null)
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
            CloseEvent(ref listenToken.CompletionToken.Event);

            if (tcp != null)
                gBS->CloseProtocol(tcpHandle, (EFI_GUID*)EFI_TCP4_PROTOCOL, gImageHandle, default);
            tcp = null;

            if (serviceBinding != null && (void*)tcpHandle != null)
                serviceBinding->DestroyChild(serviceBinding, tcpHandle);
            tcpHandle = default;

            if (ownsServiceBinding && serviceBinding != null && (void*)serviceHandle != null)
                gBS->CloseProtocol(serviceHandle, (EFI_GUID*)EFI_TCP4_SERVICE_BINDING_PROTOCOL, gImageHandle, default);
            if (ownsServiceBinding)
            {
                serviceBinding = null;
                serviceHandle = default;
            }
        }

        private static void CloseEvent(ref EFI_EVENT e)
        {
            if ((void*)e == null)
                return;

            gBS->CloseEvent(e);
            e = default;
        }
    }

    public sealed class SocketReceiveResult
    {
        internal SocketReceiveResult(int bytesReceived, IPAddress remoteAddress, int remotePort)
        {
            BytesReceived = bytesReceived;
            RemoteAddress = remoteAddress;
            RemotePort = remotePort;
        }

        public int BytesReceived { get; }
        public IPAddress RemoteAddress { get; }
        public int RemotePort { get; }
    }

    public sealed class SocketException : Exception
    {
        public SocketException(EFI_STATUS status) : base("The UEFI socket operation failed with status " + (ulong)status + ".")
            => Status = status;

        public SocketException(EFI_STATUS status, string message) : base(message)
            => Status = status;

        public EFI_STATUS Status { get; }
    }
}
