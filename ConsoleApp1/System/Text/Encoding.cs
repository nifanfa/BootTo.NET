namespace System.Text
{
    public abstract class Encoding
    {
        public static readonly Encoding UTF8 = new UTF8Encoding();
        public static readonly Encoding ASCII = new ASCIIEncoding();
        public static readonly Encoding Unicode = new UnicodeEncoding();

        public virtual int CodePage => 0;

        public virtual string GetString(ReadOnlySpan<byte> bytes)
        {
            byte[] data = bytes;
            return GetString(data, 0, data.Length);
        }

        public virtual string GetString(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("The byte buffer cannot be null.");
            return GetString((ReadOnlySpan<byte>)bytes);
        }

        public virtual string GetString(byte[] bytes, int index, int count)
        {
            ValidateBytes(bytes, index, count);
            if (index == 0 && count == bytes.Length)
                return GetString(bytes);

            byte[] data = new byte[count];
            for (int i = 0; i < count; i++)
                data[i] = bytes[index + i];
            return GetString(data);
        }

        public virtual byte[] GetBytes(string text) => throw new NotSupportedException("This encoding does not implement text encoding.");

        public virtual int GetByteCount(string text)
        {
            if (text == null)
                throw new ArgumentNullException("The text to encode cannot be null.");
            return GetBytes(text).Length;
        }

        public virtual int GetByteCount(char[] chars)
        {
            if (chars == null)
                throw new ArgumentNullException("The character buffer cannot be null.");
            return GetByteCount(new string(chars));
        }

        public virtual int GetByteCount(char[] chars, int index, int count)
        {
            ValidateChars(chars, index, count);
            return GetByteCount(Slice(chars, index, count));
        }

        public virtual int GetCharCount(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("The byte buffer cannot be null.");
            return GetString(bytes).Length;
        }

        public virtual int GetCharCount(byte[] bytes, int index, int count)
        {
            ValidateBytes(bytes, index, count);
            return GetString(bytes, index, count).Length;
        }

        public virtual int GetBytes(string text, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if (text == null)
                throw new ArgumentNullException("The text to encode cannot be null.");
            if (charIndex < 0 || charCount < 0 || charIndex > text.Length - charCount)
                throw new ArgumentException("The character range is invalid.");
            if (bytes == null)
                throw new ArgumentNullException("The destination byte buffer cannot be null.");

            byte[] data = GetBytes(charCount == text.Length && charIndex == 0
                ? text
                : text.Substring(charIndex, charCount));
            if (byteIndex < 0 || byteIndex > bytes.Length - data.Length)
                throw new ArgumentException("The destination byte range is invalid.");
            for (int i = 0; i < data.Length; i++)
                bytes[byteIndex + i] = data[i];
            return data.Length;
        }

        public virtual int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            ValidateChars(chars, charIndex, charCount);
            return GetBytes(Slice(chars, charIndex, charCount), 0, charCount, bytes, byteIndex);
        }

        public virtual byte[] GetBytes(char[] chars)
        {
            if (chars == null)
                throw new ArgumentNullException("The character buffer cannot be null.");
            return GetBytes(new string(chars));
        }

        public virtual byte[] GetPreamble() => new byte[0];

        protected static void ValidateBytes(byte[] bytes, int index, int count)
        {
            if (bytes == null)
                throw new ArgumentNullException("The byte buffer cannot be null.");
            if (index < 0 || count < 0 || index > bytes.Length - count)
                throw new ArgumentException("The byte range is invalid.");
        }

        protected static void ValidateChars(char[] chars, int index, int count)
        {
            if (chars == null)
                throw new ArgumentNullException("The character buffer cannot be null.");
            if (index < 0 || count < 0 || index > chars.Length - count)
                throw new ArgumentException("The character range is invalid.");
        }

        private static string Slice(char[] chars, int index, int count)
        {
            char[] result = new char[count];
            for (int i = 0; i < count; i++)
                result[i] = chars[index + i];
            return new string(result);
        }
    }
}
