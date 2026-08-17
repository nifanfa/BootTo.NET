namespace System.Text
{
    internal class UTF8Encoding : Encoding
    {
        public override string GetString(ReadOnlySpan<byte> utf8Bytes)
        {
            if (utf8Bytes.IsEmpty)
                return string.Empty;

            int length = utf8Bytes.Length;
            int charCount = 0;
            int i = 0;

            while (i < length)
            {
                byte b0 = utf8Bytes[i];
                if ((b0 & 0x80) == 0)
                {
                    i += 1;
                    charCount += 1;
                }
                else if ((b0 & 0xE0) == 0xC0)
                {
                    i += 2;
                    charCount += 1;
                }
                else if ((b0 & 0xF0) == 0xE0)
                {
                    i += 3;
                    charCount += 1;
                }
                else if ((b0 & 0xF8) == 0xF0)
                {
                    i += 4;
                    charCount += 2;
                }
                else
                {
                    i += 1;
                    charCount += 1;
                }
            }

            char[] chars = new char[charCount];
            int cIndex = 0;
            i = 0;

            while (i < length)
            {
                byte b0 = utf8Bytes[i];
                int codePoint = 0;

                if ((b0 & 0x80) == 0)
                {
                    codePoint = b0;
                    i += 1;
                }
                else if ((b0 & 0xE0) == 0xC0)
                {
                    byte b1 = utf8Bytes[i + 1];
                    codePoint = ((b0 & 0x1F) << 6) | (b1 & 0x3F);
                    i += 2;
                }
                else if ((b0 & 0xF0) == 0xE0)
                {
                    byte b1 = utf8Bytes[i + 1];
                    byte b2 = utf8Bytes[i + 2];
                    codePoint = ((b0 & 0x0F) << 12) | ((b1 & 0x3F) << 6) | (b2 & 0x3F);
                    i += 3;
                }
                else if ((b0 & 0xF8) == 0xF0)
                {
                    byte b1 = utf8Bytes[i + 1];
                    byte b2 = utf8Bytes[i + 2];
                    byte b3 = utf8Bytes[i + 3];
                    codePoint = ((b0 & 0x07) << 18) | ((b1 & 0x3F) << 12) | ((b2 & 0x3F) << 6) | (b3 & 0x3F);
                    i += 4;
                }
                else
                {
                    codePoint = b0;
                    i += 1;
                }

                if (codePoint <= 0xFFFF)
                {
                    chars[cIndex++] = (char)codePoint;
                }
                else
                {
                    codePoint -= 0x10000;
                    chars[cIndex++] = (char)(0xD800 + (codePoint >> 10));
                    chars[cIndex++] = (char)(0xDC00 + (codePoint & 0x3FF));
                }
            }

            return string.Ctor(chars);
        }
    }
}
