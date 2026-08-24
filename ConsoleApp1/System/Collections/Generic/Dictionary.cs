namespace System.Collections.Generic
{
    internal sealed class DictionaryDefaultEqualityComparer<T> : IEqualityComparer<T>
    {
        internal static readonly DictionaryDefaultEqualityComparer<T> Instance = new DictionaryDefaultEqualityComparer<T>();

        public bool Equals(T x, T y)
        {
            object left = x;
            object right = y;

            if (left == null)
                return right == null;

            return left.Equals(right);
        }

        public int GetHashCode(T obj)
        {
            object value = obj;
            return value == null ? 0 : value.GetHashCode();
        }
    }

    public class Dictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private const int DefaultCapacity = 4;

        private struct Entry
        {
            internal int HashCode;
            internal int Next;
            internal TKey Key;
            internal TValue Value;
        }

        private int[] _buckets;
        private Entry[] _entries;
        private int _count;
        private int _freeList;
        private int _freeCount;
        private readonly IEqualityComparer<TKey> _comparer;

        public Dictionary() : this(0, null)
        {
        }

        public Dictionary(int capacity) : this(capacity, null)
        {
        }

        public Dictionary(IEqualityComparer<TKey> comparer) : this(0, comparer)
        {
        }

        public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0)
                throw new System.ArgumentException("The dictionary capacity cannot be negative.");

            _buckets = new int[0];
            _entries = new Entry[0];
            _comparer = comparer ?? DictionaryDefaultEqualityComparer<TKey>.Instance;

            if (capacity > 0)
                Initialize(capacity);
        }

        public int Count => _count - _freeCount;

        public TValue this[TKey key]
        {
            get
            {
                int index = FindEntry(key);
                if (index < 0)
                    throw new KeyNotFoundException("The requested key was not found in the dictionary.");

                return _entries[index].Value;
            }
            set => Insert(key, value, false);
        }

        public void Add(TKey key, TValue value)
            => Insert(key, value, true);

        public bool ContainsKey(TKey key)
            => FindEntry(key) >= 0;

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = FindEntry(key);
            if (index >= 0)
            {
                value = _entries[index].Value;
                return true;
            }

            value = default;
            return false;
        }

        public bool Remove(TKey key)
        {
            ValidateKey(key);
            if (_buckets.Length == 0)
                return false;

            int hashCode = GetHashCode(key);
            int bucket = hashCode & (_buckets.Length - 1);
            int previous = -1;

            for (int index = _buckets[bucket] - 1; index >= 0; index = _entries[index].Next - 1)
            {
                ref Entry entry = ref _entries[index];
                if (entry.HashCode == hashCode && _comparer.Equals(entry.Key, key))
                {
                    if (previous < 0)
                        _buckets[bucket] = entry.Next;
                    else
                        _entries[previous].Next = entry.Next;

                    entry.HashCode = -1;
                    entry.Next = _freeList;
                    entry.Key = default;
                    entry.Value = default;
                    _freeList = index + 1;
                    _freeCount++;
                    return true;
                }

                previous = index;
            }

            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < _buckets.Length; i++)
                _buckets[i] = 0;

            for (int i = 0; i < _count; i++)
            {
                _entries[i].HashCode = -1;
                _entries[i].Next = 0;
                _entries[i].Key = default;
                _entries[i].Value = default;
            }

            _count = 0;
            _freeList = 0;
            _freeCount = 0;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => new Enumerator(this);

        private int FindEntry(TKey key)
        {
            ValidateKey(key);
            if (_buckets.Length == 0)
                return -1;

            int hashCode = GetHashCode(key);
            int bucket = hashCode & (_buckets.Length - 1);
            for (int index = _buckets[bucket] - 1; index >= 0; index = _entries[index].Next - 1)
            {
                Entry entry = _entries[index];
                if (entry.HashCode == hashCode && _comparer.Equals(entry.Key, key))
                    return index;
            }

            return -1;
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            ValidateKey(key);
            if (_buckets.Length == 0)
                Initialize(DefaultCapacity);

            int hashCode = GetHashCode(key);
            int bucket = hashCode & (_buckets.Length - 1);
            for (int index = _buckets[bucket] - 1; index >= 0; index = _entries[index].Next - 1)
            {
                ref Entry entry = ref _entries[index];
                if (entry.HashCode != hashCode || !_comparer.Equals(entry.Key, key))
                    continue;

                if (add)
                    throw new System.ArgumentException("An item with the same key has already been added.");

                entry.Value = value;
                return;
            }

            int entryIndex;
            if (_freeList != 0)
            {
                entryIndex = _freeList - 1;
                _freeList = _entries[entryIndex].Next;
                _freeCount--;
            }
            else
            {
                if (_count == _entries.Length)
                {
                    Resize();
                    bucket = hashCode & (_buckets.Length - 1);
                }

                entryIndex = _count++;
            }

            _entries[entryIndex].HashCode = hashCode;
            _entries[entryIndex].Next = _buckets[bucket];
            _entries[entryIndex].Key = key;
            _entries[entryIndex].Value = value;
            _buckets[bucket] = entryIndex + 1;
        }

        private void Initialize(int capacity)
        {
            int size = DefaultCapacity;
            while (size < capacity)
                size *= 2;

            _buckets = new int[size];
            _entries = new Entry[size];
        }

        private void Resize()
        {
            int newSize = _entries.Length * 2;
            int[] buckets = new int[newSize];
            Entry[] entries = new Entry[newSize];

            for (int i = 0; i < _count; i++)
                entries[i] = _entries[i];

            for (int i = 0; i < _count; i++)
            {
                if (entries[i].HashCode < 0)
                    continue;

                int bucket = entries[i].HashCode & (newSize - 1);
                entries[i].Next = buckets[bucket];
                buckets[bucket] = i + 1;
            }

            _buckets = buckets;
            _entries = entries;
        }

        private int GetHashCode(TKey key)
            => _comparer.GetHashCode(key) & 0x7FFFFFFF;

        private static void ValidateKey(TKey key)
        {
            if ((object)key == null)
                throw new System.ArgumentNullException("The dictionary key cannot be null.");
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly Dictionary<TKey, TValue> _dictionary;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;

            internal Enumerator(Dictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _index = 0;
                _current = default;
            }

            public KeyValuePair<TKey, TValue> Current => _current;

            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                while (_index < _dictionary._count)
                {
                    Entry entry = _dictionary._entries[_index++];
                    if (entry.HashCode < 0)
                        continue;

                    _current = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                    return true;
                }

                _current = default;
                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default;
            }

            public void Dispose()
            {
            }
        }
    }
}
