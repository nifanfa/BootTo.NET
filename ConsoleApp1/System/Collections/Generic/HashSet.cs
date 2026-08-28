namespace System.Collections.Generic
{
    public class HashSet<T> : ISet<T>
    {
        private T[] _items;
        private int _count;
        private readonly IEqualityComparer<T> _comparer;

        public HashSet()
            : this((IEqualityComparer<T>)null)
        {
        }

        public HashSet(IEqualityComparer<T> comparer)
        {
            _items = new T[0];
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public HashSet(IEnumerable<T> collection)
            : this(collection, null)
        {
        }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : this(comparer)
        {
            if (collection == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            UnionWith(collection);
        }

        public int Count => _count;
        public bool IsReadOnly => false;

        public bool Add(T item)
        {
            if (Contains(item))
                return false;
            EnsureCapacity(_count + 1);
            _items[_count++] = item;
            return true;
        }

        void ICollection<T>.Add(T item) => Add(item);

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;
            _count--;
            for (int i = index; i < _count; i++)
                _items[i] = _items[i + 1];
            _items[_count] = default;
            return true;
        }

        public bool Contains(T item) => IndexOf(item) >= 0;

        public void Clear()
        {
            for (int i = 0; i < _count; i++)
                _items[i] = default;
            _count = 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The destination array cannot be null.");
            if (arrayIndex < 0 || arrayIndex > array.Length - _count)
                throw new ArgumentException("The destination array range is invalid.");
            for (int i = 0; i < _count; i++)
                array[arrayIndex + i] = _items[i];
        }

        public T[] ToArray()
        {
            T[] result = new T[_count];
            CopyTo(result, 0);
            return result;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            foreach (T item in other)
                Add(item);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            HashSet<T> set = other as HashSet<T>;
            if (set == null)
                set = new HashSet<T>(other, _comparer);
            for (int i = _count - 1; i >= 0; i--)
                if (!set.Contains(_items[i]))
                    Remove(_items[i]);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            foreach (T item in other)
                Remove(item);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            HashSet<T> set = other as HashSet<T>;
            if (set == null)
                set = new HashSet<T>(other, _comparer);
            foreach (T item in set)
                if (!Remove(item))
                    Add(item);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
            => IsSubsetOf(new HashSet<T>(other, _comparer));

        public bool IsSubsetOf(HashSet<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            for (int i = 0; i < _count; i++)
                if (!other.Contains(_items[i]))
                    return false;
            return true;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            HashSet<T> set = new HashSet<T>(other, _comparer);
            return _count < set._count && IsSubsetOf(set);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            foreach (T item in other)
                if (!Contains(item))
                    return false;
            return true;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            HashSet<T> set = new HashSet<T>(other, _comparer);
            return _count > set._count && IsSupersetOf(set);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            foreach (T item in other)
                if (Contains(item))
                    return true;
            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            HashSet<T> set = new HashSet<T>(other, _comparer);
            return _count == set._count && IsSubsetOf(set);
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        private int IndexOf(T item)
        {
            for (int i = 0; i < _count; i++)
                if (_comparer.Equals(_items[i], item))
                    return i;
            return -1;
        }

        private void EnsureCapacity(int required)
        {
            if (_items.Length >= required)
                return;
            int capacity = _items.Length == 0 ? 4 : _items.Length * 2;
            if (capacity < required)
                capacity = required;
            T[] result = new T[capacity];
            for (int i = 0; i < _count; i++)
                result[i] = _items[i];
            _items = result;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly HashSet<T> _set;
            private int _index;
            private T _current;

            internal Enumerator(HashSet<T> set)
            {
                _set = set;
                _index = 0;
                _current = default;
            }

            public T Current => _current;
            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                if (_index >= _set._count)
                {
                    _current = default;
                    return false;
                }
                _current = _set._items[_index++];
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
