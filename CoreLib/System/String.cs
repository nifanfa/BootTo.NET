using Internal.Runtime.CompilerHelpers;
using Internal.Runtime.CompilerServices;
using System.Runtime;
using System.Runtime.CompilerServices;

namespace System
{
    public sealed class String
    {
        //
        // These fields map directly onto the fields in an EE StringObject.  See object.h for the layout.
        //
        //[NonSerialized]
        private int _stringLength;

        // For empty strings, _firstChar will be '\0', since strings are both null-terminated and length-prefixed.
        // The field is also read-only, however String uses .ctors that C# doesn't recognise as .ctors,
        // so trying to mark the field as 'readonly' causes the compiler to complain.
        //[NonSerialized]
        private char _firstChar;

        public int Length
        {
            [Intrinsic]
            get => _stringLength;
            set => _stringLength = value;
        }

        public unsafe char this[int index]
        {
            [Intrinsic]
            get => Unsafe.Add(ref _firstChar, index);

            set
            {
                fixed (char* p = &_firstChar)
                {
                    p[index] = value;
                }
            }
        }

        /*
         * CONSTRUCTORS
         *
         * Defining a new constructor for string-like types (like String) requires changes both
         * to the managed code below and to the native VM code. See the comment at the top of
         * src/vm/ecall.cpp for instructions on how to add new overloads.
         */

        public static unsafe string Ctor(char* ptr)
        {
            int i = 0;

            while (ptr[i++] != '\0')
            { }

            return Ctor(ptr, 0, i - 1);
        }

        public static unsafe string Ctor(IntPtr ptr)
        {
            return Ctor((char*)ptr);
        }

        public static unsafe string Ctor(char[] buf)
        {
            fixed (char* _buf = buf)
            {
                return Ctor(_buf, 0, buf.Length);
            }
        }

        public static unsafe string Ctor(char* ptr, int index, int length)
        {
            EETypePtr et = EETypePtr.EETypePtrOf<string>();

            char* start = ptr + index;
            object data = InternalCalls.RhpNewArray(et._value, length);
            string s = Unsafe.As<object, string>(ref data);

            fixed (char* c = &s._firstChar)
            {
                InternalCalls.memcpy((byte*)c, (byte*)start, (ulong)length * sizeof(char));
                c[length] = '\0';
            }

            return s;
        }
    }
}
