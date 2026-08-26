using System;
using System.Timers;

internal abstract class TaskPoller
{
    internal TaskPoller Next;
    internal bool IsRegistered;

    internal abstract void Poll();
}

internal static class TaskScheduler
{
    private static TaskPoller s_pollers;
    private static bool s_yielding;
    private static bool s_yieldAgain;
    private static Timer s_schedulerTimer;

    private static EFI_TPL s_previousTpl;
    private static int s_lockDepth;

    internal static unsafe void Enter(object obj, ref bool lockTaken)
    {
        if (obj == null)
            throw new Exception("The lock object cannot be null.");
        if (lockTaken)
            throw new Exception("The lock is already held.");

        if (s_lockDepth == 0)
            s_previousTpl = gBS->RaiseTPL(TPL_NOTIFY);

        s_lockDepth++;
        lockTaken = true;
    }

    internal static unsafe void Exit(object obj)
    {
        if (obj == null || s_lockDepth == 0)
            throw new Exception("The lock is not held.");

        s_lockDepth--;
        if (s_lockDepth == 0)
            gBS->RestoreTPL(s_previousTpl);
    }

    internal static void Register(TaskPoller poller)
    {
        if (poller.IsRegistered)
            return;

        poller.Next = s_pollers;
        poller.IsRegistered = true;
        s_pollers = poller;
        EnsureSchedulerTimer();

        if (s_yielding)
            s_yieldAgain = true;
    }

    private static void EnsureSchedulerTimer()
    {
        if (s_schedulerTimer != null)
            return;

        Timer timer = new Timer(1);
        timer.Elapsed += SchedulerTimerElapsed;
        timer.Start();
        s_schedulerTimer = timer;
    }

    private static void SchedulerTimerElapsed(object sender, EventArgs args) => Yield();

    internal static void Unregister(TaskPoller poller)
    {
        if (!poller.IsRegistered)
            return;

        TaskPoller previous = null;
        TaskPoller current = s_pollers;
        while (current != null)
        {
            if (current == poller)
            {
                if (previous == null)
                    s_pollers = current.Next;
                else
                    previous.Next = current.Next;

                current.Next = null;
                current.IsRegistered = false;
                return;
            }

            previous = current;
            current = current.Next;
        }

        poller.IsRegistered = false;
        poller.Next = null;
    }

    internal static bool Yield()
    {
        // A poller can complete a task and run a continuation synchronously.
        // Do not recursively walk the same list when that continuation yields.
        if (s_yielding)
        {
            s_yieldAgain = true;
            return false;
        }

        s_yielding = true;
        bool hadPollers = false;
        try
        {
            do
            {
                s_yieldAgain = false;
                if (s_pollers != null)
                    hadPollers = true;

                TaskPoller current = s_pollers;
                while (current != null)
                {
                    TaskPoller next = current.Next;
                    current.Poll();
                    current = next;
                }
            }
            while (s_yieldAgain);

            return hadPollers;
        }
        finally
        {
            s_yielding = false;
            s_yieldAgain = false;
        }
    }
}
