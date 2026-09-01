using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading
{
    public readonly struct CancellationToken
    {
        private readonly CancellationTokenSource _source;

        internal CancellationToken(CancellationTokenSource source) => _source = source;

        public static CancellationToken None => default;
        public bool CanBeCanceled => _source != null;
        public bool IsCancellationRequested => _source != null && _source.IsCancellationRequested;

        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested)
                throw new OperationCanceledException();
        }

        public CancellationTokenRegistration Register(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException("The cancellation callback cannot be null.");
            return _source == null ? default : _source.Register(callback);
        }

        public CancellationTokenRegistration Register(Action<object> callback, object state)
        {
            if (callback == null)
                throw new ArgumentNullException("The cancellation callback cannot be null.");
            return Register(() => callback(state));
        }

        public CancellationTokenRegistration Register(Action<object, CancellationToken> callback, object state)
        {
            if (callback == null)
                throw new ArgumentNullException("The cancellation callback cannot be null.");
            CancellationToken token = this;
            return Register(() => callback(state, token));
        }
    }

}
