using Internal.Runtime;
using Internal.Runtime.CompilerHelpers;
using Internal.Runtime.CompilerServices;
using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    public unsafe class Object
    {
        internal EEType* EEType;

        public virtual bool Equals(object b)
        {
            object a = this;
            return Unsafe.As<object, ulong>(ref a) == Unsafe.As<object, ulong>(ref b);
        }

        public virtual int GetHashCode()
            => 0;

        public virtual string ToString() => "System.Object";

        internal ref byte GetRawData()
        {
            return ref Unsafe.As<RawData>(this).Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class RawData
        {
            public byte Data;
        }

        internal uint GetRawDataSize()
        {
            return EEType->BaseSize - (uint)sizeof(ObjHeader) - (uint)sizeof(EEType*);
        }
    }
    public struct Void { }

    public struct Boolean { }
    public struct Char
    {
        public override unsafe string ToString()
        {
            char* ptr = stackalloc char[2];
            ptr[0] = this;
            ptr[1] = '\0';
            return new string(ptr);
        }
    }
    public struct SByte
    {
        public override string ToString() => ((long)this).ToString();
    }
    public struct Byte
    {
        public override string ToString() => ((ulong)this).ToString();
    }
    public struct Int16
    {
        public override string ToString() => ((long)this).ToString();
    }
    public struct UInt16
    {
        public override string ToString() => ((ulong)this).ToString();
    }
    public struct Int32
    {
        public override string ToString() => ((long)this).ToString();
    }
    public struct UInt32
    {
        public override string ToString() => ((ulong)this).ToString();
    }
    public struct Int64
    {
        public override unsafe string ToString()
        {
            byte[] buffer = new byte[32];
            fixed (byte* ptr = buffer)
            {
                snprintf(ptr, buffer.Length, "%lld"u8, this);
                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
    public struct UInt64
    {
        public override unsafe string ToString()
        {
            byte[] buffer = new byte[32];
            fixed (byte* ptr = buffer)
            {
                snprintf(ptr, buffer.Length, "%llu"u8, this);
                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
    public struct IntPtr
    {
        unsafe private void* _value;

        [Intrinsic]
        public static readonly IntPtr Zero;

        [Intrinsic]
        public unsafe IntPtr(void* value)
        {
            _value = value;
        }

        [Intrinsic]
        public unsafe IntPtr(int value)
        {
            _value = (void*)value;
        }

        [Intrinsic]
        public unsafe IntPtr(long value)
        {
            _value = (void*)value;
        }

        [Intrinsic]
        public static unsafe explicit operator IntPtr(int value)
        {
            return new IntPtr(value);
        }

        [Intrinsic]
        public static unsafe explicit operator IntPtr(long value)
        {
            return new IntPtr(value);
        }

        [Intrinsic]
        public static unsafe explicit operator IntPtr(void* value)
        {
            return new IntPtr(value);
        }

        [Intrinsic]
        public static unsafe explicit operator void*(IntPtr value)
        {
            return value._value;
        }

        [Intrinsic]
        public static unsafe explicit operator int(IntPtr value)
        {
            return unchecked((int)value._value);
        }

        [Intrinsic]
        public static unsafe explicit operator long(IntPtr value)
        {
            return unchecked((long)value._value);
        }

        [Intrinsic]
        public static unsafe bool operator ==(IntPtr value1, IntPtr value2)
        {
            return value1._value == value2._value;
        }

        [Intrinsic]
        public static unsafe bool operator !=(IntPtr value1, IntPtr value2)
        {
            return value1._value != value2._value;
        }
    }
    public struct UIntPtr { }
    public struct Single
    {
        public override string ToString() => ((double)this).ToString();
    }
    public struct Double
    {
        public override unsafe string ToString()
        {
            byte[] buffer = new byte[32];
            fixed (byte* ptr = buffer)
            {
                snprintf(ptr, buffer.Length, "%lf"u8, this);
                return Encoding.UTF8.GetString(buffer);
            }
        }
    }
    public class Type { }

    public class Exception
    {
        private string Message;

        public Exception(string message = "Exception of type 'System.Exception' was thrown.") => Message = message;

        [RuntimeExport("RhpThrowEx")]
        internal static void RhpThrowEx(Exception exception)
        {
            Console.WriteLine(exception.Message);
            for (; ; );
        }
    }

    internal sealed class IndexOutOfRangeException : Exception
    {
        public IndexOutOfRangeException() : base("Index was outside the bounds of the array.") { }
    }

    internal sealed class InvalidProgramException : Exception
    {
        public InvalidProgramException() : base("Common Language Runtime detected an invalid program.") { }
    }

    internal sealed class OverflowException : Exception
    {
        public OverflowException() : base("Arithmetic operation resulted in an overflow.") { }
    }

    internal sealed class TypeLoadException : Exception
    {
        public TypeLoadException() : base("Failure has occurred while loading a type.") { }
    }

    internal sealed class NotSupportedException : Exception
    {
        public NotSupportedException() : base("Specified method is not supported.") { }
    }

    internal sealed class ArgumentNullException : Exception
    {
        public ArgumentNullException() : base("Value cannot be null.") { }
    }

    public readonly unsafe ref struct ReadOnlySpan<T>
    {
        private readonly void* _pointer;
        private readonly int _length;

        public ReadOnlySpan(T[] array, int start, int length)
        {
            _pointer = Unsafe.AsPointer(ref array[start]);
            _length = length - start;
        }

        public int Length
        {
            get => _length;
        }

        public bool IsEmpty => _length == 0;

        public ref readonly T this[int index]
        {
            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)_length)
                    ThrowHelpers.ThrowIndexOutOfRangeException();
                return ref Unsafe.Add(ref Unsafe.AsRef<T>(_pointer), index);
            }
        }

        public static implicit operator ReadOnlySpan<T>(T[] array) => new ReadOnlySpan<T>(array, 0, array.Length);

        public static implicit operator void*(ReadOnlySpan<T> readOnlySpan) => readOnlySpan._pointer;
    }

    public unsafe struct EETypePtr
    {
        public EEType* Value;

        [Intrinsic]
        internal extern static EETypePtr EETypePtrOf<T>();
    }

    public abstract class ValueType { }
    public abstract class Enum : ValueType
    {
        [Intrinsic]
        public extern bool HasFlag(Enum flag);
    }

    public struct Nullable<T> where T : struct { }

    public sealed partial class String
    {
        public int Length;
        internal char FirstChar;

        [Intrinsic]
        public static readonly string Empty = "";

        public unsafe char this[int index]
        {
            [Intrinsic]
            get => Unsafe.Add(ref FirstChar, index);

            set
            {
                fixed (char* p = &FirstChar)
                {
                    p[index] = value;
                }
            }
        }

        public static bool IsNullOrEmpty(string value)
        {
            return (value == null || 0u >= (uint)value.Length) ? true : false;
        }

        public unsafe static string Concat(string a, string b)
        {
            int Length = a.Length + b.Length;
            char* ptr = stackalloc char[Length];
            int currentIndex = 0;
            for (int i = 0; i < a.Length; i++)
            {
                ptr[currentIndex] = a[i];
                currentIndex++;
            }
            for (int i = 0; i < b.Length; i++)
            {
                ptr[currentIndex] = b[i];
                currentIndex++;
            }
            return new string(ptr, 0, Length);
        }

        public static string Concat(string a, string b, string c) => Concat(Concat(a, b), c);

        public static string Concat(string a, string b, string c, string d) => Concat(Concat(a, b), Concat(c, d));

        public static string Concat(params string[] vs)
        {
            string s = Empty;
            for (int i = 0; i < vs.Length; i++) s += vs[i];
            return s;
        }

        public extern unsafe String(char* ptr);
        public extern String(IntPtr ptr);
        public extern String(char[] buf);
        public extern unsafe String(char* ptr, int index, int length);

        static unsafe string Ctor(char* ptr)
        {
            int i = 0;

            while (ptr[i++] != '\0')
            { }

            return Ctor(ptr, 0, i - 1);
        }

        static unsafe string Ctor(IntPtr ptr) => Ctor((char*)ptr);

        static unsafe string Ctor(char[] buf)
        {
            fixed (char* _buf = buf)
            {
                return Ctor(_buf, 0, buf.Length);
            }
        }

        static unsafe string Ctor(char* ptr, int index, int length)
        {
            EETypePtr et = EETypePtr.EETypePtrOf<string>();

            char* start = ptr + index;
            object data = StartupCodeHelpers.RhpNewArray(et.EEType, length);
            string s = Unsafe.As<object, string>(ref data);

            fixed (char* c = &s.FirstChar)
            {
                memcpy((byte*)c, (byte*)start, (ulong)length * sizeof(char));
                c[length] = '\0';
            }

            return s;
        }
    }

    public abstract class Array
    {
        public int Length;
    }

    public abstract class Delegate { }

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

        public static void SuppressFinalize(object obj) { }

        public static void WaitForPendingFinalizers() { }

        [Intrinsic]
        public static void KeepAlive(object obj)
        {
        }
    }

    public abstract class MulticastDelegate : Delegate { }

    public struct RuntimeTypeHandle { }
    public struct RuntimeMethodHandle { }
    public struct RuntimeFieldHandle { }

    public class Attribute { }

    public sealed class FlagsAttribute : Attribute { }

    public sealed class ParamArrayAttribute : Attribute
    {
        public ParamArrayAttribute() { }
    }

    public enum AttributeTargets { }

    public sealed class AttributeUsageAttribute : Attribute
    {
        public AttributeUsageAttribute(AttributeTargets validOn) { }
        public bool AllowMultiple { get; set; }
        public bool Inherited { get; set; }
    }

    public class AppContext
    {
        public static void SetData(string s, object o) { }
    }

    namespace Reflection
    {
        public sealed class DefaultMemberAttribute : Attribute
        {
            public DefaultMemberAttribute(string memberName) { }
        }
    }

    namespace Runtime.CompilerServices
    {
        public sealed class ExtensionAttribute : Attribute { }

        public static class IsVolatile
        {
        }

        public class RuntimeHelpers
        {
            public static unsafe int OffsetToStringData => sizeof(IntPtr) + sizeof(int);
        }

        public static class RuntimeFeature
        {
            public const string UnmanagedSignatureCallingConvention = nameof(UnmanagedSignatureCallingConvention);
        }

        internal sealed class IntrinsicAttribute : Attribute { }

        public sealed class RequiredMemberAttribute : Attribute { }

        public sealed class CompilerFeatureRequiredAttribute : Attribute
        {
            public CompilerFeatureRequiredAttribute(string featureName) { }
        }

        public sealed class MethodImplAttribute : Attribute
        {
            public MethodImplAttribute(MethodImplOptions methodImplOptions) { }
        }

        public enum MethodImplOptions
        {
            Unmanaged = 0x0004,
            NoInlining = 0x0008,
            NoOptimization = 0x0040,
            AggressiveInlining = 0x0100,
            AggressiveOptimization = 0x200,
            InternalCall = 0x1000,
        }
    }
}

namespace System.Runtime.InteropServices
{
    public class UnmanagedType { }

    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public UnmanagedCallersOnlyAttribute() { }
    }

    public sealed class InAttribute : Attribute { }

    public class Marshal
    {
        public static unsafe IntPtr AllocCoTaskMem(int cb) => (IntPtr)GarbageCollector.Allocate((ulong)cb);

        public static void FreeCoTaskMem(IntPtr ptr) { }
    }

    sealed class StructLayoutAttribute : Attribute
    {
        public StructLayoutAttribute(LayoutKind layoutKind)
        {
            Value = layoutKind;
        }

        public StructLayoutAttribute(short layoutKind)
        {
            Value = (LayoutKind)layoutKind;
        }

        public LayoutKind Value { get; }

        public int Pack;
        public int Size;
        public CharSet CharSet;
    }

    public sealed class FieldOffsetAttribute : Attribute
    {
        public FieldOffsetAttribute(int offset)
        {
            Value = offset;
        }

        public int Value { get; }
    }

    public sealed class DllImportAttribute : Attribute
    {
        public CallingConvention CallingConvention;

        public string EntryPoint;

        public DllImportAttribute(string dllName)
        {
        }
    }

    internal enum LayoutKind
    {
        Sequential = 0,
        Explicit = 2,
        Auto = 3,
    }

    internal enum CharSet
    {
        None = 1,
        Ansi = 2,
        Unicode = 3,
        Auto = 4,
    }

    public enum CallingConvention
    {
        Winapi = 1,
        Cdecl = 2,
        StdCall = 3,
        ThisCall = 4,
        FastCall = 5,
    }
}

namespace System
{
    internal sealed partial class RuntimeType
    {
    }

    namespace Runtime
    {
        internal sealed class RuntimeExportAttribute : Attribute
        {
            public RuntimeExportAttribute(string entry) { }
        }

        public sealed class RuntimeImportAttribute : Attribute
        {
            public RuntimeImportAttribute(string dllName, string entry) { }
        }

    }

    class Array<T> : Array { }
}

namespace Internal.Runtime
{
    internal struct ReadyToRunHeaderConstants
    {
        public const uint Signature = 0x00525452; // 'RTR'

        public const ushort CurrentMajorVersion = 4;
        public const ushort CurrentMinorVersion = 1;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ModuleInfoRow
    {
        public ReadyToRunSectionType SectionId;
        public int Flags;
        public IntPtr Start;
        public IntPtr End;

        public bool HasEndPointer => !End.Equals(IntPtr.Zero);
        public int Length => (int)((ulong)End - (ulong)Start);
    }

    internal struct ReadyToRunHeader
    {
        public uint Signature;
        private ushort MajorVersion;
        private ushort MinorVersion;

        private uint Flags;

        public ushort NumberOfSections;
        private byte EntrySize;
        private byte EntryType;

        // Array of sections follows.
    };

    public enum ReadyToRunSectionType
    {
        CompilerIdentifier = 100,
        ImportSections = 101,
        RuntimeFunctions = 102,
        MethodDefEntryPoints = 103,
        ExceptionInfo = 104,
        DebugInfo = 105,
        DelayLoadMethodCallThunks = 106,
        AvailableTypes = 108,
        InstanceMethodEntryPoints = 109,
        InliningInfo = 110,
        ProfileDataInfo = 111,
        ManifestMetadata = 112,
        AttributePresence = 113,
        InliningInfo2 = 114,
        ComponentAssemblies = 115,
        OwnerCompositeExecutable = 116,
        StringTable = 200,
        GCStaticRegion = 201,
        ThreadStaticRegion = 202,
        InterfaceDispatchTable = 203,
        TypeManagerIndirection = 204,
        EagerCctor = 205,
        FrozenObjectRegion = 206,
        GCStaticDesc = 207,
        ThreadStaticOffsetRegion = 208,
        ThreadStaticGCDescRegion = 209,
        ThreadStaticIndex = 210,
        LoopHijackFlag = 211,
        ImportAddressTables = 212,
        ReadonlyBlobRegionStart = 300,
        ReadonlyBlobRegionEnd = 399,
    }

    internal static class GCStaticRegionConstants
    {
        public const int Uninitialized = 0x1;
        public const int HasPreInitializedData = 0x2;
        public const int Mask = Uninitialized | HasPreInitializedData;
    }

    namespace CompilerServices
    {
        public static unsafe class Unsafe
        {
            [Intrinsic]
            public static extern ref T Add<T>(ref T source, int elementOffset);

            [Intrinsic]
            public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

            [Intrinsic]
            public static extern T As<T>(object value) where T : class;

            [Intrinsic]
            public static extern void* AsPointer<T>(ref T value);

            [Intrinsic]
            public static extern ref T AsRef<T>(void* pointer);

            public static ref T AsRef<T>(ulong pointer)
                => ref AsRef<T>((void*)pointer);

            [Intrinsic]
            public static extern int SizeOf<T>();

            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ref T AddByteOffset<T>(ref T source, ulong byteOffset)
            {
                for (; ; );
            }

            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ref T AddByteOffset<T>(ref T source, nuint byteOffset)
            {
                return ref AddByteOffset(ref source, (ulong)(void*)byteOffset);
            }
        }
    }

    namespace CompilerHelpers
    {
        using Internal.Runtime.CompilerServices;
        using System.Runtime;

        public static class ThrowHelpers
        {
            public static void ThrowInvalidProgramException(int id) => throw new InvalidProgramException();
            public static void ThrowInvalidProgramExceptionWithArgument(int id, string methodName) => throw new InvalidProgramException();
            public static void ThrowOverflowException() => throw new OverflowException();
            public static void ThrowIndexOutOfRangeException() => throw new IndexOutOfRangeException();
            public static void ThrowTypeLoadException(int id, string className, string typeName) => throw new TypeLoadException();
        }

        class StartupCodeHelpers
        {
            [RuntimeExport("__fail_fast")]
            static void __fail_fast() { while (true) ; }
            [RuntimeExport("RhpReversePInvoke")]
            static void RhpReversePInvoke(IntPtr frame) { }
            [RuntimeExport("RhpReversePInvokeReturn")]
            static void RhpReversePInvokeReturn(IntPtr frame) { }
            [RuntimeExport("RhpReversePInvoke2")]
            static void RhpReversePInvoke2(IntPtr frame) { }
            [RuntimeExport("RhpReversePInvokeReturn2")]
            static void RhpReversePInvokeReturn2(IntPtr frame) { }
            [RuntimeExport("RhpPInvoke")]
            static void RhpPInvoke(IntPtr frame) { }
            [RuntimeExport("RhpPInvokeReturn")]
            static void RhpPInvokeReturn(IntPtr frame) { }

            [RuntimeExport("RhpFallbackFailFast")]
            static void RhpFallbackFailFast() { while (true) ; }

            [RuntimeExport("RhpNewFast")]
            internal static unsafe object RhpNewFast(EEType* pEEType)
            {
                var size = pEEType->BaseSize;

                void* ptr = GarbageCollector.Allocate(size);
                IntPtr data = (IntPtr)ptr;

                var obj = Unsafe.As<IntPtr, object>(ref data);
                if (ptr == null)
                    return null;

                *(IntPtr*)data = (IntPtr)pEEType;

                return obj;
            }

            [RuntimeExport("RhpNewArray")]
            internal static unsafe object RhpNewArray(EEType* pEEType, int length)
            {
                if (length < 0)
                    return null;

                ulong componentSize = pEEType->ComponentSize;
                if (componentSize != 0 && (ulong)length > (~0UL - pEEType->BaseSize) / componentSize)
                    return null;

                var size = pEEType->BaseSize + (ulong)length * componentSize;
                void* ptr = GarbageCollector.Allocate(size);
                IntPtr data = (IntPtr)ptr;

                var obj = Unsafe.As<IntPtr, object>(ref data);
                if (ptr == null)
                    return null;

                *(IntPtr*)data = (IntPtr)pEEType;

                var b = (byte*)data;
                b += sizeof(IntPtr);
                memcpy(b, (byte*)(&length), sizeof(int));

                return obj;
            }

            [RuntimeExport("RhpAssignRef")]
            static unsafe void RhpAssignRef(void** address, void* obj)
            {
                *address = obj;
            }

            [RuntimeExport("RhpByRefAssignRef")]
            static unsafe void RhpByRefAssignRef(void** address, void* obj)
            {
                *address = obj;
            }

            [RuntimeExport("RhpCheckedAssignRef")]
            static unsafe void RhpCheckedAssignRef(void** address, void* obj)
            {
                *address = obj;
            }

            [RuntimeExport("RhpStelemRef")]
            static unsafe void RhpStelemRef(Array array, int index, object obj)
            {
                fixed (int* n = &array.Length)
                {
                    var ptr = (byte*)n;
                    ptr += sizeof(void*);
                    ptr += index * array.EEType->ComponentSize;
                    var pp = (IntPtr*)ptr;
                    *pp = Unsafe.As<object, IntPtr>(ref obj);
                }
            }

            [RuntimeExport("RhTypeCast_IsInstanceOfClass")]
            public static unsafe object RhTypeCast_IsInstanceOfClass(EEType* pTargetType, object obj)
            {
                if (obj == null)
                    return null;

                if (pTargetType == obj.EEType)
                    return obj;

                var bt = obj.EEType->RelatedType.BaseType;

                while (true)
                {
                    if (bt == null)
                        return null;

                    if (pTargetType == bt)
                        return obj;

                    bt = bt->RelatedType.BaseType;
                }
            }

            public static unsafe void InitializeModules(IntPtr Modules)
            {
                var header = (ReadyToRunHeader*)*(IntPtr*)Modules;
                var sections = (ModuleInfoRow*)(header + 1);

                if (header->Signature == ReadyToRunHeaderConstants.Signature)
                {
                    for (int k = 0; k < header->NumberOfSections; k++)
                    {
                        if (sections[k].SectionId == ReadyToRunSectionType.GCStaticRegion)
                        {
                            GarbageCollector.RegisterStatics(sections[k].Start, sections[k].End);
                            InitializeStatics(sections[k].Start, sections[k].End);
                        }

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
                        gBS->AllocatePool(EFI_MEMORY_TYPE.EfiLoaderData, (ulong)sizeof(IntPtr), &ptr);
                        IntPtr data = (IntPtr)ptr;

                        *(IntPtr*)data = Unsafe.As<object, IntPtr>(ref obj);
                        *pBlock = data;
                    }
                }
            }
        }
    }

    internal enum EETypeElementType
    {
        Unknown = 0x00,
        Void = 0x01,
        Boolean = 0x02,
        Char = 0x03,
        SByte = 0x04,
        Byte = 0x05,
        Int16 = 0x06,
        UInt16 = 0x07,
        Int32 = 0x08,
        UInt32 = 0x09,
        Int64 = 0x0A,
        UInt64 = 0x0B,
        IntPtr = 0x0C,
        UIntPtr = 0x0D,
        Single = 0x0E,
        Double = 0x0F,
        ValueType = 0x10,
        Nullable = 0x12,
        Class = 0x14,
        Interface = 0x15,
        SystemArray = 0x16,
        Array = 0x17,
        SzArray = 0x18,
        ByRef = 0x19,
        Pointer = 0x1A,
    }

    [Flags]
    internal enum EETypeFlags : ushort
    {
        EETypeKindMask = 0x0003,
        RelatedTypeViaIATFlag = 0x0004,
        IsDynamicTypeFlag = 0x0008,
        HasFinalizerFlag = 0x0010,
        HasPointersFlag = 0x0020,
        ICastableFlag = 0x0040,
        GenericVarianceFlag = 0x0080,
        OptionalFieldsFlag = 0x0100,
        IsGenericFlag = 0x0400,
        ElementTypeMask = 0xf800,
        ElementTypeShift = 11,
        ComplexCastingMask = EETypeKindMask | RelatedTypeViaIATFlag | GenericVarianceFlag
    };

    internal enum EETypeKind : ushort
    {
        CanonicalEEType = 0x0000,
        ClonedEEType = 0x0001,
        ParameterizedEEType = 0x0002,
        GenericTypeDefEEType = 0x0003,
    }


    [StructLayout(LayoutKind.Sequential)]
    internal struct ObjHeader
    {
        private IntPtr _objHeaderContents;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EEType
    {
        [StructLayout(LayoutKind.Explicit)]
        internal unsafe struct RelatedTypeUnion
        {
            [FieldOffset(0)]
            public EEType* BaseType;
            [FieldOffset(0)]
            public EEType** BaseTypeViaIAT;
            [FieldOffset(0)]
            public EEType* CanonicalType;
            [FieldOffset(0)]
            public EEType** CanonicalTypeViaIAT;
            [FieldOffset(0)]
            public EEType* RelatedParameterType;
            [FieldOffset(0)]
            public EEType** RelatedParameterTypeViaIAT;
        }

        internal ushort ComponentSize;
        internal EETypeFlags Flags;
        internal uint BaseSize;
        internal RelatedTypeUnion RelatedType;
        internal ushort NumVtableSlots;
        internal ushort NumInterfaces;
        internal uint HashCode;

        // vtable follows

        private const uint ValueTypePaddingLowMask = 0x7;
        private const uint ValueTypePaddingHighMask = 0xFFFFFF00;
        private const uint ValueTypePaddingMax = 0x07FFFFFF;
        private const int ValueTypePaddingHighShift = 8;
        private const uint ValueTypePaddingAlignmentMask = 0xF8;
        private const int ValueTypePaddingAlignmentShift = 3;
    }
}