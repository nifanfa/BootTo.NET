using System.Threading.Tasks;

namespace System.Threading
{
    public class Thread
    {
        public static void Sleep(int millisecondsTimeout) => Task.Delay(millisecondsTimeout).Wait();
    }
}
