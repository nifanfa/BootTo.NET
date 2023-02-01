/*++

Copyright (c) 1998  Intel Corporation

Module Name:

    efiprot.h

Abstract:

    EFI Protocols



Revision History

--*/

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_BLOCK_IO_MEDIA
{
    public uint MediaId;
    public bool RemovableMedia;
    public bool MediaPresent;

    public bool LogicalPartition;
    public bool ReadOnly;
    public bool WriteCaching;

    public uint BlockSize;
    public uint IoAlign;

    public EFI_LBA LastBlock;

    /* revision 2 */
    public EFI_LBA LowestAlignedLba;
    public uint LogicalBlocksPerPhysicalBlock;
    /* revision 3 */
    public uint OptimalTransferLengthGranularity;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_BLOCK_IO_PROTOCOL
{
    public ulong Revision;

    public EFI_BLOCK_IO_MEDIA* Media;

    public readonly delegate* unmanaged<EFI_BLOCK_IO_PROTOCOL*, bool, EFI_STATUS> Reset;
    public readonly delegate* unmanaged<EFI_BLOCK_IO_PROTOCOL*, uint, EFI_LBA, ulong, void*, EFI_STATUS> ReadBlocks;
    public readonly delegate* unmanaged<EFI_BLOCK_IO_PROTOCOL*, uint, EFI_LBA, ulong, void*, EFI_STATUS> WriteBlocks;
    public readonly delegate* unmanaged<EFI_BLOCK_IO_PROTOCOL*, EFI_STATUS> FlushBlocks;

}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_BLOCK_IO2_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS TransactionStatus;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_BLOCK_IO2_PROTOCOL
{
    public EFI_BLOCK_IO_MEDIA* Media;
    public readonly delegate* unmanaged<EFI_BLOCK_IO2_PROTOCOL*, bool, EFI_STATUS> Reset;
    public readonly delegate* unmanaged<EFI_BLOCK_IO2_PROTOCOL*, uint, EFI_LBA, EFI_BLOCK_IO2_TOKEN*, ulong, void*, EFI_STATUS> ReadBlocksEx;
    public readonly delegate* unmanaged<EFI_BLOCK_IO2_PROTOCOL*, uint, EFI_LBA, EFI_BLOCK_IO2_TOKEN*, ulong, void*, EFI_STATUS> WriteBlocksEx;
    public readonly delegate* unmanaged<EFI_BLOCK_IO2_PROTOCOL*, EFI_BLOCK_IO2_TOKEN*, EFI_STATUS> FlushBlocksEx;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DISK_IO_PROTOCOL
{
    public ulong Revision;
    public readonly delegate* unmanaged<EFI_DISK_IO_PROTOCOL*, uint, ulong, ulong, void*, EFI_STATUS> ReadDisk;
    public readonly delegate* unmanaged<EFI_DISK_IO_PROTOCOL*, uint, ulong, ulong, void*, EFI_STATUS> WriteDisk;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DISK_IO2_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS TransactionStatus;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DISK_IO2_PROTOCOL
{
    public ulong Revision;
    public readonly delegate* unmanaged<EFI_DISK_IO2_PROTOCOL*, EFI_STATUS> Cancel;
    public readonly delegate* unmanaged<EFI_DISK_IO2_PROTOCOL*, uint, ulong, EFI_DISK_IO2_TOKEN*, ulong, void*, EFI_STATUS> ReadDiskEx;
    public readonly delegate* unmanaged<EFI_DISK_IO2_PROTOCOL*, uint, ulong, EFI_DISK_IO2_TOKEN*, ulong, void*, EFI_STATUS> WriteDiskEx;
    public readonly delegate* unmanaged<EFI_DISK_IO2_PROTOCOL*, EFI_DISK_IO2_TOKEN*, EFI_STATUS> FlushDiskEx;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SIMPLE_FILE_SYSTEM_PROTOCOL
{
    public ulong Revision;
    public readonly delegate* unmanaged<EFI_SIMPLE_FILE_SYSTEM_PROTOCOL*, EFI_FILE_HANDLE**, EFI_STATUS> OpenVolume;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FILE_IO_TOKEN
{
    public EFI_EVENT Event;
    public EFI_STATUS Status;
    public ulong BufferSize;
    public void* Buffer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FILE_PROTOCOL
{
    public ulong Revision;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_HANDLE**, char*, ulong, ulong, EFI_STATUS> Open;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_STATUS> Close;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_STATUS> Delete;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong*, void*, EFI_STATUS> Read;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong*, void*, EFI_STATUS> Write;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong*, EFI_STATUS> GetPosition;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong, EFI_STATUS> SetPosition;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_GUID*, ulong*, void*, EFI_STATUS> GetInfo;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_GUID*, ulong, void*, EFI_STATUS> SetInfo;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_STATUS> Flush;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_HANDLE**, char*, ulong, ulong, EFI_FILE_IO_TOKEN*, EFI_STATUS> OpenEx;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_IO_TOKEN*, EFI_STATUS> ReadEx;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_IO_TOKEN*, EFI_STATUS> WriteEx;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_IO_TOKEN*, EFI_STATUS> FlushEx;
}

[StructLayout(LayoutKind.Sequential)]
//*EFI_FILE_HANDLE
public unsafe struct EFI_FILE_HANDLE
{
    public ulong Revision;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_HANDLE**, char*, ulong, ulong, EFI_STATUS> Open;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_STATUS> Close;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_STATUS> Delete;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong*, void*, EFI_STATUS> Read;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong*, void*, EFI_STATUS> Write;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong*, EFI_STATUS> GetPosition;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong, EFI_STATUS> SetPosition;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_GUID*, ulong*, void*, EFI_STATUS> GetInfo;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_GUID*, ulong, void*, EFI_STATUS> SetInfo;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_STATUS> Flush;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_HANDLE**, char*, ulong, ulong, EFI_FILE_IO_TOKEN*, EFI_STATUS> OpenEx;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_IO_TOKEN*, EFI_STATUS> ReadEx;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_IO_TOKEN*, EFI_STATUS> WriteEx;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_IO_TOKEN*, EFI_STATUS> FlushEx;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FILE
{
    public ulong Revision;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_HANDLE**, char*, ulong, ulong, EFI_STATUS> Open;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_STATUS> Close;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_STATUS> Delete;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong*, void*, EFI_STATUS> Read;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong*, void*, EFI_STATUS> Write;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong*, EFI_STATUS> GetPosition;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, ulong, EFI_STATUS> SetPosition;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_GUID*, ulong*, void*, EFI_STATUS> GetInfo;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_GUID*, ulong, void*, EFI_STATUS> SetInfo;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_STATUS> Flush;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_HANDLE**, char*, ulong, ulong, EFI_FILE_IO_TOKEN*, EFI_STATUS> OpenEx;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_IO_TOKEN*, EFI_STATUS> ReadEx;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_IO_TOKEN*, EFI_STATUS> WriteEx;
    public readonly delegate* unmanaged<EFI_FILE_HANDLE*, EFI_FILE_IO_TOKEN*, EFI_STATUS> FlushEx;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FILE_INFO
{
    public ulong Size;
    public ulong FileSize;
    public ulong PhysicalSize;
    public EFI_TIME CreateTime;
    public EFI_TIME LastAccessTime;
    public EFI_TIME ModificationTime;
    public ulong Attribute;
    public fixed char FileName[128];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FILE_SYSTEM_INFO
{
    public ulong Size;
    public bool ReadOnly;
    public ulong VolumeSize;
    public ulong FreeSpace;
    public uint BlockSize;
    public fixed char VolumeLabel[1];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FILE_SYSTEM_VOLUME_LABEL
{
    public fixed char VolumeLabel[1];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_LOAD_FILE_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_LOAD_FILE_PROTOCOL*, EFI_DEVICE_PATH*, bool, ulong*, void*, EFI_STATUS> LoadFile;
}

public enum EFI_IO_WIDTH
{
    IO_UINT8,
    IO_UINT16,
    IO_UINT32,
    IO_UINT64,
    //
    // Specification Change: Copy from MMIO to MMIO vs. MMIO to buffer, buffer to MMIO
    //
    MMIO_COPY_UINT8,
    MMIO_COPY_UINT16,
    MMIO_COPY_UINT32,
    MMIO_COPY_UINT64
}

public enum EFI_IO_OPERATION_TYPE
{
    EfiBusMasterRead,
    EfiBusMasterWrite,
    EfiBusMasterCommonBuffer
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_IO_ACCESS
{
    public readonly delegate* unmanaged<EFI_DEVICE_IO_PROTOCOL*, EFI_IO_WIDTH, ulong, ulong, void*, EFI_STATUS> Read;
    public readonly delegate* unmanaged<EFI_DEVICE_IO_PROTOCOL*, EFI_IO_WIDTH, ulong, ulong, void*, EFI_STATUS> Write;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DEVICE_IO_PROTOCOL
{
    public EFI_IO_ACCESS Mem;
    public EFI_IO_ACCESS Io;
    public EFI_IO_ACCESS Pci;
    public readonly delegate* unmanaged<EFI_DEVICE_IO_PROTOCOL*, EFI_IO_OPERATION_TYPE, EFI_PHYSICAL_ADDRESS*, ulong*, EFI_PHYSICAL_ADDRESS*, void**, EFI_STATUS> Map;
    public readonly delegate* unmanaged<EFI_DEVICE_IO_PROTOCOL*, ulong, EFI_DEVICE_PATH**, EFI_STATUS> PciDevicePath;
    public readonly delegate* unmanaged<EFI_DEVICE_IO_PROTOCOL*, void*, EFI_STATUS> Unmap;
    public readonly delegate* unmanaged<EFI_DEVICE_IO_PROTOCOL*, EFI_ALLOCATE_TYPE, EFI_MEMORY_TYPE, ulong, EFI_PHYSICAL_ADDRESS*, EFI_STATUS> AllocateBuffer;
    public readonly delegate* unmanaged<EFI_DEVICE_IO_PROTOCOL*, EFI_STATUS> Flush;
    public readonly delegate* unmanaged<EFI_DEVICE_IO_PROTOCOL*, ulong, EFI_PHYSICAL_ADDRESS, EFI_STATUS> FreeBuffer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_HASH_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_HASH_PROTOCOL*, EFI_GUID*, EFI_STATUS> GetHashSize;
    public readonly delegate* unmanaged<EFI_HASH_PROTOCOL*, EFI_GUID*, bool, byte*, ulong, EFI_STATUS> Hash;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UNICODE_COLLATION_PROTOCOL
{

    // general
    public readonly delegate* unmanaged<EFI_UNICODE_COLLATION_PROTOCOL*, char*, char*, EFI_STATUS> StriColl;
    public readonly delegate* unmanaged<EFI_UNICODE_COLLATION_PROTOCOL*, char*, char*, EFI_STATUS> MetaiMatch;
    public readonly delegate* unmanaged<EFI_UNICODE_COLLATION_PROTOCOL*, char*, EFI_STATUS> StrLwr;
    public readonly delegate* unmanaged<EFI_UNICODE_COLLATION_PROTOCOL*, char*, EFI_STATUS> StrUpr;

    // for supporting fat volumes
    public readonly delegate* unmanaged<EFI_UNICODE_COLLATION_PROTOCOL*, ulong, byte*, char*, EFI_STATUS> FatToStr;
    public readonly delegate* unmanaged<EFI_UNICODE_COLLATION_PROTOCOL*, char*, ulong, byte*, EFI_STATUS> StrToFat;

    public byte* SupportedLanguages;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PIXEL_BITMASK
{
    public uint RedMask;
    public uint GreenMask;
    public uint BlueMask;
    public uint ReservedMask;
}

public enum EFI_GRAPHICS_PIXEL_FORMAT
{
    PixelRedGreenBlueReserved8BitPerColor,
    PixelBlueGreenRedReserved8BitPerColor,
    PixelBitMask,
    PixelBltOnly,
    PixelFormatMax
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_GRAPHICS_OUTPUT_MODE_INFORMATION
{
    public uint Version;
    public uint HorizontalResolution;
    public uint VerticalResolution;
    public EFI_GRAPHICS_PIXEL_FORMAT PixelFormat;
    public EFI_PIXEL_BITMASK PixelInformation;
    public uint PixelsPerScanLine;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_GRAPHICS_OUTPUT_BLT_PIXEL
{
    public byte Blue;
    public byte Green;
    public byte Red;
    public byte Reserved;
}

public enum EFI_GRAPHICS_OUTPUT_BLT_OPERATION
{
    EfiBltVideoFill,
    EfiBltVideoToBltBuffer,
    EfiBltBufferToVideo,
    EfiBltVideoToVideo,
    EfiGraphicsOutputBltOperationMax
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_GRAPHICS_OUTPUT_PROTOCOL_MODE
{
    public uint MaxMode;
    public uint Mode;
    public EFI_GRAPHICS_OUTPUT_MODE_INFORMATION* Info;
    public ulong SizeOfInfo;
    public EFI_PHYSICAL_ADDRESS FrameBufferBase;
    public ulong FrameBufferSize;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_GRAPHICS_OUTPUT_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_GRAPHICS_OUTPUT_PROTOCOL*, uint, ulong*, EFI_GRAPHICS_OUTPUT_MODE_INFORMATION**, EFI_STATUS> QueryMode;
    public readonly delegate* unmanaged<EFI_GRAPHICS_OUTPUT_PROTOCOL*, uint, EFI_STATUS> SetMode;
    public readonly delegate* unmanaged<EFI_GRAPHICS_OUTPUT_PROTOCOL*, EFI_GRAPHICS_OUTPUT_BLT_PIXEL*, EFI_GRAPHICS_OUTPUT_BLT_OPERATION, ulong, ulong, ulong, ulong, ulong, ulong, ulong, EFI_STATUS> Blt;
    public EFI_GRAPHICS_OUTPUT_PROTOCOL_MODE* Mode;
}


[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_EDID_DISCOVERED_PROTOCOL
{
    public uint SizeOfEdid;
    public byte* Edid;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_EDID_ACTIVE_PROTOCOL
{
    public uint SizeOfEdid;
    public byte* Edid;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_EDID_OVERRIDE_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_EDID_OVERRIDE_PROTOCOL*, EFI_HANDLE*, uint*, ulong*, EFI_STATUS> GetEdid;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SERVICE_BINDING
{
    public readonly delegate* unmanaged<EFI_SERVICE_BINDING*, EFI_HANDLE*, EFI_STATUS> CreateChild;
    public readonly delegate* unmanaged<EFI_SERVICE_BINDING*, EFI_HANDLE, EFI_STATUS> DestroyChild;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DRIVER_BINDING_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_DRIVER_BINDING_PROTOCOL*, EFI_HANDLE, EFI_STATUS> Supported;
    public readonly delegate* unmanaged<EFI_DRIVER_BINDING_PROTOCOL*, EFI_HANDLE, EFI_STATUS> Start;
    public readonly delegate* unmanaged<EFI_DRIVER_BINDING_PROTOCOL*, EFI_HANDLE, ulong, EFI_STATUS> Stop;
    public uint Version;
    public EFI_HANDLE ImageHandle;
    public EFI_HANDLE DriverBindingHandle;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_COMPONENT_NAME_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_COMPONENT_NAME_PROTOCOL*, byte*, EFI_STATUS> GetDriverName;
    public readonly delegate* unmanaged<EFI_COMPONENT_NAME_PROTOCOL*, EFI_HANDLE, EFI_HANDLE, byte*, EFI_STATUS> GetControllerName;
    public byte* SupportedLanguages;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_COMPONENT_NAME2_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_COMPONENT_NAME2_PROTOCOL*, byte*, EFI_STATUS> GetDriverName;
    public readonly delegate* unmanaged<EFI_COMPONENT_NAME2_PROTOCOL*, EFI_HANDLE, EFI_HANDLE, byte*, EFI_STATUS> GetControllerName;
    public byte* SupportedLanguages;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_LOADED_IMAGE_PROTOCOL
{
    public uint Revision;
    public EFI_HANDLE ParentHandle;
    public EFI_SYSTEM_TABLE* SystemTable;

    // Source location of image
    public EFI_HANDLE DeviceHandle;
    public EFI_DEVICE_PATH* FilePath;
    public void* Reserved;

    // Images load options
    public uint LoadOptionsSize;
    public void* LoadOptions;

    // Location of where image was loaded
    public void* ImageBase;
    public ulong ImageSize;
    public EFI_MEMORY_TYPE ImageCodeType;
    public EFI_MEMORY_TYPE ImageDataType;

    // If the driver image supports a dynamic unload request
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_STATUS> Unload;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_RNG_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_RNG_PROTOCOL*, ulong*, EFI_RNG_ALGORITHM*, EFI_STATUS> GetInfo;
    public readonly delegate* unmanaged<EFI_RNG_PROTOCOL*, EFI_RNG_ALGORITHM*, ulong, byte*, EFI_STATUS> GetRNG;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PLATFORM_DRIVER_OVERRIDE_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_PLATFORM_DRIVER_OVERRIDE_PROTOCOL*, EFI_HANDLE, EFI_STATUS> GetDriver;
    public readonly delegate* unmanaged<EFI_PLATFORM_DRIVER_OVERRIDE_PROTOCOL*, EFI_HANDLE, EFI_STATUS> GetDriverPath;
    public readonly delegate* unmanaged<EFI_PLATFORM_DRIVER_OVERRIDE_PROTOCOL*, EFI_HANDLE, EFI_DEVICE_PATH*, EFI_STATUS> DriverLoaded;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_BUS_SPECIFIC_DRIVER_OVERRIDE_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_BUS_SPECIFIC_DRIVER_OVERRIDE_PROTOCOL*, EFI_STATUS> GetDriver;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_DRIVER_FAMILY_OVERRIDE_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_DRIVER_FAMILY_OVERRIDE_PROTOCOL*, uint> GetVersion;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_EBC_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_EBC_PROTOCOL*, EFI_HANDLE, void*, EFI_STATUS> CreateThunk;
    public readonly delegate* unmanaged<EFI_EBC_PROTOCOL*, EFI_STATUS> UnloadImage;
    public readonly delegate* unmanaged<EFI_EBC_PROTOCOL*, EFI_STATUS> RegisterICacheFlush;
    public readonly delegate* unmanaged<EFI_EBC_PROTOCOL*, EFI_STATUS> GetVersion;
}

