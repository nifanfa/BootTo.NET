namespace System.Collections.Generic
{
    public readonly struct KeyValuePair<TKey, TValue>(TKey key, TValue value)
    {
        private readonly TKey _key = key;
        private readonly TValue _value = value;

        public TKey Key => _key;
        public TValue Value => _value;
    }
}
