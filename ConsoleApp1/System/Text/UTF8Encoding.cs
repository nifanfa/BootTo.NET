namespace System.Text
{
    internal unsafe class UTF8Encoding : Encoding
    {
        private const uint ReplacementCharacter = 0xFFFD;

        public override byte[] GetBytes(string text)
        {
            if (text == null)
                throw new ArgumentNullException();
            if (text.Length == 0)
                return new byte[0];

            int byteCount = 0;
            fixed (char* chars = &text.FirstChar)
            {
                int index = 0;
                while (index < text.Length)
                    byteCount += GetUtf8Length(ReadUtf16(chars, text.Length, ref index));
            }

            byte[] bytes = new byte[byteCount];
            fixed (char* chars = &text.FirstChar)
            fixed (byte* destination = &bytes[0])
            {
                int sourceIndex = 0;
                int destinationIndex = 0;
                while (sourceIndex < text.Length)
                    WriteUtf8(ReadUtf16(chars, text.Length, ref sourceIndex), destination, ref destinationIndex);
            }

            return bytes;
        }

        public override string GetString(ReadOnlySpan<byte> bytes)
        {
            if (bytes.IsEmpty)
                return string.Empty;

            byte* source = (byte*)(void*)bytes;
            int charCount = 0;
            int index = 0;
            while (index < bytes.Length)
                charCount += ReadUtf8(source, bytes.Length, ref index) <= 0xFFFF ? 1 : 2;

            char[] chars = new char[charCount];
            fixed (char* destination = &chars[0])
            {
                int sourceIndex = 0;
                int destinationIndex = 0;
                while (sourceIndex < bytes.Length)
                {
                    uint codePoint = ReadUtf8(source, bytes.Length, ref sourceIndex);
                    if (codePoint <= 0xFFFF)
                    {
                        destination[destinationIndex++] = (char)codePoint;
                    }
                    else
                    {
                        codePoint -= 0x10000;
                        destination[destinationIndex++] = (char)(0xD800 + (codePoint >> 10));
                        destination[destinationIndex++] = (char)(0xDC00 + (codePoint & 0x3FF));
                    }
                }
            }

            return new string(chars);
        }

        private static uint ReadUtf16(char* chars, int length, ref int index)
        {
            uint first = chars[index++];
            if (first < 0xD800 || first > 0xDFFF)
                return first;
            if (first > 0xDBFF || index >= length)
                return ReplacementCharacter;

            uint second = chars[index];
            if (second < 0xDC00 || second > 0xDFFF)
                return ReplacementCharacter;

            index++;
            return 0x10000 + ((first - 0xD800) << 10) + second - 0xDC00;
        }

        private static int GetUtf8Length(uint codePoint)
        {
            if (codePoint <= 0x7F)
                return 1;
            if (codePoint <= 0x7FF)
                return 2;
            if (codePoint <= 0xFFFF)
                return 3;
            return 4;
        }

        private static void WriteUtf8(uint codePoint, byte* destination, ref int index)
        {
            if (codePoint <= 0x7F)
            {
                destination[index++] = (byte)codePoint;
            }
            else if (codePoint <= 0x7FF)
            {
                destination[index++] = (byte)(0xC0 | (codePoint >> 6));
                destination[index++] = (byte)(0x80 | (codePoint & 0x3F));
            }
            else if (codePoint <= 0xFFFF)
            {
                destination[index++] = (byte)(0xE0 | (codePoint >> 12));
                destination[index++] = (byte)(0x80 | ((codePoint >> 6) & 0x3F));
                destination[index++] = (byte)(0x80 | (codePoint & 0x3F));
            }
            else
            {
                destination[index++] = (byte)(0xF0 | (codePoint >> 18));
                destination[index++] = (byte)(0x80 | ((codePoint >> 12) & 0x3F));
                destination[index++] = (byte)(0x80 | ((codePoint >> 6) & 0x3F));
                destination[index++] = (byte)(0x80 | (codePoint & 0x3F));
            }
        }

        private static uint ReadUtf8(byte* source, int length, ref int index)
        {
            byte first = source[index++];
            if (first <= 0x7F)
                return first;

            if (first >= 0xC2 && first <= 0xDF && index < length)
            {
                byte second = source[index];
                if (IsContinuation(second))
                {
                    index++;
                    return (uint)(((first & 0x1F) << 6) | (second & 0x3F));
                }
            }
            else if (first >= 0xE0 && first <= 0xEF && index + 1 < length)
            {
                byte second = source[index];
                byte third = source[index + 1];
                bool validSecond = IsContinuation(second) &&
                    (first != 0xE0 || second >= 0xA0) &&
                    (first != 0xED || second <= 0x9F);
                if (validSecond && IsContinuation(third))
                {
                    index += 2;
                    return (uint)(((first & 0x0F) << 12) | ((second & 0x3F) << 6) | (third & 0x3F));
                }
            }
            else if (first >= 0xF0 && first <= 0xF4 && index + 2 < length)
            {
                byte second = source[index];
                byte third = source[index + 1];
                byte fourth = source[index + 2];
                bool validSecond = IsContinuation(second) &&
                    (first != 0xF0 || second >= 0x90) &&
                    (first != 0xF4 || second <= 0x8F);
                if (validSecond && IsContinuation(third) && IsContinuation(fourth))
                {
                    index += 3;
                    return (uint)(((first & 0x07) << 18) | ((second & 0x3F) << 12) |
                        ((third & 0x3F) << 6) | (fourth & 0x3F));
                }
            }

            return ReplacementCharacter;
        }

        private static bool IsContinuation(byte value)
            => (value & 0xC0) == 0x80;
    }
}
