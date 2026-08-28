namespace System.Collections.Generic
{
    public class Stack<T> : IEnumerable<T>, IReadOnlyCollection<T>
    {
        private T[] _items;
        private int _count;

        public Stack()
            : this(0)
        {
        }

        public Stack(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("The stack capacity cannot be negative.");
            _items = new T[capacity];
        }

        public Stack(IEnumerable<T> collection)
            : this()
        {
            if (collection == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            foreach (T item in collection)
                Push(item);
        }

        public int Count => _count;

        public void Push(T item)
        {
            EnsureCapacity(_count + 1);
            _items[_count++] = item;
        }

        public T Pop()
        {
            if (_count == 0)
                throw new InvalidOperationException("Cannot pop from an empty stack.");
            T result = _items[--_count];
            _items[_count] = default;
            return result;
        }

        public bool TryPop(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }
            result = Pop();
            return true;
        }

        public T Peek()
        {
            if (_count == 0)
                throw new InvalidOperationException("Cannot peek at an empty stack.");
            return _items[_count - 1];
        }

        public bool TryPeek(out T result)
        {
            if (_count == 0)
            {
                result = default;
                return false;
            }
            result = _items[_count - 1];
            return true;
        }

        public bool Contains(T item) => Array.IndexOf(_items, item, 0, _count) >= 0;

        public void Clear()
        {
            for (int i = 0; i < _count; i++)
                _items[i] = default;
            _count = 0;
        }

        public T[] ToArray()
        {
            T[] result = new T[_count];
            for (int i = 0; i < _count; i++)
                result[i] = _items[_count - i - 1];
            return result;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

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
            private readonly Stack<T> _stack;
            private int _index;
            private T _current;

            internal Enumerator(Stack<T> stack)
            {
                _stack = stack;
                _index = 0;
                _current = default;
            }

            public T Current => _current;
            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                if (_index >= _stack._count)
                {
                    _current = default;
                    return false;
                }
                _current = _stack._items[_stack._count - ++_index];
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
