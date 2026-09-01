namespace System
{
    public class ArgumentOutOfRangeException : ArgumentException
    {
        public ArgumentOutOfRangeException() : base("The value was out of range.") { }
        public ArgumentOutOfRangeException(string message) : base(message) { }
        public ArgumentOutOfRangeException(string parameterName, string message) : base(message) { }
    }
}
