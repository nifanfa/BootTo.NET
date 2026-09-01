using System.Text;

namespace System
{
    public partial struct Int64
    {
        public override unsafe string ToString()
        {
            byte[] buffer = new byte[32];
            fixed (byte* ptr = buffer)
            {
                int length = snprintf(ptr, buffer.Length, "%lld"u8, this);
                return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, length));
            }
        }

        public static long Parse(string value)
        {
            if (!TryParse(value, out long result)) throw new FormatException("The value is not a valid Int64.");
            return result;
        }

        public static bool TryParse(string value, out long result)
            => Number.TryParseSigned(value, out result);
    }
}
