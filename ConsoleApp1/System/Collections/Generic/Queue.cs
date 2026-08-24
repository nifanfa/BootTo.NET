namespace System.Collections.Generic
{
    public class Queue<T> : IEnumerable<T>
    {
        private const int DefaultCapacity = 4;

        private T[] _items;
        private int _head;
        private int _count;

        public Queue()
        {
            _items = new T[0];
        }

        public Queue(int capacity)
        {
            if (capacity < 0)
                throw new IndexOutOfRangeException("The queue capacity cannot be negative.");

            _items = new T[capacity];
        }

        public int Count => _count;

        public void Enqueue(T item)
        {
            EnsureCapacity(_count + 1);
            int tail = (_head + _count) % _items.Length;
            _items[tail] = item;
            _count++;
        }

        public T Dequeue()
        {
            if (!TryDequeue(out T item))
                throw new InvalidOperationException("Cannot dequeue from an empty queue.");

            return item;
        }

        public bool TryDequeue(out T item)
        {
            if (_count == 0)
            {
                item = default(T);
                return false;
            }

            item = _items[_head];
            _items[_head] = default(T);
            _head = (_head + 1) % _items.Length;
            _count--;
            if (_count == 0)
                _head = 0;
            return true;
        }

        public T Peek()
        {
            if (!TryPeek(out T item))
                throw new InvalidOperationException("Cannot peek at an empty queue.");

            return item;
        }

        public bool TryPeek(out T item)
        {
            if (_count == 0)
            {
                item = default(T);
                return false;
            }

            item = _items[_head];
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < _count; i++)
                _items[(_head + i) % _items.Length] = default(T);

            _head = 0;
            _count = 0;
        }

        public T[] ToArray()
        {
            T[] result = new T[_count];
            for (int i = 0; i < _count; i++)
                result[i] = _items[(_head + i) % _items.Length];

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

            T[] items = new T[capacity];
            for (int i = 0; i < _count; i++)
                items[i] = _items[(_head + i) % _items.Length];

            _items = items;
            _head = 0;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly Queue<T> _queue;
            private int _index;
            private T _current;

            internal Enumerator(Queue<T> queue)
            {
                _queue = queue;
                _index = 0;
                _current = default(T);
            }

            public T Current => _current;

            object System.Collections.IEnumerator.Current => _current;

            public bool MoveNext()
            {
                if (_index >= _queue._count)
                {
                    _current = default(T);
                    return false;
                }

                _current = _queue._items[(_queue._head + _index) % _queue._items.Length];
                _index++;
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
