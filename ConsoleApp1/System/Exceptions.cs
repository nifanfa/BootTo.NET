namespace System
{
    public class BadImageFormatException : Exception
    {
        public BadImageFormatException() : base("The image is invalid.") { }
        public BadImageFormatException(string message) : base(message) { }
    }

    public class FormatException : Exception
    {
        public FormatException() : base("Input string was not in a correct format.") { }
        public FormatException(string message) : base(message) { }
    }

    public class ArgumentOutOfRangeException : ArgumentException
    {
        public ArgumentOutOfRangeException() : base("The value was out of range.") { }
        public ArgumentOutOfRangeException(string message) : base(message) { }
        public ArgumentOutOfRangeException(string parameterName, string message) : base(message) { }
    }

    public class ObjectDisposedException : InvalidOperationException
    {
        public ObjectDisposedException(string objectName)
            : base("The object has been disposed." + (objectName == null ? string.Empty : " " + objectName))
        {
        }
    }

    public class NotImplementedException : Exception
    {
        public NotImplementedException() : base("The method or operation is not implemented.") { }
        public NotImplementedException(string message) : base(message) { }
    }

    public class AggregateException : Exception
    {
        private readonly Exception[] _innerExceptions;

        public AggregateException(Exception exception)
            : this(new Exception[] { exception })
        {
        }

        public AggregateException(Exception[] exceptions)
            : base("One or more errors occurred.")
        {
            if (exceptions == null || exceptions.Length == 0)
                throw new ArgumentException("At least one exception is required.");

            _innerExceptions = new Exception[exceptions.Length];
            for (int i = 0; i < exceptions.Length; i++)
                _innerExceptions[i] = exceptions[i];
        }

        public Exception[] InnerExceptions => _innerExceptions;
    }
}
