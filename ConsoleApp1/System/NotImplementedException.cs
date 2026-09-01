namespace System
{
    public class NotImplementedException : Exception
    {
        public NotImplementedException() : base("The method or operation is not implemented.") { }
        public NotImplementedException(string message) : base(message) { }
    }
}
