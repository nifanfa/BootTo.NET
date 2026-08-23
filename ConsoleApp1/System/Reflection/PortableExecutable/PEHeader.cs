namespace System.Reflection.PortableExecutable
{
    public sealed class PEHeader
    {
        public PEMagic Magic { get; }
        public byte MajorLinkerVersion { get; }
        public byte MinorLinkerVersion { get; }
        public int SizeOfCode { get; }
        public int SizeOfInitializedData { get; }
        public int SizeOfUninitializedData { get; }
        public int AddressOfEntryPoint { get; }
        public int BaseOfCode { get; }
        public int BaseOfData { get; }
        public ulong ImageBase { get; }
        public int SectionAlignment { get; }
        public int FileAlignment { get; }
        public ushort MajorOperatingSystemVersion { get; }
        public ushort MinorOperatingSystemVersion { get; }
        public ushort MajorImageVersion { get; }
        public ushort MinorImageVersion { get; }
        public ushort MajorSubsystemVersion { get; }
        public ushort MinorSubsystemVersion { get; }
        public int SizeOfImage { get; }
        public int SizeOfHeaders { get; }
        public uint CheckSum { get; }
        public Subsystem Subsystem { get; }
        public DllCharacteristics DllCharacteristics { get; }
        public ulong SizeOfStackReserve { get; }
        public ulong SizeOfStackCommit { get; }
        public ulong SizeOfHeapReserve { get; }
        public ulong SizeOfHeapCommit { get; }
        public int NumberOfRvaAndSizes { get; }

        public DirectoryEntry ExportTableDirectory { get; }
        public DirectoryEntry ImportTableDirectory { get; }
        public DirectoryEntry ResourceTableDirectory { get; }
        public DirectoryEntry ExceptionTableDirectory { get; }
        public DirectoryEntry CertificateTableDirectory { get; }
        public DirectoryEntry BaseRelocationTableDirectory { get; }
        public DirectoryEntry DebugTableDirectory { get; }
        public DirectoryEntry CopyrightTableDirectory { get; }
        public DirectoryEntry GlobalPointerTableDirectory { get; }
        public DirectoryEntry ThreadLocalStorageTableDirectory { get; }
        public DirectoryEntry LoadConfigTableDirectory { get; }
        public DirectoryEntry BoundImportTableDirectory { get; }
        public DirectoryEntry ImportAddressTableDirectory { get; }
        public DirectoryEntry DelayImportTableDirectory { get; }
        public DirectoryEntry CorHeaderTableDirectory { get; }

        internal const int OffsetOfChecksum = 64;

        internal PEHeader(ref PEBinaryReader reader)
        {
            Magic = (PEMagic)reader.ReadUInt16();
            if (Magic != PEMagic.PE32 && Magic != PEMagic.PE32Plus)
                throw new BadImageFormatException("Unknown PE optional-header magic.");

            MajorLinkerVersion = reader.ReadByte();
            MinorLinkerVersion = reader.ReadByte();
            SizeOfCode = reader.ReadInt32();
            SizeOfInitializedData = reader.ReadInt32();
            SizeOfUninitializedData = reader.ReadInt32();
            AddressOfEntryPoint = reader.ReadInt32();
            BaseOfCode = reader.ReadInt32();
            BaseOfData = Magic == PEMagic.PE32 ? reader.ReadInt32() : 0;
            ImageBase = Magic == PEMagic.PE32Plus ? reader.ReadUInt64() : reader.ReadUInt32();
            SectionAlignment = reader.ReadInt32();
            FileAlignment = reader.ReadInt32();
            MajorOperatingSystemVersion = reader.ReadUInt16();
            MinorOperatingSystemVersion = reader.ReadUInt16();
            MajorImageVersion = reader.ReadUInt16();
            MinorImageVersion = reader.ReadUInt16();
            MajorSubsystemVersion = reader.ReadUInt16();
            MinorSubsystemVersion = reader.ReadUInt16();
            reader.ReadUInt32();
            SizeOfImage = reader.ReadInt32();
            SizeOfHeaders = reader.ReadInt32();
            CheckSum = reader.ReadUInt32();
            Subsystem = (Subsystem)reader.ReadUInt16();
            DllCharacteristics = (DllCharacteristics)reader.ReadUInt16();

            if (Magic == PEMagic.PE32Plus)
            {
                SizeOfStackReserve = reader.ReadUInt64();
                SizeOfStackCommit = reader.ReadUInt64();
                SizeOfHeapReserve = reader.ReadUInt64();
                SizeOfHeapCommit = reader.ReadUInt64();
            }
            else
            {
                SizeOfStackReserve = reader.ReadUInt32();
                SizeOfStackCommit = reader.ReadUInt32();
                SizeOfHeapReserve = reader.ReadUInt32();
                SizeOfHeapCommit = reader.ReadUInt32();
            }

            reader.ReadUInt32();
            NumberOfRvaAndSizes = reader.ReadInt32();
            ExportTableDirectory = new DirectoryEntry(ref reader);
            ImportTableDirectory = new DirectoryEntry(ref reader);
            ResourceTableDirectory = new DirectoryEntry(ref reader);
            ExceptionTableDirectory = new DirectoryEntry(ref reader);
            CertificateTableDirectory = new DirectoryEntry(ref reader);
            BaseRelocationTableDirectory = new DirectoryEntry(ref reader);
            DebugTableDirectory = new DirectoryEntry(ref reader);
            CopyrightTableDirectory = new DirectoryEntry(ref reader);
            GlobalPointerTableDirectory = new DirectoryEntry(ref reader);
            ThreadLocalStorageTableDirectory = new DirectoryEntry(ref reader);
            LoadConfigTableDirectory = new DirectoryEntry(ref reader);
            BoundImportTableDirectory = new DirectoryEntry(ref reader);
            ImportAddressTableDirectory = new DirectoryEntry(ref reader);
            DelayImportTableDirectory = new DirectoryEntry(ref reader);
            CorHeaderTableDirectory = new DirectoryEntry(ref reader);
            new DirectoryEntry(ref reader);
        }
    }
}
