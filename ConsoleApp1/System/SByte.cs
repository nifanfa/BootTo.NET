namespace System
{
    public partial struct SByte
    {
        public static sbyte Parse(string value)
        {
            if (!TryParse(value, out sbyte result)) throw new FormatException("The value is not a valid SByte.");
            return result;
        }

        public static bool TryParse(string value, out sbyte result)
        {
            result = 0;
            return Number.TryParseSigned(value, out long parsed) &&
                parsed >= sbyte.MinValue && parsed <= sbyte.MaxValue && (result = (sbyte)parsed) == parsed;
        }
    }
}
