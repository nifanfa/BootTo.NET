namespace System.Text
{
    public sealed class StringBuilder
    {
        private char[] _buffer;
        private int _length;

        public StringBuilder()
            : this(16)
        {
        }

        public StringBuilder(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException();

            _buffer = new char[capacity == 0 ? 1 : capacity];
        }

        public StringBuilder(string value)
        {
            if (value == null)
                throw new ArgumentNullException();

            _buffer = new char[value.Length == 0 ? 1 : value.Length];
            Append(value);
        }

        public StringBuilder(string value, int capacity)
        {
            if (value == null)
                throw new ArgumentNullException();
            if (capacity < value.Length)
                capacity = value.Length;

            _buffer = new char[capacity == 0 ? 1 : capacity];
            Append(value);
        }

        public int Length
        {
            get => _length;
            set
            {
                if (value < 0)
                    throw new ArgumentException();
                EnsureCapacity(value);
                if (value > _length)
                {
                    for (int i = _length; i < value; i++)
                        _buffer[i] = '\0';
                }
                _length = value;
            }
        }

        public int Capacity
        {
            get => _buffer.Length;
            set
            {
                if (value < _length)
                    throw new ArgumentException();
                if (value == 0)
                    value = 1;
                if (value != _buffer.Length)
                {
                    char[] buffer = new char[value];
                    for (int i = 0; i < _length; i++)
                        buffer[i] = _buffer[i];
                    _buffer = buffer;
                }
            }
        }

        public char this[int index]
        {
            get
            {
                ValidateIndex(index);
                return _buffer[index];
            }
            set
            {
                ValidateIndex(index);
                _buffer[index] = value;
            }
        }

        public StringBuilder Append(char value)
        {
            EnsureCapacity(_length + 1);
            _buffer[_length++] = value;
            return this;
        }

        public StringBuilder Append(char value, int repeatCount)
        {
            if (repeatCount < 0)
                throw new ArgumentException();
            EnsureCapacity(_length + repeatCount);
            for (int i = 0; i < repeatCount; i++)
                _buffer[_length++] = value;
            return this;
        }

        public StringBuilder Append(string value)
        {
            if (value == null)
                return this;
            EnsureCapacity(_length + value.Length);
            for (int i = 0; i < value.Length; i++)
                _buffer[_length++] = value[i];
            return this;
        }

        public StringBuilder Append(string value, int startIndex, int count)
        {
            if (value == null)
                return this;
            if (startIndex < 0 || count < 0 || startIndex > value.Length - count)
                throw new ArgumentException();
            EnsureCapacity(_length + count);
            for (int i = 0; i < count; i++)
                _buffer[_length++] = value[startIndex + i];
            return this;
        }

        public StringBuilder Append(char[] value)
        {
            if (value == null)
                return this;
            return Append(value, 0, value.Length);
        }

        public StringBuilder Append(char[] value, int startIndex, int count)
        {
            if (value == null)
                return this;
            if (startIndex < 0 || count < 0 || startIndex > value.Length - count)
                throw new ArgumentException();
            EnsureCapacity(_length + count);
            for (int i = 0; i < count; i++)
                _buffer[_length++] = value[startIndex + i];
            return this;
        }

        public StringBuilder Append(object value)
            => Append(value == null ? null : value.ToString());

        public StringBuilder Append(int value) => Append(value.ToString());
        public StringBuilder Append(uint value) => Append(value.ToString());
        public StringBuilder Append(long value) => Append(value.ToString());
        public StringBuilder Append(ulong value) => Append(value.ToString());
        public StringBuilder Append(bool value) => Append(value.ToString());

        public StringBuilder AppendLine()
            => Append('\r').Append('\n');

        public StringBuilder AppendLine(string value)
            => Append(value).Append('\r').Append('\n');

        public StringBuilder Clear()
        {
            _length = 0;
            return this;
        }

        public StringBuilder Remove(int startIndex, int length)
        {
            if (startIndex < 0 || length < 0 || startIndex > _length - length)
                throw new ArgumentException();
            for (int i = startIndex; i < _length - length; i++)
                _buffer[i] = _buffer[i + length];
            _length -= length;
            return this;
        }

        public StringBuilder Replace(char oldChar, char newChar)
        {
            for (int i = 0; i < _length; i++)
                if (_buffer[i] == oldChar)
                    _buffer[i] = newChar;
            return this;
        }

        public override string ToString()
        {
            if (_length == 0)
                return string.Empty;

            char[] result = new char[_length];
            for (int i = 0; i < _length; i++)
                result[i] = _buffer[i];
            return new string(result);
        }

        public string ToString(int startIndex, int length)
        {
            if (startIndex < 0 || length < 0 || startIndex > _length - length)
                throw new ArgumentException();
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = _buffer[startIndex + i];
            return new string(result);
        }

        private void EnsureCapacity(int required)
        {
            if (required <= _buffer.Length)
                return;

            int capacity = _buffer.Length * 2;
            if (capacity < required)
                capacity = required;
            Capacity = capacity;
        }

        private void ValidateIndex(int index)
        {
            if ((uint)index >= (uint)_length)
                throw new IndexOutOfRangeException();
        }
    }
}
