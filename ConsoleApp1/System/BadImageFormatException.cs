namespace System
{
    public class BadImageFormatException : Exception
    {
        public BadImageFormatException() : base("The image is invalid.") { }
        public BadImageFormatException(string message) : base(message) { }
    }
}
