using System.Collections.Generic;

namespace System
{
    public abstract class StringComparer : IComparer<string>, IEqualityComparer<string>
    {
        private sealed class OrdinalComparer : StringComparer
        {
            private readonly bool _ignoreCase;

            internal OrdinalComparer(bool ignoreCase) => _ignoreCase = ignoreCase;

            public override int Compare(string x, string y)
                => _ignoreCase ? CompareIgnoreCase(x, y) : string.CompareOrdinal(x, y);

            public override bool Equals(string x, string y)
                => _ignoreCase ? EqualsIgnoreCase(x, y) : string.Equals(x, y);

            public override int GetHashCode(string obj)
            {
                if (obj == null)
                    return 0;
                return _ignoreCase ? HashIgnoreCase(obj) : obj.GetHashCode();
            }
        }

        public static StringComparer Ordinal { get; } = new OrdinalComparer(false);
        public static StringComparer OrdinalIgnoreCase { get; } = new OrdinalComparer(true);

        public abstract int Compare(string x, string y);
        public abstract bool Equals(string x, string y);
        public abstract int GetHashCode(string obj);

        bool IEqualityComparer<string>.Equals(string x, string y) => Equals(x, y);
        int IEqualityComparer<string>.GetHashCode(string obj) => GetHashCode(obj);

        private static int CompareIgnoreCase(string x, string y)
        {
            if (x == null) return y == null ? 0 : -1;
            if (y == null) return 1;
            int length = x.Length < y.Length ? x.Length : y.Length;
            for (int i = 0; i < length; i++)
            {
                char left = LowerAscii(x[i]);
                char right = LowerAscii(y[i]);
                if (left != right)
                    return left < right ? -1 : 1;
            }
            return x.Length < y.Length ? -1 : (x.Length > y.Length ? 1 : 0);
        }

        private static bool EqualsIgnoreCase(string x, string y)
            => CompareIgnoreCase(x, y) == 0;

        private static int HashIgnoreCase(string value)
        {
            unchecked
            {
                int hash = 5381;
                for (int i = 0; i < value.Length; i++)
                    hash = ((hash << 5) + hash) ^ LowerAscii(value[i]);
                return hash;
            }
        }

        private static char LowerAscii(char value)
            => value >= 'A' && value <= 'Z' ? (char)(value + ('a' - 'A')) : value;
    }
}
