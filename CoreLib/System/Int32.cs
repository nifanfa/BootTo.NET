namespace System
{
    public struct Int32
    {
        public override string ToString() => ((long)this).ToString();
    }
}
