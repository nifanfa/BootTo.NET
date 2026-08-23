namespace System.Threading.Tasks
{
    public partial class Task
    {
        public static Task Delay(int millisecondsDelay)
        {
            if (millisecondsDelay < Timeout.Infinite)
                throw new Exception("The delay must be -1 or a non-negative value.");
            if (millisecondsDelay == Timeout.Infinite)
                return new Task();
            if (millisecondsDelay == 0)
                return CompletedTask;

            return new DelayOperation(millisecondsDelay).Start();
        }

        private unsafe sealed class DelayOperation : TaskPoller
        {
            private readonly int _millisecondsDelay;
            private EFI_EVENT _event;
            private TaskCompletionSource _completion;

            internal DelayOperation(int millisecondsDelay)
            {
                _millisecondsDelay = millisecondsDelay;
            }

            internal Task Start()
            {
                TaskCompletionSource completion = new TaskCompletionSource();
                _completion = completion;

                EFI_STATUS status;
                fixed (EFI_EVENT* eventPointer = &_event)
                    status = gBS->CreateEvent((uint)EVT_TIMER, TPL_APPLICATION, null, null, eventPointer);
                if ((ulong)status != EFI_SUCCESS)
                {
                    Complete(status);
                    return completion.Task;
                }

                status = gBS->SetTimer(
                    _event,
                    TimerRelative,
                    (ulong)_millisecondsDelay * 10000);
                if ((ulong)status != EFI_SUCCESS)
                {
                    Complete(status);
                    return completion.Task;
                }

                TaskScheduler.Register(this);
                return completion.Task;
            }

            internal override void Poll()
            {
                if ((void*)_event != null && (ulong)gBS->CheckEvent(_event) == EFI_SUCCESS)
                    Complete(EFI_SUCCESS);
            }

            private void Complete(EFI_STATUS status)
            {
                TaskCompletionSource completion = _completion;
                _completion = null;
                TaskScheduler.Unregister(this);

                if ((void*)_event != null)
                {
                    gBS->CloseEvent(_event);
                    _event = default;
                }

                if (completion == null)
                    return;
                if ((ulong)status == EFI_SUCCESS)
                    completion.TrySetResult();
                else
                    completion.TrySetException(new Exception("The delay timer could not be started."));
            }
        }
    }
}
