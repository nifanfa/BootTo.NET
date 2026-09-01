namespace System.Reflection.PortableExecutable
{
    [Flags]
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
}
