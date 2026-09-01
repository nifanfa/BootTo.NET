namespace System
{
    public partial struct Int32
    {
        public static int Parse(string value)
        {
            if (!TryParse(value, out int result)) throw new FormatException("The value is not a valid Int32.");
            return result;
        }

        public static bool TryParse(string value, out int result)
        {
            result = 0;
            return Number.TryParseSigned(value, out long parsed) &&
                parsed >= int.MinValue && parsed <= int.MaxValue && (result = (int)parsed) == parsed;
        }
    }
}
