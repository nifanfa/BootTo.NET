using System;

internal static unsafe class DateTimeValidation
{
    public static void Run()
    {
        Ensure(sizeof(DateTime) == sizeof(long), "DateTime size");
        DateTime value = new DateTime(2024, 2, 29, 23, 58, 57, 123);
        Ensure(value.Year == 2024 && value.Month == 2 && value.Day == 29, "date components");
        Ensure(value.Hour == 23 && value.Minute == 58 && value.Second == 57 && value.Millisecond == 123,
            "time components");
        Ensure(value.DayOfWeek == DayOfWeek.Thursday, "day of week");
        Ensure(value.Date == new DateTime(2024, 2, 29), "date truncation");
        Ensure(value.TimeOfDay.Ticks == 23L * TimeSpan.TicksPerHour + 58L * TimeSpan.TicksPerMinute +
            57L * TimeSpan.TicksPerSecond + 123L * TimeSpan.TicksPerMillisecond, "time of day");
        Ensure($"{value}" == value.ToString(), "interpolated formatting");
        Ensure(new DateTime(value.Ticks, DateTimeKind.Utc).Kind == DateTimeKind.Utc, "utc kind");
        Ensure(DateTime.SpecifyKind(value, DateTimeKind.Utc).Kind == DateTimeKind.Utc, "specify kind");
        Ensure(DateTime.Compare(value, value.AddTicks(1)) < 0, "date comparison");
        Ensure(DateTime.FromFileTime(value.ToFileTime()) == value, "file time round trip");
        Ensure(DateTime.MinValue.Ticks == 0 && DateTime.MaxValue.Ticks == 3155378975999999999L,
            "date limits");
        Ensure(value.Ticks == 638448479371230000L, "CLR ticks");
        TimeSpan delta = new TimeSpan(877 * TimeSpan.TicksPerMillisecond);
        Ensure(delta.TotalMilliseconds == 877 && delta.Milliseconds == 877, "time span milliseconds");
        Ensure(TimeSpan.FromSeconds(2).TotalMilliseconds == 2000, "time span totals");
        Ensure(TimeSpan.Zero.Ticks == 0 && TimeSpan.MinValue.Ticks == long.MinValue &&
            TimeSpan.MaxValue.Ticks == long.MaxValue, "time span limits");
        Ensure((value + delta).Second == 58 && (value + delta).Millisecond == 0, "date addition");
        Ensure((value + delta - delta).Ticks == value.Ticks, "date subtraction");
        Ensure((value + delta - value).Ticks == delta.Ticks, "date difference");
        Ensure(value.AddDays(1).Month == 3 && value.AddDays(1).Day == 1, "date day addition");
        Ensure(DateTime.IsLeapYear(2024) && !DateTime.IsLeapYear(2100), "leap year calculation");
        Ensure(DateTime.DaysInMonth(2024, 2) == 29 && DateTime.DaysInMonth(2023, 2) == 28,
            "days in month");

        DateTime now = DateTime.UtcNow;
        Ensure(now.Year >= 2020 && now.Year <= 9999, "current year");
        Ensure(now.Kind == DateTimeKind.Utc, "current UTC kind");
        Ensure(now.AddMilliseconds(1).Ticks - now.Ticks == TimeSpan.TicksPerMillisecond,
            "millisecond precision");
        DateTime current = DateTime.Now;
        Ensure(current.Ticks > 0 && current.Kind != DateTimeKind.Unspecified, "current time");
        Console.WriteLine("DateTime validation passed.");
    }

    private static void Ensure(bool condition, string name)
    {
        if (!condition)
            throw new Exception("DateTime validation failed: " + name);
    }
}
