namespace System.Reflection.PortableExecutable
{
    public readonly struct SectionHeader
    {
        public string Name { get; }
        public int VirtualSize { get; }
        public int VirtualAddress { get; }
        public int SizeOfRawData { get; }
        public int PointerToRawData { get; }
        public int PointerToRelocations { get; }
        public int PointerToLineNumbers { get; }
        public ushort NumberOfRelocations { get; }
        public ushort NumberOfLineNumbers { get; }
        public SectionCharacteristics SectionCharacteristics { get; }

        internal const int NameSize = 8;
        internal const int Size = 40;

        internal SectionHeader(ref PEBinaryReader reader)
        {
            Name = reader.ReadNullPaddedUTF8(NameSize);
            VirtualSize = reader.ReadInt32();
            VirtualAddress = reader.ReadInt32();
            SizeOfRawData = reader.ReadInt32();
            PointerToRawData = reader.ReadInt32();
            PointerToRelocations = reader.ReadInt32();
            PointerToLineNumbers = reader.ReadInt32();
            NumberOfRelocations = reader.ReadUInt16();
            NumberOfLineNumbers = reader.ReadUInt16();
            SectionCharacteristics = (SectionCharacteristics)reader.ReadUInt32();
        }
    }
}
