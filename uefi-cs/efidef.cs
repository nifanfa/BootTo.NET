/*++

Copyright (c) 1998  Intel Corporation

Module Name:

    efidef.h

Abstract:

    EFI definitions




Revision History

--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_STATUS
{
    ulong Value;

    public static implicit operator ulong(EFI_STATUS sts) => sts.Value;
    public static implicit operator EFI_STATUS(ulong value) => new EFI_STATUS() { Value = value };
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_LBA
{
    ulong Value;

    public static implicit operator EFI_LBA(ulong value) => new EFI_LBA() { Value = value };
    public static implicit operator ulong(EFI_LBA value) => value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TPL
{
    ulong Value;

    public static implicit operator EFI_TPL(ulong value) => new EFI_TPL() { Value = value };
    public static implicit operator ulong(EFI_TPL value) => value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_HANDLE
{
    void* Value;

    public static implicit operator EFI_HANDLE(void* value) => new EFI_HANDLE() { Value = value };
    public static implicit operator void*(EFI_HANDLE value) => value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_EVENT
{
    void* Value;

    public static implicit operator EFI_EVENT(void* value) => new EFI_EVENT() { Value = value };
    public static implicit operator void*(EFI_EVENT value) => value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_GUID
{
    public static explicit operator EFI_GUID*(EFI_GUID guid) => &guid;

    public uint Data1;
    public ushort Data2;
    public ushort Data3;
    public fixed byte Data4[8];

    public EFI_GUID(uint d1, ushort d2, ushort d3, byte d4_0, byte d4_1, byte d4_2, byte d4_3, byte d4_4, byte d4_5, byte d4_6, byte d4_7)
    {
        Data1 = d1;
        Data2 = d2;
        Data3 = d3;
        Data4[0] = d4_0;
        Data4[1] = d4_1;
        Data4[2] = d4_2;
        Data4[3] = d4_3;
        Data4[4] = d4_4;
        Data4[5] = d4_5;
        Data4[6] = d4_6;
        Data4[7] = d4_7;
    }
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_RNG_ALGORITHM
{
    public uint Data1;
    public ushort Data2;
    public ushort Data3;
    public fixed byte Data4[8];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TIME
{
    public ushort Year;       // 1998 - 20XX
    public byte Month;      // 1 - 12
    public byte Day;        // 1 - 31
    public byte Hour;       // 0 - 23
    public byte Minute;     // 0 - 59
    public byte Second;     // 0 - 59
    public byte Pad1;
    public uint Nanosecond; // 0 - 999,999,999
    public short TimeZone;   // -1440 to 1440 or 2047
    public byte Daylight;
    public byte Pad2;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IPv4_ADDRESS
{
    public fixed byte Addr[4];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IPv6_ADDRESS
{
    public fixed byte Addr[16];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_MAC_ADDRESS
{
    public fixed byte Addr[32];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_MANAGED_NETWORK_CONFIG_DATA
{
    public uint ReceivedQueueTimeoutValue;
    public uint TransmitQueueTimeoutValue;
    public ushort ProtocolTypeFilter;
    public bool EnableUnicastReceive;
    public bool EnableMulticastReceive;
    public bool EnableBroadcastReceive;
    public bool EnablePromiscuousReceive;
    public bool FlushQueuesOnReset;
    public bool EnableReceiveTimestamps;
    public bool DisableBackgroundPolling;
}

public enum EFI_ALLOCATE_TYPE
{
    AllocateAnyPages,
    AllocateMaxAddress,
    AllocateAddress,
    MaxAllocateType
}

public enum EFI_MEMORY_TYPE
{
    EfiReservedMemoryType,
    EfiLoaderCode,
    EfiLoaderData,
    EfiBootServicesCode,
    EfiBootServicesData,
    EfiRuntimeServicesCode,
    EfiRuntimeServicesData,
    EfiConventionalMemory,
    EfiUnusableMemory,
    EfiACPIReclaimMemory,
    EfiACPIMemoryNVS,
    EfiMemoryMappedIO,
    EfiMemoryMappedIOPortSpace,
    EfiPalCode,
    EfiMaxMemoryType
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PHYSICAL_ADDRESS
{
    ulong Value;

    public static implicit operator EFI_PHYSICAL_ADDRESS(void* value) => new EFI_PHYSICAL_ADDRESS() { Value = (ulong)value };
    public static implicit operator void*(EFI_PHYSICAL_ADDRESS value) => (void*)value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_VIRTUAL_ADDRESS
{
    ulong Value;

    public static implicit operator EFI_VIRTUAL_ADDRESS(void* value) => new EFI_VIRTUAL_ADDRESS() { Value = (ulong)value };
    public static implicit operator void*(EFI_VIRTUAL_ADDRESS value) => (void*)value.Value;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_MEMORY_DESCRIPTOR
{
    public uint Type;           // Field size is 32 bits followed by 32 bit pad
    public uint Pad;
    public EFI_PHYSICAL_ADDRESS PhysicalStart;  // Field size is 64 bits
    public EFI_VIRTUAL_ADDRESS VirtualStart;   // Field size is 64 bits
    public ulong NumberOfPages;  // Field size is 64 bits
    public ulong Attribute;      // Field size is 64 bits
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ISO_639_2
{
    byte Value;

    public static implicit operator ISO_639_2(byte value) => new ISO_639_2() { Value = value };
    public static implicit operator byte(ISO_639_2 value) => value.Value;
}