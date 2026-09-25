using System;
using System.Runtime;
using System.Runtime.InteropServices;

public static unsafe class RuntimeExports
{
    [RuntimeExport("GetCurrentTimeMilliseconds")]
    public static long GetCurrentTimeMilliseconds()
    {
        if (gRT == null || gRT->GetTime == null)
            return 0;

        EFI_TIME time = default;
        if ((ulong)gRT->GetTime(&time, null) != EFI_SUCCESS)
            return 0;

        const long ticksAtUnixEpoch = 621355968000000000;
        DateTime value = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute,
            time.Second, (int)(time.Nanosecond / 1000000), DateTimeKind.Utc);
        return (value.Ticks - ticksAtUnixEpoch) / TimeSpan.TicksPerMillisecond;
    }

    [RuntimeExport("Enter")]
    public static void Enter(object value)
    {
        bool lockTaken = false;
        TaskScheduler.Enter(value, ref lockTaken);
    }

    [RuntimeExport("Exit")]
    public static void Exit(object value) => TaskScheduler.Exit(value);

}
