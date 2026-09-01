namespace System.Net
{
    public class WebException : Exception
    {
        public WebException(string message) : base(message) { }

        internal WebException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }
}
