namespace System
{
    public class FormatException : Exception
    {
        public FormatException() : base("Input string was not in a correct format.") { }
        public FormatException(string message) : base(message) { }
    }
}
