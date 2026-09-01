namespace System
{
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
