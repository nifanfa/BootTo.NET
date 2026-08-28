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

    public sealed class CancellationTokenSource : IDisposable
    {
        internal sealed class Callback
        {
            internal Action Action;
            internal bool Removed;
            internal Callback Next;
        }

        private Callback _callbacks;
        private bool _canceled;
        private bool _disposed;
        private Task _cancelAfterTask;

        public CancellationTokenSource()
        {
        }

        public CancellationToken Token
        {
            get
            {
                EnsureNotDisposed();
                return new CancellationToken(this);
            }
        }

        public bool IsCancellationRequested => _canceled;

        internal CancellationTokenRegistration Register(Action callback)
        {
            EnsureNotDisposed();
            if (_canceled)
            {
                callback();
                return default;
            }

            Callback registration = new Callback { Action = callback, Next = _callbacks };
            _callbacks = registration;
            return new CancellationTokenRegistration(this, registration);
        }

        internal void Unregister(Callback callback)
        {
            if (callback != null)
                callback.Removed = true;
        }

        public void Cancel() => Cancel(false);

        public void Cancel(bool throwOnFirstException)
        {
            EnsureNotDisposed();
            if (_canceled)
                return;
            _canceled = true;

            List<Exception> exceptions = new List<Exception>();
            Callback callback = _callbacks;
            _callbacks = null;
            while (callback != null)
            {
                Callback next = callback.Next;
                if (!callback.Removed)
                {
                    try { callback.Action(); }
                    catch (Exception exception)
                    {
                        if (throwOnFirstException)
                            throw;
                        exceptions.Add(exception);
                    }
                }
                callback = next;
            }

            if (exceptions.Count != 0)
                throw new AggregateException(exceptions.ToArray());
        }

        public void CancelAfter(int millisecondsDelay)
        {
            EnsureNotDisposed();
            if (millisecondsDelay < Timeout.Infinite)
                throw new ArgumentOutOfRangeException("The cancellation delay must be -1 or non-negative.");
            if (millisecondsDelay == Timeout.Infinite)
                return;

            Task pending = Task.Delay(millisecondsDelay);
            _cancelAfterTask = pending;
            pending.AddContinuation(() =>
            {
                if (!_disposed && !_canceled && ReferenceEquals(_cancelAfterTask, pending))
                    Cancel();
            });
        }

        public void Dispose()
        {
            _disposed = true;
            _callbacks = null;
            _cancelAfterTask = null;
        }

        public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token)
        {
            CancellationTokenSource result = new CancellationTokenSource();
            if (token.CanBeCanceled)
                result._link = token.Register(result.Cancel);
            return result;
        }

        public static CancellationTokenSource CreateLinkedTokenSource(CancellationToken token1, CancellationToken token2)
        {
            CancellationTokenSource result = new CancellationTokenSource();
            if (token1.CanBeCanceled)
                result._link1 = token1.Register(result.Cancel);
            if (token2.CanBeCanceled)
                result._link2 = token2.Register(result.Cancel);
            return result;
        }

        private CancellationTokenRegistration _link;
        private CancellationTokenRegistration _link1;
        private CancellationTokenRegistration _link2;

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("CancellationTokenSource");
        }
    }
}
