namespace System.Diagnostics
{
    public unsafe sealed class Stopwatch
    {
        private const long FemtosecondsPerSecond = 1000000000000000L;

        private static CpuArchProtocol* s_cpu;
        private static long s_lastTimestamp;

        public static readonly long Frequency;
        public static readonly bool IsHighResolution;

        private long _elapsed;
        private long _startTimestamp;
        private bool _isRunning;

        static Stopwatch()
        {
            Frequency = TimeSpan.TicksPerSecond;
            IsHighResolution = false;

            if (gBS == null)
                return;

            EFI_GUID cpuArchGuid = new EFI_GUID(
                0x26baccb1, 0x6f42, 0x11d4,
                0xbc, 0xe7, 0x00, 0x80, 0xc7, 0x3c, 0x88, 0x81);
            CpuArchProtocol* cpu = null;
            EFI_STATUS locateStatus = gBS->LocateProtocol(
                &cpuArchGuid,
                null,
                (void**)&cpu);
            if ((ulong)locateStatus != EFI_SUCCESS || cpu == null || cpu->GetTimerValue == null)
                return;

            ulong timerValue = 0;
            ulong timerPeriod = 0;
            EFI_STATUS timerStatus = cpu->GetTimerValue(cpu, 0, &timerValue, &timerPeriod);
            if ((ulong)timerStatus != EFI_SUCCESS || timerPeriod == 0 || timerPeriod > FemtosecondsPerSecond)
                return;

            long frequency = FemtosecondsPerSecond / (long)timerPeriod;
            if (frequency <= 0)
                return;

            s_cpu = cpu;
            s_lastTimestamp = unchecked((long)timerValue);
            Frequency = frequency;
            IsHighResolution = true;
        }

        public TimeSpan Elapsed => new TimeSpan(ToTimeSpanTicks(ElapsedTicks));
        public long ElapsedMilliseconds => ToTimeSpanTicks(ElapsedTicks) / TimeSpan.TicksPerMillisecond;
        public long ElapsedTicks
        {
            get
            {
                long elapsed = _elapsed;
                if (_isRunning)
                    elapsed = unchecked(elapsed + GetTimestamp() - _startTimestamp);
                return elapsed;
            }
        }
        public bool IsRunning => _isRunning;

        public static Stopwatch StartNew()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            return stopwatch;
        }

        public static long GetTimestamp()
        {
            if (s_cpu != null)
            {
                ulong timerValue = 0;
                if ((ulong)s_cpu->GetTimerValue(s_cpu, 0, &timerValue, null) == EFI_SUCCESS)
                {
                    s_lastTimestamp = unchecked((long)timerValue);
                    return s_lastTimestamp;
                }

                // Keep a single clock source for existing instances. A failed
                // firmware read is treated as a zero-length interval.
                return s_lastTimestamp;
            }

            return DateTime.UtcNow.Ticks;
        }

        public void Start()
        {
            if (_isRunning)
                return;
            _startTimestamp = GetTimestamp();
            _isRunning = true;
        }

        public void Stop()
        {
            if (!_isRunning)
                return;
            _elapsed = unchecked(_elapsed + GetTimestamp() - _startTimestamp);
            _isRunning = false;
        }

        public void Reset()
        {
            _elapsed = 0;
            _startTimestamp = 0;
            _isRunning = false;
        }

        public void Restart()
        {
            _elapsed = 0;
            _startTimestamp = GetTimestamp();
            _isRunning = true;
        }

        private static long ToTimeSpanTicks(long timestampTicks)
            => Scale(timestampTicks, Frequency, TimeSpan.TicksPerSecond);

        private static long Scale(long value, long sourceFrequency, long destinationFrequency)
        {
            long whole = value / sourceFrequency;
            long remainder = value % sourceFrequency;
            return unchecked(whole * destinationFrequency + remainder * destinationFrequency / sourceFrequency);
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct CpuArchProtocol
        {
            public void* FlushDataCache;
            public void* EnableInterrupt;
            public void* DisableInterrupt;
            public void* GetInterruptState;
            public void* Init;
            public void* RegisterInterruptHandler;
            public delegate* unmanaged<CpuArchProtocol*, uint, ulong*, ulong*, EFI_STATUS> GetTimerValue;
        }
    }
}
