using System.Collections;
using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            ValidateSourceAndPredicate(source, predicate);
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    if (!predicate(enumerator.Current))
                        return false;
                return true;
            }
            finally { enumerator.Dispose(); }
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            ValidateSourceAndPredicate(source, predicate);
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    if (predicate(enumerator.Current))
                        return enumerator.Current;
            }
            finally { enumerator.Dispose(); }
            throw new InvalidOperationException("The source sequence contains no matching element.");
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            return LastCore(source, null, false);
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            ValidateSourceAndPredicate(source, predicate);
            return LastCore(source, predicate, true);
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            return LastCore(source, null, false, true);
        }

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            ValidateSourceAndPredicate(source, predicate);
            return LastCore(source, predicate, true, true);
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            return SingleCore(source, null, false, false);
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            ValidateSourceAndPredicate(source, predicate);
            return SingleCore(source, predicate, true, false);
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            return SingleCore(source, null, false, true);
        }

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            ValidateSourceAndPredicate(source, predicate);
            return SingleCore(source, predicate, true, true);
        }

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            if (count < 0)
                throw new ArgumentOutOfRangeException("The skip count cannot be negative.");
            return new SkipTakeEnumerable<TSource>(source, count, -1);
        }

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            if (count < 0)
                throw new ArgumentOutOfRangeException("The take count cannot be negative.");
            return new SkipTakeEnumerable<TSource>(source, 0, count);
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            if (index < 0)
                throw new ArgumentOutOfRangeException("The element index cannot be negative.");
            TSource result;
            if (TryElementAt(source, index, out result))
                return result;
            throw new ArgumentOutOfRangeException("The element index is outside the source sequence.");
        }

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            if (index < 0)
                return default;
            TSource result;
            return TryElementAt(source, index, out result) ? result : default;
        }

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
            => Distinct(source, null);

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source,
            IEqualityComparer<TSource> comparer)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            return new DistinctEnumerable<TSource>(source, comparer);
        }

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first,
            IEnumerable<TSource> second)
        {
            if (first == null || second == null)
                throw new ArgumentNullException("The source sequences cannot be null.");
            return new ConcatEnumerable<TSource>(first, second);
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first,
            IEnumerable<TSource> second)
            => SequenceEqual(first, second, EqualityComparer<TSource>.Default);

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first,
            IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null || second == null)
                throw new ArgumentNullException("The source sequences cannot be null.");
            comparer = comparer ?? EqualityComparer<TSource>.Default;
            IEnumerator<TSource> left = first.GetEnumerator();
            IEnumerator<TSource> right = second.GetEnumerator();
            try
            {
                while (true)
                {
                    bool leftHasValue = left.MoveNext();
                    bool rightHasValue = right.MoveNext();
                    if (!leftHasValue || !rightHasValue)
                        return leftHasValue == rightHasValue;
                    if (!comparer.Equals(left.Current, right.Current))
                        return false;
                }
            }
            finally
            {
                left.Dispose();
                right.Dispose();
            }
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            => ToDictionary(source, keySelector, value => value, null);

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
            => ToDictionary(source, keySelector, elementSelector, null);

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null || keySelector == null || elementSelector == null)
                throw new ArgumentNullException("The source and dictionary selectors cannot be null.");
            Dictionary<TKey, TElement> result = new Dictionary<TKey, TElement>(comparer);
            foreach (TSource item in source)
                result.Add(keySelector(item), elementSelector(item));
            return result;
        }

        public static int Sum(this IEnumerable<int> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            int result = 0;
            foreach (int value in source)
                result = checked(result + value);
            return result;
        }

        public static long Sum(this IEnumerable<long> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            long result = 0;
            foreach (long value in source)
                result = checked(result + value);
            return result;
        }

        public static float Sum(this IEnumerable<float> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            float result = 0;
            foreach (float value in source)
                result += value;
            return result;
        }

        public static double Sum(this IEnumerable<double> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            double result = 0;
            foreach (double value in source)
                result += value;
            return result;
        }

        public static TSource Min<TSource>(this IEnumerable<TSource> source)
            => MinMax(source, false);

        public static TSource Max<TSource>(this IEnumerable<TSource> source)
            => MinMax(source, true);

        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
            => SelectMinMax(source, selector, false);

        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
            => SelectMinMax(source, selector, true);

        private static TSource LastCore<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate,
            bool usePredicate, bool returnDefault = false)
        {
            TSource result = default;
            bool found = false;
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    TSource value = enumerator.Current;
                    if (!usePredicate || predicate(value))
                    {
                        result = value;
                        found = true;
                    }
                }
            }
            finally { enumerator.Dispose(); }
            if (!found && !returnDefault)
                throw new InvalidOperationException("The source sequence contains no matching element.");
            return result;
        }

        private static TSource SingleCore<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate,
            bool usePredicate, bool returnDefault)
        {
            TSource result = default;
            bool found = false;
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    TSource value = enumerator.Current;
                    if (usePredicate && !predicate(value))
                        continue;
                    if (found)
                        throw new InvalidOperationException("The source sequence contains more than one matching element.");
                    result = value;
                    found = true;
                }
            }
            finally { enumerator.Dispose(); }
            if (!found && !returnDefault)
                throw new InvalidOperationException("The source sequence contains no matching element.");
            return result;
        }

        private static bool TryElementAt<TSource>(IEnumerable<TSource> source, int index, out TSource result)
        {
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (index-- >= 0)
                {
                    if (!enumerator.MoveNext())
                    {
                        result = default;
                        return false;
                    }
                }
                result = enumerator.Current;
                return true;
            }
            finally { enumerator.Dispose(); }
        }

        private static TSource MinMax<TSource>(IEnumerable<TSource> source, bool maximum)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("The source sequence contains no elements.");
                TSource result = enumerator.Current;
                Comparer<TSource> comparer = Comparer<TSource>.Default;
                while (enumerator.MoveNext())
                {
                    TSource value = enumerator.Current;
                    int comparison = comparer.Compare(value, result);
                    if (maximum ? comparison > 0 : comparison < 0)
                        result = value;
                }
                return result;
            }
            finally { enumerator.Dispose(); }
        }

        private static int SelectMinMax<TSource>(IEnumerable<TSource> source, Func<TSource, int> selector,
            bool maximum)
        {
            ValidateSourceAndSelector(source, selector);
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("The source sequence contains no elements.");
                int result = selector(enumerator.Current);
                while (enumerator.MoveNext())
                {
                    int value = selector(enumerator.Current);
                    if (maximum ? value > result : value < result)
                        result = value;
                }
                return result;
            }
            finally { enumerator.Dispose(); }
        }

        private static void ValidateSourceAndPredicate<TSource>(IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            if (predicate == null)
                throw new ArgumentNullException("The predicate cannot be null.");
        }

        private static void ValidateSourceAndSelector<TSource, TResult>(IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            if (source == null)
                throw new ArgumentNullException("The source sequence cannot be null.");
            if (selector == null)
                throw new ArgumentNullException("The selector cannot be null.");
        }

        private sealed class SkipTakeEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _source;
            private readonly int _skip;
            private readonly int _take;

            internal SkipTakeEnumerable(IEnumerable<T> source, int skip, int take)
            {
                _source = source;
                _skip = skip;
                _take = take;
            }

            public IEnumerator<T> GetEnumerator() => new SkipTakeEnumerator<T>(_source.GetEnumerator(), _skip, _take);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class SkipTakeEnumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator<T> _source;
            private int _skip;
            private int _remaining;

            internal SkipTakeEnumerator(IEnumerator<T> source, int skip, int take)
            {
                _source = source;
                _skip = skip;
                _remaining = take;
            }

            public T Current => _source.Current;
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                while (_skip > 0)
                {
                    if (!_source.MoveNext())
                        return false;
                    _skip--;
                }
                if (_remaining == 0 || !_source.MoveNext())
                    return false;
                if (_remaining > 0)
                    _remaining--;
                return true;
            }

            public void Reset() => throw new NotSupportedException("The query enumerator cannot be reset.");
            public void Dispose() => _source.Dispose();
        }

        private sealed class DistinctEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _source;
            private readonly IEqualityComparer<T> _comparer;

            internal DistinctEnumerable(IEnumerable<T> source, IEqualityComparer<T> comparer)
            {
                _source = source;
                _comparer = comparer;
            }

            public IEnumerator<T> GetEnumerator()
                => new DistinctEnumerator<T>(_source.GetEnumerator(), _comparer);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class DistinctEnumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator<T> _source;
            private readonly HashSet<T> _seen;

            internal DistinctEnumerator(IEnumerator<T> source, IEqualityComparer<T> comparer)
            {
                _source = source;
                _seen = new HashSet<T>(comparer);
            }

            public T Current => _source.Current;
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                while (_source.MoveNext())
                    if (_seen.Add(_source.Current))
                        return true;
                return false;
            }

            public void Reset() => throw new NotSupportedException("The query enumerator cannot be reset.");
            public void Dispose() => _source.Dispose();
        }

        private sealed class ConcatEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _first;
            private readonly IEnumerable<T> _second;

            internal ConcatEnumerable(IEnumerable<T> first, IEnumerable<T> second)
            {
                _first = first;
                _second = second;
            }

            public IEnumerator<T> GetEnumerator()
                => new ConcatEnumerator<T>(_first.GetEnumerator(), _second.GetEnumerator());
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class ConcatEnumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator<T> _first;
            private readonly IEnumerator<T> _second;
            private bool _usingSecond;

            internal ConcatEnumerator(IEnumerator<T> first, IEnumerator<T> second)
            {
                _first = first;
                _second = second;
            }

            public T Current => _usingSecond ? _second.Current : _first.Current;
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (!_usingSecond && _first.MoveNext())
                    return true;
                _usingSecond = true;
                return _second.MoveNext();
            }

            public void Reset() => throw new NotSupportedException("The query enumerator cannot be reset.");

            public void Dispose()
            {
                _first.Dispose();
                _second.Dispose();
            }
        }
    }
}
