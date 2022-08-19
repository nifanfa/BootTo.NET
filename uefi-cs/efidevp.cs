using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DEVICE_PATH_PROTOCOL
{
    public byte Type;
    public byte SubType;
    public fixed byte Length[2];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DEVICE_PATH
{
    public byte Type;
    public byte SubType;
    public fixed byte Length[2];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PCI_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public byte Function;
    public byte Device;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct PCCARD_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public byte FunctionNumber;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MEMMAP_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint MemoryType;
    public EFI_PHYSICAL_ADDRESS StartingAddress;
    public EFI_PHYSICAL_ADDRESS EndingAddress;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct VENDOR_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public EFI_GUID Guid;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct UNKNOWN_DEVICE_VENDOR_DEVICE_PATH
{
    public VENDOR_DEVICE_PATH DevicePath;
    public byte LegacyDriveLetter;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct CONTROLLER_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint Controller;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ACPI_HID_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint HID;
    public uint UID;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EXPANDED_ACPI_HID_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint HID;
    public uint UID;
    public uint CID;
    public fixed byte HidStr[1];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ACPI_ADR_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint ADR;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ATAPI_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public byte PrimarySecondary;
    public byte SlaveMaster;
    public ushort Lun;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SCSI_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public ushort Pun;
    public ushort Lun;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FIBRECHANNEL_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint Reserved;
    public ulong WWN;
    public ulong Lun;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FIBRECHANNELEX_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint Reserved;
    public fixed byte WWN[8]; /* World Wide Name */
    public fixed byte Lun[8]; /* Logical unit, T-10 SCSI Architecture Model 4 specification */
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct F1394_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint Reserved;
    public ulong Guid;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct USB_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public byte Port;
    public byte Endpoint;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SATA_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public ushort HBAPortNumber;
    public ushort PortMultiplierPortNumber;
    public ushort Lun; /* Logical Unit Number */
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct USB_WWID_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public ushort InterfaceNumber;
    public ushort VendorId;
    public ushort ProductId;
    public fixed char SerialNumber[1]; /* UTF-16 characters of the USB serial number */
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct DEVICE_LOGICAL_UNIT_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public byte Lun; /* Logical Unit Number */
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct USB_CLASS_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public ushort VendorId;
    public ushort ProductId;
    public byte DeviceClass;
    public byte DeviceSubclass;
    public byte DeviceProtocol;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct I2O_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint Tid;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MAC_ADDR_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public EFI_MAC_ADDRESS MacAddress;
    public byte IfType;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct IPv4_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public EFI_IPv4_ADDRESS LocalIpAddress;
    public EFI_IPv4_ADDRESS RemoteIpAddress;
    public ushort LocalPort;
    public ushort RemotePort;
    public ushort Protocol;
    public bool StaticIpAddress;
    /* new from UEFI version 2, code must check Length field in Header */
    public EFI_IPv4_ADDRESS GatewayIpAddress;
    public EFI_IPv4_ADDRESS SubnetMask;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct IPv6_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public EFI_IPv6_ADDRESS LocalIpAddress;
    public EFI_IPv6_ADDRESS RemoteIpAddress;
    public ushort LocalPort;
    public ushort RemotePort;
    public ushort Protocol;
    public bool IPAddressOrigin;
    /* new from UEFI version 2, code must check Length field in Header */
    public byte PrefixLength;
    public EFI_IPv6_ADDRESS GatewayIpAddress;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct URI_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public fixed byte Uri[1];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct VLAN_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public ushort VlanId;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct INFINIBAND_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint ResourceFlags;
    public fixed byte PortGid[16];
    public ulong ServiceId;
    public ulong TargetPortId;
    public ulong DeviceId;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct UART_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint Reserved;
    public ulong BaudRate;
    public byte DataBits;
    public byte Parity;
    public byte StopBits;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct HARDDRIVE_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint PartitionNumber;
    public ulong PartitionStart;
    public ulong PartitionSize;
    public fixed byte Signature[16];
    public byte MBRType;
    public byte SignatureType;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct CDROM_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint BootEntry;
    public ulong PartitionStart;
    public ulong PartitionSize;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FILEPATH_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public fixed char PathName[1];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MEDIA_PROTOCOL_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public EFI_GUID Protocol;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MEDIA_FW_VOL_FILEPATH_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public EFI_GUID FvFileName;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MEDIA_FW_VOL_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public EFI_GUID FvName;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MEDIA_RELATIVE_OFFSET_RANGE_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public uint Reserved;
    public ulong StartingOffset;
    public ulong EndingOffset;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct BBS_BBS_DEVICE_PATH
{
    public EFI_DEVICE_PATH_PROTOCOL Header;
    public ushort DeviceType;
    public ushort StatusFlag;
    public fixed byte String[1];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DEVICE_PATH_TO_TEXT_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, bool, bool, EFI_STATUS> ConvertDeviceNodeToText;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, bool, bool, EFI_STATUS> ConvertDevicePathToText;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DEVICE_PATH_FROM_TEXT_PROTOCOL
{
    public readonly delegate* unmanaged<char*, EFI_STATUS> ConvertTextToDeviceNode;
    public readonly delegate* unmanaged<char*, EFI_STATUS> ConvertTextToDevicePath;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DEVICE_PATH_UTILITIES_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, EFI_STATUS> GetDevicePathSize;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, EFI_STATUS> DuplicateDevicePath;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, EFI_DEVICE_PATH_PROTOCOL*, EFI_STATUS> AppendDevicePath;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, EFI_DEVICE_PATH_PROTOCOL*, EFI_STATUS> AppendDeviceNode;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, EFI_DEVICE_PATH_PROTOCOL*, EFI_STATUS> AppendDevicePathInstance;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL**, ulong*, EFI_STATUS> GetNextDevicePathInstance;
    public readonly delegate* unmanaged<EFI_DEVICE_PATH_PROTOCOL*, EFI_STATUS> IsDevicePathMultiInstance;
    public readonly delegate* unmanaged<byte, byte, ushort, EFI_STATUS> CreateDeviceNode;
}

