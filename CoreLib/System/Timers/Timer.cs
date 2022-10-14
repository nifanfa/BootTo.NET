namespace System.Timers
{
    public unsafe class Timer
    {
        public bool Enabled = false;
        public ulong Interval;
        public EventHandler<EventArgs> Elapsed;

        Action<EFI_EVENT, IntPtr> _action;
        EFI_EVENT _evt;

        public Timer(ulong interval)
        {
            this.Interval = interval;
        }

        public void Start()
        {
            Enabled = true;

            _action = new Action<EFI_EVENT, IntPtr>((evt, ptr) =>
            {
                Elapsed.Invoke(this, null);
            });

            gBS->CreateEvent(EVT_TIMER | EVT_NOTIFY_SIGNAL, TPL_CALLBACK, (delegate* unmanaged<EFI_EVENT,void*,void>)_action.m_functionPointer, null, (EFI_EVENT*)_evt);
            gBS->SetTimer(_evt, EFI_TIMER_DELAY.TimerPeriodic, this.Interval * 10000);
        }

        public void Stop()
        {
            gBS->SetTimer(_evt, EFI_TIMER_DELAY.TimerCancel, 0);
            gBS->CloseEvent(_evt);
        }
    }
}
