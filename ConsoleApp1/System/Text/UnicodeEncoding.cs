namespace System.Text
{
    public sealed class UnicodeEncoding : Encoding
    {
        private readonly bool _byteOrderMark;

        public UnicodeEncoding()
            : this(false, false)
        {
        }

        public UnicodeEncoding(bool bigEndian, bool byteOrderMark)
        {
            if (bigEndian)
                throw new NotSupportedException("Big-endian Unicode encoding is not supported.");
            _byteOrderMark = byteOrderMark;
        }

        public UnicodeEncoding(bool bigEndian, bool byteOrderMark, bool throwOnInvalidBytes)
            : this(bigEndian, byteOrderMark)
        {
        }

        public override int CodePage => 1200;

        public override byte[] GetBytes(string text)
        {
            if (text == null)
                throw new ArgumentNullException("The text to encode cannot be null.");

            byte[] result = new byte[text.Length * 2];
            for (int i = 0; i < text.Length; i++)
            {
                result[i * 2] = (byte)text[i];
                result[i * 2 + 1] = (byte)(text[i] >> 8);
            }
            return result;
        }

        public override int GetByteCount(string text)
        {
            if (text == null)
                throw new ArgumentNullException("The text to encode cannot be null.");
            return text.Length * 2;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            ValidateBytes(bytes, index, count);
            return (count + 1) / 2;
        }

        public override string GetString(ReadOnlySpan<byte> bytes)
        {
            byte[] data = bytes;
            char[] result = new char[(data.Length + 1) / 2];
            for (int i = 0; i < result.Length; i++)
            {
                int offset = i * 2;
                byte low = data[offset];
                byte high = offset + 1 < data.Length ? data[offset + 1] : (byte)0;
                result[i] = (char)(low | (high << 8));
            }
            return new string(result);
        }

        public override byte[] GetPreamble()
            => _byteOrderMark ? new byte[] { 0xFF, 0xFE } : new byte[0];
    }
}
