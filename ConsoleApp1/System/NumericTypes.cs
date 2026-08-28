namespace System
{
    internal static class NumericParser
    {
        internal static bool TryParseSigned(string text, out long result)
        {
            result = 0;
            if (text == null)
                return false;

            int index = SkipWhiteSpace(text, 0);
            bool negative = false;
            if (index < text.Length && (text[index] == '+' || text[index] == '-'))
            {
                negative = text[index] == '-';
                index++;
            }
            if (index == text.Length)
                return false;

            ulong limit = negative ? 0x8000000000000000UL : 0x7FFFFFFFFFFFFFFFUL;
            ulong value = 0;
            int digits = 0;
            while (index < text.Length && text[index] >= '0' && text[index] <= '9')
            {
                uint digit = (uint)(text[index++] - '0');
                if (value > (limit - digit) / 10)
                    return false;
                value = value * 10 + digit;
                digits++;
            }

            if (digits == 0 || SkipWhiteSpace(text, index) != text.Length)
                return false;
            result = negative
                ? (value == 0x8000000000000000UL ? long.MinValue : -(long)value)
                : (long)value;
            return true;
        }

        internal static bool TryParseUnsigned(string text, out ulong result)
        {
            result = 0;
            if (text == null)
                return false;

            int index = SkipWhiteSpace(text, 0);
            if (index < text.Length && text[index] == '+')
                index++;
            if (index == text.Length)
                return false;

            int digits = 0;
            while (index < text.Length && text[index] >= '0' && text[index] <= '9')
            {
                uint digit = (uint)(text[index++] - '0');
                if (result > (ulong.MaxValue - digit) / 10)
                    return false;
                result = result * 10 + digit;
                digits++;
            }

            return digits != 0 && SkipWhiteSpace(text, index) == text.Length;
        }

        internal static bool TryParseDouble(string text, out double result)
        {
            result = 0;
            if (text == null)
                return false;

            int index = SkipWhiteSpace(text, 0);
            bool negative = false;
            if (index < text.Length && (text[index] == '+' || text[index] == '-'))
            {
                negative = text[index] == '-';
                index++;
            }

            int digits = 0;
            while (index < text.Length && text[index] >= '0' && text[index] <= '9')
            {
                result = result * 10 + (text[index++] - '0');
                digits++;
            }
            if (index < text.Length && text[index] == '.')
            {
                index++;
                double place = 0.1;
                while (index < text.Length && text[index] >= '0' && text[index] <= '9')
                {
                    result += (text[index++] - '0') * place;
                    place *= 0.1;
                    digits++;
                }
            }

            if (digits == 0)
                return false;

            int exponent = 0;
            bool exponentNegative = false;
            if (index < text.Length && (text[index] == 'e' || text[index] == 'E'))
            {
                index++;
                if (index < text.Length && (text[index] == '+' || text[index] == '-'))
                {
                    exponentNegative = text[index] == '-';
                    index++;
                }
                int exponentDigits = 0;
                while (index < text.Length && text[index] >= '0' && text[index] <= '9')
                {
                    if (exponent < 10000)
                        exponent = exponent * 10 + text[index] - '0';
                    index++;
                    exponentDigits++;
                }
                if (exponentDigits == 0)
                    return false;
            }

            if (SkipWhiteSpace(text, index) != text.Length)
                return false;
            while (exponent-- > 0)
                result = exponentNegative ? result * 0.1 : result * 10;
            if (negative)
                result = -result;
            return !Math.IsNaN(result) && !Math.IsInfinity(result);
        }

        private static int SkipWhiteSpace(string value, int index)
        {
            while (index < value.Length &&
                (value[index] == ' ' || value[index] == '\t' || value[index] == '\r' ||
                 value[index] == '\n' || value[index] == '\f' || value[index] == '\v'))
                index++;
            return index;
        }
    }

    public partial struct SByte
    {
        public static sbyte Parse(string value)
        {
            if (!TryParse(value, out sbyte result)) throw new FormatException("The value is not a valid SByte.");
            return result;
        }

        public static bool TryParse(string value, out sbyte result)
        {
            result = 0;
            return NumericParser.TryParseSigned(value, out long parsed) &&
                parsed >= sbyte.MinValue && parsed <= sbyte.MaxValue && (result = (sbyte)parsed) == parsed;
        }
    }

    public partial struct Byte
    {
        public static byte Parse(string value)
        {
            if (!TryParse(value, out byte result)) throw new FormatException("The value is not a valid Byte.");
            return result;
        }

        public static bool TryParse(string value, out byte result)
        {
            result = 0;
            return NumericParser.TryParseUnsigned(value, out ulong parsed) &&
                parsed <= byte.MaxValue && (result = (byte)parsed) == parsed;
        }
    }

    public partial struct Int16
    {
        public static short Parse(string value)
        {
            if (!TryParse(value, out short result)) throw new FormatException("The value is not a valid Int16.");
            return result;
        }

        public static bool TryParse(string value, out short result)
        {
            result = 0;
            return NumericParser.TryParseSigned(value, out long parsed) &&
                parsed >= short.MinValue && parsed <= short.MaxValue && (result = (short)parsed) == parsed;
        }
    }

    public partial struct UInt16
    {
        public static ushort Parse(string value)
        {
            if (!TryParse(value, out ushort result)) throw new FormatException("The value is not a valid UInt16.");
            return result;
        }

        public static bool TryParse(string value, out ushort result)
        {
            result = 0;
            return NumericParser.TryParseUnsigned(value, out ulong parsed) &&
                parsed <= ushort.MaxValue && (result = (ushort)parsed) == parsed;
        }
    }

    public partial struct Int32
    {
        public static int Parse(string value)
        {
            if (!TryParse(value, out int result)) throw new FormatException("The value is not a valid Int32.");
            return result;
        }

        public static bool TryParse(string value, out int result)
        {
            result = 0;
            return NumericParser.TryParseSigned(value, out long parsed) &&
                parsed >= int.MinValue && parsed <= int.MaxValue && (result = (int)parsed) == parsed;
        }
    }

    public partial struct UInt32
    {
        public static uint Parse(string value)
        {
            if (!TryParse(value, out uint result)) throw new FormatException("The value is not a valid UInt32.");
            return result;
        }

        public static bool TryParse(string value, out uint result)
        {
            result = 0;
            return NumericParser.TryParseUnsigned(value, out ulong parsed) &&
                parsed <= uint.MaxValue && (result = (uint)parsed) == parsed;
        }
    }

    public partial struct Int64
    {
        public static long Parse(string value)
        {
            if (!TryParse(value, out long result)) throw new FormatException("The value is not a valid Int64.");
            return result;
        }

        public static bool TryParse(string value, out long result)
            => NumericParser.TryParseSigned(value, out result);
    }

    public partial struct UInt64
    {
        public static ulong Parse(string value)
        {
            if (!TryParse(value, out ulong result)) throw new FormatException("The value is not a valid UInt64.");
            return result;
        }

        public static bool TryParse(string value, out ulong result)
            => NumericParser.TryParseUnsigned(value, out result);
    }

    public partial struct Single
    {
        public static float Parse(string value)
        {
            if (!TryParse(value, out float result)) throw new FormatException("The value is not a valid Single.");
            return result;
        }

        public static bool TryParse(string value, out float result)
        {
            result = 0;
            return NumericParser.TryParseDouble(value, out double parsed) && (result = (float)parsed) == (float)parsed;
        }
    }

    public partial struct Double
    {
        public static double Parse(string value)
        {
            if (!TryParse(value, out double result)) throw new FormatException("The value is not a valid Double.");
            return result;
        }

        public static bool TryParse(string value, out double result)
            => NumericParser.TryParseDouble(value, out result);
    }
}
