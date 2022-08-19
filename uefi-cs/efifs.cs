using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PARTITION_HEADER
{
    public EFI_TABLE_HEADER Hdr;
    public uint DirectoryAllocationNumber;
    public uint BlockSize;
    public EFI_LBA FirstUsableLba;
    public EFI_LBA LastUsableLba;
    public EFI_LBA UnusableSpace;
    public EFI_LBA FreeSpace;
    public EFI_LBA RootFile;
    public EFI_LBA SecutiryFile;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_FILE_HEADER
{
    public EFI_TABLE_HEADER Hdr;
    public uint Class;
    public uint LBALOffset;
    public EFI_LBA Parent;
    public ulong FileSize;
    public ulong FileAttributes;
    public EFI_TIME FileCreateTime;
    public EFI_TIME FileModificationTime;
    public EFI_GUID VendorGuid;
    public fixed char FileString[260];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_LBAL
{
    public EFI_TABLE_HEADER Hdr;
    public uint Class;
    public EFI_LBA Parent;
    public EFI_LBA Next;
    public uint ArraySize;
    public uint ArrayCount;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_RL
{
    public EFI_LBA Start;
    public ulong Length;
}

