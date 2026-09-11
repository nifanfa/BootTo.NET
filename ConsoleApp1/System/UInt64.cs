namespace System
{
    public partial struct UInt64
    {
        public static ulong Parse(string value)
        {
            if (!TryParse(value, out ulong result)) throw new FormatException("The value is not a valid UInt64.");
            return result;
        }

        public static bool TryParse(string value, out ulong result)
            => Number.TryParseUnsigned(value, out result);
    }
}
