using System.Text;

namespace System
{
    public partial struct UInt64
    {
        public override unsafe string ToString()
        {
            byte[] buffer = new byte[32];
            fixed (byte* ptr = buffer)
            {
                int length = snprintf(ptr, buffer.Length, "%llu"u8, this);
                return Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, length));
            }
        }
    }
}
