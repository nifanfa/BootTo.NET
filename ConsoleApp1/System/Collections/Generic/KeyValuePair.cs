namespace System.Collections.Generic
{
    public readonly struct KeyValuePair<TKey, TValue>
    {
        private readonly TKey _key;
        private readonly TValue _value;

        public KeyValuePair(TKey key, TValue value)
        {
            _key = key;
            _value = value;
        }

        public TKey Key => _key;

        public TValue Value => _value;
    }
}
