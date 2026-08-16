namespace System
{
    public struct UInt16
    {
        public override string ToString() => ((ulong)this).ToString();
    }
}
