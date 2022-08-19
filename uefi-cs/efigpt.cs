using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PARTITION_TABLE_HEADER
{
    public EFI_TABLE_HEADER Header;
    public EFI_LBA MyLBA;
    public EFI_LBA AlternateLBA;
    public EFI_LBA FirstUsableLBA;
    public EFI_LBA LastUsableLBA;
    public EFI_GUID DiskGUID;
    public EFI_LBA PartitionEntryLBA;
    public uint NumberOfPartitionEntries;
    public uint SizeOfPartitionEntry;
    public uint PartitionEntryArrayCRC32;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct EFI_PARTITION_ENTRY
{
    public EFI_GUID PartitionTypeGUID;
    public EFI_GUID UniquePartitionGUID;
    public EFI_LBA StartingLBA;
    public EFI_LBA EndingLBA;
    public ulong Attributes;
    public fixed char PartitionName[36];
}

