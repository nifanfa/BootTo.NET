using Internal.Runtime.CompilerServices;

namespace System
{
    public static class BitConverter
    {
        public static readonly bool IsLittleEndian = true;

        public static byte[] GetBytes(bool value) => new byte[] { (byte)(value ? 1 : 0) };
        public static byte[] GetBytes(char value) => GetBytes((ushort)value);
        public static byte[] GetBytes(short value) => GetBytes(unchecked((ushort)value));
        public static byte[] GetBytes(ushort value) => new byte[] { (byte)value, (byte)(value >> 8) };
        public static byte[] GetBytes(int value) => GetBytes(unchecked((uint)value));
        public static byte[] GetBytes(uint value) => new byte[] { (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) };
        public static byte[] GetBytes(long value) => GetBytes(unchecked((ulong)value));

        public static byte[] GetBytes(ulong value)
            => new byte[] { (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24),
                (byte)(value >> 32), (byte)(value >> 40), (byte)(value >> 48), (byte)(value >> 56) };

        public static byte[] GetBytes(float value)
        {
            int bits = Unsafe.As<float, int>(ref value);
            return GetBytes(bits);
        }

        public static byte[] GetBytes(double value)
        {
            long bits = Unsafe.As<double, long>(ref value);
            return GetBytes(bits);
        }

        public static bool ToBoolean(byte[] value, int startIndex) => ReadByte(value, startIndex) != 0;
        public static char ToChar(byte[] value, int startIndex) => (char)ToUInt16(value, startIndex);
        public static short ToInt16(byte[] value, int startIndex) => unchecked((short)ToUInt16(value, startIndex));
        public static ushort ToUInt16(byte[] value, int startIndex)
            => (ushort)(ReadByte(value, startIndex) | (ReadByte(value, startIndex + 1) << 8));
        public static int ToInt32(byte[] value, int startIndex) => unchecked((int)ToUInt32(value, startIndex));
        public static uint ToUInt32(byte[] value, int startIndex)
            => (uint)(ReadByte(value, startIndex) | (ReadByte(value, startIndex + 1) << 8) |
                (ReadByte(value, startIndex + 2) << 16) | (ReadByte(value, startIndex + 3) << 24));
        public static long ToInt64(byte[] value, int startIndex) => unchecked((long)ToUInt64(value, startIndex));

        public static ulong ToUInt64(byte[] value, int startIndex)
        {
            ulong result = ToUInt32(value, startIndex);
            result |= (ulong)ToUInt32(value, startIndex + 4) << 32;
            return result;
        }

        public static float ToSingle(byte[] value, int startIndex)
        {
            int bits = ToInt32(value, startIndex);
            return Unsafe.As<int, float>(ref bits);
        }

        public static double ToDouble(byte[] value, int startIndex)
        {
            long bits = ToInt64(value, startIndex);
            return Unsafe.As<long, double>(ref bits);
        }

        public static long DoubleToInt64Bits(double value) => Unsafe.As<double, long>(ref value);
        public static double Int64BitsToDouble(long value) => Unsafe.As<long, double>(ref value);

        public static string ToString(byte[] value) => ToString(value, 0, value == null ? 0 : value.Length);
        public static string ToString(byte[] value, int startIndex)
            => ToString(value, startIndex, value == null ? 0 : value.Length - startIndex);

        public static string ToString(byte[] value, int startIndex, int length)
        {
            ValidateRange(value, startIndex, length);
            if (length == 0)
                return string.Empty;

            char[] result = new char[length * 3 - 1];
            int offset = 0;
            for (int i = 0; i < length; i++)
            {
                if (i != 0)
                    result[offset++] = '-';
                byte current = value[startIndex + i];
                result[offset++] = Hex((byte)(current >> 4));
                result[offset++] = Hex((byte)(current & 0xF));
            }
            return new string(result);
        }

        private static byte ReadByte(byte[] value, int index)
        {
            if (value == null)
                throw new ArgumentNullException();
            if (index < 0 || index >= value.Length)
                throw new ArgumentException();
            return value[index];
        }

        private static void ValidateRange(byte[] value, int startIndex, int length)
        {
            if (value == null)
                throw new ArgumentNullException();
            if (startIndex < 0 || length < 0 || startIndex > value.Length - length)
                throw new ArgumentException();
        }

        private static char Hex(byte value) => (char)(value < 10 ? '0' + value : 'A' + value - 10);
    }
}
