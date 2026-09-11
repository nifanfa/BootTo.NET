using System.Text;

namespace System
{
    public partial struct Single
    {
        public override unsafe string ToString()
        {
            byte[] buffer = new byte[32];
            byte[] format = new byte[] { (byte)'%', (byte)'f', 0 };
            fixed (byte* pointer = buffer)
            fixed (byte* formatPointer = format)
            {
                int length = snprintf(pointer, buffer.Length, formatPointer, this);
                return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, length));
            }
        }

        public static float Parse(string value)
        {
            if (!TryParse(value, out float result)) throw new FormatException("The value is not a valid Single.");
            return result;
        }

        public static bool TryParse(string value, out float result)
        {
            result = 0;
            return Number.TryParseDouble(value, out double parsed) && (result = (float)parsed) == (float)parsed;
        }
    }
}
