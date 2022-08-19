using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct UI_STRING_ENTRY
{
    public ISO_639_2* LangCode;
    public char* UiString;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_UI_INTERFACE_PROTOCOL
{
    public uint Version;
    public UI_STRING_ENTRY* Entry;
}

