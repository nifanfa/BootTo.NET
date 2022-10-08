/*++
Copyright (c) 2013  Intel Corporation

--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_ADDRESS_PAIR
{
    public EFI_HANDLE InstanceHandle;
    public EFI_IPv4_ADDRESS Ip4Address;
    public EFI_IPv4_ADDRESS SubnetMask;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_VARIABLE_DATA
{
    public EFI_HANDLE DriverHandle;
    public uint AddressCount;
    public EFI_IP4_ADDRESS_PAIR AddressPairs;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_CONFIG_DATA
{
    public byte DefaultProtocol;
    public bool AcceptAnyProtocol;
    public bool AcceptIcmpErrors;
    public bool AcceptBroadcast;
    public bool AcceptPromiscuous;
    public bool UseDefaultAddress;
    public EFI_IPv4_ADDRESS StationAddress;
    public EFI_IPv4_ADDRESS SubnetMask;
    public byte TypeOfService;
    public byte TimeToLive;
    public bool DoNotFragment;
    public bool RawData;
    public uint ReceiveTimeout;
    public uint TransmitTimeout;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_ROUTE_TABLE
{
    public EFI_IPv4_ADDRESS SubnetAddress;
    public EFI_IPv4_ADDRESS SubnetMask;
    public EFI_IPv4_ADDRESS GatewayAddress;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_ICMP_TYPE
{
    public byte Type;
    public byte Code;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_MODE_DATA
{
    public bool IsStarted;
    public uint MaxPacketSize;
    public EFI_IP4_CONFIG_DATA ConfigData;
    public bool IsConfigured;
    public uint GroupCount;
    public EFI_IPv4_ADDRESS* GroupTable;
    public uint RouteCount;
    public EFI_IP4_ROUTE_TABLE* RouteTable;
    public uint IcmpTypeCount;
    public EFI_IP4_ICMP_TYPE* IcmpTypeList;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_HEADER
{
    public byte HeaderLength_Version;
    public byte TypeOfService;
    public ushort TotalLength;
    public ushort Identification;
    public ushort Fragmentation;
    public byte TimeToLive;
    public byte Protocol;
    public ushort Checksum;
    public EFI_IPv4_ADDRESS SourceAddress;
    public EFI_IPv4_ADDRESS DestinationAddress;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_FRAGMENT_DATA
{
    public uint FragmentLength;
    public void* FragmentBuffer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_RECEIVE_DATA
{
    public EFI_TIME TimeStamp;
    public EFI_EVENT RecycleSignal;
    public uint HeaderLength;
    public EFI_IP4_HEADER* Header;
    public uint OptionsLength;
    public void* Options;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_IP4_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_OVERRIDE_DATA
{
    public EFI_IPv4_ADDRESS SourceAddress;
    public EFI_IPv4_ADDRESS GatewayAddress;
    public byte Protocol;
    public byte TypeOfService;
    public byte TimeToLive;
    public bool DoNotFragment;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_TRANSMIT_DATA
{
    public EFI_IPv4_ADDRESS DestinationAddress;
    public EFI_IP4_OVERRIDE_DATA* OverrideData;
    public uint OptionsLength;
    public void* OptionsBuffer;
    public uint TotalDataLength;
    public uint FragmentCount;
    public EFI_IP4_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4_COMPLETION_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS Status;
    void* Packet;
    public EFI_IP4_RECEIVE_DATA* Packet_RxData => (EFI_IP4_RECEIVE_DATA*)Packet;
    public EFI_IP4_TRANSMIT_DATA* Packet_TxData => (EFI_IP4_TRANSMIT_DATA*)Packet;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP4
{
    public readonly delegate* unmanaged<EFI_IP4*, EFI_IP4_MODE_DATA*, EFI_MANAGED_NETWORK_CONFIG_DATA*, EFI_SIMPLE_NETWORK_MODE*, EFI_STATUS> GetModeData;
    public readonly delegate* unmanaged<EFI_IP4*, EFI_IP4_CONFIG_DATA*, EFI_STATUS> Configure;
    public readonly delegate* unmanaged<EFI_IP4*, bool, EFI_IPv4_ADDRESS*, EFI_STATUS> Groups;
    public readonly delegate* unmanaged<EFI_IP4*, bool, EFI_IPv4_ADDRESS*, EFI_IPv4_ADDRESS*, EFI_IPv4_ADDRESS*, EFI_STATUS> Routes;
    public readonly delegate* unmanaged<EFI_IP4*, EFI_IP4_COMPLETION_TOKEN*, EFI_STATUS> Transmit;
    public readonly delegate* unmanaged<EFI_IP4*, EFI_IP4_COMPLETION_TOKEN*, EFI_STATUS> Receive;
    public readonly delegate* unmanaged<EFI_IP4*, EFI_IP4_COMPLETION_TOKEN*, EFI_STATUS> Cancel;
    public readonly delegate* unmanaged<EFI_IP4*, EFI_STATUS> Poll;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_CONFIG_DATA
{
    public byte DefaultProtocol;
    public bool AcceptAnyProtocol;
    public bool AcceptIcmpErrors;
    public bool AcceptPromiscuous;
    public EFI_IPv6_ADDRESS DestinationAddress;
    public EFI_IPv6_ADDRESS StationAddress;
    public byte TrafficClass;
    public byte HopLimit;
    public uint FlowLabel;
    public uint ReceiveTimeout;
    public uint TransmitTimeout;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_ADDRESS_INFO
{
    public EFI_IPv6_ADDRESS Address;
    public byte PrefixLength;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_ROUTE_TABLE
{
    public EFI_IPv6_ADDRESS Gateway;
    public EFI_IPv6_ADDRESS Destination;
    public byte PrefixLength;
}

public enum EFI_IP6_NEIGHBOR_STATE
{
    EfiNeighborInComplete,
    EfiNeighborReachable,
    EfiNeighborStale,
    EfiNeighborDelay,
    EfiNeighborProbe
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_NEIGHBOR_CACHE
{
    public EFI_IPv6_ADDRESS Neighbor;
    public EFI_MAC_ADDRESS LinkAddress;
    public EFI_IP6_NEIGHBOR_STATE State;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_ICMP_TYPE
{
    public byte Type;
    public byte Code;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_MODE_DATA
{
    public bool IsStarted;
    public uint MaxPacketSize;
    public EFI_IP6_CONFIG_DATA ConfigData;
    public bool IsConfigured;
    public uint AddressCount;
    public EFI_IP6_ADDRESS_INFO* AddressList;
    public uint GroupCount;
    public EFI_IPv6_ADDRESS* GroupTable;
    public uint RouteCount;
    public EFI_IP6_ROUTE_TABLE* RouteTable;
    public uint NeighborCount;
    public EFI_IP6_NEIGHBOR_CACHE* NeighborCache;
    public uint PrefixCount;
    public EFI_IP6_ADDRESS_INFO* PrefixTable;
    public uint IcmpTypeCount;
    public EFI_IP6_ICMP_TYPE* IcmpTypeList;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_FRAGMENT_DATA
{
    public uint FragmentLength;
    public void* FragmentBuffer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_OVERRIDE_DATA
{
    public byte Protocol;
    public byte HopLimit;
    public uint FlowLabel;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_TRANSMIT_DATA
{
    public EFI_IPv6_ADDRESS DestinationAddress;
    public EFI_IP6_OVERRIDE_DATA* OverrideData;
    public uint ExtHdrsLength;
    public void* ExtHdrs;
    public byte NextHeader;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_IP6_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_HEADER
{
    public byte TrafficClassH_Version;
    public byte FlowLabelH_TrafficClassL;
    public ushort FlowLabelL;
    public ushort PayloadLength;
    public byte NextHeader;
    public byte HopLimit;
    public EFI_IPv6_ADDRESS SourceAddress;
    public EFI_IPv6_ADDRESS DestinationAddress;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_RECEIVE_DATA
{
    public EFI_TIME TimeStamp;
    public EFI_EVENT RecycleSignal;
    public uint HeaderLength;
    public EFI_IP6_HEADER* Header;
    public uint DataLength;
    public uint FragmentCount;
    public EFI_IP6_FRAGMENT_DATA FragmentTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6_COMPLETION_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS Status;
    void* Packet;
    public EFI_IP6_RECEIVE_DATA* Packet_RxData => (EFI_IP6_RECEIVE_DATA*)Packet;
    public EFI_IP6_TRANSMIT_DATA* Packet_TxData => (EFI_IP6_TRANSMIT_DATA*)Packet;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IP6
{
    public readonly delegate* unmanaged<EFI_IP6*, EFI_IP6_MODE_DATA*, EFI_MANAGED_NETWORK_CONFIG_DATA*, EFI_SIMPLE_NETWORK_MODE*, EFI_STATUS> GetModeData;
    public readonly delegate* unmanaged<EFI_IP6*, EFI_IP6_CONFIG_DATA*, EFI_STATUS> Configure;
    public readonly delegate* unmanaged<EFI_IP6*, bool, EFI_IPv6_ADDRESS*, EFI_STATUS> Groups;
    public readonly delegate* unmanaged<EFI_IP6*, bool, EFI_IPv6_ADDRESS*, byte, EFI_IPv6_ADDRESS*, EFI_STATUS> Routes;
    public readonly delegate* unmanaged<EFI_IP6*, bool, EFI_IPv6_ADDRESS*, EFI_MAC_ADDRESS*, uint, bool, EFI_STATUS> Neighbors;
    public readonly delegate* unmanaged<EFI_IP6*, EFI_IP6_COMPLETION_TOKEN*, EFI_STATUS> Transmit;
    public readonly delegate* unmanaged<EFI_IP6*, EFI_IP6_COMPLETION_TOKEN*, EFI_STATUS> Receive;
    public readonly delegate* unmanaged<EFI_IP6*, EFI_IP6_COMPLETION_TOKEN*, EFI_STATUS> Cancel;
    public readonly delegate* unmanaged<EFI_IP6*, EFI_STATUS> Poll;
}

