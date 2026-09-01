namespace System
{
    public partial struct Int16
    {
        public static short Parse(string value)
        {
            if (!TryParse(value, out short result)) throw new FormatException("The value is not a valid Int16.");
            return result;
        }

        public static bool TryParse(string value, out short result)
        {
            result = 0;
            return Number.TryParseSigned(value, out long parsed) &&
                parsed >= short.MinValue && parsed <= short.MaxValue && (result = (short)parsed) == parsed;
        }
    }
}
