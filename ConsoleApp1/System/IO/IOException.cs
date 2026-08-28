namespace System.IO
{
    public class IOException : Exception
    {
        public IOException() : base("I/O error occurred.") { }
        public IOException(string message) : base(message) { }
    }
}
