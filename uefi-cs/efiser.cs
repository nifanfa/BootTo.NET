using System.Runtime.InteropServices;

public enum EFI_PARITY_TYPE
{
    DefaultParity,
    NoParity,
    EvenParity,
    OddParity,
    MarkParity,
    SpaceParity
}

public enum EFI_STOP_BITS_TYPE
{
    DefaultStopBits,
    OneStopBit,         // 1 stop bit
    OneFiveStopBits,    // 1.5 stop bits
    TwoStopBits         // 2 stop bits
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SERIAL_IO_MODE
{
    public uint ControlMask;

    // current Attributes
    public uint Timeout;
    public ulong BaudRate;
    public uint ReceiveFifoDepth;
    public uint DataBits;
    public uint Parity;
    public uint StopBits;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SERIAL_IO_PROTOCOL
{
    public uint Revision;
    public readonly delegate* unmanaged<EFI_SERIAL_IO_PROTOCOL*, EFI_STATUS> Reset;
    public readonly delegate* unmanaged<EFI_SERIAL_IO_PROTOCOL*, ulong, uint, uint, EFI_PARITY_TYPE, byte, EFI_STOP_BITS_TYPE, EFI_STATUS> SetAttributes;
    public readonly delegate* unmanaged<EFI_SERIAL_IO_PROTOCOL*, uint, EFI_STATUS> SetControl;
    public readonly delegate* unmanaged<EFI_SERIAL_IO_PROTOCOL*, uint*, EFI_STATUS> GetControl;
    public readonly delegate* unmanaged<EFI_SERIAL_IO_PROTOCOL*, ulong*, void*, EFI_STATUS> Write;
    public readonly delegate* unmanaged<EFI_SERIAL_IO_PROTOCOL*, ulong*, void*, EFI_STATUS> Read;

    public SERIAL_IO_MODE* Mode;
}

