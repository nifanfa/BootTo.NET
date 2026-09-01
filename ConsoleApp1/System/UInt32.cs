namespace System
{
    public partial struct UInt32
    {
        public static uint Parse(string value)
        {
            if (!TryParse(value, out uint result)) throw new FormatException("The value is not a valid UInt32.");
            return result;
        }

        public static bool TryParse(string value, out uint result)
        {
            result = 0;
            return Number.TryParseUnsigned(value, out ulong parsed) &&
                parsed <= uint.MaxValue && (result = (uint)parsed) == parsed;
        }
    }
}
