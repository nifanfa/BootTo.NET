namespace System
{
    public readonly ref partial struct ReadOnlySpan<T>
    {
        public T[] ToArray()
        {
            T[] result = new T[_length];
            for (int index = 0; index < _length; index++)
                result[index] = this[index];
            return result;
        }

        public static implicit operator T[](ReadOnlySpan<T> span) => span.ToArray();
        public static unsafe implicit operator void*(ReadOnlySpan<T> span)
        {
            fixed(T* ptr = &span.GetPinnableReference())
            {
                return ptr;
            }
        }
    }
}
