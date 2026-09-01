using System.Collections.Generic;
using System.Text;

namespace System.Net
{
    public sealed class WebHeaderCollection
    {
        private sealed class Header
        {
            internal readonly string Name;
            internal string Value;

            internal Header(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        private readonly List<Header> _headers = new List<Header>();
        public int Count => _headers.Count;

        public string this[string name]
        {
            get => Get(name);
            set => Set(name, value);
        }

        public void Add(string name, string value)
        {
            Validate(name, value);
            int index = Find(name);
            if (index >= 0)
                _headers[index].Value = _headers[index].Value + ", " + value;
            else
                _headers.Add(new Header(name, value));
        }

        public void Set(string name, string value)
        {
            Validate(name, value);
            int index = Find(name);
            if (index >= 0)
                _headers[index].Value = value;
            else
                _headers.Add(new Header(name, value));
        }

        public string Get(string name)
        {
            if (name == null)
                throw new ArgumentNullException("The header name cannot be null.");
            int index = Find(name);
            return index < 0 ? null : _headers[index].Value;
        }

        public string GetKey(int index)
        {
            if ((uint)index >= (uint)_headers.Count)
                throw new ArgumentException("The header index is outside the collection.");
            return _headers[index].Name;
        }

        public string Get(int index)
        {
            if ((uint)index >= (uint)_headers.Count)
                throw new ArgumentException("The header index is outside the collection.");
            return _headers[index].Value;
        }

        public bool Remove(string name)
        {
            if (name == null)
                throw new ArgumentNullException("The header name cannot be null.");
            int index = Find(name);
            if (index < 0)
                return false;
            _headers.RemoveAt(index);
            return true;
        }

        public void Clear() => _headers.Clear();

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < _headers.Count; i++)
                result.Append(_headers[i].Name).Append(": ").Append(_headers[i].Value).Append("\r\n");
            return result.ToString();
        }

        private int Find(string name)
        {
            for (int i = 0; i < _headers.Count; i++)
                if (EqualsIgnoreCase(_headers[i].Name, name))
                    return i;
            return -1;
        }

        private static void Validate(string name, string value)
        {
            if (string.IsNullOrEmpty(name) || value == null)
                throw new ArgumentException("The header name must be non-empty and the value cannot be null.");

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c <= ' ' || c == ':' || c == '\r' || c == '\n')
                    throw new ArgumentException("The header name contains an invalid character.");
            }
            for (int i = 0; i < value.Length; i++)
                if (value[i] == '\r' || value[i] == '\n')
                    throw new ArgumentException("The header value cannot contain CR or LF characters.");
        }

        internal static bool EqualsIgnoreCase(string left, string right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            for (int i = 0; i < left.Length; i++)
            {
                char a = left[i];
                char b = right[i];
                if (a >= 'A' && a <= 'Z')
                    a = (char)(a + ('a' - 'A'));
                if (b >= 'A' && b <= 'Z')
                    b = (char)(b + ('a' - 'A'));
                if (a != b)
                    return false;
            }
            return true;
        }
    }
}
