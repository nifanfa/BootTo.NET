namespace System.Reflection.PortableExecutable
{
    [System.Flags]
    public enum PEStreamOptions
    {
        Default = 0,
        LeaveOpen = 1,
        PrefetchMetadata = 2,
        PrefetchEntireImage = 4,
        IsLoadedImage = 8,
    }
}
