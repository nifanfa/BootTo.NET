namespace System
{
    public static class Convert
    {
        public static unsafe string ToString(ulong value, ulong toBase)
        {
            if (toBase > 0 && toBase <= 16)
            {
                char* x = stackalloc char[128];
                var i = 126;

                x[127] = '\0';

                do
                {
                    var d = value % toBase;
                    value /= toBase;

                    if (d > 9)
                        d += 0x37;
                    else
                        d += 0x30;

                    x[i--] = (char)d;
                } while (value > 0);

                i++;

                return String.Ctor(x + i, 0, 127 - i);
            }
            return null;
        }
    }
}
