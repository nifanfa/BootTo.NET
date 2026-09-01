namespace System
{
    public readonly struct DateTime
    {
        public static readonly DateTime MinValue = new DateTime(0);
        public static readonly DateTime MaxValue = new DateTime(3155378975999999999);

        private const long TicksAtUnixEpoch = 621355968000000000;
        private readonly long _ticks;

        public DateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0, 0) { }

        public DateTime(int year, int month, int day, int hour, int minute, int second)
            : this(year, month, day, hour, minute, second, 0) { }

        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            Validate(year, month, day, hour, minute, second, millisecond);
            _ticks = GetTicks(year, month, day) +
                (long)hour * TimeSpan.TicksPerHour +
                (long)minute * TimeSpan.TicksPerMinute +
                (long)second * TimeSpan.TicksPerSecond +
                (long)millisecond * TimeSpan.TicksPerMillisecond;
        }

        private DateTime(long ticks) => _ticks = ticks;

        public long Ticks => _ticks;
        public int Year => GetDatePart(0);
        public int Month => GetDatePart(1);
        public int Day => GetDatePart(2);
        public int Hour => (int)((_ticks / TimeSpan.TicksPerHour) % 24);
        public int Minute => (int)((_ticks / TimeSpan.TicksPerMinute) % 60);
        public int Second => (int)((_ticks / TimeSpan.TicksPerSecond) % 60);
        public int Millisecond => (int)((_ticks / TimeSpan.TicksPerMillisecond) % 1000);
        public DateTime Date => new DateTime(_ticks - _ticks % TimeSpan.TicksPerDay);
        public TimeSpan TimeOfDay => new TimeSpan(_ticks % TimeSpan.TicksPerDay);
        public DayOfWeek DayOfWeek => (DayOfWeek)((_ticks / TimeSpan.TicksPerDay + 1) % 7);
        public DateTimeKind Kind => DateTimeKind.Unspecified;

        public static DateTime Now => ReadFirmwareTime();
        public static DateTime UtcNow => ReadFirmwareTime();
        public static DateTime Today => Now.Date;

        public static DateTime FromFileTime(long fileTime) => new DateTime(fileTime + TicksAtUnixEpoch);
        public long ToFileTime() => _ticks - TicksAtUnixEpoch;

        public static int Compare(DateTime left, DateTime right)
            => left._ticks < right._ticks ? -1 : (left._ticks > right._ticks ? 1 : 0);

        public int CompareTo(DateTime other) => Compare(this, other);

        public static DateTime SpecifyKind(DateTime value, DateTimeKind kind) => value;

        public DateTime ToUniversalTime() => this;
        public DateTime ToLocalTime() => this;

        public DateTime Add(TimeSpan value) => new DateTime(_ticks + value.Ticks);
        public DateTime AddMilliseconds(double value) => Add(TimeSpan.FromMilliseconds(value));
        public DateTime AddSeconds(double value) => Add(TimeSpan.FromSeconds(value));
        public DateTime AddMinutes(double value) => Add(TimeSpan.FromMinutes(value));
        public DateTime AddHours(double value) => Add(TimeSpan.FromHours(value));
        public DateTime AddDays(double value) => Add(TimeSpan.FromDays(value));

        public override string ToString()
            => string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", Year, Month, Day, Hour, Minute, Second);

        public override bool Equals(object obj) => obj is DateTime && ((DateTime)obj)._ticks == _ticks;
        public override int GetHashCode() => (int)(_ticks ^ (_ticks >> 32));
        public static bool operator ==(DateTime left, DateTime right) => left._ticks == right._ticks;
        public static bool operator !=(DateTime left, DateTime right) => left._ticks != right._ticks;
        public static bool operator <(DateTime left, DateTime right) => left._ticks < right._ticks;
        public static bool operator >(DateTime left, DateTime right) => left._ticks > right._ticks;
        public static bool operator <=(DateTime left, DateTime right) => left._ticks <= right._ticks;
        public static bool operator >=(DateTime left, DateTime right) => left._ticks >= right._ticks;
        public static TimeSpan operator -(DateTime left, DateTime right) => new TimeSpan(left._ticks - right._ticks);
        public static DateTime operator +(DateTime value, TimeSpan span) => value.Add(span);
        public static DateTime operator -(DateTime value, TimeSpan span) => value.Add(new TimeSpan(-span.Ticks));

        public static bool IsLeapYear(int year)
        {
            if (year < 1 || year > 9999)
                throw new ArgumentException("The year must be between 1 and 9999.");
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }

        public static int DaysInMonth(int year, int month)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("The month must be between 1 and 12.");
            int[] days = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            if (month == 2 && IsLeapYear(year))
                return 29;
            return days[month - 1];
        }

        private static unsafe DateTime ReadFirmwareTime()
        {
            if (gRT == null || gRT->GetTime == null)
                return new DateTime(1970, 1, 1);

            EFI_TIME time = default;
            EFI_STATUS status = gRT->GetTime(&time, null);
            if ((ulong)status != EFI_SUCCESS)
                return new DateTime(1970, 1, 1);
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, (int)(time.Nanosecond / 1000000));
        }

        private int GetDatePart(int part)
        {
            long days = _ticks / TimeSpan.TicksPerDay;
            int low = 1;
            int high = 9999;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (GetTicks(mid, 1, 1) / TimeSpan.TicksPerDay <= days)
                    low = mid;
                else
                    high = mid - 1;
            }
            int year = low;
            long dayOfYear = days - GetTicks(year, 1, 1) / TimeSpan.TicksPerDay;
            if (part == 0)
                return year;

            int month = 1;
            while (month < 12 && dayOfYear >= DaysInMonth(year, month))
            {
                dayOfYear -= DaysInMonth(year, month);
                month++;
            }
            return part == 1 ? month : (int)dayOfYear + 1;
        }

        private static long GetTicks(int year, int month, int day)
        {
            long y = year - 1;
            long days = y * 365 + y / 4 - y / 100 + y / 400;
            int[] daysToMonth = new int[] { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
            days += daysToMonth[month - 1];
            if (month > 2 && IsLeapYear(year))
                days++;
            days += day - 1;
            return days * TimeSpan.TicksPerDay;
        }

        private static void Validate(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            if (year < 1 || year > 9999 || month < 1 || month > 12 || day < 1 || day > DaysInMonth(year, month) ||
                hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59 || millisecond < 0 || millisecond > 999)
                throw new ArgumentException("The date or time components are outside their valid ranges.");
        }
    }
}
