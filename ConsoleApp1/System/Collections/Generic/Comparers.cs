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

    public sealed class Comparer<T> : IComparer<T>
    {
        private static readonly Comparer<T> s_default = new Comparer<T>();

        public static Comparer<T> Default => s_default;

        public int Compare(T x, T y)
        {
            object left = x;
            object right = y;
            if (left == null)
                return right == null ? 0 : -1;
            if (right == null)
                return 1;

            IComparable<T> generic = left as IComparable<T>;
            if (generic != null)
                return generic.CompareTo(y);

            IComparable comparable = left as IComparable;
            if (comparable != null)
                return comparable.CompareTo(right);

            if (typeof(T) == typeof(string))
                return string.CompareOrdinal((string)left, (string)right);
            if (typeof(T) == typeof(int))
                return CompareInt((int)left, (int)right);
            if (typeof(T) == typeof(uint))
                return CompareUInt((uint)left, (uint)right);
            if (typeof(T) == typeof(long))
                return CompareLong((long)left, (long)right);
            if (typeof(T) == typeof(ulong))
                return CompareULong((ulong)left, (ulong)right);
            if (typeof(T) == typeof(short))
                return CompareInt((short)left, (short)right);
            if (typeof(T) == typeof(ushort))
                return CompareUInt((ushort)left, (ushort)right);
            if (typeof(T) == typeof(byte))
                return CompareUInt((byte)left, (byte)right);
            if (typeof(T) == typeof(sbyte))
                return CompareInt((sbyte)left, (sbyte)right);
            if (typeof(T) == typeof(float))
                return CompareFloat((float)left, (float)right);
            if (typeof(T) == typeof(double))
                return CompareDouble((double)left, (double)right);

            throw new InvalidOperationException("The type does not implement a comparable interface.");
        }

        private static int CompareInt(int left, int right) => left < right ? -1 : (left > right ? 1 : 0);
        private static int CompareUInt(uint left, uint right) => left < right ? -1 : (left > right ? 1 : 0);
        private static int CompareLong(long left, long right) => left < right ? -1 : (left > right ? 1 : 0);
        private static int CompareULong(ulong left, ulong right) => left < right ? -1 : (left > right ? 1 : 0);
        private static int CompareFloat(float left, float right) => left < right ? -1 : (left > right ? 1 : 0);
        private static int CompareDouble(double left, double right) => left < right ? -1 : (left > right ? 1 : 0);
    }

    internal sealed class ComparisonComparer<T> : IComparer<T>
    {
        private readonly Comparison<T> _comparison;

        internal ComparisonComparer(Comparison<T> comparison)
            => _comparison = comparison ?? throw new ArgumentNullException("The comparison cannot be null.");

        public int Compare(T x, T y) => _comparison(x, y);
    }
}
