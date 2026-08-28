namespace System.Collections.Generic
{
    public class List<T> : IList<T>, IReadOnlyList<T>
    {
        private const int DefaultCapacity = 4;

        private T[] _items;
        private int _size;

        public List()
        {
            _items = new T[0];
        }

        public List(int capacity)
        {
            if (capacity < 0)
                throw new IndexOutOfRangeException("The list capacity cannot be negative.");

            _items = new T[capacity];
        }

        public List(T[] items)
        {
            if (items == null)
                throw new ArgumentNullException("The source items cannot be null.");

            _items = new T[items.Length];
            for (int i = 0; i < items.Length; i++)
                _items[i] = items[i];

            _size = items.Length;
        }

        public List(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("The source items cannot be null.");

            _items = new T[0];
            AddRange(items);
        }

        public int Count => _size;
        public bool IsReadOnly => false;

        public int Capacity
        {
            get => _items.Length;
            set
            {
                if (value < _size)
                    throw new IndexOutOfRangeException("The list capacity cannot be less than Count.");

                if (value != _items.Length)
                    Resize(value);
            }
        }

        public T this[int index]
        {
            get
            {
                ValidateIndex(index);
                return _items[index];
            }
            set
            {
                ValidateIndex(index);
                _items[index] = value;
            }
        }

        public void Add(T item)
        {
            EnsureCapacity(_size + 1);
            _items[_size++] = item;
        }

        public void AddRange(T[] items)
        {
            if (items == null)
                throw new ArgumentNullException("The source items cannot be null.");

            EnsureCapacity(_size + items.Length);
            for (int i = 0; i < items.Length; i++)
                _items[_size + i] = items[i];

            _size += items.Length;
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("The source items cannot be null.");
            foreach (T item in items)
                Add(item);
        }

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)_size)
                throw new IndexOutOfRangeException("The insertion index is outside the list.");

            EnsureCapacity(_size + 1);
            for (int i = _size; i > index; i--)
                _items[i] = _items[i - 1];

            _items[index] = item;
            _size++;
        }

        public void RemoveAt(int index)
        {
            ValidateIndex(index);

            _size--;
            for (int i = index; i < _size; i++)
                _items[i] = _items[i + 1];

            _items[_size] = default(T);
        }

        public int IndexOf(T item)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _size; i++)
                if (comparer.Equals(_items[i], item))
                    return i;
            return -1;
        }

        public bool Contains(T item) => IndexOf(item) >= 0;

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The destination array cannot be null.");
            if (arrayIndex < 0 || arrayIndex > array.Length - _size)
                throw new ArgumentException("The destination array range is invalid.");
            for (int i = 0; i < _size; i++)
                array[arrayIndex + i] = _items[i];
        }

        public void InsertRange(int index, IEnumerable<T> items)
        {
            if ((uint)index > (uint)_size)
                throw new ArgumentOutOfRangeException("The insertion index is outside the list.");
            if (items == null)
                throw new ArgumentNullException("The source items cannot be null.");

            List<T> pending = items as List<T>;
            if (pending == this)
                pending = new List<T>(ToArray());
            if (pending != null)
            {
                EnsureCapacity(_size + pending._size);
                for (int i = _size - 1; i >= index; i--)
                    _items[i + pending._size] = _items[i];
                for (int i = 0; i < pending._size; i++)
                    _items[index + i] = pending._items[i];
                _size += pending._size;
                return;
            }

            foreach (T item in items)
            {
                Insert(index++, item);
            }
        }

        public int RemoveAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("The match predicate cannot be null.");
            int write = 0;
            int removed = 0;
            for (int i = 0; i < _size; i++)
            {
                if (match(_items[i]))
                {
                    removed++;
                    continue;
                }
                _items[write++] = _items[i];
            }
            for (int i = write; i < _size; i++)
                _items[i] = default;
            _size = write;
            return removed;
        }

        public T Find(Predicate<T> match)
        {
            int index = FindIndex(match);
            return index < 0 ? default : _items[index];
        }

        public int FindIndex(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("The match predicate cannot be null.");
            for (int i = 0; i < _size; i++)
                if (match(_items[i]))
                    return i;
            return -1;
        }

        public void ForEach(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("The action cannot be null.");
            for (int i = 0; i < _size; i++)
                action(_items[i]);
        }

        public void Reverse() => Array.Reverse(_items, 0, _size);

        public void Sort() => Sort((IComparer<T>)null);

        public void Sort(IComparer<T> comparer)
            => Array.Sort(_items, 0, _size, comparer);

        public void Sort(Comparison<T> comparison)
            => Sort(new ComparisonComparer<T>(comparison));

        public List<T> GetRange(int index, int count)
        {
            if (index < 0 || count < 0 || index > _size - count)
                throw new ArgumentException("The list range is invalid.");
            List<T> result = new List<T>(count);
            for (int i = 0; i < count; i++)
                result.Add(_items[index + i]);
            return result;
        }

        public void Clear()
        {
            for (int i = 0; i < _size; i++)
                _items[i] = default(T);

            _size = 0;
        }

        public T[] ToArray()
        {
            T[] result = new T[_size];
            for (int i = 0; i < _size; i++)
                result[i] = _items[i];

            return result;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        private void EnsureCapacity(int minimum)
        {
            if (_items.Length >= minimum)
                return;

            int capacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
            if (capacity < minimum)
                capacity = minimum;

            Resize(capacity);
        }

        private void Resize(int capacity)
        {
            T[] items = new T[capacity];
            for (int i = 0; i < _size; i++)
                items[i] = _items[i];

            _items = items;
        }

        private void ValidateIndex(int index)
        {
            if ((uint)index >= (uint)_size)
                throw new IndexOutOfRangeException("The list index is outside the collection.");
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly List<T> _list;
            private int _index;
            private T _current;

            internal Enumerator(List<T> list)
            {
                _list = list;
                _index = 0;
                _current = default(T);
            }

            public T Current => _current;

            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                if (_index >= _list._size)
                {
                    _current = default(T);
                    return false;
                }

                _current = _list._items[_index++];
                return true;
            }

            public void Reset()
            {
                _index = 0;
                _current = default(T);
            }

            public void Dispose()
            {
            }
        }
    }
}
