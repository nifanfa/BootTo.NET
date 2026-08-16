namespace System
{
    public struct UInt32
    {
        public override string ToString() => ((ulong)this).ToString();
    }
}
