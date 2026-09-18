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

internal static unsafe class EfiNativeMemory
{
    [DllImport("*", EntryPoint = "malloc")]
    private static extern void* Malloc(nuint size);

    [DllImport("*", EntryPoint = "free")]
    private static extern void FreeCore(void* allocation);

    internal static void* Allocate(ulong size) => Malloc((nuint)size);

    internal static void Free(void* allocation) => FreeCore(allocation);

    internal static void Copy(void* destination, void* source, ulong length)
    {
        byte* destinationBytes = (byte*)destination;
        byte* sourceBytes = (byte*)source;
        for (ulong index = 0; index < length; index++)
            destinationBytes[index] = sourceBytes[index];
    }
}

internal static class RuntimeObjectHandle
{
    private static object[] handles = new object[4];

    internal static IntPtr Allocate(object value)
    {
        for (int index = 0; index < handles.Length; index++)
        {
            if (handles[index] != null)
                continue;
            handles[index] = value;
            return new IntPtr(index + 1);
        }

        int oldLength = handles.Length;
        Array.Resize(ref handles, oldLength * 2);
        handles[oldLength] = value;
        return new IntPtr(oldLength + 1);
    }

    internal static T Get<T>(IntPtr handle) where T : class
    {
        int index = (int)handle - 1;
        if ((uint)index >= (uint)handles.Length)
            return null;
        return handles[index] as T;
    }

    internal static void Free(IntPtr handle)
    {
        int index = (int)handle - 1;
        if ((uint)index < (uint)handles.Length)
            handles[index] = null;
    }
}
