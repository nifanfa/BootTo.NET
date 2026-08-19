namespace System
{
    public static class Convert
    {
        public static int ToUInt16(bool boolean)
        {
            return boolean ? 1 : 0;
        }

        public static int ToInt16(bool boolean)
        {
            return boolean ? 1 : 0;
        }

        public static int ToInt16(byte b)
        {
            return b;
        }

        public static bool ToBoolean(int integer)
        {
            return integer != 0;
        }

        public static byte ToByte(int v)
        {
            return (byte)v;
        }

        public static byte ToByte(uint v)
        {
            return (byte)v;
        }

        public static int ToInt32(byte b)
        {
            return b;
        }

        public static int ToInt32(int b)
        {
            return b;
        }
    }
}