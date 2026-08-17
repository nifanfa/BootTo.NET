namespace System.Timers
{
    internal unsafe class Timer
    {
        EFI_EVENT TimerEvent;

        public required delegate* unmanaged<EFI_EVENT, void*, void> Elapsed;

        public int Interval;

        public Timer(int interval) => Interval = interval;

        public void Start()
        {
            fixed (EFI_EVENT* evt = &TimerEvent)
            {
                gBS->CreateEvent(
                    (uint)EVT_TIMER | EVT_NOTIFY_SIGNAL,
                    TPL_CALLBACK,
                    Elapsed,
                    null,
                    evt
                );
            }
            gBS->SetTimer(
                TimerEvent,
                EFI_TIMER_DELAY.TimerPeriodic,
                (ulong)Interval * 10000
              );
        }

        public void Stop()
        {
            gBS->SetTimer(TimerEvent, EFI_TIMER_DELAY.TimerCancel, 0);
            gBS->CloseEvent(TimerEvent);
        }
    }
}
