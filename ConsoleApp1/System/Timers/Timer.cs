using Internal.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Timers
{
    internal unsafe class Timer
    {
        public delegate void TimerEventHandler(EFI_EVENT Event, IntPtr Context);
        public event TimerEventHandler Elapsed;
        EFI_EVENT TimerEvent;
        bool Started;

        [UnmanagedCallersOnly]
        static void TimerProc(EFI_EVENT Event, void* Context)
        {
            IntPtr context = (IntPtr)Context;
            Timer timer = Unsafe.As<IntPtr, Timer>(ref context);
            timer.Elapsed?.Invoke(Event, context);
        }

        public double Interval;

        public Timer(double interval) => Interval = interval;

        public void Start()
        {
            if (Started)
            {
                gBS->SetTimer(TimerEvent, EFI_TIMER_DELAY.TimerPeriodic, (ulong)Interval * 10000);
                return;
            }

            Timer timer = this;
            IntPtr context = Unsafe.As<Timer, IntPtr>(ref timer);
            fixed (EFI_EVENT* evt = &TimerEvent)
            {
                EFI_STATUS status = gBS->CreateEvent(
                    (uint)EVT_TIMER | EVT_NOTIFY_SIGNAL,
                    TPL_CALLBACK,
                    &TimerProc,
                    (void*)context,
                    evt
                );
                if ((ulong)status != EFI_SUCCESS)
                    return;
            }

            EFI_STATUS setTimerStatus = gBS->SetTimer(
                TimerEvent,
                EFI_TIMER_DELAY.TimerPeriodic,
                (ulong)Interval * 10000
            );
            if ((ulong)setTimerStatus != EFI_SUCCESS)
            {
                gBS->CloseEvent(TimerEvent);
                TimerEvent = default;
                return;
            }

            Started = true;
        }

        public void Stop()
        {
            if (!Started)
                return;

            gBS->SetTimer(TimerEvent, EFI_TIMER_DELAY.TimerCancel, 0);
            gBS->CloseEvent(TimerEvent);
            TimerEvent = default;
            Started = false;
        }
    }
}
