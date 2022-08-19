using System.Runtime.InteropServices;

public enum EFI_PCI_IO_PROTOCOL_WIDTH
{
    EfiPciIoWidthUint8,
    EfiPciIoWidthUint16,
    EfiPciIoWidthUint32,
    EfiPciIoWidthUint64,
    EfiPciIoWidthFifoUint8,
    EfiPciIoWidthFifoUint16,
    EfiPciIoWidthFifoUint32,
    EfiPciIoWidthFifoUint64,
    EfiPciIoWidthFillUint8,
    EfiPciIoWidthFillUint16,
    EfiPciIoWidthFillUint32,
    EfiPciIoWidthFillUint64,
    EfiPciIoWidthMaximum
}

public enum EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_WIDTH
{
    EfiPciIoWidthUint8,
    EfiPciIoWidthUint16,
    EfiPciIoWidthUint32,
    EfiPciIoWidthUint64,
    EfiPciIoWidthFifoUint8,
    EfiPciIoWidthFifoUint16,
    EfiPciIoWidthFifoUint32,
    EfiPciIoWidthFifoUint64,
    EfiPciIoWidthFillUint8,
    EfiPciIoWidthFillUint16,
    EfiPciIoWidthFillUint32,
    EfiPciIoWidthFillUint64,
    EfiPciIoWidthMaximum
}

public enum EFI_PCI_IO_PROTOCOL_OPERATION
{
    EfiPciIoOperationBusMasterRead,
    EfiPciIoOperationBusMasterWrite,
    EfiPciIoOperationBusMasterCommonBuffer,
    EfiPciIoOperationMaximum
}

public enum EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_OPERATION
{
    EfiPciOperationBusMasterRead,
    EfiPciOperationBusMasterWrite,
    EfiPciOperationBusMasterCommonBuffer,
    EfiPciOperationBusMasterRead64,
    EfiPciOperationBusMasterWrite64,
    EfiPciOperationBusMasterCommonBuffer64,
    EfiPciOperationMaximum
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PCI_IO_PROTOCOL_ACCESS
{
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_PCI_IO_PROTOCOL_WIDTH, byte, ulong, ulong, void*, EFI_STATUS> Read;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_PCI_IO_PROTOCOL_WIDTH, byte, ulong, ulong, void*, EFI_STATUS> Write;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_ACCESS
{
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_WIDTH, ulong, ulong, void*, EFI_STATUS> Read;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_WIDTH, ulong, ulong, void*, EFI_STATUS> Write;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PCI_IO_PROTOCOL_CONFIG_ACCESS
{
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_PCI_IO_PROTOCOL_WIDTH, uint, ulong, void*, EFI_STATUS> Read;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_PCI_IO_PROTOCOL_WIDTH, uint, ulong, void*, EFI_STATUS> Write;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_PCI_ADDRESS
{
    public byte Register;
    public byte Function;
    public byte Device;
    public byte Bus;
    public uint ExtendedRegister;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PCI_IO_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_PCI_IO_PROTOCOL_WIDTH, byte, ulong, ulong, ulong, ulong, ulong*, EFI_STATUS> PollMem;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_PCI_IO_PROTOCOL_WIDTH, byte, ulong, ulong, ulong, ulong, ulong*, EFI_STATUS> PollIo;
    public EFI_PCI_IO_PROTOCOL_ACCESS Mem;
    public EFI_PCI_IO_PROTOCOL_ACCESS Io;
    public EFI_PCI_IO_PROTOCOL_CONFIG_ACCESS Pci;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_PCI_IO_PROTOCOL_WIDTH, byte, ulong, byte, ulong, ulong, EFI_STATUS> CopyMem;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_PCI_IO_PROTOCOL_OPERATION, void*, ulong*, EFI_PHYSICAL_ADDRESS*, void**, EFI_STATUS> Map;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, void*, EFI_STATUS> Unmap;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_ALLOCATE_TYPE, EFI_MEMORY_TYPE, ulong, void**, ulong, EFI_STATUS> AllocateBuffer;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, ulong, void*, EFI_STATUS> FreeBuffer;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_STATUS> Flush;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, ulong*, ulong*, ulong*, ulong*, EFI_STATUS> GetLocation;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, EFI_PCI_IO_PROTOCOL_ATTRIBUTE_OPERATION, ulong, ulong*, EFI_STATUS> Attributes;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, byte, ulong*, void**, EFI_STATUS> GetBarAttributes;
    public readonly delegate* unmanaged<EFI_PCI_IO_PROTOCOL*, ulong, byte, ulong*, ulong*, EFI_STATUS> SetBarAttributes;
    public ulong RomSize;
    public void* RomImage;
}

public enum EFI_PCI_IO_PROTOCOL_ATTRIBUTE_OPERATION
{
    EfiPciIoAttributeOperationGet,
    EfiPciIoAttributeOperationSet,
    EfiPciIoAttributeOperationEnable,
    EfiPciIoAttributeOperationDisable,
    EfiPciIoAttributeOperationSupported,
    EfiPciIoAttributeOperationMaximum
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL
{
    public EFI_HANDLE ParentHandle;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_WIDTH, ulong, ulong, ulong, ulong, ulong*, EFI_STATUS> PollMem;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_WIDTH, ulong, ulong, ulong, ulong, ulong*, EFI_STATUS> PollIo;
    public EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_ACCESS Mem;
    public EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_ACCESS Io;
    public EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_ACCESS Pci;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_WIDTH, ulong, ulong, ulong, EFI_STATUS> CopyMem;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL_OPERATION, void*, ulong*, EFI_PHYSICAL_ADDRESS*, void**, EFI_STATUS> Map;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, void*, EFI_STATUS> Unmap;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, EFI_ALLOCATE_TYPE, EFI_MEMORY_TYPE, ulong, void**, ulong, EFI_STATUS> AllocateBuffer;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, ulong, void*, EFI_STATUS> FreeBuffer;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, EFI_STATUS> Flush;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, ulong*, ulong*, EFI_STATUS> GetAttributes;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, ulong, ulong*, ulong*, EFI_STATUS> SetAttributes;
    public readonly delegate* unmanaged<EFI_PCI_ROOT_BRIDGE_IO_PROTOCOL*, void**, EFI_STATUS> Configuration;
    public uint SegmentNumber;
}

