namespace System.Text
{
    public sealed class ASCIIEncoding : Encoding
    {
        public override int CodePage => 20127;

        public override byte[] GetBytes(string text)
        {
            if (text == null)
                throw new ArgumentNullException("The text to encode cannot be null.");

            byte[] result = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
                result[i] = text[i] <= 0x7F ? (byte)text[i] : (byte)'?';
            return result;
        }

        public override int GetByteCount(string text)
        {
            if (text == null)
                throw new ArgumentNullException("The text to encode cannot be null.");
            return text.Length;
        }

        public override string GetString(ReadOnlySpan<byte> bytes)
        {
            byte[] data = bytes;
            char[] result = new char[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = data[i] <= 0x7F ? (char)data[i] : '?';
            return new string(result);
        }
    }
}
