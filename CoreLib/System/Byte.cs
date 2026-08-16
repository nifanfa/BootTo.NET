namespace System
{
    public struct Byte
    {
        public override string ToString() => ((ulong)this).ToString();
    }
}
