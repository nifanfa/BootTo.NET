using System;
using System.Runtime;

public static unsafe class Lock
{
    private static EFI_TPL s_previousTpl;
    private static int s_lockDepth;

    internal static void Enter(object obj, ref bool lockTaken)
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

    internal static void Exit(object obj)
    {
        if (obj == null || s_lockDepth == 0)
            throw new Exception("The lock is not held.");

        s_lockDepth--;
        if (s_lockDepth == 0)
            gBS->RestoreTPL(s_previousTpl);
    }
}

