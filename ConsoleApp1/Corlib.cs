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
            => 0;

        public virtual string ToString() => "System.Object";

        internal ref byte GetRawData()
        {
            return ref Unsafe.As<RawData>(this).Data;
        }

        internal uint GetRawDataSize()
        {
            return EEType->BaseSize - (uint)sizeof(ObjHeader) - (uint)sizeof(EEType*);
        }
    }
    public struct Void { }

    public struct Boolean { }
    public partial struct Char { }
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
    public partial struct Int64 { }
    public partial struct UInt64 { }
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
    public partial struct Double { }
    public class Type { }

    public class Exception
    {
        private readonly string _message;

        public Exception(string message = "Exception of type 'System.Exception' was thrown.") => _message = message;

        public virtual string Message => _message;
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

    public class ArgumentException : Exception
    {
        public ArgumentException() : base("Value does not fall within the expected range.") { }
    }

    internal sealed class ArgumentNullException : Exception
    {
        public ArgumentNullException() : base("Value cannot be null.") { }
    }

    internal sealed class InvalidCastException : Exception
    {
        public InvalidCastException() : base("Specified cast is not valid.") { }
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
            object data = InternalCalls.RhNewString(et.EEType, length);
            string s = Unsafe.As<object, string>(ref data);

            fixed (char* c = &s.FirstChar)
            {
                Unsafe.CopyBlock((byte*)c, (byte*)start, (ulong)length * sizeof(char));
                c[length] = '\0';
            }

            return s;
        }
    }

    public abstract class Array
    {
        public int Length;
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
                throw new ArgumentException();

            return source.RemoveImpl(value);
        }

        protected virtual Delegate CombineImpl(Delegate follow)
        {
            if (!InternalEqualTypes(this, follow))
                throw new ArgumentException();

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
            if (Object.ReferenceEquals(first, second))
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

            if (!Object.ReferenceEquals(first.m_helperObject, second.m_helperObject) ||
                first.m_extraFunctionPointerOrData != second.m_extraFunctionPointerOrData ||
                first.m_functionPointer != second.m_functionPointer)
            {
                return false;
            }

            if (Object.ReferenceEquals(first.m_firstParameter, first))
                return Object.ReferenceEquals(second.m_firstParameter, second);

            return Object.ReferenceEquals(first.m_firstParameter, second.m_firstParameter);
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

        protected unsafe void InitializeClosedInstance(object firstParameter, IntPtr functionPointer)
        {
            m_firstParameter = firstParameter;
            m_functionPointer = functionPointer;
        }

        protected unsafe void InitializeClosedInstanceSlow(object firstParameter, IntPtr functionPointer)
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

        protected unsafe void InitializeClosedStaticThunk(object firstParameter, IntPtr functionPointer, IntPtr functionPointerThunk)
        {
            m_firstParameter = this;
            m_helperObject = firstParameter;
            m_extraFunctionPointerOrData = functionPointer;
            m_functionPointer = functionPointerThunk;
        }

        protected unsafe void InitializeOpenStaticThunk(object firstParameter, IntPtr functionPointer, IntPtr functionPointerThunk)
        {
            m_firstParameter = this;
            m_extraFunctionPointerOrData = functionPointer;
            m_functionPointer = functionPointerThunk;
        }

        protected unsafe void InitializeOpenInstanceThunkDynamic(IntPtr functionPointer, IntPtr functionPointerThunk)
        {
            m_firstParameter = this;
            m_extraFunctionPointerOrData = functionPointer;
            m_functionPointer = functionPointerThunk;
        }

        private unsafe void InitializeClosedInstanceToInterface(object firstParameter, IntPtr dispatchCell)
        {
            m_functionPointer = CachedInterfaceDispatch.RhpResolveInterfaceMethod(firstParameter, dispatchCell);
            m_firstParameter = firstParameter;
        }
    }

    public delegate void Action();

    public static class GC
    {
        [RuntimeImport("*", "GcCollect")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int Collect();

        [Intrinsic]
        public static void KeepAlive(object obj) { }
    }

    public abstract class MulticastDelegate : Delegate
    {
        public override bool Equals(object obj)
        {
            Delegate other = obj as Delegate;
            return other != null && EqualsImpl(this, other);
        }
    }

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

        public class StateMachineAttribute : Attribute
        {
            public StateMachineAttribute(Type stateMachineType) => StateMachineType = stateMachineType;

            public Type StateMachineType { get; }
        }

        public sealed class AsyncStateMachineAttribute : StateMachineAttribute
        {
            public AsyncStateMachineAttribute(Type stateMachineType) : base(stateMachineType) { }
        }

        public sealed class AsyncMethodBuilderAttribute : Attribute
        {
            public AsyncMethodBuilderAttribute(Type builderType) => BuilderType = builderType;

            public Type BuilderType { get; }
        }

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

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                Task outputTask = Task;
                AsyncStateMachineContinuation<TStateMachine> continuation = new AsyncStateMachineContinuation<TStateMachine>(stateMachine);
                awaiter.OnCompleted(continuation.Invoke);
            }

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                Task outputTask = Task;
                AsyncStateMachineContinuation<TStateMachine> continuation = new AsyncStateMachineContinuation<TStateMachine>(stateMachine);
                awaiter.UnsafeOnCompleted(continuation.Invoke);
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

            public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                Task<TResult> outputTask = Task;
                AsyncStateMachineContinuation<TStateMachine> continuation = new AsyncStateMachineContinuation<TStateMachine>(stateMachine);
                awaiter.OnCompleted(continuation.Invoke);
            }

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                Task<TResult> outputTask = Task;
                AsyncStateMachineContinuation<TStateMachine> continuation = new AsyncStateMachineContinuation<TStateMachine>(stateMachine);
                awaiter.UnsafeOnCompleted(continuation.Invoke);
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
                AsyncStateMachineContinuation<TStateMachine> continuation = new AsyncStateMachineContinuation<TStateMachine>(stateMachine);
                awaiter.OnCompleted(continuation.Invoke);
            }

            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                AsyncStateMachineContinuation<TStateMachine> continuation = new AsyncStateMachineContinuation<TStateMachine>(stateMachine);
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
    public interface IEnumerable<out T> : System.Collections.IEnumerable
    {
        new IEnumerator<T> GetEnumerator();
    }

    public interface IEnumerator<out T> : IDisposable, System.Collections.IEnumerator
    {
        new T Current { get; }
    }
}

namespace System.Threading.Tasks
{
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

    internal sealed class AsyncStateMachineContinuation<TStateMachine> : TaskContinuation
        where TStateMachine : IAsyncStateMachine
    {
        private TStateMachine _stateMachine;

        internal AsyncStateMachineContinuation(TStateMachine stateMachine) => _stateMachine = stateMachine;

        internal override void Invoke() => _stateMachine.MoveNext();
    }

    public partial class Task
    {
        private const int Pending = 0;
        private const int Completed = 1;
        private const int Faulted = 2;

        private int _state;
        private Exception _exception;
        private TaskContinuation _continuations;

        internal Task() { }

        public bool IsCompleted => _state != Pending;
        public bool IsCompletedSuccessfully => _state == Completed;
        public bool IsFaulted => _state == Faulted;
        public Exception Exception => _exception;

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

        [RuntimeImport("*", "TaskYield")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern bool Yield();

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

        internal void GetResult()
        {
            Wait();
        }

        protected void ThrowIfFaulted()
        {
            if (_state == Faulted)
                throw _exception;
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
            return base.TrySetResult();
        }

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

        [RuntimeImport("*", "MonitorEnter")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Enter(object obj, ref bool lockTaken);

        [RuntimeImport("*", "MonitorExit")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Exit(object obj);
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
        [RuntimeImport("*", "AllocCoTaskMem")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern IntPtr AllocCoTaskMem(int cb);

        [RuntimeImport("*", "FreeCoTaskMem")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void FreeCoTaskMem(IntPtr ptr);
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

namespace System.Runtime
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EHVector128
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

    internal static unsafe class EH
    {
        // A PE x64 RUNTIME_FUNCTION contains three 32-bit RVAs.
        private static int RuntimeFunctionSize => sizeof(RuntimeFunction);

        private static byte* s_imageBase;
        private static RuntimeFunction* s_exceptionTable;
        private static int s_runtimeFunctionCount;

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

        // The EFI startup code supplies raw addresses. Keeping the state here
        // lets the compiler runtime stay independent of EFI protocols and PE
        // header types.
        internal static void Initialize(
            byte* imageBase,
            byte* exceptionTable,
            uint exceptionTableSize)
        {
            s_imageBase = imageBase;
            s_exceptionTable = (RuntimeFunction*)exceptionTable;
            s_runtimeFunctionCount = (int)(exceptionTableSize / RuntimeFunctionSize);
        }

        // RhpThrowEx is the native entry point in ExceptionHandling.asm.
        // This managed method has the CoreRT name and performs the metadata
        // lookup before the assembly helper calls the selected funclet.
        [RuntimeExport("RhThrowEx")]
        internal static void RhThrowEx(object exception, ref ExInfo exInfo)
        {
            if (exception == null)
                exception = new Exception();

            if (s_imageBase != null &&
                s_exceptionTable != null &&
                s_runtimeFunctionCount > 0 &&
                TryFindHandler(exception, ref exInfo))
                return;

            Console.WriteLine("Unhandled exception: " + ((Exception)exception).Message);
            exInfo.Handler = null;
        }

        private static bool TryFindHandler(object exception, ref ExInfo exInfo)
        {
            RuntimeFunction* current = FindRuntimeFunction(
                s_exceptionTable,
                s_runtimeFunctionCount,
                s_imageBase,
                exInfo.ControlPC);
            if (current == null)
                return false;

            for (int depth = 0; depth < 64 && current != null; depth++)
            {
                RuntimeFunction* root = FindRootFunction(
                    s_exceptionTable,
                    current,
                    s_imageBase);
                if (root == null)
                    return false;

                if (TryFindTypedHandler(s_imageBase, root,
                    exception, ref exInfo))
                    return true;

                if (!UnwindFrame(s_imageBase, current, ref exInfo))
                    break;

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
            ref ExInfo exInfo)
        {
            byte* unwind = GetUnwindInfo(imageBase, root);
            byte* cursor = GetEhInfoCursor(unwind, imageBase);

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

                    if (codeOffset >= tryStart && codeOffset < tryEnd)
                    {
                        EEType* targetType = (EEType*)(imageBase + typeRva);
                        if (TypeCast.IsInstanceOfClass(targetType, exception) != null)
                        {
                            exInfo.Handler = methodStart + handlerOffset;
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
                    ReadVarUInt(ref cursor);
                    ReadVarUInt(ref cursor);
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private static byte* GetUnwindInfo(byte* imageBase, RuntimeFunction* function)
        {
            return imageBase + function->UnwindData;
        }

        private static byte GetFunctionBlockFlags(byte* unwind)
        {
            int codeSize = 4 + unwind[2] * 2;
            codeSize = (codeSize + 3) & ~3;
            if ((unwind[0] >> 3 & UnwindFlagHandlerMask) != 0)
                codeSize += sizeof(uint);

            return unwind[codeSize];
        }

        private static byte* GetEhInfoCursor(byte* unwind, byte* imageBase)
        {
            int codeSize = 4 + unwind[2] * 2;
            codeSize = (codeSize + 3) & ~3;
            if ((unwind[0] >> 3 & UnwindFlagHandlerMask) != 0)
                codeSize += sizeof(uint);

            byte* cursor = unwind + codeSize;
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
            exInfo.ControlPC = (byte*)returnAddress;
            exInfo.StackPointer = (byte*)stackPointer;
            return returnAddress != 0;
        }

        private static ulong GetRegister(ref ExInfo exInfo, byte register)
        {
            return register switch
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
        }

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
        internal static void __fail_fast()
        {
            for (; ; );
        }

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

        [RuntimeExport("RhpFallbackFailFast")]
        internal static void RhpFallbackFailFast()
        {
            for (; ; );
        }

        [RuntimeImport("*", "GcAllocate")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        static extern void* GcAllocate(ulong size);

        [RuntimeExport("RhpNewFast")]
        internal static object RhpNewFast(EEType* pEEType)
        {
            void* ptr = GcAllocate(pEEType->BaseSize);
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
            void* ptr = GcAllocate(size);
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
            void* ptr = GcAllocate(size);
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

            EEType* baseType = obj.EEType->RelatedType.BaseType;
            while (baseType != null)
            {
                if (pTargetType == baseType)
                    return obj;

                baseType = baseType->RelatedType.BaseType;
            }

            return null;
        }

        [RuntimeExport("RhTypeCast_CheckCastClass")]
        public static unsafe object CheckCastClass(EEType* pTargetEEType, object obj)
        {
            if (obj == null)
                return null;

            object result = IsInstanceOfClass(pTargetEEType, obj);

            if (result == null)
            {
                throw new InvalidCastException();
            }

            return result;
        }

        [RuntimeExport("RhTypeCast_IsInstanceOfArray")]
        public static unsafe object IsInstanceOfArray(EEType* pTargetType, object obj)
        {
            if (obj == null)
                return null;

            // The current runtime only needs exact array casts here. This is
            // sufficient for the Delegate[] invocation lists used by events.
            return obj.EEType == pTargetType ? obj : null;
        }

        [RuntimeExport("RhTypeCast_CheckCastArray")]
        public static unsafe object CheckCastArray(EEType* pTargetEEType, object obj)
        {
            object result = IsInstanceOfArray(pTargetEEType, obj);
            if (result == null && obj != null)
                throw new InvalidCastException();

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
            internal static ref T AddByteOffset<T>(ref T source, nuint byteOffset)
            {
                return ref AddByteOffset(ref source, (ulong)(void*)byteOffset);
            }

            [RuntimeImport("*", "memcpy")]
            [MethodImpl(MethodImplOptions.InternalCall)]
            public unsafe static extern void CopyBlock(void* destination, void* source, ulong byteCount);
        }
    }

    namespace CompilerHelpers
    {
        using Internal.Runtime.CompilerServices;
        using System.Runtime;

        internal static class SynchronizedMethodHelpers
        {
            private static readonly object s_staticLock = new object();

            private static void MonitorEnter(object obj, ref bool lockTaken)
            {
                System.Threading.Monitor.Enter(obj, ref lockTaken);
            }

            private static void MonitorExit(object obj, ref bool lockTaken)
            {
                if (!lockTaken)
                    return;

                System.Threading.Monitor.Exit(obj);
                lockTaken = false;
            }

            private static void MonitorEnterStatic(IntPtr pEEType, ref bool lockTaken)
            {
                System.Threading.Monitor.Enter(s_staticLock, ref lockTaken);
            }

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
            public static void ThrowInvalidProgramException(int id) => throw new InvalidProgramException();
            public static void ThrowInvalidProgramExceptionWithArgument(int id, string methodName) => throw new InvalidProgramException();
            public static void ThrowOverflowException() => throw new OverflowException();
            public static void ThrowIndexOutOfRangeException() => throw new IndexOutOfRangeException();
            public static void ThrowTypeLoadException(int id, string className, string typeName) => throw new TypeLoadException();
        }

        public static partial class StartupCodeHelpers
        {
            [RuntimeImport("*", "GcRegisterStatics")]
            [MethodImpl(MethodImplOptions.InternalCall)]
            internal static extern void RegisterStatics(IntPtr start, IntPtr end);

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
                            RegisterStatics(sections[k].Start, sections[k].End);
                            InitializeStatics(sections[k].Start, sections[k].End);
                        }

                        if (sections[k].SectionId == ReadyToRunSectionType.InterfaceDispatchTable)
                            CachedInterfaceDispatch.RegisterDispatchMaps(sections[k].Start);

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
