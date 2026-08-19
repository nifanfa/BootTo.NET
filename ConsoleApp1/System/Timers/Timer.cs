using Internal.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Timers
{
    internal unsafe class Timer
    {
        public event EventHandler Elapsed;
        bool Started;

        [UnmanagedCallersOnly]
        static void TimerProc(EFI_EVENT Event, void* Context)
        {
            IntPtr context = (IntPtr)Context;
            Timer timer = Unsafe.As<IntPtr, Timer>(ref context);
            timer.Elapsed?.Invoke(timer, EventArgs.Empty);
        }

        public double Interval;

        public Timer(double interval) => Interval = interval;

        EFI_EVENT TimerEvent;

        public void Start()
        {
            if (Started)
            {
                gBS->SetTimer(TimerEvent, TimerPeriodic, (ulong)Interval * 10000);
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
                TimerPeriodic,
                (ulong)(Interval * 10000)
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

            gBS->SetTimer(TimerEvent, TimerCancel, 0);
            gBS->CloseEvent(TimerEvent);
            TimerEvent = default;
            Started = false;
        }
    }
}
