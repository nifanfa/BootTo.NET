namespace System.Collections.Generic
{
    public sealed class EqualityComparer<T> : IEqualityComparer<T>
    {
        private static readonly EqualityComparer<T> s_default = new EqualityComparer<T>();

        public static EqualityComparer<T> Default => s_default;

        public bool Equals(T x, T y)
        {
            object left = x;
            object right = y;
            return left == null ? right == null : left.Equals(right);
        }

        public int GetHashCode(T obj)
        {
            object value = obj;
            return value == null ? 0 : value.GetHashCode();
        }
    }
}
