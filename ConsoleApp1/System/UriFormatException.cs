namespace System
{
    public class UriFormatException : FormatException
    {
        public UriFormatException() : this("Invalid URI: The format of the URI could not be determined.") { }

        public UriFormatException(string message) : base(message) { }
    }
}
