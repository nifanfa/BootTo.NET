using System.IO;
using System.Text;

namespace System.Reflection.PortableExecutable
{
    // Header parsing needs bounded reads because malformed PE offsets must not
    // make the underlying stream read past the image supplied by the caller.
    internal struct PEBinaryReader
    {
        private readonly long _startOffset;
        private readonly long _maxOffset;
        private readonly BinaryReader _reader;

        public PEBinaryReader(Stream stream, int size)
        {
            _startOffset = stream.Position;
            _maxOffset = _startOffset + size;
            _reader = new BinaryReader(stream, Encoding.UTF8, true);
        }

        public int CurrentOffset => (int)(_reader.BaseStream.Position - _startOffset);

        public void Seek(int offset)
        {
            CheckBounds(offset, 0);
            _reader.BaseStream.Seek(_startOffset + offset, SeekOrigin.Begin);
        }

        public byte[] ReadBytes(int count)
        {
            CheckBounds(CurrentOffset, count);
            byte[] bytes = _reader.ReadBytes(count);
            if (bytes.Length != count)
                throw new BadImageFormatException("The PE image is truncated.");
            return bytes;
        }

        public byte ReadByte() { CheckBounds(CurrentOffset, 1); return _reader.ReadByte(); }
        public short ReadInt16() { CheckBounds(CurrentOffset, 2); return _reader.ReadInt16(); }
        public ushort ReadUInt16() { CheckBounds(CurrentOffset, 2); return _reader.ReadUInt16(); }
        public int ReadInt32() { CheckBounds(CurrentOffset, 4); return _reader.ReadInt32(); }
        public uint ReadUInt32() { CheckBounds(CurrentOffset, 4); return _reader.ReadUInt32(); }
        public ulong ReadUInt64() { CheckBounds(CurrentOffset, 8); return _reader.ReadUInt64(); }

        public string ReadNullPaddedUTF8(int byteCount)
        {
            byte[] bytes = ReadBytes(byteCount);
            int length = bytes.Length;
            while (length > 0 && bytes[length - 1] == 0)
                length--;
            if (length == 0)
                return string.Empty;
            return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(bytes, 0, length));
        }

        private void CheckBounds(int start, int count)
        {
            if (start < 0 || count < 0 || (long)start + count > _maxOffset - _startOffset)
                throw new BadImageFormatException("The PE image contains an invalid offset or size.");
        }
    }
}
