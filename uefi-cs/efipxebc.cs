/*++

Copyright (c) 1998  Intel Corporation

Module Name:

    efipxebc.h

Abstract:

    EFI PXE Base Code Protocol



Revision History

--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct EFI_IP_ADDRESS
{
    [FieldOffset(0)]
    public fixed uint Addr[4];
    [FieldOffset(0)]
    public EFI_IPv4_ADDRESS v4;
    [FieldOffset(0)]
    public EFI_IPv6_ADDRESS v6;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_UDP_PORT
{
    ushort Value;

    public static implicit operator EFI_PXE_BASE_CODE_UDP_PORT(ushort value) => new EFI_PXE_BASE_CODE_UDP_PORT() { Value = value };
    public static implicit operator ushort(EFI_PXE_BASE_CODE_UDP_PORT value) => value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_DHCPV4_PACKET
{
    public byte BootpOpcode;
    public byte BootpHwType;
    public byte BootpHwAddrLen;
    public byte BootpGateHops;
    public uint BootpIdent;
    public ushort BootpSeconds;
    public ushort BootpFlags;
    public fixed byte BootpCiAddr[4];
    public fixed byte BootpYiAddr[4];
    public fixed byte BootpSiAddr[4];
    public fixed byte BootpGiAddr[4];
    public fixed byte BootpHwAddr[16];
    public fixed byte BootpSrvName[64];
    public fixed byte BootpBootFile[128];
    public uint DhcpMagik;
    public fixed byte DhcpOptions[56];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_DHCPV6_PACKET
{
    public uint MessageType_TransactionId;
    public fixed byte DhcpOptions[1024];
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct EFI_PXE_BASE_CODE_PACKET
{
    [FieldOffset(0)]
    public fixed byte Raw[1472];
    [FieldOffset(0)]
    public EFI_PXE_BASE_CODE_DHCPV4_PACKET Dhcpv4;
    [FieldOffset(0)]
    public EFI_PXE_BASE_CODE_DHCPV6_PACKET Dhcpv6;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct EFI_PXE_BASE_CODE_ICMP_ERROR
{
    [FieldOffset(0)]
    public byte Type;
    [FieldOffset(1)]
    public byte Code;
    [FieldOffset(2)]
    public ushort Checksum;
    [FieldOffset(4)]
    public uint u_reserved;
    [FieldOffset(4)]
    public uint u_Mtu;
    [FieldOffset(4)]
    public uint u_Pointer;
    [FieldOffset(4)]
    EFI_PXE_BASE_CODE_ICMP_ERROR_ECHO u_Echo;
    [FieldOffset(8)]
    public fixed byte Data[494];
}


[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_ICMP_ERROR_ECHO
{
    public ushort Identifier;
    public ushort Sequence;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_TFTP_ERROR
{
    public byte ErrorCode;
    public fixed byte ErrorString[127];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_IP_FILTER
{
    public byte Filters;
    public byte IpCnt;
    public ushort reserved;
    public EFI_IP_ADDRESS IpList_0;
    public EFI_IP_ADDRESS IpList_1;
    public EFI_IP_ADDRESS IpList_2;
    public EFI_IP_ADDRESS IpList_3;
    public EFI_IP_ADDRESS IpList_4;
    public EFI_IP_ADDRESS IpList_5;
    public EFI_IP_ADDRESS IpList_6;
    public EFI_IP_ADDRESS IpList_7;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_ARP_ENTRY
{
    public EFI_IP_ADDRESS IpAddr;
    public EFI_MAC_ADDRESS MacAddr;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_ROUTE_ENTRY
{
    public EFI_IP_ADDRESS IpAddr;
    public EFI_IP_ADDRESS SubnetMask;
    public EFI_IP_ADDRESS GwAddr;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_SRVLIST
{
    public ushort Type;
    public bool AcceptAnyResponse;
    public byte Reserved;
    public EFI_IP_ADDRESS IpAddr;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_DISCOVER_INFO
{
    public bool UseMCast;
    public bool UseBCast;
    public bool UseUCast;
    public bool MustUseList;
    public EFI_IP_ADDRESS ServerMCastIp;
    public ushort IpCnt;
    public EFI_PXE_BASE_CODE_SRVLIST SrvList;
}

public enum EFI_PXE_BASE_CODE_TFTP_OPCODE
{
    EFI_PXE_BASE_CODE_TFTP_FIRST,
    EFI_PXE_BASE_CODE_TFTP_GET_FILE_SIZE,
    EFI_PXE_BASE_CODE_TFTP_READ_FILE,
    EFI_PXE_BASE_CODE_TFTP_WRITE_FILE,
    EFI_PXE_BASE_CODE_TFTP_READ_DIRECTORY,
    EFI_PXE_BASE_CODE_MTFTP_GET_FILE_SIZE,
    EFI_PXE_BASE_CODE_MTFTP_READ_FILE,
    EFI_PXE_BASE_CODE_MTFTP_READ_DIRECTORY,
    EFI_PXE_BASE_CODE_MTFTP_LAST
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_MTFTP_INFO
{
    public EFI_IP_ADDRESS MCastIp;
    public EFI_PXE_BASE_CODE_UDP_PORT CPort;
    public EFI_PXE_BASE_CODE_UDP_PORT SPort;
    public ushort ListenTimeout;
    public ushort TransmitTimeout;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_MODE
{
    public bool Started;
    public bool Ipv6Available;
    public bool Ipv6Supported;
    public bool UsingIpv6;
    public bool BisSupported;
    public bool BisDetected;
    public bool AutoArp;
    public bool SendGUID;
    public bool DhcpDiscoverValid;
    public bool DhcpAckReceived;
    public bool ProxyOfferReceived;
    public bool PxeDiscoverValid;
    public bool PxeReplyReceived;
    public bool PxeBisReplyReceived;
    public bool IcmpErrorReceived;
    public bool TftpErrorReceived;
    public bool MakeCallbacks;
    public byte TTL;
    public byte ToS;
    public EFI_IP_ADDRESS StationIp;
    public EFI_IP_ADDRESS SubnetMask;
    public EFI_PXE_BASE_CODE_PACKET DhcpDiscover;
    public EFI_PXE_BASE_CODE_PACKET DhcpAck;
    public EFI_PXE_BASE_CODE_PACKET ProxyOffer;
    public EFI_PXE_BASE_CODE_PACKET PxeDiscover;
    public EFI_PXE_BASE_CODE_PACKET PxeReply;
    public EFI_PXE_BASE_CODE_PACKET PxeBisReply;
    public EFI_PXE_BASE_CODE_IP_FILTER IpFilter;
    public uint ArpCacheEntries;
    public EFI_PXE_BASE_CODE_ARP_ENTRY ArpCache_0;
    public EFI_PXE_BASE_CODE_ARP_ENTRY ArpCache_1;
    public EFI_PXE_BASE_CODE_ARP_ENTRY ArpCache_2;
    public EFI_PXE_BASE_CODE_ARP_ENTRY ArpCache_3;
    public EFI_PXE_BASE_CODE_ARP_ENTRY ArpCache_4;
    public EFI_PXE_BASE_CODE_ARP_ENTRY ArpCache_5;
    public EFI_PXE_BASE_CODE_ARP_ENTRY ArpCache_6;
    public EFI_PXE_BASE_CODE_ARP_ENTRY ArpCache_7;
    public uint RouteTableEntries;
    public EFI_PXE_BASE_CODE_ROUTE_ENTRY RouteTable_0;
    public EFI_PXE_BASE_CODE_ROUTE_ENTRY RouteTable_1;
    public EFI_PXE_BASE_CODE_ROUTE_ENTRY RouteTable_2;
    public EFI_PXE_BASE_CODE_ROUTE_ENTRY RouteTable_3;
    public EFI_PXE_BASE_CODE_ROUTE_ENTRY RouteTable_4;
    public EFI_PXE_BASE_CODE_ROUTE_ENTRY RouteTable_5;
    public EFI_PXE_BASE_CODE_ROUTE_ENTRY RouteTable_6;
    public EFI_PXE_BASE_CODE_ROUTE_ENTRY RouteTable_7;
    public EFI_PXE_BASE_CODE_ICMP_ERROR IcmpError;
    public EFI_PXE_BASE_CODE_TFTP_ERROR TftpError;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_PROTOCOL
{
    public ulong Revision;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, bool, EFI_STATUS> Start;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, EFI_STATUS> Stop;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, bool, EFI_STATUS> Dhcp;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, ushort, ushort*, bool, EFI_PXE_BASE_CODE_DISCOVER_INFO*, EFI_STATUS> Discover;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, EFI_PXE_BASE_CODE_TFTP_OPCODE, void*, bool, ulong*, ulong*, EFI_IP_ADDRESS*, byte*, EFI_PXE_BASE_CODE_MTFTP_INFO*, bool, EFI_STATUS> Mtftp;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, ushort, EFI_IP_ADDRESS*, EFI_PXE_BASE_CODE_UDP_PORT*, EFI_IP_ADDRESS*, EFI_IP_ADDRESS*, EFI_PXE_BASE_CODE_UDP_PORT*, ulong*, void*, ulong*, void*, EFI_STATUS> UdpWrite;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, ushort, EFI_IP_ADDRESS*, EFI_PXE_BASE_CODE_UDP_PORT*, EFI_IP_ADDRESS*, EFI_PXE_BASE_CODE_UDP_PORT*, ulong*, void*, ulong*, void*, EFI_STATUS> UdpRead;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, EFI_PXE_BASE_CODE_IP_FILTER*, EFI_STATUS> SetIpFilter;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, EFI_IP_ADDRESS*, EFI_MAC_ADDRESS*, EFI_STATUS> Arp;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, bool*, bool*, byte*, byte*, bool*, EFI_STATUS> SetParameters;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, EFI_IP_ADDRESS*, EFI_IP_ADDRESS*, EFI_STATUS> SetStationIp;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_PROTOCOL*, bool*, bool*, bool*, bool*, bool*, bool*, EFI_PXE_BASE_CODE_PACKET*, EFI_PXE_BASE_CODE_PACKET*, EFI_PXE_BASE_CODE_PACKET*, EFI_PXE_BASE_CODE_PACKET*, EFI_PXE_BASE_CODE_PACKET*, EFI_PXE_BASE_CODE_PACKET*, EFI_STATUS> SetPackets;
    public EFI_PXE_BASE_CODE_MODE* Mode;
}

public enum EFI_PXE_BASE_CODE_FUNCTION
{
    EFI_PXE_BASE_CODE_FUNCTION_FIRST,
    EFI_PXE_BASE_CODE_FUNCTION_DHCP,
    EFI_PXE_BASE_CODE_FUNCTION_DISCOVER,
    EFI_PXE_BASE_CODE_FUNCTION_MTFTP,
    EFI_PXE_BASE_CODE_FUNCTION_UDP_WRITE,
    EFI_PXE_BASE_CODE_FUNCTION_UDP_READ,
    EFI_PXE_BASE_CODE_FUNCTION_ARP,
    EFI_PXE_BASE_CODE_FUNCTION_IGMP,
    EFI_PXE_BASE_CODE_PXE_FUNCTION_LAST
}

public enum EFI_PXE_BASE_CODE_CALLBACK_STATUS
{
    EFI_PXE_BASE_CODE_CALLBACK_STATUS_FIRST,
    EFI_PXE_BASE_CODE_CALLBACK_STATUS_CONTINUE,
    EFI_PXE_BASE_CODE_CALLBACK_STATUS_ABORT,
    EFI_PXE_BASE_CODE_CALLBACK_STATUS_LAST
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PXE_BASE_CODE_CALLBACK_PROTOCOL
{
    public ulong Revision;
    public readonly delegate* unmanaged<EFI_PXE_BASE_CODE_CALLBACK_PROTOCOL*, EFI_PXE_BASE_CODE_FUNCTION, bool, uint, EFI_PXE_BASE_CODE_PACKET*, EFI_STATUS> Callback;
}

