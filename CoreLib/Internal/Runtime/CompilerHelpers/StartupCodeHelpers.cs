using Internal.Runtime.CompilerServices;
using System;
using System.Runtime;

namespace Internal.Runtime.CompilerHelpers
{
    // A class that the compiler looks for that has helpers to initialize the
    // process. The compiler can gracefully handle the helpers not being present,
    // but the class itself being absent is unhandled. Let's add an empty class.
    unsafe class StartupCodeHelpers
    {
        // A couple symbols the generated code will need we park them in this class
        // for no particular reason. These aid in transitioning to/from managed code.
        // Since we don't have a GC, the transition is a no-op.
        [RuntimeExport("RhpReversePInvoke")]
        static void RhpReversePInvoke(IntPtr frame) { }
        [RuntimeExport("RhpReversePInvokeReturn")]
        static void RhpReversePInvokeReturn(IntPtr frame) { }
        [RuntimeExport("RhpPInvoke")]
        static void RhpPInvoke(IntPtr frame) { }
        [RuntimeExport("RhpPInvokeReturn")]
        static void RhpPInvokeReturn(IntPtr frame) { }

        [RuntimeExport("RhpFallbackFailFast")]
        static void RhpFallbackFailFast() { while (true) ; }

        [RuntimeExport("RhpAssignRef")]
        static unsafe void RhpAssignRef(void** address, void* obj) => *address = obj;

        [RuntimeExport("RhpByRefAssignRef")]
        static unsafe void RhpByRefAssignRef(void** address, void* obj) => *address = obj;

        [RuntimeExport("RhpCheckedAssignRef")]
        static unsafe void RhpCheckedAssignRef(void** address, void* obj) => *address = obj;

        [RuntimeExport("RhpStelemRef")]
        static unsafe void RhpStelemRef(Array array, int index, object obj)
        {
            fixed (int* n = &array._numComponents)
            {
                var ptr = (byte*)n;
                ptr += sizeof(void*);
                ptr += index * array.m_pEEType->ComponentSize;
                var pp = (IntPtr*)ptr;
                *pp = Unsafe.As<object, IntPtr>(ref obj);
            }
        }

        [RuntimeExport("RhTypeCast_IsInstanceOfClass")]
        public static unsafe object RhTypeCast_IsInstanceOfClass(EEType* pTargetType, object obj)
        {
            if (obj == null)
                return null;

            if (pTargetType == obj.m_pEEType)
                return obj;

            var bt = obj.m_pEEType->RawBaseType;

            while (true)
            {
                if (bt == null)
                    return null;

                if (pTargetType == bt)
                    return obj;

                bt = bt->RawBaseType;
            }
        }

        [RuntimeExport("RhpNewFast")]
        static unsafe object RhpNewFast(EEType* pEEType)
        {
            var size = pEEType->BaseSize;

            if (size % 8 > 0)
                size = ((size / 8) + 1) * 8;

            void* ptr = null;
            gBS->AllocatePool(EFI_MEMORY_TYPE.EfiLoaderCode, size, &ptr);
            IntPtr data = (IntPtr)ptr;

            var obj = Unsafe.As<IntPtr, object>(ref data);
            memset((byte*)data, 0, size);
            *(IntPtr*)data = (IntPtr)pEEType;

            return obj;
        }

        [RuntimeExport("RhpNewArray")]
        internal static unsafe object RhpNewArray(EEType* pEEType, int length)
        {
            var size = pEEType->BaseSize + (ulong)length * pEEType->ComponentSize;

            if (size % 8 > 0)
                size = ((size / 8) + 1) * 8;

            void* ptr = null;
            gBS->AllocatePool(EFI_MEMORY_TYPE.EfiLoaderCode, size, &ptr);
            IntPtr data = (IntPtr)ptr;

            var obj = Unsafe.As<IntPtr, object>(ref data);
            memset((byte*)data, 0, size);
            *(IntPtr*)data = (IntPtr)pEEType;

            var b = (byte*)data;
            b += sizeof(IntPtr);
            memcpy(b, (byte*)(&length), sizeof(int));

            return obj;
        }

        public static void InitializeModules(IntPtr Modules)
        {
            var header = (ReadyToRunHeader*)*(IntPtr*)Modules;
            var sections = (ModuleInfoRow*)(header + 1);

            if (header->Signature == ReadyToRunHeaderConstants.Signature)
            {
                for (int k = 0; k < header->NumberOfSections; k++)
                {
                    if (sections[k].SectionId == ReadyToRunSectionType.GCStaticRegion)
                        InitializeStatics(sections[k].Start, sections[k].End);

                    if (sections[k].SectionId == ReadyToRunSectionType.EagerCctor)
                        RunEagerClassConstructors(sections[k].Start, sections[k].End);
                }
            }
        }

        private static unsafe void RunEagerClassConstructors(IntPtr cctorTableStart, IntPtr cctorTableEnd)
        {
            for (IntPtr* tab = (IntPtr*)cctorTableStart; tab < (IntPtr*)cctorTableEnd; tab++)
            {
                ((delegate*<void>)(*tab))();
            }
        }

        static unsafe void InitializeStatics(IntPtr rgnStart, IntPtr rgnEnd)
        {
            for (IntPtr* block = (IntPtr*)rgnStart; block < (IntPtr*)rgnEnd; block++)
            {
                var pBlock = (IntPtr*)*block;
                var blockAddr = (long)(*pBlock);

                if ((blockAddr & GCStaticRegionConstants.Uninitialized) == GCStaticRegionConstants.Uninitialized)
                {
                    var obj = RhpNewFast((EEType*)(blockAddr & ~GCStaticRegionConstants.Mask));

                    if ((blockAddr & GCStaticRegionConstants.HasPreInitializedData) == GCStaticRegionConstants.HasPreInitializedData)
                    {
                        IntPtr pPreInitDataAddr = *(pBlock + 1);
                        fixed (byte* p = &obj.GetRawData())
                        {
                            memcpy(p, (byte*)pPreInitDataAddr, obj.GetRawDataSize());
                        }
                    }

                    void* ptr = null;
                    gBS->AllocatePool(EFI_MEMORY_TYPE.EfiLoaderCode, (ulong)sizeof(IntPtr), &ptr);
                    IntPtr data = (IntPtr)ptr;

                    *(IntPtr*)data = Unsafe.As<object, IntPtr>(ref obj);
                    *pBlock = data;
                }
            }
        }
    }
}
