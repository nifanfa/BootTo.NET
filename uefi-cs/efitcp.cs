/*++
Copyright (c) 2013  Intel Corporation

--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_ACCESS_POINT
{
    public bool UseDefaultAddress;
    public EFI_IPv4_ADDRESS StationAddress;
    public EFI_IPv4_ADDRESS SubnetMask;
    public ushort StationPort;
    public EFI_IPv4_ADDRESS RemoteAddress;
    public ushort RemotePort;
    public bool ActiveFlag;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_OPTION
{
    public uint ReceiveBufferSize;
    public uint SendBufferSize;
    public uint MaxSynBackLog;
    public uint ConnectionTimeout;
    public uint DataRetries;
    public uint FinTimeout;
    public uint TimeWaitTimeout;
    public uint KeepAliveProbes;
    public uint KeepAliveTime;
    public uint KeepAliveInterval;
    public bool EnableNagle;
    public bool EnableTimeStamp;
    public bool EnableWindowScaling;
    public bool EnableSelectiveAck;
    public bool EnablePAthMtuDiscovery;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_CONFIG_DATA
{
    // Receiving Filters
    // I/O parameters
    public byte TypeOfService;
    public byte TimeToLive;

    // Access Point
    public EFI_TCP4_ACCESS_POINT AccessPoint;

    // TCP Control Options
    public EFI_TCP4_OPTION* ControlOption;
}

public enum EFI_TCP4_CONNECTION_STATE
{
    Tcp4StateClosed = 0,
    Tcp4StateListen = 1,
    Tcp4StateSynSent = 2,
    Tcp4StateSynReceived = 3,
    Tcp4StateEstablished = 4,
    Tcp4StateFinWait1 = 5,
    Tcp4StateFinWait2 = 6,
    Tcp4StateClosing = 7,
    Tcp4StateTimeWait = 8,
    Tcp4StateCloseWait = 9,
    Tcp4StateLastAck = 10
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_COMPLETION_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS Status;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_CONNECTION_TOKEN
{
    public EFI_TCP4_COMPLETION_TOKEN CompletionToken;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_LISTEN_TOKEN
{
    public EFI_TCP4_COMPLETION_TOKEN CompletionToken;
    public EFI_HANDLE NewChildHandle;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_FRAGMENT_DATA
{
    public uint FragmentLength;
    public void* FragmentBuffer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_RECEIVE_DATA
{
    public bool UrgentFlag;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_TCP4_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_TRANSMIT_DATA
{
    public bool Push;
    public bool Urgent;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_TCP4_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_IO_TOKEN
{
    public EFI_TCP4_COMPLETION_TOKEN CompletionToken;
    void* Packet;
    public EFI_TCP4_RECEIVE_DATA* Packet_RxData
    {
        get => (EFI_TCP4_RECEIVE_DATA*)Packet;
        set => Packet = value;
    }
    public EFI_TCP4_TRANSMIT_DATA* Packet_TxData
    {
        get => (EFI_TCP4_TRANSMIT_DATA*)Packet;
        set => Packet = value;
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4_CLOSE_TOKEN
{
    public EFI_TCP4_COMPLETION_TOKEN CompletionToken;
    public bool AbortOnClose;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP4
{
    public readonly delegate* unmanaged<EFI_TCP4*, EFI_TCP4_CONNECTION_STATE*, EFI_TCP4_CONFIG_DATA*, EFI_IP4_MODE_DATA*, EFI_MANAGED_NETWORK_CONFIG_DATA*, EFI_SIMPLE_NETWORK_MODE*, EFI_STATUS> GetModeData;
    public readonly delegate* unmanaged<EFI_TCP4*, EFI_TCP4_CONFIG_DATA*, EFI_STATUS> Configure;
    public readonly delegate* unmanaged<EFI_TCP4*, bool, EFI_IPv4_ADDRESS*, EFI_IPv4_ADDRESS*, EFI_IPv4_ADDRESS*, EFI_STATUS> Routes;
    public readonly delegate* unmanaged<EFI_TCP4*, EFI_TCP4_CONNECTION_TOKEN*, EFI_STATUS> Connect;
    public readonly delegate* unmanaged<EFI_TCP4*, EFI_TCP4_LISTEN_TOKEN*, EFI_STATUS> Accept;
    public readonly delegate* unmanaged<EFI_TCP4*, EFI_TCP4_IO_TOKEN*, EFI_STATUS> Transmit;
    public readonly delegate* unmanaged<EFI_TCP4*, EFI_TCP4_IO_TOKEN*, EFI_STATUS> Receive;
    public readonly delegate* unmanaged<EFI_TCP4*, EFI_TCP4_CLOSE_TOKEN*, EFI_STATUS> Close;
    public readonly delegate* unmanaged<EFI_TCP4*, EFI_TCP4_COMPLETION_TOKEN*, EFI_STATUS> Cancel;
    public readonly delegate* unmanaged<EFI_TCP4*, EFI_STATUS> Poll;
}

public enum EFI_TCP6_CONNECTION_STATE
{
    Tcp6StateClosed = 0,
    Tcp6StateListen = 1,
    Tcp6StateSynSent = 2,
    Tcp6StateSynReceived = 3,
    Tcp6StateEstablished = 4,
    Tcp6StateFinWait1 = 5,
    Tcp6StateFinWait2 = 6,
    Tcp6StateClosing = 7,
    Tcp6StateTimeWait = 8,
    Tcp6StateCloseWait = 9,
    Tcp6StateLastAck = 10
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_ACCESS_POINT
{
    public EFI_IPv6_ADDRESS StationAddress;
    public ushort StationPort;
    public EFI_IPv6_ADDRESS RemoteAddress;
    public ushort RemotePort;
    public bool ActiveFlag;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_OPTION
{
    public uint ReceiveBufferSize;
    public uint SendBufferSize;
    public uint MaxSynBackLog;
    public uint ConnectionTimeout;
    public uint DataRetries;
    public uint FinTimeout;
    public uint TimeWaitTimeout;
    public uint KeepAliveProbes;
    public uint KeepAliveTime;
    public uint KeepAliveInterval;
    public bool EnableNagle;
    public bool EnableTimeStamp;
    public bool EnableWindbowScaling;
    public bool EnableSelectiveAck;
    public bool EnablePathMtuDiscovery;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_CONFIG_DATA
{
    public byte TrafficClass;
    public byte HopLimit;
    public EFI_TCP6_ACCESS_POINT AccessPoint;
    public EFI_TCP6_OPTION* ControlOption;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_COMPLETION_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS Status;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_CONNECTION_TOKEN
{
    public EFI_TCP6_COMPLETION_TOKEN CompletionToken;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_LISTEN_TOKEN
{
    public EFI_TCP6_COMPLETION_TOKEN CompletionToken;
    public EFI_HANDLE NewChildHandle;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_FRAGMENT_DATA
{
    public uint FragmentLength;
    public void* FragmentBuffer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_RECEIVE_DATA
{
    public bool UrgentFlag;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_TCP6_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_TRANSMIT_DATA
{
    public bool Push;
    public bool Urgent;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_TCP6_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_IO_TOKEN
{
    public EFI_TCP6_COMPLETION_TOKEN CompletionToken;
    void* Packet;
    public EFI_TCP6_RECEIVE_DATA* Packet_RxData
    {
        get => (EFI_TCP6_RECEIVE_DATA*)Packet;
        set => Packet = value;
    }
    public EFI_TCP6_TRANSMIT_DATA* Packet_TxData
    {
        get => (EFI_TCP6_TRANSMIT_DATA*)Packet;
        set => Packet = value;
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6_CLOSE_TOKEN
{
    public EFI_TCP6_COMPLETION_TOKEN CompletionToken;
    public bool AbortOnClose;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TCP6
{
    public readonly delegate* unmanaged<EFI_TCP6*, EFI_TCP6_CONNECTION_STATE*, EFI_TCP6_CONFIG_DATA*, EFI_IP6_MODE_DATA*, EFI_MANAGED_NETWORK_CONFIG_DATA*, EFI_SIMPLE_NETWORK_MODE*, EFI_STATUS> GetModeData;
    public readonly delegate* unmanaged<EFI_TCP6*, EFI_TCP6_CONFIG_DATA*, EFI_STATUS> Configure;
    public readonly delegate* unmanaged<EFI_TCP6*, EFI_TCP6_CONNECTION_TOKEN*, EFI_STATUS> Connect;
    public readonly delegate* unmanaged<EFI_TCP6*, EFI_TCP6_LISTEN_TOKEN*, EFI_STATUS> Accept;
    public readonly delegate* unmanaged<EFI_TCP6*, EFI_TCP6_IO_TOKEN*, EFI_STATUS> Transmit;
    public readonly delegate* unmanaged<EFI_TCP6*, EFI_TCP6_IO_TOKEN*, EFI_STATUS> Receive;
    public readonly delegate* unmanaged<EFI_TCP6*, EFI_TCP6_CLOSE_TOKEN*, EFI_STATUS> Close;
    public readonly delegate* unmanaged<EFI_TCP6*, EFI_TCP6_COMPLETION_TOKEN*, EFI_STATUS> Cancel;
    public readonly delegate* unmanaged<EFI_TCP6*, EFI_STATUS> Poll;
}

