namespace System
{
    public readonly struct TimeSpan
    {
        public static readonly TimeSpan MinValue = new TimeSpan(long.MinValue);
        public static readonly TimeSpan MaxValue = new TimeSpan(long.MaxValue);

        public const long TicksPerMillisecond = 10000;
        public const long TicksPerSecond = TicksPerMillisecond * 1000;
        public const long TicksPerMinute = TicksPerSecond * 60;
        public const long TicksPerHour = TicksPerMinute * 60;
        public const long TicksPerDay = TicksPerHour * 24;

        private readonly long _ticks;

        public TimeSpan(long ticks) => _ticks = ticks;
        public TimeSpan(int hours, int minutes, int seconds)
            => _ticks = (long)hours * TicksPerHour + (long)minutes * TicksPerMinute + (long)seconds * TicksPerSecond;

        public long Ticks => _ticks;
        public int Days => (int)(_ticks / TicksPerDay);
        public int Hours => (int)((_ticks / TicksPerHour) % 24);
        public int Minutes => (int)((_ticks / TicksPerMinute) % 60);
        public int Seconds => (int)((_ticks / TicksPerSecond) % 60);
        public int Milliseconds => (int)((_ticks / TicksPerMillisecond) % 1000);
        public double TotalMilliseconds => (double)_ticks / TicksPerMillisecond;
        public double TotalSeconds => (double)_ticks / TicksPerSecond;

        public static TimeSpan FromMilliseconds(double value) => new TimeSpan((long)(value * TicksPerMillisecond));
        public static TimeSpan FromSeconds(double value) => new TimeSpan((long)(value * TicksPerSecond));
        public static TimeSpan FromMinutes(double value) => new TimeSpan((long)(value * TicksPerMinute));
        public static TimeSpan FromHours(double value) => new TimeSpan((long)(value * TicksPerHour));
        public static TimeSpan FromDays(double value) => new TimeSpan((long)(value * TicksPerDay));

        public override string ToString()
            => string.Format("{0}:{1:D2}:{2:D2}", Hours, Minutes, Seconds);

        public static TimeSpan operator +(TimeSpan left, TimeSpan right) => new TimeSpan(left._ticks + right._ticks);
        public static TimeSpan operator -(TimeSpan left, TimeSpan right) => new TimeSpan(left._ticks - right._ticks);
        public static bool operator ==(TimeSpan left, TimeSpan right) => left._ticks == right._ticks;
        public static bool operator !=(TimeSpan left, TimeSpan right) => left._ticks != right._ticks;
        public override bool Equals(object obj) => obj is TimeSpan && ((TimeSpan)obj)._ticks == _ticks;
        public override int GetHashCode() => (int)(_ticks ^ (_ticks >> 32));
    }
}
