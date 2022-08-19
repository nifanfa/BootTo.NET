using static efi;
using System.Runtime.InteropServices;

namespace System.Net.Sockets
{
    public unsafe class Socket
    {
        EFI_TCP4* tcp;
        EFI_TCP4_RECEIVE_DATA receiveData;
        EFI_TCP4_TRANSMIT_DATA transmitData;
        EFI_TCP4_IO_TOKEN receiveToken;
        EFI_TCP4_IO_TOKEN transmitToken;
        EFI_TCP4_CONFIG_DATA configuration;
        bool receiveDone;
        bool transmitDone;

        public Socket(SocketType socketType, ProtocolType protocolType)
        {
            if (socketType != SocketType.Stream || protocolType != ProtocolType.Tcp)
            {
                Console.WriteLine("Unsupported socket type!");
                for (; ; );
            }
        }

        public void Connect(EFI_IPv4_ADDRESS address, ushort port)
        {
            EFI_STATUS sts = EFI_SUCCESS;

            configuration = new EFI_TCP4_CONFIG_DATA();
            configuration.TimeToLive = 188;
            configuration.AccessPoint.UseDefaultAddress = true;

            configuration.AccessPoint.ActiveFlag = true;
            configuration.AccessPoint.RemotePort = port;
            configuration.AccessPoint.RemoteAddress = address;

            ulong numdevices;
            EFI_HANDLE* devices;

            sts = gBS->LocateHandleBuffer(
                EFI_LOCATE_SEARCH_TYPE.ByProtocol,
                EFI_TCP4_SERVICE_BINDING_PROTOCOL,
                null,
                &numdevices,
                &devices
                );

            if(sts == EFI_NOT_FOUND)
            {
                Console.WriteLine("Your UEFI firmware does not support TCP!");
                Console.WriteLine("Make sure you have \"Network Stack\" enabled on your BIOS!");
                for (; ; );
            }

            EFI_HANDLE dev = devices[0];

            EFI_SERVICE_BINDING* bs;

            gBS->OpenProtocol(
                dev,
                EFI_TCP4_SERVICE_BINDING_PROTOCOL,
                (void**)&bs,
                gImageHandle,
                default,
                EFI_OPEN_PROTOCOL_GET_PROTOCOL
                );

            EFI_HANDLE tcphandle;

            bs->CreateChild(bs, &tcphandle);

            fixed (EFI_TCP4** pcli = &tcp)
                gBS->OpenProtocol(
                    tcphandle,
                    EFI_TCP4_PROTOCOL,
                    (void**)pcli,
                    gImageHandle,
                    default,
                    EFI_OPEN_PROTOCOL_GET_PROTOCOL
                    );

            receiveData = new EFI_TCP4_RECEIVE_DATA();
            transmitData = new EFI_TCP4_TRANSMIT_DATA();
            receiveToken = new EFI_TCP4_IO_TOKEN();
            transmitToken = new EFI_TCP4_IO_TOKEN();

            receiveData.FragmentCount = 1;

            fixed (EFI_TCP4_RECEIVE_DATA* prx = &receiveData)
                receiveToken.Packet_RxData = prx;

            transmitData.FragmentCount = 1;
            transmitData.Push = true;
            fixed (EFI_TCP4_TRANSMIT_DATA* ptx = &transmitData)
                transmitToken.Packet_TxData = ptx;

            fixed (EFI_TCP4_CONFIG_DATA* pcfg = &configuration)
                sts = tcp->Configure(tcp, pcfg);

            EFI_IP4_MODE_DATA mode = new EFI_IP4_MODE_DATA();

            if (sts == EFI_NO_MAPPING)
            {
                Console.WriteLine("Trying to get an IP from DHCP server...");

                do
                {
                    tcp->GetModeData(tcp, null, null, &mode, null, null);
                } while (!mode.IsConfigured);
                fixed (EFI_TCP4_CONFIG_DATA* pcfg = &configuration)
                    tcp->Configure(tcp, pcfg);

                Console.Write("Your IP is: ");
                Console.Write(Convert.ToString(mode.ConfigData.StationAddress.Addr[0], 10));
                Console.Write('.');
                Console.Write(Convert.ToString(mode.ConfigData.StationAddress.Addr[1], 10));
                Console.Write('.');
                Console.Write(Convert.ToString(mode.ConfigData.StationAddress.Addr[2], 10));
                Console.Write('.');
                Console.Write(Convert.ToString(mode.ConfigData.StationAddress.Addr[3], 10));
                Console.WriteLine();
            }

            EFI_TCP4_CONNECTION_TOKEN conn;

            fixed (bool* preceiveDone = &receiveDone)
            fixed (EFI_TCP4_IO_TOKEN* preceiveToken = &receiveToken)
                gBS->CreateEvent(
                    EVT_NOTIFY_SIGNAL,
                    TPL_NOTIFY,
                    &OnEvent,
                    preceiveDone,
                    &preceiveToken->CompletionToken.Event
                    );

            fixed (bool* ptransmitDone = &transmitDone)
            fixed (EFI_TCP4_IO_TOKEN* ptransmitToken = &transmitToken)
                gBS->CreateEvent(
                    EVT_NOTIFY_SIGNAL,
                    TPL_NOTIFY,
                    &OnEvent,
                    ptransmitDone,
                    &ptransmitToken->CompletionToken.Event
                    );

            bool isConnected = false;

            gBS->CreateEvent(
                EVT_NOTIFY_SIGNAL,
                TPL_NOTIFY,
                &OnEvent,
                &isConnected,
                &conn.CompletionToken.Event
                );

            tcp->Connect(tcp, &conn);

            while (!isConnected) ;

            gBS->CloseEvent(conn.CompletionToken.Event);
        }

        public void Send(byte[] buffer)
        {
            transmitDone = false;

            transmitData.DataLength = (uint)buffer.Length;
            transmitData.FragmentCount = 1;
            transmitData.FragmentTable.FragmentLength = (uint)buffer.Length;
            fixed (byte* pbuf = buffer)
                transmitData.FragmentTable.FragmentBuffer = pbuf;

            fixed (EFI_TCP4_IO_TOKEN* ptransmitToken = &transmitToken)
                tcp->Transmit(tcp, ptransmitToken);

            while (!transmitDone) ;
        }

        public int Receive(byte[] buffer)
        {
            receiveDone = false;

            receiveData.DataLength = (uint)buffer.Length;
            receiveData.FragmentTable.FragmentLength = (uint)buffer.Length;
            fixed (byte* pbuf = buffer)
                receiveData.FragmentTable.FragmentBuffer = pbuf;

            fixed (EFI_TCP4_IO_TOKEN* preceiveToken = &receiveToken)
                tcp->Receive(tcp, preceiveToken);

            while (!receiveDone) tcp->Poll(tcp);

            return (int)receiveData.FragmentTable.FragmentLength;
        }

        [UnmanagedCallersOnly]
        public static void OnEvent(EFI_EVENT e, void* ctx)
        {
            if (ctx != null)
            {
                *(bool*)ctx = true;
            }
        }

        internal void Close()
        {
            bool isClosed = false;
            EFI_TCP4_CLOSE_TOKEN closeToken;
            gBS->CreateEvent(
                EVT_NOTIFY_SIGNAL,
                TPL_NOTIFY,
                &OnEvent,
                &isClosed,
                &closeToken.CompletionToken.Event
                );

            tcp->Close(tcp, &closeToken);

            while (!isClosed) ;

            gBS->CloseEvent(receiveToken.CompletionToken.Event);
            gBS->CloseEvent(transmitToken.CompletionToken.Event);
            gBS->CloseEvent(closeToken.CompletionToken.Event);
        }
    }
}
