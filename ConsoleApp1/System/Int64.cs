namespace System
{
    public partial struct Int64
    {
        public static long Parse(string value)
        {
            if (!TryParse(value, out long result)) throw new FormatException("The value is not a valid Int64.");
            return result;
        }

        public static bool TryParse(string value, out long result)
            => Number.TryParseSigned(value, out result);
    }
}
