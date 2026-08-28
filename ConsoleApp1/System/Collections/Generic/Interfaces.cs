namespace System.Collections.Generic
{
    public interface IEqualityComparer<in T>
    {
        bool Equals(T x, T y);
        int GetHashCode(T obj);
    }

    public interface IComparer<in T>
    {
        int Compare(T x, T y);
    }

    public interface ICollection<T> : IEnumerable<T>
    {
        int Count { get; }
        bool IsReadOnly { get; }
        void Add(T item);
        void Clear();
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
        bool Remove(T item);
    }

    public interface IList<T> : ICollection<T>
    {
        T this[int index] { get; set; }
        int IndexOf(T item);
        void Insert(int index, T item);
        void RemoveAt(int index);
    }

    public interface IReadOnlyCollection<out T> : IEnumerable<T>
    {
        int Count { get; }
    }

    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>
    {
        T this[int index] { get; }
    }

    public interface IDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
    {
        TValue this[TKey key] { get; set; }
        ICollection<TKey> Keys { get; }
        ICollection<TValue> Values { get; }
        bool ContainsKey(TKey key);
        void Add(TKey key, TValue value);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }

    public interface ISet<T> : ICollection<T>
    {
        new bool Add(T item);
        new bool Contains(T item);
        void ExceptWith(IEnumerable<T> other);
        void IntersectWith(IEnumerable<T> other);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
        void SymmetricExceptWith(IEnumerable<T> other);
        void UnionWith(IEnumerable<T> other);
    }

    public readonly struct KeyValuePair<TKey, TValue>(TKey key, TValue value)
    {
        private readonly TKey _key = key;
        private readonly TValue _value = value;

        public TKey Key => _key;
        public TValue Value => _value;
    }
}

