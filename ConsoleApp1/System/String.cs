namespace System
{
    public partial class String
    {
        public string Substring(int startIndex)
            => Substring(startIndex, Length - startIndex);

        public string Substring(int startIndex, int length)
        {
            if (startIndex < 0 || length < 0 || startIndex > Length - length)
                throw new ArgumentException("The substring start index and length are outside the string.");
            if (length == 0)
                return Empty;

            char[] result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = this[startIndex + i];
            return new string(result);
        }

        public bool StartsWith(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The value cannot be null.");
            if (value.Length > Length)
                return false;
            for (int i = 0; i < value.Length; i++)
                if (this[i] != value[i])
                    return false;
            return true;
        }

        public bool EndsWith(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The value cannot be null.");
            if (value.Length > Length)
                return false;
            int start = Length - value.Length;
            for (int i = 0; i < value.Length; i++)
                if (this[start + i] != value[i])
                    return false;
            return true;
        }

        public bool Contains(char value) => IndexOf(value) >= 0;
        public bool Contains(string value) => IndexOf(value) >= 0;

        public int IndexOf(char value) => IndexOf(value, 0);

        public int IndexOf(char value, int startIndex)
        {
            if (startIndex < 0 || startIndex > Length)
                throw new ArgumentOutOfRangeException("The start index is outside the string.");
            for (int i = startIndex; i < Length; i++)
                if (this[i] == value)
                    return i;
            return -1;
        }

        public int IndexOf(string value) => IndexOf(value, 0);

        public int IndexOf(string value, int startIndex)
        {
            if (value == null)
                throw new ArgumentNullException("The value cannot be null.");
            if (startIndex < 0 || startIndex > Length)
                throw new ArgumentOutOfRangeException("The start index is outside the string.");
            if (value.Length == 0)
                return startIndex;
            for (int i = startIndex; i <= Length - value.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < value.Length; j++)
                    if (this[i + j] != value[j])
                    {
                        match = false;
                        break;
                    }
                if (match)
                    return i;
            }
            return -1;
        }

        public int LastIndexOf(char value)
        {
            for (int i = Length - 1; i >= 0; i--)
                if (this[i] == value)
                    return i;
            return -1;
        }

        public int LastIndexOf(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The value cannot be null.");
            if (value.Length == 0)
                return Length;
            for (int i = Length - value.Length; i >= 0; i--)
            {
                bool match = true;
                for (int j = 0; j < value.Length; j++)
                    if (this[i + j] != value[j])
                    {
                        match = false;
                        break;
                    }
                if (match)
                    return i;
            }
            return -1;
        }

        public string Trim() => TrimWhitespace(true, true);
        public string TrimStart() => TrimWhitespace(true, false);
        public string TrimEnd() => TrimWhitespace(false, true);

        public string Trim(params char[] trimChars)
        {
            if (trimChars == null || trimChars.Length == 0)
                return Trim();
            int start = 0;
            int end = Length;
            while (start < end && ContainsChar(trimChars, this[start])) start++;
            while (end > start && ContainsChar(trimChars, this[end - 1])) end--;
            return Substring(start, end - start);
        }

        public string Replace(char oldChar, char newChar)
        {
            if (oldChar == newChar)
                return this;
            char[] result = new char[Length];
            for (int i = 0; i < Length; i++)
                result[i] = this[i] == oldChar ? newChar : this[i];
            return new string(result);
        }

        public string Replace(string oldValue, string newValue)
        {
            if (oldValue == null)
                throw new ArgumentNullException("The old value cannot be null.");
            if (oldValue.Length == 0)
                throw new ArgumentException("The old value cannot be empty.");
            newValue = newValue ?? Empty;

            Text.StringBuilder result = new Text.StringBuilder(Length);
            int position = 0;
            while (position < Length)
            {
                int match = IndexOf(oldValue, position);
                if (match < 0)
                {
                    result.Append(Substring(position));
                    position = Length;
                    break;
                }
                result.Append(Substring(position, match - position));
                result.Append(newValue);
                position = match + oldValue.Length;
            }
            return Length == 0 ? this : result.ToString();
        }

        public string ToLowerInvariant() => ConvertAsciiCase(false);
        public string ToUpperInvariant() => ConvertAsciiCase(true);

        public string[] Split(params char[] separator)
            => Split(separator, StringSplitOptions.None);

        public string[] Split(char separator)
            => Split(new char[] { separator }, StringSplitOptions.None);

        public string[] Split(char[] separator, StringSplitOptions options)
        {
            if ((options & ~(StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) != 0)
                throw new ArgumentException("The string split options are invalid.");

            System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();
            int start = 0;
            for (int i = 0; i <= Length; i++)
            {
                bool atSeparator = i == Length || IsSplitSeparator(separator, this[i]);
                if (!atSeparator)
                    continue;

                string part = Substring(start, i - start);
                if ((options & StringSplitOptions.TrimEntries) != 0)
                    part = part.Trim();
                if (part.Length != 0 || (options & StringSplitOptions.RemoveEmptyEntries) == 0)
                    result.Add(part);
                start = i + 1;
            }
            return result.ToArray();
        }

        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
                return true;
            for (int i = 0; i < value.Length; i++)
                if (!IsWhiteSpace(value[i]))
                    return false;
            return true;
        }

        public static int CompareOrdinal(string left, string right)
        {
            if (ReferenceEquals(left, right))
                return 0;
            if (left == null)
                return -1;
            if (right == null)
                return 1;
            int length = left.Length < right.Length ? left.Length : right.Length;
            for (int i = 0; i < length; i++)
                if (left[i] != right[i])
                    return left[i] < right[i] ? -1 : 1;
            return left.Length < right.Length ? -1 : (left.Length > right.Length ? 1 : 0);
        }

        public static string Join(string separator, string[] values)
        {
            if (values == null)
                throw new ArgumentNullException("The values cannot be null.");
            Text.StringBuilder result = new Text.StringBuilder();
            separator = separator ?? Empty;
            for (int i = 0; i < values.Length; i++)
            {
                if (i != 0)
                    result.Append(separator);
                result.Append(values[i]);
            }
            return result.ToString();
        }

        public static string Join<T>(string separator, System.Collections.Generic.IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException("The values cannot be null.");
            Text.StringBuilder result = new Text.StringBuilder();
            separator = separator ?? Empty;
            int index = 0;
            foreach (T value in values)
            {
                if (index++ != 0)
                    result.Append(separator);
                result.Append(Convert.ToString(value));
            }
            return result.ToString();
        }

        public static string Format(string format, object arg0)
            => Format(format, new object[] { arg0 });

        public static string Format(string format, object arg0, object arg1)
            => Format(format, new object[] { arg0, arg1 });

        public static string Format(string format, object arg0, object arg1, object arg2)
            => Format(format, new object[] { arg0, arg1, arg2 });

        public static string Format(string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("The format string cannot be null.");
            if (args == null)
                throw new ArgumentNullException("The format arguments cannot be null.");

            Text.StringBuilder result = new Text.StringBuilder(format.Length + 16);
            int index = 0;
            while (index < format.Length)
            {
                char current = format[index++];
                if (current == '{')
                {
                    if (index < format.Length && format[index] == '{')
                    {
                        result.Append('{');
                        index++;
                        continue;
                    }

                    int argumentIndex = 0;
                    int digits = 0;
                    while (index < format.Length && format[index] >= '0' && format[index] <= '9')
                    {
                        argumentIndex = argumentIndex * 10 + (format[index++] - '0');
                        digits++;
                    }

                    if (digits == 0 || argumentIndex >= args.Length)
                        throw new FormatException("The format contains an invalid or out-of-range argument index.");

                    while (index < format.Length && format[index] == ' ')
                        index++;

                    string specifier = null;
                    if (index < format.Length && format[index] == ':')
                    {
                        index++;
                        int specifierStart = index;
                        while (index < format.Length && format[index] != '}')
                            index++;
                        if (index >= format.Length)
                            throw new FormatException("The format item is missing its closing brace.");
                        if (index > specifierStart)
                        {
                            char[] specifierChars = new char[index - specifierStart];
                            for (int i = 0; i < specifierChars.Length; i++)
                                specifierChars[i] = format[specifierStart + i];
                            specifier = new string(specifierChars);
                        }
                    }

                    if (index >= format.Length || format[index] != '}')
                        throw new FormatException("The format item is missing its closing brace.");
                    index++;
                    result.Append(FormatValue(args[argumentIndex], specifier));
                    continue;
                }

                if (current == '}')
                {
                    if (index < format.Length && format[index] == '}')
                    {
                        result.Append('}');
                        index++;
                        continue;
                    }
                    throw new FormatException("The format contains an unmatched closing brace.");
                }

                result.Append(current);
            }

            return result.ToString();
        }

        private static string FormatValue(object value, string specifier)
        {
            if (value == null)
                return Empty;
            if (IsNullOrEmpty(specifier))
                return value.ToString();

            char type = specifier[0];
            if (type == 'x' || type == 'X')
            {
                int width = 0;
                for (int i = 1; i < specifier.Length; i++)
                {
                    if (specifier[i] < '0' || specifier[i] > '9')
                        throw new FormatException("The numeric format width is invalid.");
                    width = width * 10 + (specifier[i] - '0');
                }

                if (value is byte)
                    return FormatUnsigned((byte)value, type == 'X', width);
                if (value is ushort)
                    return FormatUnsigned((ushort)value, type == 'X', width);
                if (value is uint)
                    return FormatUnsigned((uint)value, type == 'X', width);
                if (value is ulong)
                    return FormatUnsigned((ulong)value, type == 'X', width);
                if (value is sbyte)
                    return FormatUnsigned(unchecked((byte)(sbyte)value), type == 'X', width < 2 ? 2 : width);
                if (value is short)
                    return FormatUnsigned(unchecked((ushort)(short)value), type == 'X', width < 4 ? 4 : width);
                if (value is int)
                    return FormatUnsigned(unchecked((uint)(int)value), type == 'X', width < 8 ? 8 : width);
                if (value is long)
                    return FormatUnsigned(unchecked((ulong)(long)value), type == 'X', width < 16 ? 16 : width);
            }

            if (type == 'd' || type == 'D')
            {
                int width = 0;
                for (int i = 1; i < specifier.Length; i++)
                {
                    if (specifier[i] < '0' || specifier[i] > '9')
                        throw new FormatException("The numeric format width is invalid.");
                    width = width * 10 + (specifier[i] - '0');
                }
                string text = value.ToString();
                Text.StringBuilder padded = new Text.StringBuilder(text.Length + width);
                int start = text.Length > 0 && text[0] == '-' ? 1 : 0;
                if (start != 0)
                {
                    padded.Append('-');
                    for (int i = text.Length; i < width + start; i++)
                        padded.Append('0');
                    for (int i = 1; i < text.Length; i++)
                        padded.Append(text[i]);
                }
                else
                {
                    for (int i = text.Length; i < width; i++)
                        padded.Append('0');
                    padded.Append(text);
                }
                return padded.ToString();
            }

            throw new FormatException("The format specifier is not supported.");
        }

        private static string FormatUnsigned(ulong value, bool upper, int width)
        {
            char[] digits = new char[32];
            int position = digits.Length;
            do
            {
                int digit = (int)(value & 0xF);
                digits[--position] = (char)(digit < 10 ? '0' + digit : (upper ? 'A' : 'a') + digit - 10);
                value >>= 4;
            }
            while (value != 0);

            int count = digits.Length - position;
            int total = count > width ? count : width;
            char[] result = new char[total];
            int padding = total - count;
            for (int i = 0; i < padding; i++)
                result[i] = '0';
            for (int i = 0; i < count; i++)
                result[padding + i] = digits[position + i];
            return new string(result);
        }

        private string TrimWhitespace(bool trimStart, bool trimEnd)
        {
            int start = 0;
            int end = Length;
            if (trimStart)
                while (start < end && IsWhiteSpace(this[start])) start++;
            if (trimEnd)
                while (end > start && IsWhiteSpace(this[end - 1])) end--;
            return Substring(start, end - start);
        }

        private static bool ContainsChar(char[] values, char value)
        {
            for (int i = 0; i < values.Length; i++)
                if (values[i] == value)
                    return true;
            return false;
        }

        private static bool IsSplitSeparator(char[] separators, char value)
        {
            if (separators == null || separators.Length == 0)
                return IsWhiteSpace(value);
            return ContainsChar(separators, value);
        }

        private string ConvertAsciiCase(bool upper)
        {
            char[] result = new char[Length];
            for (int i = 0; i < Length; i++)
            {
                char value = this[i];
                if (!upper && value >= 'A' && value <= 'Z')
                    value = (char)(value + ('a' - 'A'));
                else if (upper && value >= 'a' && value <= 'z')
                    value = (char)(value - ('a' - 'A'));
                result[i] = value;
            }
            return new string(result);
        }

        private static bool IsWhiteSpace(char value)
            => value == ' ' || value == '\t' || value == '\r' || value == '\n' ||
               value == '\f' || value == '\v';
    }
}
