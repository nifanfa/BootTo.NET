namespace System.Threading
{
    public readonly struct CancellationTokenRegistration : IDisposable
    {
        private readonly CancellationTokenSource _source;
        private readonly CancellationTokenSource.Callback _callback;

        internal CancellationTokenRegistration(CancellationTokenSource source, CancellationTokenSource.Callback callback)
        {
            _source = source;
            _callback = callback;
        }

        public void Dispose()
        {
            if (_source != null)
                _source.Unregister(_callback);
        }
    }
}
