namespace System
{
    public partial struct Byte
    {
        public static byte Parse(string value)
        {
            if (!TryParse(value, out byte result)) throw new FormatException("The value is not a valid Byte.");
            return result;
        }

        public static bool TryParse(string value, out byte result)
        {
            result = 0;
            return Number.TryParseUnsigned(value, out ulong parsed) &&
                parsed <= byte.MaxValue && (result = (byte)parsed) == parsed;
        }
    }
}
