using System.Text;

namespace System
{
    public partial struct Int64
    {
        public override unsafe string ToString()
        {
            byte[] buffer = new byte[32];
            fixed (byte* ptr = buffer)
            {
                snprintf(ptr, buffer.Length, "%lld"u8, this);
                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
