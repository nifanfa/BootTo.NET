using System.Runtime.InteropServices;

namespace System.Timers
{
    public unsafe class Timer
    {
        public event EventHandler Elapsed;
        bool Started;

        [UnmanagedCallersOnly]
        static void TimerProc(EFI_EVENT Event, void* Context)
        {
            Timer timer = RuntimeObjectHandle.Get<Timer>((IntPtr)Context);
            if (timer == null)
                return;
            if (timer.Elapsed == null)
                return;
            timer.Elapsed.Invoke(timer, EventArgs.Empty);
        }

        public double Interval;

        public Timer(double interval) => Interval = interval;

        EFI_EVENT TimerEvent;
        IntPtr ContextHandle;

        public void Start()
        {
            if (Started)
            {
                gBS->SetTimer(TimerEvent, TimerPeriodic, (ulong)Interval * 10000);
                return;
            }

            ContextHandle = RuntimeObjectHandle.Allocate(this);
            fixed (EFI_EVENT* evt = &TimerEvent)
            {
                EFI_STATUS status = gBS->CreateEvent(
                    (uint)EVT_TIMER | EVT_NOTIFY_SIGNAL,
                    TPL_CALLBACK,
                    &TimerProc,
                    (void*)ContextHandle,
                    evt
                );
                if ((ulong)status != EFI_SUCCESS)
                {
                    RuntimeObjectHandle.Free(ContextHandle);
                    ContextHandle = IntPtr.Zero;
                    return;
                }
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
                RuntimeObjectHandle.Free(ContextHandle);
                ContextHandle = IntPtr.Zero;
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
            RuntimeObjectHandle.Free(ContextHandle);
            ContextHandle = IntPtr.Zero;
            Started = false;
        }
    }
}
