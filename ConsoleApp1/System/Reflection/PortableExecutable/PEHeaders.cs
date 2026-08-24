using System.IO;

namespace System.Reflection.PortableExecutable
{
    public sealed class PEHeaders
    {
        private const ushort DosSignature = 0x5A4D;
        private const int PESignatureOffsetLocation = 0x3C;
        private const uint PESignature = 0x00004550;
        private const int CorHeaderSize = 72;

        private readonly CoffHeader _coffHeader;
        private readonly PEHeader _peHeader;
        private readonly SectionHeader[] _sectionHeaders;
        private readonly CorHeader _corHeader;
        private readonly bool _isLoadedImage;
        private readonly int _metadataStartOffset;
        private readonly int _metadataSize;
        private readonly int _coffHeaderStartOffset;
        private readonly int _corHeaderStartOffset;
        private readonly int _peHeaderStartOffset;

        public PEHeaders(Stream peStream)
            : this(peStream, 0, false)
        {
        }

        public PEHeaders(Stream peStream, int size)
            : this(peStream, size, false)
        {
        }

        public PEHeaders(Stream peStream, int size, bool isLoadedImage)
        {
            if (peStream == null)
                throw new ArgumentNullException("The PE stream cannot be null.");
            if (!peStream.CanRead || !peStream.CanSeek)
                throw new ArgumentException("The PE stream must support reading and seeking.");

            long remaining = peStream.Length - peStream.Position;
            if (remaining < 0 || remaining > int.MaxValue || size < 0 || (size != 0 && size > remaining))
                throw new ArgumentException("The requested PE image size is outside the stream bounds.");

            int actualSize = size == 0 ? (int)remaining : size;
            if (actualSize <= 0)
                throw new ArgumentException("The PE image cannot be empty.");

            _isLoadedImage = isLoadedImage;
            var reader = new PEBinaryReader(peStream, actualSize);
            bool coffOnly;
            SkipDosHeader(ref reader, out coffOnly);

            _coffHeaderStartOffset = reader.CurrentOffset;
            _coffHeader = new CoffHeader(ref reader);
            if (!coffOnly)
            {
                _peHeaderStartOffset = reader.CurrentOffset;
                _peHeader = new PEHeader(ref reader);
            }
            else
            {
                _peHeaderStartOffset = -1;
            }

            if (_coffHeader.NumberOfSections < 0)
                throw new BadImageFormatException("Invalid number of PE sections.");

            _sectionHeaders = new SectionHeader[_coffHeader.NumberOfSections];
            for (int i = 0; i < _sectionHeaders.Length; i++)
                _sectionHeaders[i] = new SectionHeader(ref reader);

            _corHeaderStartOffset = -1;
            if (!coffOnly && TryCalculateCorHeaderOffset(actualSize, out int corOffset))
            {
                _corHeaderStartOffset = corOffset;
                reader.Seek(corOffset);
                _corHeader = new CorHeader(ref reader);
            }
            else
            {
                _corHeader = null;
            }

            CalculateMetadataLocation(actualSize, out _metadataStartOffset, out _metadataSize);
        }

        public int MetadataStartOffset => _metadataStartOffset;
        public int MetadataSize => _metadataSize;
        public CoffHeader CoffHeader => _coffHeader;
        public int CoffHeaderStartOffset => _coffHeaderStartOffset;
        public bool IsCoffOnly => _peHeader == null;
        public PEHeader PEHeader => _peHeader;
        public int PEHeaderStartOffset => _peHeaderStartOffset;
        public SectionHeader[] SectionHeaders => _sectionHeaders;
        public CorHeader CorHeader => _corHeader;
        public int CorHeaderStartOffset => _corHeaderStartOffset;

        public bool IsConsoleApplication
            => _peHeader != null && _peHeader.Subsystem == Subsystem.WindowsCui;

        public bool IsDll
            => (_coffHeader.Characteristics & Characteristics.Dll) != 0;

        public bool IsExe => !IsDll;

        public bool TryGetDirectoryOffset(DirectoryEntry directory, out int offset)
        {
            int sectionIndex = GetContainingSectionIndex(directory.RelativeVirtualAddress);
            if (sectionIndex < 0)
            {
                offset = -1;
                return false;
            }

            SectionHeader section = _sectionHeaders[sectionIndex];
            int relativeOffset = directory.RelativeVirtualAddress - section.VirtualAddress;
            if (relativeOffset < 0 || relativeOffset > section.VirtualSize)
            {
                offset = -1;
                return false;
            }

            long calculatedOffset = _isLoadedImage
                ? directory.RelativeVirtualAddress
                : (long)section.PointerToRawData + relativeOffset;
            if (calculatedOffset < 0 || calculatedOffset > int.MaxValue)
            {
                offset = -1;
                return false;
            }

            offset = (int)calculatedOffset;
            return true;
        }

        public int GetContainingSectionIndex(int relativeVirtualAddress)
        {
            for (int i = 0; i < _sectionHeaders.Length; i++)
            {
                SectionHeader section = _sectionHeaders[i];
                long start = section.VirtualAddress;
                long size = section.VirtualSize;
                if (start <= relativeVirtualAddress && relativeVirtualAddress < start + size)
                    return i;
            }
            return -1;
        }

        private static void SkipDosHeader(ref PEBinaryReader reader, out bool coffOnly)
        {
            ushort signature = reader.ReadUInt16();
            if (signature != DosSignature)
            {
                if (signature == 0 && reader.ReadUInt16() == 0xFFFF)
                {
                    coffOnly = true;
                    reader.Seek(0);
                    return;
                }
                throw new BadImageFormatException("Unknown PE file format.");
            }

            reader.Seek(PESignatureOffsetLocation);
            int ntHeaderOffset = reader.ReadInt32();
            reader.Seek(ntHeaderOffset);
            if (reader.ReadUInt32() != PESignature)
                throw new BadImageFormatException("Invalid PE signature.");
            coffOnly = false;
        }

        private bool TryCalculateCorHeaderOffset(int imageSize, out int offset)
        {
            if (_peHeader == null || !TryGetDirectoryOffset(_peHeader.CorHeaderTableDirectory, out offset))
            {
                offset = -1;
                return false;
            }

            if (_peHeader.CorHeaderTableDirectory.Size < CorHeaderSize)
                throw new BadImageFormatException("Invalid CLR header size.");
            if (offset < 0 || offset > imageSize - CorHeaderSize)
                throw new BadImageFormatException("CLR header is outside the image.");
            return true;
        }

        private void CalculateMetadataLocation(int imageSize, out int start, out int size)
        {
            if (IsCoffOnly)
            {
                int section = IndexOfSection(".cormeta");
                if (section < 0)
                {
                    start = -1;
                    size = 0;
                    return;
                }

                SectionHeader header = _sectionHeaders[section];
                start = _isLoadedImage ? header.VirtualAddress : header.PointerToRawData;
                size = _isLoadedImage ? header.VirtualSize : header.SizeOfRawData;
            }
            else if (_corHeader == null)
            {
                start = 0;
                size = 0;
                return;
            }
            else
            {
                if (!TryGetDirectoryOffset(_corHeader.MetadataDirectory, out start))
                    throw new BadImageFormatException("CLR metadata directory is missing.");
                size = _corHeader.MetadataDirectory.Size;
            }

            if (start < 0 || size <= 0 || start > imageSize - size)
                throw new BadImageFormatException("Metadata is outside the image.");
        }

        private int IndexOfSection(string name)
        {
            for (int i = 0; i < _sectionHeaders.Length; i++)
                if (StringEquals(_sectionHeaders[i].Name, name))
                    return i;
            return -1;
        }

        private static bool StringEquals(string first, string second)
        {
            if (ReferenceEquals(first, null) || ReferenceEquals(second, null) || first.Length != second.Length)
                return false;
            for (int i = 0; i < first.Length; i++)
                if (first[i] != second[i])
                    return false;
            return true;
        }
    }
}
