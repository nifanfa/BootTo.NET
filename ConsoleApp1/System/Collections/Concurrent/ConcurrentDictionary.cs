using System.Collections.Generic;

namespace System.Collections.Concurrent
{
    public class ConcurrentDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;

        public ConcurrentDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public int Count
        {
            get
            {
                lock (this)
                    return _dictionary.Count;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (this)
                    return _dictionary[key];
            }
            set
            {
                lock (this)
                    _dictionary[key] = value;
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (this)
                return _dictionary.ContainsKey(key);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            lock (this)
            {
                if (_dictionary.ContainsKey(key))
                    return false;
                _dictionary.Add(key, value);
                return true;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (this)
                return _dictionary.TryGetValue(key, out value);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            lock (this)
            {
                if (!_dictionary.TryGetValue(key, out value))
                    return false;
                _dictionary.Remove(key);
                return true;
            }
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            lock (this)
            {
                if (_dictionary.TryGetValue(key, out TValue existing))
                    return existing;
                _dictionary.Add(key, value);
                return value;
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (valueFactory == null)
                throw new ArgumentNullException("The value factory cannot be null.");

            lock (this)
            {
                if (_dictionary.TryGetValue(key, out TValue existing))
                    return existing;
                TValue value = valueFactory(key);
                _dictionary.Add(key, value);
                return value;
            }
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (updateValueFactory == null)
                throw new ArgumentNullException("The update value factory cannot be null.");

            lock (this)
            {
                if (_dictionary.TryGetValue(key, out TValue existing))
                {
                    TValue updated = updateValueFactory(key, existing);
                    _dictionary[key] = updated;
                    return updated;
                }

                _dictionary.Add(key, addValue);
                return addValue;
            }
        }

        public void Clear()
        {
            lock (this)
                _dictionary.Clear();
        }

        public Enumerator GetEnumerator()
        {
            lock (this)
            {
                List<KeyValuePair<TKey, TValue>> snapshot = new List<KeyValuePair<TKey, TValue>>();
                foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
                    snapshot.Add(pair);
                return new Enumerator(snapshot.ToArray());
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly KeyValuePair<TKey, TValue>[] _items;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;

            internal Enumerator(KeyValuePair<TKey, TValue>[] items)
            {
                _items = items;
                _index = 0;
                _current = default;
            }

            public KeyValuePair<TKey, TValue> Current => _current;
            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                if (_index >= _items.Length)
                {
                    _current = default;
                    return false;
                }

                _current = _items[_index++];
                return true;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public void Dispose() { }
        }
    }
}
