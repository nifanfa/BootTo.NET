using Internal.Runtime.CompilerServices;
using System.Text;

namespace System.IO
{
    public sealed unsafe class BinaryReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly Encoding _encoding;
        private readonly bool _leaveOpen;
        private bool _closed;

        public BinaryReader(Stream input)
            : this(input, Encoding.UTF8, false)
        {
        }

        public BinaryReader(Stream input, Encoding encoding)
            : this(input, encoding, false)
        {
        }

        public BinaryReader(Stream input, Encoding encoding, bool leaveOpen)
        {
            if (input == null || encoding == null)
                throw new ArgumentNullException();
            _stream = input;
            _encoding = encoding;
            _leaveOpen = leaveOpen;
        }

        public Stream BaseStream => _stream;

        public bool ReadBoolean() => ReadByte() != 0;
        public byte ReadByte()
        {
            EnsureOpen();
            int value = _stream.ReadByte();
            if (value < 0)
                throw new IOException();
            return (byte)value;
        }

        public sbyte ReadSByte() => unchecked((sbyte)ReadByte());
        public short ReadInt16() => unchecked((short)ReadUInt16());
        public ushort ReadUInt16() => (ushort)(ReadByte() | (ReadByte() << 8));
        public int ReadInt32() => unchecked((int)ReadUInt32());

        public uint ReadUInt32()
        {
            uint value = ReadByte();
            value |= (uint)ReadByte() << 8;
            value |= (uint)ReadByte() << 16;
            value |= (uint)ReadByte() << 24;
            return value;
        }

        public long ReadInt64() => unchecked((long)ReadUInt64());

        public ulong ReadUInt64()
        {
            ulong value = ReadUInt32();
            value |= (ulong)ReadUInt32() << 32;
            return value;
        }

        public float ReadSingle()
        {
            int bits = ReadInt32();
            return Unsafe.As<int, float>(ref bits);
        }

        public double ReadDouble()
        {
            long bits = ReadInt64();
            return Unsafe.As<long, double>(ref bits);
        }

        public char ReadChar() => (char)ReadUInt16();

        public char[] ReadChars(int count)
        {
            if (count < 0)
                throw new ArgumentException();
            char[] result = new char[count];
            for (int i = 0; i < count; i++)
                result[i] = ReadChar();
            return result;
        }

        public byte[] ReadBytes(int count)
        {
            EnsureOpen();
            if (count < 0)
                throw new ArgumentException();
            byte[] result = new byte[count];
            int total = 0;
            while (total < count)
            {
                int read = _stream.Read(result, total, count - total);
                if (read == 0)
                    break;
                total += read;
            }
            if (total == count)
                return result;

            byte[] partial = new byte[total];
            for (int i = 0; i < total; i++)
                partial[i] = result[i];
            return partial;
        }

        public string ReadString()
        {
            int length = Read7BitEncodedInt();
            byte[] bytes = ReadExactly(length);
            return _encoding.GetString(bytes);
        }

        public void Close()
        {
            if (_closed)
                return;
            _closed = true;
            if (!_leaveOpen)
                _stream.Close();
        }

        public void Dispose() => Close();

        private int Read7BitEncodedInt()
        {
            int result = 0;
            int shift = 0;
            while (shift < 35)
            {
                byte value = ReadByte();
                result |= (value & 0x7F) << shift;
                if ((value & 0x80) == 0)
                    return result;
                shift += 7;
            }
            throw new FormatException();
        }

        private byte[] ReadExactly(int count)
        {
            byte[] result = new byte[count];
            int total = 0;
            while (total < count)
            {
                int read = _stream.Read(result, total, count - total);
                if (read == 0)
                    throw new IOException();
                total += read;
            }
            return result;
        }

        private void EnsureOpen()
        {
            if (_closed)
                throw new IOException();
        }
    }
}
