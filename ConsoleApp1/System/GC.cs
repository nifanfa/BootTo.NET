using System.Runtime.CompilerServices;

namespace System
{
    public static class GC
    {
        public static int Collect() => GarbageCollector.Collect();

        [Intrinsic]
        public static void KeepAlive(object obj) { }
    }
}
