namespace System
{
    public static partial class Convert
    {
        public static bool ToBoolean(bool value) => value;
        public static bool ToBoolean(byte value) => value != 0;
        public static bool ToBoolean(sbyte value) => value != 0;
        public static bool ToBoolean(short value) => value != 0;
        public static bool ToBoolean(ushort value) => value != 0;
        public static bool ToBoolean(int value) => value != 0;
        public static bool ToBoolean(uint value) => value != 0;
        public static bool ToBoolean(long value) => value != 0;
        public static bool ToBoolean(ulong value) => value != 0;
        public static bool ToBoolean(float value) => value != 0;
        public static bool ToBoolean(double value) => value != 0;

        public static bool ToBoolean(string value)
        {
            if (value == null)
                return false;

            int start = SkipWhiteSpace(value, 0);
            int end = value.Length;
            while (end > start && IsWhiteSpace(value[end - 1]))
                end--;
            if (end - start == 4 && EqualsIgnoreCase(value, start, "True"))
                return true;
            if (end - start == 5 && EqualsIgnoreCase(value, start, "False"))
                return false;
            throw new FormatException("The value must be either 'True' or 'False'.");
        }

        public static bool ToBoolean(object value)
        {
            if (value == null)
                return false;
            if (value is bool) return (bool)value;
            if (value is string) return ToBoolean((string)value);
            if (value is byte) return ToBoolean((byte)value);
            if (value is sbyte) return ToBoolean((sbyte)value);
            if (value is short) return ToBoolean((short)value);
            if (value is ushort) return ToBoolean((ushort)value);
            if (value is int) return ToBoolean((int)value);
            if (value is uint) return ToBoolean((uint)value);
            if (value is long) return ToBoolean((long)value);
            if (value is ulong) return ToBoolean((ulong)value);
            if (value is float) return ToBoolean((float)value);
            if (value is double) return ToBoolean((double)value);
            throw new InvalidCastException("The value cannot be converted to Boolean.");
        }

        public static byte ToByte(bool value) => value ? (byte)1 : (byte)0;
        public static byte ToByte(byte value) => value;
        public static byte ToByte(sbyte value) => checked((byte)value);
        public static byte ToByte(short value) => checked((byte)value);
        public static byte ToByte(ushort value) => checked((byte)value);
        public static byte ToByte(int value) => checked((byte)value);
        public static byte ToByte(uint value) => checked((byte)value);
        public static byte ToByte(long value) => checked((byte)value);
        public static byte ToByte(ulong value) => checked((byte)value);
        public static byte ToByte(char value) => checked((byte)value);
        public static byte ToByte(float value) => checked((byte)value);
        public static byte ToByte(double value) => checked((byte)value);
        public static byte ToByte(string value) => checked((byte)ParseSigned(value));

        public static byte ToByte(object value)
        {
            if (value == null) return 0;
            if (value is byte) return ToByte((byte)value);
            if (value is string) return ToByte((string)value);
            if (value is bool) return ToByte((bool)value);
            if (value is char) return ToByte((char)value);
            if (value is int) return ToByte((int)value);
            if (value is uint) return ToByte((uint)value);
            if (value is long) return ToByte((long)value);
            if (value is ulong) return ToByte((ulong)value);
            if (value is short) return ToByte((short)value);
            if (value is ushort) return ToByte((ushort)value);
            if (value is sbyte) return ToByte((sbyte)value);
            if (value is float) return ToByte((float)value);
            if (value is double) return ToByte((double)value);
            throw new InvalidCastException("The value cannot be converted to Byte.");
        }

        public static sbyte ToSByte(bool value) => value ? (sbyte)1 : (sbyte)0;
        public static sbyte ToSByte(byte value) => checked((sbyte)value);
        public static sbyte ToSByte(sbyte value) => value;
        public static sbyte ToSByte(short value) => checked((sbyte)value);
        public static sbyte ToSByte(ushort value) => checked((sbyte)value);
        public static sbyte ToSByte(int value) => checked((sbyte)value);
        public static sbyte ToSByte(uint value) => checked((sbyte)value);
        public static sbyte ToSByte(long value) => checked((sbyte)value);
        public static sbyte ToSByte(ulong value) => checked((sbyte)value);
        public static sbyte ToSByte(char value) => checked((sbyte)value);
        public static sbyte ToSByte(float value) => checked((sbyte)value);
        public static sbyte ToSByte(double value) => checked((sbyte)value);
        public static sbyte ToSByte(string value) => checked((sbyte)ParseSigned(value));

        public static short ToInt16(bool value) => value ? (short)1 : (short)0;
        public static short ToInt16(byte value) => value;
        public static short ToInt16(sbyte value) => value;
        public static short ToInt16(short value) => value;
        public static short ToInt16(ushort value) => checked((short)value);
        public static short ToInt16(int value) => checked((short)value);
        public static short ToInt16(uint value) => checked((short)value);
        public static short ToInt16(long value) => checked((short)value);
        public static short ToInt16(ulong value) => checked((short)value);
        public static short ToInt16(char value) => checked((short)value);
        public static short ToInt16(float value) => checked((short)value);
        public static short ToInt16(double value) => checked((short)value);
        public static short ToInt16(string value) => checked((short)ParseSigned(value));

        public static ushort ToUInt16(bool value) => value ? (ushort)1 : (ushort)0;
        public static ushort ToUInt16(byte value) => value;
        public static ushort ToUInt16(sbyte value) => checked((ushort)value);
        public static ushort ToUInt16(short value) => checked((ushort)value);
        public static ushort ToUInt16(ushort value) => value;
        public static ushort ToUInt16(int value) => checked((ushort)value);
        public static ushort ToUInt16(uint value) => checked((ushort)value);
        public static ushort ToUInt16(long value) => checked((ushort)value);
        public static ushort ToUInt16(ulong value) => checked((ushort)value);
        public static ushort ToUInt16(char value) => value;
        public static ushort ToUInt16(float value) => checked((ushort)value);
        public static ushort ToUInt16(double value) => checked((ushort)value);
        public static ushort ToUInt16(string value) => checked((ushort)ParseSigned(value));

        public static int ToInt32(bool value) => value ? 1 : 0;
        public static int ToInt32(byte value) => value;
        public static int ToInt32(sbyte value) => value;
        public static int ToInt32(short value) => value;
        public static int ToInt32(ushort value) => value;
        public static int ToInt32(int value) => value;
        public static int ToInt32(uint value) => checked((int)value);
        public static int ToInt32(long value) => checked((int)value);
        public static int ToInt32(ulong value) => checked((int)value);
        public static int ToInt32(char value) => value;
        public static int ToInt32(float value) => checked((int)value);
        public static int ToInt32(double value) => checked((int)value);
        public static int ToInt32(string value) => checked((int)ParseSigned(value));

        public static int ToInt32(object value)
        {
            if (value == null) return 0;
            if (value is int) return ToInt32((int)value);
            if (value is string) return ToInt32((string)value);
            if (value is bool) return ToInt32((bool)value);
            if (value is char) return ToInt32((char)value);
            if (value is byte) return ToInt32((byte)value);
            if (value is sbyte) return ToInt32((sbyte)value);
            if (value is short) return ToInt32((short)value);
            if (value is ushort) return ToInt32((ushort)value);
            if (value is uint) return ToInt32((uint)value);
            if (value is long) return ToInt32((long)value);
            if (value is ulong) return ToInt32((ulong)value);
            if (value is float) return ToInt32((float)value);
            if (value is double) return ToInt32((double)value);
            throw new InvalidCastException("The value cannot be converted to Int32.");
        }

        public static uint ToUInt32(bool value) => value ? 1u : 0u;
        public static uint ToUInt32(byte value) => value;
        public static uint ToUInt32(sbyte value) => checked((uint)value);
        public static uint ToUInt32(short value) => checked((uint)value);
        public static uint ToUInt32(ushort value) => value;
        public static uint ToUInt32(int value) => checked((uint)value);
        public static uint ToUInt32(uint value) => value;
        public static uint ToUInt32(long value) => checked((uint)value);
        public static uint ToUInt32(ulong value) => checked((uint)value);
        public static uint ToUInt32(char value) => value;
        public static uint ToUInt32(float value) => checked((uint)value);
        public static uint ToUInt32(double value) => checked((uint)value);
        public static uint ToUInt32(string value) => checked((uint)ParseUnsigned(value));

        public static long ToInt64(bool value) => value ? 1L : 0L;
        public static long ToInt64(byte value) => value;
        public static long ToInt64(sbyte value) => value;
        public static long ToInt64(short value) => value;
        public static long ToInt64(ushort value) => value;
        public static long ToInt64(int value) => value;
        public static long ToInt64(uint value) => value;
        public static long ToInt64(long value) => value;
        public static long ToInt64(ulong value) => checked((long)value);
        public static long ToInt64(char value) => value;
        public static long ToInt64(float value) => checked((long)value);
        public static long ToInt64(double value) => checked((long)value);
        public static long ToInt64(string value) => ParseSigned(value);

        public static ulong ToUInt64(bool value) => value ? 1UL : 0UL;
        public static ulong ToUInt64(byte value) => value;
        public static ulong ToUInt64(sbyte value) => checked((ulong)value);
        public static ulong ToUInt64(short value) => checked((ulong)value);
        public static ulong ToUInt64(ushort value) => value;
        public static ulong ToUInt64(int value) => checked((ulong)value);
        public static ulong ToUInt64(uint value) => value;
        public static ulong ToUInt64(long value) => checked((ulong)value);
        public static ulong ToUInt64(ulong value) => value;
        public static ulong ToUInt64(char value) => value;
        public static ulong ToUInt64(float value) => checked((ulong)value);
        public static ulong ToUInt64(double value) => checked((ulong)value);
        public static ulong ToUInt64(string value) => ParseUnsigned(value);

        public static char ToChar(char value) => value;
        public static char ToChar(byte value) => (char)value;
        public static char ToChar(ushort value) => (char)value;
        public static char ToChar(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The string to convert to Char cannot be null.");
            if (value.Length != 1)
                throw new FormatException("The string to convert to Char must contain exactly one character.");
            return value[0];
        }

        public static char ToChar(object value)
        {
            if (value == null)
                return '\0';
            if (value is char) return (char)value;
            if (value is byte) return ToChar((byte)value);
            if (value is ushort) return ToChar((ushort)value);
            if (value is string) return ToChar((string)value);
            throw new InvalidCastException("The value cannot be converted to Char.");
        }

        public static float ToSingle(bool value) => value ? 1f : 0f;
        public static float ToSingle(byte value) => value;
        public static float ToSingle(sbyte value) => value;
        public static float ToSingle(short value) => value;
        public static float ToSingle(ushort value) => value;
        public static float ToSingle(int value) => value;
        public static float ToSingle(uint value) => value;
        public static float ToSingle(long value) => value;
        public static float ToSingle(ulong value) => value;
        public static float ToSingle(float value) => value;
        public static float ToSingle(double value) => (float)value;
        public static float ToSingle(string value) => (float)ParseDouble(value);

        public static double ToDouble(bool value) => value ? 1d : 0d;
        public static double ToDouble(byte value) => value;
        public static double ToDouble(sbyte value) => value;
        public static double ToDouble(short value) => value;
        public static double ToDouble(ushort value) => value;
        public static double ToDouble(int value) => value;
        public static double ToDouble(uint value) => value;
        public static double ToDouble(long value) => value;
        public static double ToDouble(ulong value) => value;
        public static double ToDouble(float value) => value;
        public static double ToDouble(double value) => value;
        public static double ToDouble(string value) => ParseDouble(value);

        public static string ToString(bool value) => value.ToString();
        public static string ToString(char value) => value.ToString();
        public static string ToString(byte value) => value.ToString();
        public static string ToString(sbyte value) => value.ToString();
        public static string ToString(short value) => value.ToString();
        public static string ToString(ushort value) => value.ToString();
        public static string ToString(int value) => value.ToString();
        public static string ToString(uint value) => value.ToString();
        public static string ToString(long value) => value.ToString();
        public static string ToString(ulong value) => value.ToString();
        public static string ToString(float value) => value.ToString();
        public static string ToString(double value) => value.ToString();
        public static string ToString(string value) => value ?? string.Empty;

        private static long ParseSigned(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The signed numeric string cannot be null.");

            int index = SkipWhiteSpace(value, 0);
            bool negative = false;
            if (index < value.Length && (value[index] == '+' || value[index] == '-'))
            {
                negative = value[index] == '-';
                index++;
            }

            if (index == value.Length)
                throw new FormatException("The signed numeric string contains no digits.");

            ulong limit = negative ? 0x8000000000000000UL : 0x7FFFFFFFFFFFFFFFUL;
            ulong result = 0;
            int digits = 0;
            while (index < value.Length && value[index] >= '0' && value[index] <= '9')
            {
                uint digit = (uint)(value[index++] - '0');
                if (result > (limit - digit) / 10)
                    throw new OverflowException("The signed numeric value is outside the Int64 range.");
                result = result * 10 + digit;
                digits++;
            }

            index = SkipWhiteSpace(value, index);
            if (digits == 0 || index != value.Length)
                throw new FormatException("The signed numeric string is not valid.");
            if (negative)
                return result == 0x8000000000000000UL ? long.MinValue : -(long)result;
            return (long)result;
        }

        private static ulong ParseUnsigned(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The unsigned numeric string cannot be null.");

            int index = SkipWhiteSpace(value, 0);
            if (index < value.Length && value[index] == '+')
                index++;
            if (index == value.Length)
                throw new FormatException("The unsigned numeric string contains no digits.");

            ulong result = 0;
            int digits = 0;
            while (index < value.Length && value[index] >= '0' && value[index] <= '9')
            {
                uint digit = (uint)(value[index++] - '0');
                if (result > (ulong.MaxValue - digit) / 10)
                    throw new OverflowException("The unsigned numeric value is outside the UInt64 range.");
                result = result * 10 + digit;
                digits++;
            }

            index = SkipWhiteSpace(value, index);
            if (digits == 0 || index != value.Length)
                throw new FormatException("The unsigned numeric string is not valid.");
            return result;
        }

        private static double ParseDouble(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The floating-point string cannot be null.");

            int index = SkipWhiteSpace(value, 0);
            bool negative = false;
            if (index < value.Length && (value[index] == '+' || value[index] == '-'))
            {
                negative = value[index] == '-';
                index++;
            }

            double result = 0;
            int digits = 0;
            while (index < value.Length && value[index] >= '0' && value[index] <= '9')
            {
                result = result * 10 + (value[index++] - '0');
                digits++;
            }
            if (index < value.Length && value[index] == '.')
            {
                index++;
                double place = 0.1;
                while (index < value.Length && value[index] >= '0' && value[index] <= '9')
                {
                    result += (value[index++] - '0') * place;
                    place *= 0.1;
                    digits++;
                }
            }
            if (digits == 0)
                throw new FormatException("The floating-point string contains no digits.");

            int exponent = 0;
            bool exponentNegative = false;
            if (index < value.Length && (value[index] == 'e' || value[index] == 'E'))
            {
                index++;
                if (index < value.Length && (value[index] == '+' || value[index] == '-'))
                {
                    exponentNegative = value[index] == '-';
                    index++;
                }
                int exponentDigits = 0;
                while (index < value.Length && value[index] >= '0' && value[index] <= '9')
                {
                    if (exponent < 10000)
                        exponent = exponent * 10 + value[index] - '0';
                    index++;
                    exponentDigits++;
                }
                if (exponentDigits == 0)
                    throw new FormatException("The exponent contains no digits.");
            }

            index = SkipWhiteSpace(value, index);
            if (index != value.Length)
                throw new FormatException("The floating-point string contains invalid trailing characters.");
            while (exponent-- > 0)
                result = exponentNegative ? result * 0.1 : result * 10;
            return negative ? -result : result;
        }

        private static int SkipWhiteSpace(string value, int index)
        {
            while (index < value.Length && IsWhiteSpace(value[index]))
                index++;
            return index;
        }

        private static bool IsWhiteSpace(char value)
            => value == ' ' || value == '\t' || value == '\r' || value == '\n' ||
               value == '\f' || value == '\v';

        private static bool EqualsIgnoreCase(string value, int start, string expected)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                char actual = value[start + i];
                if (actual >= 'a' && actual <= 'z')
                    actual = (char)(actual - ('a' - 'A'));
                if (actual != expected[i])
                    return false;
            }
            return true;
        }
    }
}
