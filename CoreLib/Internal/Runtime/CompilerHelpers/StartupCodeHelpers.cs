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
                    var obj = InternalCalls.RhpNewFast((EEType*)(blockAddr & ~GCStaticRegionConstants.Mask));

                    if ((blockAddr & GCStaticRegionConstants.HasPreInitializedData) == GCStaticRegionConstants.HasPreInitializedData)
                    {
                        IntPtr pPreInitDataAddr = *(pBlock + 1);
                        fixed (byte* p = &obj.GetRawData())
                        {
                            InternalCalls.memcpy(p, (byte*)pPreInitDataAddr, obj.GetRawDataSize());
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
