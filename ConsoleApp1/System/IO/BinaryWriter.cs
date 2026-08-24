using Internal.Runtime.CompilerServices;
using System.Text;

namespace System.IO
{
    public sealed unsafe class BinaryWriter : IDisposable
    {
        private readonly Stream _stream;
        private readonly Encoding _encoding;
        private readonly bool _leaveOpen;
        private bool _closed;

        public BinaryWriter(Stream output)
            : this(output, Encoding.UTF8, false)
        {
        }

        public BinaryWriter(Stream output, Encoding encoding)
            : this(output, encoding, false)
        {
        }

        public BinaryWriter(Stream output, Encoding encoding, bool leaveOpen)
        {
            if (output == null || encoding == null)
                throw new ArgumentNullException("The output stream and encoding cannot be null.");
            _stream = output;
            _encoding = encoding;
            _leaveOpen = leaveOpen;
        }

        public Stream BaseStream => _stream;

        public void Write(bool value) => Write((byte)(value ? 1 : 0));
        public void Write(byte value) { EnsureOpen(); _stream.WriteByte(value); }
        public void Write(sbyte value) => Write(unchecked((byte)value));
        public void Write(short value) => Write(unchecked((ushort)value));

        public void Write(ushort value)
        {
            Write((byte)value);
            Write((byte)(value >> 8));
        }

        public void Write(int value) => Write(unchecked((uint)value));

        public void Write(uint value)
        {
            Write((byte)value);
            Write((byte)(value >> 8));
            Write((byte)(value >> 16));
            Write((byte)(value >> 24));
        }

        public void Write(long value) => Write(unchecked((ulong)value));

        public void Write(ulong value)
        {
            Write((uint)value);
            Write((uint)(value >> 32));
        }

        public void Write(float value)
        {
            int bits = Unsafe.As<float, int>(ref value);
            Write(bits);
        }

        public void Write(double value)
        {
            long bits = Unsafe.As<double, long>(ref value);
            Write(bits);
        }

        public void Write(char value) => Write((ushort)value);

        public void Write(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("The byte buffer cannot be null.");
            Write(buffer, 0, buffer.Length);
        }

        public void Write(byte[] buffer, int index, int count)
        {
            EnsureOpen();
            if (buffer == null)
                throw new ArgumentNullException("The byte buffer cannot be null.");
            if (index < 0 || count < 0 || index > buffer.Length - count)
                throw new ArgumentException("The byte buffer offset and count do not describe a valid range.");
            _stream.Write(buffer, index, count);
        }

        public void Write(char[] chars)
        {
            if (chars == null)
                throw new ArgumentNullException("The character buffer cannot be null.");
            Write(new string(chars));
        }

        public void Write(string value)
        {
            EnsureOpen();
            if (value == null)
                throw new ArgumentNullException("The string value cannot be null.");
            byte[] bytes = _encoding.GetBytes(value);
            Write7BitEncodedInt(bytes.Length);
            Write(bytes, 0, bytes.Length);
        }

        public void Flush()
        {
            EnsureOpen();
            _stream.Flush();
        }

        public void Close()
        {
            if (_closed)
                return;
            try
            {
                Flush();
            }
            finally
            {
                _closed = true;
                if (!_leaveOpen)
                    _stream.Close();
            }
        }

        public void Dispose() => Close();

        private void Write7BitEncodedInt(int value)
        {
            uint unsigned = (uint)value;
            while (unsigned >= 0x80)
            {
                Write((byte)(unsigned | 0x80));
                unsigned >>= 7;
            }
            Write((byte)unsigned);
        }

        private void EnsureOpen()
        {
            if (_closed)
                throw new IOException("The binary writer is closed.");
        }
    }
}
