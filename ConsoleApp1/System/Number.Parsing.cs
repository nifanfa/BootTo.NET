namespace System
{
    internal static class Number
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
}
