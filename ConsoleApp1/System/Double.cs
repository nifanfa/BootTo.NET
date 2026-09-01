using System.Text;

namespace System
{
    public partial struct Double
    {
        public override unsafe string ToString()
        {
            byte[] buffer = new byte[32];
            fixed (byte* ptr = buffer)
            {
                int length = snprintf(ptr, buffer.Length, "%lf"u8, this);
                return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, length));
            }
        }

        public static double Parse(string value)
        {
            if (!TryParse(value, out double result)) throw new FormatException("The value is not a valid Double.");
            return result;
        }

        public static bool TryParse(string value, out double result)
            => Number.TryParseDouble(value, out result);
    }
}
