namespace System.Reflection.PortableExecutable
{
    [System.Flags]
    public enum Characteristics : ushort
    {
        RelocsStripped = 0x0001,
        ExecutableImage = 0x0002,
        LineNumsStripped = 0x0004,
        LocalSymsStripped = 0x0008,
        AggressiveWSTrim = 0x0010,
        LargeAddressAware = 0x0020,
        BytesReversedLo = 0x0080,
        Bit32Machine = 0x0100,
        DebugStripped = 0x0200,
        RemovableRunFromSwap = 0x0400,
        NetRunFromSwap = 0x0800,
        System = 0x1000,
        Dll = 0x2000,
        UpSystemOnly = 0x4000,
        BytesReversedHi = 0x8000,
    }

    public enum PEMagic : ushort
    {
        PE32 = 0x010B,
        PE32Plus = 0x020B,
    }

    public enum Subsystem : ushort
    {
        Unknown = 0,
        Native = 1,
        WindowsGui = 2,
        WindowsCui = 3,
        OS2Cui = 5,
        PosixCui = 7,
        NativeWindows = 8,
        WindowsCEGui = 9,
        EfiApplication = 10,
        EfiBootServiceDriver = 11,
        EfiRuntimeDriver = 12,
        EfiRom = 13,
        Xbox = 14,
        WindowsBootApplication = 16,
    }

    [System.Flags]
    public enum DllCharacteristics : ushort
    {
        ProcessInit = 0x0001,
        ProcessTerm = 0x0002,
        ThreadInit = 0x0004,
        ThreadTerm = 0x0008,
        HighEntropyVirtualAddressSpace = 0x0020,
        DynamicBase = 0x0040,
        NxCompatible = 0x0100,
        NoIsolation = 0x0200,
        NoSeh = 0x0400,
        NoBind = 0x0800,
        AppContainer = 0x1000,
        WdmDriver = 0x2000,
        TerminalServerAware = 0x8000,
    }

    [System.Flags]
    public enum SectionCharacteristics : uint
    {
        TypeReg = 0x00000000,
        TypeDSect = 0x00000001,
        TypeNoLoad = 0x00000002,
        TypeGroup = 0x00000004,
        TypeNoPad = 0x00000008,
        TypeCopy = 0x00000010,
        ContainsCode = 0x00000020,
        ContainsInitializedData = 0x00000040,
        ContainsUninitializedData = 0x00000080,
        LinkerOther = 0x00000100,
        LinkerInfo = 0x00000200,
        TypeOver = 0x00000400,
        LinkerRemove = 0x00000800,
        LinkerComdat = 0x00001000,
        MemProtected = 0x00004000,
        NoDeferSpecExc = 0x00004000,
        GPRel = 0x00008000,
        MemFardata = 0x00008000,
        MemSysheap = 0x00010000,
        MemPurgeable = 0x00020000,
        Mem16Bit = 0x00020000,
        MemLocked = 0x00040000,
        MemPreload = 0x00080000,
        Align1Bytes = 0x00100000,
        Align2Bytes = 0x00200000,
        Align4Bytes = 0x00300000,
        Align8Bytes = 0x00400000,
        Align16Bytes = 0x00500000,
        Align32Bytes = 0x00600000,
        Align64Bytes = 0x00700000,
        Align128Bytes = 0x00800000,
        Align256Bytes = 0x00900000,
        Align512Bytes = 0x00A00000,
        Align1024Bytes = 0x00B00000,
        Align2048Bytes = 0x00C00000,
        Align4096Bytes = 0x00D00000,
        Align8192Bytes = 0x00E00000,
        AlignMask = 0x00F00000,
        LinkerNRelocOvfl = 0x01000000,
        MemDiscardable = 0x02000000,
        MemNotCached = 0x04000000,
        MemNotPaged = 0x08000000,
        MemShared = 0x10000000,
        MemExecute = 0x20000000,
        MemRead = 0x40000000,
        MemWrite = 0x80000000,
    }
}
