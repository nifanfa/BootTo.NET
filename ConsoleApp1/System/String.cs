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
    }
}
