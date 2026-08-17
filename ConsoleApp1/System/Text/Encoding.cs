namespace System.Text
{
    internal abstract class Encoding
    {
        public static Encoding UTF8 = new UTF8Encoding();

        public virtual string GetString(ReadOnlySpan<byte> bytes) => throw new NotSupportedException();
        public virtual byte[] GetBytes(string text) => throw new NotSupportedException();
    }
}
