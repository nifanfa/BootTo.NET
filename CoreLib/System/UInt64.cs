using System.Runtime;
using System.Runtime.InteropServices;

namespace System
{
    public struct UInt64
    {
        [DllImport("*")]
        public static unsafe extern int snprintf_(byte* buffer, int count, IntPtr format, ulong value);

        public override unsafe string ToString()
        {
            const int bufferSize = 32;
            byte* buffer = stackalloc byte[bufferSize];
            snprintf_(buffer, bufferSize, "%llu"u8._pointer, this);
            char* strBuffer = stackalloc char[bufferSize];
            for (int i = 0; i < bufferSize; i++)
            {
                strBuffer[i] = (char)buffer[i];
                if (buffer[i] == 0)
                    break;
            }
            return string.Ctor(strBuffer);
        }
    }
}
