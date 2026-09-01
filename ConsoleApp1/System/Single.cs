namespace System
{
    public partial struct Single
    {
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
