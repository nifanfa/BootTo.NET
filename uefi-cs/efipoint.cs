using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SIMPLE_POINTER_STATE
{
    public int RelativeMovementX;
    public int RelativeMovementY;
    public int RelativeMovementZ;
    public bool LeftButton;
    public bool RightButton;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SIMPLE_POINTER_MODE
{
    public ulong ResolutionX;
    public ulong ResolutionY;
    public ulong ResolutionZ;
    public bool LeftButton;
    public bool RightButton;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SIMPLE_POINTER_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_SIMPLE_POINTER_PROTOCOL*, bool, EFI_STATUS> Reset;
    public readonly delegate* unmanaged<EFI_SIMPLE_POINTER_PROTOCOL*, EFI_SIMPLE_POINTER_STATE*, EFI_STATUS> GetState;
    public EFI_EVENT WaitForInput;
    public EFI_SIMPLE_POINTER_MODE* Mode;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_ABSOLUTE_POINTER_MODE
{
    public ulong AbsoluteMinX;
    public ulong AbsoluteMinY;
    public ulong AbsoluteMinZ;
    public ulong AbsoluteMaxX;
    public ulong AbsoluteMaxY;
    public ulong AbsoluteMaxZ;
    public uint Attributes;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_ABSOLUTE_POINTER_STATE
{
    public ulong CurrentX;
    public ulong CurrentY;
    public ulong CurrentZ;
    public uint ActiveButtons;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_ABSOLUTE_POINTER_PROTOCOL
{
    public readonly delegate* unmanaged<EFI_ABSOLUTE_POINTER_PROTOCOL*, bool, EFI_STATUS> Reset;
    public readonly delegate* unmanaged<EFI_ABSOLUTE_POINTER_PROTOCOL*, EFI_ABSOLUTE_POINTER_STATE*, EFI_STATUS> GetState;
    public EFI_EVENT WaitForInput;
    public EFI_ABSOLUTE_POINTER_MODE* Mode;
}

