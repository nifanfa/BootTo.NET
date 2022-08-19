using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct POOL_PRINT
{
    public char* str;
    public ulong len;
    public ulong maxlen;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_ADDRESS
{
    public byte Register;
    public byte Function;
    public byte Device;
    public byte Bus;
    public uint Reserved;
}

