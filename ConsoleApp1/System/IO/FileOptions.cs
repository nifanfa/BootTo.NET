namespace System.IO
{
    [Flags]
    public enum FileOptions
    {
        None = 0,
        WriteThrough = unchecked((int)0x80000000),
        Asynchronous = 0x40000000,
        RandomAccess = 0x10000000,
        DeleteOnClose = 0x04000000,
        SequentialScan = 0x08000000,
        Encrypted = 0x00004000,
        NoBuffering = 0x20000000,
        Overlapped = Asynchronous,
        BackupSemantics = 0x02000000,
        OpenReparsePoint = 0x00200000,
        SessionAware = 0x00800000,
        OpenNoRecall = 0x00100000,
        NoRecall = 0x00400000,
    }
}
