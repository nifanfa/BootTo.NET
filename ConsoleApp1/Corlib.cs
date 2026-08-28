using Internal.Runtime;
using Internal.Runtime.CompilerHelpers;
using Internal.Runtime.CompilerServices;
using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System
{
    public interface IDisposable
    {
        void Dispose();
    }


    public unsafe class Object
    {
        private EEType* m_pEEType;

        internal EEType* EEType => m_pEEType;

        [Intrinsic]
        public Type GetType()
        {
            return Type.GetTypeFromEETypePtr(new EETypePtr((IntPtr)m_pEEType));
        }

        public virtual bool Equals(object b)
        {
            object a = this;
            return Unsafe.As<object, ulong>(ref a) == Unsafe.As<object, ulong>(ref b);
        }

        public static bool Equals(object a, object b)
        {
            if (ReferenceEquals(a, b))
                return true;

            return a != null && a.Equals(b);
        }

        public static bool ReferenceEquals(object a, object b)
            => Unsafe.As<object, ulong>(ref a) == Unsafe.As<object, ulong>(ref b);

        public virtual int GetHashCode()
        {
            // The collector contract for this runtime is non-moving, so the
            // object reference remains stable for the lifetime of this object.
            // Mix both halves of the native pointer into the 32-bit hash value.
            object value = this;
            ulong address = Unsafe.As<object, ulong>(ref value);
            int hash = unchecked((int)(address ^ (address >> 32)));
            return hash == 0 ? 1 : hash;
        }

        public virtual string ToString() => GetType().FullName;

        internal ref byte GetRawData() => ref Unsafe.As<RawData>(this).Data;

        internal uint GetRawDataSize() => EEType->BaseSize - (uint)sizeof(ObjHeader) - (uint)sizeof(EEType*);
    }

    public struct Void { }

    public struct Boolean
    {
        public override string ToString() => this ? "True" : "False";
    }
    public partial struct Char
    {
        public const char MinValue = (char)0;
        public const char MaxValue = (char)0xFFFF;

        public override unsafe string ToString()
        {
            char* ptr = stackalloc char[2];
            ptr[0] = this;
            ptr[1] = '\0';
            return new string(ptr);
        }
    }
    public partial struct SByte
    {
        public const sbyte MinValue = -128;
        public const sbyte MaxValue = 127;

        public override string ToString() => ((long)this).ToString();
    }
    public partial struct Byte
    {
        public const byte MinValue = 0;
        public const byte MaxValue = 0xFF;

        public override string ToString() => ((ulong)this).ToString();
    }
    public partial struct Int16
    {
        public const short MinValue = -32768;
        public const short MaxValue = 32767;

        public override string ToString() => ((long)this).ToString();
    }
    public partial struct UInt16
    {
        public const ushort MinValue = 0;
        public const ushort MaxValue = 0xFFFF;

        public override string ToString() => ((ulong)this).ToString();
    }
    public partial struct Int32
    {
        public const int MinValue = -2147483648;
        public const int MaxValue = 2147483647;

        public override string ToString() => ((long)this).ToString();
    }
    public partial struct UInt32
    {
        public const uint MinValue = 0;
        public const uint MaxValue = 0xFFFFFFFF;

        public override string ToString() => ((ulong)this).ToString();
    }
    public partial struct Int64
    {
        public const long MinValue = -9223372036854775808;
        public const long MaxValue = 9223372036854775807;
    }
    public partial struct UInt64
    {
        public const ulong MinValue = 0;
        public const ulong MaxValue = 0xFFFFFFFFFFFFFFFF;
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
        public static explicit operator IntPtr(int value)
        {
            return new IntPtr(value);
        }

        [Intrinsic]
        public static explicit operator IntPtr(long value)
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
    public partial struct Single
    {
        public const float MinValue = -3.4028234663852886E+38F;
        public const float MaxValue = 3.4028234663852886E+38F;

        public override string ToString() => ((double)this).ToString();
    }
    public partial struct Double
    {
        public const double MinValue = -1.7976931348623157E+308;
        public const double MaxValue = 1.7976931348623157E+308;
    }
    public unsafe class Type
    {
        private readonly EEType* _eeType;

        private Type(EEType* eeType)
        {
            _eeType = eeType;
        }

        [Intrinsic]
        public static Type GetTypeFromHandle(RuntimeTypeHandle handle)
        {
            return handle.IsNull ? null : GetTypeFromEETypePtr(new EETypePtr((IntPtr)handle.EEType));
        }

        internal static Type GetTypeFromEETypePtr(EETypePtr eeType)
        {
            return eeType.Value == null ? null : new Type(eeType.Value);
        }

        public string FullName => GetFullName();

        public string Name
        {
            get
            {
                return TryGetName(out string name) ? name : "EEType";
            }
        }

        public string Namespace
        {
            get
            {
                if (!TryGetNamespace(out string namespaceName) || namespaceName.Length == 0)
                    return null;

                return namespaceName;
            }
        }

        public override string ToString() => GetFullName();

        private string GetFullName()
        {
            return TryGetFullName(out string name) ? name : "EEType";
        }

        private bool TryGetFullName(out string name)
        {
            name = null;
            return _eeType != null && Internal.Runtime.TypeLoader.TypeLoaderEnvironment.Instance.TryGetTypeFullName(_eeType, out name);
        }

        private bool TryGetName(out string name)
        {
            name = null;
            return _eeType != null && Internal.Runtime.TypeLoader.TypeLoaderEnvironment.Instance.TryGetTypeName(_eeType, out name);
        }

        private bool TryGetNamespace(out string namespaceName)
        {
            namespaceName = null;
            return _eeType != null && Internal.Runtime.TypeLoader.TypeLoaderEnvironment.Instance.TryGetTypeNamespace(_eeType, out namespaceName);
        }
    }
    public class Exception
    {
        private readonly string _message;
        private readonly Exception _innerException;

        public Exception()
            : this("Exception of type 'System.Exception' was thrown.", null)
        {
        }

        public Exception(string message)
            : this(message, null)
        {
        }

        public Exception(string message, Exception innerException)
        {
            _message = message ?? "Exception of type 'System.Exception' was thrown.";
            _innerException = innerException;
        }

        public virtual string Message => _message;
        public Exception InnerException => _innerException;
    }

    public class NotSupportedException : Exception
    {
        public NotSupportedException() : base("Specified method is not supported.") { }
        public NotSupportedException(string message) : base(message) { }
    }

    public class ArgumentException : Exception
    {
        public ArgumentException() : base("Value does not fall within the expected range.") { }
        public ArgumentException(string message) : base(message) { }
    }

    public class ArgumentNullException : ArgumentException
    {
        public ArgumentNullException() : base("Value cannot be null.") { }
        public ArgumentNullException(string message) : base(message) { }
    }

    public class InvalidOperationException : Exception
    {
        public InvalidOperationException() : base("The operation is not valid due to the current state of the object.") { }
        public InvalidOperationException(string message) : base(message) { }
    }

    public class OperationCanceledException : Exception
    {
        public OperationCanceledException() : base("The operation was canceled.") { }
        public OperationCanceledException(string message) : base(message) { }
    }

    public sealed class IndexOutOfRangeException : Exception
    {
        public IndexOutOfRangeException() : base("Index was outside the bounds of the array.") { }
        public IndexOutOfRangeException(string message) : base(message) { }
    }

    public sealed class InvalidProgramException : Exception
    {
        public InvalidProgramException() : base("Common Language Runtime detected an invalid program.") { }
        public InvalidProgramException(string message) : base(message) { }
    }

    public sealed class OverflowException : Exception
    {
        public OverflowException() : base("Arithmetic operation resulted in an overflow.") { }
        public OverflowException(string message) : base(message) { }
    }

    public sealed class TypeLoadException : Exception
    {
        public TypeLoadException() : base("Failure has occurred while loading a type.") { }
        public TypeLoadException(string message) : base(message) { }
    }

    public sealed class InvalidCastException : Exception
    {
        public InvalidCastException() : base("Specified cast is not valid.") { }
        public InvalidCastException(string message) : base(message) { }
    }

    public sealed class NullReferenceException : Exception
    {
        public NullReferenceException() : base("Object reference not set to an instance of an object.") { }
        public NullReferenceException(string message) : base(message) { }
    }

    public ref struct ByReference<T>
    {
        private readonly IntPtr _value;

        [Intrinsic]
        public extern ByReference(ref T value);

        public extern ref T Value
        {
            [Intrinsic]
            get;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public ref struct TypedReference
    {
        // ILC and the JIT depend on this field order for typed-reference IL.
        private readonly ByReference<byte> _value;
        private readonly RuntimeTypeHandle _typeHandle;

        private TypedReference(ref byte value, RuntimeTypeHandle typeHandle)
        {
            _value = new ByReference<byte>(ref value);
            _typeHandle = typeHandle;
        }

        public static Type GetTargetType(TypedReference value)
            => Type.GetTypeFromHandle(value._typeHandle);

        public static RuntimeTypeHandle TargetTypeToken(TypedReference value)
        {
            if (value._typeHandle.IsNull)
                throw new NullReferenceException();

            return value._typeHandle;
        }

        internal static RuntimeTypeHandle RawTargetTypeToken(TypedReference value)
            => value._typeHandle;

        internal ref byte Value
        {
            [Intrinsic]
            get => ref _value.Value;
        }
    }

    public readonly unsafe ref struct ReadOnlySpan<T>(T[] array, int start, int length)
    {
        // An empty span has no element at index zero, but its reference still
        // needs a valid representation because Encoding and parsers routinely
        // construct spans over empty arrays.
        internal readonly ByReference<T> _pointer = new ByReference<T>(ref (array.Length == 0
            ? ref Unsafe.AsRef<T>((void*)0)
            : ref Unsafe.Add(ref array[0], start)));
        private readonly int _length = length;

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
                return ref Unsafe.Add(ref _pointer.Value, index);
            }
        }

        public static implicit operator ReadOnlySpan<T>(T[] array) => new ReadOnlySpan<T>(array, 0, array.Length);

        public static implicit operator T[](ReadOnlySpan<T> readOnlySpan)
        {
            var array = new T[readOnlySpan.Length];
            for (int i = 0; i < readOnlySpan.Length; i++)
            {
                array[i] = readOnlySpan[i];
            }
            return array;
        }

        public static implicit operator void*(ReadOnlySpan<T> readOnlySpan) => Unsafe.AsPointer(ref readOnlySpan._pointer.Value);
    }

    public unsafe struct EETypePtr
    {
        public EEType* Value;

        // EETypePtrOf<T> is replaced by the CoreRT intrinsic with an EEType
        // pointer passed through this constructor.
        internal EETypePtr(IntPtr value)
        {
            Value = (EEType*)(void*)value;
        }

        [Intrinsic]
        internal extern static EETypePtr EETypePtrOf<T>();
    }

    public abstract class ValueType { }
    public abstract class Enum : ValueType
    {
        public override unsafe string ToString()
        {
            ref byte data = ref GetRawData();
            EETypeElementType elementType = (EETypeElementType)(((ushort)EEType->Flags & (ushort)EETypeFlags.ElementTypeMask) >> (int)EETypeFlags.ElementTypeShift);

            return elementType switch
            {
                EETypeElementType.SByte => Unsafe.As<byte, sbyte>(ref data).ToString(),
                EETypeElementType.Byte => data.ToString(),
                EETypeElementType.Int16 => Unsafe.As<byte, short>(ref data).ToString(),
                EETypeElementType.UInt16 => Unsafe.As<byte, ushort>(ref data).ToString(),
                EETypeElementType.Int32 => Unsafe.As<byte, int>(ref data).ToString(),
                EETypeElementType.UInt32 => Unsafe.As<byte, uint>(ref data).ToString(),
                EETypeElementType.Int64 => Unsafe.As<byte, long>(ref data).ToString(),
                EETypeElementType.UInt64 => Unsafe.As<byte, ulong>(ref data).ToString(),
                _ => throw new InvalidOperationException("The enum has an unsupported underlying type."),
            };
        }

        [Intrinsic]
        public extern bool HasFlag(Enum flag);
    }

    // Nullable<T> has special boxing support in the runtime, so it must not
    // implement interfaces: T itself is not required to implement them.
    public struct Nullable<T> where T : struct
    {
        private readonly bool hasValue;
        internal T value;

        public Nullable(T value)
        {
            this.value = value;
            hasValue = true;
        }

        public bool HasValue => hasValue;

        public T Value
        {
            get
            {
                if (!hasValue)
                    throw new InvalidOperationException("Nullable object must have a value.");

                return value;
            }
        }

        public T GetValueOrDefault() => value;

        public T GetValueOrDefault(T defaultValue) => hasValue ? value : defaultValue;

        public override bool Equals(object other)
        {
            if (!hasValue)
                return other == null;

            return other != null && value.Equals(other);
        }

        public override int GetHashCode() => hasValue ? value.GetHashCode() : 0;

        public override string ToString() => hasValue ? value.ToString() : string.Empty;

        public static implicit operator T?(T value) => new T?(value);

        public static explicit operator T(T? value) => value.Value;
    }


    public sealed partial class String
    {
        public int Length;
        internal char FirstChar;

        public override string ToString() => this;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return obj is string && Equals(this, (string)obj);
        }

        public bool Equals(string value) => Equals(this, value);

        public static bool Equals(string a, string b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null || a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }

        public static bool operator ==(string a, string b) => Equals(a, b);

        public static bool operator !=(string a, string b) => !Equals(a, b);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 5381;
                for (int i = 0; i < Length; i++)
                    hash = ((hash << 5) + hash) ^ this[i];
                return hash;
            }
        }

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

        public static bool IsNullOrEmpty(string value) => (value == null || 0u >= (uint)value.Length) ? true : false;

        public static string Concat(string a, string b)
        {
            return ConcatStrings(a, b);
        }

        public static string Concat(string a, string b, string c) => Concat(Concat(a, b), c);

        public static string Concat(string a, string b, string c, string d) => Concat(Concat(a, b), Concat(c, d));

        public static string Concat(object value)
            => Convert.ToString(value);

        public static string Concat(object value0, object value1)
            => Concat(Convert.ToString(value0), Convert.ToString(value1));

        public static string Concat(object value0, object value1, object value2)
            => Concat(Convert.ToString(value0), Convert.ToString(value1), Convert.ToString(value2));

        public static string Concat(object[] values)
        {
            if (values == null)
                throw new ArgumentNullException("The object array passed to Concat cannot be null.");

            if (values.Length == 0)
                return Empty;

            string[] strings = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
                strings[i] = Convert.ToString(values[i]);

            return ConcatStrings(strings);
        }

        public static string Concat(params string[] values) => ConcatStrings(values);

        private static unsafe string ConcatStrings(string[] values)
        {
            if (values == null)
                throw new ArgumentNullException("The string array passed to Concat cannot be null.");

            int length = 0;
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (value != null)
                    length += value.Length;
            }

            if (length == 0)
                return Empty;

            char* buffer = stackalloc char[length];
            int offset = 0;
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i];
                if (value == null)
                    continue;

                for (int j = 0; j < value.Length; j++)
                    buffer[offset++] = value[j];
            }

            return new string(buffer, 0, length);
        }

        private static unsafe string ConcatStrings(string a, string b)
        {
            int aLength = a == null ? 0 : a.Length;
            int bLength = b == null ? 0 : b.Length;
            int length = aLength + bLength;
            if (length == 0)
                return Empty;

            char* buffer = stackalloc char[length];
            int offset = 0;
            for (int i = 0; i < aLength; i++)
                buffer[offset++] = a[i];
            for (int i = 0; i < bLength; i++)
                buffer[offset++] = b[i];

            return new string(buffer, 0, length);
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
            object data = InternalCalls.RhNewString(et.Value, length);
            string s = Unsafe.As<object, string>(ref data);

            fixed (char* c = &s.FirstChar)
            {
                Unsafe.CopyBlock((byte*)c, (byte*)start, (ulong)length * sizeof(char));
                c[length] = '\0';
            }

            return s;
        }
    }

    public abstract unsafe partial class Array
    {
        public int Length;

        public int Rank
        {
            get
            {
                EEType* type = EEType;
                uint boundsSize = type->BaseSize - (uint)(sizeof(IntPtr) * 3);
                int rank = (int)(boundsSize / (uint)(sizeof(int) * 2));
                return rank > 0 ? rank : 1;
            }
        }

        public int GetLength(int dimension)
        {
            int rank = Rank;
            if ((uint)dimension >= (uint)rank)
                throw new IndexOutOfRangeException("The array dimension is outside the array rank.");

            if (rank == 1)
                return Length;

            fixed (int* bounds = &Length)
            {
                return bounds[2 + dimension];
            }
        }

        public int GetLowerBound(int dimension)
        {
            if ((uint)dimension >= (uint)Rank)
                throw new IndexOutOfRangeException("The array dimension is outside the array rank.");
            return 0;
        }

        public int GetUpperBound(int dimension) => GetLength(dimension) - 1;
    }

    [StructLayout(LayoutKind.Sequential)]
    public abstract class Delegate
    {
        // These fields and initialization helpers are part of the CoreRT delegate
        // contract used by ILCompiler when it lowers delegate construction.
        protected internal object m_firstParameter;
        protected internal object m_helperObject;
        protected internal IntPtr m_extraFunctionPointerOrData;
        protected internal IntPtr m_functionPointer;

        protected const int MulticastThunk = 0;
        protected const int ClosedStaticThunk = 1;
        protected const int OpenStaticThunk = 2;
        protected const int ClosedInstanceThunkOverGenericMethod = 3;
        protected const int DelegateInvokeThunk = 4;
        protected const int OpenInstanceThunk = 5;

        protected virtual IntPtr GetThunk(int whichThunk) => IntPtr.Zero;

        public static Delegate Combine(Delegate a, Delegate b)
        {
            if (a == null)
                return b;
            if (b == null)
                return a;

            return a.CombineImpl(b);
        }

        public static Delegate Remove(Delegate source, Delegate value)
        {
            if (source == null)
                return null;
            if (value == null)
                return source;
            if (!InternalEqualTypes(source, value))
                throw new ArgumentException("The delegates must have compatible types.");

            return source.RemoveImpl(value);
        }

        protected virtual Delegate CombineImpl(Delegate follow)
        {
            if (!InternalEqualTypes(this, follow))
                throw new ArgumentException("The delegates must have compatible types.");

            int currentCount = GetInvocationCount(this);
            int followCount = GetInvocationCount(follow);
            Delegate[] invocationList = new Delegate[currentCount + followCount];

            CopyInvocationList(this, invocationList, 0);
            CopyInvocationList(follow, invocationList, currentCount);
            return NewMulticastDelegate(invocationList, invocationList.Length);
        }

        protected virtual Delegate RemoveImpl(Delegate value)
        {
            int sourceCount = GetInvocationCount(this);
            int valueCount = GetInvocationCount(value);
            if (valueCount > sourceCount)
                return this;

            for (int start = sourceCount - valueCount; start >= 0; start--)
            {
                bool equal = true;
                for (int i = 0; i < valueCount; i++)
                {
                    if (!EqualsImpl(GetInvocation(this, start + i), GetInvocation(value, i)))
                    {
                        equal = false;
                        break;
                    }
                }

                if (!equal)
                    continue;

                int resultCount = sourceCount - valueCount;
                if (resultCount == 0)
                    return null;

                if (resultCount == 1)
                    return GetInvocation(this, start == 0 ? valueCount : 0);

                Delegate[] invocationList = new Delegate[resultCount];
                int destination = 0;
                for (int source = 0; source < sourceCount; source++)
                {
                    if (source >= start && source < start + valueCount)
                        continue;

                    invocationList[destination++] = GetInvocation(this, source);
                }

                return NewMulticastDelegate(invocationList, resultCount);
            }

            return this;
        }

        public virtual Delegate[] GetInvocationList()
        {
            int count = GetInvocationCount(this);
            Delegate[] invocationList = new Delegate[count];
            CopyInvocationList(this, invocationList, 0);
            return invocationList;
        }

        internal static bool EqualsImpl(Delegate first, Delegate second)
        {
            if (ReferenceEquals(first, second))
                return true;
            if (first == null || second == null || !InternalEqualTypes(first, second))
                return false;

            bool firstIsMulticast = IsMulticast(first);
            bool secondIsMulticast = IsMulticast(second);
            if (firstIsMulticast || secondIsMulticast)
            {
                if (!firstIsMulticast || !secondIsMulticast)
                    return false;

                Delegate[] firstList = GetInvocationArray(first);
                Delegate[] secondList = GetInvocationArray(second);
                int count = (int)first.m_extraFunctionPointerOrData;
                if (count != (int)second.m_extraFunctionPointerOrData)
                    return false;

                for (int i = 0; i < count; i++)
                {
                    if (!EqualsImpl(firstList[i], secondList[i]))
                        return false;
                }

                return true;
            }

            if (!ReferenceEquals(first.m_helperObject, second.m_helperObject) ||
                first.m_extraFunctionPointerOrData != second.m_extraFunctionPointerOrData ||
                first.m_functionPointer != second.m_functionPointer)
            {
                return false;
            }

            if (ReferenceEquals(first.m_firstParameter, first))
                return ReferenceEquals(second.m_firstParameter, second);

            return ReferenceEquals(first.m_firstParameter, second.m_firstParameter);
        }

        private static int GetInvocationCount(Delegate value)
            => IsMulticast(value) ? (int)value.m_extraFunctionPointerOrData : 1;

        private static Delegate GetInvocation(Delegate value, int index)
        {
            if (!IsMulticast(value))
                return value;

            return GetInvocationArray(value)[index];
        }

        private static void CopyInvocationList(Delegate value, Delegate[] destination, int destinationIndex)
        {
            if (!IsMulticast(value))
            {
                destination[destinationIndex] = value;
                return;
            }

            Delegate[] invocationList = GetInvocationArray(value);
            int count = (int)value.m_extraFunctionPointerOrData;
            for (int i = 0; i < count; i++)
                destination[destinationIndex + i] = invocationList[i];
        }

        private static bool IsMulticast(Delegate value)
            => value.m_functionPointer == value.GetThunk(MulticastThunk);

        private static Delegate[] GetInvocationArray(Delegate value)
            => Unsafe.As<object, Delegate[]>(ref value.m_helperObject);

        private unsafe MulticastDelegate NewMulticastDelegate(Delegate[] invocationList, int invocationCount)
        {
            MulticastDelegate result = (MulticastDelegate)InternalCalls.RhpNewFast(EEType);
            result.m_firstParameter = result;
            result.m_helperObject = invocationList;
            result.m_extraFunctionPointerOrData = (IntPtr)invocationCount;
            result.m_functionPointer = GetThunk(MulticastThunk);
            return result;
        }

        private static unsafe bool InternalEqualTypes(Delegate first, Delegate second)
            => first.EEType == second.EEType;

        protected void InitializeClosedInstance(object firstParameter, IntPtr functionPointer)
        {
            m_firstParameter = firstParameter;
            m_functionPointer = functionPointer;
        }

        protected void InitializeClosedInstanceSlow(object firstParameter, IntPtr functionPointer)
        {
            if ((((long)functionPointer) & 2) == 0)
            {
                m_firstParameter = firstParameter;
                m_functionPointer = functionPointer;
            }
            else
            {
                m_firstParameter = this;
                m_helperObject = firstParameter;
                m_extraFunctionPointerOrData = functionPointer;
                m_functionPointer = GetThunk(ClosedInstanceThunkOverGenericMethod);
            }
        }

        protected void InitializeClosedStaticThunk(object firstParameter, IntPtr functionPointer, IntPtr functionPointerThunk)
        {
            m_firstParameter = this;
            m_helperObject = firstParameter;
            m_extraFunctionPointerOrData = functionPointer;
            m_functionPointer = functionPointerThunk;
        }

        protected void InitializeOpenStaticThunk(object firstParameter, IntPtr functionPointer, IntPtr functionPointerThunk)
        {
            m_firstParameter = this;
            m_extraFunctionPointerOrData = functionPointer;
            m_functionPointer = functionPointerThunk;
        }

        protected void InitializeOpenInstanceThunkDynamic(IntPtr functionPointer, IntPtr functionPointerThunk)
        {
            m_firstParameter = this;
            m_extraFunctionPointerOrData = functionPointer;
            m_functionPointer = functionPointerThunk;
        }

        private void InitializeClosedInstanceToInterface(object firstParameter, IntPtr dispatchCell)
        {
            m_functionPointer = CachedInterfaceDispatch.RhpResolveInterfaceMethod(firstParameter, dispatchCell);
            m_firstParameter = firstParameter;
        }
    }

    public delegate void Action();
    public delegate void Action<in T>(T arg);
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
    public delegate void Action<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate TResult Func<out TResult>();
    public delegate TResult Func<in T, out TResult>(T arg);
    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
    public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);


    public abstract class MulticastDelegate : Delegate
    {
        public override bool Equals(object obj)
        {
            Delegate other = obj as Delegate;
            return other != null && EqualsImpl(this, other);
        }
    }

    public unsafe struct RuntimeTypeHandle
    {
        private IntPtr _value;

        internal RuntimeTypeHandle(EETypePtr eeType)
        {
            _value = (IntPtr)eeType.Value;
        }

        internal EEType* EEType => (EEType*)(void*)_value;

        internal bool IsNull => _value == IntPtr.Zero;
    }
    public struct RuntimeMethodHandle { }
    public struct RuntimeFieldHandle { }

    public class Attribute { }

    public sealed class FlagsAttribute : Attribute { }

    [Flags]
    public enum AttributeTargets
    {
        Assembly = 0x0001,
        Module = 0x0002,
        Class = 0x0004,
        Struct = 0x0008,
        Enum = 0x0010,
        Constructor = 0x0020,
        Method = 0x0040,
        Property = 0x0080,
        Field = 0x0100,
        Event = 0x0200,
        Interface = 0x0400,
        Parameter = 0x0800,
        Delegate = 0x1000,
        ReturnValue = 0x2000,
        GenericParameter = 0x4000,
        All = Assembly | Module | Class | Struct | Enum | Constructor |
              Method | Property | Field | Event | Interface | Parameter |
              Delegate | ReturnValue | GenericParameter
    }

    public sealed class AttributeUsageAttribute : Attribute
    {
        private readonly AttributeTargets _validOn;
        private bool _allowMultiple;
        private bool _inherited;

        public AttributeUsageAttribute(AttributeTargets validOn)
        {
            _validOn = validOn;
            _inherited = true;
        }

        public AttributeTargets ValidOn => _validOn;

        public bool AllowMultiple
        {
            get => _allowMultiple;
            set => _allowMultiple = value;
        }

        public bool Inherited
        {
            get => _inherited;
            set => _inherited = value;
        }
    }

    public sealed class ParamArrayAttribute : Attribute
    {
        public ParamArrayAttribute() { }
    }

    public static partial class AppContext
    {
        public static void SetData(string s, object o) { }
    }



    namespace Reflection
    {
        public sealed class DefaultMemberAttribute(string memberName) : Attribute
        {
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

        [StructLayout(LayoutKind.Sequential)]
        internal class RawData
        {
            public byte Data;
        }

        public static class RuntimeFeature
        {
            public const string UnmanagedSignatureCallingConvention = nameof(UnmanagedSignatureCallingConvention);
        }

        internal sealed class IntrinsicAttribute : Attribute { }

        public sealed class RequiredMemberAttribute : Attribute { }

        public sealed class CompilerGeneratedAttribute : Attribute { }

        public interface IAsyncStateMachine
        {
            void MoveNext();
            void SetStateMachine(IAsyncStateMachine stateMachine);
        }

        public interface INotifyCompletion
        {
            void OnCompleted(Action continuation);
        }

        public interface ICriticalNotifyCompletion : INotifyCompletion
        {
            void UnsafeOnCompleted(Action continuation);
        }

        public class StateMachineAttribute(Type stateMachineType) : Attribute
        {
            public Type StateMachineType { get; } = stateMachineType;
        }

        public sealed class AsyncStateMachineAttribute(Type stateMachineType) : StateMachineAttribute(stateMachineType)
        {
        }

        public sealed class AsyncMethodBuilderAttribute(Type builderType) : Attribute
        {
            public Type BuilderType { get; } = builderType;
        }

        public sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute
        {
        }

        public sealed class MethodImplAttribute(MethodImplOptions methodImplOptions) : Attribute
        {
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

        public readonly struct TaskAwaiter : ICriticalNotifyCompletion
        {
            private readonly Task _task;

            internal TaskAwaiter(Task task) => _task = task;

            public bool IsCompleted => _task.IsCompleted;

            public void OnCompleted(Action continuation) => _task.AddContinuation(continuation);

            public void UnsafeOnCompleted(Action continuation) => _task.AddContinuation(continuation);

            public void GetResult() => _task.GetResult();
        }

        public readonly struct TaskAwaiter<TResult> : ICriticalNotifyCompletion
        {
            private readonly Task<TResult> _task;

            internal TaskAwaiter(Task<TResult> task) => _task = task;

            public bool IsCompleted => _task.IsCompleted;

            public void OnCompleted(Action continuation) => _task.AddContinuation(continuation);

            public void UnsafeOnCompleted(Action continuation) => _task.AddContinuation(continuation);

            public TResult GetResult() => _task.GetResultValue();
        }

        public readonly struct ConfiguredTaskAwaitable
        {
            private readonly Task _task;

            internal ConfiguredTaskAwaitable(Task task) => _task = task;

            public ConfiguredTaskAwaiter GetAwaiter() => new ConfiguredTaskAwaiter(_task);

            public readonly struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion
            {
                private readonly Task _task;

                internal ConfiguredTaskAwaiter(Task task) => _task = task;

                public bool IsCompleted => _task.IsCompleted;
                public void OnCompleted(Action continuation) => _task.AddContinuation(continuation);
                public void UnsafeOnCompleted(Action continuation) => _task.AddContinuation(continuation);
                public void GetResult() => _task.GetResult();
            }
        }

        public readonly struct ConfiguredTaskAwaitable<TResult>
        {
            private readonly Task<TResult> _task;

            internal ConfiguredTaskAwaitable(Task<TResult> task) => _task = task;

            public ConfiguredTaskAwaiter GetAwaiter() => new ConfiguredTaskAwaiter(_task);

            public readonly struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion
            {
                private readonly Task<TResult> _task;

                internal ConfiguredTaskAwaiter(Task<TResult> task) => _task = task;

                public bool IsCompleted => _task.IsCompleted;
                public void OnCompleted(Action continuation) => _task.AddContinuation(continuation);
                public void UnsafeOnCompleted(Action continuation) => _task.AddContinuation(continuation);
                public TResult GetResult() => _task.GetResultValue();
            }
        }

        public struct AsyncTaskMethodBuilder
        {
            private Task _task;

            public static AsyncTaskMethodBuilder Create() => default;

            public Task Task
            {
                get
                {
                    if (_task == null)
                        _task = new Task();
                    return _task;
                }
            }

            public void Start<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine
                => stateMachine.MoveNext();

            public void SetStateMachine(IAsyncStateMachine stateMachine) { }

            public void SetResult() => Task.TrySetResult();

            public void SetException(Exception exception) => Task.TrySetException(exception);

            private AsyncStateMachineBox<TStateMachine> GetStateMachineBox<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine
            {
                if (_task is AsyncStateMachineBox<TStateMachine> box)
                {
                    box.StateMachine = stateMachine;
                    return box;
                }

                box = new AsyncStateMachineBox<TStateMachine>();
                _task = box;
                box.StateMachine = stateMachine;
                return box;
            }

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                awaiter.OnCompleted(GetStateMachineBox(ref stateMachine).MoveNextAction);
            }

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                awaiter.UnsafeOnCompleted(GetStateMachineBox(ref stateMachine).MoveNextAction);
            }
        }

        public struct AsyncTaskMethodBuilder<TResult>
        {
            private Task<TResult> _task;

            public static AsyncTaskMethodBuilder<TResult> Create() => default;

            public Task<TResult> Task
            {
                get
                {
                    if (_task == null)
                        _task = new Task<TResult>();
                    return _task;
                }
            }

            public void Start<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine
                => stateMachine.MoveNext();

            public void SetStateMachine(IAsyncStateMachine stateMachine) { }

            public void SetResult(TResult result) => Task.TrySetResult(result);

            public void SetException(Exception exception) => Task.TrySetException(exception);

            private AsyncStateMachineBox<TStateMachine, TResult> GetStateMachineBox<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine
            {
                if (_task is AsyncStateMachineBox<TStateMachine, TResult> box)
                {
                    box.StateMachine = stateMachine;
                    return box;
                }

                box = new AsyncStateMachineBox<TStateMachine, TResult>();
                _task = box;
                box.StateMachine = stateMachine;
                return box;
            }

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                awaiter.OnCompleted(GetStateMachineBox(ref stateMachine).MoveNextAction);
            }

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                awaiter.UnsafeOnCompleted(GetStateMachineBox(ref stateMachine).MoveNextAction);
            }
        }

        public struct AsyncVoidMethodBuilder
        {
            public static AsyncVoidMethodBuilder Create() => default;

            public void Start<TStateMachine>(ref TStateMachine stateMachine)
                where TStateMachine : IAsyncStateMachine
                => stateMachine.MoveNext();

            public void SetStateMachine(IAsyncStateMachine stateMachine) { }
            public void SetResult() { }
            public void SetException(Exception exception) => throw exception;

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                IAsyncStateMachine boxedStateMachine = stateMachine;
                AsyncStateMachineContinuation continuation = new AsyncStateMachineContinuation(boxedStateMachine.MoveNext);
                awaiter.OnCompleted(continuation.Invoke);
            }

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                IAsyncStateMachine boxedStateMachine = stateMachine;
                AsyncStateMachineContinuation continuation = new AsyncStateMachineContinuation(boxedStateMachine.MoveNext);
                awaiter.UnsafeOnCompleted(continuation.Invoke);
            }
        }
    }
}

namespace System.Collections
{
    public interface IEnumerable
    {
        IEnumerator GetEnumerator();
    }

    public interface IEnumerator
    {
        object Current { get; }
        bool MoveNext();
        void Reset();
    }
}

namespace System.Collections.Generic
{

    public interface IEnumerable<out T> : IEnumerable
    {
        new IEnumerator<T> GetEnumerator();
    }

    public interface IEnumerator<out T> : IDisposable, IEnumerator
    {
        new T Current { get; }
    }
}

namespace System.Threading.Tasks
{
    public enum TaskStatus
    {
        Created,
        WaitingForActivation,
        WaitingToRun,
        Running,
        WaitingForChildrenToComplete,
        RanToCompletion,
        Canceled,
        Faulted,
    }

    internal abstract class TaskContinuation
    {
        internal TaskContinuation Next;

        internal abstract void Invoke();
    }

    internal sealed class ActionTaskContinuation : TaskContinuation
    {
        private readonly Action _continuation;

        internal ActionTaskContinuation(Action continuation) => _continuation = continuation;

        internal override void Invoke() => _continuation();
    }

    internal sealed class AsyncStateMachineContinuation : TaskContinuation
    {
        private readonly Action _moveNext;

        internal AsyncStateMachineContinuation(Action moveNext) => _moveNext = moveNext;

        internal override void Invoke() => _moveNext();
    }

    internal sealed class AsyncStateMachineBox<TStateMachine> : Task
        where TStateMachine : IAsyncStateMachine
    {
        private readonly Action _moveNextAction;

        internal TStateMachine StateMachine;

        internal AsyncStateMachineBox()
        {
            _moveNextAction = MoveNext;
        }

        internal Action MoveNextAction => _moveNextAction;

        private void MoveNext() => StateMachine.MoveNext();
    }

    internal sealed class AsyncStateMachineBox<TStateMachine, TResult> : Task<TResult>
        where TStateMachine : IAsyncStateMachine
    {
        private readonly Action _moveNextAction;

        internal TStateMachine StateMachine;

        internal AsyncStateMachineBox()
        {
            _moveNextAction = MoveNext;
        }

        internal Action MoveNextAction => _moveNextAction;

        private void MoveNext() => StateMachine.MoveNext();
    }

    public partial class Task
    {
        private const int Pending = 0;
        private const int Completed = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;

        private int _state;
        private Exception _exception;
        private TaskContinuation _continuations;

        internal Task() { }

        public bool IsCompleted => _state != Pending;
        public bool IsCompletedSuccessfully => _state == Completed;
        public bool IsFaulted => _state == Faulted;
        public bool IsCanceled => _state == Canceled;
        public Exception Exception => _exception;
        public TaskStatus Status => _state == Pending
            ? TaskStatus.WaitingForActivation
            : (_state == Completed ? TaskStatus.RanToCompletion :
              (_state == Canceled ? TaskStatus.Canceled : TaskStatus.Faulted));

        public static Task CompletedTask
        {
            get
            {
                Task task = new Task();
                task.TrySetResult();
                return task;
            }
        }

        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            Task<TResult> task = new Task<TResult>();
            task.TrySetResult(result);
            return task;
        }

        public static Task FromException(Exception exception)
        {
            Task task = new Task();
            task.TrySetException(exception);
            return task;
        }

        public static Task<TResult> FromException<TResult>(Exception exception)
        {
            Task<TResult> task = new Task<TResult>();
            task.TrySetException(exception);
            return task;
        }

        public TaskAwaiter GetAwaiter() => new TaskAwaiter(this);

        public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext)
            => new ConfiguredTaskAwaitable(this);

        public void Wait()
        {
            while (!IsCompleted)
                Yield();

            ThrowIfFaulted();
        }

        public static bool Yield() => TaskScheduler.Yield();

        internal void AddContinuation(Action continuation)
        {
            if (continuation == null)
                throw new Exception("The task continuation cannot be null.");

            if (IsCompleted)
            {
                continuation();
                return;
            }

            AddContinuation(new ActionTaskContinuation(continuation));
        }

        private void AddContinuation(TaskContinuation continuation)
        {
            continuation.Next = _continuations;
            _continuations = continuation;
        }

        internal bool TrySetResult()
        {
            if (_state != Pending)
                return false;

            _state = Completed;
            RunContinuations();
            return true;
        }

        internal bool TrySetException(Exception exception)
        {
            if (_state != Pending)
                return false;

            _exception = exception;
            _state = Faulted;
            RunContinuations();
            return true;
        }

        internal bool TrySetCanceled()
        {
            if (_state != Pending)
                return false;

            _state = Canceled;
            RunContinuations();
            return true;
        }

        internal void GetResult() => Wait();

        protected void ThrowIfFaulted()
        {
            if (_state == Faulted)
                throw _exception;
            if (_state == Canceled)
                throw new OperationCanceledException();
        }

        private void RunContinuations()
        {
            TaskContinuation continuation = _continuations;
            _continuations = null;
            while (continuation != null)
            {
                TaskContinuation next = continuation.Next;
                continuation.Next = null;
                continuation.Invoke();
                continuation = next;
            }
        }
    }

    public class Task<TResult> : Task
    {
        private TResult _result;

        internal Task() { }

        public TResult Result => GetResultValue();

        public new TaskAwaiter<TResult> GetAwaiter() => new TaskAwaiter<TResult>(this);

        public new ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            => new ConfiguredTaskAwaitable<TResult>(this);

        internal bool TrySetResult(TResult result)
        {
            _result = result;
            return TrySetResult();
        }

        internal new bool TrySetCanceled() => base.TrySetCanceled();

        internal TResult GetResultValue()
        {
            Wait();
            return _result;
        }
    }

    public sealed class TaskCompletionSource
    {
        private readonly Task _task = new Task();

        public Task Task => _task;

        public void SetResult()
        {
            if (!_task.TrySetResult())
                throw new Exception("The task has already completed.");
        }

        public bool TrySetResult() => _task.TrySetResult();

        public void SetException(Exception exception)
        {
            if (!_task.TrySetException(exception))
                throw new Exception("The task has already completed.");
        }

        public bool TrySetException(Exception exception) => _task.TrySetException(exception);

        public void SetCanceled()
        {
            if (!_task.TrySetCanceled())
                throw new Exception("The task has already completed.");
        }

        public bool TrySetCanceled() => _task.TrySetCanceled();
    }

    public sealed class TaskCompletionSource<TResult>
    {
        private readonly Task<TResult> _task = new Task<TResult>();

        public Task<TResult> Task => _task;

        public void SetResult(TResult result)
        {
            if (!_task.TrySetResult(result))
                throw new Exception("The task has already completed.");
        }

        public bool TrySetResult(TResult result) => _task.TrySetResult(result);

        public void SetException(Exception exception)
        {
            if (!_task.TrySetException(exception))
                throw new Exception("The task has already completed.");
        }

        public bool TrySetException(Exception exception) => _task.TrySetException(exception);

        public void SetCanceled()
        {
            if (!_task.TrySetCanceled())
                throw new Exception("The task has already completed.");
        }

        public bool TrySetCanceled() => _task.TrySetCanceled();
    }
}

namespace System.Threading
{
    public static class Monitor
    {
        public static void Enter(object obj)
        {
            bool lockTaken = false;
            Enter(obj, ref lockTaken);
        }

        public static void Enter(object obj, ref bool lockTaken) => TaskScheduler.Enter(obj, ref lockTaken);

        public static void Exit(object obj) => TaskScheduler.Exit(obj);
    }
}

namespace System.Runtime.InteropServices
{

    public class UnmanagedType { }

    public class Marshal
    {
        public static unsafe nint AllocHGlobal(int cb) => (nint)GarbageCollector.AllocateNative((ulong)cb);

        public static unsafe void FreeHGlobal(IntPtr hglobal) => GarbageCollector.FreeNative((void*)hglobal);

        public static IntPtr AllocCoTaskMem(int cb) => AllocHGlobal(cb);

        public static void FreeCoTaskMem(IntPtr ptr) => FreeHGlobal(ptr);
    }

    public sealed class InAttribute : Attribute { }

    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public UnmanagedCallersOnlyAttribute() { }
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

    public sealed class FieldOffsetAttribute(int offset) : Attribute
    {
        public int Value { get; } = offset;
    }

    public sealed class DllImportAttribute(string dllName) : Attribute
    {
        public CallingConvention CallingConvention;

        public string EntryPoint;
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
        internal sealed class RuntimeExportAttribute(string entry) : Attribute
        {
        }

        public sealed class RuntimeImportAttribute(string dllName, string entry) : Attribute
        {
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

    internal enum ReflectionMapBlob
    {
        TypeMap = 1,
        CommonFixupsTable = 8,
        EmbeddedMetadata = 13,
        BlobIdStackTraceMethodRvaToTokenMapping = 27,
    }

    internal static class GCStaticRegionConstants
    {
        public const int Uninitialized = 0x1;
        public const int HasPreInitializedData = 0x2;
        public const int Mask = Uninitialized | HasPreInitializedData;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EEInterfaceInfo
    {
        private EEType* _interfaceType;

        internal EEType* InterfaceType
        {
            get
            {
                ulong value = (ulong)_interfaceType;
                if ((value & 1) != 0)
                    return *(EEType**)(value - 1);
                return _interfaceType;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct DispatchMap
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Entry
        {
            internal ushort InterfaceIndex;
            internal ushort InterfaceMethodSlot;
            internal ushort ImplementationMethodSlot;
        }

        internal uint EntryCount;

        internal Entry* Entries
        {
            get
            {
                fixed (DispatchMap* map = &this)
                    return (Entry*)((byte*)map + sizeof(uint));
            }
        }
    }

}

namespace Internal.Metadata.NativeFormat
{
    public sealed unsafe partial class MetadataReader
    {
        private const byte MetadataHandleConstantString = 0x1a;
        private const byte MetadataHandleMethod = 0x28;
        private const byte MetadataHandleNamespaceDefinition = 0x2f;
        private const byte MetadataHandleNamespaceReference = 0x30;
        private const byte MetadataHandleMemberReference = 0x27;
        private const byte MetadataHandleQualifiedMethod = 0x36;
        private const byte MetadataHandleTypeDefinition = 0x3a;
        private const byte MetadataHandleTypeReference = 0x3d;

        private readonly byte* _nativeMetadata;
        private readonly byte* _nativeMetadataEnd;

        public MetadataReader(IntPtr pBuffer, int cbBuffer)
        {
            _nativeMetadata = (byte*)pBuffer;
            _nativeMetadataEnd = _nativeMetadata + cbBuffer;
        }

        internal bool TryGetTypeFullName(uint token, out string typeName) =>
            TryFormatTypeName(token, 0, out typeName);

        internal bool TryGetTypeName(uint token, out string typeName) =>
            TryGetTypeSimpleName(token, out typeName);

        internal bool TryGetTypeNamespace(uint token, out string namespaceName) =>
            TryGetTypeNamespace(token, 0, out namespaceName);

        internal bool TryFormatMethodName(uint token, out string methodName) =>
            TryFormatStackTraceMethod(token, out methodName);

        private bool TryFormatStackTraceMethod(uint token, out string methodName)
        {
            byte type = (byte)(token >> 24);
            if (type == MetadataHandleQualifiedMethod)
                return TryFormatQualifiedMethod(token, out methodName);
            if (type == MetadataHandleMemberReference)
                return TryFormatMemberReference(token, out methodName);

            methodName = null;
            return false;
        }

        private bool TryFormatQualifiedMethod(uint token, out string methodName)
        {
            methodName = null;
            if (!TryGetMetadataCursor(token, MetadataHandleQualifiedMethod, out byte* cursor) ||
                !TryReadMetadataTypedHandle(ref cursor, MetadataHandleMethod, out uint methodToken) ||
                !TryReadMetadataTypedHandle(ref cursor, MetadataHandleTypeDefinition, out uint typeToken) ||
                !TryReadMethodName(methodToken, out string name) ||
                !TryFormatTypeName(typeToken, 0, out string typeName))
                return false;

            methodName = typeName.Length == 0 ? name + "()" : typeName + "." + name + "()";
            return true;
        }

        private bool TryFormatMemberReference(uint token, out string methodName)
        {
            methodName = null;
            if (!TryGetMetadataCursor(token, MetadataHandleMemberReference, out byte* cursor) ||
                !TryReadMetadataHandle(ref cursor, out uint parentTypeToken) ||
                !TryReadMetadataTypedHandle(ref cursor, MetadataHandleConstantString, out uint nameToken) ||
                !TryReadMetadataHandle(ref cursor, out _) ||
                !TryReadMetadataString(nameToken, out string name) ||
                !TryFormatTypeName(parentTypeToken, 0, out string typeName))
                return false;

            methodName = typeName.Length == 0 ? name + "()" : typeName + "." + name + "()";
            return true;
        }

        private bool TryReadMethodName(uint token, out string methodName)
        {
            methodName = null;
            if (!TryGetMetadataCursor(token, MetadataHandleMethod, out byte* cursor) ||
                !TryReadMetadataUnsigned(ref cursor, out _) ||
                !TryReadMetadataUnsigned(ref cursor, out _) ||
                !TryReadMetadataTypedHandle(ref cursor, MetadataHandleConstantString, out uint nameToken))
                return false;

            return TryReadMetadataString(nameToken, out methodName);
        }

        private bool TryGetTypeSimpleName(uint token, out string typeName)
        {
            typeName = null;
            byte type = (byte)(token >> 24);
            if (type == MetadataHandleTypeDefinition)
            {
                return TryReadTypeDefinition(token, out _, out uint nameToken, out _) &&
                    TryReadMetadataString(nameToken, out typeName);
            }

            if (type == MetadataHandleTypeReference)
            {
                return TryReadTypeReference(token, out _, out uint nameToken) &&
                    TryReadMetadataString(nameToken, out typeName);
            }

            return false;
        }

        private bool TryGetTypeNamespace(uint token, int depth, out string namespaceName)
        {
            namespaceName = null;
            if (depth == 16)
                return false;

            byte type = (byte)(token >> 24);
            if (type == MetadataHandleTypeDefinition)
            {
                if (!TryReadTypeDefinition(token, out uint namespaceToken, out _, out uint enclosingTypeToken))
                    return false;

                if ((enclosingTypeToken >> 24) == MetadataHandleTypeDefinition)
                    return TryGetTypeNamespace(enclosingTypeToken, depth + 1, out namespaceName);

                return TryFormatNamespaceName(namespaceToken, depth, out namespaceName);
            }

            if (type == MetadataHandleTypeReference)
            {
                if (!TryReadTypeReference(token, out uint parentToken, out _))
                    return false;

                if ((parentToken >> 24) == MetadataHandleTypeReference)
                    return TryGetTypeNamespace(parentToken, depth + 1, out namespaceName);

                return TryFormatNamespaceName(parentToken, depth, out namespaceName);
            }

            return false;
        }

        private bool TryFormatTypeName(uint token, int depth, out string typeName)
        {
            typeName = null;
            if (depth == 16)
                return false;

            byte type = (byte)(token >> 24);
            if (type == MetadataHandleTypeReference)
                return TryFormatTypeReferenceName(token, depth, out typeName);

            if (type != MetadataHandleTypeDefinition ||
                !TryReadTypeDefinition(token, out uint namespaceToken, out uint nameToken, out uint enclosingTypeToken) ||
                !TryReadMetadataString(nameToken, out string name))
                return false;

            if ((enclosingTypeToken >> 24) == MetadataHandleTypeDefinition &&
                TryFormatTypeName(enclosingTypeToken, depth + 1, out string enclosingTypeName))
            {
                typeName = enclosingTypeName + "+" + name;
                return true;
            }

            if (TryFormatNamespaceName(namespaceToken, depth, out string namespaceName) && namespaceName.Length != 0)
                typeName = namespaceName + "." + name;
            else
                typeName = name;

            return true;
        }

        private bool TryFormatTypeReferenceName(uint token, int depth, out string typeName)
        {
            typeName = null;
            if (!TryReadTypeReference(token, out uint parentToken, out uint nameToken) ||
                !TryReadMetadataString(nameToken, out string name))
                return false;

            byte parentType = (byte)(parentToken >> 24);
            if (parentType == MetadataHandleTypeReference &&
                TryFormatTypeReferenceName(parentToken, depth + 1, out string enclosingTypeName))
            {
                typeName = enclosingTypeName + "+" + name;
                return true;
            }

            if (TryFormatNamespaceName(parentToken, depth, out string namespaceName) && namespaceName.Length != 0)
                typeName = namespaceName + "." + name;
            else
                typeName = name;

            return true;
        }

        private bool TryReadTypeDefinition(
            uint token,
            out uint namespaceToken,
            out uint nameToken,
            out uint enclosingTypeToken)
        {
            namespaceToken = 0;
            nameToken = 0;
            enclosingTypeToken = 0;
            if (!TryGetMetadataCursor(token, MetadataHandleTypeDefinition, out byte* cursor) ||
                !TryReadMetadataUnsigned(ref cursor, out _) ||
                !TryReadMetadataHandle(ref cursor, out _) ||
                !TryReadMetadataTypedHandle(ref cursor, MetadataHandleNamespaceDefinition, out namespaceToken) ||
                !TryReadMetadataTypedHandle(ref cursor, MetadataHandleConstantString, out nameToken) ||
                !TryReadMetadataUnsigned(ref cursor, out _) ||
                !TryReadMetadataUnsigned(ref cursor, out _) ||
                !TryReadMetadataTypedHandle(ref cursor, MetadataHandleTypeDefinition, out enclosingTypeToken))
                return false;

            return true;
        }

        private bool TryReadTypeReference(uint token, out uint parentToken, out uint nameToken)
        {
            parentToken = 0;
            nameToken = 0;
            if (!TryGetMetadataCursor(token, MetadataHandleTypeReference, out byte* cursor) ||
                !TryReadMetadataHandle(ref cursor, out parentToken) ||
                !TryReadMetadataTypedHandle(ref cursor, MetadataHandleConstantString, out nameToken))
                return false;

            return true;
        }

        private bool TryFormatNamespaceName(uint token, int depth, out string namespaceName)
        {
            namespaceName = string.Empty;
            if (token == 0)
                return true;

            byte type = (byte)(token >> 24);
            if (depth == 16)
                return false;

            if (type != MetadataHandleNamespaceDefinition && type != MetadataHandleNamespaceReference)
                return true;

            if (!TryGetMetadataCursor(token, type, out byte* cursor) ||
                !TryReadMetadataHandle(ref cursor, out uint parentToken) ||
                !TryReadMetadataTypedHandle(ref cursor, MetadataHandleConstantString, out uint nameToken) ||
                !TryReadMetadataString(nameToken, out string name) ||
                !TryFormatNamespaceName(parentToken, depth + 1, out string parentName))
                return false;

            namespaceName = parentName.Length == 0 ? name : parentName + "." + name;
            return true;
        }

        private bool TryReadMetadataString(uint token, out string value)
        {
            value = null;
            if (token == 0)
            {
                value = string.Empty;
                return true;
            }

            if (!TryGetMetadataCursor(token, MetadataHandleConstantString, out byte* cursor) ||
                !TryReadMetadataUnsigned(ref cursor, out uint byteCount) ||
                byteCount > int.MaxValue || cursor + byteCount > _nativeMetadataEnd)
                return false;

            // Convert UTF-8 bytes to a string. Since we don't have access to System.Text.Encoding.UTF8, we will assume the bytes are ASCII for simplicity.
            char[] bytes = new char[byteCount];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = (char)cursor[i];

            value = new string(bytes);
            return true;
        }

        private bool TryGetMetadataCursor(uint token, byte expectedType, out byte* cursor)
        {
            cursor = null;
            if ((token >> 24) != expectedType || _nativeMetadata == null || _nativeMetadataEnd <= _nativeMetadata)
                return false;

            uint offset = token & 0x00FFFFFF;
            if (offset >= (uint)(_nativeMetadataEnd - _nativeMetadata))
                return false;

            cursor = _nativeMetadata + offset;
            return true;
        }

        private bool TryReadMetadataHandle(ref byte* cursor, out uint token)
        {
            token = 0;
            if (!TryReadMetadataUnsigned(ref cursor, out uint value))
                return false;

            token = ((value & 0xFF) << 24) | (value >> 8);
            return true;
        }

        private bool TryReadMetadataTypedHandle(ref byte* cursor, byte type, out uint token)
        {
            token = 0;
            if (!TryReadMetadataUnsigned(ref cursor, out uint offset))
                return false;

            token = offset == 0 ? 0u : ((uint)type << 24) | offset;
            return true;
        }

        private bool TryReadMetadataUnsigned(ref byte* cursor, out uint value)
        {
            value = 0;
            if (cursor >= _nativeMetadataEnd)
                return false;

            byte first = *cursor;
            if ((first & 1) == 0)
            {
                value = (uint)(first >> 1);
                cursor += 1;
                return true;
            }

            if ((first & 2) == 0)
            {
                if (cursor + 1 >= _nativeMetadataEnd)
                    return false;
                value = (uint)((first >> 2) | (cursor[1] << 6));
                cursor += 2;
                return true;
            }

            if ((first & 4) == 0)
            {
                if (cursor + 2 >= _nativeMetadataEnd)
                    return false;
                value = (uint)((first >> 3) | (cursor[1] << 5) | (cursor[2] << 13));
                cursor += 3;
                return true;
            }

            if ((first & 8) == 0)
            {
                if (cursor + 3 >= _nativeMetadataEnd)
                    return false;
                value = (uint)((first >> 4) | (cursor[1] << 4) | (cursor[2] << 12) | (cursor[3] << 20));
                cursor += 4;
                return true;
            }

            if ((first & 16) != 0 || cursor + 4 >= _nativeMetadataEnd)
                return false;

            value = *(uint*)(cursor + 1);
            cursor += 5;
            return true;
        }
    }
}

namespace Internal.StackTraceMetadata
{
    internal static unsafe class StackTraceMetadata
    {
        private static byte* s_imageBase;
        private static Metadata.NativeFormat.MetadataReader s_metadataReader;
        private static byte* s_methodRvaToTokenMap;
        private static byte* s_methodRvaToTokenMapEnd;

        internal static void Initialize(
            byte* imageBase,
            Metadata.NativeFormat.MetadataReader metadataReader,
            IntPtr methodRvaToTokenMapStart,
            IntPtr methodRvaToTokenMapEnd)
        {
            s_imageBase = imageBase;
            s_metadataReader = metadataReader;
            s_methodRvaToTokenMap = (byte*)methodRvaToTokenMapStart;
            s_methodRvaToTokenMapEnd = (byte*)methodRvaToTokenMapEnd;
        }

        public static string GetMethodNameFromStartAddressIfAvailable(IntPtr methodStartAddress)
        {
            if (s_imageBase == null || methodStartAddress == IntPtr.Zero || s_metadataReader == null ||
                s_methodRvaToTokenMap == null || s_methodRvaToTokenMapEnd <= s_methodRvaToTokenMap)
                return null;

            uint methodRva = (uint)((byte*)methodStartAddress - s_imageBase);

            for (StackTraceMethodMapEntry* entry = (StackTraceMethodMapEntry*)s_methodRvaToTokenMap;
                (byte*)(entry + 1) <= s_methodRvaToTokenMapEnd;
                entry++)
            {
                byte* mappedMethodAddress = (byte*)entry + entry->MethodStartRelPtr;
                if ((uint)(mappedMethodAddress - s_imageBase) != methodRva)
                    continue;

                return s_metadataReader.TryFormatMethodName(unchecked((uint)entry->MetadataToken), out string methodName)
                    ? methodName
                    : null;
            }

            return null;
        }
    }
}

namespace Internal.Runtime.TypeLoader
{
    public sealed unsafe partial class TypeLoaderEnvironment
    {
        public static TypeLoaderEnvironment Instance { get; } = new TypeLoaderEnvironment();

        private byte* _typeMap;
        private byte* _typeMapEnd;
        private byte* _commonFixups;
        private byte* _commonFixupsEnd;
        private Metadata.NativeFormat.MetadataReader _metadataReader;

        internal void Initialize(
            Metadata.NativeFormat.MetadataReader metadataReader,
            IntPtr typeMapStart,
            IntPtr typeMapEnd,
            IntPtr commonFixupsStart,
            IntPtr commonFixupsEnd)
        {
            _metadataReader = metadataReader;
            _typeMap = (byte*)typeMapStart;
            _typeMapEnd = (byte*)typeMapEnd;
            _commonFixups = (byte*)commonFixupsStart;
            _commonFixupsEnd = (byte*)commonFixupsEnd;
        }

        internal bool TryGetTypeFullName(EEType* eeType, out string typeName)
        {
            typeName = null;
            return _metadataReader != null && TryGetMetadataForNamedType(eeType, out uint token) &&
                _metadataReader.TryGetTypeFullName(token, out typeName);
        }

        internal bool TryGetTypeName(EEType* eeType, out string typeName)
        {
            typeName = null;
            return _metadataReader != null && TryGetMetadataForNamedType(eeType, out uint token) &&
                _metadataReader.TryGetTypeName(token, out typeName);
        }

        internal bool TryGetTypeNamespace(EEType* eeType, out string namespaceName)
        {
            namespaceName = null;
            return _metadataReader != null && TryGetMetadataForNamedType(eeType, out uint token) &&
                _metadataReader.TryGetTypeNamespace(token, out namespaceName);
        }

        private bool TryGetMetadataForNamedType(EEType* eeType, out uint metadataToken)
        {
            metadataToken = 0;
            if (eeType == null || _typeMap == null || _typeMapEnd <= _typeMap ||
                _commonFixups == null || _commonFixupsEnd <= _commonFixups)
                return false;

            byte header = *_typeMap;
            int entryIndexSize = header & 3;
            int bucketShift = header >> 2;
            if (entryIndexSize > 2 || bucketShift > 31)
                return false;

            uint bucketMask = bucketShift == 31 ? 0x7FFFFFFFu : (1u << bucketShift) - 1;
            uint indexEntrySize = 1u << entryIndexSize;
            byte* tableBase = _typeMap + 1;
            ulong indexTableSize = ((ulong)bucketMask + 2) * indexEntrySize;
            if (tableBase + indexTableSize > _typeMapEnd)
                return false;

            uint typeHashCode = eeType->HashCode;
            uint bucket = (typeHashCode >> 8) & bucketMask;
            uint start = ReadIndex(tableBase + (ulong)bucket * indexEntrySize, entryIndexSize);
            uint end = ReadIndex(tableBase + ((ulong)bucket + 1) * indexEntrySize, entryIndexSize);
            if (end < start || tableBase + end > _typeMapEnd)
                return false;

            byte* entry = tableBase + start;
            byte* entryEnd = tableBase + end;
            byte lowHashCode = (byte)typeHashCode;
            uint fixupCount = (uint)((_commonFixupsEnd - _commonFixups) / sizeof(int));
            while (entry < entryEnd)
            {
                byte entryLowHashCode = *entry++;
                if (entryLowHashCode > lowHashCode)
                    break;

                byte* relativeOffset = entry;
                if (!TryReadSigned(ref entry, entryEnd, out int delta))
                    return false;

                if (entryLowHashCode != lowHashCode)
                    continue;

                byte* value = relativeOffset + delta;
                if (value < _typeMap || value >= _typeMapEnd ||
                    !TryReadUnsigned(ref value, _typeMapEnd, out uint fixupIndex) ||
                    !TryReadUnsigned(ref value, _typeMapEnd, out uint token) ||
                    fixupIndex >= fixupCount)
                    return false;

                int* fixup = (int*)_commonFixups + fixupIndex;
                EEType* candidate = (EEType*)((byte*)fixup + *fixup);
                if (candidate == eeType)
                {
                    metadataToken = token;
                    return true;
                }
            }

            return false;
        }

        private static uint ReadIndex(byte* address, int entryIndexSize)
        {
            if (entryIndexSize == 0)
                return *address;
            if (entryIndexSize == 1)
                return (uint)(address[0] | (address[1] << 8));

            return (uint)(address[0] | (address[1] << 8) | (address[2] << 16) | (address[3] << 24));
        }

        private static bool TryReadUnsigned(ref byte* cursor, byte* end, out uint value)
        {
            value = 0;
            if (cursor >= end)
                return false;

            byte first = *cursor;
            if ((first & 1) == 0)
            {
                value = (uint)(first >> 1);
                cursor += 1;
                return true;
            }

            if ((first & 2) == 0)
            {
                if (cursor + 1 >= end)
                    return false;
                value = (uint)((first >> 2) | (cursor[1] << 6));
                cursor += 2;
                return true;
            }

            if ((first & 4) == 0)
            {
                if (cursor + 2 >= end)
                    return false;
                value = (uint)((first >> 3) | (cursor[1] << 5) | (cursor[2] << 13));
                cursor += 3;
                return true;
            }

            if ((first & 8) == 0)
            {
                if (cursor + 3 >= end)
                    return false;
                value = (uint)((first >> 4) | (cursor[1] << 4) | (cursor[2] << 12) | (cursor[3] << 20));
                cursor += 4;
                return true;
            }

            if ((first & 16) != 0 || cursor + 4 >= end)
                return false;

            value = *(uint*)(cursor + 1);
            cursor += 5;
            return true;
        }

        private static bool TryReadSigned(ref byte* cursor, byte* end, out int value)
        {
            value = 0;
            if (cursor >= end)
                return false;

            int first = *cursor;
            if ((first & 1) == 0)
            {
                value = ((sbyte)first) >> 1;
                cursor += 1;
                return true;
            }

            if ((first & 2) == 0)
            {
                if (cursor + 1 >= end)
                    return false;
                value = (first >> 2) | ((sbyte)cursor[1] << 6);
                cursor += 2;
                return true;
            }

            if ((first & 4) == 0)
            {
                if (cursor + 2 >= end)
                    return false;
                value = (first >> 3) | (cursor[1] << 5) | ((sbyte)cursor[2] << 13);
                cursor += 3;
                return true;
            }

            if ((first & 8) == 0)
            {
                if (cursor + 3 >= end)
                    return false;
                value = (first >> 4) | (cursor[1] << 4) | (cursor[2] << 12) | ((sbyte)cursor[3] << 20);
                cursor += 4;
                return true;
            }

            if ((first & 16) != 0 || cursor + 4 >= end)
                return false;

            value = *(int*)(cursor + 1);
            cursor += 5;
            return true;
        }
    }
}

namespace System.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct EHVector128
    {
        internal ulong Low;
        internal ulong High;
    }

    // This is the small register context shared by the UEFI throw helper and
    // the managed dispatcher. It deliberately contains only the state needed
    // to resume a Native AOT funclet; the full CoreRT ExInfo chain is not
    // available in this single-threaded UEFI runtime.
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ExInfo
    {
        internal byte* ControlPC;
        internal byte* StackPointer;
        internal byte* FramePointer;
        internal byte* Handler;
        internal ulong Rbx;
        internal ulong Rsi;
        internal ulong Rdi;
        internal ulong R12;
        internal ulong R13;
        internal ulong R14;
        internal ulong R15;
        internal EHVector128 Xmm6;
        internal EHVector128 Xmm7;
        internal EHVector128 Xmm8;
        internal EHVector128 Xmm9;
        internal EHVector128 Xmm10;
        internal EHVector128 Xmm11;
        internal EHVector128 Xmm12;
        internal EHVector128 Xmm13;
        internal EHVector128 Xmm14;
        internal EHVector128 Xmm15;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RuntimeFunction
    {
        internal uint BeginAddress;
        internal uint EndAddress;
        internal uint UnwindData;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct StackTraceMethodMapEntry
    {
        internal int MethodStartRelPtr;
        internal int MetadataToken;
    }

    internal static unsafe class EH
    {
        private struct ActiveExceptionState
        {
            internal object Exception;
            internal ExInfo ExInfo;
            internal uint ClauseIndex;
        }

        // A PE x64 RUNTIME_FUNCTION contains three 32-bit RVAs.
        internal static int RuntimeFunctionSize => sizeof(RuntimeFunction);

        internal static byte* s_imageBase;
        internal static RuntimeFunction* s_exceptionTable;
        internal static int s_runtimeFunctionCount;
        private static readonly ActiveExceptionState[] s_activeExceptions = new ActiveExceptionState[64];
        private static int s_activeExceptionCount;

        private const byte UnwindFlagHandlerMask = 0x03;
        private const byte FunctionKindMask = 0x03;
        private const byte FunctionKindRoot = 0x00;
        private const byte FunctionHasEhInfo = 0x04;
        private const byte FunctionHasAssociatedData = 0x10;

        private const byte UnwindOpPushNonVol = 0;
        private const byte UnwindOpAllocLarge = 1;
        private const byte UnwindOpAllocSmall = 2;
        private const byte UnwindOpSetFpReg = 3;
        private const byte UnwindOpSaveNonVol = 4;
        private const byte UnwindOpSaveNonVolFar = 5;
        private const byte UnwindOpSaveXmm128 = 8;
        private const byte UnwindOpSaveXmm128Far = 9;

        private const byte EhClauseTyped = 0;
        private const byte EhClauseFault = 1;
        private const byte EhClauseFilter = 2;

        // RhpThrowEx is the native entry point in ExceptionHandling.asm.
        // This managed method has the CoreRT name and performs the metadata
        // lookup before the assembly helper calls the selected funclet.
        [RuntimeExport("RhThrowEx")]
        private static void RhThrowEx(object exception, ref ExInfo exInfo)
        {
            if (exception == null)
                exception = new Exception("The runtime raised an exception without an exception object.");

            // Handler search unwinds its ExInfo as it walks callers. Keep the
            // throw-site context intact so an unhandled exception reports the
            // actual frame where it was raised.
            ExInfo searchInfo = exInfo;
            if (s_imageBase != null &&
                s_exceptionTable != null &&
                s_runtimeFunctionCount > 0 &&
                TryFindHandler(exception, ref searchInfo, 0, out uint clauseIndex))
            {
                exInfo = searchInfo;
                PushActiveException(exception, ref exInfo, clauseIndex);
                return;
            }

            ReportUnhandledException(exception, ref exInfo);
            exInfo.Handler = null;
        }

        // CoreRT passes the active ExInfo to RhRethrow. The reduced runtime
        // keeps the equivalent logical context in a single-threaded stack.
        [RuntimeExport("RhRethrow")]
        private static object RhRethrow(ref ExInfo exInfo)
        {
            if (s_activeExceptionCount == 0)
            {
                object missingException = new Exception("The runtime attempted to rethrow without an active exception.");
                ReportUnhandledException(missingException, ref exInfo);
                exInfo.Handler = null;
                return missingException;
            }

            int activeIndex = s_activeExceptionCount - 1;
            ActiveExceptionState active = s_activeExceptions[activeIndex];
            object exception = active.Exception;

            // A catch funclet is physically called by RhpThrowEx, so walking
            // from RhpRethrow's return address would enter the native bridge.
            // Resume from the logical frame saved when this catch was chosen.
            exInfo = active.ExInfo;

            ExInfo searchInfo = exInfo;
            if (s_imageBase != null &&
                s_exceptionTable != null &&
                s_runtimeFunctionCount > 0 &&
                TryFindHandler(exception, ref searchInfo, active.ClauseIndex + 1, out uint clauseIndex))
            {
                exInfo = searchInfo;
                s_activeExceptions[activeIndex].Exception = exception;
                s_activeExceptions[activeIndex].ExInfo = exInfo;
                s_activeExceptions[activeIndex].ClauseIndex = clauseIndex;
                return exception;
            }

            ReportUnhandledException(exception, ref exInfo);
            exInfo.Handler = null;
            return exception;
        }

        [RuntimeExport("RhEndCatch")]
        private static void RhEndCatch()
        {
            if (s_activeExceptionCount == 0)
                return;

            int index = --s_activeExceptionCount;
            s_activeExceptions[index] = default;
        }

        private static void PushActiveException(object exception, ref ExInfo exInfo, uint clauseIndex)
        {
            if (s_activeExceptionCount == s_activeExceptions.Length)
            {
                ReportUnhandledException(new InvalidProgramException("Exception nesting is too deep."), ref exInfo);
                exInfo.Handler = null;
                return;
            }

            int index = s_activeExceptionCount++;
            s_activeExceptions[index].Exception = exception;
            s_activeExceptions[index].ExInfo = exInfo;
            s_activeExceptions[index].ClauseIndex = clauseIndex;
        }

        private static void ReportUnhandledException(object exception, ref ExInfo exInfo)
        {
            Console.WriteLine("Unhandled exception. " + exception.GetType().ToString() + ": " + ((Exception)exception).Message);
            for (int depth = 0; depth < 64 && exInfo.ControlPC != null; depth++)
            {
                byte* controlPC = exInfo.ControlPC;
                RuntimeFunction* function = FindRuntimeFunction(
                    s_exceptionTable,
                    s_runtimeFunctionCount,
                    s_imageBase,
                    controlPC);

                // A return address outside the image has no unwind record. It is
                // stack data, not another managed frame.
                if (function == null)
                    break;

                WriteStackFrame(controlPC, function);
                if (!UnwindFrame(s_imageBase, function, ref exInfo))
                    break;
            }
        }

        private static void WriteStackFrame(byte* controlPC, RuntimeFunction* function)
        {
            ulong address = (ulong)controlPC;
            uint rva = unchecked((uint)(address - (ulong)s_imageBase));
            RuntimeFunction* root = function == null
                ? null
                : FindRootFunction(s_exceptionTable, function, s_imageBase);

            string methodName = root == null
                ? null
                : Internal.StackTraceMetadata.StackTraceMetadata.GetMethodNameFromStartAddressIfAvailable(
                    (IntPtr)(s_imageBase + root->BeginAddress));
            if (methodName != null)
            {
                uint offset = rva - root->BeginAddress;
                Console.WriteLine("   at " + methodName + " + 0x" + Convert.ToString(offset, 16));
                return;
            }

            Console.WriteLine("   at 0x" + Convert.ToString(address, 16) + " (RVA 0x" + Convert.ToString(rva, 16) + ")");
        }

        private static bool TryFindHandler(
            object exception,
            ref ExInfo exInfo,
            uint firstClauseIndex,
            out uint handlerClauseIndex)
        {
            handlerClauseIndex = uint.MaxValue;
            RuntimeFunction* current = FindRuntimeFunction(
                s_exceptionTable,
                s_runtimeFunctionCount,
                s_imageBase,
                exInfo.ControlPC);
            if (current == null)
                return false;

            bool firstFrame = true;
            for (int depth = 0; depth < 64 && current != null; depth++)
            {
                RuntimeFunction* root = FindRootFunction(
                    s_exceptionTable,
                    current,
                    s_imageBase);
                if (root == null)
                    return false;

                if (TryFindTypedHandler(s_imageBase, root,
                    exception, ref exInfo,
                    firstFrame ? firstClauseIndex : 0,
                    out handlerClauseIndex))
                    return true;

                if (!UnwindFrame(s_imageBase, current, ref exInfo))
                    break;

                firstFrame = false;
                current = FindRuntimeFunction(
                    s_exceptionTable,
                    s_runtimeFunctionCount,
                    s_imageBase,
                    exInfo.ControlPC);
            }

            return false;
        }

        private static RuntimeFunction* FindRuntimeFunction(RuntimeFunction* table, int count, byte* imageBase, byte* controlPC)
        {
            ulong address = (ulong)controlPC;
            ulong image = (ulong)imageBase;
            if (address < image)
                return null;

            uint relative = (uint)(address - image);
            for (int i = 0; i < count; i++)
            {
                RuntimeFunction* function = table + i;
                if (relative >= function->BeginAddress && relative < function->EndAddress)
                    return function;
            }

            return null;
        }

        private static RuntimeFunction* FindRootFunction(
            RuntimeFunction* table,
            RuntimeFunction* function,
            byte* imageBase)
        {
            int index = (int)(function - table);
            while (index >= 0)
            {
                byte* unwind = GetUnwindInfo(imageBase, table + index);
                byte blockFlags = GetFunctionBlockFlags(unwind);
                if ((blockFlags & FunctionKindMask) == FunctionKindRoot)
                    return table + index;

                index--;
            }

            return null;
        }

        private static bool TryFindTypedHandler(
            byte* imageBase,
            RuntimeFunction* root,
            object exception,
            ref ExInfo exInfo,
            uint firstClauseIndex,
            out uint handlerClauseIndex)
        {
            handlerClauseIndex = uint.MaxValue;
            byte* unwind = GetUnwindInfo(imageBase, root);
            byte* cursor = GetEhInfoCursor(unwind, imageBase);
            if (cursor == null)
                return false;

            uint clauseCount = ReadVarUInt(ref cursor);
            byte* methodStart = imageBase + root->BeginAddress;
            ulong control = (ulong)exInfo.ControlPC;
            ulong startAddress = (ulong)methodStart;
            if (control < startAddress)
                return false;

            uint codeOffset = (uint)(control - startAddress);
            for (uint clauseIndex = 0; clauseIndex < clauseCount; clauseIndex++)
            {
                uint tryStart = ReadVarUInt(ref cursor);
                uint tryEndAndKind = ReadVarUInt(ref cursor);
                uint tryEnd = tryStart + (tryEndAndKind >> 2);
                byte kind = (byte)(tryEndAndKind & 3);

                if (kind == EhClauseTyped)
                {
                    uint handlerOffset = ReadVarUInt(ref cursor);
                    uint typeRva = *(uint*)cursor;
                    cursor += sizeof(uint);

                    if (clauseIndex >= firstClauseIndex &&
                        codeOffset >= tryStart && codeOffset < tryEnd)
                    {
                        EEType* targetType = (EEType*)(imageBase + typeRva);
                        if (TypeCast.IsInstanceOfClass(targetType, exception) != null)
                        {
                            exInfo.Handler = methodStart + handlerOffset;
                            handlerClauseIndex = clauseIndex;
                            return true;
                        }
                    }
                }
                else if (kind == EhClauseFault)
                {
                    ReadVarUInt(ref cursor);
                }
                else if (kind == EhClauseFilter)
                {
                    uint handlerOffset = ReadVarUInt(ref cursor);
                    uint filterOffset = ReadVarUInt(ref cursor);

                    if (clauseIndex >= firstClauseIndex &&
                        codeOffset >= tryStart && codeOffset < tryEnd &&
                        InternalCalls.RhpCallFilterFunclet(
                            exception,
                            (IntPtr)(methodStart + filterOffset),
                            exInfo.StackPointer,
                            exInfo.FramePointer))
                    {
                        exInfo.Handler = methodStart + handlerOffset;
                        handlerClauseIndex = clauseIndex;
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private static byte* GetUnwindInfo(byte* imageBase, RuntimeFunction* function) => imageBase + function->UnwindData;

        // This is GetUnwindDataBlob from CoreRT's CoffNativeCodeManager.
        // The UBF byte immediately follows the raw unwind-code array unless
        // Windows supplied a personality routine, which adds an aligned RVA.
        private static int GetUnwindDataBlobSize(byte* unwind)
        {
            int size = 4 + unwind[2] * 2;
            if (((unwind[0] >> 3) & UnwindFlagHandlerMask) != 0)
                size = (size + 3 & ~3) + sizeof(uint);

            return size;
        }

        private static byte GetFunctionBlockFlags(byte* unwind)
        {
            return unwind[GetUnwindDataBlobSize(unwind)];
        }

        private static byte* GetEhInfoCursor(byte* unwind, byte* imageBase)
        {
            byte* cursor = unwind + GetUnwindDataBlobSize(unwind);
            byte blockFlags = *cursor++;
            if ((blockFlags & FunctionHasAssociatedData) != 0)
                cursor += sizeof(uint);
            if ((blockFlags & FunctionHasEhInfo) == 0)
                return null;

            uint ehRva = *(uint*)cursor;
            return imageBase + ehRva;
        }

        private static uint ReadVarUInt(ref byte* cursor)
        {
            byte first = *cursor;
            int lengthBits = first & 0x0F;
            uint value;
            switch (lengthBits)
            {
                case 0:
                case 2:
                case 4:
                case 6:
                case 8:
                case 10:
                case 12:
                case 14:
                    value = (uint)(first >> 1);
                    cursor++;
                    return value;
                case 1:
                case 5:
                case 9:
                case 13:
                    value = (uint)(first >> 2) | ((uint)cursor[1] << 6);
                    cursor += 2;
                    return value;
                case 3:
                case 11:
                    value = (uint)(first >> 3) |
                        ((uint)cursor[1] << 5) |
                        ((uint)cursor[2] << 13);
                    cursor += 3;
                    return value;
                case 7:
                    value = (uint)(first >> 4) |
                        ((uint)cursor[1] << 4) |
                        ((uint)cursor[2] << 12) |
                        ((uint)cursor[3] << 20);
                    cursor += 4;
                    return value;
                default:
                    value = (uint)cursor[1] |
                        ((uint)cursor[2] << 8) |
                        ((uint)cursor[3] << 16) |
                        ((uint)cursor[4] << 24);
                    cursor += 5;
                    return value;
            }
        }

        private static bool UnwindFrame(byte* imageBase, RuntimeFunction* function, ref ExInfo exInfo)
        {
            byte* unwind = GetUnwindInfo(imageBase, function);
            if ((unwind[0] & 0x07) == 0x04)
                return false;

            ulong stackPointer = (ulong)exInfo.StackPointer;
            ulong framePointer = (ulong)exInfo.FramePointer;
            byte frameRegister = (byte)(unwind[3] & 0x0F);
            byte frameOffset = (byte)(unwind[3] >> 4);
            if (frameRegister != 0)
            {
                stackPointer = GetRegister(ref exInfo, frameRegister) - (ulong)frameOffset * 16;
            }

            byte count = unwind[2];
            byte* code = unwind + 4;
            for (int i = 0; i < count; i++)
            {
                byte op = (byte)(code[i * 2 + 1] & 0x0F);
                byte info = (byte)(code[i * 2 + 1] >> 4);
                switch (op)
                {
                    case UnwindOpPushNonVol:
                        SetRegister(ref exInfo, info, *(ulong*)stackPointer);
                        stackPointer += 8;
                        break;
                    case UnwindOpAllocLarge:
                        if (info == 0)
                        {
                            stackPointer += (ulong)(*(ushort*)(code + (i + 1) * 2)) * 8;
                            i++;
                        }
                        else
                        {
                            stackPointer += *(uint*)(code + (i + 1) * 2);
                            i += 2;
                        }
                        break;
                    case UnwindOpAllocSmall:
                        stackPointer += (ulong)(info * 8 + 8);
                        break;
                    case UnwindOpSetFpReg:
                        stackPointer = framePointer - (ulong)frameOffset * 16;
                        break;
                    case UnwindOpSaveNonVol:
                        SetRegister(ref exInfo, info, *(ulong*)(stackPointer + (ulong)(*(ushort*)(code + (i + 1) * 2)) * 8));
                        i++;
                        break;
                    case UnwindOpSaveNonVolFar:
                        SetRegister(ref exInfo, info, *(ulong*)(stackPointer + *(uint*)(code + (i + 1) * 2)));
                        i += 2;
                        break;
                    case UnwindOpSaveXmm128:
                    case UnwindOpSaveXmm128Far:
                        // XMM save slots are not needed to find a handler. The
                        // native helper preserves the current values, and the
                        // generated catch funclets do not use them as inputs.
                        i += op == UnwindOpSaveXmm128 ? 1 : 2;
                        break;
                    default:
                        return false;
                }
            }

            ulong returnAddress = *(ulong*)stackPointer;
            stackPointer += 8;
            // A return address points immediately after the call instruction.
            // EH regions are half-open, so use the call site when matching the
            // caller's try range, as CoreRT's stack walker does.
            exInfo.ControlPC = (byte*)(returnAddress - 1);
            exInfo.StackPointer = (byte*)stackPointer;
            return returnAddress != 0;
        }

        private static ulong GetRegister(ref ExInfo exInfo, byte register) => register switch
        {
            3 => exInfo.Rbx,
            5 => (ulong)exInfo.FramePointer,
            6 => exInfo.Rsi,
            7 => exInfo.Rdi,
            12 => exInfo.R12,
            13 => exInfo.R13,
            14 => exInfo.R14,
            15 => exInfo.R15,
            _ => 0
        };

        private static void SetRegister(ref ExInfo exInfo, byte register, ulong value)
        {
            switch (register)
            {
                case 3: exInfo.Rbx = value; break;
                case 5: exInfo.FramePointer = (byte*)value; break;
                case 6: exInfo.Rsi = value; break;
                case 7: exInfo.Rdi = value; break;
                case 12: exInfo.R12 = value; break;
                case 13: exInfo.R13 = value; break;
                case 14: exInfo.R14 = value; break;
                case 15: exInfo.R15 = value; break;
            }
        }
    }

    internal static unsafe class InternalCalls
    {
        [RuntimeExport("__fail_fast")]
        internal static void __fail_fast() => throw new Exception("__fail_fast");

        [RuntimeExport("RhpFallbackFailFast")]
        internal static void RhpFallbackFailFast() => throw new Exception("RhpFallbackFailFast");

        [RuntimeExport("RhpReversePInvoke")]
        internal static void RhpReversePInvoke(IntPtr frame) { }

        [RuntimeExport("RhpReversePInvokeReturn")]
        internal static void RhpReversePInvokeReturn(IntPtr frame) { }

        [RuntimeExport("RhpReversePInvoke2")]
        internal static void RhpReversePInvoke2(IntPtr frame) { }

        [RuntimeExport("RhpReversePInvokeReturn2")]
        internal static void RhpReversePInvokeReturn2(IntPtr frame) { }

        [RuntimeExport("RhpPInvoke")]
        internal static void RhpPInvoke(IntPtr frame) { }

        [RuntimeExport("RhpPInvokeReturn")]
        internal static void RhpPInvokeReturn(IntPtr frame) { }

        [RuntimeImport("*", "RhpCallFilterFunclet")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern bool RhpCallFilterFunclet(
            object exception,
            IntPtr filter,
            byte* stackPointer,
            byte* framePointer);

        [RuntimeExport("RhpNewFast")]
        internal static object RhpNewFast(EEType* pEEType)
        {
            void* ptr = GarbageCollector.Allocate(pEEType->BaseSize);
            if (ptr == null)
                return null;

            object obj = null;
            *(void**)Unsafe.AsPointer(ref obj) = ptr;
            *(EEType**)ptr = pEEType;
            return obj;
        }

        [RuntimeExport("RhpNewArray")]
        internal static object RhpNewArray(EEType* pEEType, int length)
        {
            if (length < 0)
                return null;

            ulong componentSize = pEEType->ComponentSize;
            if (componentSize != 0 && (ulong)length > (~0UL - pEEType->BaseSize) / componentSize)
                return null;

            ulong size = pEEType->BaseSize + (ulong)length * componentSize;
            void* ptr = GarbageCollector.Allocate(size);
            if (ptr == null)
                return null;

            object obj = null;
            *(void**)Unsafe.AsPointer(ref obj) = ptr;
            *(EEType**)ptr = pEEType;

            byte* data = (byte*)ptr + sizeof(IntPtr);
            Unsafe.CopyBlock(data, &length, sizeof(int));
            return obj;
        }

        [RuntimeExport("RhNewString")]
        internal static object RhNewString(EEType* pEEType, int length)
        {
            if (length < 0)
                return null;

            ulong componentSize = sizeof(char);
            if ((ulong)length > (~0UL - pEEType->BaseSize) / componentSize)
                return null;

            ulong size = pEEType->BaseSize + (ulong)length * componentSize;
            void* ptr = GarbageCollector.Allocate(size);
            if (ptr == null)
                return null;

            object obj = null;
            *(void**)Unsafe.AsPointer(ref obj) = ptr;
            *(EEType**)ptr = pEEType;

            byte* lengthAddress = (byte*)ptr + sizeof(IntPtr);
            Unsafe.CopyBlock(lengthAddress, &length, sizeof(int));
            return obj;
        }

        [RuntimeExport("RhBox")]
        internal static object RhBox(EEType* pEEType, ref byte data)
        {
            if (pEEType == null)
                return null;

            object result = RhpNewFast(pEEType);
            if (result == null)
                return null;

            // BaseSize includes the object header and EEType pointer. The remaining
            // bytes are the value-type payload (including any alignment padding).
            uint valueSize = pEEType->BaseSize - (uint)sizeof(ObjHeader) - (uint)sizeof(EEType*);
            fixed (byte* destination = &result.GetRawData())
            fixed (byte* source = &data)
                Unsafe.CopyBlock(destination, source, valueSize);

            return result;
        }

        [RuntimeExport("RhUnbox2")]
        internal static ref byte RhUnbox2(EEType* pUnboxToEEType, object obj)
        {
            if (obj == null)
                throw new NullReferenceException("Cannot unbox a null object.");

            if (!UnboxAnyTypeCompare(obj.EEType, pUnboxToEEType))
                throw new InvalidCastException("The object type cannot be unboxed to the requested value type.");

            return ref obj.GetRawData();
        }

        private static bool UnboxAnyTypeCompare(EEType* objectType, EEType* targetType)
        {
            if (AreTypesEquivalent(objectType, targetType))
                return true;

            EETypeElementType objectElementType = GetElementType(objectType);
            EETypeElementType targetElementType = GetElementType(targetType);
            if (objectElementType != targetElementType)
                return false;

            switch (targetElementType)
            {
                case EETypeElementType.Byte:
                case EETypeElementType.SByte:
                case EETypeElementType.Int16:
                case EETypeElementType.UInt16:
                case EETypeElementType.Int32:
                case EETypeElementType.UInt32:
                case EETypeElementType.Int64:
                case EETypeElementType.UInt64:
                case EETypeElementType.IntPtr:
                case EETypeElementType.UIntPtr:
                    return true;
                default:
                    return false;
            }
        }

        private static bool AreTypesEquivalent(EEType* first, EEType* second)
        {
            if (first == second)
                return true;
            if (first == null || second == null)
                return false;

            if (GetKind(first) == EETypeKind.ClonedEEType)
                first = GetRelatedType(first);
            if (GetKind(second) == EETypeKind.ClonedEEType)
                second = GetRelatedType(second);

            if (first == second)
                return true;

            if (GetKind(first) != EETypeKind.ParameterizedEEType ||
                GetKind(second) != EETypeKind.ParameterizedEEType ||
                first->BaseSize != second->BaseSize)
                return false;

            return AreTypesEquivalent(GetRelatedType(first), GetRelatedType(second));
        }

        private static EETypeKind GetKind(EEType* type)
            => (EETypeKind)((ushort)type->Flags & (ushort)EETypeFlags.EETypeKindMask);

        private static EETypeElementType GetElementType(EEType* type)
            => type == null
                ? EETypeElementType.Unknown
                : (EETypeElementType)(((ushort)type->Flags & (ushort)EETypeFlags.ElementTypeMask) >> (int)EETypeFlags.ElementTypeShift);

        private static EEType* GetRelatedType(EEType* type)
        {
            if ((type->Flags & EETypeFlags.RelatedTypeViaIATFlag) != 0)
                return *type->RelatedType.RelatedParameterTypeViaIAT;

            return type->RelatedType.RelatedParameterType;
        }
    }

    internal static unsafe class TypeCast
    {
        [RuntimeExport("RhpStelemRef")]
        public static void StelemRef(Array array, int index, object obj)
        {
            fixed (int* length = &array.Length)
            {
                byte* element = (byte*)length + sizeof(void*) + index * array.EEType->ComponentSize;
                *(IntPtr*)element = Unsafe.As<object, IntPtr>(ref obj);
            }
        }

        [RuntimeExport("RhTypeCast_IsInstanceOfClass")]
        public static object IsInstanceOfClass(EEType* pTargetType, object obj)
        {
            if (obj == null)
                return null;

            if (pTargetType == obj.EEType)
                return obj;

            // Parameterized array EETypes do not use their RelatedType union
            // as a normal class base. Arrays are nevertheless assignable to
            // System.Array (and System.Object), so handle System.Array before
            // walking the ordinary class hierarchy.
            if (IsArrayType(obj.EEType) && GetElementType(pTargetType) == EETypeElementType.SystemArray)
                return obj;

            EEType* baseType = GetBaseType(obj.EEType);
            while (baseType != null)
            {
                if (pTargetType == baseType)
                    return obj;

                baseType = GetBaseType(baseType);
            }

            return null;
        }

        private static EEType* GetBaseType(EEType* type)
        {
            if (type == null)
                return null;

            EETypeElementType elementType = GetElementType(type);

            // Parameterized array EETypes store their element type in the
            // RelatedType union. Their actual managed base type is System.Array.
            if (elementType == EETypeElementType.Array || elementType == EETypeElementType.SzArray)
                return EETypePtr.EETypePtrOf<Array>().Value;

            if ((type->Flags & EETypeFlags.RelatedTypeViaIATFlag) != 0)
                return *type->RelatedType.BaseTypeViaIAT;

            return type->RelatedType.BaseType;
        }

        private static bool IsArrayType(EEType* type)
        {
            EETypeElementType elementType = GetElementType(type);
            return elementType == EETypeElementType.Array || elementType == EETypeElementType.SzArray;
        }

        private static EETypeElementType GetElementType(EEType* type)
        {
            if (type == null)
                return EETypeElementType.Unknown;

            return (EETypeElementType)(((ushort)type->Flags & (ushort)EETypeFlags.ElementTypeMask) >> (int)EETypeFlags.ElementTypeShift);
        }

        private static EEInterfaceInfo* GetInterfaceMap(EEType* type)
            => (EEInterfaceInfo*)(GetVTable(type) + type->NumVtableSlots);

        private static IntPtr* GetVTable(EEType* type)
            => (IntPtr*)((byte*)type + sizeof(EEType));

        [RuntimeExport("RhTypeCast_CheckCastClass")]
        public static object CheckCastClass(EEType* pTargetEEType, object obj)
        {
            if (obj == null)
                return null;

            object result = IsInstanceOfClass(pTargetEEType, obj);

            if (result == null)
            {
                throw new InvalidCastException("The object cannot be cast to the requested class type.");
            }

            return result;
        }

        [RuntimeExport("RhTypeCast_IsInstanceOfInterface")]
        public static object IsInstanceOfInterface(EEType* pTargetType, object obj)
        {
            if (obj == null || pTargetType == null)
                return null;

            for (EEType* currentType = obj.EEType; currentType != null; currentType = GetBaseType(currentType))
            {
                EEInterfaceInfo* interfaces = GetInterfaceMap(currentType);
                for (ushort index = 0; index < currentType->NumInterfaces; index++)
                {
                    if (interfaces[index].InterfaceType == pTargetType)
                        return obj;
                }
            }

            return null;
        }

        [RuntimeExport("RhTypeCast_CheckCastInterface")]
        public static object CheckCastInterface(EEType* pTargetEEType, object obj)
        {
            object result = IsInstanceOfInterface(pTargetEEType, obj);
            if (result == null && obj != null)
                throw new InvalidCastException("The object does not implement the requested interface.");

            return result;
        }

        [RuntimeExport("RhTypeCast_IsInstanceOfArray")]
        public static object IsInstanceOfArray(EEType* pTargetType, object obj)
        {
            if (obj == null)
                return null;

            // The current runtime only needs exact array casts here. This is
            // sufficient for the Delegate[] invocation lists used by events.
            return obj.EEType == pTargetType ? obj : null;
        }

        [RuntimeExport("RhTypeCast_CheckCastArray")]
        public static object CheckCastArray(EEType* pTargetEEType, object obj)
        {
            object result = IsInstanceOfArray(pTargetEEType, obj);
            if (result == null && obj != null)
                throw new InvalidCastException("The object is not an instance of the requested array type.");

            return result;
        }
    }

    internal static unsafe class CachedInterfaceDispatch
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct InterfaceDispatchCell
        {
            internal IntPtr Stub;
            internal ulong Cache;
        }

        private static DispatchMap** s_dispatchMaps;

        internal static void RegisterDispatchMaps(IntPtr dispatchMaps)
            => s_dispatchMaps = (DispatchMap**)dispatchMaps;

        [RuntimeExport("RhpResolveInterfaceMethod")]
        internal static IntPtr RhpResolveInterfaceMethod(object pObject, IntPtr pCell)
        {
            if (pObject == null || pCell == IntPtr.Zero)
                return IntPtr.Zero;

            InterfaceDispatchCell* cell = (InterfaceDispatchCell*)(void*)pCell;
            EEType* interfaceType = GetInterfaceType(cell);
            ushort interfaceSlot = GetInterfaceSlot(cell);
            EEType* targetType = pObject.EEType;

            for (EEType* currentType = targetType; currentType != null; currentType = GetBaseType(currentType))
            {
                DispatchMap* map = GetDispatchMap(currentType);
                if (map == null)
                    continue;

                DispatchMap.Entry* entries = map->Entries;
                EEInterfaceInfo* interfaces = GetInterfaceMap(currentType);
                for (uint i = 0; i < map->EntryCount; i++)
                {
                    DispatchMap.Entry* entry = entries + i;
                    if (entry->InterfaceMethodSlot != interfaceSlot || entry->InterfaceIndex >= currentType->NumInterfaces)
                        continue;
                    if (interfaces[entry->InterfaceIndex].InterfaceType != interfaceType)
                        continue;

                    ushort implementationSlot = entry->ImplementationMethodSlot;
                    if (implementationSlot < currentType->NumVtableSlots)
                        return GetVTable(targetType)[implementationSlot];

                    return GetSealedVirtualSlot(currentType, (ushort)(implementationSlot - currentType->NumVtableSlots));
                }
            }

            return IntPtr.Zero;
        }

        private static EEType* GetInterfaceType(InterfaceDispatchCell* cell)
        {
            ulong cache = cell->Cache;
            ulong kind = cache & 3;
            if (kind == 1)
                return (EEType*)(cache & ~3UL);

            long relativeOffset = (int)(uint)cache;
            ulong address = (ulong)&cell->Cache + (ulong)relativeOffset;
            address &= ~3UL;
            if (kind == 2)
                return *(EEType**)address;
            return (EEType*)address;
        }

        private static ushort GetInterfaceSlot(InterfaceDispatchCell* cell)
        {
            InterfaceDispatchCell* end = cell;
            while (end->Stub != IntPtr.Zero)
                end++;
            return (ushort)end->Cache;
        }

        private static EEType* GetBaseType(EEType* type)
        {
            if ((type->Flags & EETypeFlags.EETypeKindMask) != 0)
                return null;
            if ((type->Flags & EETypeFlags.RelatedTypeViaIATFlag) != 0)
                return *type->RelatedType.BaseTypeViaIAT;
            return type->RelatedType.BaseType;
        }

        private static IntPtr* GetVTable(EEType* type)
            => (IntPtr*)((byte*)type + sizeof(EEType));

        private static EEInterfaceInfo* GetInterfaceMap(EEType* type)
            => (EEInterfaceInfo*)(GetVTable(type) + type->NumVtableSlots);

        private static DispatchMap* GetDispatchMap(EEType* type)
        {
            if (type->NumInterfaces == 0 || s_dispatchMaps == null)
                return null;

            byte* optionalFields = GetOptionalFields(type);
            uint index = GetOptionalField(optionalFields, 1, 0xffffffff);
            if (index == 0xffffffff)
                return null;
            return s_dispatchMaps[index];
        }

        private static IntPtr GetSealedVirtualSlot(EEType* type, ushort slot)
        {
            byte* field = GetOptionalFieldsPointerField(type);
            if ((type->Flags & EETypeFlags.OptionalFieldsFlag) != 0)
                field += sizeof(int);

            int* tableReference = (int*)field;
            int* table = (int*)((byte*)tableReference + *tableReference);
            int* entry = table + slot;
            return (IntPtr)((byte*)entry + *entry);
        }

        private static byte* GetOptionalFields(EEType* type)
        {
            if ((type->Flags & EETypeFlags.OptionalFieldsFlag) == 0)
                return null;

            int* field = (int*)GetOptionalFieldsPointerField(type);
            return (byte*)field + *field;
        }

        private static byte* GetOptionalFieldsPointerField(EEType* type)
        {
            byte* field = (byte*)GetInterfaceMap(type) + sizeof(EEInterfaceInfo) * type->NumInterfaces;
            field += sizeof(int); // Type manager indirection
            field += sizeof(int); // Writable data
            if ((type->Flags & EETypeFlags.HasFinalizerFlag) != 0)
                field += sizeof(int);
            return field;
        }

        private static uint GetOptionalField(byte* fields, byte requestedTag, uint defaultValue)
        {
            if (fields == null)
                return defaultValue;

            bool last;
            do
            {
                byte header = *fields++;
                last = (header & 0x80) != 0;
                byte tag = (byte)(header & 0x7f);
                uint value = DecodeUnsigned(ref fields);
                if (tag == requestedTag)
                    return value;
            }
            while (!last);

            return defaultValue;
        }

        private static uint DecodeUnsigned(ref byte* data)
        {
            uint first = *data;
            if ((first & 1) == 0)
            {
                data++;
                return first >> 1;
            }
            if ((first & 2) == 0)
            {
                uint value = (first >> 2) | ((uint)data[1] << 6);
                data += 2;
                return value;
            }
            if ((first & 4) == 0)
            {
                uint value = (first >> 3) | ((uint)data[1] << 5) | ((uint)data[2] << 13);
                data += 3;
                return value;
            }
            if ((first & 8) == 0)
            {
                uint value = (first >> 4) | ((uint)data[1] << 4) | ((uint)data[2] << 12) | ((uint)data[3] << 20);
                data += 4;
                return value;
            }

            data++;
            uint result = *(uint*)data;
            data += sizeof(uint);
            return result;
        }
    }

}

namespace Internal.Runtime
{

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
            internal static ref T AddByteOffset<T>(ref T source, nuint byteOffset) => ref AddByteOffset(ref source, (ulong)(void*)byteOffset);

            [RuntimeImport("*", "memcpy")]
            [MethodImpl(MethodImplOptions.InternalCall)]
            public static extern void CopyBlock(void* destination, void* source, ulong byteCount);

            [RuntimeImport("*", "memset")]
            [MethodImpl(MethodImplOptions.InternalCall)]
            public static extern void InitBlock(void* startAddress, int value, ulong byteCount);
        }
    }

    namespace CompilerHelpers
    {
        using Internal.Runtime.CompilerServices;
        using System.Runtime;

        internal static class SynchronizedMethodHelpers
        {
            private static readonly object s_staticLock = new object();

            private static void MonitorEnter(object obj, ref bool lockTaken) => System.Threading.Monitor.Enter(obj, ref lockTaken);

            private static void MonitorExit(object obj, ref bool lockTaken)
            {
                if (!lockTaken)
                    return;

                System.Threading.Monitor.Exit(obj);
                lockTaken = false;
            }

            private static void MonitorEnterStatic(IntPtr pEEType, ref bool lockTaken) => System.Threading.Monitor.Enter(s_staticLock, ref lockTaken);

            private static void MonitorExitStatic(IntPtr pEEType, ref bool lockTaken)
            {
                if (!lockTaken)
                    return;

                System.Threading.Monitor.Exit(s_staticLock);
                lockTaken = false;
            }
        }

        public static class ThrowHelpers
        {
            public static void ThrowInvalidProgramException(int id)
                => throw new InvalidProgramException("The generated method is invalid.");
            public static void ThrowInvalidProgramExceptionWithArgument(int id, string methodName)
                => throw new InvalidProgramException("The generated method with an invalid argument was called.");
            public static void ThrowOverflowException()
                => throw new OverflowException("The generated operation overflowed its numeric range.");
            public static void ThrowIndexOutOfRangeException()
                => throw new IndexOutOfRangeException("The generated array index is outside the valid range.");
            public static void ThrowTypeLoadException(int id, string className, string typeName)
                => throw new TypeLoadException("The requested type could not be loaded.");
        }

        public static partial class StartupCodeHelpers
        {
            private static string[] s_mainMethodArguments;

            internal static unsafe void InitializeCommandLineArgsW(int argc, char** argv)
            {
                if (argc <= 1 || argv == null)
                {
                    s_mainMethodArguments = new string[0];
                    return;
                }

                string[] arguments = new string[argc - 1];
                for (int i = 1; i < argc; i++)
                    arguments[i - 1] = new string(argv[i]);

                s_mainMethodArguments = arguments;
            }

            private static string[] GetMainMethodArguments()
            {
                if (s_mainMethodArguments == null)
                    s_mainMethodArguments = new string[0];

                return s_mainMethodArguments;
            }

            public static unsafe void InitializeModules(byte* ImageBase, IntPtr Modules, byte* ExceptionTable, uint ExceptionTableSize)
            {
                var header = (ReadyToRunHeader*)*(IntPtr*)Modules;
                var sections = (ModuleInfoRow*)(header + 1);
                IntPtr nativeMetadataStart = IntPtr.Zero;
                IntPtr nativeMetadataEnd = IntPtr.Zero;
                IntPtr stackTraceMethodMapStart = IntPtr.Zero;
                IntPtr stackTraceMethodMapEnd = IntPtr.Zero;
                IntPtr typeMapStart = IntPtr.Zero;
                IntPtr typeMapEnd = IntPtr.Zero;
                IntPtr commonFixupsStart = IntPtr.Zero;
                IntPtr commonFixupsEnd = IntPtr.Zero;

                if (header->Signature == ReadyToRunHeaderConstants.Signature)
                {
                    for (int k = 0; k < header->NumberOfSections; k++)
                    {
                        if (sections[k].SectionId == ReadyToRunSectionType.GCStaticRegion)
                        {
                            GarbageCollector.RegisterStatics(sections[k].Start, sections[k].End);
                            InitializeStatics(sections[k].Start, sections[k].End);
                        }

                        if (sections[k].SectionId == ReadyToRunSectionType.InterfaceDispatchTable)
                            CachedInterfaceDispatch.RegisterDispatchMaps(sections[k].Start);

                        if (sections[k].SectionId == ReadyToRunSectionType.EagerCctor)
                            RunEagerClassConstructors(sections[k].Start, sections[k].End);

                        if (sections[k].SectionId == (ReadyToRunSectionType)(
                            (int)ReadyToRunSectionType.ReadonlyBlobRegionStart + (int)ReflectionMapBlob.EmbeddedMetadata))
                        {
                            nativeMetadataStart = sections[k].Start;
                            nativeMetadataEnd = sections[k].End;
                        }

                        if (sections[k].SectionId == (ReadyToRunSectionType)(
                            (int)ReadyToRunSectionType.ReadonlyBlobRegionStart + (int)ReflectionMapBlob.BlobIdStackTraceMethodRvaToTokenMapping))
                        {
                            stackTraceMethodMapStart = sections[k].Start;
                            stackTraceMethodMapEnd = sections[k].End;
                        }

                        if (sections[k].SectionId == (ReadyToRunSectionType)(
                            (int)ReadyToRunSectionType.ReadonlyBlobRegionStart + (int)ReflectionMapBlob.TypeMap))
                        {
                            typeMapStart = sections[k].Start;
                            typeMapEnd = sections[k].End;
                        }

                        if (sections[k].SectionId == (ReadyToRunSectionType)(
                            (int)ReadyToRunSectionType.ReadonlyBlobRegionStart + (int)ReflectionMapBlob.CommonFixupsTable))
                        {
                            commonFixupsStart = sections[k].Start;
                            commonFixupsEnd = sections[k].End;
                        }
                    }
                }

                EH.s_imageBase = ImageBase;
                EH.s_exceptionTable = (RuntimeFunction*)ExceptionTable;
                EH.s_runtimeFunctionCount = (int)(ExceptionTableSize / EH.RuntimeFunctionSize);
                var metadataReader = new Metadata.NativeFormat.MetadataReader(
                    nativeMetadataStart,
                    (int)((byte*)nativeMetadataEnd - (byte*)nativeMetadataStart));
                StackTraceMetadata.StackTraceMetadata.Initialize(
                    ImageBase,
                    metadataReader,
                    stackTraceMethodMapStart,
                    stackTraceMethodMapEnd);
                TypeLoader.TypeLoaderEnvironment.Instance.Initialize(
                    metadataReader,
                    typeMapStart,
                    typeMapEnd,
                    commonFixupsStart,
                    commonFixupsEnd);
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
                                Unsafe.CopyBlock(p, (byte*)pPreInitDataAddr, obj.GetRawDataSize());
                            }
                        }

                        // This ILCompiler's GetGCStaticBase helper dereferences the
                        // GC static cell once, so the cell must contain the object.
                        *pBlock = Unsafe.As<object, IntPtr>(ref obj);
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

namespace Internal.Runtime.CompilerHelpers
{
    // Entry points used by ILC for mkrefany, refanyval and refanytype.
    internal static unsafe class TypedReferenceHelpers
    {
        public static Type TypeHandleToRuntimeTypeMaybeNull(RuntimeTypeHandle typeHandle)
        {
            if (typeHandle.IsNull)
                return null;

            return Type.GetTypeFromHandle(typeHandle);
        }

        public static RuntimeTypeHandle TypeHandleToRuntimeTypeHandleMaybeNull(RuntimeTypeHandle typeHandle)
            => typeHandle;

        public static ref byte GetRefAny(RuntimeTypeHandle type, TypedReference typedRef)
        {
            if (TypedReference.RawTargetTypeToken(typedRef).EEType != type.EEType)
                throw new InvalidCastException();

            return ref typedRef.Value;
        }
    }

    internal static class LdTokenHelpers
    {
        private static RuntimeTypeHandle GetRuntimeTypeHandle(IntPtr pEEType) => new RuntimeTypeHandle(new EETypePtr(pEEType));

        private static RuntimeMethodHandle GetRuntimeMethodHandle(IntPtr pHandleSignature) => new RuntimeMethodHandle();

        private static RuntimeFieldHandle GetRuntimeFieldHandle(IntPtr pHandleSignature) => new RuntimeFieldHandle();

        private static Type GetRuntimeType(IntPtr pEEType) => Type.GetTypeFromEETypePtr(new EETypePtr(pEEType));
    }

    // Entry point used by ILC for array constructors emitted as newobj.
    internal static unsafe class ArrayHelpers
    {
        private static uint SzArrayBaseSize => (uint)(sizeof(IntPtr) * 3);

        public static Array NewObjArray(IntPtr pEEType, int nDimensions, int* pDimensions)
        {
            if (pDimensions == null || nDimensions <= 0)
                throw new ArgumentException("Array dimensions must be provided and the rank must be positive.");

            EEType* eeType = (EEType*)(void*)pEEType;
            if (eeType == null || eeType->BaseSize < SzArrayBaseSize)
                throw new ArgumentException("The array type metadata is invalid.");

            if (eeType->BaseSize == SzArrayBaseSize)
            {
                int length = pDimensions[0];
                if (length < 0)
                    throw new OverflowException("An array length cannot be negative.");

                object resultObject = InternalCalls.RhpNewArray(eeType, length);
                Array result = Unsafe.As<object, Array>(ref resultObject);
                if (result == null || nDimensions == 1)
                    return result;

                // Jagged arrays carry one dimension for each nested SZ array.
                EEType* elementType = GetArrayElementType(eeType);
                object resultReference = result;
                byte* resultAddress = (byte*)(void*)Unsafe.As<object, IntPtr>(ref resultReference);
                byte* elementAddress = resultAddress + eeType->BaseSize - sizeof(ObjHeader);

                for (int i = 0; i < length; i++)
                {
                    Array nested = NewObjArray(
                        (IntPtr)(void*)elementType,
                        nDimensions - 1,
                        pDimensions + 1);
                    object nestedObject = nested;
                    *(IntPtr*)(elementAddress + i * eeType->ComponentSize) =
                        Unsafe.As<object, IntPtr>(ref nestedObject);
                }

                return result;
            }

            uint boundsSize = eeType->BaseSize - SzArrayBaseSize;
            int rank = (int)(boundsSize / (uint)(sizeof(int) * 2));
            if (rank <= 0 || rank != nDimensions && rank * 2 != nDimensions)
                throw new ArgumentException("The array rank does not match the supplied dimensions.");

            // The alternate constructor form supplies lower-bound/length pairs.
            // This runtime supports only zero lower bounds.
            if (rank * 2 == nDimensions)
            {
                for (int i = 0; i < rank; i++)
                {
                    if (pDimensions[i * 2] != 0)
                        throw new NotSupportedException("Non-zero lower array bounds are not supported.");

                    pDimensions[i] = pDimensions[i * 2 + 1];
                }
            }

            ulong totalLength = 1;
            for (int i = 0; i < rank; i++)
            {
                int length = pDimensions[i];
                if (length < 0)
                    throw new OverflowException("An array length cannot be negative.");

                totalLength *= (ulong)length;
                if (totalLength > (~0U >> 1))
                    throw new OverflowException("The requested array is too large for the runtime.");
            }

            object arrayObject = InternalCalls.RhpNewArray(eeType, (int)totalLength);
            Array array = Unsafe.As<object, Array>(ref arrayObject);
            if (array == null)
                return null;

            byte* arrayAddress = (byte*)(void*)Unsafe.As<object, IntPtr>(ref arrayObject);
            int* bounds = (int*)(arrayAddress + sizeof(IntPtr) * 2);
            for (int i = 0; i < rank; i++)
                bounds[i] = pDimensions[i];

            return array;
        }

        private static EEType* GetArrayElementType(EEType* eeType)
        {
            if ((eeType->Flags & EETypeFlags.RelatedTypeViaIATFlag) != 0)
                return *eeType->RelatedType.RelatedParameterTypeViaIAT;

            return eeType->RelatedType.RelatedParameterType;
        }
    }
}
