using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MBR_PARTITION_RECORD
{
    public byte BootIndicator;
    public byte StartHead;
    public byte StartSector;
    public byte StartTrack;
    public byte OSIndicator;
    public byte EndHead;
    public byte EndSector;
    public byte EndTrack;
    public fixed byte StartingLBA[4];
    public fixed byte SizeInLBA[4];
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct MASTER_BOOT_RECORD
{
    public fixed byte BootStrapCode[440];
    public fixed byte UniqueMbrSignature[4];
    public fixed byte Unknown[2];
    public MBR_PARTITION_RECORD Partition_0;
    public MBR_PARTITION_RECORD Partition_1;
    public MBR_PARTITION_RECORD Partition_2;
    public MBR_PARTITION_RECORD Partition_3;
    public ushort Signature;
}

