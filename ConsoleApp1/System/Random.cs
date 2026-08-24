namespace System
{
    public class Random
    {
        private static int s_hardwareRdrand = -1;

        public static Random Shared { get; } = new Random();

        public Random() => EnsureHardwareRandom();

        public Random(int seed)
        {
            EnsureHardwareRandom();
        }

        public virtual int Next()
            => (int)((ulong)NextUInt() * 2147483647UL >> 32);

        public virtual int Next(int maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentException("The maximum random value cannot be negative.");
            if (maxValue == 0)
                return 0;
            return (int)((ulong)NextUInt() * (ulong)maxValue >> 32);
        }

        public virtual int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentException("The minimum random value cannot exceed the maximum.");
            long range = (long)maxValue - minValue;
            if (range == 0)
                return minValue;
            return (int)(minValue + (long)((ulong)NextUInt() * (ulong)range >> 32));
        }

        public virtual long NextInt64()
            => (long)(NextUInt64() & 0x7FFFFFFFFFFFFFFFUL);

        public virtual long NextInt64(long maxValue)
        {
            if (maxValue < 0)
                throw new ArgumentException("The maximum random value cannot be negative.");
            if (maxValue == 0)
                return 0;
            return (long)(NextUInt64() % (ulong)maxValue);
        }

        public virtual long NextInt64(long minValue, long maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentException("The minimum random value cannot exceed the maximum.");
            ulong range = (ulong)(maxValue - minValue);
            if (range == 0)
                return minValue;
            return minValue + (long)(NextUInt64() % range);
        }

        public virtual double NextDouble() => (double)NextUInt() / 4294967296.0;

        public virtual void NextBytes(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("The random byte buffer cannot be null.");
            NextBytes(buffer, 0, buffer.Length);
        }

        public virtual void NextBytes(byte[] buffer, int index, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("The random byte buffer cannot be null.");
            if (index < 0 || count < 0 || index > buffer.Length - count)
                throw new ArgumentException("The random byte buffer range is invalid.");
            for (int i = 0; i < count; i++)
                buffer[index + i] = (byte)NextUInt();
        }

        protected virtual double Sample() => NextDouble();

        private uint NextUInt()
            => (uint)NextHardwareRandom();

        private ulong NextUInt64() => NextHardwareRandom();

        private static void EnsureHardwareRandom()
        {
            if (s_hardwareRdrand < 0)
                s_hardwareRdrand = SupportRdrand();
            if (s_hardwareRdrand == 0)
                throw new NotSupportedException("The processor does not support the RDRAND instruction.");
        }

        private static ulong NextHardwareRandom()
        {
            EnsureHardwareRandom();
            for (int attempt = 0; attempt < 10; attempt++)
            {
                if (Rdrand64(out ulong value) != 0)
                    return value;
            }

            throw new InvalidOperationException("The hardware random number generator did not return a value.");
        }
    }
}
