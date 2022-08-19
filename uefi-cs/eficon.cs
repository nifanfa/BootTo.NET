using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SIMPLE_TEXT_OUTPUT_MODE
{
    public int MaxMode;
    // current settings
    public int Mode;
    public int Attribute;
    public int CursorColumn;
    public int CursorRow;
    public bool CursorVisible;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SIMPLE_TEXT_OUTPUT_INTERFACE
{
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, bool, EFI_STATUS> Reset;

    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, char*, EFI_STATUS> OutputString;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, char*, EFI_STATUS> TestString;

    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, ulong, ulong*, ulong*, EFI_STATUS> QueryMode;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, ulong, EFI_STATUS> SetMode;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, ulong, EFI_STATUS> SetAttribute;

    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, EFI_STATUS> ClearScreen;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, ulong, ulong, EFI_STATUS> SetCursorPosition;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, bool, EFI_STATUS> EnableCursor;

    // Current mode
    public SIMPLE_TEXT_OUTPUT_MODE* Mode;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SIMPLE_TEXT_OUT_PROTOCOL
{
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, bool, EFI_STATUS> Reset;

    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, char*, EFI_STATUS> OutputString;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, char*, EFI_STATUS> TestString;

    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, ulong, ulong*, ulong*, EFI_STATUS> QueryMode;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, ulong, EFI_STATUS> SetMode;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, ulong, EFI_STATUS> SetAttribute;

    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, EFI_STATUS> ClearScreen;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, ulong, ulong, EFI_STATUS> SetCursorPosition;
    public readonly delegate* unmanaged<SIMPLE_TEXT_OUTPUT_INTERFACE*, bool, EFI_STATUS> EnableCursor;

    // Current mode
    public SIMPLE_TEXT_OUTPUT_MODE* Mode;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_INPUT_KEY
{
    public ushort ScanCode;
    public char UnicodeChar;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct SIMPLE_INPUT_INTERFACE
{
    public readonly delegate* unmanaged<SIMPLE_INPUT_INTERFACE*, bool, EFI_STATUS> Reset;
    public readonly delegate* unmanaged<SIMPLE_INPUT_INTERFACE*, EFI_INPUT_KEY*, EFI_STATUS> ReadKeyStroke;
    public EFI_EVENT WaitForKey;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_SIMPLE_TEXT_IN_PROTOCOL
{
    public readonly delegate* unmanaged<SIMPLE_INPUT_INTERFACE*, bool, EFI_STATUS> Reset;
    public readonly delegate* unmanaged<SIMPLE_INPUT_INTERFACE*, EFI_INPUT_KEY*, EFI_STATUS> ReadKeyStroke;
    public EFI_EVENT WaitForKey;
}

