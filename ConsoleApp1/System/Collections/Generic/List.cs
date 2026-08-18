namespace System.Collections.Generic
{
    public class List<T> : IEnumerable<T>
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
                throw new IndexOutOfRangeException();

            _items = new T[capacity];
        }

        public List(T[] items)
        {
            if (items == null)
                throw new ArgumentNullException();

            _items = new T[items.Length];
            for (int i = 0; i < items.Length; i++)
                _items[i] = items[i];

            _size = items.Length;
        }

        public int Count => _size;

        public int Capacity
        {
            get => _items.Length;
            set
            {
                if (value < _size)
                    throw new IndexOutOfRangeException();

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
                throw new ArgumentNullException();

            EnsureCapacity(_size + items.Length);
            for (int i = 0; i < items.Length; i++)
                _items[_size + i] = items[i];

            _size += items.Length;
        }

        public void Insert(int index, T item)
        {
            if ((uint)index > (uint)_size)
                throw new IndexOutOfRangeException();

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

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new Enumerator(this);

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
                throw new IndexOutOfRangeException();
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

            object System.Collections.IEnumerator.Current => _current;

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
