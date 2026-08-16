// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime;
using System.Runtime.CompilerServices;

namespace System
{
    public static class GC
    {
        public static void Collect()
        {
            GarbageCollector.Collect();
        }

        public static void Collect(int generation)
        {
            GarbageCollector.Collect();
        }

        public static long GetTotalMemory(bool forceFullCollection)
        {
            if (forceFullCollection)
                GarbageCollector.Collect();

            return (long)GarbageCollector.LiveBytes;
        }

        public static long GetTotalAllocatedBytes()
        {
            return (long)GarbageCollector.TotalAllocatedBytes;
        }

        public static int CollectionCount(int generation)
        {
            return GarbageCollector.CollectionCount;
        }

        public static void SuppressFinalize(object obj)
        {
        }

        public static void WaitForPendingFinalizers()
        {
        }

        [Intrinsic]
        public static void KeepAlive(object obj)
        {
        }
    }
}
