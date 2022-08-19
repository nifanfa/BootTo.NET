using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP4_CONFIG_DATA
{
    public bool AcceptBroadcast;
    public bool AcceptPromiscuous;
    public bool AcceptAnyPort;
    public bool AllowDuplicatePort;
    public byte TypeOfService;
    public byte TimeToLive;
    public bool DoNotFragment;
    public uint ReceiveTimeout;
    public uint TransmitTimeout;
    public bool UseDefaultAddress;
    public EFI_IPv4_ADDRESS StationAddress;
    public EFI_IPv4_ADDRESS SubnetMask;
    public ushort StationPort;
    public EFI_IPv4_ADDRESS RemoteAddress;
    public ushort RemotePort;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP4_SESSION_DATA
{
    public EFI_IPv4_ADDRESS SourceAddress;
    public ushort SourcePort;
    public EFI_IPv4_ADDRESS DestinationAddress;
    public ushort DestinationPort;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP4_FRAGMENT_DATA
{
    public uint FragmentLength;
    public void* FragmentBuffer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP4_RECEIVE_DATA
{
    public EFI_TIME TimeStamp;
    public EFI_EVENT RecycleSignal;
    public EFI_UDP4_SESSION_DATA UdpSession;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_UDP4_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP4_TRANSMIT_DATA
{
    public EFI_UDP4_SESSION_DATA* UdpSessionData;
    public EFI_IPv4_ADDRESS* GatewayAddress;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_UDP4_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP4_COMPLETION_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS Status;
    void* Packet;
    public EFI_UDP4_RECEIVE_DATA* Packet_RxData => (EFI_UDP4_RECEIVE_DATA*)Packet;
    public EFI_UDP4_TRANSMIT_DATA* Packet_TxData => (EFI_UDP4_TRANSMIT_DATA*)Packet;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP4
{
    public readonly delegate* unmanaged<EFI_UDP4*, EFI_UDP4_CONFIG_DATA*, EFI_IP4_MODE_DATA*, EFI_MANAGED_NETWORK_CONFIG_DATA*, EFI_SIMPLE_NETWORK_MODE*, EFI_STATUS> GetModeData;
    public readonly delegate* unmanaged<EFI_UDP4*, EFI_UDP4_CONFIG_DATA*, EFI_STATUS> Configure;
    public readonly delegate* unmanaged<EFI_UDP4*, bool, EFI_IPv4_ADDRESS*, EFI_STATUS> Groups;
    public readonly delegate* unmanaged<EFI_UDP4*, bool, EFI_IPv4_ADDRESS*, EFI_IPv4_ADDRESS*, EFI_IPv4_ADDRESS*, EFI_STATUS> Routes;
    public readonly delegate* unmanaged<EFI_UDP4*, EFI_UDP4_COMPLETION_TOKEN*, EFI_STATUS> Transmit;
    public readonly delegate* unmanaged<EFI_UDP4*, EFI_UDP4_COMPLETION_TOKEN*, EFI_STATUS> Receive;
    public readonly delegate* unmanaged<EFI_UDP4*, EFI_UDP4_COMPLETION_TOKEN*, EFI_STATUS> Cancel;
    public readonly delegate* unmanaged<EFI_UDP4*, EFI_STATUS> Poll;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP6_CONFIG_DATA
{
    public bool AcceptPromiscuous;
    public bool AcceptAnyPort;
    public bool AllowDuplicatePort;
    public byte TrafficClass;
    public byte HopLimit;
    public uint ReceiveTimeout;
    public uint TransmitTimeout;
    public EFI_IPv6_ADDRESS StationAddress;
    public ushort StationPort;
    public EFI_IPv6_ADDRESS RemoteAddress;
    public ushort RemotePort;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP6_SESSION_DATA
{
    public EFI_IPv6_ADDRESS SourceAddress;
    public ushort SourcePort;
    public EFI_IPv6_ADDRESS DestinationAddress;
    public ushort DestinationPort;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP6_FRAGMENT_DATA
{
    public uint FragmentLength;
    public void* FragmentBuffer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP6_RECEIVE_DATA
{
    public EFI_TIME TimeStamp;
    public EFI_EVENT RecycleSignal;
    public EFI_UDP6_SESSION_DATA UdpSession;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_UDP6_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP6_TRANSMIT_DATA
{
    public EFI_UDP6_SESSION_DATA* UdpSessionData;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_UDP6_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP6_COMPLETION_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS Status;
    void* Packet;
    public EFI_UDP6_RECEIVE_DATA* Packet_RxData => (EFI_UDP6_RECEIVE_DATA*)Packet;
    public EFI_UDP6_TRANSMIT_DATA* Packet_TxData => (EFI_UDP6_TRANSMIT_DATA*)Packet;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UDP6
{
    public readonly delegate* unmanaged<EFI_UDP6*, EFI_UDP6_CONFIG_DATA*, EFI_IP6_MODE_DATA*, EFI_MANAGED_NETWORK_CONFIG_DATA*, EFI_SIMPLE_NETWORK_MODE*, EFI_STATUS> GetModeData;
    public readonly delegate* unmanaged<EFI_UDP6*, EFI_UDP6_CONFIG_DATA*, EFI_STATUS> Configure;
    public readonly delegate* unmanaged<EFI_UDP6*, bool, EFI_IPv6_ADDRESS*, EFI_STATUS> Groups;
    public readonly delegate* unmanaged<EFI_UDP6*, EFI_UDP6_COMPLETION_TOKEN*, EFI_STATUS> Transmit;
    public readonly delegate* unmanaged<EFI_UDP6*, EFI_UDP6_COMPLETION_TOKEN*, EFI_STATUS> Receive;
    public readonly delegate* unmanaged<EFI_UDP6*, EFI_UDP6_COMPLETION_TOKEN*, EFI_STATUS> Cancel;
    public readonly delegate* unmanaged<EFI_UDP6*, EFI_STATUS> Poll;
}

