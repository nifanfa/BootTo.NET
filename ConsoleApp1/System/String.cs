namespace System
{
    public partial class String
    {
        public ref char FirstChar => ref _chars[0];

        public unsafe String(char* value, int startIndex, int length)
        {
            if (value == null)
                throw new ArgumentNullException("The character pointer cannot be null.");
            if (startIndex < 0 || length < 0)
                throw new ArgumentException("The string range is invalid.");

            _chars = new char[length + 1];
            for (int index = 0; index < length; index++)
                _chars[index] = value[startIndex + index];
            Length = length;
        }

        public string ToUpperInvariant() => ConvertAsciiCase(true);

        public static int CompareOrdinal(string left, string right)
        {
            if (ReferenceEquals(left, right))
                return 0;
            if (left == null)
                return -1;
            if (right == null)
                return 1;

            int length = left.Length < right.Length ? left.Length : right.Length;
            for (int index = 0; index < length; index++)
            {
                if (left[index] != right[index])
                    return left[index] < right[index] ? -1 : 1;
            }
            return left.Length < right.Length ? -1 : (left.Length > right.Length ? 1 : 0);
        }

        public static string Concat(params string[] values)
        {
            if (values == null)
                throw new ArgumentNullException("The values cannot be null.");

            int length = 0;
            for (int index = 0; index < values.Length; index++)
                if (values[index] != null)
                    length += values[index].Length;

            char[] result = new char[length];
            int destination = 0;
            for (int index = 0; index < values.Length; index++)
            {
                string value = values[index];
                if (value == null)
                    continue;
                for (int source = 0; source < value.Length; source++)
                    result[destination++] = value[source];
            }
            return new string(result);
        }

        public static string Concat(object value) => value == null ? Empty : value.ToString();
        public static string Concat(object first, object second)
            => Concat(Concat(first), Concat(second));
        public static string Concat(object first, object second, object third)
            => Concat(Concat(first), Concat(second), Concat(third));
        public static string Concat(params object[] values)
        {
            if (values == null)
                throw new ArgumentNullException("The values cannot be null.");
            string[] strings = new string[values.Length];
            for (int index = 0; index < values.Length; index++)
                strings[index] = Concat(values[index]);
            return Concat(strings);
        }

        private string ConvertAsciiCase(bool upper)
        {
            char[] result = new char[Length];
            for (int index = 0; index < Length; index++)
            {
                char value = this[index];
                if (upper && value >= 'a' && value <= 'z')
                    value = (char)(value - ('a' - 'A'));
                result[index] = value;
            }
            return new string(result);
        }
    }
}
