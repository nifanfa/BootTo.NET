namespace System.Reflection.PortableExecutable
{
    [Flags]
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
}
