namespace System.Threading
{
    public static class Interlocked
    {
        public static int Increment(ref int location) => ++location;
        public static int Decrement(ref int location) => --location;
        public static int Add(ref int location, int value) => location += value;
        public static int Exchange(ref int location, int value)
        {
            int original = location;
            location = value;
            return original;
        }

        public static int CompareExchange(ref int location, int value, int comparand)
        {
            int original = location;
            if (original == comparand)
                location = value;
            return original;
        }

        public static long Increment(ref long location) => ++location;
        public static long Decrement(ref long location) => --location;
        public static long Add(ref long location, long value) => location += value;
        public static long Exchange(ref long location, long value)
        {
            long original = location;
            location = value;
            return original;
        }

        public static long CompareExchange(ref long location, long value, long comparand)
        {
            long original = location;
            if (original == comparand)
                location = value;
            return original;
        }

        public static long Read(ref long location) => location;
    }
}
