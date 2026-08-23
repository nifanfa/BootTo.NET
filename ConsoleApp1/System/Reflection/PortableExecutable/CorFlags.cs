namespace System.Reflection.PortableExecutable
{
    [System.Flags]
    public enum CorFlags
    {
        ILOnly = 0x00000001,
        Requires32Bit = 0x00000002,
        ILLibrary = 0x00000004,
        StrongNameSigned = 0x00000008,
        NativeEntryPoint = 0x00000010,
        TrackDebugData = 0x00010000,
        Prefers32Bit = 0x00020000,
    }
}
