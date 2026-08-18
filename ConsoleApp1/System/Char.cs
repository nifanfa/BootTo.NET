namespace System
{
    public partial struct Char
    {
        public override unsafe string ToString()
        {
            char* ptr = stackalloc char[2];
            ptr[0] = this;
            ptr[1] = '\0';
            return new string(ptr);
        }
    }
}
