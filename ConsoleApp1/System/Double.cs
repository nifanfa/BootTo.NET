using System.Text;

namespace System
{
    public partial struct Double
    {
        public override unsafe string ToString()
        {
            byte[] buffer = new byte[32];
            fixed (byte* ptr = buffer)
            {
                snprintf(ptr, buffer.Length, "%lf"u8, this);
                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
