// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Internal.Runtime;
using Internal.Runtime.CompilerServices;

namespace System
{
    // CONTRACT with Runtime
    // The Object type is one of the primitives understood by the compilers and runtime
    // Data Contract: Single field of type EEType*
    // VTable Contract: The first vtable slot should be the finalizer for object => The first virtual method in the object class should be the Finalizer

    public unsafe class Object
    {
        // CS0649: Field '{blah}' is never assigned to, and will always have its default value
#pragma warning disable 649
        internal EEType* m_pEEType;
#pragma warning restore

        // Creates a new instance of an Object.
        public Object()
        {
        }

        // Allow an object to free resources before the object is reclaimed by the GC.
        // CONTRACT with runtime: This method's virtual slot number is hardcoded in the binder. It is an
        // implementation detail where it winds up at runtime.
        // **** Do not add any virtual methods in this class ahead of this ****

        ~Object()
        {
        }

        internal EEType* EEType
        {
            get
            {
                // NOTE:  if managed code can be run when the GC has objects marked, then this method is 
                //        unsafe.  But, generically, we don't expect managed code such as this to be allowed
                //        to run while the GC is running.
                return m_pEEType;
            }
        }

        internal EETypePtr EETypePtr
        {
            get
            {
                return new EETypePtr(new IntPtr(m_pEEType));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private class RawData
        {
            public byte Data;
        }

        /// <summary>
        /// Return beginning of all data (excluding ObjHeader and EEType*) within this object.
        /// Note that for strings/arrays this would include the Length as well. 
        /// </summary>
        internal ref byte GetRawData()
        {
            return ref Unsafe.As<RawData>(this).Data;
        }

        /// <summary>
        /// Return size of all data (excluding ObjHeader and EEType*).
        /// Note that for strings/arrays this would include the Length as well.
        /// </summary>
        internal uint GetRawDataSize()
        {
            return EETypePtr.BaseSize - (uint)sizeof(ObjHeader) - (uint)sizeof(EEType*);
        }

        // Returns a String which represents the object instance.  The default
        // for an object is to return the fully qualified name of the class.
        public virtual string? ToString()
        {
            return "System.Object";
        }

        public virtual bool Equals(object? obj)
        {
            return false;
        }

        // GetHashCode is intended to serve as a hash function for this object.
        // Based on the contents of the object, the hash function will return a suitable
        // value with a relatively random distribution over the various inputs.
        //
        // The default implementation returns the sync block index for this instance.
        // Calling it on the same object multiple times will return the same value, so
        // it will technically meet the needs of a hash function, but it's less than ideal.
        // Objects (& especially value classes) should override this method.
        public virtual int GetHashCode()
        {
            return 0;
        }

        public virtual void Dispose()
        {
            var obj = this;
            gBS->FreePool((void*)Unsafe.As<object, IntPtr>(ref obj));
        }
    }
}
