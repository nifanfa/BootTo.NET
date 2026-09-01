namespace System
{
    public partial struct UInt16
    {
        public static ushort Parse(string value)
        {
            if (!TryParse(value, out ushort result)) throw new FormatException("The value is not a valid UInt16.");
            return result;
        }

        public static bool TryParse(string value, out ushort result)
        {
            result = 0;
            return Number.TryParseUnsigned(value, out ulong parsed) &&
                parsed <= ushort.MaxValue && (result = (ushort)parsed) == parsed;
        }
    }
}
