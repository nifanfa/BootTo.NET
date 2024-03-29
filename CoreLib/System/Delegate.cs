﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//using System.Text;
using System.Runtime;
using System.Reflection;
//using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
//using System.Diagnostics;
//using Internal.Reflection.Augments;
//using Internal.Runtime.Augments;
using Internal.Runtime.CompilerServices;

namespace System
{
    // WARNING: Bartok hard-codes offsets to the delegate fields, so it's notion of where fields are may 
    // differ from the runtime's notion.  Bartok honors sequential and explicit layout directives, so I've 
    // ordered the fields in such a way as to match the runtime's layout and then just tacked on this 
    // sequential layout directive so that Bartok matches it.
    [StructLayout(LayoutKind.Sequential)]
    //[DebuggerDisplay("Target method(s) = {GetTargetMethodsDescriptionForDebugger()}")]
    public abstract partial class Delegate // : ICloneable, ISerializable
    {
        //public virtual object Clone() => MemberwiseClone();

        //[return: NotNullIfNotNull("a")]
        //[return: NotNullIfNotNull("b")]
        public static Delegate? Combine(Delegate? a, Delegate? b)
        {
            if (a is null)
                return b;

            return a.CombineImpl(b);
        }

        public static Delegate? Combine(params Delegate?[]? delegates)
        {
            if (delegates == null || delegates.Length == 0)
                return null;

            Delegate? d = delegates[0];
            for (int i = 1; i < delegates.Length; i++)
                d = Combine(d, delegates[i]);

            return d;
        }

        // V2 api: Creates open or closed delegates to static or instance methods - relaxed signature checking allowed.
        //public static Delegate CreateDelegate(Type type, object? firstArgument, MethodInfo method) => CreateDelegate(type, firstArgument, method, throwOnBindFailure: true)!;

        // V1 api: Creates open delegates to static or instance methods - relaxed signature checking allowed.
        //public static Delegate CreateDelegate(Type type, MethodInfo method) => CreateDelegate(type, method, throwOnBindFailure: true)!;

        // V1 api: Creates closed delegates to instance methods only, relaxed signature checking disallowed.
        //public static Delegate CreateDelegate(Type type, object target, string method) => CreateDelegate(type, target, method, ignoreCase: false, throwOnBindFailure: true)!;
        //public static Delegate CreateDelegate(Type type, object target, string method, bool ignoreCase) => CreateDelegate(type, target, method, ignoreCase, throwOnBindFailure: true)!;

        // V1 api: Creates open delegates to static methods only, relaxed signature checking disallowed.
        //public static Delegate CreateDelegate(Type type, Type target, string method) => CreateDelegate(type, target, method, ignoreCase: false, throwOnBindFailure: true)!;
        //public static Delegate CreateDelegate(Type type, Type target, string method, bool ignoreCase) => CreateDelegate(type, target, method, ignoreCase, throwOnBindFailure: true)!;

#if !CORERT
        //protected virtual Delegate CombineImpl(Delegate? d) => null;

        //protected virtual Delegate? RemoveImpl(Delegate d) => d.Equals(this) ? null : this;

        //public virtual Delegate[] GetInvocationList() => new Delegate[] { this };

        //public object? DynamicInvoke(params object?[]? args)
        //{
        //    return DynamicInvokeImpl(args);
        //}
#endif

        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context) => throw new PlatformNotSupportedException();

        //public MethodInfo Method => GetMethodImpl();

        public static Delegate? Remove(Delegate? source, Delegate? value)
        {
            if (source == null)
                return null;

            if (value == null)
                return source;

            if (!InternalEqualTypes(source, value))
                return null;
            //throw new ArgumentException(SR.Arg_DlgtTypeMis);

            return source.RemoveImpl(value);
        }

        public static Delegate? RemoveAll(Delegate? source, Delegate? value)
        {
            Delegate? newDelegate;

            do
            {
                newDelegate = source;
                source = Remove(source, value);
            }
            while (newDelegate != source);

            return newDelegate;
        }

        // Force inline as the true/false ternary takes it above ALWAYS_INLINE size even though the asm ends up smaller
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Delegate? d1, Delegate? d2)
        {
            // Test d2 first to allow branch elimination when inlined for null checks (== null)
            // so it can become a simple test
            if (d2 is null)
            {
                // return true/false not the test result https://github.com/dotnet/runtime/issues/4207
                return (d1 is null) ? true : false;
            }

            return /*ReferenceEquals(d2, d1) ? true : */d2.Equals((object?)d1);
        }

        // Force inline as the true/false ternary takes it above ALWAYS_INLINE size even though the asm ends up smaller
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Delegate? d1, Delegate? d2)
        {
            // Test d2 first to allow branch elimination when inlined for not null checks (!= null)
            // so it can become a simple test
            if (d2 is null)
            {
                // return true/false not the test result https://github.com/dotnet/runtime/issues/4207
                return (d1 is null) ? false : true;
            }

            return /*ReferenceEquals(d2, d1) ? false : */!d2.Equals(d1);
        }

        // V1 API: Create closed instance delegates. Method name matching is case sensitive.
        protected Delegate(object target, string method)
        {
            // This constructor cannot be used by application code. To create a delegate by specifying the name of a method, an
            // overload of the public static CreateDelegate method is used. This will eventually end up calling into the internal
            // implementation of CreateDelegate below, and does not invoke this constructor.
            // The constructor is just for API compatibility with the public contract of the Delegate class.
            //throw new PlatformNotSupportedException();
        }

        /*
        // V1 API: Create open static delegates. Method name matching is case insensitive.
        protected Delegate(Type target, string method)
        {
            // This constructor cannot be used by application code. To create a delegate by specifying the name of a method, an
            // overload of the public static CreateDelegate method is used. This will eventually end up calling into the internal
            // implementation of CreateDelegate below, and does not invoke this constructor.
            // The constructor is just for API compatibility with the public contract of the Delegate class.
            throw new PlatformNotSupportedException();
        }
        */

        // New Delegate Implementation

        protected internal object m_firstParameter;
        protected internal object m_helperObject;
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
        protected internal IntPtr m_extraFunctionPointerOrData;
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
        protected internal IntPtr m_functionPointer;

        //[ThreadStatic]
        protected static string s_DefaultValueString;

        // WARNING: These constants are also declared in System.Private.TypeLoader\Internal\Runtime\TypeLoader\CallConverterThunk.cs
        // Do not change their values without updating the values in the calling convention converter component
        protected const int MulticastThunk = 0;
        protected const int ClosedStaticThunk = 1;
        protected const int OpenStaticThunk = 2;
        protected const int ClosedInstanceThunkOverGenericMethod = 3; // This may not exist
        protected const int DelegateInvokeThunk = 4;
        protected const int OpenInstanceThunk = 5;        // This may not exist
        protected const int ObjectArrayThunk = 6;         // This may not exist

        //
        // If the thunk does not exist, the function will return IntPtr.Zero.
        protected virtual IntPtr GetThunk(int whichThunk)
        {
#if PROJECTN
            // The GetThunk function should be overriden on all delegate types, except for universal
            // canonical delegates which use calling convention converter thunks to marshal arguments
            // for the delegate call. If we execute this version of GetThunk, we can at least assert
            // that the current delegate type is a generic type.
            Debug.Assert(this.EETypePtr.IsGeneric);
            return TypeLoaderExports.GetDelegateThunk(this, whichThunk);
#else
            // CoreRT doesn't support Universal Shared Code right now, so let's make this method return null for now.
            // When CoreRT adds USG support we'll probably want to do some level of IL switching here so that
            // we don't have this static call into type loader when USG is not enabled at compile time.
            // The static call hurts size in our minimal targets.
            return IntPtr.Zero;
#endif
        }

        //
        // If there is a default value string, the overridden function should set the 
        // s_DefaultValueString field and return true.
        protected virtual bool LoadDefaultValueString() { return false; }

        /// <summary>
        /// Used by various parts of the runtime as a replacement for Delegate.Method
        /// 
        /// The Interop layer uses this to distinguish between different methods on a 
        /// single type, and to get the function pointer for delegates to static functions
        /// 
        /// The reflection apis use this api to figure out what MethodInfo is related
        /// to a delegate.
        /// 
        /// </summary>
        /// <param name="typeOfFirstParameterIfInstanceDelegate"> 
        ///   This value indicates which type an delegate's function pointer is associated with
        ///   This value is ONLY set for delegates where the function pointer points at an instance method
        /// </param>
        /// <param name="isOpenResolver"> 
        ///   This value indicates if the returned pointer is an open resolver structure.
        /// </param>
        /// <param name="isInterpreterEntrypoint"> 
        ///   Delegate points to an object array thunk (the delegate wraps a Func<object[], object> delegate). This
        ///   is typically a delegate pointing to the LINQ expression interpreter.
        /// </param>
        /// <returns></returns>
        unsafe internal IntPtr GetFunctionPointer(out RuntimeTypeHandle typeOfFirstParameterIfInstanceDelegate, out bool isOpenResolver, out bool isInterpreterEntrypoint)
        {
            typeOfFirstParameterIfInstanceDelegate = default(RuntimeTypeHandle);
            isOpenResolver = false;
            isInterpreterEntrypoint = false;

            if (GetThunk(MulticastThunk) == m_functionPointer)
            {
                return IntPtr.Zero;
            }
            else if (GetThunk(ObjectArrayThunk) == m_functionPointer)
            {
                isInterpreterEntrypoint = true;
                return IntPtr.Zero;
            }
            else if (m_extraFunctionPointerOrData != IntPtr.Zero)
            {
                if (GetThunk(OpenInstanceThunk) == m_functionPointer)
                {
                    typeOfFirstParameterIfInstanceDelegate = ((OpenMethodResolver*)m_extraFunctionPointerOrData)->DeclaringType;
                    isOpenResolver = true;
                }
                return m_extraFunctionPointerOrData;
            }
            else
            {
                if (m_firstParameter != null)
                    typeOfFirstParameterIfInstanceDelegate = new RuntimeTypeHandle(m_firstParameter.EETypePtr);

                // TODO! Implementation issue for generic invokes here ... we need another IntPtr for uniqueness.

                return m_functionPointer;
            }
        }

        // This function is known to the IL Transformer.
        protected void InitializeClosedInstance(object firstParameter, IntPtr functionPointer)
        {
            if (firstParameter == null)
                return;

            m_functionPointer = functionPointer;
            m_firstParameter = firstParameter;
        }

        // This function is known to the IL Transformer.
        protected void InitializeClosedInstanceSlow(object firstParameter, IntPtr functionPointer)
        {
            // This method is like InitializeClosedInstance, but it handles ALL cases. In particular, it handles generic method with fun function pointers.

            if (firstParameter == null)
                return;

            if (!FunctionPointerOps.IsGenericMethodPointer(functionPointer))
            {
                m_functionPointer = functionPointer;
                m_firstParameter = firstParameter;
            }
            else
            {
                m_firstParameter = this;
                m_functionPointer = GetThunk(ClosedInstanceThunkOverGenericMethod);
                m_extraFunctionPointerOrData = functionPointer;
                m_helperObject = firstParameter;
            }
        }

        /*
        // This function is known to the compiler.
        protected void InitializeClosedInstanceWithGVMResolution(object firstParameter, RuntimeMethodHandle tokenOfGenericVirtualMethod)
        {
            if (firstParameter == null)
                throw new ArgumentException(SR.Arg_DlgtNullInst);

            IntPtr functionResolution = TypeLoaderExports.GVMLookupForSlot(firstParameter, tokenOfGenericVirtualMethod);

            if (functionResolution == IntPtr.Zero)
            {
                // TODO! What to do when GVM resolution fails. Should never happen
                throw new InvalidOperationException();
            }
            if (!FunctionPointerOps.IsGenericMethodPointer(functionResolution))
            {
                m_functionPointer = functionResolution;
                m_firstParameter = firstParameter;
            }
            else
            {
                m_firstParameter = this;
                m_functionPointer = GetThunk(ClosedInstanceThunkOverGenericMethod);
                m_extraFunctionPointerOrData = functionResolution;
                m_helperObject = firstParameter;
            }

            return;
        }
        */

        /*
        private void InitializeClosedInstanceToInterface(object firstParameter, IntPtr dispatchCell)
        {
            if (firstParameter == null)
                throw new ArgumentException(SR.Arg_DlgtNullInst);

            m_functionPointer = RuntimeImports.RhpResolveInterfaceMethod(firstParameter, dispatchCell);
            m_firstParameter = firstParameter;
        }
        */

        // This is used to implement MethodInfo.CreateDelegate() in a desktop-compatible way. Yes, the desktop really
        // let you use that api to invoke an instance method with a null 'this'.
        private void InitializeClosedInstanceWithoutNullCheck(object firstParameter, IntPtr functionPointer)
        {
            if (!FunctionPointerOps.IsGenericMethodPointer(functionPointer))
            {
                m_functionPointer = functionPointer;
                m_firstParameter = firstParameter;
            }
            else
            {
                m_firstParameter = this;
                m_functionPointer = GetThunk(ClosedInstanceThunkOverGenericMethod);
                m_extraFunctionPointerOrData = functionPointer;
                m_helperObject = firstParameter;
            }
        }

        // This function is known to the compiler backend.
        protected void InitializeClosedStaticThunk(object firstParameter, IntPtr functionPointer, IntPtr functionPointerThunk)
        {
            m_extraFunctionPointerOrData = functionPointer;
            m_helperObject = firstParameter;
            m_functionPointer = functionPointerThunk;
            m_firstParameter = this;
        }

        // This function is known to the compiler backend.
        protected void InitializeOpenStaticThunk(object firstParameter, IntPtr functionPointer, IntPtr functionPointerThunk)
        {
            // This sort of delegate is invoked by calling the thunk function pointer with the arguments to the delegate + a reference to the delegate object itself.
            m_firstParameter = this;
            m_functionPointer = functionPointerThunk;
            m_extraFunctionPointerOrData = functionPointer;
        }

        protected void InitializeOpenInstanceThunkDynamic(IntPtr functionPointer, IntPtr functionPointerThunk)
        {
            // This sort of delegate is invoked by calling the thunk function pointer with the arguments to the delegate + a reference to the delegate object itself.
            m_firstParameter = this;
            m_functionPointer = functionPointerThunk;
            m_extraFunctionPointerOrData = functionPointer;
        }

        internal void SetClosedStaticFirstParameter(object firstParameter)
        {
            // Closed static delegates place a value in m_helperObject that they pass to the target method.
            //Debug.Assert(m_functionPointer == GetThunk(ClosedStaticThunk));
            m_helperObject = firstParameter;
        }

        /*
        // This function is only ever called by the open instance method thunk, and in that case,
        // m_extraFunctionPointerOrData always points to an OpenMethodResolver
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected IntPtr GetActualTargetFunctionPointer(object thisObject)
        {
            return OpenMethodResolver.ResolveMethod(m_extraFunctionPointerOrData, thisObject);
        }
        */

        /*
        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }
        */

        private bool IsDynamicDelegate()
        {
            if (this.GetThunk(MulticastThunk) == IntPtr.Zero)
            {
                return true;
            }

            return false;
        }

        /*
        [DebuggerGuidedStepThroughAttribute]
        protected virtual object DynamicInvokeImpl(object[] args)
        {
            if (IsDynamicDelegate())
            {
                // DynamicDelegate case
                object result = ((Func<object[], object>)m_helperObject)(args);
                DebugAnnotations.PreviousCallContainsDebuggerStepInCode();
                return result;
            }
            else
            {
                IntPtr invokeThunk = this.GetThunk(DelegateInvokeThunk);

                IntPtr genericDictionary = IntPtr.Zero;
                if (FunctionPointerOps.IsGenericMethodPointer(invokeThunk))
                {
                    unsafe
                    {
                        GenericMethodDescriptor* descriptor = FunctionPointerOps.ConvertToGenericDescriptor(invokeThunk);
                        genericDictionary = descriptor->InstantiationArgument;
                        invokeThunk = descriptor->MethodFunctionPointer;
                    }
                }

                object result = InvokeUtils.CallDynamicInvokeMethod(this.m_firstParameter, this.m_functionPointer, null, invokeThunk, genericDictionary, this, args, binderBundle: null, wrapInTargetInvocationException: true, invokeMethodHelperIsThisCall: false);
                DebugAnnotations.PreviousCallContainsDebuggerStepInCode();
                return result;
            }
        }
        */

        /*
        [DebuggerGuidedStepThroughAttribute]
        public object DynamicInvoke(params object[] args)
        {
            object result = DynamicInvokeImpl(args);
            DebugAnnotations.PreviousCallContainsDebuggerStepInCode();
            return result;
        }
        */

        private MulticastDelegate NewMulticastDelegate(Delegate[] invocationList, int invocationCount, bool thisIsMultiCastAlready)
        {
            // First, allocate a new multicast delegate just like this one, i.e. same type as the this object
            MulticastDelegate result = (MulticastDelegate)RuntimeImports.RhNewObject(this.EETypePtr);

            // Performance optimization - if this already points to a true multicast delegate,
            // copy _methodPtr and _methodPtrAux fields rather than calling into the EE to get them
            if (thisIsMultiCastAlready)
            {
                result.m_functionPointer = this.m_functionPointer;
            }
            else
            {
                result.m_functionPointer = GetThunk(MulticastThunk);
            }
            result.m_firstParameter = result;
            result.m_helperObject = invocationList;
            result.m_extraFunctionPointerOrData = (IntPtr)invocationCount;

            return result;
        }

        internal MulticastDelegate NewMulticastDelegate(Delegate[] invocationList, int invocationCount)
        {
            return NewMulticastDelegate(invocationList, invocationCount, false);
        }

        private bool TrySetSlot(Delegate[] a, int index, Delegate o)
        {
            if (a[index] == null /* && System.Threading.Interlocked.CompareExchange<Delegate>(ref a[index], o, null) == null */)
                return true;

            // The slot may be already set because we have added and removed the same method before.
            // Optimize this case, because it's cheaper than copying the array.
            if (a[index] != null)
            {
                MulticastDelegate d = (MulticastDelegate)o;
                MulticastDelegate dd = (MulticastDelegate)a[index];

                if ((dd.m_firstParameter == d.m_firstParameter) &&
                    (dd.m_helperObject == d.m_helperObject) &&
                    dd.m_extraFunctionPointerOrData == d.m_extraFunctionPointerOrData &&
                    dd.m_functionPointer == d.m_functionPointer)
                {
                    return true;
                }
            }
            return false;
        }


        // This method will combine this delegate with the passed delegate
        //  to form a new delegate.
        protected virtual Delegate CombineImpl(Delegate d)
        {
            if ((object)d == null) // cast to object for a more efficient test
                return this;

            // Verify that the types are the same...
            if (!InternalEqualTypes(this, d))
                return null;

            if (IsDynamicDelegate() && d.IsDynamicDelegate())
            {
                return null;
            }

            MulticastDelegate dFollow = (MulticastDelegate)d;
            Delegate[] resultList;
            int followCount = 1;
            Delegate[] followList = dFollow.m_helperObject as Delegate[];
            if (followList != null)
                followCount = (int)dFollow.m_extraFunctionPointerOrData;

            int resultCount;
            Delegate[] invocationList = m_helperObject as Delegate[];
            if (invocationList == null)
            {
                resultCount = 1 + followCount;
                resultList = new Delegate[resultCount];
                resultList[0] = this;
                if (followList == null)
                {
                    resultList[1] = dFollow;
                }
                else
                {
                    for (int i = 0; i < followCount; i++)
                        resultList[1 + i] = followList[i];
                }
                return NewMulticastDelegate(resultList, resultCount);
            }
            else
            {
                int invocationCount = (int)m_extraFunctionPointerOrData;
                resultCount = invocationCount + followCount;
                resultList = null;
                if (resultCount <= invocationList.Length)
                {
                    resultList = invocationList;
                    if (followList == null)
                    {
                        if (!TrySetSlot(resultList, invocationCount, dFollow))
                            resultList = null;
                    }
                    else
                    {
                        for (int i = 0; i < followCount; i++)
                        {
                            if (!TrySetSlot(resultList, invocationCount + i, followList[i]))
                            {
                                resultList = null;
                                break;
                            }
                        }
                    }
                }

                if (resultList == null)
                {
                    int allocCount = invocationList.Length;
                    while (allocCount < resultCount)
                        allocCount *= 2;

                    resultList = new Delegate[allocCount];

                    for (int i = 0; i < invocationCount; i++)
                        resultList[i] = invocationList[i];

                    if (followList == null)
                    {
                        resultList[invocationCount] = dFollow;
                    }
                    else
                    {
                        for (int i = 0; i < followCount; i++)
                            resultList[invocationCount + i] = followList[i];
                    }
                }
                return NewMulticastDelegate(resultList, resultCount, true);
            }
        }

        private Delegate[] DeleteFromInvocationList(Delegate[] invocationList, int invocationCount, int deleteIndex, int deleteCount)
        {
            Delegate[] thisInvocationList = m_helperObject as Delegate[];
            int allocCount = thisInvocationList.Length;
            while (allocCount / 2 >= invocationCount - deleteCount)
                allocCount /= 2;

            Delegate[] newInvocationList = new Delegate[allocCount];

            for (int i = 0; i < deleteIndex; i++)
                newInvocationList[i] = invocationList[i];

            for (int i = deleteIndex + deleteCount; i < invocationCount; i++)
                newInvocationList[i - deleteCount] = invocationList[i];

            return newInvocationList;
        }

        private bool EqualInvocationLists(Delegate[] a, Delegate[] b, int start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!(a[start + i].Equals(b[i])))
                    return false;
            }
            return true;
        }

        // This method currently looks backward on the invocation list
        //  for an element that has Delegate based equality with value.  (Doesn't
        //  look at the invocation list.)  If this is found we remove it from
        //  this list and return a new delegate.  If its not found a copy of the
        //  current list is returned.
        protected virtual Delegate RemoveImpl(Delegate d)
        {
            // There is a special case were we are removing using a delegate as
            //    the value we need to check for this case
            //
            MulticastDelegate v = d as MulticastDelegate;

            if (v == null)
                return this;
            if (v.m_helperObject as Delegate[] == null)
            {
                Delegate[] invocationList = m_helperObject as Delegate[];
                if (invocationList == null)
                {
                    // they are both not real Multicast
                    if (this.Equals(d))
                        return null;
                }
                else
                {
                    int invocationCount = (int)m_extraFunctionPointerOrData;
                    for (int i = invocationCount; --i >= 0;)
                    {
                        if (d.Equals(invocationList[i]))
                        {
                            if (invocationCount == 2)
                            {
                                // Special case - only one value left, either at the beginning or the end
                                return invocationList[1 - i];
                            }
                            else
                            {
                                Delegate[] list = DeleteFromInvocationList(invocationList, invocationCount, i, 1);
                                return NewMulticastDelegate(list, invocationCount - 1, true);
                            }
                        }
                    }
                }
            }
            else
            {
                Delegate[] invocationList = m_helperObject as Delegate[];
                if (invocationList != null)
                {
                    int invocationCount = (int)m_extraFunctionPointerOrData;
                    int vInvocationCount = (int)v.m_extraFunctionPointerOrData;
                    for (int i = invocationCount - vInvocationCount; i >= 0; i--)
                    {
                        if (EqualInvocationLists(invocationList, v.m_helperObject as Delegate[], i, vInvocationCount))
                        {
                            if (invocationCount - vInvocationCount == 0)
                            {
                                // Special case - no values left
                                return null;
                            }
                            else if (invocationCount - vInvocationCount == 1)
                            {
                                // Special case - only one value left, either at the beginning or the end
                                return invocationList[i != 0 ? 0 : invocationCount - 1];
                            }
                            else
                            {
                                Delegate[] list = DeleteFromInvocationList(invocationList, invocationCount, i, vInvocationCount);
                                return NewMulticastDelegate(list, invocationCount - vInvocationCount, true);
                            }
                        }
                    }
                }
            }

            return this;
        }

        public virtual Delegate[] GetInvocationList()
        {
            Delegate[] del;
            Delegate[] invocationList = m_helperObject as Delegate[];
            if (invocationList == null)
            {
                del = new Delegate[1];
                del[0] = this;
            }
            else
            {
                // Create an array of delegate copies and each
                //    element into the array
                int invocationCount = (int)m_extraFunctionPointerOrData;
                del = new Delegate[invocationCount];

                for (int i = 0; i < del.Length; i++)
                    del[i] = invocationList[i];
            }
            return del;
        }

        /*
        protected virtual MethodInfo GetMethodImpl()
        {
            return RuntimeAugments.Callbacks.GetDelegateMethod(this);
        }
        */

        public override bool Equals(object obj)
        {
            // It is expected that all real uses of the Equals method will hit the MulticastDelegate.Equals logic instead of this
            // therefore, instead of duplicating the desktop behavior where direct calls to this Equals function do not behave
            // correctly, we'll just throw here.
            //throw new PlatformNotSupportedException();
            return false;
        }

        public object Target
        {
            get
            {
                // Multi-cast delegates return the Target of the last delegate in the list
                if (m_functionPointer == GetThunk(MulticastThunk))
                {
                    Delegate[] invocationList = (Delegate[])m_helperObject;
                    int invocationCount = (int)m_extraFunctionPointerOrData;
                    return invocationList[invocationCount - 1].Target;
                }

                // Closed static delegates place a value in m_helperObject that they pass to the target method.
                if (m_functionPointer == GetThunk(ClosedStaticThunk) ||
                    m_functionPointer == GetThunk(ClosedInstanceThunkOverGenericMethod) ||
                    m_functionPointer == GetThunk(ObjectArrayThunk))
                    return m_helperObject;

                // Other non-closed thunks can be identified as the m_firstParameter field points at this.
                if ((m_firstParameter == this))
                {
                    return null;
                }

                // Closed instance delegates place a value in m_firstParameter, and we've ruled out all other types of delegates
                return m_firstParameter;
            }
        }

        // V2 api: Creates open or closed delegates to static or instance methods - relaxed signature checking allowed. 
        //public static Delegate CreateDelegate(Type type, object firstArgument, MethodInfo method, bool throwOnBindFailure) => ReflectionAugments.ReflectionCoreCallbacks.CreateDelegate(type, firstArgument, method, throwOnBindFailure);

        // V1 api: Creates open delegates to static or instance methods - relaxed signature checking allowed.
        //public static Delegate CreateDelegate(Type type, MethodInfo method, bool throwOnBindFailure) => ReflectionAugments.ReflectionCoreCallbacks.CreateDelegate(type, method, throwOnBindFailure);

        // V1 api: Creates closed delegates to instance methods only, relaxed signature checking disallowed.
        //public static Delegate CreateDelegate(Type type, object target, string method, bool ignoreCase, bool throwOnBindFailure) => ReflectionAugments.ReflectionCoreCallbacks.CreateDelegate(type, target, method, ignoreCase, throwOnBindFailure);

        // V1 api: Creates open delegates to static methods only, relaxed signature checking disallowed.
        //public static Delegate CreateDelegate(Type type, Type target, string method, bool ignoreCase, bool throwOnBindFailure) => ReflectionAugments.ReflectionCoreCallbacks.CreateDelegate(type, target, method, ignoreCase, throwOnBindFailure);

        internal bool IsOpenStatic
        {
            get
            {
                return GetThunk(OpenStaticThunk) == m_functionPointer;
            }
        }

        internal static bool InternalEqualTypes(object a, object b)
        {
            return a.EETypePtr == b.EETypePtr;
        }

        // Returns a new delegate of the specified type whose implementation is provied by the
        // provided delegate.
        /*
        internal static Delegate CreateObjectArrayDelegate(Type t, Func<object[], object> handler)
        {
            EETypePtr delegateEEType;
            if (!t.TryGetEEType(out delegateEEType))
            {
                throw new InvalidOperationException();
            }

            if (!delegateEEType.IsDefType || delegateEEType.IsGenericTypeDefinition)
            {
                throw new InvalidOperationException();
            }

            Delegate del = (Delegate)(RuntimeImports.RhNewObject(delegateEEType));

            IntPtr objArrayThunk = del.GetThunk(Delegate.ObjectArrayThunk);
            if (objArrayThunk == IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }

            del.m_helperObject = handler;
            del.m_functionPointer = objArrayThunk;
            del.m_firstParameter = del;
            return del;
        }
        */

        //
        // Internal (and quite unsafe) helper to create delegates of an arbitrary type. This is used to support Reflection invoke.
        //
        // Note that delegates constructed the normal way do not come through here. The IL transformer generates the equivalent of
        // this code customized for each delegate type.
        //
        internal static Delegate CreateDelegate(EETypePtr delegateEEType, IntPtr ldftnResult, object thisObject, bool isStatic, bool isOpen)
        {
            Delegate del = (Delegate)(RuntimeImports.RhNewObject(delegateEEType));

            // What? No constructor call? That's right, and it's not an oversight. All "construction" work happens in 
            // the Initialize() methods. This helper has a hard dependency on this invariant.

            if (isStatic)
            {
                if (isOpen)
                {
                    IntPtr thunk = del.GetThunk(Delegate.OpenStaticThunk);
                    del.InitializeOpenStaticThunk(null, ldftnResult, thunk);
                }
                else
                {
                    IntPtr thunk = del.GetThunk(Delegate.ClosedStaticThunk);
                    del.InitializeClosedStaticThunk(thisObject, ldftnResult, thunk);
                }
            }
            else
            {
                if (isOpen)
                {
                    IntPtr thunk = del.GetThunk(Delegate.OpenInstanceThunk);
                    del.InitializeOpenInstanceThunkDynamic(ldftnResult, thunk);
                }
                else
                {
                    del.InitializeClosedInstanceWithoutNullCheck(thisObject, ldftnResult);
                }
            }
            return del;
        }

        /*
        private string GetTargetMethodsDescriptionForDebugger()
        {
            if (m_functionPointer == GetThunk(MulticastThunk))
            {
                // Multi-cast delegates return the Target of the last delegate in the list
                Delegate[] invocationList = (Delegate[])m_helperObject;
                int invocationCount = (int)m_extraFunctionPointerOrData;
                StringBuilder builder = new StringBuilder();
                for (int c = 0; c < invocationCount; c++)
                {
                    if (c != 0)
                        builder.Append(", ");

                    builder.Append(invocationList[c].GetTargetMethodsDescriptionForDebugger());
                }

                return builder.ToString();
            }
            else
            {
                RuntimeTypeHandle typeOfFirstParameterIfInstanceDelegate;
                IntPtr functionPointer = GetFunctionPointer(out typeOfFirstParameterIfInstanceDelegate, out bool _, out bool _);
                if (!FunctionPointerOps.IsGenericMethodPointer(functionPointer))
                {
                    return DebuggerFunctionPointerFormattingHook(functionPointer, typeOfFirstParameterIfInstanceDelegate);
                }
                else
                {
                    unsafe
                    {
                        GenericMethodDescriptor* pointerDef = FunctionPointerOps.ConvertToGenericDescriptor(functionPointer);
                        return DebuggerFunctionPointerFormattingHook(pointerDef->InstantiationArgument, typeOfFirstParameterIfInstanceDelegate);
                    }
                }
            }
        }
        */

        private static string DebuggerFunctionPointerFormattingHook(IntPtr functionPointer, RuntimeTypeHandle typeOfFirstParameterIfInstanceDelegate)
        {
            // This method will be hooked by the debugger and the debugger will cause it to return a description for the function pointer
            //throw new NotSupportedException();
            return null;
        }
    }
}