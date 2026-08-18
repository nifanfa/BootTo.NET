using System.Threading.Tasks;

namespace System.Threading
{
    internal class Thread
    {
        public static void Sleep(int millisecondsTimeout) => Task.Delay(millisecondsTimeout).Wait();
    }
}
