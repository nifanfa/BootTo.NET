namespace System.Reflection.PortableExecutable
{
    public readonly struct DirectoryEntry
    {
        public int RelativeVirtualAddress { get; }
        public int Size { get; }

        public DirectoryEntry(int relativeVirtualAddress, int size)
        {
            RelativeVirtualAddress = relativeVirtualAddress;
            Size = size;
        }

        internal DirectoryEntry(ref PEBinaryReader reader)
        {
            RelativeVirtualAddress = reader.ReadInt32();
            Size = reader.ReadInt32();
        }
    }
}
