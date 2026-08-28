using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static bool Any<TSource>(this TSource[] source)
        {
            if (source == null)
                throw new ArgumentNullException("The source array cannot be null.");
            return source.Length != 0;
        }

        public static bool Any<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source array and predicate cannot be null.");
            for (int i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                    return true;
            return false;
        }

        public static int Count<TSource>(this TSource[] source)
        {
            if (source == null)
                throw new ArgumentNullException("The source array cannot be null.");
            return source.Length;
        }

        public static int Count<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source array and predicate cannot be null.");
            int count = 0;
            for (int i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                    count++;
            return count;
        }

        public static TSource First<TSource>(this TSource[] source)
        {
            if (source == null)
                throw new ArgumentNullException("The source array cannot be null.");
            if (source.Length == 0)
                throw new InvalidOperationException("The source array contains no elements.");
            return source[0];
        }

        public static TSource FirstOrDefault<TSource>(this TSource[] source)
        {
            if (source == null)
                throw new ArgumentNullException("The source array cannot be null.");
            return source.Length == 0 ? default : source[0];
        }

        public static TSource FirstOrDefault<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source array and predicate cannot be null.");
            for (int i = 0; i < source.Length; i++)
                if (predicate(source[i]))
                    return source[i];
            return default;
        }

        public static IEnumerable<TSource> Where<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source array and predicate cannot be null.");
            return new ArrayWhereEnumerable<TSource>(source, predicate);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            if (source == null || selector == null)
                throw new ArgumentNullException("The source array and selector cannot be null.");
            return new ArraySelectEnumerable<TSource, TResult>(source, selector);
        }

        public static bool Contains<TSource>(this TSource[] source, TSource value)
        {
            if (source == null)
                throw new ArgumentNullException("The source array cannot be null.");
            for (int i = 0; i < source.Length; i++)
                if (Equals(source[i], value))
                    return true;
            return false;
        }

        public static TSource[] ToArray<TSource>(this TSource[] source)
        {
            if (source == null)
                throw new ArgumentNullException("The source array cannot be null.");
            TSource[] result = new TSource[source.Length];
            for (int i = 0; i < source.Length; i++)
                result[i] = source[i];
            return result;
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try { return enumerator.MoveNext(); }
            finally { enumerator.Dispose(); }
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source sequence and predicate cannot be null.");
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    if (predicate(enumerator.Current))
                        return true;
                return false;
            }
            finally { enumerator.Dispose(); }
        }

        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            int count = 0;
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    count++;
            }
            finally { enumerator.Dispose(); }
            return count;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source sequence and predicate cannot be null.");
            int count = 0;
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    if (predicate(enumerator.Current))
                        count++;
            }
            finally { enumerator.Dispose(); }
            return count;
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                if (enumerator.MoveNext())
                    return enumerator.Current;
            }
            finally { enumerator.Dispose(); }
            throw new InvalidOperationException("The source sequence contains no elements.");
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try { return enumerator.MoveNext() ? enumerator.Current : default; }
            finally { enumerator.Dispose(); }
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source sequence and predicate cannot be null.");
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    if (predicate(enumerator.Current))
                        return enumerator.Current;
                return default;
            }
            finally { enumerator.Dispose(); }
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source sequence and predicate cannot be null.");
            return new WhereEnumerable<TSource>(source, predicate);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null || selector == null)
                throw new ArgumentNullException("The source sequence and selector cannot be null.");
            return new SelectEnumerable<TSource, TResult>(source, selector);
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    if (Equals(enumerator.Current, value))
                        return true;
                return false;
            }
            finally { enumerator.Dispose(); }
        }

        private sealed class ArrayWhereEnumerable<TSource> : IEnumerable<TSource>
        {
            private readonly TSource[] _source;
            private readonly Func<TSource, bool> _predicate;

            internal ArrayWhereEnumerable(TSource[] source, Func<TSource, bool> predicate)
            {
                _source = source;
                _predicate = predicate;
            }

            public IEnumerator<TSource> GetEnumerator()
                => new ArrayWhereEnumerator<TSource>(_source, _predicate);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class ArrayWhereEnumerator<TSource> : IEnumerator<TSource>
        {
            private readonly TSource[] _source;
            private readonly Func<TSource, bool> _predicate;
            private int _index;
            private TSource _current;

            internal ArrayWhereEnumerator(TSource[] source, Func<TSource, bool> predicate)
            {
                _source = source;
                _predicate = predicate;
                _index = 0;
                _current = default;
            }

            public TSource Current => _current;
            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                while (_index < _source.Length)
                {
                    TSource value = _source[_index++];
                    if (_predicate(value))
                    {
                        _current = value;
                        return true;
                    }
                }
                _current = default;
                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public void Dispose() { }
        }

        private sealed class ArraySelectEnumerable<TSource, TResult> : IEnumerable<TResult>
        {
            private readonly TSource[] _source;
            private readonly Func<TSource, TResult> _selector;

            internal ArraySelectEnumerable(TSource[] source, Func<TSource, TResult> selector)
            {
                _source = source;
                _selector = selector;
            }

            public IEnumerator<TResult> GetEnumerator()
                => new ArraySelectEnumerator<TSource, TResult>(_source, _selector);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class ArraySelectEnumerator<TSource, TResult> : IEnumerator<TResult>
        {
            private readonly TSource[] _source;
            private readonly Func<TSource, TResult> _selector;
            private int _index;
            private TResult _current;

            internal ArraySelectEnumerator(TSource[] source, Func<TSource, TResult> selector)
            {
                _source = source;
                _selector = selector;
                _index = 0;
                _current = default;
            }

            public TResult Current => _current;
            object IEnumerator.Current => Current;
            public bool MoveNext()
            {
                if (_index >= _source.Length)
                {
                    _current = default;
                    return false;
                }

                _current = _selector(_source[_index++]);
                return true;
            }
            public void Reset()
            {
                _index = 0;
                _current = default;
            }
            public void Dispose() { }
        }

        private sealed class WhereEnumerable<TSource> : IEnumerable<TSource>
        {
            private readonly IEnumerable<TSource> _source;
            private readonly Func<TSource, bool> _predicate;

            internal WhereEnumerable(IEnumerable<TSource> source, Func<TSource, bool> predicate)
            {
                _source = source;
                _predicate = predicate;
            }

            public IEnumerator<TSource> GetEnumerator()
                => new WhereEnumerator<TSource>(_source.GetEnumerator(), _predicate);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class WhereEnumerator<TSource> : IEnumerator<TSource>
        {
            private readonly IEnumerator<TSource> _source;
            private readonly Func<TSource, bool> _predicate;

            internal WhereEnumerator(IEnumerator<TSource> source, Func<TSource, bool> predicate)
            {
                _source = source;
                _predicate = predicate;
            }

            public TSource Current => _source.Current;
            object IEnumerator.Current => Current;
            public bool MoveNext()
            {
                while (_source.MoveNext())
                    if (_predicate(_source.Current))
                        return true;
                return false;
            }
            public void Reset() => _source.Reset();
            public void Dispose() => _source.Dispose();
        }

        private sealed class SelectEnumerable<TSource, TResult> : IEnumerable<TResult>
        {
            private readonly IEnumerable<TSource> _source;
            private readonly Func<TSource, TResult> _selector;

            internal SelectEnumerable(IEnumerable<TSource> source, Func<TSource, TResult> selector)
            {
                _source = source;
                _selector = selector;
            }

            public IEnumerator<TResult> GetEnumerator()
                => new SelectEnumerator<TSource, TResult>(_source.GetEnumerator(), _selector);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class SelectEnumerator<TSource, TResult> : IEnumerator<TResult>
        {
            private readonly IEnumerator<TSource> _source;
            private readonly Func<TSource, TResult> _selector;

            internal SelectEnumerator(IEnumerator<TSource> source, Func<TSource, TResult> selector)
            {
                _source = source;
                _selector = selector;
            }

            public TResult Current => _selector(_source.Current);
            object IEnumerator.Current => Current;
            public bool MoveNext() => _source.MoveNext();
            public void Reset() => _source.Reset();
            public void Dispose() => _source.Dispose();
        }
    }
}
