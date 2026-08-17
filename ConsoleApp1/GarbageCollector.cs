using Internal.Runtime;
using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// BootTo.NET is single-threaded while UEFI boot services are active. This collector
// deliberately uses a non-moving, conservative mark/sweep policy for that environment.
internal static unsafe class GarbageCollector
{
    [StructLayout(LayoutKind.Sequential)]
    private struct AllocationHeader
    {
        public AllocationHeader* Next;
        public ulong Size;
        public ulong Flags;
        public ulong Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RegisterSnapshot
    {
        public ulong Rax;
        public ulong Rbx;
        public ulong Rcx;
        public ulong Rdx;
        public ulong Rsi;
        public ulong Rdi;
        public ulong Rbp;
        public ulong Rsp;
        public ulong R8;
        public ulong R9;
        public ulong R10;
        public ulong R11;
        public ulong R12;
        public ulong R13;
        public ulong R14;
        public ulong R15;
    }

    private const ulong Marked = 1;
    private const ulong Scanned = 2;
    private const ulong InitialCollectionThreshold = 512 * 1024;
    private const ulong MinimumCollectionThreshold = 256 * 1024;
    private const ulong StackBoundaryAllowance = 512;
    private const ulong MaximumUnsignedValue = ~0UL;

    private static AllocationHeader* s_allocations;
    private static IntPtr s_gcStaticsStart;
    private static IntPtr s_gcStaticsEnd;
    private static ulong s_stackUpperBound;
    private static ulong s_lowestObject = MaximumUnsignedValue;
    private static ulong s_highestObjectEnd;
    private static ulong s_liveBytes;
    private static ulong s_allocatedSinceCollection;
    private static ulong s_totalAllocatedBytes;
    private static ulong s_nextCollectionThreshold = InitialCollectionThreshold;
    private static int s_objectCount;
    private static int s_collectionCount;
    private static bool s_collecting;

    [RuntimeImport("*", "RhpCaptureRegisters")]
    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern void RhpCaptureRegisters(RegisterSnapshot* snapshot);

    internal static ulong LiveBytes => s_liveBytes;
    internal static ulong TotalAllocatedBytes => s_totalAllocatedBytes;
    internal static int ObjectCount => s_objectCount;
    internal static int CollectionCount => s_collectionCount;

    internal static void InitializeStack(void* stackMarker)
    {
        ulong marker = (ulong)stackMarker;
        ulong upperBound = marker + StackBoundaryAllowance;
        if (upperBound >= marker && upperBound > s_stackUpperBound)
            s_stackUpperBound = upperBound;
    }

    internal static void RegisterStatics(IntPtr start, IntPtr end)
    {
        s_gcStaticsStart = start;
        s_gcStaticsEnd = end;
    }

    internal static void* Allocate(ulong size)
    {
        if (size > MaximumUnsignedValue - 7)
            return null;

        size = (size + 7) & ~7UL;

        if (!s_collecting && s_stackUpperBound != 0 && s_allocatedSinceCollection >= s_nextCollectionThreshold)
            Collect();

        void* allocation = null;
        ulong allocationSize = size + (ulong)sizeof(AllocationHeader);
        if (allocationSize < size)
            return null;

        EFI_STATUS status = efi.gBS->AllocatePool(EFI_MEMORY_TYPE.EfiLoaderData, allocationSize, &allocation);
        if ((ulong)status != efi.EFI_SUCCESS)
        {
            Collect();
            status = efi.gBS->AllocatePool(EFI_MEMORY_TYPE.EfiLoaderData, allocationSize, &allocation);
            if ((ulong)status != efi.EFI_SUCCESS)
                return null;
        }

        AllocationHeader* header = (AllocationHeader*)allocation;
        header->Next = s_allocations;
        header->Size = size;
        header->Flags = 0;
        header->Reserved = 0;
        s_allocations = header;

        byte* objectAddress = (byte*)allocation + sizeof(AllocationHeader);
        memset(objectAddress, 0, size);

        ulong objectStart = (ulong)objectAddress;
        ulong objectEnd = objectStart + size;
        if (objectStart < s_lowestObject)
            s_lowestObject = objectStart;
        if (objectEnd > s_highestObjectEnd)
            s_highestObjectEnd = objectEnd;

        s_liveBytes += size;
        s_allocatedSinceCollection += size;
        s_totalAllocatedBytes += size;
        s_objectCount++;
        return objectAddress;
    }

    internal static void Collect()
    {
        if (s_collecting || s_allocations == null || s_stackUpperBound == 0)
            return;

        s_collecting = true;

        RegisterSnapshot registers;
        RhpCaptureRegisters(&registers);
        ScanMemory(&registers, (ulong)sizeof(RegisterSnapshot));

        ulong stackCurrent = (ulong)&registers;
        if (stackCurrent < s_stackUpperBound)
            ScanMemory((void*)stackCurrent, s_stackUpperBound - stackCurrent);
        else
            ScanMemory((void*)s_stackUpperBound, stackCurrent - s_stackUpperBound + (ulong)sizeof(RegisterSnapshot));

        MarkStaticRoots();

        // Finalizer execution is not available without the CoreRT thread/finalizer
        // subsystem. Retain finalizable objects rather than reclaiming them unsafely.
        for (AllocationHeader* header = s_allocations; header != null; header = header->Next)
        {
            EEType* type = *(EEType**)ObjectAddress(header);
            if (type != null && (type->Flags & EETypeFlags.HasFinalizerFlag) != 0)
                header->Flags |= Marked;
        }

        bool foundUnscanned;
        do
        {
            foundUnscanned = false;
            for (AllocationHeader* header = s_allocations; header != null; header = header->Next)
            {
                if ((header->Flags & Marked) == 0 || (header->Flags & Scanned) != 0)
                    continue;

                header->Flags |= Scanned;
                foundUnscanned = true;

                EEType* type = *(EEType**)ObjectAddress(header);
                if (type != null && (type->Flags & EETypeFlags.HasPointersFlag) != 0 && header->Size > (ulong)sizeof(IntPtr))
                {
                    // Native AOT strictly confirms that GC pointers must be aligned.
                    /*
                     * For example 
                     * class 
                     * { 
                     *    byte A; 
                     *    int B;
                     * } 
                     * is laid out as 
                     * class
                     * { 
                     *    byte A; 
                     *    byte[7] padding1; 
                     *    int B; 
                     *    byte[4] padding2;
                     * }
                     */
                    byte* fields = ObjectAddress(header) + sizeof(IntPtr);
                    ScanMemory(fields, header->Size - (ulong)sizeof(IntPtr));
                }
            }
        }
        while (foundUnscanned);

        Sweep();
        s_collectionCount++;
        s_allocatedSinceCollection = 0;

        ulong nextThreshold = s_liveBytes;
        if (nextThreshold <= MaximumUnsignedValue / 2)
            nextThreshold *= 2;
        if (nextThreshold < MinimumCollectionThreshold)
            nextThreshold = MinimumCollectionThreshold;
        s_nextCollectionThreshold = nextThreshold;
        s_collecting = false;
    }

    private static byte* ObjectAddress(AllocationHeader* header)
    {
        return (byte*)header + sizeof(AllocationHeader);
    }

    private static void MarkStaticRoots()
    {
        if (s_gcStaticsStart == IntPtr.Zero || s_gcStaticsEnd == IntPtr.Zero)
            return;

        for (IntPtr* entry = (IntPtr*)s_gcStaticsStart; entry < (IntPtr*)s_gcStaticsEnd; entry++)
        {
            IntPtr* staticBlock = (IntPtr*)*entry;
            if (staticBlock == null)
                continue;

            ulong blockValue = (ulong)*staticBlock;
            if (blockValue == 0 || (blockValue & (ulong)GCStaticRegionConstants.Uninitialized) != 0)
                continue;

            MarkCandidate(*(ulong*)blockValue);
        }
    }

    private static void ScanMemory(void* start, ulong size)
    {
        ulong address = (ulong)start;
        ulong end = address + size;
        if (end < address)
            return;

        address = (address + 7) & ~7UL;
        while (address <= end && end - address >= (ulong)sizeof(ulong))
        {
            MarkCandidate(*(ulong*)address);
            address += (ulong)sizeof(ulong);
        }
    }

    private static void MarkCandidate(ulong candidate)
    {
        if (candidate < s_lowestObject || candidate >= s_highestObjectEnd)
            return;

        for (AllocationHeader* header = s_allocations; header != null; header = header->Next)
        {
            ulong objectStart = (ulong)ObjectAddress(header);
            ulong objectEnd = objectStart + header->Size;
            if (candidate >= objectStart && candidate < objectEnd)
            {
                header->Flags |= Marked;
                return;
            }
        }
    }

    private static void Sweep()
    {
        AllocationHeader* previous = null;
        AllocationHeader* current = s_allocations;
        ulong lowestObject = MaximumUnsignedValue;
        ulong highestObjectEnd = 0;
        ulong liveBytes = 0;
        int objectCount = 0;

        while (current != null)
        {
            AllocationHeader* next = current->Next;
            if ((current->Flags & Marked) != 0)
            {
                current->Flags = 0;
                previous = current;

                ulong objectStart = (ulong)ObjectAddress(current);
                ulong objectEnd = objectStart + current->Size;
                if (objectStart < lowestObject)
                    lowestObject = objectStart;
                if (objectEnd > highestObjectEnd)
                    highestObjectEnd = objectEnd;
                liveBytes += current->Size;
                objectCount++;
            }
            else
            {
                if (previous == null)
                    s_allocations = next;
                else
                    previous->Next = next;

                efi.gBS->FreePool(current);
            }

            current = next;
        }

        s_lowestObject = lowestObject;
        s_highestObjectEnd = highestObjectEnd;
        s_liveBytes = liveBytes;
        s_objectCount = objectCount;
    }
}