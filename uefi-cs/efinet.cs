/*++
Copyright (c) 1999  Intel Corporation

Module Name:
    efinet.h

Abstract:
    EFI Simple Network protocol

Revision History
--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_NETWORK_STATISTICS
{
    //
    // Total number of frames received.  Includes frames with errors and
    // dropped frames.
    //
    public ulong RxTotalFrames;

    //
    // Number of valid frames received and copied into receive buffers.
    //
    public ulong RxGoodFrames;

    //
    // Number of frames below the minimum length for the media.
    // This would be <64 for ethernet.
    //
    public ulong RxUndersizeFrames;

    //
    // Number of frames longer than the maxminum length for the
    // media.  This would be >1500 for ethernet.
    //
    public ulong RxOversizeFrames;

    //
    // Valid frames that were dropped because receive buffers were full.
    //
    public ulong RxDroppedFrames;

    //
    // Number of valid unicast frames received and not dropped.
    //
    public ulong RxUnicastFrames;

    //
    // Number of valid broadcast frames received and not dropped.
    //
    public ulong RxBroadcastFrames;

    //
    // Number of valid mutlicast frames received and not dropped.
    //
    public ulong RxMulticastFrames;

    //
    // Number of frames w/ CRC or alignment errors.
    //
    public ulong RxCrcErrorFrames;

    //
    // Total number of bytes received.  Includes frames with errors
    // and dropped frames.
    //
    public ulong RxTotalBytes;

    //
    // Transmit statistics.
    //
    public ulong TxTotalFrames;
    public ulong TxGoodFrames;
    public ulong TxUndersizeFrames;
    public ulong TxOversizeFrames;
    public ulong TxDroppedFrames;
    public ulong TxUnicastFrames;
    public ulong TxBroadcastFrames;
    public ulong TxMulticastFrames;
    public ulong TxCrcErrorFrames;
    public ulong TxTotalBytes;

    //
    // Number of collisions detection on this subnet.
    //
    public ulong Collisions;

    //
    // Number of frames destined for unsupported protocol.
    //
    public ulong UnsupportedProtocol;

}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SIMPLE_NETWORK_MODE
{
    public uint State;
    public uint HwAddressSize;
    public uint MediaHeaderSize;
    public uint MaxPacketSize;
    public uint NvRamSize;
    public uint NvRamAccessSize;
    public uint ReceiveFilterMask;
    public uint ReceiveFilterSetting;
    public uint MaxMCastFilterCount;
    public uint MCastFilterCount;
    public EFI_MAC_ADDRESS MCastFilter_0;
    public EFI_MAC_ADDRESS MCastFilter_1;
    public EFI_MAC_ADDRESS MCastFilter_2;
    public EFI_MAC_ADDRESS MCastFilter_3;
    public EFI_MAC_ADDRESS MCastFilter_4;
    public EFI_MAC_ADDRESS MCastFilter_5;
    public EFI_MAC_ADDRESS MCastFilter_6;
    public EFI_MAC_ADDRESS MCastFilter_7;
    public EFI_MAC_ADDRESS MCastFilter_8;
    public EFI_MAC_ADDRESS MCastFilter_9;
    public EFI_MAC_ADDRESS MCastFilter_10;
    public EFI_MAC_ADDRESS MCastFilter_11;
    public EFI_MAC_ADDRESS MCastFilter_12;
    public EFI_MAC_ADDRESS MCastFilter_13;
    public EFI_MAC_ADDRESS MCastFilter_14;
    public EFI_MAC_ADDRESS MCastFilter_15;
    public EFI_MAC_ADDRESS CurrentAddress;
    public EFI_MAC_ADDRESS BroadcastAddress;
    public EFI_MAC_ADDRESS PermanentAddress;
    public byte IfType;
    public bool MacAddressChangeable;
    public bool MultipleTxSupported;
    public bool MediaPresentSupported;
    public bool MediaPresent;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SIMPLE_NETWORK_PROTOCOL
{
    public ulong Revision;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, EFI_STATUS> Start;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, EFI_STATUS> Stop;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, ulong, ulong, EFI_STATUS> Initialize;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, bool, EFI_STATUS> Reset;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, EFI_STATUS> Shutdown;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, uint, uint, bool, ulong, EFI_MAC_ADDRESS*, EFI_STATUS> ReceiveFilters;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, bool, EFI_MAC_ADDRESS*, EFI_STATUS> StationAddress;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, bool, ulong*, EFI_NETWORK_STATISTICS*, EFI_STATUS> Statistics;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, bool, EFI_IP_ADDRESS*, EFI_MAC_ADDRESS*, EFI_STATUS> MCastIpToMac;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, bool, ulong, ulong, void*, EFI_STATUS> NvData;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, uint*, void**, EFI_STATUS> GetStatus;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, ulong, ulong, void*, EFI_MAC_ADDRESS*, EFI_MAC_ADDRESS*, ushort*, EFI_STATUS> Transmit;
    public readonly delegate* unmanaged<EFI_SIMPLE_NETWORK_PROTOCOL*, ulong*, ulong*, void*, EFI_MAC_ADDRESS*, EFI_MAC_ADDRESS*, ushort*, EFI_STATUS> Receive;
    public EFI_EVENT WaitForPacket;
    public EFI_SIMPLE_NETWORK_MODE* Mode;
}

