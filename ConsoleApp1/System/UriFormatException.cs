namespace System
{
    public class UriFormatException : FormatException
    {
        private readonly string _message;

        public UriFormatException() : this("Invalid URI: The format of the URI could not be determined.") { }

        public UriFormatException(string message) => _message = message;

        public override string Message => _message;
    }
}
