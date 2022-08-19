using System.Runtime.InteropServices;

public partial class efi
{
    public const uint EVT_NOTIFY_WAIT = 0x00000100;
    public const uint EVT_NOTIFY_SIGNAL = 0x00000200;

    public const uint TPL_APPLICATION = 4;
    public const uint TPL_CALLBACK = 8;
    public const uint TPL_NOTIFY = 16;
    public const uint TPL_HIGH_LEVEL = 31;
}

public enum EFI_TIMER_DELAY
{
    TimerCancel,
    TimerPeriodic,
    TimerRelative,
    TimerTypeMax
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TIME_CAPABILITIES
{
    public uint Resolution;     // 1e-6 parts per million
    public uint Accuracy;       // hertz
    public bool SetsToZero;     // Set clears sub-second time
}

/*
[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_LOADED_IMAGE
{
    public uint Revision;
    public EFI_HANDLE ParentHandle;
    public _EFI_SYSTEM_TABLE* SystemTable;

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
    public EFI_IMAGE_UNLOAD Unload;

}
*/

public partial class efi
{
    public const uint EFI_OPEN_PROTOCOL_BY_HANDLE_PROTOCOL = 0x00000001;
    public const uint EFI_OPEN_PROTOCOL_GET_PROTOCOL = 0x00000002;
    public const uint EFI_OPEN_PROTOCOL_TEST_PROTOCOL = 0x00000004;
    public const uint EFI_OPEN_PROTOCOL_BY_CHILD_CONTROLLER = 0x00000008;
    public const uint EFI_OPEN_PROTOCOL_BY_DRIVER = 0x00000010;
    public const uint EFI_OPEN_PROTOCOL_EXCLUSIVE = 0x00000020;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_OPEN_PROTOCOL_INFORMATION_ENTRY
{
    public EFI_HANDLE AgentHandle;
    public EFI_HANDLE ControllerHandle;
    public uint Attributes;
    public uint OpenCount;
}

public enum EFI_LOCATE_SEARCH_TYPE
{
    AllHandles,
    ByRegisterNotify,
    ByProtocol
}

public enum EFI_RESET_TYPE
{
    EfiResetCold,
    EfiResetWarm,
    EfiResetShutdown
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct EFI_CAPSULE_BLOCK_DESCRIPTOR
{
    [FieldOffset(0)]
    public ulong Length;
    [FieldOffset(8)]
    public EFI_PHYSICAL_ADDRESS DataBlock;
    [FieldOffset(8)]
    public EFI_PHYSICAL_ADDRESS ContinuationPointer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_CAPSULE_HEADER
{
    public EFI_GUID CapsuleGuid;
    public uint HeaderSize;
    public uint Flags;
    public uint CapsuleImageSize;
}

public enum EFI_INTERFACE_TYPE
{
    EFI_NATIVE_INTERFACE,
    EFI_PCODE_INTERFACE
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_TABLE_HEADER
{
    public ulong Signature;
    public uint Revision;
    public uint HeaderSize;
    public uint CRC32;
    public uint Reserved;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_RUNTIME_SERVICES
{
    public EFI_TABLE_HEADER Hdr;

    //
    // Time services
    //

    public readonly delegate* unmanaged<EFI_TIME*, EFI_TIME_CAPABILITIES*, EFI_STATUS> GetTime;
    public readonly delegate* unmanaged<EFI_TIME*, EFI_STATUS> SetTime;
    public readonly delegate* unmanaged<bool*, bool*, EFI_TIME*, EFI_STATUS> GetWakeupTime;
    public readonly delegate* unmanaged<bool, EFI_TIME*, EFI_STATUS> SetWakeupTime;

    //
    // Virtual memory services
    //

    public readonly delegate* unmanaged<ulong, ulong, uint, EFI_MEMORY_DESCRIPTOR*, EFI_STATUS> SetVirtualAddressMap;
    public readonly delegate* unmanaged<ulong, void**, EFI_STATUS> ConvertPointer;

    //
    // Variable serviers
    //

    public readonly delegate* unmanaged<char*, EFI_GUID*, uint*, ulong*, void*, EFI_STATUS> GetVariable;
    public readonly delegate* unmanaged<ulong*, char*, EFI_GUID*, EFI_STATUS> GetNextVariableName;
    public readonly delegate* unmanaged<char*, EFI_GUID*, uint, ulong, void*, EFI_STATUS> SetVariable;

    //
    // Misc
    //

    public readonly delegate* unmanaged<uint*, EFI_STATUS> GetNextHighMonotonicCount;
    public readonly delegate* unmanaged<EFI_RESET_TYPE, EFI_STATUS, ulong, char*, EFI_STATUS> ResetSystem;

    public readonly delegate* unmanaged<EFI_CAPSULE_HEADER**, ulong, EFI_PHYSICAL_ADDRESS, EFI_STATUS> UpdateCapsule;
    public readonly delegate* unmanaged<EFI_CAPSULE_HEADER**, ulong, ulong*, EFI_RESET_TYPE*, EFI_STATUS> QueryCapsuleCapabilities;
    public readonly delegate* unmanaged<uint, ulong*, ulong*, ulong*, EFI_STATUS> QueryVariableInfo;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_BOOT_SERVICES
{

    public EFI_TABLE_HEADER Hdr;

    //
    // Task priority functions
    //

    public readonly delegate* unmanaged<EFI_TPL, EFI_STATUS> RaiseTPL;
    public readonly delegate* unmanaged<EFI_TPL, EFI_STATUS> RestoreTPL;

    //
    // Memory functions
    //

    public readonly delegate* unmanaged<EFI_ALLOCATE_TYPE, EFI_MEMORY_TYPE, ulong, EFI_PHYSICAL_ADDRESS*, EFI_STATUS> AllocatePages;
    public readonly delegate* unmanaged<EFI_PHYSICAL_ADDRESS, ulong, EFI_STATUS> FreePages;
    public readonly delegate* unmanaged<ulong*, EFI_MEMORY_DESCRIPTOR*, ulong*, ulong*, uint*, EFI_STATUS> GetMemoryMap;
    public readonly delegate* unmanaged<EFI_MEMORY_TYPE, ulong, void**, EFI_STATUS> AllocatePool;
    public readonly delegate* unmanaged<void*, EFI_STATUS> FreePool;

    //
    // Event & timer functions
    //

    public readonly delegate* unmanaged<uint, EFI_TPL, delegate* unmanaged<EFI_EVENT, void*, void>, void*, EFI_EVENT*, EFI_STATUS> CreateEvent;
    public readonly delegate* unmanaged<EFI_EVENT, EFI_TIMER_DELAY, ulong, EFI_STATUS> SetTimer;
    public readonly delegate* unmanaged<ulong, EFI_EVENT*, ulong*, EFI_STATUS> WaitForEvent;
    public readonly delegate* unmanaged<EFI_EVENT, EFI_STATUS> SignalEvent;
    public readonly delegate* unmanaged<EFI_EVENT, EFI_STATUS> CloseEvent;
    public readonly delegate* unmanaged<EFI_EVENT, EFI_STATUS> CheckEvent;

    //
    // Protocol handler functions
    //

    public readonly delegate* unmanaged<EFI_HANDLE*, EFI_GUID*, EFI_INTERFACE_TYPE, void*, EFI_STATUS> InstallProtocolInterface;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_GUID*, void*, void*, EFI_STATUS> ReinstallProtocolInterface;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_GUID*, void*, EFI_STATUS> UninstallProtocolInterface;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_GUID*, void**, EFI_STATUS> HandleProtocol;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_GUID*, void**, EFI_STATUS> PCHandleProtocol;
    public readonly delegate* unmanaged<EFI_GUID*, EFI_EVENT, void**, EFI_STATUS> RegisterProtocolNotify;
    public readonly delegate* unmanaged<EFI_LOCATE_SEARCH_TYPE, EFI_GUID*, void*, ulong*, EFI_HANDLE*, EFI_STATUS> LocateHandle;
    public readonly delegate* unmanaged<EFI_GUID*, EFI_DEVICE_PATH**, EFI_HANDLE*, EFI_STATUS> LocateDevicePath;
    public readonly delegate* unmanaged<EFI_GUID*, void*, EFI_STATUS> InstallConfigurationTable;

    //
    // Image functions
    //

    public readonly delegate* unmanaged<bool, EFI_HANDLE, EFI_DEVICE_PATH*, void*, ulong, EFI_HANDLE*, EFI_STATUS> LoadImage;
    public readonly delegate* unmanaged<EFI_HANDLE, ulong*, char**, EFI_STATUS> StartImage;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_STATUS, ulong, char*, EFI_STATUS> Exit;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_STATUS> UnloadImage;
    public readonly delegate* unmanaged<EFI_HANDLE, ulong, EFI_STATUS> ExitBootServices;

    //
    // Misc functions
    //

    public readonly delegate* unmanaged<ulong*, EFI_STATUS> GetNextMonotonicCount;
    public readonly delegate* unmanaged<ulong, EFI_STATUS> Stall;
    public readonly delegate* unmanaged<ulong, ulong, ulong, char*, EFI_STATUS> SetWatchdogTimer;

    //
    // DriverSupport Services
    //

    public readonly delegate* unmanaged<EFI_HANDLE, EFI_HANDLE*, EFI_DEVICE_PATH*, bool, EFI_STATUS> ConnectController;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_HANDLE, EFI_HANDLE, EFI_STATUS> DisconnectController;

    //
    // Open and Close Protocol Services
    //
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_GUID*, void**, EFI_HANDLE, EFI_HANDLE, uint, EFI_STATUS> OpenProtocol;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_GUID*, EFI_HANDLE, EFI_HANDLE, EFI_STATUS> CloseProtocol;
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_GUID*, EFI_OPEN_PROTOCOL_INFORMATION_ENTRY**, ulong*, EFI_STATUS> OpenProtocolInformation;

    //
    // Library Services
    //
    public readonly delegate* unmanaged<EFI_HANDLE, EFI_GUID***, ulong*, EFI_STATUS> ProtocolsPerHandle;
    public readonly delegate* unmanaged<EFI_LOCATE_SEARCH_TYPE, EFI_GUID*, void*, ulong*, EFI_HANDLE**, EFI_STATUS> LocateHandleBuffer;
    public readonly delegate* unmanaged<EFI_GUID*, void*, void**, EFI_STATUS> LocateProtocol;
    public void* InstallMultipleProtocolInterfaces;
    public void* UninstallMultipleProtocolInterfaces;

    //
    // 32-bit CRC Services
    //
    public readonly delegate* unmanaged<void*, ulong, uint*, EFI_STATUS> CalculateCrc32;

    //
    // Misc Services
    //
    public readonly delegate* unmanaged<void*, void*, ulong, EFI_STATUS> CopyMem;
    public readonly delegate* unmanaged<void*, ulong, byte, EFI_STATUS> SetMem;
    public readonly delegate* unmanaged<uint, EFI_TPL, delegate* unmanaged<EFI_EVENT, void*, void>, void*, EFI_GUID*, EFI_EVENT*, EFI_STATUS> CreateEventEx;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_CONFIGURATION_TABLE
{
    public EFI_GUID VendorGuid;
    public void* VendorTable;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SYSTEM_TABLE
{
    public EFI_TABLE_HEADER Hdr;

    public char* FirmwareVendor;
    public uint FirmwareRevision;

    public EFI_HANDLE ConsoleInHandle;
    public SIMPLE_INPUT_INTERFACE* ConIn;

    public EFI_HANDLE ConsoleOutHandle;
    public SIMPLE_TEXT_OUTPUT_INTERFACE* ConOut;

    public EFI_HANDLE StandardErrorHandle;
    public SIMPLE_TEXT_OUTPUT_INTERFACE* StdErr;

    public EFI_RUNTIME_SERVICES* RuntimeServices;
    public EFI_BOOT_SERVICES* BootServices;

    public ulong NumberOfTableEntries;
    public EFI_CONFIGURATION_TABLE* ConfigurationTable;

}

