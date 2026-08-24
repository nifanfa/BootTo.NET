namespace System.Text
{
    public abstract class Encoding
    {
        public static readonly Encoding UTF8 = new UTF8Encoding();

        public virtual string GetString(ReadOnlySpan<byte> bytes) => throw new NotSupportedException("This encoding does not implement byte decoding.");
        public virtual string GetString(byte[] bytes) => GetString((ReadOnlySpan<byte>)bytes);
        public virtual byte[] GetBytes(string text) => throw new NotSupportedException("This encoding does not implement text encoding.");
    }
}
