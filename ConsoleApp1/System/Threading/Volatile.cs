namespace System.Threading
{
    public static class Volatile
    {
        public static bool Read(ref bool location) => location;
        public static void Write(ref bool location, bool value) => location = value;
        public static int Read(ref int location) => location;
        public static void Write(ref int location, int value) => location = value;
        public static long Read(ref long location) => location;
        public static void Write(ref long location, long value) => location = value;
        public static T Read<T>(ref T location) where T : class => location;
        public static void Write<T>(ref T location, T value) where T : class => location = value;
    }
}
