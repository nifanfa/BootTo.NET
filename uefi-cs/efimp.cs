using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct EFI_CPU_PHYSICAL_LOCATION
{
    public uint Package;
    public uint Core;
    public uint Thread;
}

[StructLayout(LayoutKind.Sequential)]
public struct EFI_CPU_PHYSICAL_LOCATION2
{
    public uint Package;
    public uint Module;
    public uint Tile;
    public uint Die;
    public uint Core;
    public uint Thread;
}

[StructLayout(LayoutKind.Explicit)]
public struct EXTENDED_PROCESSOR_INFORMATION
{
    [FieldOffset(0)]
    public EFI_CPU_PHYSICAL_LOCATION2 Location2;
}

[StructLayout(LayoutKind.Sequential)]
public struct EFI_PROCESSOR_INFORMATION
{
    public ulong ProcessorId;
    public uint StatusFlag;
    public EFI_CPU_PHYSICAL_LOCATION Location;
    public EXTENDED_PROCESSOR_INFORMATION ExtendedInformation;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_MP_SERVICES_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_MP_SERVICES_PROTOCOL*, ulong*, ulong*, EFI_STATUS> GetNumberOfProcessors;
    public readonly delegate* unmanaged<EFI_MP_SERVICES_PROTOCOL*, ulong, EFI_PROCESSOR_INFORMATION*, EFI_STATUS> GetProcessorInfo;
    public readonly delegate* unmanaged<EFI_MP_SERVICES_PROTOCOL*, delegate* unmanaged<void*, void>, bool, EFI_EVENT, ulong, void*, ulong**, EFI_STATUS> StartupAllAPs;
    public readonly delegate* unmanaged<EFI_MP_SERVICES_PROTOCOL*, delegate* unmanaged<void*, void>, ulong, EFI_EVENT, ulong, void*, bool*, EFI_STATUS> StartupThisAP;
    public readonly delegate* unmanaged<EFI_MP_SERVICES_PROTOCOL*, ulong, bool, EFI_STATUS> SwitchBSP;
    public readonly delegate* unmanaged<EFI_MP_SERVICES_PROTOCOL*, ulong, bool, uint*, EFI_STATUS> EnableDisableAP;
    public readonly delegate* unmanaged<EFI_MP_SERVICES_PROTOCOL*, ulong*, EFI_STATUS> WhoAmI;
}
