using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct _LIST_ENTRY
{
    public _LIST_ENTRY* Flink;
    public _LIST_ENTRY* Blink;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct LIST_ENTRY
{
    public _LIST_ENTRY* Flink;
    public _LIST_ENTRY* Blink;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_LIST_ENTRY
{
    public _LIST_ENTRY* Flink;
    public _LIST_ENTRY* Blink;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FLOCK
{
    public EFI_TPL Tpl;
    public EFI_TPL OwnerTpl;
    public ulong Lock;
}

