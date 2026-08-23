using System.Text;

namespace System
{
    public struct Guid
    {
        public static readonly Guid Empty = new Guid();

        private int _a;
        private short _b;
        private short _c;
        private byte _d;
        private byte _e;
        private byte _f;
        private byte _g;
        private byte _h;
        private byte _i;
        private byte _j;
        private byte _k;

        public Guid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            _a = a;
            _b = b;
            _c = c;
            _d = d;
            _e = e;
            _f = f;
            _g = g;
            _h = h;
            _i = i;
            _j = j;
            _k = k;
        }

        public Guid(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException();
            if (bytes.Length != 16)
                throw new ArgumentException();
            this = FromBytes(bytes, 0);
        }

        public Guid(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 16)
                throw new ArgumentException();
            byte[] value = bytes;
            this = FromBytes(value, 0);
        }

        public Guid(string value)
        {
            Guid parsed;
            if (!TryParse(value, out parsed))
                throw new FormatException();
            this = parsed;
        }

        public static Guid NewGuid()
        {
            byte[] bytes = new byte[16];
            Random.Shared.NextBytes(bytes);
            bytes[7] = (byte)((bytes[7] & 0x0F) | 0x40);
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
            return new Guid(bytes);
        }

        public static Guid Parse(string value)
        {
            Guid result;
            if (!TryParse(value, out result))
                throw new FormatException();
            return result;
        }

        public static bool TryParse(string value, out Guid result)
        {
            result = Empty;
            if (value == null)
                return false;

            int start = 0;
            int end = value.Length;
            while (start < end && IsWhiteSpace(value[start])) start++;
            while (end > start && IsWhiteSpace(value[end - 1])) end--;
            if (end - start == 0)
                return false;

            if (value[start] == '{' && start + 2 < end && value[start + 1] == '0' &&
                (value[start + 2] == 'x' || value[start + 2] == 'X'))
                return TryParseX(Slice(value, start, end - start), out result);

            char first = value[start];
            char last = value[end - 1];
            if ((first == '{' && last == '}') || (first == '(' && last == ')'))
            {
                start++;
                end--;
            }

            int length = end - start;
            if (length == 32)
                return TryParseN(value, start, out result);
            if (length == 36)
                return TryParseD(value, start, out result);
            return false;
        }

        public static bool TryParseExact(string value, string format, out Guid result)
        {
            result = Empty;
            if (value == null || format == null || format.Length != 1)
                return false;

            char specifier = format[0];
            if (specifier == 'N' || specifier == 'n')
                return value.Length == 32 && TryParseN(value, 0, out result);
            if (specifier == 'D' || specifier == 'd')
                return value.Length == 36 && TryParseD(value, 0, out result);
            if (specifier == 'B' || specifier == 'b')
                return value.Length == 38 && value[0] == '{' && value[37] == '}' && TryParseD(value, 1, out result);
            if (specifier == 'P' || specifier == 'p')
                return value.Length == 38 && value[0] == '(' && value[37] == ')' && TryParseD(value, 1, out result);
            if (specifier == 'X' || specifier == 'x')
                return TryParseX(value, out result);
            return false;
        }

        public byte[] ToByteArray()
        {
            return new byte[]
            {
                (byte)_a, (byte)(_a >> 8), (byte)(_a >> 16), (byte)(_a >> 24),
                (byte)_b, (byte)(_b >> 8), (byte)_c, (byte)(_c >> 8),
                _d, _e, _f, _g, _h, _i, _j, _k
            };
        }

        public override string ToString() => ToString("D");

        public string ToString(string format)
        {
            if (string.IsNullOrEmpty(format))
                format = "D";
            if (format.Length != 1)
                throw new FormatException();

            char specifier = format[0];
            if (specifier == 'N' || specifier == 'n')
                return FormatD(false, '\0');
            if (specifier == 'D' || specifier == 'd')
                return FormatD(true, '\0');
            if (specifier == 'B' || specifier == 'b')
                return FormatD(true, '{');
            if (specifier == 'P' || specifier == 'p')
                return FormatD(true, '(');
            if (specifier == 'X' || specifier == 'x')
                return FormatX();
            throw new FormatException();
        }

        public override bool Equals(object value) => value is Guid && Equals((Guid)value);
        public bool Equals(Guid value)
            => _a == value._a && _b == value._b && _c == value._c && _d == value._d && _e == value._e &&
               _f == value._f && _g == value._g && _h == value._h && _i == value._i && _j == value._j && _k == value._k;

        public override int GetHashCode()
            => _a ^ (_b << 16 | (ushort)_c) ^ (_d << 24 | _e << 16 | _f << 8 | _g) ^ (_h << 24 | _i << 16 | _j << 8 | _k);

        public static bool operator ==(Guid left, Guid right) => left.Equals(right);
        public static bool operator !=(Guid left, Guid right) => !left.Equals(right);

        private static Guid FromBytes(byte[] bytes, int offset)
        {
            return new Guid(
                bytes[offset] | bytes[offset + 1] << 8 | bytes[offset + 2] << 16 | bytes[offset + 3] << 24,
                (short)(bytes[offset + 4] | bytes[offset + 5] << 8),
                (short)(bytes[offset + 6] | bytes[offset + 7] << 8),
                bytes[offset + 8], bytes[offset + 9], bytes[offset + 10], bytes[offset + 11],
                bytes[offset + 12], bytes[offset + 13], bytes[offset + 14], bytes[offset + 15]);
        }

        private static bool TryParseN(string value, int start, out Guid result)
        {
            result = Empty;
            byte[] bytes = new byte[16];
            for (int i = 0; i < bytes.Length; i++)
            {
                int high = HexValue(value[start + i * 2]);
                int low = HexValue(value[start + i * 2 + 1]);
                if (high < 0 || low < 0)
                    return false;
                bytes[i] = (byte)(high << 4 | low);
            }

            result = new Guid(
                (int)((uint)bytes[0] << 24 | (uint)bytes[1] << 16 | (uint)bytes[2] << 8 | bytes[3]),
                (short)(bytes[4] << 8 | bytes[5]), (short)(bytes[6] << 8 | bytes[7]),
                bytes[8], bytes[9], bytes[10], bytes[11], bytes[12], bytes[13], bytes[14], bytes[15]);
            return true;
        }

        private static bool TryParseD(string value, int start, out Guid result)
        {
            result = Empty;
            if (value[start + 8] != '-' || value[start + 13] != '-' || value[start + 18] != '-' || value[start + 23] != '-')
                return false;
            if (!TryParseHex(value, start, 8, out uint a) || !TryParseHex(value, start + 9, 4, out uint b) ||
                !TryParseHex(value, start + 14, 4, out uint c) || !TryParseHex(value, start + 19, 4, out uint d) ||
                !TryParseHex(value, start + 24, 12, out ulong tail))
                return false;

            result = new Guid((int)a, (short)b, (short)c, (byte)(d >> 8), (byte)d,
                (byte)(tail >> 40), (byte)(tail >> 32), (byte)(tail >> 24), (byte)(tail >> 16), (byte)(tail >> 8), (byte)tail);
            return true;
        }

        private static bool TryParseX(string value, out Guid result)
        {
            result = Empty;
            int index = 0;
            if (value == null || value.Length < 3 || value[index++] != '{')
                return false;
            if (!ReadLiteral(value, ref index, "0x") || !ReadHex(value, ref index, 8, out uint a) || !ReadLiteral(value, ref index, ",0x") ||
                !ReadHex(value, ref index, 4, out uint b) || !ReadLiteral(value, ref index, ",0x") || !ReadHex(value, ref index, 4, out uint c) ||
                !ReadLiteral(value, ref index, ",{"))
                return false;

            byte[] bytes = new byte[8];
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i != 0 && !ReadLiteral(value, ref index, ",0x"))
                    return false;
                if (!ReadHex(value, ref index, 2, out uint part))
                    return false;
                bytes[i] = (byte)part;
            }
            if (!ReadLiteral(value, ref index, "}}") || index != value.Length)
                return false;
            result = new Guid((int)a, (short)b, (short)c, bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7]);
            return true;
        }

        private string FormatD(bool separators, char wrapper)
        {
            int length = separators ? 36 : 32;
            if (wrapper != '\0') length += 2;
            char[] result = new char[length];
            int index = 0;
            if (wrapper != '\0') result[index++] = wrapper;
            WriteHex(result, ref index, unchecked((uint)_a), 8);
            if (separators) result[index++] = '-';
            WriteHex(result, ref index, (ushort)_b, 4);
            if (separators) result[index++] = '-';
            WriteHex(result, ref index, (ushort)_c, 4);
            if (separators) result[index++] = '-';
            WriteHex(result, ref index, (ushort)(_d << 8 | _e), 4);
            if (separators) result[index++] = '-';
            ulong tail = (ulong)_f << 40 | (ulong)_g << 32 | (ulong)_h << 24 | (ulong)_i << 16 | (ulong)_j << 8 | _k;
            WriteHex(result, ref index, tail, 12);
            if (wrapper != '\0') result[index] = wrapper == '{' ? '}' : ')';
            return new string(result);
        }

        private string FormatX()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('{').Append("0x").Append(Hex(unchecked((uint)_a), 8)).Append(",0x");
            builder.Append(Hex((ushort)_b, 4)).Append(",0x").Append(Hex((ushort)_c, 4)).Append(",{0x");
            byte[] bytes = new byte[] { _f, _g, _h, _i, _j, _k };
            builder.Append(Hex(_d, 2));
            builder.Append(",0x").Append(Hex(_e, 2));
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(",0x").Append(Hex(bytes[i], 2));
            return builder.Append("}}").ToString();
        }

        private static string Hex(ulong value, int digits)
        {
            char[] result = new char[digits];
            for (int i = digits - 1; i >= 0; i--)
            {
                int digit = (int)(value & 0xF);
                result[i] = (char)(digit < 10 ? '0' + digit : 'a' + digit - 10);
                value >>= 4;
            }
            return new string(result);
        }

        private static void WriteHex(char[] result, ref int index, ulong value, int digits)
        {
            for (int i = digits - 1; i >= 0; i--)
            {
                int digit = (int)(value & 0xF);
                result[index + i] = (char)(digit < 10 ? '0' + digit : 'a' + digit - 10);
                value >>= 4;
            }
            index += digits;
        }

        private static bool ReadLiteral(string value, ref int index, string literal)
        {
            if (index + literal.Length > value.Length)
                return false;
            for (int i = 0; i < literal.Length; i++)
                if (value[index + i] != literal[i] && value[index + i] != (literal[i] >= 'a' && literal[i] <= 'z' ? (char)(literal[i] - 32) : literal[i]))
                    return false;
            index += literal.Length;
            return true;
        }

        private static bool ReadHex(string value, ref int index, int digits, out uint result)
        {
            result = 0;
            if (index + digits > value.Length)
                return false;
            for (int i = 0; i < digits; i++)
            {
                int digit = HexValue(value[index++]);
                if (digit < 0) return false;
                result = (result << 4) | (uint)digit;
            }
            return true;
        }

        private static bool TryParseHex(string value, int start, int digits, out uint result)
        {
            result = 0;
            for (int i = 0; i < digits; i++)
            {
                int digit = HexValue(value[start + i]);
                if (digit < 0) return false;
                result = (result << 4) | (uint)digit;
            }
            return true;
        }

        private static bool TryParseHex(string value, int start, int digits, out ulong result)
        {
            result = 0;
            for (int i = 0; i < digits; i++)
            {
                int digit = HexValue(value[start + i]);
                if (digit < 0) return false;
                result = (result << 4) | (uint)digit;
            }
            return true;
        }

        private static int HexValue(char value)
        {
            if (value >= '0' && value <= '9') return value - '0';
            if (value >= 'a' && value <= 'f') return value - 'a' + 10;
            if (value >= 'A' && value <= 'F') return value - 'A' + 10;
            return -1;
        }

        private static bool IsWhiteSpace(char value)
            => value == ' ' || value == '\t' || value == '\r' || value == '\n';

        private static string Slice(string value, int start, int length)
        {
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = value[start + i];
            return new string(result);
        }
    }
}
