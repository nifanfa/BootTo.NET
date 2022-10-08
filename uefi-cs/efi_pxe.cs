/*++
Copyright (c) Intel  1999

Module name:
    efi_pxe.h

32/64-bit PXE specification:
    alpha-4, 99-Dec-17

Abstract:
    This header file contains all of the PXE type definitions,
    structure prototypes, global variables and constants that
    are needed for porting PXE to EFI.
--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_OPCODE
{
    public ushort Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_OPFLAGS
{
    public ushort Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_STATCODE
{
    public ushort Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_STATFLAGS
{
    public ushort Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CONTROL
{
    public ushort Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_MAC_ADDR
{
    public fixed byte Value[32];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_IPV4
{
    public uint Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_IPV6
{
    public fixed uint Value[4];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_MEDIA_PROTOCOL
{
    public ushort Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_FRAME_TYPE
{
    public byte Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_HW_UNDI
{
    public uint Signature;       // PXE_ROMID_SIGNATURE
    public byte Len;          // sizeof(PXE_HW_UNDI)
    public byte Fudge;            // makes 8-bit cksum equal zero
    public byte Rev;          // PXE_ROMID_REV
    public byte IFcnt;            // physical connector count
    public byte MajorVer;         // PXE_ROMID_MAJORVER
    public byte MinorVer;         // PXE_ROMID_MINORVER
    public ushort reserved;        // zero, not used
    public uint Implementation;      // implementation flags
                                     // reserved             // vendor use
                                     // uint Status;       // status port
                                     // uint Command;      // command port
                                     // ulong CDBaddr;      // CDB address port
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_SW_UNDI
{
    public uint Signature;       // PXE_ROMID_SIGNATURE
    public byte Len;          // sizeof(PXE_SW_UNDI)
    public byte Fudge;            // makes 8-bit cksum zero
    public byte Rev;          // PXE_ROMID_REV
    public byte IFcnt;            // physical connector count
    public byte MajorVer;         // PXE_ROMID_MAJORVER
    public byte MinorVer;         // PXE_ROMID_MINORVER
    public ushort reserved1;       // zero, not used
    public uint Implementation;      // Implementation flags
    public ulong EntryPoint;      // API entry point
    public fixed byte reserved2[3];     // zero, not used
    public byte BusCnt;           // number of bustypes supported
    public fixed uint BusType[1];      // list of supported bustypes
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CDB
{
    public PXE_OPCODE OpCode;
    public PXE_OPFLAGS OpFlags;
    public ushort CPBsize;
    public ushort DBsize;
    public ulong CPBaddr;
    public ulong DBaddr;
    public PXE_STATCODE StatCode;
    public PXE_STATFLAGS StatFlags;
    public ushort IFnum;
    public PXE_CONTROL Control;
}

[StructLayout(LayoutKind.Explicit)]
public struct PXE_IP_ADDR
{
    [FieldOffset(0)]
    public PXE_IPV6 IPv6;
    [FieldOffset(0)]
    public PXE_IPV4 IPv4;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_START
{
    //
    // PXE_void Delay(ulong microseconds);
    //
    // UNDI will never request a delay smaller than 10 microseconds
    // and will always request delays in increments of 10 microseconds.
    // The Delay() CallBack routine must delay between n and n + 10
    // microseconds before returning control to the UNDI.
    //
    // This field cannot be set to zero.
    //
    public ulong Delay;

    //
    // PXE_void Block(uint enable);
    //
    // UNDI may need to block multi-threaded/multi-processor access to
    // critical code sections when programming or accessing the network
    // device.  To this end, a blocking service is needed by the UNDI.
    // When UNDI needs a block, it will call Block() passing a non-zero
    // value.  When UNDI no longer needs a block, it will call Block()
    // with a zero value.  When called, if the Block() is already enabled,
    // do not return control to the UNDI until the previous Block() is
    // disabled.
    //
    // This field cannot be set to zero.
    //
    public ulong Block;

    //
    // PXE_void Virt2Phys(ulong virtual, ulong physical_ptr);
    //
    // UNDI will pass the virtual address of a buffer and the virtual
    // address of a 64-bit physical buffer.  Convert the virtual address
    // to a physical address and write the result to the physical address
    // buffer.  If virtual and physical addresses are the same, just
    // copy the virtual address to the physical address buffer.
    //
    // This field can be set to zero if virtual and physical addresses
    // are equal.
    //
    public ulong Virt2Phys;
    //
    // PXE_void Mem_IO(byte read_write, byte len, ulong port,
    //              ulong buf_addr);
    //
    // UNDI will read or write the device io space using this call back
    // function. It passes the number of bytes as the len parameter and it
    // will be either 1,2,4 or 8.
    //
    // This field can not be set to zero.
    //
    public ulong Mem_IO;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_DB_GET_INIT_INFO
{
    //
    // Minimum length of locked memory buffer that must be given to
    // the Initialize command. Giving UNDI more memory will generally
    // give better performance.
    //
    // If MemoryRequired is zero, the UNDI does not need and will not
    // use system memory to receive and transmit packets.
    //
    public uint MemoryRequired;

    //
    // Maximum frame data length for Tx/Rx excluding the media header.
    //
    public uint FrameDataLen;

    //
    // Supported link speeds are in units of mega bits.  Common ethernet
    // values are 10, 100 and 1000.  Unused LinkSpeeds[] entries are zero
    // filled.
    //
    public fixed uint LinkSpeeds[4];

    //
    // Number of non-volatile storage items.
    //
    public uint NvCount;

    //
    // Width of non-volatile storage item in bytes.  0, 1, 2 or 4
    //
    public ushort NvWidth;

    //
    // Media header length.  This is the typical media header length for
    // this UNDI.  This information is needed when allocating receive
    // and transmit buffers.
    //
    public ushort MediaHeaderLen;

    //
    // Number of bytes in the NIC hardware (MAC) address.
    //
    public ushort HWaddrLen;

    //
    // Maximum number of multicast MAC addresses in the multicast
    // MAC address filter list.
    //
    public ushort MCastFilterCnt;

    //
    // Default number and size of transmit and receive buffers that will
    // be allocated by the UNDI.  If MemoryRequired is non-zero, this
    // allocation will come out of the memory buffer given to the Initialize
    // command.  If MemoryRequired is zero, this allocation will come out of
    // memory on the NIC.
    //
    public ushort TxBufCnt;
    public ushort TxBufSize;
    public ushort RxBufCnt;
    public ushort RxBufSize;

    //
    // Hardware interface types defined in the Assigned Numbers RFC
    // and used in DHCP and ARP packets.
    // See the PXE_IFTYPE typedef and PXE_IFTYPE_xxx macros.
    //
    public byte IFtype;

    //
    // Supported duplex.  See PXE_DUPLEX_xxxxx #defines below.
    //
    public byte Duplex;

    //
    // Supported loopback options.  See PXE_LOOPBACK_xxxxx #defines below.
    //
    public byte LoopBack;
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PXE_PCI_CONFIG_INFO
{
    //
    // This is the flag field for the PXE_DB_GET_CONFIG_INFO union.
    // For PCI bus devices, this field is set to PXE_BUSTYPE_PCI.
    //
    [FieldOffset(0)]
    public uint BusType;

    //
    // This identifies the PCI network device that this UNDI interface
    // is bound to.
    //
    [FieldOffset(4)]
    public ushort Bus;
    [FieldOffset(6)]
    public byte Device;
    [FieldOffset(7)]
    public byte Function;

    //
    // This is a copy of the PCI configuration space for this
    // network device.
    //

    [FieldOffset(8)]
    public fixed byte Config_Byte[256];
    [FieldOffset(8)]
    public fixed ushort Config_Word[128];
    [FieldOffset(8)]
    public fixed uint Config_Dword[64];
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PXE_PCC_CONFIG_INFO
{
    //
    // This is the flag field for the PXE_DB_GET_CONFIG_INFO union.
    // For PCC bus devices, this field is set to PXE_BUSTYPE_PCC.
    //
    [FieldOffset(0)]
    public uint BusType;

    //
    // This identifies the PCC network device that this UNDI interface
    // is bound to.
    //
    [FieldOffset(4)]
    public ushort Bus;
    [FieldOffset(6)]
    public byte Device;
    [FieldOffset(7)]
    public byte Function;

    //
    // This is a copy of the PCC configuration space for this
    // network device.
    //
    [FieldOffset(8)]
    public fixed byte Config_Byte[256];
    [FieldOffset(8)]
    public fixed ushort Config_Word[128];
    [FieldOffset(8)]
    public fixed uint Config_Dword[64];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_USB_CONFIG_INFO
{
    public uint BusType;
    // %%TBD What should we return here...
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_1394_CONFIG_INFO
{
    public uint BusType;
    // %%TBD What should we return here...
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_INITIALIZE
{
    //
    // Address of first (lowest) byte of the memory buffer.  This buffer must
    // be in contiguous physical memory and cannot be swapped out.  The UNDI
    // will be using this for transmit and receive buffering.
    //
    public ulong MemoryAddr;

    //
    // MemoryLength must be greater than or equal to MemoryRequired
    // returned by the Get Init Info command.
    //
    public uint MemoryLength;

    //
    // Desired link speed in Mbit/sec.  Common ethernet values are 10, 100
    // and 1000.  Setting a value of zero will auto-detect and/or use the
    // default link speed (operation depends on UNDI/NIC functionality).
    //
    public uint LinkSpeed;

    //
    // Suggested number and size of receive and transmit buffers to
    // allocate.  If MemoryAddr and MemoryLength are non-zero, this
    // allocation comes out of the supplied memory buffer.  If MemoryAddr
    // and MemoryLength are zero, this allocation comes out of memory
    // on the NIC.
    //
    // If these fields are set to zero, the UNDI will allocate buffer
    // counts and sizes as it sees fit.
    //
    public ushort TxBufCnt;
    public ushort TxBufSize;
    public ushort RxBufCnt;
    public ushort RxBufSize;

    //
    // The following configuration parameters are optional and must be zero
    // to use the default values.
    //
    public byte Duplex;

    public byte LoopBack;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_DB_INITIALIZE
{
    //
    // Actual amount of memory used from the supplied memory buffer.  This
    // may be less that the amount of memory suppllied and may be zero if
    // the UNDI and network device do not use external memory buffers.
    //
    // Memory used by the UNDI and network device is allocated from the
    // lowest memory buffer address.
    //
    public uint MemoryUsed;

    //
    // Actual number and size of receive and transmit buffers that were
    // allocated.
    //
    public ushort TxBufCnt;
    public ushort TxBufSize;
    public ushort RxBufCnt;
    public ushort RxBufSize;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_RECEIVE_FILTERS
{
    //
    // List of multicast MAC addresses.  This list, if present, will
    // replace the existing multicast MAC address filter list.
    //
    public PXE_MAC_ADDR MCastList_0;
    public PXE_MAC_ADDR MCastList_1;
    public PXE_MAC_ADDR MCastList_2;
    public PXE_MAC_ADDR MCastList_3;
    public PXE_MAC_ADDR MCastList_4;
    public PXE_MAC_ADDR MCastList_5;
    public PXE_MAC_ADDR MCastList_6;
    public PXE_MAC_ADDR MCastList_7;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_DB_RECEIVE_FILTERS
{
    //
    // Filtered multicast MAC address list.
    //
    public PXE_MAC_ADDR MCastList_0;
    public PXE_MAC_ADDR MCastList_1;
    public PXE_MAC_ADDR MCastList_2;
    public PXE_MAC_ADDR MCastList_3;
    public PXE_MAC_ADDR MCastList_4;
    public PXE_MAC_ADDR MCastList_5;
    public PXE_MAC_ADDR MCastList_6;
    public PXE_MAC_ADDR MCastList_7;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_STATION_ADDRESS
{
    //
    // If supplied and supported, the current station MAC address
    // will be changed.
    //
    public PXE_MAC_ADDR StationAddr;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_DB_STATION_ADDRESS
{
    //
    // Current station MAC address.
    //
    public PXE_MAC_ADDR StationAddr;

    //
    // Station broadcast MAC address.
    //
    public PXE_MAC_ADDR BroadcastAddr;

    //
    // Permanent station MAC address.
    //
    public PXE_MAC_ADDR PermanentAddr;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_DB_STATISTICS
{
    //
    // Bit field identifying what statistic data is collected by the
    // UNDI/NIC.
    // If bit 0x00 is set, Data[0x00] is collected.
    // If bit 0x01 is set, Data[0x01] is collected.
    // If bit 0x20 is set, Data[0x20] is collected.
    // If bit 0x21 is set, Data[0x21] is collected.
    // Etc.
    //
    public ulong Supported;

    //
    // Statistic data.
    //
    public fixed ulong Data[64];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_MCAST_IP_TO_MAC
{
    //
    // Multicast IP address to be converted to multicast MAC address.
    //
    public PXE_IP_ADDR IP;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_DB_MCAST_IP_TO_MAC
{
    //
    // Multicast MAC address.
    //
    public PXE_MAC_ADDR MAC;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_NVDATA_SPARSE
{
    //
    // NvData item list.  Only items in this list will be updated.
    //
    //  Non-volatile storage address to be changed.
    public uint Addr;

    // Data item to write into above storage address.

    public fixed uint Item[128];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_DB_NVDATA
{
    public fixed uint Data[128];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_DB_GET_STATUS
{
    //
    // Length of next receive frame (header + data).  If this is zero,
    // there is no next receive frame available.
    //
    public uint RxFrameLen;

    //
    // Reserved, set to zero.
    //
    public uint reserved;

    //
    //  Addresses of transmitted buffers that need to be recycled.
    //
    public fixed ulong TxBuffer[32];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_FILL_HEADER
{
    //
    // Source and destination MAC addresses.  These will be copied into
    // the media header without doing byte swapping.
    //
    public PXE_MAC_ADDR SrcAddr;
    public PXE_MAC_ADDR DestAddr;

    //
    // Address of first byte of media header.  The first byte of packet data
    // follows the last byte of the media header.
    //
    public ulong MediaHeader;

    //
    // Length of packet data in bytes (not including the media header).
    //
    public uint PacketLen;

    //
    // Protocol type.  This will be copied into the media header without
    // doing byte swapping.  Protocol type numbers can be obtained from
    // the Assigned Numbers RFC 1700.
    //
    public ushort Protocol;

    //
    // Length of the media header in bytes.
    //
    public ushort MediaHeaderLen;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_FILL_HEADER_FRAGMENTED
{
    //
    // Source and destination MAC addresses.  These will be copied into
    // the media header without doing byte swapping.
    //
    public PXE_MAC_ADDR SrcAddr;
    public PXE_MAC_ADDR DestAddr;

    //
    // Length of packet data in bytes (not including the media header).
    //
    public uint PacketLen;

    //
    // Protocol type.  This will be copied into the media header without
    // doing byte swapping.  Protocol type numbers can be obtained from
    // the Assigned Numbers RFC 1700.
    //
    public PXE_MEDIA_PROTOCOL Protocol;

    //
    // Length of the media header in bytes.
    //
    public ushort MediaHeaderLen;

    //
    // Number of packet fragment descriptors.
    //
    public ushort FragCnt;

    //
    // Reserved, must be set to zero.
    //
    public ushort reserved;

    //
    // Array of packet fragment descriptors.  The first byte of the media
    // header is the first byte of the first fragment.
    //
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_0;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_1;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_2;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_3;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_4;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_5;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_6;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_7;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_8;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_9;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_10;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_11;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_12;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_13;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_14;
    public PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC FragDesc_15;
}


[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_FILL_HEADER_FRAGMENTED_FRAG_DESC
{
    //
    // Address of this packet fragment.
    //
    public ulong FragAddr;

    //
    // Length of this packet fragment.
    //
    public uint FragLen;

    //
    // Reserved, must be set to zero.
    //
    public uint reserved;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_TRANSMIT
{
    //
    // Address of first byte of frame buffer.  This is also the first byte
    // of the media header.
    //
    public ulong FrameAddr;

    //
    // Length of the data portion of the frame buffer in bytes.  Do not
    // include the length of the media header.
    //
    public uint DataLen;

    //
    // Length of the media header in bytes.
    //
    public ushort MediaheaderLen;

    //
    // Reserved, must be zero.
    //
    public ushort reserved;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_TRANSMIT_FRAGMENTS
{
    //
    // Length of packet data in bytes (not including the media header).
    //
    public uint FrameLen;

    //
    // Length of the media header in bytes.
    //
    public ushort MediaheaderLen;

    //
    // Number of packet fragment descriptors.
    //
    public ushort FragCnt;

    //
    // Array of frame fragment descriptors.  The first byte of the first
    // fragment is also the first byte of the media header.
    //
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_0;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_1;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_2;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_3;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_4;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_5;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_6;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_7;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_8;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_9;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_10;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_11;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_12;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_13;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_14;
    public PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC FragDesc_15;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_TRANSMIT_FRAGMENTS_FRAG_DESC
{
    //
    // Address of this frame fragment.
    //
    public ulong FragAddr;

    //
    // Length of this frame fragment.
    //
    public uint FragLen;

    //
    // Reserved, must be set to zero.
    //
    public uint reserved;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_CPB_RECEIVE
{
    //
    // Address of first byte of receive buffer.  This is also the first byte
    // of the frame header.
    //
    public ulong BufferAddr;

    //
    // Length of receive buffer.  This must be large enough to hold the
    // received frame (media header + data).  If the length of smaller than
    // the received frame, data will be lost.
    //
    public uint BufferLen;

    //
    // Reserved, must be set to zero.
    //
    public uint reserved;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PXE_DB_RECEIVE
{
    //
    // Source and destination MAC addresses from media header.
    //
    public PXE_MAC_ADDR SrcAddr;
    public PXE_MAC_ADDR DestAddr;

    //
    // Length of received frame.  May be larger than receive buffer size.
    // The receive buffer will not be overwritten.  This is how to tell
    // if data was lost because the receive buffer was too small.
    //
    public uint FrameLen;

    //
    // Protocol type from media header.
    //
    public PXE_MEDIA_PROTOCOL Protocol;

    //
    // Length of media header in received frame.
    //
    public ushort MediaHeaderLen;

    //
    // Type of receive frame.
    //
    public PXE_FRAME_TYPE Type;

    //
    // Reserved, must be zero.
    //
    public fixed byte reserved[7];

}

