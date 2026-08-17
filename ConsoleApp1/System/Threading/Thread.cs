namespace System.Threading
{
    internal class Thread
    {
        public static unsafe void Sleep(int millisecondsTimeout) => gBS->Stall((ulong)millisecondsTimeout * 1000);
    }
}
