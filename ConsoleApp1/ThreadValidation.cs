using System;
using System.Threading;

internal static class ThreadValidation
{
    private static volatile int s_firstReady;
    private static volatile int s_secondReady;
    private static volatile int s_rootSurvived;
    private static volatile int s_automaticWaiterReady;
    private static volatile int s_automaticRelease;

    internal static void Run()
    {
        ValidateYieldAndPreciseRoots();
        ValidateAutomaticYield();
        ValidateSleep();
        Console.WriteLine("Thread validation passed.");
    }

    private static void ValidateYieldAndPreciseRoots()
    {
        s_firstReady = 0;
        s_secondReady = 0;
        s_rootSurvived = 0;

        Thread collector = new Thread(() =>
        {
            while (s_firstReady == 0)
                Thread.Yield();
            GC.Collect();
            s_secondReady = 1;
        });
        Thread owner = new Thread(() =>
        {
            string stackRoot = new string(new[] { 'r', 'o', 'o', 't' });
            s_firstReady = 1;
            while (s_secondReady == 0)
                Thread.Yield();
            if (stackRoot == "root")
                s_rootSurvived = 1;
        });

        collector.Start();
        owner.Start();
        owner.Join();
        collector.Join();
        if (s_rootSurvived == 0)
            throw new Exception("Thread validation failed: precise suspended root.");
    }

    private static void ValidateAutomaticYield()
    {
        s_automaticWaiterReady = 0;
        s_automaticRelease = 0;
        Thread release = new Thread(() => s_automaticRelease = 1);
        Thread waiter = new Thread(() =>
        {
            s_automaticWaiterReady = 1;
            while (s_automaticRelease == 0)
            {
            }
        });

        release.Start();
        waiter.Start();
        waiter.Join();
        release.Join();
        if (s_automaticWaiterReady == 0 || s_automaticRelease == 0)
            throw new Exception("Thread validation failed: automatic yield.");
    }

    private static void ValidateSleep()
    {
        int completed = 0;
        Thread sleeper = new Thread(() =>
        {
            Thread.Sleep(1);
            completed = 1;
        });
        sleeper.Start();
        sleeper.Join();
        if (completed != 1)
            throw new Exception("Thread validation failed: sleep.");
    }
}
