namespace System.IO
{
    internal class IOException : Exception
    {
        public IOException() : base("I/O error occurred.") { }
    }
}
