#pragma warning disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class LanguageFeatureValidation
{
    private const int ExpectedSum = 15;
    internal const string FeatureName = "LanguageFeatureValidation";
    private static readonly object s_lock = new object();
    private static volatile int s_volatileValue;
    private static volatile int s_filterMode;
    private static volatile int s_expectedSum;
    private static volatile int s_runtimeBias = 1;
    private static int s_instructionStatic;
    private static readonly bool s_boolean = true;
    private static readonly char s_character = 'L';
    private static readonly sbyte s_sbyte = -1;
    private static readonly byte s_byte = 1;
    private static readonly short s_short = 2;
    private static readonly ushort s_ushort = 3;
    private static readonly long s_long = 4;
    private static readonly ulong s_ulong = 5;
    private static readonly float s_single = 6;
    private static readonly double s_double = 7;
    private static readonly string s_string = "features";

    private enum FeatureKind : byte
    {
        None,
        Value,
    }

    [Flags]
    private enum FeatureFlags : ushort
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4,
    }

    private enum SignedFeatureKind : int
    {
        Negative = -1,
        Positive = 2,
    }

    private interface IFeatureValue
    {
        int Value { get; }
    }

    private interface IExplicitFeature
    {
        int Add(int value);
    }

    private interface IGenericFeature<T>
    {
        T GetValue();
    }

    private interface IBoxingMutation
    {
        int Increment();
    }

    private interface IHierarchyFeature
    {
        int EvaluateHierarchy();
    }

    private interface ISecondaryFeature
    {
        int SecondaryValue { get; }
    }

    private readonly struct FeatureValue : IFeatureValue
    {
        private readonly int _value;

        public FeatureValue(int value)
        {
            _value = value;
        }

        public int Value => _value;

        public static implicit operator FeatureValue(int value) => new FeatureValue(value);
        public static explicit operator int(FeatureValue value) => value._value;
        public static FeatureValue operator +(FeatureValue left, FeatureValue right)
            => new FeatureValue(left._value + right._value);
    }

    private struct BoxingValue : IFeatureValue, IBoxingMutation
    {
        public int Number;
        public short Offset;
        public object Reference;

        public int Value => Number + Offset;

        public int Increment() => ++Number;
    }

    private struct PointerValues
    {
        public int Integer;
        public short Short;
    }

    private struct NativeHandlePair
    {
        public IntPtr Signed;
        public UIntPtr Unsigned;
        public int Tag;

        public NativeHandlePair(IntPtr signed, UIntPtr unsigned, int tag)
        {
            Signed = signed;
            Unsigned = unsigned;
            Tag = tag;
        }
    }

    private struct Coordinate
    {
        public int X;
        public int Y;

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int Sum() => X + Y;
    }

    private struct NestedValue
    {
        public Coordinate Coordinate;
        public FeatureValue Feature;
        public long Wide;
        public byte Tag;

        public NestedValue(Coordinate coordinate, FeatureValue feature, long wide, byte tag)
        {
            Coordinate = coordinate;
            Feature = feature;
            Wide = wide;
            Tag = tag;
        }

        public int Total => Coordinate.Sum() + Feature.Value + (int)Wide + Tag;
    }

    private struct GenericPair<TFirst, TSecond>
    {
        public TFirst First;
        public TSecond Second;

        public GenericPair(TFirst first, TSecond second)
        {
            First = first;
            Second = second;
        }
    }

    private abstract class FeatureBase
    {
        protected FeatureBase(int seed)
        {
            Seed = seed;
        }

        protected int Seed { get; }

        public virtual int Evaluate() => Seed;
    }

    private sealed class FeatureObject : FeatureBase, IFeatureValue
    {
        public FeatureObject(int seed) : base(seed) { }

        public event Action Changed;

        public int Value => Evaluate();

        public override int Evaluate() => base.Evaluate() + 1;

        public void RaiseChanged() => Changed?.Invoke();
    }

    private sealed class HiddenFeature : FeatureBase
    {
        public HiddenFeature(int seed) : base(seed) { }

        public new int Evaluate() => 100;
    }

    private sealed class StaticHandleFeature
    {
        public StaticHandleFeature(int value) => Value = value;
        public int Value { get; }
    }

    private struct StaticFieldValue
    {
        public int Number;
        public object Reference;
    }

    private static class ExplicitStaticInitialization
    {
        private static int s_phase;

        public static readonly int First = InitializeFirst();
        public static readonly StaticHandleFeature Reference = InitializeReference();
        public static readonly int[] Values = InitializeValues();
        public static int ConstructorRuns;
        public static int ConstructorPhase;

        static ExplicitStaticInitialization()
        {
            ConstructorRuns++;
            ConstructorPhase = ++s_phase;
        }

        private static int InitializeFirst()
        {
            s_phase++;
            return 41;
        }

        private static StaticHandleFeature InitializeReference()
        {
            s_phase++;
            return new StaticHandleFeature(42);
        }

        private static int[] InitializeValues()
        {
            s_phase++;
            return new int[] { 43, 44 };
        }
    }

    private static class GenericStaticStorage<T>
    {
        public static readonly object[] Handles = new object[2];
        public static readonly Type StoredType = typeof(T);
        public static int ConstructorRuns;

        static GenericStaticStorage()
        {
            ConstructorRuns++;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct StaticCallbackEvent
    {
        public void* NativeEvent;
    }

    private static object s_defaultStaticReference;
    private static bool s_defaultStaticBoolean;
    private static int s_defaultStaticInteger;
    private static long s_defaultStaticLong;
    private static double s_defaultStaticDouble;
    private static StaticFieldValue s_defaultStaticValue;
    private static int[] s_defaultStaticArray;
    private static IntPtr s_defaultStaticIntPtr;
    private static UIntPtr s_defaultStaticUIntPtr;
    private static readonly object[] s_staticHandleValues = new object[2];
    private static int s_staticInitializerCount;
    private static readonly int s_initializedStaticValue = InitializeStaticValue();
    private static readonly IntPtr s_initializedStaticIntPtr = new IntPtr(1);
    private static unsafe void* s_defaultStaticPointer;
    private static readonly object[] s_callbackHandles = new object[2];
    private static int s_callbackResult;
    private static unsafe delegate* unmanaged<StaticCallbackEvent, void*, void> s_staticCallback = &StaticFieldCallback;

    private static int InitializeStaticValue()
    {
        s_staticInitializerCount++;
        return 37;
    }

    private static IntPtr ReturnIntPtr(int value) => new IntPtr(value);

    private static IntPtr ReturnIntPtrFromLoop(int occupiedSlots)
    {
        for (int index = 0; index <= occupiedSlots; index++)
            if (index == occupiedSlots)
                return new IntPtr(index + 1);
        return IntPtr.Zero;
    }

    private static T GetStaticHandle<T>(int index) where T : class
        => s_staticHandleValues[index] as T;

    [UnmanagedCallersOnly]
    private static unsafe void StaticFieldCallback(StaticCallbackEvent callbackEvent, void* context)
    {
        if (context != (void*)1)
        {
            s_callbackResult = -1;
            return;
        }

        if (callbackEvent.NativeEvent != (void*)0x1234)
        {
            s_callbackResult = -2;
            return;
        }

        StaticHandleFeature handle = s_callbackHandles[1] as StaticHandleFeature;
        s_callbackResult = handle == null ? -3 : handle.Value;
    }

    private class HierarchyRoot
    {
        private readonly int _rootValue;
        protected readonly object RootReference;

        public HierarchyRoot(int rootValue, object rootReference)
        {
            _rootValue = rootValue;
            RootReference = rootReference;
        }

        public virtual int EvaluateHierarchy() => _rootValue;
        public object GetRootReference() => RootReference;
    }

    private class HierarchyMiddle : HierarchyRoot
    {
        private readonly long _middleValue;

        public HierarchyMiddle(int rootValue, long middleValue, object rootReference)
            : base(rootValue, rootReference)
        {
            _middleValue = middleValue;
        }

        public override int EvaluateHierarchy() => base.EvaluateHierarchy() + (int)_middleValue;
    }

    private sealed class HierarchyLeaf : HierarchyMiddle, IHierarchyFeature, ISecondaryFeature
    {
        private readonly byte _leafValue;
        private readonly object _leafReference;

        public HierarchyLeaf(int rootValue, long middleValue, byte leafValue, object rootReference, object leafReference)
            : base(rootValue, middleValue, rootReference)
        {
            _leafValue = leafValue;
            _leafReference = leafReference;
        }

        public override int EvaluateHierarchy() => base.EvaluateHierarchy() + _leafValue;
        int IHierarchyFeature.EvaluateHierarchy() => EvaluateHierarchy() + 1;
        public int SecondaryValue => _leafValue;
        public object GetLeafReference() => _leafReference;
    }

    private sealed class AlternateHierarchyFeature : IHierarchyFeature, ISecondaryFeature
    {
        private readonly int _value;

        public AlternateHierarchyFeature(int value)
        {
            _value = value;
        }

        public int EvaluateHierarchy() => _value * 2;
        public int SecondaryValue => _value;
    }

    private sealed class GenericContainer<T>
    {
        public static int InstanceCount;
        public T Value;

        public GenericContainer(T value)
        {
            Value = value;
            InstanceCount++;
        }

        public T Replace(T value)
        {
            T previous = Value;
            Value = value;
            return previous;
        }
    }

    private sealed class PrimaryFeature(int value)
    {
        public int Value { get; } = value;
        public int Double() => value * 2;
    }

    private sealed class RequiredFeature
    {
        public required int Value { get; init; }
        public required string Name { get; init; }
    }

    private sealed class InitializerFeature : IExplicitFeature
    {
        public int Value { get; set; }
        public string Label { get; set; }

        int IExplicitFeature.Add(int value) => Value + value;
    }

    private sealed class IndexedFeature
    {
        private readonly int[] _values = new int[2];

        public int this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }
    }

    private sealed class GenericFeature<T> : IGenericFeature<T>
        where T : IFeatureValue
    {
        private readonly T _value;

        public GenericFeature(T value)
        {
            _value = value;
        }

        public T GetValue() => _value;
    }

    private sealed class DisposableFeature : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private delegate int BinaryOperator(int left, int right);

    public static void Run()
    {
        s_expectedSum = RuntimeValue(ExpectedSum);
        int[] values = new int[RuntimeValue(5)];
        values[0] = RuntimeValue(1);
        values[1] = RuntimeValue(2);
        values[2] = RuntimeValue(3);
        values[3] = RuntimeValue(4);
        values[4] = RuntimeValue(5);
        List<int> list = new List<int>();
        for (int index = 0; index < values.Length; index++)
            list.Add(values[index]);
        VerifyTypes(values, list);
        VerifyEnums();
        VerifyNumericOperators();
        VerifyInstructionCoverage(values);
        VerifyConversionsAndUnsignedArithmetic();
        VerifyStrings();
        VerifyObjectAndGenericFeatures(values);
        VerifyInheritanceAndInterfaces();
        VerifyGenericFeatures();
        VerifyBoxing();
        VerifyStructures();
        VerifyLatestSyntax();
        VerifyModernLanguageFeatures(values);
        VerifySpans();
        VerifyArrays();
        VerifyDelegatesAndLinq(values);
        VerifyControlFlow(values);
        VerifyReferencesAndPointers(values);
        VerifyExceptionsAndResources();
        VerifyAsync().GetAwaiter().GetResult();
        VerifyExtendedLanguageFeatures();
        VerifyStaticFields();
        VerifyComprehensiveNativeValues();
        VerifyAllPrimitiveNullableValues();
        Console.WriteLine("Language feature validation passed.");
    }

    private static unsafe void VerifyStaticFields()
    {
        if (s_defaultStaticReference != null || s_defaultStaticBoolean ||
            s_defaultStaticInteger != 0 || s_defaultStaticLong != 0 ||
            s_defaultStaticDouble != 0 || s_defaultStaticArray != null ||
            s_defaultStaticValue.Number != 0 || s_defaultStaticValue.Reference != null ||
            s_defaultStaticPointer != null || (long)s_defaultStaticIntPtr != 0 ||
            s_defaultStaticUIntPtr.ToString() != "0")
            Fail("static field zero initialization");

        if (ExplicitStaticInitialization.First != RuntimeValue(41) ||
            ExplicitStaticInitialization.Reference == null ||
            ExplicitStaticInitialization.Reference.Value != RuntimeValue(42) ||
            ExplicitStaticInitialization.Values == null ||
            ExplicitStaticInitialization.Values.Length != RuntimeValue(2) ||
            ExplicitStaticInitialization.Values[0] != RuntimeValue(43) ||
            ExplicitStaticInitialization.Values[1] != RuntimeValue(44) ||
            ExplicitStaticInitialization.ConstructorRuns != RuntimeValue(1) ||
            ExplicitStaticInitialization.ConstructorPhase != RuntimeValue(4))
            Fail("static field initializer and constructor order");

        object integerHandle = new StaticHandleFeature(RuntimeValue(51));
        object stringHandle = new StaticHandleFeature(RuntimeValue(52));
        object[] integerHandles = GenericStaticStorage<int>.Handles;
        object[] stringHandles = GenericStaticStorage<string>.Handles;
        if (integerHandles == null || stringHandles == null)
            Fail("closed generic static field initialization");
        if (GenericStaticStorage<int>.ConstructorRuns != RuntimeValue(1) ||
            GenericStaticStorage<string>.ConstructorRuns != RuntimeValue(1))
            Fail("closed generic static constructor execution");
        if (GenericStaticStorage<int>.StoredType != typeof(int) ||
            GenericStaticStorage<string>.StoredType != typeof(string))
            Fail("closed generic static type initialization");
        integerHandles[0] = integerHandle;
        stringHandles[0] = stringHandle;
        StaticHandleFeature recoveredIntegerHandle = integerHandles[0] as StaticHandleFeature;
        StaticHandleFeature recoveredStringHandle = stringHandles[0] as StaticHandleFeature;
        if (recoveredIntegerHandle == null || recoveredIntegerHandle.Value != RuntimeValue(51) ||
            recoveredStringHandle == null || recoveredStringHandle.Value != RuntimeValue(52))
            Fail("closed generic static object recovery");
        if (ReferenceEquals(integerHandles, stringHandles))
            Fail("closed generic static storage isolation");

        if (s_staticCallback == null)
            Fail("static function pointer initialization");
        s_callbackHandles[1] = new StaticHandleFeature(RuntimeValue(61));
        s_callbackResult = 0;
        StaticCallbackEvent callbackEvent = new StaticCallbackEvent { NativeEvent = (void*)0x1234 };
        s_staticCallback(callbackEvent, (void*)1);
        if (s_callbackResult != RuntimeValue(61))
            Fail("unmanaged callback static handle recovery");

        if ((long)ReturnIntPtr(RuntimeValue(1)) != RuntimeValue(1) ||
            (long)ReturnIntPtrFromLoop(RuntimeValue(0)) != RuntimeValue(1) ||
            (long)ReturnIntPtrFromLoop(RuntimeValue(2)) != RuntimeValue(3))
            Fail("managed IntPtr return value");

        if ((long)s_initializedStaticIntPtr != RuntimeValue(1))
            Fail("static IntPtr initializer");
    }

    private static void VerifyTypes(int[] values, List<int> list)
    {
        if (!s_boolean || s_character != (char)RuntimeValue('L') ||
            s_sbyte != (sbyte)RuntimeValue(-1) || s_byte != (byte)RuntimeValue(1) ||
            s_short != (short)RuntimeValue(2) || s_ushort != (ushort)RuntimeValue(3) ||
            s_long != RuntimeValue(4) || s_ulong != (ulong)RuntimeValue(5) ||
            s_single != RuntimeValue(6) || s_double != RuntimeValue(7) ||
            s_string.Length != RuntimeValue(8) || s_string[RuntimeValue(0)] != 'f' ||
            FeatureName.Length != RuntimeValue(25) || FeatureName[0] != s_character)
            Fail("primitive types or const");

        string firstString = new string(new char[] { 'v', 'a', 'l', 'u', 'e' });
        string secondString = new string(new char[] { 'v', 'a', 'l', 'u', 'e' });
        string differentString = new string(new char[] { 'o', 't', 'h', 'e', 'r' });
        string nullString = null;
        if (!(firstString == secondString) || firstString != secondString ||
            firstString == differentString || firstString == nullString ||
            !(nullString == null) || !firstString.Equals((object)secondString) ||
            !string.Equals(firstString, secondString) ||
            firstString.GetHashCode() != secondString.GetHashCode())
            Fail("string equality");

        Type intType = typeof(int);
        Type stringType = typeof(string);
        Type localType = typeof(FeatureObject);
        Type validationType = typeof(LanguageFeatureValidation);
        Type genericType = typeof(GenericContainer<int>);
        Type genericValueType = typeof(GenericPair<int, FeatureValue>);
        if (intType == null || stringType == null || localType == null || validationType == null ||
            genericType == null || genericValueType == null)
            Fail("typeof");

        if (validationType.Name != nameof(LanguageFeatureValidation) ||
            validationType.Namespace != null ||
            validationType.FullName != nameof(LanguageFeatureValidation) ||
            localType.Name != nameof(FeatureObject) ||
            localType.Namespace != null ||
            localType.FullName != nameof(LanguageFeatureValidation) + "+" + nameof(FeatureObject))
            Fail("type metadata");

        FeatureValue first = values[0];
        FeatureValue second = RuntimeValue(2);
        FeatureValue combined = first + second;
        IFeatureValue boxed = new FeatureObject(combined.Value);
        object valueObject = boxed;

        if (!(valueObject is IFeatureValue value) || value.Value != RuntimeValue(4))
            Fail("interface and pattern matching");

        IFeatureValue cast = valueObject as IFeatureValue;
        if (cast == null || cast.Value != RuntimeValue(4))
            Fail("as cast");

        int changed = 0;
        FeatureObject eventSource = new FeatureObject(3);
        Action handler = () => changed++;
        eventSource.Changed += handler;
        eventSource.RaiseChanged();
        eventSource.Changed -= handler;
        if (changed != 1)
            Fail("event");

        if (values[0].Identity() != RuntimeValue(1))
            Fail("extension method");

        if (s_defaultStaticReference != null || s_staticInitializerCount != 1 ||
            s_initializedStaticValue != RuntimeValue(37) || s_staticHandleValues.Length != 2)
            Fail("static field initialization");

        StaticHandleFeature staticHandle = new StaticHandleFeature(RuntimeValue(29));
        s_staticHandleValues[0] = staticHandle;
        StaticHandleFeature resolvedHandle = GetStaticHandle<StaticHandleFeature>(0);
        if (resolvedHandle == null || resolvedHandle.Value != RuntimeValue(29))
            Fail("static object array generic type recovery");

        unsafe
        {
            IntPtr smallPointer = new IntPtr((void*)1);
            IntPtr widePointer = new IntPtr(unchecked((long)0x1234567887654321UL));
            if ((int)smallPointer != 1 || (long)smallPointer != 1 ||
                (long)widePointer != unchecked((long)0x1234567887654321UL))
                Fail("native pointer integer conversion");
        }

        FeatureKind kind = (FeatureKind)RuntimeValue((int)FeatureKind.Value);
        switch (kind)
        {
            case FeatureKind.Value:
                break;
            default:
                Fail("enum switch");
                break;
        }

        string typeName = nameof(FeatureObject);
        if (typeName.Length != RuntimeValue(13) || typeName[RuntimeValue(0)] != 'F')
            Fail("nameof");

        if (default(FeatureValue).Value != RuntimeValue(0) || list.Count != values.Length)
            Fail("default or collection");
    }

    private static void VerifyEnums()
    {
        FeatureFlags configured = FeatureFlags.Read | FeatureFlags.Execute;
        FeatureFlags all = FeatureFlags.Read | FeatureFlags.Write | FeatureFlags.Execute;
        FeatureFlags withoutRead = configured & ~FeatureFlags.Read;
        FeatureFlags fromValue = (FeatureFlags)RuntimeValue(3);
        int selected = configured switch
        {
            FeatureFlags.Read => 1,
            FeatureFlags.Read | FeatureFlags.Execute => 5,
            _ => 0,
        };

        if ((ushort)configured != RuntimeValue(5) ||
            (configured & FeatureFlags.Read) != FeatureFlags.Read ||
            (configured & FeatureFlags.Write) != FeatureFlags.None ||
            withoutRead != FeatureFlags.Execute ||
            (all & configured) != configured ||
            fromValue != (FeatureFlags.Read | FeatureFlags.Write) ||
            selected != RuntimeValue(5))
            Fail("enum flags");

        if (FeatureKind.Value.ToString() != "Value")
            Fail("named enum ToString");
        if (((FeatureKind)RuntimeValue(7)).ToString() != "7")
            Fail("unnamed enum ToString");
        if (FeatureFlags.Read.ToString() != "Read")
            Fail("single flag ToString");
        if (configured.ToString() != "Read, Execute")
            Fail("combined flags ToString");
        if (FeatureFlags.None.ToString() != "None")
            Fail("zero flag ToString");
        if (((FeatureFlags)RuntimeValue(8)).ToString() != "8")
            Fail("unknown flag ToString");
        if (SignedFeatureKind.Negative.ToString() != "Negative")
            Fail("negative named enum ToString");
        if (((SignedFeatureKind)(-RuntimeValue(3))).ToString() != "-3")
            Fail("negative unnamed enum ToString");
        if (SignedFeatureKind.Positive.ToString() != "Positive")
            Fail("signed named enum ToString");
    }

    private static void VerifyNumericOperators()
    {
        int left = RuntimeValue(42);
        int right = RuntimeValue(5);
        if (left + right != RuntimeValue(47) || left - right != RuntimeValue(37) ||
            left * right != RuntimeValue(210) || left / right != RuntimeValue(8) ||
            left % right != RuntimeValue(2) || -right != RuntimeValue(-5) ||
            ~right != RuntimeValue(-6) || (left & right) != 0 ||
            (left | right) != RuntimeValue(47) || (left ^ right) != RuntimeValue(47) ||
            (right << 3) != RuntimeValue(40) || (left >> 1) != RuntimeValue(21))
            Fail("integer arithmetic or bitwise operators");

        uint unsigned = 0xf0000000u;
        ulong wideUnsigned = 0xf000000000000000ul;
        long wideSigned = RuntimeValue(-81);
        if ((unsigned >> RuntimeValue(28)) != RuntimeValue(15) || unsigned <= int.MaxValue ||
            (wideUnsigned >> RuntimeValue(60)) != (ulong)RuntimeValue(15) ||
            wideSigned / RuntimeValue(9) != RuntimeValue(-9) ||
            wideSigned % RuntimeValue(10) != RuntimeValue(-1))
            Fail("signed or unsigned arithmetic");

        sbyte signedByte = (sbyte)RuntimeValue(-120);
        byte unsignedByte = (byte)RuntimeValue(250);
        short signedShort = (short)RuntimeValue(-30000);
        ushort unsignedShort = (ushort)RuntimeValue(60000);
        byte wrappedByte = unchecked((byte)(unsignedByte + RuntimeValue(10)));
        uint widenedSigned = unchecked((uint)signedByte);
        if (signedByte != -120 || unsignedByte != 250 || signedShort != -30000 ||
            unsignedShort != 60000 || wrappedByte != 4 || widenedSigned != 0xffffff88u ||
            (char)RuntimeValue(0x03a9) != 'Ω')
            Fail("numeric conversions");

        float single = RuntimeValue(15) / 2.0f;
        double precision = RuntimeValue(22) / 4.0;
        if (single != 7.5f || precision != 5.5 || single <= 7.0f || precision >= 6.0)
            Fail("floating point arithmetic");

        nint native = RuntimeValue(12);
        nuint nativeUnsigned = (nuint)RuntimeValue(13);
        native++;
        --nativeUnsigned;
        if (native != RuntimeValue(13) || nativeUnsigned != (nuint)RuntimeValue(12))
            Fail("native integer arithmetic");

        s_volatileValue = 0;
        bool skippedAnd = false && SetVolatileAndReturnTrue();
        bool skippedOr = true || SetVolatileAndReturnTrue();
        bool evaluated = true && SetVolatileAndReturnTrue();
        if (skippedAnd || !skippedOr || !evaluated || s_volatileValue != RuntimeValue(1))
            Fail("short circuit boolean operators");
    }

    private static void VerifyInstructionCoverage(int[] values)
    {
        s_instructionStatic = RuntimeValue(2);
        s_instructionStatic += RuntimeValue(3);
        ref int staticReference = ref s_instructionStatic;
        staticReference++;

        byte[] bytes = new byte[1];
        sbyte[] signedBytes = new sbyte[1];
        short[] shorts = new short[1];
        ushort[] unsignedShorts = new ushort[1];
        int[] integers = new int[1];
        uint[] unsignedIntegers = new uint[1];
        long[] longs = new long[1];
        ulong[] unsignedLongs = new ulong[1];
        float[] singles = new float[1];
        double[] doubles = new double[1];
        bool[] booleans = new bool[1];
        char[] characters = new char[1];
        object[] references = new object[1];
        nint[] nativeIntegers = new nint[1];

        bytes[0] = 1;
        signedBytes[0] = -2;
        shorts[0] = -3;
        unsignedShorts[0] = 4;
        integers[0] = 5;
        unsignedIntegers[0] = 6;
        longs[0] = 7;
        unsignedLongs[0] = 8;
        singles[0] = 9.0f;
        doubles[0] = 10.0;
        booleans[0] = true;
        characters[0] = 'A';
        references[0] = new FeatureObject(RuntimeValue(11));
        nativeIntegers[0] = RuntimeValue(12);

        FeatureObject feature = (FeatureObject)references[0];
        FeatureBase baseFeature = feature;
        HiddenFeature hidden = new HiddenFeature(RuntimeValue(12));
        FeatureBase hiddenAsBase = hidden;
        Func<int> virtualMethod = baseFeature.Evaluate;
        Func<int> hiddenMethod = hidden.Evaluate;
        IHierarchyFeature interfaceFeature = new AlternateHierarchyFeature(RuntimeValue(13));
        Func<int> interfaceMethod = interfaceFeature.EvaluateHierarchy;

        int switched = SelectInstruction(RuntimeValue(3));
        int mutated = MutateArgument(RuntimeValue(2));
        int local = RuntimeValue(4);
        local = local + RuntimeValue(5);
        ref int localReference = ref integers[0];
        localReference = RuntimeValue(14);

        Coordinate[] coordinates = new Coordinate[] { new Coordinate(RuntimeValue(1), RuntimeValue(2)) };
        Coordinate coordinate = ReadArrayElement(coordinates, RuntimeValue(0));
        WriteArrayElement(coordinates, RuntimeValue(0), new Coordinate(RuntimeValue(3), RuntimeValue(4)));

        if (s_instructionStatic != RuntimeValue(6))
            Fail("static field instructions");

        if (bytes[0] != 1 || signedBytes[0] != -2 || shorts[0] != -3 ||
            unsignedShorts[0] != 4 || integers[0] != RuntimeValue(14) ||
            unsignedIntegers[0] != 6 || longs[0] != 7 || unsignedLongs[0] != 8 ||
            singles[0] != 9.0f || doubles[0] != 10.0 || !booleans[0] ||
            characters[0] != 'A' || feature.Value != RuntimeValue(12))
            Fail("array element instructions");

        if (nativeIntegers[0] != RuntimeValue(12) || coordinate.Sum() != RuntimeValue(3) ||
            coordinates[0].Sum() != RuntimeValue(7))
            Fail("native or generic array elements");

        if (baseFeature.Evaluate() != RuntimeValue(12))
            Fail("override dispatch");
        if (hidden.Evaluate() != RuntimeValue(100))
            Fail("hidden method direct call");
        if (hiddenAsBase.Evaluate() != RuntimeValue(12))
            Fail("hidden method base dispatch");
        if (virtualMethod() != RuntimeValue(12))
            Fail("virtual method delegate");
        if (hiddenMethod() != RuntimeValue(100))
            Fail("hidden method delegate");
        if (interfaceMethod() != RuntimeValue(26))
            Fail("interface method delegate");

        if (switched != RuntimeValue(8) || mutated != RuntimeValue(6) ||
            local != RuntimeValue(9) || values[0] != RuntimeValue(1))
            Fail("argument, local, or switch instructions");
    }

    private static void VerifyConversionsAndUnsignedArithmetic()
    {
        uint unsignedLeft = (uint)RuntimeValue(100);
        uint unsignedRight = (uint)RuntimeValue(9);
        ulong wideUnsignedLeft = (ulong)RuntimeValue(1000);
        ulong wideUnsignedRight = (ulong)RuntimeValue(64);

        int signedAdd = checked(RuntimeValue(20) + RuntimeValue(3));
        int signedSubtract = checked(RuntimeValue(20) - RuntimeValue(3));
        int signedMultiply = checked(RuntimeValue(20) * RuntimeValue(3));
        uint unsignedAdd = checked(unsignedLeft + unsignedRight);
        uint unsignedSubtract = checked(unsignedLeft - unsignedRight);
        uint unsignedMultiply = checked(unsignedRight * (uint)RuntimeValue(3));

        long signedSource = RuntimeValue(120);
        ulong unsignedSource = (ulong)RuntimeValue(120);
        sbyte signedByte = checked((sbyte)signedSource);
        byte unsignedByte = checked((byte)signedSource);
        short signedShort = checked((short)signedSource);
        ushort unsignedShort = checked((ushort)signedSource);
        int signedInteger = checked((int)signedSource);
        uint unsignedInteger = checked((uint)signedSource);
        nint nativeInteger = checked((nint)signedSource);
        nuint nativeUnsigned = checked((nuint)signedSource);
        int integerFromUnsigned = checked((int)unsignedSource);
        uint unsignedFromUnsigned = checked((uint)unsignedSource);
        sbyte signedByteFromUnsigned = checked((sbyte)unsignedSource);
        byte unsignedByteFromUnsigned = checked((byte)unsignedSource);
        short signedShortFromUnsigned = checked((short)unsignedSource);
        ushort unsignedShortFromUnsigned = checked((ushort)unsignedSource);
        long signedLongFromUnsigned = checked((long)unsignedSource);
        ulong unsignedLongFromSigned = checked((ulong)signedSource);
        nint nativeFromUnsigned = checked((nint)unsignedSource);
        nuint nativeUnsignedFromUnsigned = checked((nuint)unsignedSource);
        uint uncheckedUnsignedInteger = unchecked((uint)signedSource);
        long signedLongFromDouble = checked((long)(120.0 + ZeroForConversion()));
        ulong unsignedLongFromDouble = checked((ulong)(120.0 + ZeroForConversion()));
        double floatingUnsigned = wideUnsignedLeft;

        if (unsignedLeft / unsignedRight != (uint)RuntimeValue(11) ||
            unsignedLeft % unsignedRight != (uint)RuntimeValue(1) ||
            wideUnsignedLeft / wideUnsignedRight != (ulong)RuntimeValue(15) ||
            wideUnsignedLeft % wideUnsignedRight != (ulong)RuntimeValue(40))
            Fail("unsigned division or remainder");

        if (signedAdd != RuntimeValue(23) || signedSubtract != RuntimeValue(17) ||
            signedMultiply != RuntimeValue(60) || unsignedAdd != (uint)RuntimeValue(109) ||
            unsignedSubtract != (uint)RuntimeValue(91) || unsignedMultiply != (uint)RuntimeValue(27))
            Fail("checked arithmetic");

        bool signedAddOverflow = false;
        bool signedSubtractOverflow = false;
        bool signedMultiplyOverflow = false;
        bool unsignedAddOverflow = false;
        bool unsignedSubtractOverflow = false;
        bool unsignedMultiplyOverflow = false;
        try { _ = checked(int.MaxValue + RuntimeValue(1)); }
        catch (OverflowException) { signedAddOverflow = true; }
        try { _ = checked(int.MinValue - RuntimeValue(1)); }
        catch (OverflowException) { signedSubtractOverflow = true; }
        try { _ = checked(int.MaxValue * RuntimeValue(2)); }
        catch (OverflowException) { signedMultiplyOverflow = true; }
        try { _ = checked(uint.MaxValue + (uint)RuntimeValue(1)); }
        catch (OverflowException) { unsignedAddOverflow = true; }
        try { _ = checked((uint)RuntimeValue(0) - (uint)RuntimeValue(1)); }
        catch (OverflowException) { unsignedSubtractOverflow = true; }
        try { _ = checked(uint.MaxValue * (uint)RuntimeValue(2)); }
        catch (OverflowException) { unsignedMultiplyOverflow = true; }
        if (!signedAddOverflow || !signedSubtractOverflow || !signedMultiplyOverflow ||
            !unsignedAddOverflow || !unsignedSubtractOverflow || !unsignedMultiplyOverflow)
            Fail("checked arithmetic overflow");

        bool signedDivideByZero = false;
        bool unsignedDivideByZero = false;
        bool signedRemainderByZero = false;
        bool divisionOverflow = false;
        try { _ = RuntimeValue(1) / RuntimeValue(0); }
        catch (DivideByZeroException) { signedDivideByZero = true; }
        try { _ = (uint)RuntimeValue(1) / (uint)RuntimeValue(0); }
        catch (DivideByZeroException) { unsignedDivideByZero = true; }
        try { _ = RuntimeValue(1) % RuntimeValue(0); }
        catch (DivideByZeroException) { signedRemainderByZero = true; }
        try { _ = int.MinValue / -RuntimeValue(1); }
        catch (OverflowException) { divisionOverflow = true; }
        if (!signedDivideByZero || !unsignedDivideByZero || !signedRemainderByZero || !divisionOverflow)
            Fail("integer division exceptions");

        if (signedByte != (sbyte)RuntimeValue(120) || unsignedByte != (byte)RuntimeValue(120) ||
            signedShort != (short)RuntimeValue(120) || unsignedShort != (ushort)RuntimeValue(120) ||
            signedInteger != RuntimeValue(120) || unsignedInteger != (uint)RuntimeValue(120) ||
            nativeInteger != RuntimeValue(120) || nativeUnsigned != (nuint)RuntimeValue(120) ||
            integerFromUnsigned != RuntimeValue(120) || unsignedFromUnsigned != (uint)RuntimeValue(120) ||
            signedByteFromUnsigned != (sbyte)RuntimeValue(120) ||
            unsignedByteFromUnsigned != (byte)RuntimeValue(120) ||
            signedShortFromUnsigned != (short)RuntimeValue(120) ||
            unsignedShortFromUnsigned != (ushort)RuntimeValue(120) ||
            signedLongFromUnsigned != RuntimeValue(120) || unsignedLongFromSigned != (ulong)RuntimeValue(120) ||
            nativeFromUnsigned != RuntimeValue(120) || nativeUnsignedFromUnsigned != (nuint)RuntimeValue(120) ||
            uncheckedUnsignedInteger != (uint)RuntimeValue(120) ||
            signedLongFromDouble != RuntimeValue(120) || unsignedLongFromDouble != (ulong)RuntimeValue(120) ||
            floatingUnsigned != RuntimeValue(1000))
            Fail("checked numeric conversions");

        double zero = RuntimeValue(0);
        double notANumber = zero / zero;
        if (notANumber == notANumber || notANumber < zero || notANumber > zero ||
            !(notANumber != zero) || notANumber <= zero || notANumber >= zero)
            Fail("floating point unordered comparisons");
    }

    private static double ZeroForConversion() => RuntimeValue(0);

    private static void VerifyStrings()
    {
        string unicode = "汉字Ω";
        string combined = "prefix-" + unicode + "-suffix";
        string padded = "  value  ";
        string[] parts = "one,two,three".Split(',');
        string joined = string.Join("|", parts);
        string empty = string.Empty;

        if (unicode.Length != RuntimeValue(3) || unicode[0] != '汉' || unicode[2] != 'Ω' ||
            combined.Length != RuntimeValue(17) || !combined.StartsWith("prefix-") ||
            !combined.EndsWith("-suffix") || combined.IndexOf(unicode) != RuntimeValue(7) ||
            combined.IndexOf('Ω') != RuntimeValue(9))
            Fail("unicode or string search");

        if (padded.Trim() != "value" || "abcabc".Replace('b', 'x') != "axcaxc" ||
            combined.Substring(RuntimeValue(7), RuntimeValue(3)) != unicode ||
            parts.Length != RuntimeValue(3) || parts[1] != "two" || joined != "one|two|three" ||
            !string.IsNullOrEmpty(empty) || !string.IsNullOrEmpty(null) || string.IsNullOrEmpty("x"))
            Fail("string operations");

        if (RuntimeValue(-123).ToString() != "-123" || ((uint)RuntimeValue(456)).ToString() != "456" ||
            true.ToString() != "True" || 'Z'.ToString() != "Z")
            Fail("primitive formatting");
    }

    private static void VerifyObjectAndGenericFeatures(int[] values)
    {
        FeatureValue value = values[0];
        object boxedValue = value;
        if (!(boxedValue is FeatureValue unboxedValue) ||
            ((FeatureValue)boxedValue).Value != RuntimeValue(1) ||
            ((IFeatureValue)boxedValue).Value != RuntimeValue(1) ||
            unboxedValue.Value != RuntimeValue(1))
            Fail("boxing or unboxing");

        FeatureObject feature = new FeatureObject(RuntimeValue(4));
        FeatureBase baseFeature = feature;
        if (EvaluateBase(baseFeature) != RuntimeValue(5))
            Fail("virtual dispatch");

        FeatureObject[] derivedArray = new[] { feature };
        FeatureBase[] baseArray = derivedArray;
        if (baseArray[0] != feature || baseArray[0].Evaluate() != RuntimeValue(5))
            Fail("array covariance");

        IGenericFeature<FeatureValue> valueReader = new GenericFeature<FeatureValue>(value);
        IGenericFeature<FeatureObject> objectReader = new GenericFeature<FeatureObject>(feature);
        if (ReadGenericValue(valueReader.GetValue()) != RuntimeValue(1) ||
            ReadGenericValue(objectReader.GetValue()) != RuntimeValue(5) ||
            PreserveReference(feature) != feature)
            Fail("generic constraint or interface dispatch");
    }

    private static void VerifyInheritanceAndInterfaces()
    {
        object rootReference = new object();
        object leafReference = new object();
        HierarchyLeaf leaf = new HierarchyLeaf(
            RuntimeValue(1), RuntimeValue(2), (byte)RuntimeValue(3), rootReference, leafReference);
        HierarchyRoot root = leaf;
        HierarchyMiddle middle = leaf;
        IHierarchyFeature first = leaf;
        IHierarchyFeature second = new AlternateHierarchyFeature(RuntimeValue(4));
        IHierarchyFeature[] implementations = new IHierarchyFeature[] { first, second };
        int total = 0;
        for (int index = 0; index < implementations.Length; index++)
            total += implementations[index].EvaluateHierarchy();

        if (root.EvaluateHierarchy() != RuntimeValue(6) || middle.EvaluateHierarchy() != RuntimeValue(6) ||
            first.EvaluateHierarchy() != RuntimeValue(7) || second.EvaluateHierarchy() != RuntimeValue(8) ||
            total != RuntimeValue(15) || ((ISecondaryFeature)leaf).SecondaryValue != RuntimeValue(3) ||
            root.GetRootReference() != rootReference || leaf.GetLeafReference() != leafReference)
            Fail("multi-level inheritance or interface dispatch");

        object boxedLeaf = HideObject(leaf);
        object unrelated = HideObject(new object());
        if (!(boxedLeaf is HierarchyRoot) || !(boxedLeaf is IHierarchyFeature) ||
            boxedLeaf is FeatureObject || unrelated is IHierarchyFeature ||
            boxedLeaf as HierarchyLeaf != leaf || unrelated as HierarchyLeaf != null ||
            null is HierarchyLeaf)
            Fail("runtime type tests");
    }

    private static void VerifyGenericFeatures()
    {
        GenericContainer<int>.InstanceCount = 0;
        GenericContainer<string>.InstanceCount = 0;
        GenericContainer<int> integers = new GenericContainer<int>(RuntimeValue(4));
        GenericContainer<int> moreIntegers = new GenericContainer<int>(RuntimeValue(8));
        GenericContainer<string> strings = new GenericContainer<string>("first");
        int previousInteger = integers.Replace(RuntimeValue(6));
        string previousString = strings.Replace("second");

        if (GenericContainer<int>.InstanceCount != RuntimeValue(2) ||
            GenericContainer<string>.InstanceCount != RuntimeValue(1) ||
            previousInteger != RuntimeValue(4) || integers.Value != RuntimeValue(6) ||
            moreIntegers.Value != RuntimeValue(8) || previousString != "first" || strings.Value != "second")
            Fail("closed generic classes or static fields");

        GenericPair<int, FeatureValue> pair = new GenericPair<int, FeatureValue>(
            RuntimeValue(3), new FeatureValue(RuntimeValue(5)));
        GenericPair<int, FeatureValue> pairCopy = IdentityGeneric(pair);
        int first = RuntimeValue(10);
        int second = RuntimeValue(20);
        Swap(ref first, ref second);
        FeatureValue feature = new FeatureValue(RuntimeValue(9));

        if (pairCopy.First != RuntimeValue(3) || pairCopy.Second.Value != RuntimeValue(5) ||
            first != RuntimeValue(20) || second != RuntimeValue(10) ||
            IdentityGeneric(feature).Value != RuntimeValue(9) ||
            DefaultGeneric<int>() != 0 || DefaultGeneric<FeatureObject>() != null ||
            ReadGenericValue(feature) != RuntimeValue(9))
            Fail("generic methods or value types");
    }

    private static void VerifyBoxing()
    {
        object marker = new object();
        BoxingValue original = new BoxingValue
        {
            Number = RuntimeValue(7),
            Offset = (short)RuntimeValue(2),
            Reference = marker,
        };
        object boxedValue = original;
        original.Number = RuntimeValue(30);
        BoxingValue unboxedValue = (BoxingValue)boxedValue;
        IFeatureValue featureValue = (IFeatureValue)boxedValue;
        IBoxingMutation mutation = (IBoxingMutation)boxedValue;

        if (unboxedValue.Number != RuntimeValue(7) ||
            unboxedValue.Offset != (short)RuntimeValue(2) ||
            unboxedValue.Reference != marker ||
            featureValue.Value != RuntimeValue(9) ||
            mutation.Increment() != RuntimeValue(8) ||
            featureValue.Value != RuntimeValue(10))
            Fail("struct boxing");

        int integer = RoundTripBox(RuntimeValue(-7));
        long longInteger = RoundTripBox((long)RuntimeValue(9));
        FeatureKind kind = RoundTripBox(FeatureKind.Value);
        BoxingValue genericValue = RoundTripBox(original);
        int? nullableValue = RuntimeValue(5);
        int? nullableNull = null;
        object boxedNullableValue = nullableValue;
        object boxedNullableNull = nullableNull;

        if (integer != RuntimeValue(-7) || longInteger != RuntimeValue(9) ||
            kind != FeatureKind.Value || genericValue.Number != RuntimeValue(30) ||
            (int)boxedNullableValue != RuntimeValue(5) || boxedNullableNull != null)
            Fail("generic or nullable boxing");

        FeatureObject feature = new FeatureObject(RuntimeValue(6));
        object boxedReference = feature;
        if ((FeatureObject)boxedReference != feature || ((IFeatureValue)boxedReference).Value != RuntimeValue(7))
            Fail("reference conversion");
    }

    private static T RoundTripBox<T>(T value)
        where T : struct
    {
        object boxed = value;
        return (T)boxed;
    }

    private static void VerifyStructures()
    {
        NestedValue empty = default;
        NestedValue value = new NestedValue(
            new Coordinate(RuntimeValue(1), RuntimeValue(2)),
            new FeatureValue(RuntimeValue(3)),
            RuntimeValue(4),
            (byte)RuntimeValue(5));
        NestedValue copy = value;
        copy.Coordinate.X = RuntimeValue(10);

        if (empty.Total != RuntimeValue(0) || value.Total != RuntimeValue(15) ||
            copy.Total != RuntimeValue(24) || value.Coordinate.X != RuntimeValue(1))
            Fail("struct value semantics");

        NestedValue[] values = new NestedValue[RuntimeValue(2)];
        values[0] = value;
        CreateNestedValue(out values[1]);
        OffsetNestedValue(ref values[0], RuntimeValue(1));

        if (ReadNestedValue(in values[0]) != RuntimeValue(17) ||
            ReadNestedValue(in values[1]) != RuntimeValue(15) ||
            values[0].Coordinate.Sum() != RuntimeValue(4))
            Fail("struct fields or parameters");

        Coordinate original = new Coordinate(RuntimeValue(3), RuntimeValue(4));
        Coordinate changed = ChangeCoordinate(original);
        if (original.Sum() != RuntimeValue(7) || changed.Sum() != RuntimeValue(16))
            Fail("struct value parameter copy");

    }

    private static void CreateNestedValue(out NestedValue value)
    {
        value = new NestedValue(
            new Coordinate(RuntimeValue(1), RuntimeValue(2)),
            new FeatureValue(RuntimeValue(3)),
            RuntimeValue(4),
            (byte)RuntimeValue(5));
    }

    private static void OffsetNestedValue(ref NestedValue value, int offset)
    {
        value.Coordinate.X += offset;
        value.Wide += offset;
    }

    private static int ReadNestedValue(in NestedValue value) => value.Total;

    private static void VerifyLatestSyntax()
    {
        int[] collection = [RuntimeValue(1), RuntimeValue(2), RuntimeValue(3)];
        List<int> list = [RuntimeValue(4), RuntimeValue(5)];
        PrimaryFeature primary = new(RuntimeValue(6));
        RequiredFeature required = new()
        {
            Value = RuntimeValue(7),
            Name = "required",
        };
        string raw = """raw value""";
        int pattern = list switch
        {
            [4, 5] => 9,
            [4] => 4,
            _ => 0,
        };
        int relational = required.Value switch
        {
            < 0 => -1,
            >= 7 and < 8 => 1,
            _ => 0,
        };

        if (collection.Length != RuntimeValue(3) || list.Count != RuntimeValue(2) ||
            primary.Value != RuntimeValue(6) || primary.Double() != RuntimeValue(12) ||
            required.Value != RuntimeValue(7) || required.Name != "required" ||
            raw != "raw value" || pattern != RuntimeValue(9) || relational != RuntimeValue(1))
            Fail("latest language syntax");
    }

    private static unsafe void VerifyModernLanguageFeatures(int[] values)
    {
        string fallback = null;
        fallback ??= "fallback";
        InitializerFeature initialized = new InitializerFeature
        {
            Value = RuntimeValue(8),
            Label = "initializer",
        };
        IndexedFeature indexed = new IndexedFeature
        {
            [0] = RuntimeValue(3),
            [1] = RuntimeValue(4),
        };
        List<int> initializedList = new List<int>
        {
            RuntimeValue(1),
            RuntimeValue(2),
            RuntimeValue(3),
        };
        int? present = RuntimeValue(8);
        int? absent = null;
        int? conditionalValue = initialized?.Value;

        if (fallback?.ToString() != "fallback" ||
            initialized is not { Value: >= 8 and <= 8, Label: "initializer" } ||
            ((IExplicitFeature)initialized).Add(RuntimeValue(1)) != RuntimeValue(9) ||
            indexed[0] + indexed[1] != RuntimeValue(7) ||
            initializedList.Count != RuntimeValue(3) ||
            !present.HasValue || present.Value != RuntimeValue(8) ||
            (int)present != RuntimeValue(8) || absent.HasValue ||
            absent.GetValueOrDefault(RuntimeValue(6)) != RuntimeValue(6) ||
            !conditionalValue.HasValue || conditionalValue.Value != RuntimeValue(8))
            Fail("initializers, null handling, nullable values, or patterns");

        string category = initialized switch
        {
            { Value: < 0 } => "negative",
            { Value: >= 8 and <= 8 } => "expected",
            _ => "unexpected",
        };
        object boxedNumber = RuntimeValue(3);
        string matched = boxedNumber switch
        {
            int number when number == RuntimeValue(3) => "matched",
            _ => "unexpected",
        };
        if (category != "expected" || matched != "matched")
            Fail("switch patterns");

        int optional = CombineValues(right: RuntimeValue(3), left: RuntimeValue(2), multiplier: RuntimeValue(2));
        if (optional != RuntimeValue(10) || Sum(RuntimeValue(1), RuntimeValue(2), RuntimeValue(3)) != RuntimeValue(6) || Sum() != RuntimeValue(0))
            Fail("optional, named, or params arguments");

        int original = values[1];
        ref int second = ref GetElement(values, RuntimeValue(1));
        second = RuntimeValue(9);
        bool finallyRan = false;
        int returnValue = ReturnFromFinally(ref finallyRan);
        second = original;
        if (returnValue != RuntimeValue(6) || !finallyRan || values[1] != RuntimeValue(2))
            Fail("ref return or finally return");

        int captured = RuntimeValue(4);
        int CapturingLocalFunction(int value) => value + captured;
        static int StaticLocalFunction(int left, int right) => left * right;

        delegate* managed<int, int> doubleValue = &DoubleValue;
        if (CapturingLocalFunction(RuntimeValue(3)) != RuntimeValue(7) ||
            StaticLocalFunction(RuntimeValue(2), RuntimeValue(3)) != RuntimeValue(6) ||
            doubleValue(RuntimeValue(5)) != RuntimeValue(10))
            Fail("local function or function pointer");
    }

    private static void VerifySpans()
    {
        int[] values = [RuntimeValue(1), RuntimeValue(2), RuntimeValue(3), RuntimeValue(4), RuntimeValue(5)];
        Span<int> span = values;
        span[RuntimeValue(1)] = RuntimeValue(9);
        Span<int> slice = span.Slice(RuntimeValue(1), RuntimeValue(3));
        ReadOnlySpan<int> readOnly = values;
        ReadOnlySpan<int> converted = span;
        ReadOnlySpan<int> readOnlySlice = readOnly.Slice(RuntimeValue(2));
        ReadOnlySpan<byte> utf8 = "IL2LLVM"u8;

        if (span.Length != RuntimeValue(5) || span.IsEmpty || values[1] != RuntimeValue(9) ||
            slice.Length != RuntimeValue(3) || slice[0] != RuntimeValue(9) || slice[2] != RuntimeValue(4) ||
            readOnly.Length != RuntimeValue(5) || readOnly[1] != RuntimeValue(9) ||
            converted[4] != RuntimeValue(5) || readOnlySlice.Length != RuntimeValue(3) ||
            readOnlySlice[0] != RuntimeValue(3) || utf8.Length != RuntimeValue(7) ||
            utf8[0] != (byte)'I' || utf8[2] != (byte)'2' || utf8[6] != (byte)'M')
            Fail("span or UTF-8 string literal");
    }

    private static void VerifyArrays()
    {
        int rows = RuntimeValue(2);
        int columns = RuntimeValue(3);
        int[,] matrix = new int[rows, columns];
        matrix[0, 0] = RuntimeValue(1);
        matrix[rows - 1, columns - 1] = RuntimeValue(6);

        if (matrix.Rank != RuntimeValue(2) || matrix.Length != RuntimeValue(6) ||
            matrix.GetLength(0) != rows || matrix.GetLength(1) != columns ||
            matrix.GetLowerBound(RuntimeValue(0)) != RuntimeValue(0) ||
            matrix.GetUpperBound(RuntimeValue(1)) != columns - RuntimeValue(1) ||
            matrix[0, 0] != RuntimeValue(1) || matrix[rows - 1, columns - 1] != RuntimeValue(6))
            Fail("multidimensional array");

        Coordinate[,] coordinates = new Coordinate[rows, columns];
        coordinates[rows - 1, columns - 1] = new Coordinate(RuntimeValue(10), RuntimeValue(20));
        if (coordinates[rows - 1, columns - 1].Sum() != RuntimeValue(30))
            Fail("multidimensional value element set");
        coordinates[rows - 1, columns - 1].X++;
        FeatureObject[,] objects = new FeatureObject[rows, columns];
        objects[0, columns - 1] = new FeatureObject(RuntimeValue(8));
        if (coordinates[rows - 1, columns - 1].Sum() != RuntimeValue(31))
            Fail("multidimensional value element address");
        if (objects[0, columns - 1].Value != RuntimeValue(9))
            Fail("multidimensional reference element");

        int[,,] cube = new int[rows, rows, rows];
        cube[rows - 1, 0, rows - 1] = RuntimeValue(7);
        if (cube.Rank != RuntimeValue(3) || cube.GetLength(0) != rows ||
            cube.GetLength(1) != rows || cube.GetLength(2) != rows ||
            cube[rows - 1, 0, rows - 1] != RuntimeValue(7))
            Fail("three-dimensional array");

        int[][] jagged = new int[rows][];
        jagged[0] = new int[columns];
        jagged[1] = new int[RuntimeValue(1)];
        jagged[0][columns - 1] = RuntimeValue(8);
        if (jagged.Length != rows || jagged[0].Length != columns ||
            jagged[1].Length != RuntimeValue(1) || jagged[0][columns - 1] != RuntimeValue(8))
            Fail("jagged array");

        bool negativeIndex = false;
        bool upperIndex = false;
        bool nullArray = false;
        bool negativeLength = false;
        int[] one = new int[RuntimeValue(1)];
        try { _ = one[-RuntimeValue(1)]; }
        catch (IndexOutOfRangeException) { negativeIndex = true; }
        try { one[RuntimeValue(1)] = RuntimeValue(2); }
        catch (IndexOutOfRangeException) { upperIndex = true; }
        try
        {
            int[] missing = null;
            _ = missing[RuntimeValue(0)];
        }
        catch (NullReferenceException) { nullArray = true; }
        try { _ = new int[-RuntimeValue(1)]; }
        catch (OverflowException) { negativeLength = true; }
        if (!negativeIndex || !upperIndex || !nullArray || !negativeLength)
            Fail("array exceptions");
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int EvaluateBase(FeatureBase feature) => feature.Evaluate();

    private static int ReadGenericValue<T>(T value)
        where T : IFeatureValue
        => value.Value;

    private static T PreserveReference<T>(T value)
        where T : class
        => value;

    private static T ReadArrayElement<T>(T[] values, int index) => values[index];

    private static void WriteArrayElement<T>(T[] values, int index, T value) => values[index] = value;

    private static int CombineValues(int left, int right = 4, int multiplier = 1)
        => (left + right) * multiplier;

    private static int MutateArgument(int value)
    {
        value += RuntimeValue(1);
        value *= RuntimeValue(2);
        return value;
    }

    private static int SelectInstruction(int value)
    {
        switch (value)
        {
            case 0:
                return RuntimeValue(1);
            case 1:
                return RuntimeValue(2);
            case 2:
                return RuntimeValue(4);
            case 3:
                return RuntimeValue(8);
            default:
                return RuntimeValue(16);
        }
    }

    private static ref int GetElement(int[] values, int index) => ref values[index];

    private static int ReturnFromFinally(ref bool finallyRan)
    {
        try
        {
            return RuntimeValue(6);
        }
        finally
        {
            finallyRan = true;
        }
    }

    private static int DoubleValue(int value) => value * 2;

    private static void VerifyDelegatesAndLinq(int[] values)
    {
        BinaryOperator add = (left, right) => left + right;
        Func<int, int> doubleValue = value => value * 2;
        Action<int> setVolatile = value => s_volatileValue = value;
        setVolatile(add(RuntimeValue(2), RuntimeValue(3)));

        IEnumerable<int> query = from value in values
                                 where value > 1
                                 select doubleValue(value);
        int[] projected = query.ToArray();
        if (projected.Length != RuntimeValue(4) || projected[0] != RuntimeValue(4) ||
            s_volatileValue != RuntimeValue(5))
            Fail("delegates or LINQ");

        int total = Sum(values);
        if (total != s_expectedSum || query.Count() != RuntimeValue(4) ||
            query.First() != RuntimeValue(4))
            Fail("LINQ operators");
    }

    private static void VerifyControlFlow(int[] values)
    {
        int total = 0;
        int index = 0;
        do
        {
            if (values[index] == 2)
            {
                index++;
                continue;
            }

            total += values[index];
            index++;
        }
        while (index < values.Length);

        int foreachTotal = 0;
        foreach (int value in values)
            foreachTotal += value;

        int gotoValue = 0;
        goto AssignValue;
    AddValue:
        gotoValue++;
        goto End;
    AssignValue:
        gotoValue = 4;
        goto AddValue;
    End:
        if (total != RuntimeValue(13) || foreachTotal != s_expectedSum ||
            gotoValue != RuntimeValue(5))
            Fail("loop or goto");

        int switchValue = values[0] switch
        {
            0 => 0,
            1 => 10,
            _ => -1,
        };
        if (switchValue != RuntimeValue(10))
            Fail("switch expression");
        else
            switchValue = 0;

        if (switchValue != 0)
            Fail("else");
    }

    private static unsafe void VerifyReferencesAndPointers(int[] values)
    {
        int first = values[0];
        Increment(ref first);

        int second = values[1];
        AddThroughReferences(ref first, ref second);

        if (!TryRead(values, 2, out int third) ||
            TryRead(values, values.Length, out int missing) || missing != 0)
            Fail("out parameter");

        int inValue = RuntimeValue(4);
        Coordinate inCoordinate = new Coordinate { X = RuntimeValue(5), Y = RuntimeValue(6) };
        int outValue;
        int outDouble;
        WriteOut(out outValue, out outDouble);
        if (ReadIn(in inValue) != RuntimeValue(4) ||
            ForwardIn(in inValue) != RuntimeValue(8) ||
            ReadCoordinate(in inCoordinate) != RuntimeValue(11) ||
            first != RuntimeValue(3) || second != RuntimeValue(4) ||
            third != RuntimeValue(3) || outValue != RuntimeValue(7) ||
            outDouble != RuntimeValue(14))
            Fail("ref or in parameter");

        int[] refValues = new int[] { RuntimeValue(8), RuntimeValue(9), RuntimeValue(10) };
        ref int refElement = ref GetElement(refValues, 1);
        Increment(ref refElement);
        ref int sameElement = ref GetElement(refValues, 1);
        if (refElement != RuntimeValue(10) || sameElement != RuntimeValue(10) ||
            refValues[1] != RuntimeValue(10))
            Fail("ref alias or ref return");

        FeatureObject original = new FeatureObject(RuntimeValue(1));
        FeatureObject replacement = new FeatureObject(RuntimeValue(2));
        ReplaceReference(ref original, replacement);
        if (original != replacement || original.Value != RuntimeValue(3))
            Fail("ref reference value");

        sbyte signedByteReference = (sbyte)RuntimeValue(-7);
        ushort unsignedShortReference = (ushort)RuntimeValue(60000);
        uint unsignedIntegerReference = 0xf0000000u;
        nint nativeReference = RuntimeValue(15);
        object objectReference = replacement;
        WriteNativeReference(ref nativeReference, RuntimeValue(16));
        if (ReadSignedByteReference(ref signedByteReference) != (sbyte)RuntimeValue(-7))
            Fail("indirect signed byte load");
        if (ReadUnsignedShortReference(ref unsignedShortReference) != (ushort)RuntimeValue(60000))
            Fail("indirect unsigned short load");
        if (ReadUnsignedIntegerReference(ref unsignedIntegerReference) != 0xf0000000u)
            Fail("indirect unsigned integer load");
        if (ReadNativeReference(ref nativeReference) != RuntimeValue(16))
            Fail("indirect native integer load or store");
        if (ReadObjectReference(ref objectReference) != replacement)
            Fail("indirect reference load");

        int* stackValues = stackalloc int[2];
        stackValues[0] = RuntimeValue(6);
        stackValues[1] = RuntimeValue(7);
        if (stackValues[0] + stackValues[1] != RuntimeValue(13))
            Fail("stackalloc");

        fixed (int* pinned = values)
        {
            if (pinned[0] != RuntimeValue(1) || sizeof(int) != RuntimeValue(4))
                Fail("fixed or sizeof");
        }

        int localFirst = 1;
        int localSecond = 2;
        int* firstPointer = &localFirst;
        int* secondPointer = &localSecond;
        if (*firstPointer != 1 || *secondPointer != 2)
            Fail("local pointer load");

        *firstPointer = 3;
        WritePointer(secondPointer, 4);
        if (localFirst != 3 || localSecond != 4 || ReadPointer(firstPointer) != 3)
            Fail("local pointer store or parameter");

        int* returnedPointer = ReturnPointer(firstPointer);
        if (returnedPointer != firstPointer || *returnedPointer != 3)
            Fail("pointer return or equality");

        int[] pointerValues = new int[] { 5, 6, 7, 8 };
        fixed (int* pinnedValues = pointerValues)
        {
            int* secondValue = pinnedValues + 1;
            if (pinnedValues[0] != 5 || *secondValue != 6 || *(secondValue + 1) != 7)
                Fail("pointer indexing or addition");
            *(pinnedValues + 2) = 9;
            secondValue++;
            if (pointerValues[2] != 9 || *secondValue != 9)
                Fail("pointer increment or write");
            secondValue -= 1;
            if (secondValue != pinnedValues + 1 || *secondValue != 6)
                Fail("pointer subtraction");

            void* raw = pinnedValues;
            int* converted = (int*)raw;
            if (converted != pinnedValues || converted[3] != 8)
                Fail("pointer conversion");
        }

        PointerValues fields = new PointerValues();
        fields.Integer = 10;
        fields.Short = 11;
        int* integerField = &fields.Integer;
        short* shortField = &fields.Short;
        *integerField += 2;
        *shortField = 12;
        if (fields.Integer != 12 || fields.Short != 12)
            Fail("struct field pointer");

        byte byteValue = 13;
        short shortValue = 14;
        long longValue = 15;
        float floatValue = 16;
        double doubleValue = 17;
        byte* bytePointer = &byteValue;
        short* shortPointer = &shortValue;
        long* longPointer = &longValue;
        float* floatPointer = &floatValue;
        double* doublePointer = &doubleValue;
        *bytePointer = 18;
        *shortPointer = 19;
        *longPointer = 20;
        *floatPointer = 21;
        *doublePointer = 22;
        if (*bytePointer != 18 || *shortPointer != 19 || *longPointer != 20 ||
            *floatPointer != 21 || *doublePointer != 22)
            Fail("primitive pointer load or store");

        int pointedValue = 23;
        int* pointedValuePointer = &pointedValue;
        int** pointerPointer = &pointedValuePointer;
        if (**pointerPointer != 23)
            Fail("pointer to pointer load");
        **pointerPointer = 24;
        if (pointedValue != 24)
            Fail("pointer to pointer store");

        int* stackPointerValues = stackalloc int[3];
        stackPointerValues[0] = 25;
        stackPointerValues[1] = 26;
        stackPointerValues[2] = 27;
        if (SumPointerValues(stackPointerValues, 3) != 78)
            Fail("stack pointer parameter");

        delegate* managed<int, int> functionPointer = &DoublePointerValue;
        if (functionPointer(14) != 28)
            Fail("managed function pointer");

        if (sizeof(byte) != 1 || sizeof(short) != 2 || sizeof(int) != 4 ||
            sizeof(long) != 8 || sizeof(float) != 4 || sizeof(double) != 8 ||
            sizeof(void*) != sizeof(int*))
            Fail("pointer sizeof");

        int checkedValue = checked(RuntimeValue(1) + RuntimeValue(2));
        int uncheckedValue = unchecked(int.MaxValue + RuntimeValue(1));
        if (checkedValue != RuntimeValue(3) || uncheckedValue != int.MinValue)
            Fail("checked or unchecked");
    }

    private static void VerifyExceptionsAndResources()
    {
        bool caught = false;
        s_filterMode = 0;
        try
        {
            throw new InvalidOperationException("Validation exception used to test exception filters.");
        }
        catch (Exception) when (RejectExceptionFilter())
        {
            Fail("exception filter ordering");
        }
        catch (Exception) when (AcceptExceptionFilter())
        {
            caught = true;
        }
        finally
        {
            s_volatileValue++;
        }

        DisposableFeature resource;
        using (resource = new DisposableFeature())
        {
            if (resource.IsDisposed)
                Fail("using");
        }

        lock (s_lock)
        {
            s_volatileValue++;
        }

        if (!caught || !resource.IsDisposed)
            Fail("try, catch, finally, or using");

        Exception expected = new Exception("rethrow validation");
        Exception rethrown = null;
        try
        {
            RethrowException(expected);
        }
        catch (Exception exception)
        {
            rethrown = exception;
        }

        if (rethrown != expected)
            Fail("exception rethrow");

        if (CatchSameFrameRethrow(expected) != expected)
            Fail("same-frame exception rethrow");

        Exception nestedRethrown = null;
        try
        {
            RethrowAfterNestedCatch(expected, new Exception("nested exception"));
        }
        catch (Exception exception)
        {
            nestedRethrown = exception;
        }

        if (nestedRethrown != expected)
            Fail("nested exception rethrow");

        int nestedFinallyState = 0;
        if (ReturnThroughNestedFinally(ref nestedFinallyState) != RuntimeValue(7) ||
            nestedFinallyState != RuntimeValue(123))
            Fail("nested finally or leave");

        bool invalidCastCaught = false;
        try
        {
            object value = HideObject(new FeatureObject(RuntimeValue(1)));
            AlternateHierarchyFeature invalid = (AlternateHierarchyFeature)value;
            if (invalid.SecondaryValue == RuntimeValue(1))
                Fail("invalid cast");
        }
        catch (InvalidCastException)
        {
            invalidCastCaught = true;
        }

        if (!invalidCastCaught)
            Fail("invalid cast exception");
    }

    private static int ReturnThroughNestedFinally(ref int state)
    {
        try
        {
            try
            {
                state = RuntimeValue(1);
                return RuntimeValue(7);
            }
            finally
            {
                state = state * RuntimeValue(10) + RuntimeValue(2);
            }
        }
        finally
        {
            state = state * RuntimeValue(10) + RuntimeValue(3);
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void RethrowException(Exception exception)
    {
        try
        {
            throw exception;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static Exception CatchSameFrameRethrow(Exception exception)
    {
        Exception caught = null;
        try
        {
            try
            {
                throw exception;
            }
            catch (Exception)
            {
                throw;
            }
        }
        catch (Exception rethrown)
        {
            caught = rethrown;
        }
        return caught;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void RethrowAfterNestedCatch(Exception outer, Exception inner)
    {
        try
        {
            throw outer;
        }
        catch (Exception)
        {
            try
            {
                throw inner;
            }
            catch (Exception)
            {
            }
            throw;
        }
    }

    private static async Task VerifyAsync()
    {
        int result = await Task.FromResult(RuntimeValue(7));
        if (result != RuntimeValue(7))
            Fail("async or await");

        TaskCompletionSource<int> completion = new TaskCompletionSource<int>();
        Task<int> pending = AwaitPending(completion.Task);
        if (pending.IsCompleted)
            Fail("pending async state");

        completion.SetResult(RuntimeValue(10));
        if (pending.GetAwaiter().GetResult() != RuntimeValue(11))
            Fail("async continuation");

        Exception expected = new Exception("async exception validation");
        TaskCompletionSource failedCompletion = new TaskCompletionSource();
        Task<bool> recovered = CatchPendingException(failedCompletion.Task, expected);
        if (recovered.IsCompleted)
            Fail("pending async exception state");

        failedCompletion.SetException(expected);
        if (!recovered.GetAwaiter().GetResult())
            Fail("async exception");

        int count = 0;
        int total = 0;
        foreach (int value in YieldValues())
        {
            count++;
            total += value;
        }
        if (count != RuntimeValue(2) || total != RuntimeValue(3))
            Fail("yield");
    }

    private static async Task<int> AwaitPending(Task<int> task)
    {
        int value = await task.ConfigureAwait(false);
        return value + RuntimeValue(1);
    }

    private static async Task<bool> CatchPendingException(Task task, Exception expected)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception actual)
        {
            return actual == expected;
        }

        return false;
    }

    private static IEnumerable<int> YieldValues()
    {
        yield return RuntimeValue(1);
        yield return RuntimeValue(2);
    }

    private static int Sum(params int[] values)
    {
        int result = 0;
        for (int index = 0; index < values.Length; index++)
            result += values[index];
        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int RuntimeValue(int value)
    {
        return value + s_runtimeBias - 1;
    }

    private static bool SetVolatileAndReturnTrue()
    {
        s_volatileValue++;
        return true;
    }

    private static T IdentityGeneric<T>(T value) => value;

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static object HideObject(object value) => value;

    private static T DefaultGeneric<T>() => default;

    private static void Swap<T>(ref T left, ref T right)
    {
        T value = left;
        left = right;
        right = value;
    }

    private static void Increment(ref int value) => value++;

    private static int ReadIn(in int value) => value;

    private static int ForwardIn(in int value) => ReadIn(in value) + value;

    private static int ReadCoordinate(in Coordinate value) => value.X + value.Y;

    private static void AddThroughReferences(ref int left, ref int right)
    {
        left++;
        right += RuntimeValue(2);
    }

    private static void WriteOut(out int value, out int doubled)
    {
        value = RuntimeValue(7);
        doubled = value * 2;
    }

    private static void ReplaceReference(ref FeatureObject value, FeatureObject replacement)
        => value = replacement;

    private static sbyte ReadSignedByteReference(ref sbyte value) => value;

    private static ushort ReadUnsignedShortReference(ref ushort value) => value;

    private static uint ReadUnsignedIntegerReference(ref uint value) => value;

    private static nint ReadNativeReference(ref nint value) => value;

    private static object ReadObjectReference(ref object value) => value;

    private static void WriteNativeReference(ref nint value, nint replacement) => value = replacement;

    private static Coordinate ChangeCoordinate(Coordinate value)
    {
        value.X += RuntimeValue(9);
        return value;
    }

    private static unsafe void WritePointer(int* pointer, int value) => *pointer = value;

    private static unsafe int ReadPointer(int* pointer) => *pointer;

    private static unsafe int* ReturnPointer(int* pointer) => pointer;

    private static unsafe int SumPointerValues(int* values, int length)
    {
        int result = 0;
        for (int index = 0; index < length; index++)
            result += values[index];
        return result;
    }

    private static int DoublePointerValue(int value) => value * 2;

    private static int Identity(this int value) => value;

    private static bool TryRead(int[] values, int index, out int value)
    {
        if ((uint)index < (uint)values.Length)
        {
            value = values[index];
            return true;
        }

        value = default;
        return false;
    }

    private static bool RejectExceptionFilter() => s_filterMode != 0;

    private static bool AcceptExceptionFilter() => s_filterMode == 0;

    private static void Fail(string feature)
    {
        throw new Exception("Language feature validation failed: " + feature);
    }
private static readonly object s_extendedLock = new object();
    private static volatile int s_extendedVolatile;
    private static int s_partialMethodValue;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PackedFeature
    {
        public byte Prefix;
        public int Value;
        public short Suffix;
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    private struct ExplicitFeature
    {
        [FieldOffset(0)]
        public int Integer;

        [FieldOffset(0)]
        public uint UnsignedInteger;

        [FieldOffset(4)]
        public short Tail;
    }

    private unsafe struct FixedBufferFeature
    {
        public fixed byte Bytes[5];
    }

    private ref struct RefBuffer
    {
        private Span<int> _values;

        public RefBuffer(Span<int> values)
        {
            _values = values;
        }

        public ref int this[int index] => ref _values[index];
        public int Length => _values.Length;
    }

    private readonly ref struct ReadOnlyRefBuffer
    {
        private readonly ReadOnlySpan<int> _values;

        public ReadOnlyRefBuffer(ReadOnlySpan<int> values)
        {
            _values = values;
        }

        public ref readonly int this[int index] => ref _values[index];
        public int Length => _values.Length;
    }

    private record class RecordFeature(int Value, string Name);

    private readonly record struct RecordValue(int X, int Y)
    {
        public int Sum => X + Y;
    }

    private sealed class PatternFeature
    {
        public PatternFeature(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public int Value { get; }
        public string Name { get; }
        public void Deconstruct(out int value, out string name)
        {
            value = Value;
            name = Name;
        }
    }

    private sealed class PatternList
    {
        private readonly int[] _values;

        public PatternList(int[] values)
        {
            _values = values;
        }

        public int Count => _values.Length;
        public int this[int index] => _values[index];
    }

    private interface IDefaultInterfaceFeature
    {
        int Value { get; }
        int DoubleValue() => Value * 2;
    }

    private sealed class DefaultInterfaceFeature : IDefaultInterfaceFeature
    {
        public DefaultInterfaceFeature(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    private interface IStaticFeature<TSelf> where TSelf : IStaticFeature<TSelf>
    {
        static abstract int Combine(TSelf left, TSelf right);
    }

    private readonly struct StaticFeature : IStaticFeature<StaticFeature>
    {
        public StaticFeature(int value)
        {
            Value = value;
        }

        public int Value { get; }
        public static int Combine(StaticFeature left, StaticFeature right) => left.Value + right.Value;
    }

    private interface ICovariantFeature<out T>
    {
        T GetValue();
    }

    private interface IContravariantFeature<in T>
    {
        int GetValue(T value);
    }

    private sealed class VariantFeature<T> : ICovariantFeature<T>, IContravariantFeature<T>
    {
        private readonly T _value;

        public VariantFeature(T value)
        {
            _value = value;
        }

        public T GetValue() => _value;
        public int GetValue(T value) => object.Equals(value, _value) ? 1 : 0;
    }

    private abstract class CovariantReturnBase
    {
        public abstract FeatureBase Create(int value);
    }

    private sealed class CovariantReturnDerived : CovariantReturnBase
    {
        public override FeatureObject Create(int value) => new FeatureObject(value);
    }

    private sealed class NewConstraintFeature
    {
        public NewConstraintFeature() { }
        public int Value = 17;
    }

    private class GenericMethodFeature
    {
        public T Identity<T>(T value) => value;
        public virtual int Evaluate<T>(T value) where T : IFeatureValue => value.Value;
    }

    private sealed class GenericMethodDerived : GenericMethodFeature
    {
        public override int Evaluate<T>(T value) => value.Value + 1;
    }

    private sealed class DisposablePair : IDisposable
    {
        private readonly int _digit;

        public DisposablePair(int digit)
        {
            _digit = digit;
        }

        public void Dispose()
        {
            s_partialMethodValue = s_partialMethodValue * 10 + _digit;
        }
    }

    private static void ApplyPartialMethod(ref int value)
    {
        value += RuntimeValue(3);
    }

    private static void VerifyExtendedLanguageFeatures()
    {
        VerifyStructureLayouts();
        VerifyIndirectMemoryOperations();
        VerifyRecordsAndPatterns();
        VerifyInterfacesAndGenericConstraints();
        VerifyRefLikeTypes();
        VerifyAdditionalStatementsAndExpressions();
        VerifyGeneratedPointerMatrix();
    }

    private static IntPtr IdentityIntPtr(IntPtr value) => value;

    private static UIntPtr IdentityUIntPtr(UIntPtr value) => value;

    private static NativeHandlePair IdentityNativeHandlePair(NativeHandlePair value) => value;

    private static IntPtr ReadIntPtr(in IntPtr value) => value;

    private static void WriteIntPtr(ref IntPtr destination, IntPtr value) => destination = value;

    private static void CreateIntPtr(out IntPtr value) => value = new IntPtr(3);

    private static unsafe IntPtr EchoPointer(void* value) => new IntPtr(value);

    [UnmanagedCallersOnly]
    private static IntPtr UnmanagedIdentityIntPtr(IntPtr value) => value;

    [UnmanagedCallersOnly]
    private static UIntPtr UnmanagedIdentityUIntPtr(UIntPtr value) => value;

    private static unsafe void* EchoRawPointer(void* value) => value;

    [UnmanagedCallersOnly]
    private static unsafe void* UnmanagedEchoRawPointer(void* value) => value;

    private static void SetFunctionMarker() => s_instructionStatic++;

    [UnmanagedCallersOnly]
    private static void UnmanagedSetFunctionMarker() => s_instructionStatic++;

    private static unsafe void VerifyComprehensiveNativeValues()
    {
        IntPtr zero = default;
        IntPtr one = new IntPtr(RuntimeValue(1));
        IntPtr two = (IntPtr)RuntimeValue(2);
        IntPtr wide = new IntPtr(unchecked((long)0x1234567887654321UL));
        IntPtr copied = IdentityIntPtr(one);
        IntPtr fromIn = ReadIntPtr(in copied);
        IntPtr fromOut;
        CreateIntPtr(out fromOut);

        if ((long)zero != 0 || (long)one != RuntimeValue(1) ||
            (long)two != RuntimeValue(2) ||
            (long)wide != unchecked((long)0x1234567887654321UL) ||
            (long)copied != RuntimeValue(1) || (long)fromIn != RuntimeValue(1) ||
            (long)fromOut != RuntimeValue(3) || one == zero || one != copied)
            Fail("IntPtr constructors, equality, or value flow");

        unsafe
        {
            void* raw = (void*)0x1234;
            IntPtr fromPointer = new IntPtr(raw);
            IntPtr explicitPointer = (IntPtr)raw;
            void* restored = (void*)fromPointer;
            void* restoredExplicit = (void*)explicitPointer;
            void* rawResult = EchoRawPointer(raw);
            IntPtr pointerResult = EchoPointer(raw);
            IntPtr* slots = stackalloc IntPtr[2];
            slots[0] = fromPointer;
            slots[1] = explicitPointer;
            IntPtr** slotAddress = &slots;
            delegate* managed<void*, void*> rawFunction = &EchoRawPointer;
            delegate* managed<IntPtr, IntPtr> managedFunction = &IdentityIntPtr;
            delegate* unmanaged<IntPtr, IntPtr> unmanagedFunction = &UnmanagedIdentityIntPtr;
            delegate* unmanaged<void*, void*> unmanagedRawFunction = &UnmanagedEchoRawPointer;
            delegate* managed<void> managedVoidFunction = &SetFunctionMarker;
            delegate* unmanaged<void> unmanagedVoidFunction = &UnmanagedSetFunctionMarker;
            int marker = s_instructionStatic;
            managedVoidFunction();
            unmanagedVoidFunction();

            if (restored != raw || restoredExplicit != raw || rawResult != raw ||
                (long)pointerResult != RuntimeValue(0x1234) ||
                (long)slots[0] != RuntimeValue(0x1234) ||
                (long)(*slotAddress)[1] != RuntimeValue(0x1234) ||
                rawFunction(raw) != raw || managedFunction(one) != one ||
                unmanagedFunction(two) != two || unmanagedRawFunction(raw) != raw ||
                s_instructionStatic != marker + RuntimeValue(2))
                Fail("IntPtr and void pointer conversions or function pointers");
        }

        IntPtr replacement = zero;
        WriteIntPtr(ref replacement, wide);
        NativeHandlePair pair = new NativeHandlePair(one, default, RuntimeValue(7));
        NativeHandlePair pairCopy = IdentityNativeHandlePair(pair);
        NativeHandlePair[] pairs = new NativeHandlePair[2];
        pairs[0] = pair;
        pairs[1] = pairCopy;
        ref NativeHandlePair pairReference = ref pairs[1];
        pairReference.Tag = RuntimeValue(8);
        if ((long)replacement != unchecked((long)0x1234567887654321UL) ||
            (long)pairCopy.Signed != RuntimeValue(1) ||
            pairCopy.Unsigned.ToString() != "0" || pairCopy.Tag != RuntimeValue(7) ||
            (long)pairs[0].Signed != RuntimeValue(1) || pairs[1].Tag != RuntimeValue(8))
            Fail("IntPtr struct fields, arrays, or ref alias");

        UIntPtr unsignedZero = default;
        UIntPtr unsignedCopy = IdentityUIntPtr(unsignedZero);
        UIntPtr? nullableUnsigned = unsignedZero;
        UIntPtr? nullableMissing = null;
        object boxedUnsigned = unsignedZero;
        UIntPtr unboxedUnsigned = (UIntPtr)boxedUnsigned;
        delegate* managed<UIntPtr, UIntPtr> unsignedFunction = &IdentityUIntPtr;
        delegate* unmanaged<UIntPtr, UIntPtr> unmanagedUnsignedFunction = &UnmanagedIdentityUIntPtr;
        if (unsignedZero.ToString() != "0" || unsignedCopy.ToString() != "0" ||
            !nullableUnsigned.HasValue || nullableUnsigned.Value.ToString() != "0" ||
            nullableMissing.HasValue || unboxedUnsigned.ToString() != "0" ||
            unsignedFunction(unsignedZero).ToString() != "0" ||
            unmanagedUnsignedFunction(unsignedZero).ToString() != "0")
            Fail("UIntPtr default, nullable, boxing, or function pointer flow");

        IntPtr? nullableOne = one;
        IntPtr? nullableMissingIntPtr = null;
        object boxedIntPtr = nullableOne;
        IntPtr unboxedIntPtr = (IntPtr)boxedIntPtr;
        IntPtr genericIntPtr = IdentityGeneric(one);
        IntPtr? genericNullable = IdentityGeneric(nullableOne);
        if (!nullableOne.HasValue || (long)nullableOne.Value != RuntimeValue(1) ||
            nullableMissingIntPtr.HasValue || (long)unboxedIntPtr != RuntimeValue(1) ||
            (long)genericIntPtr != RuntimeValue(1) ||
            !genericNullable.HasValue || (long)genericNullable.Value != RuntimeValue(1))
            Fail("IntPtr nullable, boxing, or generic flow");

        int[] indexedValues = new int[] { RuntimeValue(4), RuntimeValue(5), RuntimeValue(6) };
        if (indexedValues[^1] != RuntimeValue(6) || indexedValues[^3] != RuntimeValue(4))
            Fail("Index-from-end array access");

        unsafe
        {
            IntPtr pointerValue = new IntPtr((void*)0x55);
            IntPtr* pointer = &pointerValue;
            IntPtr** pointerToPointer = &pointer;
            if ((long)(*pointer) != RuntimeValue(0x55) ||
                (long)(**pointerToPointer) != RuntimeValue(0x55))
                Fail("IntPtr pointer indirection");
        }
    }

    private static void VerifyAllPrimitiveNullableValues()
    {
        bool? boolean = true;
        bool? missingBoolean = null;
        char? character = 'Q';
        char? missingCharacter = null;
        sbyte? signedByte = -7;
        byte? unsignedByte = 250;
        short? signedShort = -30000;
        ushort? unsignedShort = 60000;
        int? signedInteger = RuntimeValue(123456);
        uint? unsignedInteger = 3456789012u;
        long? signedLong = -1234567890123L;
        ulong? unsignedLong = 12345678901234567890UL;
        nint? nativeInteger = (nint)0x12345678;
        nuint? nativeUnsigned = (nuint)0x87654321u;
        float? single = 12.5f;
        double? precision = -25.25;
        FeatureValue? value = new FeatureValue(RuntimeValue(9));
        FeatureValue? missingValue = null;

        if (!boolean.HasValue || !boolean.Value || missingBoolean.HasValue ||
            character.Value != 'Q' || missingCharacter.GetValueOrDefault('Z') != 'Z' ||
            signedByte.Value != -7 || unsignedByte.Value != 250 ||
            signedShort.Value != -30000 || unsignedShort.Value != 60000 ||
            signedInteger.Value != RuntimeValue(123456) || unsignedInteger.Value != 3456789012u ||
            signedLong.Value != -1234567890123L || unsignedLong.Value != 12345678901234567890UL ||
            nativeInteger.Value != (nint)0x12345678 || nativeUnsigned.Value != (nuint)0x87654321u ||
            single.Value != 12.5f || precision.Value != -25.25 ||
            !value.HasValue || value.Value.Value != RuntimeValue(9) || missingValue.HasValue)
            Fail("nullable values for all primitive and unmanaged types");

        int lifted = (signedInteger ?? 0) + (int)(unsignedByte ?? 0);
        bool liftedComparison = signedInteger < (int?)null;
        if (lifted != RuntimeValue(123706) || liftedComparison ||
            (missingBoolean ?? false) || (boolean ?? false) != true)
            Fail("nullable lifted operators and coalescing");

        int?[] nullableArray = new int?[] { null, RuntimeValue(2), RuntimeValue(3) };
        nullableArray[0] ??= RuntimeValue(1);
        if (nullableArray[0] != RuntimeValue(1) ||
            nullableArray[1].GetValueOrDefault() != RuntimeValue(2) ||
            nullableArray[2].Value != RuntimeValue(3))
            Fail("nullable array elements and assignment");

        unsafe
        {
            if (sizeof(sbyte) != 1 || sizeof(byte) != 1 || sizeof(short) != 2 ||
                sizeof(ushort) != 2 || sizeof(char) != 2 || sizeof(bool) != 1 ||
                sizeof(int) != 4 || sizeof(uint) != 4 || sizeof(float) != 4 ||
                sizeof(long) != 8 || sizeof(ulong) != 8 || sizeof(double) != 8 ||
                sizeof(nint) != sizeof(void*) || sizeof(nuint) != sizeof(void*) ||
                sizeof(IntPtr) != sizeof(void*))
                Fail("primitive and native type sizes");
        }
    }

    private static void VerifyStructureLayouts()
    {
        PackedFeature packed = new PackedFeature
        {
            Prefix = (byte)RuntimeValue(1),
            Value = RuntimeValue(0x12345678),
            Suffix = (short)RuntimeValue(0x2345),
        };
        if (packed.Prefix != 1 || packed.Value != RuntimeValue(0x12345678) || packed.Suffix != 0x2345)
            Fail("packed sequential struct fields");

        ExplicitFeature explicitValue = new ExplicitFeature
        {
            UnsignedInteger = 0x89abcdefu,
            Tail = (short)RuntimeValue(0x1234),
        };
        if (explicitValue.Integer != unchecked((int)0x89abcdefu) || explicitValue.Tail != 0x1234)
            Fail("explicit layout overlapping fields");
    }

    private static unsafe void VerifyIndirectMemoryOperations()
    {
        sbyte signedByte = 0;
        byte unsignedByte = 0;
        short signedShort = 0;
        ushort unsignedShort = 0;
        int signedInteger = 0;
        uint unsignedInteger = 0;
        long signedLong = 0;
        ulong unsignedLong = 0;
        char character = '\0';
        bool boolean = false;
        float single = 0;
        double precision = 0;
        nint nativeInteger = 0;
        nuint nativeUnsigned = 0;

        WriteIndirect(&signedByte, (sbyte)-7);
        WriteIndirect(&unsignedByte, (byte)250);
        WriteIndirect(&signedShort, (short)-30000);
        WriteIndirect(&unsignedShort, (ushort)60000);
        WriteIndirect(&signedInteger, -123456789);
        WriteIndirect(&unsignedInteger, 0xf1234567u);
        WriteIndirect(&signedLong, -0x1234567890L);
        WriteIndirect(&unsignedLong, 0xf123456789abcdefUL);
        WriteIndirect(&character, 'Z');
        WriteIndirect(&boolean, true);
        WriteIndirect(&single, 12.5f);
        WriteIndirect(&precision, -25.25);
        WriteIndirect(&nativeInteger, (nint)0x12345678);
        WriteIndirect(&nativeUnsigned, (nuint)0x87654321u);

        if (ReadIndirect(&signedByte) != -7 || ReadIndirect(&unsignedByte) != 250 ||
            ReadIndirect(&signedShort) != -30000 || ReadIndirect(&unsignedShort) != 60000 ||
            ReadIndirect(&signedInteger) != -123456789 || ReadIndirect(&unsignedInteger) != 0xf1234567u ||
            ReadIndirect(&signedLong) != -0x1234567890L ||
            ReadIndirect(&unsignedLong) != 0xf123456789abcdefUL ||
            ReadIndirect(&character) != 'Z' || !ReadIndirect(&boolean) ||
            ReadIndirect(&single) != 12.5f || ReadIndirect(&precision) != -25.25 ||
            ReadIndirect(&nativeInteger) != (nint)0x12345678 ||
            ReadIndirect(&nativeUnsigned) != (nuint)0x87654321u)
            Fail("all indirect primitive loads and stores");

        int[] pointerValues = new int[] { RuntimeValue(3), RuntimeValue(4), RuntimeValue(5) };
        fixed (int* first = pointerValues)
        {
            int* second = first + 1;
            if (*second != RuntimeValue(4) || second - first != 1 || !(second > first) || first >= second)
                Fail("pointer arithmetic or comparison");
        }

        string text = "fixed";
        fixed (char* textPointer = text)
        {
            if (textPointer == null || textPointer[0] != 'f' || textPointer[4] != 'd')
                Fail("fixed string");
        }

        ReadOnlySpan<byte> utf8 = "span"u8;
        fixed (byte* utf8Pointer = utf8)
        {
            if (utf8Pointer == null || utf8Pointer[0] != (byte)'s' || utf8Pointer[3] != (byte)'n')
                Fail("fixed readonly span");
        }

        FixedBufferFeature fixedBuffer = default;
        for (int index = 0; index < 5; index++)
            fixedBuffer.Bytes[index] = (byte)(index + 1);
        if (fixedBuffer.Bytes[0] != 1 || fixedBuffer.Bytes[4] != 5)
            Fail("fixed size buffer");
    }

    private static void VerifyRecordsAndPatterns()
    {
        RecordFeature first = new RecordFeature(RuntimeValue(7), "record");
        RecordFeature equal = new RecordFeature(RuntimeValue(7), "record");
        RecordFeature changed = first with { Value = RuntimeValue(9) };
        RecordValue value = new RecordValue(RuntimeValue(2), RuntimeValue(3));
        RecordValue changedValue = value with { Y = RuntimeValue(8) };
        var anonymous = new { Value = RuntimeValue(4), Name = "anonymous" };
        var anonymousEqual = new { Value = RuntimeValue(4), Name = "anonymous" };

        var (recordX, recordY) = value;
        if (first != equal)
        {
            Fail("records, with expressions, deconstruction, or anonymous types");
        }
        if (first == changed)
            Fail("records, with expressions, deconstruction, or anonymous types");
        if (changed.Value != RuntimeValue(9))
            Fail("records, with expressions, deconstruction, or anonymous types");
        if (value.Sum != RuntimeValue(5))
            Fail("records, with expressions, deconstruction, or anonymous types");
        if (changedValue.Sum != RuntimeValue(10))
            Fail("records, with expressions, deconstruction, or anonymous types");
        if (recordX != RuntimeValue(2))
            Fail("records, with expressions, deconstruction, or anonymous types");
        if (recordY != RuntimeValue(3))
            Fail("records, with expressions, deconstruction, or anonymous types");
        if (!anonymous.Equals(anonymousEqual))
            Fail("records, with expressions, deconstruction, or anonymous types");
        if (anonymous.Value != RuntimeValue(4))
            Fail("records, with expressions, deconstruction, or anonymous types");

        object candidate = new PatternFeature(RuntimeValue(6), "pattern");
        PatternList list = new PatternList(new int[] { RuntimeValue(1), RuntimeValue(2), RuntimeValue(3) });
        int patternValue = candidate switch
        {
            PatternFeature { Value: > 5 and < 10, Name: "pattern" } feature => feature.Value,
            null => -1,
            _ => 0,
        };
        bool positional = candidate is PatternFeature(6, "pattern");
        bool listPattern = list is [1, 2, 3];
        bool typePattern = candidate is PatternFeature typed && typed.Name is not null;
        if (patternValue != RuntimeValue(6) || !positional || !listPattern || !typePattern)
            Fail("recursive, relational, logical, positional, list, or type patterns");
    }

    private static void VerifyInterfacesAndGenericConstraints()
    {
        IDefaultInterfaceFeature defaultFeature = new DefaultInterfaceFeature(RuntimeValue(8));
        if (defaultFeature.DoubleValue() != RuntimeValue(16))
            Fail("default interface method");

        int staticResult = CombineStatic(new StaticFeature(RuntimeValue(4)), new StaticFeature(RuntimeValue(5)));
        if (staticResult != RuntimeValue(9))
            Fail("static abstract interface method");

        VariantFeature<FeatureObject> variant = new VariantFeature<FeatureObject>(new FeatureObject(RuntimeValue(4)));
        ICovariantFeature<FeatureBase> covariant = variant;
        IContravariantFeature<FeatureObject> contravariant = new VariantFeature<FeatureBase>(variant.GetValue());
        if (covariant.GetValue().Evaluate() != RuntimeValue(5) ||
            contravariant.GetValue(variant.GetValue()) != RuntimeValue(1))
            Fail("generic interface variance");

        CovariantReturnBase factory = new CovariantReturnDerived();
        if (factory.Create(RuntimeValue(10)).Evaluate() != RuntimeValue(11))
            Fail("covariant return");

        GenericMethodFeature generic = new GenericMethodDerived();
        FeatureValue featureValue = new FeatureValue(RuntimeValue(12));
        if (generic.Identity(RuntimeValue(6)) != RuntimeValue(6))
            Fail("generic methods or constraints");
        if (generic.Identity("generic") != "generic")
            Fail("generic methods or constraints");
        int genericEvaluation = generic.Evaluate(featureValue);
        if (genericEvaluation != RuntimeValue(13))
            Fail("generic methods or constraints");
        if (CreateWithNewConstraint<NewConstraintFeature>().Value != RuntimeValue(17))
            Fail("generic methods or constraints");
        if (ReadStructConstraint(featureValue) != RuntimeValue(12))
            Fail("generic methods or constraints");
        if (ReadClassConstraint(variant.GetValue()) != RuntimeValue(5))
            Fail("generic methods or constraints");
        if (SizeOfUnmanaged<int>() != sizeof(int))
            Fail("generic methods or constraints");
    }

    private static void VerifyRefLikeTypes()
    {
        Span<int> storage = stackalloc int[] { RuntimeValue(2), RuntimeValue(4), RuntimeValue(6) };
        RefBuffer writable = new RefBuffer(storage);
        ref int middle = ref writable[1];
        middle += RuntimeValue(3);
        ReadOnlyRefBuffer readOnly = new ReadOnlyRefBuffer(storage);
        ref readonly int last = ref readOnly[2];
        ref int selected = ref SelectReference(RuntimeValue(0) != 0, ref storage[0], ref storage[2]);
        selected += RuntimeValue(5);

        if (writable.Length != RuntimeValue(3))
            Fail("ref struct, readonly ref struct, ref readonly, or conditional ref");
        if (writable[1] != RuntimeValue(7))
            Fail("ref struct, readonly ref struct, ref readonly, or conditional ref");
        if (readOnly.Length != RuntimeValue(3))
            Fail("ref struct, readonly ref struct, ref readonly, or conditional ref");
        if (last != RuntimeValue(11))
            Fail("ref struct, readonly ref struct, ref readonly, or conditional ref");
        if (storage[2] != RuntimeValue(11))
            Fail("ref struct, readonly ref struct, ref readonly, or conditional ref");
    }

    private static void VerifyAdditionalStatementsAndExpressions()
    {
        s_extendedVolatile = RuntimeValue(1);
        lock (s_extendedLock)
        {
            s_extendedVolatile += RuntimeValue(2);
        }
        if (s_extendedVolatile != RuntimeValue(3))
            Fail("volatile field or lock statement");

        int partialValue = RuntimeValue(4);
        ApplyPartialMethod(ref partialValue);
        if (partialValue != RuntimeValue(7))
            Fail("partial method");

        s_partialMethodValue = 0;
        using (DisposablePair first = new DisposablePair(RuntimeValue(1)))
        using (DisposablePair second = new DisposablePair(RuntimeValue(2)))
        {
            if (first == null || second == null)
                Fail("using declaration values");
        }
        if (s_partialMethodValue != RuntimeValue(21))
            Fail("nested using disposal order");

        string missing = null;
        missing ??= "assigned";
        int? nullable = RuntimeValue(5);
        int nullableValue = nullable?.Identity() ?? -1;
        int unsignedShift = -RuntimeValue(8) >>> RuntimeValue(1);
        string raw = """
            raw
            string
            """;
        string interpolated = $"value:{RuntimeValue(3)}:{true}";
        if (missing != "assigned" || nullableValue != RuntimeValue(5) ||
            unsignedShift != 0x7ffffffc || raw != "raw\nstring" ||
            interpolated != "value:3:True")
            Fail("null operators, unsigned shift, or raw string literal");
    }

    private static int CombineStatic<T>(T left, T right) where T : IStaticFeature<T>
        => T.Combine(left, right);

    private static T CreateWithNewConstraint<T>() where T : new() => new T();

    private static int ReadStructConstraint<T>(T value) where T : struct, IFeatureValue => value.Value;

    private static int ReadClassConstraint<T>(T value) where T : FeatureBase => value.Evaluate();

    private static unsafe int SizeOfUnmanaged<T>() where T : unmanaged => sizeof(T);

    private static ref int SelectReference(bool first, ref int left, ref int right)
        => ref first ? ref left : ref right;

    private static unsafe void WriteIndirect(sbyte* pointer, sbyte value) => *pointer = value;
    private static unsafe void WriteIndirect(byte* pointer, byte value) => *pointer = value;
    private static unsafe void WriteIndirect(short* pointer, short value) => *pointer = value;
    private static unsafe void WriteIndirect(ushort* pointer, ushort value) => *pointer = value;
    private static unsafe void WriteIndirect(int* pointer, int value) => *pointer = value;
    private static unsafe void WriteIndirect(uint* pointer, uint value) => *pointer = value;
    private static unsafe void WriteIndirect(long* pointer, long value) => *pointer = value;
    private static unsafe void WriteIndirect(ulong* pointer, ulong value) => *pointer = value;
    private static unsafe void WriteIndirect(char* pointer, char value) => *pointer = value;
    private static unsafe void WriteIndirect(bool* pointer, bool value) => *pointer = value;
    private static unsafe void WriteIndirect(float* pointer, float value) => *pointer = value;
    private static unsafe void WriteIndirect(double* pointer, double value) => *pointer = value;
    private static unsafe void WriteIndirect(nint* pointer, nint value) => *pointer = value;
    private static unsafe void WriteIndirect(nuint* pointer, nuint value) => *pointer = value;

    private static unsafe sbyte ReadIndirect(sbyte* pointer) => *pointer;
    private static unsafe byte ReadIndirect(byte* pointer) => *pointer;
    private static unsafe short ReadIndirect(short* pointer) => *pointer;
    private static unsafe ushort ReadIndirect(ushort* pointer) => *pointer;
    private static unsafe int ReadIndirect(int* pointer) => *pointer;
    private static unsafe uint ReadIndirect(uint* pointer) => *pointer;
    private static unsafe long ReadIndirect(long* pointer) => *pointer;
    private static unsafe ulong ReadIndirect(ulong* pointer) => *pointer;
    private static unsafe char ReadIndirect(char* pointer) => *pointer;
    private static unsafe bool ReadIndirect(bool* pointer) => *pointer;
    private static unsafe float ReadIndirect(float* pointer) => *pointer;
    private static unsafe double ReadIndirect(double* pointer) => *pointer;
    private static unsafe nint ReadIndirect(nint* pointer) => *pointer;
    private static unsafe nuint ReadIndirect(nuint* pointer) => *pointer;

    // <generated-pointer-tests>
    private static unsafe void VerifyGeneratedPointerMatrix()
    {
        VerifyGeneratedScalarPointers();
        VerifyGeneratedPointerCasts();
        VerifyGeneratedFunctionPointers();
        VerifyGeneratedPointerFunctionPointers();
    }

    private static void VerifyGeneratedScalarPointers()
    {
        VerifyScalarPointerSByte();
        VerifyScalarPointerByte();
        VerifyScalarPointerInt16();
        VerifyScalarPointerUInt16();
        VerifyScalarPointerChar();
        VerifyScalarPointerInt32();
        VerifyScalarPointerUInt32();
        VerifyScalarPointerInt64();
        VerifyScalarPointerUInt64();
        VerifyScalarPointerNativeInt();
        VerifyScalarPointerNativeUInt();
        VerifyScalarPointerSingle();
        VerifyScalarPointerDouble();
        VerifyScalarPointerBoolean();
    }

    private static unsafe sbyte ManagedIdentitySByte(sbyte value) => value;
    private static unsafe sbyte ManagedReadSByte(sbyte* value) => *value;
    private static unsafe void ManagedWriteSByte(sbyte* target, sbyte value) => *target = value;
    private static unsafe sbyte* ManagedReturnPointerSByte(sbyte* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe sbyte UnmanagedIdentitySByte(sbyte value) => value;

    private static unsafe void VerifyScalarPointerSByte()
    {
        sbyte value = (sbyte)-11;
        sbyte replacement = (sbyte)-11;
        sbyte* pointer1 = &value;
        sbyte** pointer2 = &pointer1;
        sbyte*** pointer3 = &pointer2;
        sbyte**** pointer4 = &pointer3;
        sbyte***** pointer5 = &pointer4;
        sbyte****** pointer6 = &pointer5;
        sbyte******* pointer7 = &pointer6;
        sbyte******** pointer8 = &pointer7;
        if (********pointer8 != (sbyte)-11)
            Fail("generated sbyte eight-level pointer dereference");
        void* erased = pointer1;
        sbyte* restored = (sbyte*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != (sbyte)-11 || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated sbyte pointer address conversions");
        sbyte* values = stackalloc sbyte[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != (sbyte)-11 || values[3] != (sbyte)-11 || (values + 3) - values != 3)
            Fail("generated sbyte stackalloc indexing and arithmetic");
        delegate* managed<sbyte, sbyte> identity = &ManagedIdentitySByte;
        delegate* managed<sbyte*, sbyte> read = &ManagedReadSByte;
        delegate* managed<sbyte*, sbyte, void> write = &ManagedWriteSByte;
        delegate* managed<sbyte*, sbyte*> returnPointer = &ManagedReturnPointerSByte;
        delegate* unmanaged<sbyte, sbyte> unmanagedIdentity = &UnmanagedIdentitySByte;
        delegate* managed<sbyte, sbyte> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<sbyte, sbyte> restoredFunction = (delegate* managed<sbyte, sbyte>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != (sbyte)-11 || read(pointer1) != (sbyte)-11 || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != (sbyte)-11 || restoredFunction(value) != (sbyte)-11 || nullFunction != null)
            Fail("generated sbyte managed or unmanaged scalar function pointers");
    }

    private static unsafe byte ManagedIdentityByte(byte value) => value;
    private static unsafe byte ManagedReadByte(byte* value) => *value;
    private static unsafe void ManagedWriteByte(byte* target, byte value) => *target = value;
    private static unsafe byte* ManagedReturnPointerByte(byte* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe byte UnmanagedIdentityByte(byte value) => value;

    private static unsafe void VerifyScalarPointerByte()
    {
        byte value = (byte)211;
        byte replacement = (byte)211;
        byte* pointer1 = &value;
        byte** pointer2 = &pointer1;
        byte*** pointer3 = &pointer2;
        byte**** pointer4 = &pointer3;
        byte***** pointer5 = &pointer4;
        byte****** pointer6 = &pointer5;
        byte******* pointer7 = &pointer6;
        byte******** pointer8 = &pointer7;
        if (********pointer8 != (byte)211)
            Fail("generated byte eight-level pointer dereference");
        void* erased = pointer1;
        byte* restored = (byte*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != (byte)211 || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated byte pointer address conversions");
        byte* values = stackalloc byte[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != (byte)211 || values[3] != (byte)211 || (values + 3) - values != 3)
            Fail("generated byte stackalloc indexing and arithmetic");
        delegate* managed<byte, byte> identity = &ManagedIdentityByte;
        delegate* managed<byte*, byte> read = &ManagedReadByte;
        delegate* managed<byte*, byte, void> write = &ManagedWriteByte;
        delegate* managed<byte*, byte*> returnPointer = &ManagedReturnPointerByte;
        delegate* unmanaged<byte, byte> unmanagedIdentity = &UnmanagedIdentityByte;
        delegate* managed<byte, byte> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<byte, byte> restoredFunction = (delegate* managed<byte, byte>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != (byte)211 || read(pointer1) != (byte)211 || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != (byte)211 || restoredFunction(value) != (byte)211 || nullFunction != null)
            Fail("generated byte managed or unmanaged scalar function pointers");
    }

    private static unsafe short ManagedIdentityInt16(short value) => value;
    private static unsafe short ManagedReadInt16(short* value) => *value;
    private static unsafe void ManagedWriteInt16(short* target, short value) => *target = value;
    private static unsafe short* ManagedReturnPointerInt16(short* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe short UnmanagedIdentityInt16(short value) => value;

    private static unsafe void VerifyScalarPointerInt16()
    {
        short value = (short)-1234;
        short replacement = (short)-1234;
        short* pointer1 = &value;
        short** pointer2 = &pointer1;
        short*** pointer3 = &pointer2;
        short**** pointer4 = &pointer3;
        short***** pointer5 = &pointer4;
        short****** pointer6 = &pointer5;
        short******* pointer7 = &pointer6;
        short******** pointer8 = &pointer7;
        if (********pointer8 != (short)-1234)
            Fail("generated short eight-level pointer dereference");
        void* erased = pointer1;
        short* restored = (short*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != (short)-1234 || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated short pointer address conversions");
        short* values = stackalloc short[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != (short)-1234 || values[3] != (short)-1234 || (values + 3) - values != 3)
            Fail("generated short stackalloc indexing and arithmetic");
        delegate* managed<short, short> identity = &ManagedIdentityInt16;
        delegate* managed<short*, short> read = &ManagedReadInt16;
        delegate* managed<short*, short, void> write = &ManagedWriteInt16;
        delegate* managed<short*, short*> returnPointer = &ManagedReturnPointerInt16;
        delegate* unmanaged<short, short> unmanagedIdentity = &UnmanagedIdentityInt16;
        delegate* managed<short, short> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<short, short> restoredFunction = (delegate* managed<short, short>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != (short)-1234 || read(pointer1) != (short)-1234 || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != (short)-1234 || restoredFunction(value) != (short)-1234 || nullFunction != null)
            Fail("generated short managed or unmanaged scalar function pointers");
    }

    private static unsafe ushort ManagedIdentityUInt16(ushort value) => value;
    private static unsafe ushort ManagedReadUInt16(ushort* value) => *value;
    private static unsafe void ManagedWriteUInt16(ushort* target, ushort value) => *target = value;
    private static unsafe ushort* ManagedReturnPointerUInt16(ushort* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe ushort UnmanagedIdentityUInt16(ushort value) => value;

    private static unsafe void VerifyScalarPointerUInt16()
    {
        ushort value = (ushort)54321;
        ushort replacement = (ushort)54321;
        ushort* pointer1 = &value;
        ushort** pointer2 = &pointer1;
        ushort*** pointer3 = &pointer2;
        ushort**** pointer4 = &pointer3;
        ushort***** pointer5 = &pointer4;
        ushort****** pointer6 = &pointer5;
        ushort******* pointer7 = &pointer6;
        ushort******** pointer8 = &pointer7;
        if (********pointer8 != (ushort)54321)
            Fail("generated ushort eight-level pointer dereference");
        void* erased = pointer1;
        ushort* restored = (ushort*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != (ushort)54321 || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated ushort pointer address conversions");
        ushort* values = stackalloc ushort[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != (ushort)54321 || values[3] != (ushort)54321 || (values + 3) - values != 3)
            Fail("generated ushort stackalloc indexing and arithmetic");
        delegate* managed<ushort, ushort> identity = &ManagedIdentityUInt16;
        delegate* managed<ushort*, ushort> read = &ManagedReadUInt16;
        delegate* managed<ushort*, ushort, void> write = &ManagedWriteUInt16;
        delegate* managed<ushort*, ushort*> returnPointer = &ManagedReturnPointerUInt16;
        delegate* unmanaged<ushort, ushort> unmanagedIdentity = &UnmanagedIdentityUInt16;
        delegate* managed<ushort, ushort> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<ushort, ushort> restoredFunction = (delegate* managed<ushort, ushort>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != (ushort)54321 || read(pointer1) != (ushort)54321 || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != (ushort)54321 || restoredFunction(value) != (ushort)54321 || nullFunction != null)
            Fail("generated ushort managed or unmanaged scalar function pointers");
    }

    private static unsafe char ManagedIdentityChar(char value) => value;
    private static unsafe char ManagedReadChar(char* value) => *value;
    private static unsafe void ManagedWriteChar(char* target, char value) => *target = value;
    private static unsafe char* ManagedReturnPointerChar(char* value) => value;

    private static unsafe void VerifyScalarPointerChar()
    {
        char value = 'K';
        char replacement = 'K';
        char* pointer1 = &value;
        char** pointer2 = &pointer1;
        char*** pointer3 = &pointer2;
        char**** pointer4 = &pointer3;
        char***** pointer5 = &pointer4;
        char****** pointer6 = &pointer5;
        char******* pointer7 = &pointer6;
        char******** pointer8 = &pointer7;
        if (********pointer8 != 'K')
            Fail("generated char eight-level pointer dereference");
        void* erased = pointer1;
        char* restored = (char*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != 'K' || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated char pointer address conversions");
        char* values = stackalloc char[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != 'K' || values[3] != 'K' || (values + 3) - values != 3)
            Fail("generated char stackalloc indexing and arithmetic");
        delegate* managed<char, char> identity = &ManagedIdentityChar;
        delegate* managed<char*, char> read = &ManagedReadChar;
        delegate* managed<char*, char, void> write = &ManagedWriteChar;
        delegate* managed<char*, char*> returnPointer = &ManagedReturnPointerChar;
        delegate* managed<char, char> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<char, char> restoredFunction = (delegate* managed<char, char>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != 'K' || read(pointer1) != 'K' || returnPointer(pointer1) != pointer1 ||
            restoredFunction(value) != 'K' || nullFunction != null)
            Fail("generated char managed or unmanaged scalar function pointers");
    }

    private static unsafe int ManagedIdentityInt32(int value) => value;
    private static unsafe int ManagedReadInt32(int* value) => *value;
    private static unsafe void ManagedWriteInt32(int* target, int value) => *target = value;
    private static unsafe int* ManagedReturnPointerInt32(int* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe int UnmanagedIdentityInt32(int value) => value;

    private static unsafe void VerifyScalarPointerInt32()
    {
        int value = -1234567;
        int replacement = -1234567;
        int* pointer1 = &value;
        int** pointer2 = &pointer1;
        int*** pointer3 = &pointer2;
        int**** pointer4 = &pointer3;
        int***** pointer5 = &pointer4;
        int****** pointer6 = &pointer5;
        int******* pointer7 = &pointer6;
        int******** pointer8 = &pointer7;
        if (********pointer8 != -1234567)
            Fail("generated int eight-level pointer dereference");
        void* erased = pointer1;
        int* restored = (int*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != -1234567 || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated int pointer address conversions");
        int* values = stackalloc int[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != -1234567 || values[3] != -1234567 || (values + 3) - values != 3)
            Fail("generated int stackalloc indexing and arithmetic");
        delegate* managed<int, int> identity = &ManagedIdentityInt32;
        delegate* managed<int*, int> read = &ManagedReadInt32;
        delegate* managed<int*, int, void> write = &ManagedWriteInt32;
        delegate* managed<int*, int*> returnPointer = &ManagedReturnPointerInt32;
        delegate* unmanaged<int, int> unmanagedIdentity = &UnmanagedIdentityInt32;
        delegate* managed<int, int> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<int, int> restoredFunction = (delegate* managed<int, int>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != -1234567 || read(pointer1) != -1234567 || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != -1234567 || restoredFunction(value) != -1234567 || nullFunction != null)
            Fail("generated int managed or unmanaged scalar function pointers");
    }

    private static unsafe uint ManagedIdentityUInt32(uint value) => value;
    private static unsafe uint ManagedReadUInt32(uint* value) => *value;
    private static unsafe void ManagedWriteUInt32(uint* target, uint value) => *target = value;
    private static unsafe uint* ManagedReturnPointerUInt32(uint* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe uint UnmanagedIdentityUInt32(uint value) => value;

    private static unsafe void VerifyScalarPointerUInt32()
    {
        uint value = 3456789012u;
        uint replacement = 3456789012u;
        uint* pointer1 = &value;
        uint** pointer2 = &pointer1;
        uint*** pointer3 = &pointer2;
        uint**** pointer4 = &pointer3;
        uint***** pointer5 = &pointer4;
        uint****** pointer6 = &pointer5;
        uint******* pointer7 = &pointer6;
        uint******** pointer8 = &pointer7;
        if (********pointer8 != 3456789012u)
            Fail("generated uint eight-level pointer dereference");
        void* erased = pointer1;
        uint* restored = (uint*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != 3456789012u || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated uint pointer address conversions");
        uint* values = stackalloc uint[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != 3456789012u || values[3] != 3456789012u || (values + 3) - values != 3)
            Fail("generated uint stackalloc indexing and arithmetic");
        delegate* managed<uint, uint> identity = &ManagedIdentityUInt32;
        delegate* managed<uint*, uint> read = &ManagedReadUInt32;
        delegate* managed<uint*, uint, void> write = &ManagedWriteUInt32;
        delegate* managed<uint*, uint*> returnPointer = &ManagedReturnPointerUInt32;
        delegate* unmanaged<uint, uint> unmanagedIdentity = &UnmanagedIdentityUInt32;
        delegate* managed<uint, uint> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<uint, uint> restoredFunction = (delegate* managed<uint, uint>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != 3456789012u || read(pointer1) != 3456789012u || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != 3456789012u || restoredFunction(value) != 3456789012u || nullFunction != null)
            Fail("generated uint managed or unmanaged scalar function pointers");
    }

    private static unsafe long ManagedIdentityInt64(long value) => value;
    private static unsafe long ManagedReadInt64(long* value) => *value;
    private static unsafe void ManagedWriteInt64(long* target, long value) => *target = value;
    private static unsafe long* ManagedReturnPointerInt64(long* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe long UnmanagedIdentityInt64(long value) => value;

    private static unsafe void VerifyScalarPointerInt64()
    {
        long value = -1234567890123L;
        long replacement = -1234567890123L;
        long* pointer1 = &value;
        long** pointer2 = &pointer1;
        long*** pointer3 = &pointer2;
        long**** pointer4 = &pointer3;
        long***** pointer5 = &pointer4;
        long****** pointer6 = &pointer5;
        long******* pointer7 = &pointer6;
        long******** pointer8 = &pointer7;
        if (********pointer8 != -1234567890123L)
            Fail("generated long eight-level pointer dereference");
        void* erased = pointer1;
        long* restored = (long*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != -1234567890123L || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated long pointer address conversions");
        long* values = stackalloc long[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != -1234567890123L || values[3] != -1234567890123L || (values + 3) - values != 3)
            Fail("generated long stackalloc indexing and arithmetic");
        delegate* managed<long, long> identity = &ManagedIdentityInt64;
        delegate* managed<long*, long> read = &ManagedReadInt64;
        delegate* managed<long*, long, void> write = &ManagedWriteInt64;
        delegate* managed<long*, long*> returnPointer = &ManagedReturnPointerInt64;
        delegate* unmanaged<long, long> unmanagedIdentity = &UnmanagedIdentityInt64;
        delegate* managed<long, long> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<long, long> restoredFunction = (delegate* managed<long, long>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != -1234567890123L || read(pointer1) != -1234567890123L || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != -1234567890123L || restoredFunction(value) != -1234567890123L || nullFunction != null)
            Fail("generated long managed or unmanaged scalar function pointers");
    }

    private static unsafe ulong ManagedIdentityUInt64(ulong value) => value;
    private static unsafe ulong ManagedReadUInt64(ulong* value) => *value;
    private static unsafe void ManagedWriteUInt64(ulong* target, ulong value) => *target = value;
    private static unsafe ulong* ManagedReturnPointerUInt64(ulong* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe ulong UnmanagedIdentityUInt64(ulong value) => value;

    private static unsafe void VerifyScalarPointerUInt64()
    {
        ulong value = 12345678901234567890UL;
        ulong replacement = 12345678901234567890UL;
        ulong* pointer1 = &value;
        ulong** pointer2 = &pointer1;
        ulong*** pointer3 = &pointer2;
        ulong**** pointer4 = &pointer3;
        ulong***** pointer5 = &pointer4;
        ulong****** pointer6 = &pointer5;
        ulong******* pointer7 = &pointer6;
        ulong******** pointer8 = &pointer7;
        if (********pointer8 != 12345678901234567890UL)
            Fail("generated ulong eight-level pointer dereference");
        void* erased = pointer1;
        ulong* restored = (ulong*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != 12345678901234567890UL || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated ulong pointer address conversions");
        ulong* values = stackalloc ulong[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != 12345678901234567890UL || values[3] != 12345678901234567890UL || (values + 3) - values != 3)
            Fail("generated ulong stackalloc indexing and arithmetic");
        delegate* managed<ulong, ulong> identity = &ManagedIdentityUInt64;
        delegate* managed<ulong*, ulong> read = &ManagedReadUInt64;
        delegate* managed<ulong*, ulong, void> write = &ManagedWriteUInt64;
        delegate* managed<ulong*, ulong*> returnPointer = &ManagedReturnPointerUInt64;
        delegate* unmanaged<ulong, ulong> unmanagedIdentity = &UnmanagedIdentityUInt64;
        delegate* managed<ulong, ulong> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<ulong, ulong> restoredFunction = (delegate* managed<ulong, ulong>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != 12345678901234567890UL || read(pointer1) != 12345678901234567890UL || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != 12345678901234567890UL || restoredFunction(value) != 12345678901234567890UL || nullFunction != null)
            Fail("generated ulong managed or unmanaged scalar function pointers");
    }

    private static unsafe nint ManagedIdentityNativeInt(nint value) => value;
    private static unsafe nint ManagedReadNativeInt(nint* value) => *value;
    private static unsafe void ManagedWriteNativeInt(nint* target, nint value) => *target = value;
    private static unsafe nint* ManagedReturnPointerNativeInt(nint* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe nint UnmanagedIdentityNativeInt(nint value) => value;

    private static unsafe void VerifyScalarPointerNativeInt()
    {
        nint value = (nint)0x123456;
        nint replacement = (nint)0x123456;
        nint* pointer1 = &value;
        nint** pointer2 = &pointer1;
        nint*** pointer3 = &pointer2;
        nint**** pointer4 = &pointer3;
        nint***** pointer5 = &pointer4;
        nint****** pointer6 = &pointer5;
        nint******* pointer7 = &pointer6;
        nint******** pointer8 = &pointer7;
        if (********pointer8 != (nint)0x123456)
            Fail("generated nint eight-level pointer dereference");
        void* erased = pointer1;
        nint* restored = (nint*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != (nint)0x123456 || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated nint pointer address conversions");
        nint* values = stackalloc nint[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != (nint)0x123456 || values[3] != (nint)0x123456 || (values + 3) - values != 3)
            Fail("generated nint stackalloc indexing and arithmetic");
        delegate* managed<nint, nint> identity = &ManagedIdentityNativeInt;
        delegate* managed<nint*, nint> read = &ManagedReadNativeInt;
        delegate* managed<nint*, nint, void> write = &ManagedWriteNativeInt;
        delegate* managed<nint*, nint*> returnPointer = &ManagedReturnPointerNativeInt;
        delegate* unmanaged<nint, nint> unmanagedIdentity = &UnmanagedIdentityNativeInt;
        delegate* managed<nint, nint> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<nint, nint> restoredFunction = (delegate* managed<nint, nint>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != (nint)0x123456 || read(pointer1) != (nint)0x123456 || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != (nint)0x123456 || restoredFunction(value) != (nint)0x123456 || nullFunction != null)
            Fail("generated nint managed or unmanaged scalar function pointers");
    }

    private static unsafe nuint ManagedIdentityNativeUInt(nuint value) => value;
    private static unsafe nuint ManagedReadNativeUInt(nuint* value) => *value;
    private static unsafe void ManagedWriteNativeUInt(nuint* target, nuint value) => *target = value;
    private static unsafe nuint* ManagedReturnPointerNativeUInt(nuint* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe nuint UnmanagedIdentityNativeUInt(nuint value) => value;

    private static unsafe void VerifyScalarPointerNativeUInt()
    {
        nuint value = (nuint)0xabcdefu;
        nuint replacement = (nuint)0xabcdefu;
        nuint* pointer1 = &value;
        nuint** pointer2 = &pointer1;
        nuint*** pointer3 = &pointer2;
        nuint**** pointer4 = &pointer3;
        nuint***** pointer5 = &pointer4;
        nuint****** pointer6 = &pointer5;
        nuint******* pointer7 = &pointer6;
        nuint******** pointer8 = &pointer7;
        if (********pointer8 != (nuint)0xabcdefu)
            Fail("generated nuint eight-level pointer dereference");
        void* erased = pointer1;
        nuint* restored = (nuint*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != (nuint)0xabcdefu || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated nuint pointer address conversions");
        nuint* values = stackalloc nuint[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != (nuint)0xabcdefu || values[3] != (nuint)0xabcdefu || (values + 3) - values != 3)
            Fail("generated nuint stackalloc indexing and arithmetic");
        delegate* managed<nuint, nuint> identity = &ManagedIdentityNativeUInt;
        delegate* managed<nuint*, nuint> read = &ManagedReadNativeUInt;
        delegate* managed<nuint*, nuint, void> write = &ManagedWriteNativeUInt;
        delegate* managed<nuint*, nuint*> returnPointer = &ManagedReturnPointerNativeUInt;
        delegate* unmanaged<nuint, nuint> unmanagedIdentity = &UnmanagedIdentityNativeUInt;
        delegate* managed<nuint, nuint> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<nuint, nuint> restoredFunction = (delegate* managed<nuint, nuint>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != (nuint)0xabcdefu || read(pointer1) != (nuint)0xabcdefu || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != (nuint)0xabcdefu || restoredFunction(value) != (nuint)0xabcdefu || nullFunction != null)
            Fail("generated nuint managed or unmanaged scalar function pointers");
    }

    private static unsafe float ManagedIdentitySingle(float value) => value;
    private static unsafe float ManagedReadSingle(float* value) => *value;
    private static unsafe void ManagedWriteSingle(float* target, float value) => *target = value;
    private static unsafe float* ManagedReturnPointerSingle(float* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe float UnmanagedIdentitySingle(float value) => value;

    private static unsafe void VerifyScalarPointerSingle()
    {
        float value = 12.25f;
        float replacement = 12.25f;
        float* pointer1 = &value;
        float** pointer2 = &pointer1;
        float*** pointer3 = &pointer2;
        float**** pointer4 = &pointer3;
        float***** pointer5 = &pointer4;
        float****** pointer6 = &pointer5;
        float******* pointer7 = &pointer6;
        float******** pointer8 = &pointer7;
        if (********pointer8 != 12.25f)
            Fail("generated float eight-level pointer dereference");
        void* erased = pointer1;
        float* restored = (float*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != 12.25f || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated float pointer address conversions");
        float* values = stackalloc float[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != 12.25f || values[3] != 12.25f || (values + 3) - values != 3)
            Fail("generated float stackalloc indexing and arithmetic");
        delegate* managed<float, float> identity = &ManagedIdentitySingle;
        delegate* managed<float*, float> read = &ManagedReadSingle;
        delegate* managed<float*, float, void> write = &ManagedWriteSingle;
        delegate* managed<float*, float*> returnPointer = &ManagedReturnPointerSingle;
        delegate* unmanaged<float, float> unmanagedIdentity = &UnmanagedIdentitySingle;
        delegate* managed<float, float> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<float, float> restoredFunction = (delegate* managed<float, float>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != 12.25f || read(pointer1) != 12.25f || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != 12.25f || restoredFunction(value) != 12.25f || nullFunction != null)
            Fail("generated float managed or unmanaged scalar function pointers");
    }

    private static unsafe double ManagedIdentityDouble(double value) => value;
    private static unsafe double ManagedReadDouble(double* value) => *value;
    private static unsafe void ManagedWriteDouble(double* target, double value) => *target = value;
    private static unsafe double* ManagedReturnPointerDouble(double* value) => value;
    [UnmanagedCallersOnly]
    private static unsafe double UnmanagedIdentityDouble(double value) => value;

    private static unsafe void VerifyScalarPointerDouble()
    {
        double value = -33.5;
        double replacement = -33.5;
        double* pointer1 = &value;
        double** pointer2 = &pointer1;
        double*** pointer3 = &pointer2;
        double**** pointer4 = &pointer3;
        double***** pointer5 = &pointer4;
        double****** pointer6 = &pointer5;
        double******* pointer7 = &pointer6;
        double******** pointer8 = &pointer7;
        if (********pointer8 != -33.5)
            Fail("generated double eight-level pointer dereference");
        void* erased = pointer1;
        double* restored = (double*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != -33.5 || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated double pointer address conversions");
        double* values = stackalloc double[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != -33.5 || values[3] != -33.5 || (values + 3) - values != 3)
            Fail("generated double stackalloc indexing and arithmetic");
        delegate* managed<double, double> identity = &ManagedIdentityDouble;
        delegate* managed<double*, double> read = &ManagedReadDouble;
        delegate* managed<double*, double, void> write = &ManagedWriteDouble;
        delegate* managed<double*, double*> returnPointer = &ManagedReturnPointerDouble;
        delegate* unmanaged<double, double> unmanagedIdentity = &UnmanagedIdentityDouble;
        delegate* managed<double, double> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<double, double> restoredFunction = (delegate* managed<double, double>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != -33.5 || read(pointer1) != -33.5 || returnPointer(pointer1) != pointer1 ||
            unmanagedIdentity(value) != -33.5 || restoredFunction(value) != -33.5 || nullFunction != null)
            Fail("generated double managed or unmanaged scalar function pointers");
    }

    private static unsafe bool ManagedIdentityBoolean(bool value) => value;
    private static unsafe bool ManagedReadBoolean(bool* value) => *value;
    private static unsafe void ManagedWriteBoolean(bool* target, bool value) => *target = value;
    private static unsafe bool* ManagedReturnPointerBoolean(bool* value) => value;

    private static unsafe void VerifyScalarPointerBoolean()
    {
        bool value = true;
        bool replacement = true;
        bool* pointer1 = &value;
        bool** pointer2 = &pointer1;
        bool*** pointer3 = &pointer2;
        bool**** pointer4 = &pointer3;
        bool***** pointer5 = &pointer4;
        bool****** pointer6 = &pointer5;
        bool******* pointer7 = &pointer6;
        bool******** pointer8 = &pointer7;
        if (********pointer8 != true)
            Fail("generated bool eight-level pointer dereference");
        void* erased = pointer1;
        bool* restored = (bool*)erased;
        nint signedAddress = (nint)erased;
        nuint unsignedAddress = (nuint)erased;
        if (restored != pointer1 || *restored != true || (void*)signedAddress != erased || (void*)unsignedAddress != erased)
            Fail("generated bool pointer address conversions");
        bool* values = stackalloc bool[4];
        values[0] = value;
        values[1] = replacement;
        values[2] = value;
        values[3] = replacement;
        if (*(values + 2) != true || values[3] != true || (values + 3) - values != 3)
            Fail("generated bool stackalloc indexing and arithmetic");
        delegate* managed<bool, bool> identity = &ManagedIdentityBoolean;
        delegate* managed<bool*, bool> read = &ManagedReadBoolean;
        delegate* managed<bool*, bool, void> write = &ManagedWriteBoolean;
        delegate* managed<bool*, bool*> returnPointer = &ManagedReturnPointerBoolean;
        delegate* managed<bool, bool> nullFunction = null;
        void* functionAddress = (void*)identity;
        delegate* managed<bool, bool> restoredFunction = (delegate* managed<bool, bool>)functionAddress;
        write(pointer1, replacement);
        if (identity(value) != true || read(pointer1) != true || returnPointer(pointer1) != pointer1 ||
            restoredFunction(value) != true || nullFunction != null)
            Fail("generated bool managed or unmanaged scalar function pointers");
    }

    private static void VerifyGeneratedPointerCasts()
    {
        VerifyPointerCastSByteToSByte();
        VerifyPointerCastSByteToByte();
        VerifyPointerCastSByteToInt16();
        VerifyPointerCastSByteToUInt16();
        VerifyPointerCastSByteToChar();
        VerifyPointerCastSByteToInt32();
        VerifyPointerCastSByteToUInt32();
        VerifyPointerCastSByteToInt64();
        VerifyPointerCastSByteToUInt64();
        VerifyPointerCastSByteToNativeInt();
        VerifyPointerCastSByteToNativeUInt();
        VerifyPointerCastSByteToSingle();
        VerifyPointerCastSByteToDouble();
        VerifyPointerCastSByteToBoolean();
        VerifyPointerCastByteToSByte();
        VerifyPointerCastByteToByte();
        VerifyPointerCastByteToInt16();
        VerifyPointerCastByteToUInt16();
        VerifyPointerCastByteToChar();
        VerifyPointerCastByteToInt32();
        VerifyPointerCastByteToUInt32();
        VerifyPointerCastByteToInt64();
        VerifyPointerCastByteToUInt64();
        VerifyPointerCastByteToNativeInt();
        VerifyPointerCastByteToNativeUInt();
        VerifyPointerCastByteToSingle();
        VerifyPointerCastByteToDouble();
        VerifyPointerCastByteToBoolean();
        VerifyPointerCastInt16ToSByte();
        VerifyPointerCastInt16ToByte();
        VerifyPointerCastInt16ToInt16();
        VerifyPointerCastInt16ToUInt16();
        VerifyPointerCastInt16ToChar();
        VerifyPointerCastInt16ToInt32();
        VerifyPointerCastInt16ToUInt32();
        VerifyPointerCastInt16ToInt64();
        VerifyPointerCastInt16ToUInt64();
        VerifyPointerCastInt16ToNativeInt();
        VerifyPointerCastInt16ToNativeUInt();
        VerifyPointerCastInt16ToSingle();
        VerifyPointerCastInt16ToDouble();
        VerifyPointerCastInt16ToBoolean();
        VerifyPointerCastUInt16ToSByte();
        VerifyPointerCastUInt16ToByte();
        VerifyPointerCastUInt16ToInt16();
        VerifyPointerCastUInt16ToUInt16();
        VerifyPointerCastUInt16ToChar();
        VerifyPointerCastUInt16ToInt32();
        VerifyPointerCastUInt16ToUInt32();
        VerifyPointerCastUInt16ToInt64();
        VerifyPointerCastUInt16ToUInt64();
        VerifyPointerCastUInt16ToNativeInt();
        VerifyPointerCastUInt16ToNativeUInt();
        VerifyPointerCastUInt16ToSingle();
        VerifyPointerCastUInt16ToDouble();
        VerifyPointerCastUInt16ToBoolean();
        VerifyPointerCastCharToSByte();
        VerifyPointerCastCharToByte();
        VerifyPointerCastCharToInt16();
        VerifyPointerCastCharToUInt16();
        VerifyPointerCastCharToChar();
        VerifyPointerCastCharToInt32();
        VerifyPointerCastCharToUInt32();
        VerifyPointerCastCharToInt64();
        VerifyPointerCastCharToUInt64();
        VerifyPointerCastCharToNativeInt();
        VerifyPointerCastCharToNativeUInt();
        VerifyPointerCastCharToSingle();
        VerifyPointerCastCharToDouble();
        VerifyPointerCastCharToBoolean();
        VerifyPointerCastInt32ToSByte();
        VerifyPointerCastInt32ToByte();
        VerifyPointerCastInt32ToInt16();
        VerifyPointerCastInt32ToUInt16();
        VerifyPointerCastInt32ToChar();
        VerifyPointerCastInt32ToInt32();
        VerifyPointerCastInt32ToUInt32();
        VerifyPointerCastInt32ToInt64();
        VerifyPointerCastInt32ToUInt64();
        VerifyPointerCastInt32ToNativeInt();
        VerifyPointerCastInt32ToNativeUInt();
        VerifyPointerCastInt32ToSingle();
        VerifyPointerCastInt32ToDouble();
        VerifyPointerCastInt32ToBoolean();
        VerifyPointerCastUInt32ToSByte();
        VerifyPointerCastUInt32ToByte();
        VerifyPointerCastUInt32ToInt16();
        VerifyPointerCastUInt32ToUInt16();
        VerifyPointerCastUInt32ToChar();
        VerifyPointerCastUInt32ToInt32();
        VerifyPointerCastUInt32ToUInt32();
        VerifyPointerCastUInt32ToInt64();
        VerifyPointerCastUInt32ToUInt64();
        VerifyPointerCastUInt32ToNativeInt();
        VerifyPointerCastUInt32ToNativeUInt();
        VerifyPointerCastUInt32ToSingle();
        VerifyPointerCastUInt32ToDouble();
        VerifyPointerCastUInt32ToBoolean();
        VerifyPointerCastInt64ToSByte();
        VerifyPointerCastInt64ToByte();
        VerifyPointerCastInt64ToInt16();
        VerifyPointerCastInt64ToUInt16();
        VerifyPointerCastInt64ToChar();
        VerifyPointerCastInt64ToInt32();
        VerifyPointerCastInt64ToUInt32();
        VerifyPointerCastInt64ToInt64();
        VerifyPointerCastInt64ToUInt64();
        VerifyPointerCastInt64ToNativeInt();
        VerifyPointerCastInt64ToNativeUInt();
        VerifyPointerCastInt64ToSingle();
        VerifyPointerCastInt64ToDouble();
        VerifyPointerCastInt64ToBoolean();
        VerifyPointerCastUInt64ToSByte();
        VerifyPointerCastUInt64ToByte();
        VerifyPointerCastUInt64ToInt16();
        VerifyPointerCastUInt64ToUInt16();
        VerifyPointerCastUInt64ToChar();
        VerifyPointerCastUInt64ToInt32();
        VerifyPointerCastUInt64ToUInt32();
        VerifyPointerCastUInt64ToInt64();
        VerifyPointerCastUInt64ToUInt64();
        VerifyPointerCastUInt64ToNativeInt();
        VerifyPointerCastUInt64ToNativeUInt();
        VerifyPointerCastUInt64ToSingle();
        VerifyPointerCastUInt64ToDouble();
        VerifyPointerCastUInt64ToBoolean();
        VerifyPointerCastNativeIntToSByte();
        VerifyPointerCastNativeIntToByte();
        VerifyPointerCastNativeIntToInt16();
        VerifyPointerCastNativeIntToUInt16();
        VerifyPointerCastNativeIntToChar();
        VerifyPointerCastNativeIntToInt32();
        VerifyPointerCastNativeIntToUInt32();
        VerifyPointerCastNativeIntToInt64();
        VerifyPointerCastNativeIntToUInt64();
        VerifyPointerCastNativeIntToNativeInt();
        VerifyPointerCastNativeIntToNativeUInt();
        VerifyPointerCastNativeIntToSingle();
        VerifyPointerCastNativeIntToDouble();
        VerifyPointerCastNativeIntToBoolean();
        VerifyPointerCastNativeUIntToSByte();
        VerifyPointerCastNativeUIntToByte();
        VerifyPointerCastNativeUIntToInt16();
        VerifyPointerCastNativeUIntToUInt16();
        VerifyPointerCastNativeUIntToChar();
        VerifyPointerCastNativeUIntToInt32();
        VerifyPointerCastNativeUIntToUInt32();
        VerifyPointerCastNativeUIntToInt64();
        VerifyPointerCastNativeUIntToUInt64();
        VerifyPointerCastNativeUIntToNativeInt();
        VerifyPointerCastNativeUIntToNativeUInt();
        VerifyPointerCastNativeUIntToSingle();
        VerifyPointerCastNativeUIntToDouble();
        VerifyPointerCastNativeUIntToBoolean();
        VerifyPointerCastSingleToSByte();
        VerifyPointerCastSingleToByte();
        VerifyPointerCastSingleToInt16();
        VerifyPointerCastSingleToUInt16();
        VerifyPointerCastSingleToChar();
        VerifyPointerCastSingleToInt32();
        VerifyPointerCastSingleToUInt32();
        VerifyPointerCastSingleToInt64();
        VerifyPointerCastSingleToUInt64();
        VerifyPointerCastSingleToNativeInt();
        VerifyPointerCastSingleToNativeUInt();
        VerifyPointerCastSingleToSingle();
        VerifyPointerCastSingleToDouble();
        VerifyPointerCastSingleToBoolean();
        VerifyPointerCastDoubleToSByte();
        VerifyPointerCastDoubleToByte();
        VerifyPointerCastDoubleToInt16();
        VerifyPointerCastDoubleToUInt16();
        VerifyPointerCastDoubleToChar();
        VerifyPointerCastDoubleToInt32();
        VerifyPointerCastDoubleToUInt32();
        VerifyPointerCastDoubleToInt64();
        VerifyPointerCastDoubleToUInt64();
        VerifyPointerCastDoubleToNativeInt();
        VerifyPointerCastDoubleToNativeUInt();
        VerifyPointerCastDoubleToSingle();
        VerifyPointerCastDoubleToDouble();
        VerifyPointerCastDoubleToBoolean();
        VerifyPointerCastBooleanToSByte();
        VerifyPointerCastBooleanToByte();
        VerifyPointerCastBooleanToInt16();
        VerifyPointerCastBooleanToUInt16();
        VerifyPointerCastBooleanToChar();
        VerifyPointerCastBooleanToInt32();
        VerifyPointerCastBooleanToUInt32();
        VerifyPointerCastBooleanToInt64();
        VerifyPointerCastBooleanToUInt64();
        VerifyPointerCastBooleanToNativeInt();
        VerifyPointerCastBooleanToNativeUInt();
        VerifyPointerCastBooleanToSingle();
        VerifyPointerCastBooleanToDouble();
        VerifyPointerCastBooleanToBoolean();
    }

    private static unsafe void VerifyPointerCastSByteToSByte()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to sbyte and back");
    }

    private static unsafe void VerifyPointerCastSByteToByte()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        byte* converted = (byte*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to byte and back");
    }

    private static unsafe void VerifyPointerCastSByteToInt16()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        short* converted = (short*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to short and back");
    }

    private static unsafe void VerifyPointerCastSByteToUInt16()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        ushort* converted = (ushort*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to ushort and back");
    }

    private static unsafe void VerifyPointerCastSByteToChar()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        char* converted = (char*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to char and back");
    }

    private static unsafe void VerifyPointerCastSByteToInt32()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        int* converted = (int*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to int and back");
    }

    private static unsafe void VerifyPointerCastSByteToUInt32()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        uint* converted = (uint*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to uint and back");
    }

    private static unsafe void VerifyPointerCastSByteToInt64()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        long* converted = (long*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to long and back");
    }

    private static unsafe void VerifyPointerCastSByteToUInt64()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        ulong* converted = (ulong*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to ulong and back");
    }

    private static unsafe void VerifyPointerCastSByteToNativeInt()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        nint* converted = (nint*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to nint and back");
    }

    private static unsafe void VerifyPointerCastSByteToNativeUInt()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        nuint* converted = (nuint*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to nuint and back");
    }

    private static unsafe void VerifyPointerCastSByteToSingle()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        float* converted = (float*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to float and back");
    }

    private static unsafe void VerifyPointerCastSByteToDouble()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        double* converted = (double*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to double and back");
    }

    private static unsafe void VerifyPointerCastSByteToBoolean()
    {
        sbyte value = (sbyte)-11;
        sbyte* source = &value;
        bool* converted = (bool*)(void*)source;
        sbyte* restored = (sbyte*)(void*)converted;
        if (restored != source || *restored != (sbyte)-11)
            Fail("generated pointer cast sbyte to bool and back");
    }

    private static unsafe void VerifyPointerCastByteToSByte()
    {
        byte value = (byte)211;
        byte* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to sbyte and back");
    }

    private static unsafe void VerifyPointerCastByteToByte()
    {
        byte value = (byte)211;
        byte* source = &value;
        byte* converted = (byte*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to byte and back");
    }

    private static unsafe void VerifyPointerCastByteToInt16()
    {
        byte value = (byte)211;
        byte* source = &value;
        short* converted = (short*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to short and back");
    }

    private static unsafe void VerifyPointerCastByteToUInt16()
    {
        byte value = (byte)211;
        byte* source = &value;
        ushort* converted = (ushort*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to ushort and back");
    }

    private static unsafe void VerifyPointerCastByteToChar()
    {
        byte value = (byte)211;
        byte* source = &value;
        char* converted = (char*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to char and back");
    }

    private static unsafe void VerifyPointerCastByteToInt32()
    {
        byte value = (byte)211;
        byte* source = &value;
        int* converted = (int*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to int and back");
    }

    private static unsafe void VerifyPointerCastByteToUInt32()
    {
        byte value = (byte)211;
        byte* source = &value;
        uint* converted = (uint*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to uint and back");
    }

    private static unsafe void VerifyPointerCastByteToInt64()
    {
        byte value = (byte)211;
        byte* source = &value;
        long* converted = (long*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to long and back");
    }

    private static unsafe void VerifyPointerCastByteToUInt64()
    {
        byte value = (byte)211;
        byte* source = &value;
        ulong* converted = (ulong*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to ulong and back");
    }

    private static unsafe void VerifyPointerCastByteToNativeInt()
    {
        byte value = (byte)211;
        byte* source = &value;
        nint* converted = (nint*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to nint and back");
    }

    private static unsafe void VerifyPointerCastByteToNativeUInt()
    {
        byte value = (byte)211;
        byte* source = &value;
        nuint* converted = (nuint*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to nuint and back");
    }

    private static unsafe void VerifyPointerCastByteToSingle()
    {
        byte value = (byte)211;
        byte* source = &value;
        float* converted = (float*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to float and back");
    }

    private static unsafe void VerifyPointerCastByteToDouble()
    {
        byte value = (byte)211;
        byte* source = &value;
        double* converted = (double*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to double and back");
    }

    private static unsafe void VerifyPointerCastByteToBoolean()
    {
        byte value = (byte)211;
        byte* source = &value;
        bool* converted = (bool*)(void*)source;
        byte* restored = (byte*)(void*)converted;
        if (restored != source || *restored != (byte)211)
            Fail("generated pointer cast byte to bool and back");
    }

    private static unsafe void VerifyPointerCastInt16ToSByte()
    {
        short value = (short)-1234;
        short* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to sbyte and back");
    }

    private static unsafe void VerifyPointerCastInt16ToByte()
    {
        short value = (short)-1234;
        short* source = &value;
        byte* converted = (byte*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to byte and back");
    }

    private static unsafe void VerifyPointerCastInt16ToInt16()
    {
        short value = (short)-1234;
        short* source = &value;
        short* converted = (short*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to short and back");
    }

    private static unsafe void VerifyPointerCastInt16ToUInt16()
    {
        short value = (short)-1234;
        short* source = &value;
        ushort* converted = (ushort*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to ushort and back");
    }

    private static unsafe void VerifyPointerCastInt16ToChar()
    {
        short value = (short)-1234;
        short* source = &value;
        char* converted = (char*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to char and back");
    }

    private static unsafe void VerifyPointerCastInt16ToInt32()
    {
        short value = (short)-1234;
        short* source = &value;
        int* converted = (int*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to int and back");
    }

    private static unsafe void VerifyPointerCastInt16ToUInt32()
    {
        short value = (short)-1234;
        short* source = &value;
        uint* converted = (uint*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to uint and back");
    }

    private static unsafe void VerifyPointerCastInt16ToInt64()
    {
        short value = (short)-1234;
        short* source = &value;
        long* converted = (long*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to long and back");
    }

    private static unsafe void VerifyPointerCastInt16ToUInt64()
    {
        short value = (short)-1234;
        short* source = &value;
        ulong* converted = (ulong*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to ulong and back");
    }

    private static unsafe void VerifyPointerCastInt16ToNativeInt()
    {
        short value = (short)-1234;
        short* source = &value;
        nint* converted = (nint*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to nint and back");
    }

    private static unsafe void VerifyPointerCastInt16ToNativeUInt()
    {
        short value = (short)-1234;
        short* source = &value;
        nuint* converted = (nuint*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to nuint and back");
    }

    private static unsafe void VerifyPointerCastInt16ToSingle()
    {
        short value = (short)-1234;
        short* source = &value;
        float* converted = (float*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to float and back");
    }

    private static unsafe void VerifyPointerCastInt16ToDouble()
    {
        short value = (short)-1234;
        short* source = &value;
        double* converted = (double*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to double and back");
    }

    private static unsafe void VerifyPointerCastInt16ToBoolean()
    {
        short value = (short)-1234;
        short* source = &value;
        bool* converted = (bool*)(void*)source;
        short* restored = (short*)(void*)converted;
        if (restored != source || *restored != (short)-1234)
            Fail("generated pointer cast short to bool and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToSByte()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to sbyte and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToByte()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        byte* converted = (byte*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to byte and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToInt16()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        short* converted = (short*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to short and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToUInt16()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        ushort* converted = (ushort*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to ushort and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToChar()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        char* converted = (char*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to char and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToInt32()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        int* converted = (int*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to int and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToUInt32()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        uint* converted = (uint*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to uint and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToInt64()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        long* converted = (long*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to long and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToUInt64()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        ulong* converted = (ulong*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to ulong and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToNativeInt()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        nint* converted = (nint*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to nint and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToNativeUInt()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        nuint* converted = (nuint*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to nuint and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToSingle()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        float* converted = (float*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to float and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToDouble()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        double* converted = (double*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to double and back");
    }

    private static unsafe void VerifyPointerCastUInt16ToBoolean()
    {
        ushort value = (ushort)54321;
        ushort* source = &value;
        bool* converted = (bool*)(void*)source;
        ushort* restored = (ushort*)(void*)converted;
        if (restored != source || *restored != (ushort)54321)
            Fail("generated pointer cast ushort to bool and back");
    }

    private static unsafe void VerifyPointerCastCharToSByte()
    {
        char value = 'K';
        char* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to sbyte and back");
    }

    private static unsafe void VerifyPointerCastCharToByte()
    {
        char value = 'K';
        char* source = &value;
        byte* converted = (byte*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to byte and back");
    }

    private static unsafe void VerifyPointerCastCharToInt16()
    {
        char value = 'K';
        char* source = &value;
        short* converted = (short*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to short and back");
    }

    private static unsafe void VerifyPointerCastCharToUInt16()
    {
        char value = 'K';
        char* source = &value;
        ushort* converted = (ushort*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to ushort and back");
    }

    private static unsafe void VerifyPointerCastCharToChar()
    {
        char value = 'K';
        char* source = &value;
        char* converted = (char*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to char and back");
    }

    private static unsafe void VerifyPointerCastCharToInt32()
    {
        char value = 'K';
        char* source = &value;
        int* converted = (int*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to int and back");
    }

    private static unsafe void VerifyPointerCastCharToUInt32()
    {
        char value = 'K';
        char* source = &value;
        uint* converted = (uint*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to uint and back");
    }

    private static unsafe void VerifyPointerCastCharToInt64()
    {
        char value = 'K';
        char* source = &value;
        long* converted = (long*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to long and back");
    }

    private static unsafe void VerifyPointerCastCharToUInt64()
    {
        char value = 'K';
        char* source = &value;
        ulong* converted = (ulong*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to ulong and back");
    }

    private static unsafe void VerifyPointerCastCharToNativeInt()
    {
        char value = 'K';
        char* source = &value;
        nint* converted = (nint*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to nint and back");
    }

    private static unsafe void VerifyPointerCastCharToNativeUInt()
    {
        char value = 'K';
        char* source = &value;
        nuint* converted = (nuint*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to nuint and back");
    }

    private static unsafe void VerifyPointerCastCharToSingle()
    {
        char value = 'K';
        char* source = &value;
        float* converted = (float*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to float and back");
    }

    private static unsafe void VerifyPointerCastCharToDouble()
    {
        char value = 'K';
        char* source = &value;
        double* converted = (double*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to double and back");
    }

    private static unsafe void VerifyPointerCastCharToBoolean()
    {
        char value = 'K';
        char* source = &value;
        bool* converted = (bool*)(void*)source;
        char* restored = (char*)(void*)converted;
        if (restored != source || *restored != 'K')
            Fail("generated pointer cast char to bool and back");
    }

    private static unsafe void VerifyPointerCastInt32ToSByte()
    {
        int value = -1234567;
        int* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to sbyte and back");
    }

    private static unsafe void VerifyPointerCastInt32ToByte()
    {
        int value = -1234567;
        int* source = &value;
        byte* converted = (byte*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to byte and back");
    }

    private static unsafe void VerifyPointerCastInt32ToInt16()
    {
        int value = -1234567;
        int* source = &value;
        short* converted = (short*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to short and back");
    }

    private static unsafe void VerifyPointerCastInt32ToUInt16()
    {
        int value = -1234567;
        int* source = &value;
        ushort* converted = (ushort*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to ushort and back");
    }

    private static unsafe void VerifyPointerCastInt32ToChar()
    {
        int value = -1234567;
        int* source = &value;
        char* converted = (char*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to char and back");
    }

    private static unsafe void VerifyPointerCastInt32ToInt32()
    {
        int value = -1234567;
        int* source = &value;
        int* converted = (int*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to int and back");
    }

    private static unsafe void VerifyPointerCastInt32ToUInt32()
    {
        int value = -1234567;
        int* source = &value;
        uint* converted = (uint*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to uint and back");
    }

    private static unsafe void VerifyPointerCastInt32ToInt64()
    {
        int value = -1234567;
        int* source = &value;
        long* converted = (long*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to long and back");
    }

    private static unsafe void VerifyPointerCastInt32ToUInt64()
    {
        int value = -1234567;
        int* source = &value;
        ulong* converted = (ulong*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to ulong and back");
    }

    private static unsafe void VerifyPointerCastInt32ToNativeInt()
    {
        int value = -1234567;
        int* source = &value;
        nint* converted = (nint*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to nint and back");
    }

    private static unsafe void VerifyPointerCastInt32ToNativeUInt()
    {
        int value = -1234567;
        int* source = &value;
        nuint* converted = (nuint*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to nuint and back");
    }

    private static unsafe void VerifyPointerCastInt32ToSingle()
    {
        int value = -1234567;
        int* source = &value;
        float* converted = (float*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to float and back");
    }

    private static unsafe void VerifyPointerCastInt32ToDouble()
    {
        int value = -1234567;
        int* source = &value;
        double* converted = (double*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to double and back");
    }

    private static unsafe void VerifyPointerCastInt32ToBoolean()
    {
        int value = -1234567;
        int* source = &value;
        bool* converted = (bool*)(void*)source;
        int* restored = (int*)(void*)converted;
        if (restored != source || *restored != -1234567)
            Fail("generated pointer cast int to bool and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToSByte()
    {
        uint value = 3456789012u;
        uint* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to sbyte and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToByte()
    {
        uint value = 3456789012u;
        uint* source = &value;
        byte* converted = (byte*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to byte and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToInt16()
    {
        uint value = 3456789012u;
        uint* source = &value;
        short* converted = (short*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to short and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToUInt16()
    {
        uint value = 3456789012u;
        uint* source = &value;
        ushort* converted = (ushort*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to ushort and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToChar()
    {
        uint value = 3456789012u;
        uint* source = &value;
        char* converted = (char*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to char and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToInt32()
    {
        uint value = 3456789012u;
        uint* source = &value;
        int* converted = (int*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to int and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToUInt32()
    {
        uint value = 3456789012u;
        uint* source = &value;
        uint* converted = (uint*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to uint and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToInt64()
    {
        uint value = 3456789012u;
        uint* source = &value;
        long* converted = (long*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to long and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToUInt64()
    {
        uint value = 3456789012u;
        uint* source = &value;
        ulong* converted = (ulong*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to ulong and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToNativeInt()
    {
        uint value = 3456789012u;
        uint* source = &value;
        nint* converted = (nint*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to nint and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToNativeUInt()
    {
        uint value = 3456789012u;
        uint* source = &value;
        nuint* converted = (nuint*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to nuint and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToSingle()
    {
        uint value = 3456789012u;
        uint* source = &value;
        float* converted = (float*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to float and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToDouble()
    {
        uint value = 3456789012u;
        uint* source = &value;
        double* converted = (double*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to double and back");
    }

    private static unsafe void VerifyPointerCastUInt32ToBoolean()
    {
        uint value = 3456789012u;
        uint* source = &value;
        bool* converted = (bool*)(void*)source;
        uint* restored = (uint*)(void*)converted;
        if (restored != source || *restored != 3456789012u)
            Fail("generated pointer cast uint to bool and back");
    }

    private static unsafe void VerifyPointerCastInt64ToSByte()
    {
        long value = -1234567890123L;
        long* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to sbyte and back");
    }

    private static unsafe void VerifyPointerCastInt64ToByte()
    {
        long value = -1234567890123L;
        long* source = &value;
        byte* converted = (byte*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to byte and back");
    }

    private static unsafe void VerifyPointerCastInt64ToInt16()
    {
        long value = -1234567890123L;
        long* source = &value;
        short* converted = (short*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to short and back");
    }

    private static unsafe void VerifyPointerCastInt64ToUInt16()
    {
        long value = -1234567890123L;
        long* source = &value;
        ushort* converted = (ushort*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to ushort and back");
    }

    private static unsafe void VerifyPointerCastInt64ToChar()
    {
        long value = -1234567890123L;
        long* source = &value;
        char* converted = (char*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to char and back");
    }

    private static unsafe void VerifyPointerCastInt64ToInt32()
    {
        long value = -1234567890123L;
        long* source = &value;
        int* converted = (int*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to int and back");
    }

    private static unsafe void VerifyPointerCastInt64ToUInt32()
    {
        long value = -1234567890123L;
        long* source = &value;
        uint* converted = (uint*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to uint and back");
    }

    private static unsafe void VerifyPointerCastInt64ToInt64()
    {
        long value = -1234567890123L;
        long* source = &value;
        long* converted = (long*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to long and back");
    }

    private static unsafe void VerifyPointerCastInt64ToUInt64()
    {
        long value = -1234567890123L;
        long* source = &value;
        ulong* converted = (ulong*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to ulong and back");
    }

    private static unsafe void VerifyPointerCastInt64ToNativeInt()
    {
        long value = -1234567890123L;
        long* source = &value;
        nint* converted = (nint*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to nint and back");
    }

    private static unsafe void VerifyPointerCastInt64ToNativeUInt()
    {
        long value = -1234567890123L;
        long* source = &value;
        nuint* converted = (nuint*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to nuint and back");
    }

    private static unsafe void VerifyPointerCastInt64ToSingle()
    {
        long value = -1234567890123L;
        long* source = &value;
        float* converted = (float*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to float and back");
    }

    private static unsafe void VerifyPointerCastInt64ToDouble()
    {
        long value = -1234567890123L;
        long* source = &value;
        double* converted = (double*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to double and back");
    }

    private static unsafe void VerifyPointerCastInt64ToBoolean()
    {
        long value = -1234567890123L;
        long* source = &value;
        bool* converted = (bool*)(void*)source;
        long* restored = (long*)(void*)converted;
        if (restored != source || *restored != -1234567890123L)
            Fail("generated pointer cast long to bool and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToSByte()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to sbyte and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToByte()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        byte* converted = (byte*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to byte and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToInt16()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        short* converted = (short*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to short and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToUInt16()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        ushort* converted = (ushort*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to ushort and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToChar()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        char* converted = (char*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to char and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToInt32()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        int* converted = (int*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to int and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToUInt32()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        uint* converted = (uint*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to uint and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToInt64()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        long* converted = (long*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to long and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToUInt64()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        ulong* converted = (ulong*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to ulong and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToNativeInt()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        nint* converted = (nint*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to nint and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToNativeUInt()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        nuint* converted = (nuint*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to nuint and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToSingle()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        float* converted = (float*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to float and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToDouble()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        double* converted = (double*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to double and back");
    }

    private static unsafe void VerifyPointerCastUInt64ToBoolean()
    {
        ulong value = 12345678901234567890UL;
        ulong* source = &value;
        bool* converted = (bool*)(void*)source;
        ulong* restored = (ulong*)(void*)converted;
        if (restored != source || *restored != 12345678901234567890UL)
            Fail("generated pointer cast ulong to bool and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToSByte()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to sbyte and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToByte()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        byte* converted = (byte*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to byte and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToInt16()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        short* converted = (short*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to short and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToUInt16()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        ushort* converted = (ushort*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to ushort and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToChar()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        char* converted = (char*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to char and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToInt32()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        int* converted = (int*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to int and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToUInt32()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        uint* converted = (uint*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to uint and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToInt64()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        long* converted = (long*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to long and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToUInt64()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        ulong* converted = (ulong*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to ulong and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToNativeInt()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        nint* converted = (nint*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to nint and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToNativeUInt()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        nuint* converted = (nuint*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to nuint and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToSingle()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        float* converted = (float*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to float and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToDouble()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        double* converted = (double*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to double and back");
    }

    private static unsafe void VerifyPointerCastNativeIntToBoolean()
    {
        nint value = (nint)0x123456;
        nint* source = &value;
        bool* converted = (bool*)(void*)source;
        nint* restored = (nint*)(void*)converted;
        if (restored != source || *restored != (nint)0x123456)
            Fail("generated pointer cast nint to bool and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToSByte()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to sbyte and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToByte()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        byte* converted = (byte*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to byte and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToInt16()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        short* converted = (short*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to short and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToUInt16()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        ushort* converted = (ushort*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to ushort and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToChar()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        char* converted = (char*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to char and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToInt32()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        int* converted = (int*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to int and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToUInt32()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        uint* converted = (uint*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to uint and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToInt64()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        long* converted = (long*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to long and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToUInt64()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        ulong* converted = (ulong*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to ulong and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToNativeInt()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        nint* converted = (nint*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to nint and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToNativeUInt()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        nuint* converted = (nuint*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to nuint and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToSingle()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        float* converted = (float*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to float and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToDouble()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        double* converted = (double*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to double and back");
    }

    private static unsafe void VerifyPointerCastNativeUIntToBoolean()
    {
        nuint value = (nuint)0xabcdefu;
        nuint* source = &value;
        bool* converted = (bool*)(void*)source;
        nuint* restored = (nuint*)(void*)converted;
        if (restored != source || *restored != (nuint)0xabcdefu)
            Fail("generated pointer cast nuint to bool and back");
    }

    private static unsafe void VerifyPointerCastSingleToSByte()
    {
        float value = 12.25f;
        float* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to sbyte and back");
    }

    private static unsafe void VerifyPointerCastSingleToByte()
    {
        float value = 12.25f;
        float* source = &value;
        byte* converted = (byte*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to byte and back");
    }

    private static unsafe void VerifyPointerCastSingleToInt16()
    {
        float value = 12.25f;
        float* source = &value;
        short* converted = (short*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to short and back");
    }

    private static unsafe void VerifyPointerCastSingleToUInt16()
    {
        float value = 12.25f;
        float* source = &value;
        ushort* converted = (ushort*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to ushort and back");
    }

    private static unsafe void VerifyPointerCastSingleToChar()
    {
        float value = 12.25f;
        float* source = &value;
        char* converted = (char*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to char and back");
    }

    private static unsafe void VerifyPointerCastSingleToInt32()
    {
        float value = 12.25f;
        float* source = &value;
        int* converted = (int*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to int and back");
    }

    private static unsafe void VerifyPointerCastSingleToUInt32()
    {
        float value = 12.25f;
        float* source = &value;
        uint* converted = (uint*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to uint and back");
    }

    private static unsafe void VerifyPointerCastSingleToInt64()
    {
        float value = 12.25f;
        float* source = &value;
        long* converted = (long*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to long and back");
    }

    private static unsafe void VerifyPointerCastSingleToUInt64()
    {
        float value = 12.25f;
        float* source = &value;
        ulong* converted = (ulong*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to ulong and back");
    }

    private static unsafe void VerifyPointerCastSingleToNativeInt()
    {
        float value = 12.25f;
        float* source = &value;
        nint* converted = (nint*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to nint and back");
    }

    private static unsafe void VerifyPointerCastSingleToNativeUInt()
    {
        float value = 12.25f;
        float* source = &value;
        nuint* converted = (nuint*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to nuint and back");
    }

    private static unsafe void VerifyPointerCastSingleToSingle()
    {
        float value = 12.25f;
        float* source = &value;
        float* converted = (float*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to float and back");
    }

    private static unsafe void VerifyPointerCastSingleToDouble()
    {
        float value = 12.25f;
        float* source = &value;
        double* converted = (double*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to double and back");
    }

    private static unsafe void VerifyPointerCastSingleToBoolean()
    {
        float value = 12.25f;
        float* source = &value;
        bool* converted = (bool*)(void*)source;
        float* restored = (float*)(void*)converted;
        if (restored != source || *restored != 12.25f)
            Fail("generated pointer cast float to bool and back");
    }

    private static unsafe void VerifyPointerCastDoubleToSByte()
    {
        double value = -33.5;
        double* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to sbyte and back");
    }

    private static unsafe void VerifyPointerCastDoubleToByte()
    {
        double value = -33.5;
        double* source = &value;
        byte* converted = (byte*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to byte and back");
    }

    private static unsafe void VerifyPointerCastDoubleToInt16()
    {
        double value = -33.5;
        double* source = &value;
        short* converted = (short*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to short and back");
    }

    private static unsafe void VerifyPointerCastDoubleToUInt16()
    {
        double value = -33.5;
        double* source = &value;
        ushort* converted = (ushort*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to ushort and back");
    }

    private static unsafe void VerifyPointerCastDoubleToChar()
    {
        double value = -33.5;
        double* source = &value;
        char* converted = (char*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to char and back");
    }

    private static unsafe void VerifyPointerCastDoubleToInt32()
    {
        double value = -33.5;
        double* source = &value;
        int* converted = (int*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to int and back");
    }

    private static unsafe void VerifyPointerCastDoubleToUInt32()
    {
        double value = -33.5;
        double* source = &value;
        uint* converted = (uint*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to uint and back");
    }

    private static unsafe void VerifyPointerCastDoubleToInt64()
    {
        double value = -33.5;
        double* source = &value;
        long* converted = (long*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to long and back");
    }

    private static unsafe void VerifyPointerCastDoubleToUInt64()
    {
        double value = -33.5;
        double* source = &value;
        ulong* converted = (ulong*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to ulong and back");
    }

    private static unsafe void VerifyPointerCastDoubleToNativeInt()
    {
        double value = -33.5;
        double* source = &value;
        nint* converted = (nint*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to nint and back");
    }

    private static unsafe void VerifyPointerCastDoubleToNativeUInt()
    {
        double value = -33.5;
        double* source = &value;
        nuint* converted = (nuint*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to nuint and back");
    }

    private static unsafe void VerifyPointerCastDoubleToSingle()
    {
        double value = -33.5;
        double* source = &value;
        float* converted = (float*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to float and back");
    }

    private static unsafe void VerifyPointerCastDoubleToDouble()
    {
        double value = -33.5;
        double* source = &value;
        double* converted = (double*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to double and back");
    }

    private static unsafe void VerifyPointerCastDoubleToBoolean()
    {
        double value = -33.5;
        double* source = &value;
        bool* converted = (bool*)(void*)source;
        double* restored = (double*)(void*)converted;
        if (restored != source || *restored != -33.5)
            Fail("generated pointer cast double to bool and back");
    }

    private static unsafe void VerifyPointerCastBooleanToSByte()
    {
        bool value = true;
        bool* source = &value;
        sbyte* converted = (sbyte*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to sbyte and back");
    }

    private static unsafe void VerifyPointerCastBooleanToByte()
    {
        bool value = true;
        bool* source = &value;
        byte* converted = (byte*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to byte and back");
    }

    private static unsafe void VerifyPointerCastBooleanToInt16()
    {
        bool value = true;
        bool* source = &value;
        short* converted = (short*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to short and back");
    }

    private static unsafe void VerifyPointerCastBooleanToUInt16()
    {
        bool value = true;
        bool* source = &value;
        ushort* converted = (ushort*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to ushort and back");
    }

    private static unsafe void VerifyPointerCastBooleanToChar()
    {
        bool value = true;
        bool* source = &value;
        char* converted = (char*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to char and back");
    }

    private static unsafe void VerifyPointerCastBooleanToInt32()
    {
        bool value = true;
        bool* source = &value;
        int* converted = (int*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to int and back");
    }

    private static unsafe void VerifyPointerCastBooleanToUInt32()
    {
        bool value = true;
        bool* source = &value;
        uint* converted = (uint*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to uint and back");
    }

    private static unsafe void VerifyPointerCastBooleanToInt64()
    {
        bool value = true;
        bool* source = &value;
        long* converted = (long*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to long and back");
    }

    private static unsafe void VerifyPointerCastBooleanToUInt64()
    {
        bool value = true;
        bool* source = &value;
        ulong* converted = (ulong*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to ulong and back");
    }

    private static unsafe void VerifyPointerCastBooleanToNativeInt()
    {
        bool value = true;
        bool* source = &value;
        nint* converted = (nint*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to nint and back");
    }

    private static unsafe void VerifyPointerCastBooleanToNativeUInt()
    {
        bool value = true;
        bool* source = &value;
        nuint* converted = (nuint*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to nuint and back");
    }

    private static unsafe void VerifyPointerCastBooleanToSingle()
    {
        bool value = true;
        bool* source = &value;
        float* converted = (float*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to float and back");
    }

    private static unsafe void VerifyPointerCastBooleanToDouble()
    {
        bool value = true;
        bool* source = &value;
        double* converted = (double*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to double and back");
    }

    private static unsafe void VerifyPointerCastBooleanToBoolean()
    {
        bool value = true;
        bool* source = &value;
        bool* converted = (bool*)(void*)source;
        bool* restored = (bool*)(void*)converted;
        if (restored != source || *restored != true)
            Fail("generated pointer cast bool to bool and back");
    }

    private static void VerifyGeneratedFunctionPointers()
    {
        VerifyFunctionPointerSByteToSByte();
        VerifyFunctionPointerSByteToByte();
        VerifyFunctionPointerSByteToInt16();
        VerifyFunctionPointerSByteToUInt16();
        VerifyFunctionPointerSByteToChar();
        VerifyFunctionPointerSByteToInt32();
        VerifyFunctionPointerSByteToUInt32();
        VerifyFunctionPointerSByteToInt64();
        VerifyFunctionPointerSByteToUInt64();
        VerifyFunctionPointerSByteToNativeInt();
        VerifyFunctionPointerSByteToNativeUInt();
        VerifyFunctionPointerSByteToSingle();
        VerifyFunctionPointerSByteToDouble();
        VerifyFunctionPointerByteToSByte();
        VerifyFunctionPointerByteToByte();
        VerifyFunctionPointerByteToInt16();
        VerifyFunctionPointerByteToUInt16();
        VerifyFunctionPointerByteToChar();
        VerifyFunctionPointerByteToInt32();
        VerifyFunctionPointerByteToUInt32();
        VerifyFunctionPointerByteToInt64();
        VerifyFunctionPointerByteToUInt64();
        VerifyFunctionPointerByteToNativeInt();
        VerifyFunctionPointerByteToNativeUInt();
        VerifyFunctionPointerByteToSingle();
        VerifyFunctionPointerByteToDouble();
        VerifyFunctionPointerInt16ToSByte();
        VerifyFunctionPointerInt16ToByte();
        VerifyFunctionPointerInt16ToInt16();
        VerifyFunctionPointerInt16ToUInt16();
        VerifyFunctionPointerInt16ToChar();
        VerifyFunctionPointerInt16ToInt32();
        VerifyFunctionPointerInt16ToUInt32();
        VerifyFunctionPointerInt16ToInt64();
        VerifyFunctionPointerInt16ToUInt64();
        VerifyFunctionPointerInt16ToNativeInt();
        VerifyFunctionPointerInt16ToNativeUInt();
        VerifyFunctionPointerInt16ToSingle();
        VerifyFunctionPointerInt16ToDouble();
        VerifyFunctionPointerUInt16ToSByte();
        VerifyFunctionPointerUInt16ToByte();
        VerifyFunctionPointerUInt16ToInt16();
        VerifyFunctionPointerUInt16ToUInt16();
        VerifyFunctionPointerUInt16ToChar();
        VerifyFunctionPointerUInt16ToInt32();
        VerifyFunctionPointerUInt16ToUInt32();
        VerifyFunctionPointerUInt16ToInt64();
        VerifyFunctionPointerUInt16ToUInt64();
        VerifyFunctionPointerUInt16ToNativeInt();
        VerifyFunctionPointerUInt16ToNativeUInt();
        VerifyFunctionPointerUInt16ToSingle();
        VerifyFunctionPointerUInt16ToDouble();
        VerifyFunctionPointerCharToSByte();
        VerifyFunctionPointerCharToByte();
        VerifyFunctionPointerCharToInt16();
        VerifyFunctionPointerCharToUInt16();
        VerifyFunctionPointerCharToChar();
        VerifyFunctionPointerCharToInt32();
        VerifyFunctionPointerCharToUInt32();
        VerifyFunctionPointerCharToInt64();
        VerifyFunctionPointerCharToUInt64();
        VerifyFunctionPointerCharToNativeInt();
        VerifyFunctionPointerCharToNativeUInt();
        VerifyFunctionPointerCharToSingle();
        VerifyFunctionPointerCharToDouble();
        VerifyFunctionPointerInt32ToSByte();
        VerifyFunctionPointerInt32ToByte();
        VerifyFunctionPointerInt32ToInt16();
        VerifyFunctionPointerInt32ToUInt16();
        VerifyFunctionPointerInt32ToChar();
        VerifyFunctionPointerInt32ToInt32();
        VerifyFunctionPointerInt32ToUInt32();
        VerifyFunctionPointerInt32ToInt64();
        VerifyFunctionPointerInt32ToUInt64();
        VerifyFunctionPointerInt32ToNativeInt();
        VerifyFunctionPointerInt32ToNativeUInt();
        VerifyFunctionPointerInt32ToSingle();
        VerifyFunctionPointerInt32ToDouble();
        VerifyFunctionPointerUInt32ToSByte();
        VerifyFunctionPointerUInt32ToByte();
        VerifyFunctionPointerUInt32ToInt16();
        VerifyFunctionPointerUInt32ToUInt16();
        VerifyFunctionPointerUInt32ToChar();
        VerifyFunctionPointerUInt32ToInt32();
        VerifyFunctionPointerUInt32ToUInt32();
        VerifyFunctionPointerUInt32ToInt64();
        VerifyFunctionPointerUInt32ToUInt64();
        VerifyFunctionPointerUInt32ToNativeInt();
        VerifyFunctionPointerUInt32ToNativeUInt();
        VerifyFunctionPointerUInt32ToSingle();
        VerifyFunctionPointerUInt32ToDouble();
        VerifyFunctionPointerInt64ToSByte();
        VerifyFunctionPointerInt64ToByte();
        VerifyFunctionPointerInt64ToInt16();
        VerifyFunctionPointerInt64ToUInt16();
        VerifyFunctionPointerInt64ToChar();
        VerifyFunctionPointerInt64ToInt32();
        VerifyFunctionPointerInt64ToUInt32();
        VerifyFunctionPointerInt64ToInt64();
        VerifyFunctionPointerInt64ToUInt64();
        VerifyFunctionPointerInt64ToNativeInt();
        VerifyFunctionPointerInt64ToNativeUInt();
        VerifyFunctionPointerInt64ToSingle();
        VerifyFunctionPointerInt64ToDouble();
        VerifyFunctionPointerUInt64ToSByte();
        VerifyFunctionPointerUInt64ToByte();
        VerifyFunctionPointerUInt64ToInt16();
        VerifyFunctionPointerUInt64ToUInt16();
        VerifyFunctionPointerUInt64ToChar();
        VerifyFunctionPointerUInt64ToInt32();
        VerifyFunctionPointerUInt64ToUInt32();
        VerifyFunctionPointerUInt64ToInt64();
        VerifyFunctionPointerUInt64ToUInt64();
        VerifyFunctionPointerUInt64ToNativeInt();
        VerifyFunctionPointerUInt64ToNativeUInt();
        VerifyFunctionPointerUInt64ToSingle();
        VerifyFunctionPointerUInt64ToDouble();
        VerifyFunctionPointerNativeIntToSByte();
        VerifyFunctionPointerNativeIntToByte();
        VerifyFunctionPointerNativeIntToInt16();
        VerifyFunctionPointerNativeIntToUInt16();
        VerifyFunctionPointerNativeIntToChar();
        VerifyFunctionPointerNativeIntToInt32();
        VerifyFunctionPointerNativeIntToUInt32();
        VerifyFunctionPointerNativeIntToInt64();
        VerifyFunctionPointerNativeIntToUInt64();
        VerifyFunctionPointerNativeIntToNativeInt();
        VerifyFunctionPointerNativeIntToNativeUInt();
        VerifyFunctionPointerNativeIntToSingle();
        VerifyFunctionPointerNativeIntToDouble();
        VerifyFunctionPointerNativeUIntToSByte();
        VerifyFunctionPointerNativeUIntToByte();
        VerifyFunctionPointerNativeUIntToInt16();
        VerifyFunctionPointerNativeUIntToUInt16();
        VerifyFunctionPointerNativeUIntToChar();
        VerifyFunctionPointerNativeUIntToInt32();
        VerifyFunctionPointerNativeUIntToUInt32();
        VerifyFunctionPointerNativeUIntToInt64();
        VerifyFunctionPointerNativeUIntToUInt64();
        VerifyFunctionPointerNativeUIntToNativeInt();
        VerifyFunctionPointerNativeUIntToNativeUInt();
        VerifyFunctionPointerNativeUIntToSingle();
        VerifyFunctionPointerNativeUIntToDouble();
        VerifyFunctionPointerSingleToSByte();
        VerifyFunctionPointerSingleToByte();
        VerifyFunctionPointerSingleToInt16();
        VerifyFunctionPointerSingleToUInt16();
        VerifyFunctionPointerSingleToChar();
        VerifyFunctionPointerSingleToInt32();
        VerifyFunctionPointerSingleToUInt32();
        VerifyFunctionPointerSingleToInt64();
        VerifyFunctionPointerSingleToUInt64();
        VerifyFunctionPointerSingleToNativeInt();
        VerifyFunctionPointerSingleToNativeUInt();
        VerifyFunctionPointerSingleToSingle();
        VerifyFunctionPointerSingleToDouble();
        VerifyFunctionPointerDoubleToSByte();
        VerifyFunctionPointerDoubleToByte();
        VerifyFunctionPointerDoubleToInt16();
        VerifyFunctionPointerDoubleToUInt16();
        VerifyFunctionPointerDoubleToChar();
        VerifyFunctionPointerDoubleToInt32();
        VerifyFunctionPointerDoubleToUInt32();
        VerifyFunctionPointerDoubleToInt64();
        VerifyFunctionPointerDoubleToUInt64();
        VerifyFunctionPointerDoubleToNativeInt();
        VerifyFunctionPointerDoubleToNativeUInt();
        VerifyFunctionPointerDoubleToSingle();
        VerifyFunctionPointerDoubleToDouble();
    }

    private static sbyte ManagedFunctionSByteToSByte(sbyte value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionSByteToSByte(sbyte value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerSByteToSByte()
    {
        sbyte input = (sbyte)1;
        sbyte expected = (sbyte)1;
        delegate* managed<sbyte, sbyte> managed = &ManagedFunctionSByteToSByte;
        delegate* unmanaged<sbyte, sbyte> unmanaged = &UnmanagedFunctionSByteToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, sbyte> restoredManaged = (delegate* managed<sbyte, sbyte>)(void*)managedAddress;
        delegate* unmanaged<sbyte, sbyte> restoredUnmanaged = (delegate* unmanaged<sbyte, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to sbyte return");
    }

    private static byte ManagedFunctionSByteToByte(sbyte value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionSByteToByte(sbyte value) => (byte)value;

    private static unsafe void VerifyFunctionPointerSByteToByte()
    {
        sbyte input = (sbyte)1;
        byte expected = (byte)1;
        delegate* managed<sbyte, byte> managed = &ManagedFunctionSByteToByte;
        delegate* unmanaged<sbyte, byte> unmanaged = &UnmanagedFunctionSByteToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, byte> restoredManaged = (delegate* managed<sbyte, byte>)(void*)managedAddress;
        delegate* unmanaged<sbyte, byte> restoredUnmanaged = (delegate* unmanaged<sbyte, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to byte return");
    }

    private static short ManagedFunctionSByteToInt16(sbyte value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionSByteToInt16(sbyte value) => (short)value;

    private static unsafe void VerifyFunctionPointerSByteToInt16()
    {
        sbyte input = (sbyte)1;
        short expected = (short)1;
        delegate* managed<sbyte, short> managed = &ManagedFunctionSByteToInt16;
        delegate* unmanaged<sbyte, short> unmanaged = &UnmanagedFunctionSByteToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, short> restoredManaged = (delegate* managed<sbyte, short>)(void*)managedAddress;
        delegate* unmanaged<sbyte, short> restoredUnmanaged = (delegate* unmanaged<sbyte, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to short return");
    }

    private static ushort ManagedFunctionSByteToUInt16(sbyte value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionSByteToUInt16(sbyte value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerSByteToUInt16()
    {
        sbyte input = (sbyte)1;
        ushort expected = (ushort)1;
        delegate* managed<sbyte, ushort> managed = &ManagedFunctionSByteToUInt16;
        delegate* unmanaged<sbyte, ushort> unmanaged = &UnmanagedFunctionSByteToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, ushort> restoredManaged = (delegate* managed<sbyte, ushort>)(void*)managedAddress;
        delegate* unmanaged<sbyte, ushort> restoredUnmanaged = (delegate* unmanaged<sbyte, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to ushort return");
    }

    private static char ManagedFunctionSByteToChar(sbyte value) => (char)value;

    private static unsafe void VerifyFunctionPointerSByteToChar()
    {
        sbyte input = (sbyte)1;
        char expected = (char)1;
        delegate* managed<sbyte, char> managed = &ManagedFunctionSByteToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<sbyte, char> restoredManaged = (delegate* managed<sbyte, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer sbyte parameter to char return");
    }

    private static int ManagedFunctionSByteToInt32(sbyte value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionSByteToInt32(sbyte value) => (int)value;

    private static unsafe void VerifyFunctionPointerSByteToInt32()
    {
        sbyte input = (sbyte)1;
        int expected = (int)1;
        delegate* managed<sbyte, int> managed = &ManagedFunctionSByteToInt32;
        delegate* unmanaged<sbyte, int> unmanaged = &UnmanagedFunctionSByteToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, int> restoredManaged = (delegate* managed<sbyte, int>)(void*)managedAddress;
        delegate* unmanaged<sbyte, int> restoredUnmanaged = (delegate* unmanaged<sbyte, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to int return");
    }

    private static uint ManagedFunctionSByteToUInt32(sbyte value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionSByteToUInt32(sbyte value) => (uint)value;

    private static unsafe void VerifyFunctionPointerSByteToUInt32()
    {
        sbyte input = (sbyte)1;
        uint expected = (uint)1;
        delegate* managed<sbyte, uint> managed = &ManagedFunctionSByteToUInt32;
        delegate* unmanaged<sbyte, uint> unmanaged = &UnmanagedFunctionSByteToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, uint> restoredManaged = (delegate* managed<sbyte, uint>)(void*)managedAddress;
        delegate* unmanaged<sbyte, uint> restoredUnmanaged = (delegate* unmanaged<sbyte, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to uint return");
    }

    private static long ManagedFunctionSByteToInt64(sbyte value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionSByteToInt64(sbyte value) => (long)value;

    private static unsafe void VerifyFunctionPointerSByteToInt64()
    {
        sbyte input = (sbyte)1;
        long expected = (long)1;
        delegate* managed<sbyte, long> managed = &ManagedFunctionSByteToInt64;
        delegate* unmanaged<sbyte, long> unmanaged = &UnmanagedFunctionSByteToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, long> restoredManaged = (delegate* managed<sbyte, long>)(void*)managedAddress;
        delegate* unmanaged<sbyte, long> restoredUnmanaged = (delegate* unmanaged<sbyte, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to long return");
    }

    private static ulong ManagedFunctionSByteToUInt64(sbyte value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionSByteToUInt64(sbyte value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerSByteToUInt64()
    {
        sbyte input = (sbyte)1;
        ulong expected = (ulong)1;
        delegate* managed<sbyte, ulong> managed = &ManagedFunctionSByteToUInt64;
        delegate* unmanaged<sbyte, ulong> unmanaged = &UnmanagedFunctionSByteToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, ulong> restoredManaged = (delegate* managed<sbyte, ulong>)(void*)managedAddress;
        delegate* unmanaged<sbyte, ulong> restoredUnmanaged = (delegate* unmanaged<sbyte, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to ulong return");
    }

    private static nint ManagedFunctionSByteToNativeInt(sbyte value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionSByteToNativeInt(sbyte value) => (nint)value;

    private static unsafe void VerifyFunctionPointerSByteToNativeInt()
    {
        sbyte input = (sbyte)1;
        nint expected = (nint)1;
        delegate* managed<sbyte, nint> managed = &ManagedFunctionSByteToNativeInt;
        delegate* unmanaged<sbyte, nint> unmanaged = &UnmanagedFunctionSByteToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, nint> restoredManaged = (delegate* managed<sbyte, nint>)(void*)managedAddress;
        delegate* unmanaged<sbyte, nint> restoredUnmanaged = (delegate* unmanaged<sbyte, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to nint return");
    }

    private static nuint ManagedFunctionSByteToNativeUInt(sbyte value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionSByteToNativeUInt(sbyte value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerSByteToNativeUInt()
    {
        sbyte input = (sbyte)1;
        nuint expected = (nuint)1;
        delegate* managed<sbyte, nuint> managed = &ManagedFunctionSByteToNativeUInt;
        delegate* unmanaged<sbyte, nuint> unmanaged = &UnmanagedFunctionSByteToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, nuint> restoredManaged = (delegate* managed<sbyte, nuint>)(void*)managedAddress;
        delegate* unmanaged<sbyte, nuint> restoredUnmanaged = (delegate* unmanaged<sbyte, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to nuint return");
    }

    private static float ManagedFunctionSByteToSingle(sbyte value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionSByteToSingle(sbyte value) => (float)value;

    private static unsafe void VerifyFunctionPointerSByteToSingle()
    {
        sbyte input = (sbyte)1;
        float expected = (float)1;
        delegate* managed<sbyte, float> managed = &ManagedFunctionSByteToSingle;
        delegate* unmanaged<sbyte, float> unmanaged = &UnmanagedFunctionSByteToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, float> restoredManaged = (delegate* managed<sbyte, float>)(void*)managedAddress;
        delegate* unmanaged<sbyte, float> restoredUnmanaged = (delegate* unmanaged<sbyte, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to float return");
    }

    private static double ManagedFunctionSByteToDouble(sbyte value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionSByteToDouble(sbyte value) => (double)value;

    private static unsafe void VerifyFunctionPointerSByteToDouble()
    {
        sbyte input = (sbyte)1;
        double expected = (double)1;
        delegate* managed<sbyte, double> managed = &ManagedFunctionSByteToDouble;
        delegate* unmanaged<sbyte, double> unmanaged = &UnmanagedFunctionSByteToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<sbyte, double> restoredManaged = (delegate* managed<sbyte, double>)(void*)managedAddress;
        delegate* unmanaged<sbyte, double> restoredUnmanaged = (delegate* unmanaged<sbyte, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer sbyte parameter to double return");
    }

    private static sbyte ManagedFunctionByteToSByte(byte value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionByteToSByte(byte value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerByteToSByte()
    {
        byte input = (byte)1;
        sbyte expected = (sbyte)1;
        delegate* managed<byte, sbyte> managed = &ManagedFunctionByteToSByte;
        delegate* unmanaged<byte, sbyte> unmanaged = &UnmanagedFunctionByteToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, sbyte> restoredManaged = (delegate* managed<byte, sbyte>)(void*)managedAddress;
        delegate* unmanaged<byte, sbyte> restoredUnmanaged = (delegate* unmanaged<byte, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to sbyte return");
    }

    private static byte ManagedFunctionByteToByte(byte value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionByteToByte(byte value) => (byte)value;

    private static unsafe void VerifyFunctionPointerByteToByte()
    {
        byte input = (byte)1;
        byte expected = (byte)1;
        delegate* managed<byte, byte> managed = &ManagedFunctionByteToByte;
        delegate* unmanaged<byte, byte> unmanaged = &UnmanagedFunctionByteToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, byte> restoredManaged = (delegate* managed<byte, byte>)(void*)managedAddress;
        delegate* unmanaged<byte, byte> restoredUnmanaged = (delegate* unmanaged<byte, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to byte return");
    }

    private static short ManagedFunctionByteToInt16(byte value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionByteToInt16(byte value) => (short)value;

    private static unsafe void VerifyFunctionPointerByteToInt16()
    {
        byte input = (byte)1;
        short expected = (short)1;
        delegate* managed<byte, short> managed = &ManagedFunctionByteToInt16;
        delegate* unmanaged<byte, short> unmanaged = &UnmanagedFunctionByteToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, short> restoredManaged = (delegate* managed<byte, short>)(void*)managedAddress;
        delegate* unmanaged<byte, short> restoredUnmanaged = (delegate* unmanaged<byte, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to short return");
    }

    private static ushort ManagedFunctionByteToUInt16(byte value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionByteToUInt16(byte value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerByteToUInt16()
    {
        byte input = (byte)1;
        ushort expected = (ushort)1;
        delegate* managed<byte, ushort> managed = &ManagedFunctionByteToUInt16;
        delegate* unmanaged<byte, ushort> unmanaged = &UnmanagedFunctionByteToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, ushort> restoredManaged = (delegate* managed<byte, ushort>)(void*)managedAddress;
        delegate* unmanaged<byte, ushort> restoredUnmanaged = (delegate* unmanaged<byte, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to ushort return");
    }

    private static char ManagedFunctionByteToChar(byte value) => (char)value;

    private static unsafe void VerifyFunctionPointerByteToChar()
    {
        byte input = (byte)1;
        char expected = (char)1;
        delegate* managed<byte, char> managed = &ManagedFunctionByteToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<byte, char> restoredManaged = (delegate* managed<byte, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer byte parameter to char return");
    }

    private static int ManagedFunctionByteToInt32(byte value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionByteToInt32(byte value) => (int)value;

    private static unsafe void VerifyFunctionPointerByteToInt32()
    {
        byte input = (byte)1;
        int expected = (int)1;
        delegate* managed<byte, int> managed = &ManagedFunctionByteToInt32;
        delegate* unmanaged<byte, int> unmanaged = &UnmanagedFunctionByteToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, int> restoredManaged = (delegate* managed<byte, int>)(void*)managedAddress;
        delegate* unmanaged<byte, int> restoredUnmanaged = (delegate* unmanaged<byte, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to int return");
    }

    private static uint ManagedFunctionByteToUInt32(byte value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionByteToUInt32(byte value) => (uint)value;

    private static unsafe void VerifyFunctionPointerByteToUInt32()
    {
        byte input = (byte)1;
        uint expected = (uint)1;
        delegate* managed<byte, uint> managed = &ManagedFunctionByteToUInt32;
        delegate* unmanaged<byte, uint> unmanaged = &UnmanagedFunctionByteToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, uint> restoredManaged = (delegate* managed<byte, uint>)(void*)managedAddress;
        delegate* unmanaged<byte, uint> restoredUnmanaged = (delegate* unmanaged<byte, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to uint return");
    }

    private static long ManagedFunctionByteToInt64(byte value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionByteToInt64(byte value) => (long)value;

    private static unsafe void VerifyFunctionPointerByteToInt64()
    {
        byte input = (byte)1;
        long expected = (long)1;
        delegate* managed<byte, long> managed = &ManagedFunctionByteToInt64;
        delegate* unmanaged<byte, long> unmanaged = &UnmanagedFunctionByteToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, long> restoredManaged = (delegate* managed<byte, long>)(void*)managedAddress;
        delegate* unmanaged<byte, long> restoredUnmanaged = (delegate* unmanaged<byte, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to long return");
    }

    private static ulong ManagedFunctionByteToUInt64(byte value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionByteToUInt64(byte value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerByteToUInt64()
    {
        byte input = (byte)1;
        ulong expected = (ulong)1;
        delegate* managed<byte, ulong> managed = &ManagedFunctionByteToUInt64;
        delegate* unmanaged<byte, ulong> unmanaged = &UnmanagedFunctionByteToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, ulong> restoredManaged = (delegate* managed<byte, ulong>)(void*)managedAddress;
        delegate* unmanaged<byte, ulong> restoredUnmanaged = (delegate* unmanaged<byte, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to ulong return");
    }

    private static nint ManagedFunctionByteToNativeInt(byte value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionByteToNativeInt(byte value) => (nint)value;

    private static unsafe void VerifyFunctionPointerByteToNativeInt()
    {
        byte input = (byte)1;
        nint expected = (nint)1;
        delegate* managed<byte, nint> managed = &ManagedFunctionByteToNativeInt;
        delegate* unmanaged<byte, nint> unmanaged = &UnmanagedFunctionByteToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, nint> restoredManaged = (delegate* managed<byte, nint>)(void*)managedAddress;
        delegate* unmanaged<byte, nint> restoredUnmanaged = (delegate* unmanaged<byte, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to nint return");
    }

    private static nuint ManagedFunctionByteToNativeUInt(byte value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionByteToNativeUInt(byte value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerByteToNativeUInt()
    {
        byte input = (byte)1;
        nuint expected = (nuint)1;
        delegate* managed<byte, nuint> managed = &ManagedFunctionByteToNativeUInt;
        delegate* unmanaged<byte, nuint> unmanaged = &UnmanagedFunctionByteToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, nuint> restoredManaged = (delegate* managed<byte, nuint>)(void*)managedAddress;
        delegate* unmanaged<byte, nuint> restoredUnmanaged = (delegate* unmanaged<byte, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to nuint return");
    }

    private static float ManagedFunctionByteToSingle(byte value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionByteToSingle(byte value) => (float)value;

    private static unsafe void VerifyFunctionPointerByteToSingle()
    {
        byte input = (byte)1;
        float expected = (float)1;
        delegate* managed<byte, float> managed = &ManagedFunctionByteToSingle;
        delegate* unmanaged<byte, float> unmanaged = &UnmanagedFunctionByteToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, float> restoredManaged = (delegate* managed<byte, float>)(void*)managedAddress;
        delegate* unmanaged<byte, float> restoredUnmanaged = (delegate* unmanaged<byte, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to float return");
    }

    private static double ManagedFunctionByteToDouble(byte value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionByteToDouble(byte value) => (double)value;

    private static unsafe void VerifyFunctionPointerByteToDouble()
    {
        byte input = (byte)1;
        double expected = (double)1;
        delegate* managed<byte, double> managed = &ManagedFunctionByteToDouble;
        delegate* unmanaged<byte, double> unmanaged = &UnmanagedFunctionByteToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<byte, double> restoredManaged = (delegate* managed<byte, double>)(void*)managedAddress;
        delegate* unmanaged<byte, double> restoredUnmanaged = (delegate* unmanaged<byte, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer byte parameter to double return");
    }

    private static sbyte ManagedFunctionInt16ToSByte(short value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionInt16ToSByte(short value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerInt16ToSByte()
    {
        short input = (short)1;
        sbyte expected = (sbyte)1;
        delegate* managed<short, sbyte> managed = &ManagedFunctionInt16ToSByte;
        delegate* unmanaged<short, sbyte> unmanaged = &UnmanagedFunctionInt16ToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, sbyte> restoredManaged = (delegate* managed<short, sbyte>)(void*)managedAddress;
        delegate* unmanaged<short, sbyte> restoredUnmanaged = (delegate* unmanaged<short, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to sbyte return");
    }

    private static byte ManagedFunctionInt16ToByte(short value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionInt16ToByte(short value) => (byte)value;

    private static unsafe void VerifyFunctionPointerInt16ToByte()
    {
        short input = (short)1;
        byte expected = (byte)1;
        delegate* managed<short, byte> managed = &ManagedFunctionInt16ToByte;
        delegate* unmanaged<short, byte> unmanaged = &UnmanagedFunctionInt16ToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, byte> restoredManaged = (delegate* managed<short, byte>)(void*)managedAddress;
        delegate* unmanaged<short, byte> restoredUnmanaged = (delegate* unmanaged<short, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to byte return");
    }

    private static short ManagedFunctionInt16ToInt16(short value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionInt16ToInt16(short value) => (short)value;

    private static unsafe void VerifyFunctionPointerInt16ToInt16()
    {
        short input = (short)1;
        short expected = (short)1;
        delegate* managed<short, short> managed = &ManagedFunctionInt16ToInt16;
        delegate* unmanaged<short, short> unmanaged = &UnmanagedFunctionInt16ToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, short> restoredManaged = (delegate* managed<short, short>)(void*)managedAddress;
        delegate* unmanaged<short, short> restoredUnmanaged = (delegate* unmanaged<short, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to short return");
    }

    private static ushort ManagedFunctionInt16ToUInt16(short value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionInt16ToUInt16(short value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerInt16ToUInt16()
    {
        short input = (short)1;
        ushort expected = (ushort)1;
        delegate* managed<short, ushort> managed = &ManagedFunctionInt16ToUInt16;
        delegate* unmanaged<short, ushort> unmanaged = &UnmanagedFunctionInt16ToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, ushort> restoredManaged = (delegate* managed<short, ushort>)(void*)managedAddress;
        delegate* unmanaged<short, ushort> restoredUnmanaged = (delegate* unmanaged<short, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to ushort return");
    }

    private static char ManagedFunctionInt16ToChar(short value) => (char)value;

    private static unsafe void VerifyFunctionPointerInt16ToChar()
    {
        short input = (short)1;
        char expected = (char)1;
        delegate* managed<short, char> managed = &ManagedFunctionInt16ToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<short, char> restoredManaged = (delegate* managed<short, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer short parameter to char return");
    }

    private static int ManagedFunctionInt16ToInt32(short value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionInt16ToInt32(short value) => (int)value;

    private static unsafe void VerifyFunctionPointerInt16ToInt32()
    {
        short input = (short)1;
        int expected = (int)1;
        delegate* managed<short, int> managed = &ManagedFunctionInt16ToInt32;
        delegate* unmanaged<short, int> unmanaged = &UnmanagedFunctionInt16ToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, int> restoredManaged = (delegate* managed<short, int>)(void*)managedAddress;
        delegate* unmanaged<short, int> restoredUnmanaged = (delegate* unmanaged<short, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to int return");
    }

    private static uint ManagedFunctionInt16ToUInt32(short value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionInt16ToUInt32(short value) => (uint)value;

    private static unsafe void VerifyFunctionPointerInt16ToUInt32()
    {
        short input = (short)1;
        uint expected = (uint)1;
        delegate* managed<short, uint> managed = &ManagedFunctionInt16ToUInt32;
        delegate* unmanaged<short, uint> unmanaged = &UnmanagedFunctionInt16ToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, uint> restoredManaged = (delegate* managed<short, uint>)(void*)managedAddress;
        delegate* unmanaged<short, uint> restoredUnmanaged = (delegate* unmanaged<short, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to uint return");
    }

    private static long ManagedFunctionInt16ToInt64(short value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionInt16ToInt64(short value) => (long)value;

    private static unsafe void VerifyFunctionPointerInt16ToInt64()
    {
        short input = (short)1;
        long expected = (long)1;
        delegate* managed<short, long> managed = &ManagedFunctionInt16ToInt64;
        delegate* unmanaged<short, long> unmanaged = &UnmanagedFunctionInt16ToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, long> restoredManaged = (delegate* managed<short, long>)(void*)managedAddress;
        delegate* unmanaged<short, long> restoredUnmanaged = (delegate* unmanaged<short, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to long return");
    }

    private static ulong ManagedFunctionInt16ToUInt64(short value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionInt16ToUInt64(short value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerInt16ToUInt64()
    {
        short input = (short)1;
        ulong expected = (ulong)1;
        delegate* managed<short, ulong> managed = &ManagedFunctionInt16ToUInt64;
        delegate* unmanaged<short, ulong> unmanaged = &UnmanagedFunctionInt16ToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, ulong> restoredManaged = (delegate* managed<short, ulong>)(void*)managedAddress;
        delegate* unmanaged<short, ulong> restoredUnmanaged = (delegate* unmanaged<short, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to ulong return");
    }

    private static nint ManagedFunctionInt16ToNativeInt(short value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionInt16ToNativeInt(short value) => (nint)value;

    private static unsafe void VerifyFunctionPointerInt16ToNativeInt()
    {
        short input = (short)1;
        nint expected = (nint)1;
        delegate* managed<short, nint> managed = &ManagedFunctionInt16ToNativeInt;
        delegate* unmanaged<short, nint> unmanaged = &UnmanagedFunctionInt16ToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, nint> restoredManaged = (delegate* managed<short, nint>)(void*)managedAddress;
        delegate* unmanaged<short, nint> restoredUnmanaged = (delegate* unmanaged<short, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to nint return");
    }

    private static nuint ManagedFunctionInt16ToNativeUInt(short value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionInt16ToNativeUInt(short value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerInt16ToNativeUInt()
    {
        short input = (short)1;
        nuint expected = (nuint)1;
        delegate* managed<short, nuint> managed = &ManagedFunctionInt16ToNativeUInt;
        delegate* unmanaged<short, nuint> unmanaged = &UnmanagedFunctionInt16ToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, nuint> restoredManaged = (delegate* managed<short, nuint>)(void*)managedAddress;
        delegate* unmanaged<short, nuint> restoredUnmanaged = (delegate* unmanaged<short, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to nuint return");
    }

    private static float ManagedFunctionInt16ToSingle(short value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionInt16ToSingle(short value) => (float)value;

    private static unsafe void VerifyFunctionPointerInt16ToSingle()
    {
        short input = (short)1;
        float expected = (float)1;
        delegate* managed<short, float> managed = &ManagedFunctionInt16ToSingle;
        delegate* unmanaged<short, float> unmanaged = &UnmanagedFunctionInt16ToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, float> restoredManaged = (delegate* managed<short, float>)(void*)managedAddress;
        delegate* unmanaged<short, float> restoredUnmanaged = (delegate* unmanaged<short, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to float return");
    }

    private static double ManagedFunctionInt16ToDouble(short value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionInt16ToDouble(short value) => (double)value;

    private static unsafe void VerifyFunctionPointerInt16ToDouble()
    {
        short input = (short)1;
        double expected = (double)1;
        delegate* managed<short, double> managed = &ManagedFunctionInt16ToDouble;
        delegate* unmanaged<short, double> unmanaged = &UnmanagedFunctionInt16ToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<short, double> restoredManaged = (delegate* managed<short, double>)(void*)managedAddress;
        delegate* unmanaged<short, double> restoredUnmanaged = (delegate* unmanaged<short, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer short parameter to double return");
    }

    private static sbyte ManagedFunctionUInt16ToSByte(ushort value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionUInt16ToSByte(ushort value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerUInt16ToSByte()
    {
        ushort input = (ushort)1;
        sbyte expected = (sbyte)1;
        delegate* managed<ushort, sbyte> managed = &ManagedFunctionUInt16ToSByte;
        delegate* unmanaged<ushort, sbyte> unmanaged = &UnmanagedFunctionUInt16ToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, sbyte> restoredManaged = (delegate* managed<ushort, sbyte>)(void*)managedAddress;
        delegate* unmanaged<ushort, sbyte> restoredUnmanaged = (delegate* unmanaged<ushort, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to sbyte return");
    }

    private static byte ManagedFunctionUInt16ToByte(ushort value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionUInt16ToByte(ushort value) => (byte)value;

    private static unsafe void VerifyFunctionPointerUInt16ToByte()
    {
        ushort input = (ushort)1;
        byte expected = (byte)1;
        delegate* managed<ushort, byte> managed = &ManagedFunctionUInt16ToByte;
        delegate* unmanaged<ushort, byte> unmanaged = &UnmanagedFunctionUInt16ToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, byte> restoredManaged = (delegate* managed<ushort, byte>)(void*)managedAddress;
        delegate* unmanaged<ushort, byte> restoredUnmanaged = (delegate* unmanaged<ushort, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to byte return");
    }

    private static short ManagedFunctionUInt16ToInt16(ushort value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionUInt16ToInt16(ushort value) => (short)value;

    private static unsafe void VerifyFunctionPointerUInt16ToInt16()
    {
        ushort input = (ushort)1;
        short expected = (short)1;
        delegate* managed<ushort, short> managed = &ManagedFunctionUInt16ToInt16;
        delegate* unmanaged<ushort, short> unmanaged = &UnmanagedFunctionUInt16ToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, short> restoredManaged = (delegate* managed<ushort, short>)(void*)managedAddress;
        delegate* unmanaged<ushort, short> restoredUnmanaged = (delegate* unmanaged<ushort, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to short return");
    }

    private static ushort ManagedFunctionUInt16ToUInt16(ushort value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionUInt16ToUInt16(ushort value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerUInt16ToUInt16()
    {
        ushort input = (ushort)1;
        ushort expected = (ushort)1;
        delegate* managed<ushort, ushort> managed = &ManagedFunctionUInt16ToUInt16;
        delegate* unmanaged<ushort, ushort> unmanaged = &UnmanagedFunctionUInt16ToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, ushort> restoredManaged = (delegate* managed<ushort, ushort>)(void*)managedAddress;
        delegate* unmanaged<ushort, ushort> restoredUnmanaged = (delegate* unmanaged<ushort, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to ushort return");
    }

    private static char ManagedFunctionUInt16ToChar(ushort value) => (char)value;

    private static unsafe void VerifyFunctionPointerUInt16ToChar()
    {
        ushort input = (ushort)1;
        char expected = (char)1;
        delegate* managed<ushort, char> managed = &ManagedFunctionUInt16ToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<ushort, char> restoredManaged = (delegate* managed<ushort, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer ushort parameter to char return");
    }

    private static int ManagedFunctionUInt16ToInt32(ushort value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionUInt16ToInt32(ushort value) => (int)value;

    private static unsafe void VerifyFunctionPointerUInt16ToInt32()
    {
        ushort input = (ushort)1;
        int expected = (int)1;
        delegate* managed<ushort, int> managed = &ManagedFunctionUInt16ToInt32;
        delegate* unmanaged<ushort, int> unmanaged = &UnmanagedFunctionUInt16ToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, int> restoredManaged = (delegate* managed<ushort, int>)(void*)managedAddress;
        delegate* unmanaged<ushort, int> restoredUnmanaged = (delegate* unmanaged<ushort, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to int return");
    }

    private static uint ManagedFunctionUInt16ToUInt32(ushort value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionUInt16ToUInt32(ushort value) => (uint)value;

    private static unsafe void VerifyFunctionPointerUInt16ToUInt32()
    {
        ushort input = (ushort)1;
        uint expected = (uint)1;
        delegate* managed<ushort, uint> managed = &ManagedFunctionUInt16ToUInt32;
        delegate* unmanaged<ushort, uint> unmanaged = &UnmanagedFunctionUInt16ToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, uint> restoredManaged = (delegate* managed<ushort, uint>)(void*)managedAddress;
        delegate* unmanaged<ushort, uint> restoredUnmanaged = (delegate* unmanaged<ushort, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to uint return");
    }

    private static long ManagedFunctionUInt16ToInt64(ushort value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionUInt16ToInt64(ushort value) => (long)value;

    private static unsafe void VerifyFunctionPointerUInt16ToInt64()
    {
        ushort input = (ushort)1;
        long expected = (long)1;
        delegate* managed<ushort, long> managed = &ManagedFunctionUInt16ToInt64;
        delegate* unmanaged<ushort, long> unmanaged = &UnmanagedFunctionUInt16ToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, long> restoredManaged = (delegate* managed<ushort, long>)(void*)managedAddress;
        delegate* unmanaged<ushort, long> restoredUnmanaged = (delegate* unmanaged<ushort, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to long return");
    }

    private static ulong ManagedFunctionUInt16ToUInt64(ushort value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionUInt16ToUInt64(ushort value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerUInt16ToUInt64()
    {
        ushort input = (ushort)1;
        ulong expected = (ulong)1;
        delegate* managed<ushort, ulong> managed = &ManagedFunctionUInt16ToUInt64;
        delegate* unmanaged<ushort, ulong> unmanaged = &UnmanagedFunctionUInt16ToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, ulong> restoredManaged = (delegate* managed<ushort, ulong>)(void*)managedAddress;
        delegate* unmanaged<ushort, ulong> restoredUnmanaged = (delegate* unmanaged<ushort, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to ulong return");
    }

    private static nint ManagedFunctionUInt16ToNativeInt(ushort value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionUInt16ToNativeInt(ushort value) => (nint)value;

    private static unsafe void VerifyFunctionPointerUInt16ToNativeInt()
    {
        ushort input = (ushort)1;
        nint expected = (nint)1;
        delegate* managed<ushort, nint> managed = &ManagedFunctionUInt16ToNativeInt;
        delegate* unmanaged<ushort, nint> unmanaged = &UnmanagedFunctionUInt16ToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, nint> restoredManaged = (delegate* managed<ushort, nint>)(void*)managedAddress;
        delegate* unmanaged<ushort, nint> restoredUnmanaged = (delegate* unmanaged<ushort, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to nint return");
    }

    private static nuint ManagedFunctionUInt16ToNativeUInt(ushort value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionUInt16ToNativeUInt(ushort value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerUInt16ToNativeUInt()
    {
        ushort input = (ushort)1;
        nuint expected = (nuint)1;
        delegate* managed<ushort, nuint> managed = &ManagedFunctionUInt16ToNativeUInt;
        delegate* unmanaged<ushort, nuint> unmanaged = &UnmanagedFunctionUInt16ToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, nuint> restoredManaged = (delegate* managed<ushort, nuint>)(void*)managedAddress;
        delegate* unmanaged<ushort, nuint> restoredUnmanaged = (delegate* unmanaged<ushort, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to nuint return");
    }

    private static float ManagedFunctionUInt16ToSingle(ushort value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionUInt16ToSingle(ushort value) => (float)value;

    private static unsafe void VerifyFunctionPointerUInt16ToSingle()
    {
        ushort input = (ushort)1;
        float expected = (float)1;
        delegate* managed<ushort, float> managed = &ManagedFunctionUInt16ToSingle;
        delegate* unmanaged<ushort, float> unmanaged = &UnmanagedFunctionUInt16ToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, float> restoredManaged = (delegate* managed<ushort, float>)(void*)managedAddress;
        delegate* unmanaged<ushort, float> restoredUnmanaged = (delegate* unmanaged<ushort, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to float return");
    }

    private static double ManagedFunctionUInt16ToDouble(ushort value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionUInt16ToDouble(ushort value) => (double)value;

    private static unsafe void VerifyFunctionPointerUInt16ToDouble()
    {
        ushort input = (ushort)1;
        double expected = (double)1;
        delegate* managed<ushort, double> managed = &ManagedFunctionUInt16ToDouble;
        delegate* unmanaged<ushort, double> unmanaged = &UnmanagedFunctionUInt16ToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ushort, double> restoredManaged = (delegate* managed<ushort, double>)(void*)managedAddress;
        delegate* unmanaged<ushort, double> restoredUnmanaged = (delegate* unmanaged<ushort, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ushort parameter to double return");
    }

    private static sbyte ManagedFunctionCharToSByte(char value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerCharToSByte()
    {
        char input = (char)1;
        sbyte expected = (sbyte)1;
        delegate* managed<char, sbyte> managed = &ManagedFunctionCharToSByte;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, sbyte> restoredManaged = (delegate* managed<char, sbyte>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to sbyte return");
    }

    private static byte ManagedFunctionCharToByte(char value) => (byte)value;

    private static unsafe void VerifyFunctionPointerCharToByte()
    {
        char input = (char)1;
        byte expected = (byte)1;
        delegate* managed<char, byte> managed = &ManagedFunctionCharToByte;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, byte> restoredManaged = (delegate* managed<char, byte>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to byte return");
    }

    private static short ManagedFunctionCharToInt16(char value) => (short)value;

    private static unsafe void VerifyFunctionPointerCharToInt16()
    {
        char input = (char)1;
        short expected = (short)1;
        delegate* managed<char, short> managed = &ManagedFunctionCharToInt16;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, short> restoredManaged = (delegate* managed<char, short>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to short return");
    }

    private static ushort ManagedFunctionCharToUInt16(char value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerCharToUInt16()
    {
        char input = (char)1;
        ushort expected = (ushort)1;
        delegate* managed<char, ushort> managed = &ManagedFunctionCharToUInt16;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, ushort> restoredManaged = (delegate* managed<char, ushort>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to ushort return");
    }

    private static char ManagedFunctionCharToChar(char value) => (char)value;

    private static unsafe void VerifyFunctionPointerCharToChar()
    {
        char input = (char)1;
        char expected = (char)1;
        delegate* managed<char, char> managed = &ManagedFunctionCharToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, char> restoredManaged = (delegate* managed<char, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to char return");
    }

    private static int ManagedFunctionCharToInt32(char value) => (int)value;

    private static unsafe void VerifyFunctionPointerCharToInt32()
    {
        char input = (char)1;
        int expected = (int)1;
        delegate* managed<char, int> managed = &ManagedFunctionCharToInt32;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, int> restoredManaged = (delegate* managed<char, int>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to int return");
    }

    private static uint ManagedFunctionCharToUInt32(char value) => (uint)value;

    private static unsafe void VerifyFunctionPointerCharToUInt32()
    {
        char input = (char)1;
        uint expected = (uint)1;
        delegate* managed<char, uint> managed = &ManagedFunctionCharToUInt32;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, uint> restoredManaged = (delegate* managed<char, uint>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to uint return");
    }

    private static long ManagedFunctionCharToInt64(char value) => (long)value;

    private static unsafe void VerifyFunctionPointerCharToInt64()
    {
        char input = (char)1;
        long expected = (long)1;
        delegate* managed<char, long> managed = &ManagedFunctionCharToInt64;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, long> restoredManaged = (delegate* managed<char, long>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to long return");
    }

    private static ulong ManagedFunctionCharToUInt64(char value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerCharToUInt64()
    {
        char input = (char)1;
        ulong expected = (ulong)1;
        delegate* managed<char, ulong> managed = &ManagedFunctionCharToUInt64;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, ulong> restoredManaged = (delegate* managed<char, ulong>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to ulong return");
    }

    private static nint ManagedFunctionCharToNativeInt(char value) => (nint)value;

    private static unsafe void VerifyFunctionPointerCharToNativeInt()
    {
        char input = (char)1;
        nint expected = (nint)1;
        delegate* managed<char, nint> managed = &ManagedFunctionCharToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, nint> restoredManaged = (delegate* managed<char, nint>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to nint return");
    }

    private static nuint ManagedFunctionCharToNativeUInt(char value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerCharToNativeUInt()
    {
        char input = (char)1;
        nuint expected = (nuint)1;
        delegate* managed<char, nuint> managed = &ManagedFunctionCharToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, nuint> restoredManaged = (delegate* managed<char, nuint>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to nuint return");
    }

    private static float ManagedFunctionCharToSingle(char value) => (float)value;

    private static unsafe void VerifyFunctionPointerCharToSingle()
    {
        char input = (char)1;
        float expected = (float)1;
        delegate* managed<char, float> managed = &ManagedFunctionCharToSingle;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, float> restoredManaged = (delegate* managed<char, float>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to float return");
    }

    private static double ManagedFunctionCharToDouble(char value) => (double)value;

    private static unsafe void VerifyFunctionPointerCharToDouble()
    {
        char input = (char)1;
        double expected = (double)1;
        delegate* managed<char, double> managed = &ManagedFunctionCharToDouble;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<char, double> restoredManaged = (delegate* managed<char, double>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer char parameter to double return");
    }

    private static sbyte ManagedFunctionInt32ToSByte(int value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionInt32ToSByte(int value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerInt32ToSByte()
    {
        int input = (int)1;
        sbyte expected = (sbyte)1;
        delegate* managed<int, sbyte> managed = &ManagedFunctionInt32ToSByte;
        delegate* unmanaged<int, sbyte> unmanaged = &UnmanagedFunctionInt32ToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, sbyte> restoredManaged = (delegate* managed<int, sbyte>)(void*)managedAddress;
        delegate* unmanaged<int, sbyte> restoredUnmanaged = (delegate* unmanaged<int, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to sbyte return");
    }

    private static byte ManagedFunctionInt32ToByte(int value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionInt32ToByte(int value) => (byte)value;

    private static unsafe void VerifyFunctionPointerInt32ToByte()
    {
        int input = (int)1;
        byte expected = (byte)1;
        delegate* managed<int, byte> managed = &ManagedFunctionInt32ToByte;
        delegate* unmanaged<int, byte> unmanaged = &UnmanagedFunctionInt32ToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, byte> restoredManaged = (delegate* managed<int, byte>)(void*)managedAddress;
        delegate* unmanaged<int, byte> restoredUnmanaged = (delegate* unmanaged<int, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to byte return");
    }

    private static short ManagedFunctionInt32ToInt16(int value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionInt32ToInt16(int value) => (short)value;

    private static unsafe void VerifyFunctionPointerInt32ToInt16()
    {
        int input = (int)1;
        short expected = (short)1;
        delegate* managed<int, short> managed = &ManagedFunctionInt32ToInt16;
        delegate* unmanaged<int, short> unmanaged = &UnmanagedFunctionInt32ToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, short> restoredManaged = (delegate* managed<int, short>)(void*)managedAddress;
        delegate* unmanaged<int, short> restoredUnmanaged = (delegate* unmanaged<int, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to short return");
    }

    private static ushort ManagedFunctionInt32ToUInt16(int value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionInt32ToUInt16(int value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerInt32ToUInt16()
    {
        int input = (int)1;
        ushort expected = (ushort)1;
        delegate* managed<int, ushort> managed = &ManagedFunctionInt32ToUInt16;
        delegate* unmanaged<int, ushort> unmanaged = &UnmanagedFunctionInt32ToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, ushort> restoredManaged = (delegate* managed<int, ushort>)(void*)managedAddress;
        delegate* unmanaged<int, ushort> restoredUnmanaged = (delegate* unmanaged<int, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to ushort return");
    }

    private static char ManagedFunctionInt32ToChar(int value) => (char)value;

    private static unsafe void VerifyFunctionPointerInt32ToChar()
    {
        int input = (int)1;
        char expected = (char)1;
        delegate* managed<int, char> managed = &ManagedFunctionInt32ToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<int, char> restoredManaged = (delegate* managed<int, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer int parameter to char return");
    }

    private static int ManagedFunctionInt32ToInt32(int value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionInt32ToInt32(int value) => (int)value;

    private static unsafe void VerifyFunctionPointerInt32ToInt32()
    {
        int input = (int)1;
        int expected = (int)1;
        delegate* managed<int, int> managed = &ManagedFunctionInt32ToInt32;
        delegate* unmanaged<int, int> unmanaged = &UnmanagedFunctionInt32ToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, int> restoredManaged = (delegate* managed<int, int>)(void*)managedAddress;
        delegate* unmanaged<int, int> restoredUnmanaged = (delegate* unmanaged<int, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to int return");
    }

    private static uint ManagedFunctionInt32ToUInt32(int value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionInt32ToUInt32(int value) => (uint)value;

    private static unsafe void VerifyFunctionPointerInt32ToUInt32()
    {
        int input = (int)1;
        uint expected = (uint)1;
        delegate* managed<int, uint> managed = &ManagedFunctionInt32ToUInt32;
        delegate* unmanaged<int, uint> unmanaged = &UnmanagedFunctionInt32ToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, uint> restoredManaged = (delegate* managed<int, uint>)(void*)managedAddress;
        delegate* unmanaged<int, uint> restoredUnmanaged = (delegate* unmanaged<int, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to uint return");
    }

    private static long ManagedFunctionInt32ToInt64(int value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionInt32ToInt64(int value) => (long)value;

    private static unsafe void VerifyFunctionPointerInt32ToInt64()
    {
        int input = (int)1;
        long expected = (long)1;
        delegate* managed<int, long> managed = &ManagedFunctionInt32ToInt64;
        delegate* unmanaged<int, long> unmanaged = &UnmanagedFunctionInt32ToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, long> restoredManaged = (delegate* managed<int, long>)(void*)managedAddress;
        delegate* unmanaged<int, long> restoredUnmanaged = (delegate* unmanaged<int, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to long return");
    }

    private static ulong ManagedFunctionInt32ToUInt64(int value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionInt32ToUInt64(int value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerInt32ToUInt64()
    {
        int input = (int)1;
        ulong expected = (ulong)1;
        delegate* managed<int, ulong> managed = &ManagedFunctionInt32ToUInt64;
        delegate* unmanaged<int, ulong> unmanaged = &UnmanagedFunctionInt32ToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, ulong> restoredManaged = (delegate* managed<int, ulong>)(void*)managedAddress;
        delegate* unmanaged<int, ulong> restoredUnmanaged = (delegate* unmanaged<int, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to ulong return");
    }

    private static nint ManagedFunctionInt32ToNativeInt(int value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionInt32ToNativeInt(int value) => (nint)value;

    private static unsafe void VerifyFunctionPointerInt32ToNativeInt()
    {
        int input = (int)1;
        nint expected = (nint)1;
        delegate* managed<int, nint> managed = &ManagedFunctionInt32ToNativeInt;
        delegate* unmanaged<int, nint> unmanaged = &UnmanagedFunctionInt32ToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, nint> restoredManaged = (delegate* managed<int, nint>)(void*)managedAddress;
        delegate* unmanaged<int, nint> restoredUnmanaged = (delegate* unmanaged<int, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to nint return");
    }

    private static nuint ManagedFunctionInt32ToNativeUInt(int value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionInt32ToNativeUInt(int value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerInt32ToNativeUInt()
    {
        int input = (int)1;
        nuint expected = (nuint)1;
        delegate* managed<int, nuint> managed = &ManagedFunctionInt32ToNativeUInt;
        delegate* unmanaged<int, nuint> unmanaged = &UnmanagedFunctionInt32ToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, nuint> restoredManaged = (delegate* managed<int, nuint>)(void*)managedAddress;
        delegate* unmanaged<int, nuint> restoredUnmanaged = (delegate* unmanaged<int, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to nuint return");
    }

    private static float ManagedFunctionInt32ToSingle(int value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionInt32ToSingle(int value) => (float)value;

    private static unsafe void VerifyFunctionPointerInt32ToSingle()
    {
        int input = (int)1;
        float expected = (float)1;
        delegate* managed<int, float> managed = &ManagedFunctionInt32ToSingle;
        delegate* unmanaged<int, float> unmanaged = &UnmanagedFunctionInt32ToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, float> restoredManaged = (delegate* managed<int, float>)(void*)managedAddress;
        delegate* unmanaged<int, float> restoredUnmanaged = (delegate* unmanaged<int, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to float return");
    }

    private static double ManagedFunctionInt32ToDouble(int value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionInt32ToDouble(int value) => (double)value;

    private static unsafe void VerifyFunctionPointerInt32ToDouble()
    {
        int input = (int)1;
        double expected = (double)1;
        delegate* managed<int, double> managed = &ManagedFunctionInt32ToDouble;
        delegate* unmanaged<int, double> unmanaged = &UnmanagedFunctionInt32ToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<int, double> restoredManaged = (delegate* managed<int, double>)(void*)managedAddress;
        delegate* unmanaged<int, double> restoredUnmanaged = (delegate* unmanaged<int, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer int parameter to double return");
    }

    private static sbyte ManagedFunctionUInt32ToSByte(uint value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionUInt32ToSByte(uint value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerUInt32ToSByte()
    {
        uint input = (uint)1;
        sbyte expected = (sbyte)1;
        delegate* managed<uint, sbyte> managed = &ManagedFunctionUInt32ToSByte;
        delegate* unmanaged<uint, sbyte> unmanaged = &UnmanagedFunctionUInt32ToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, sbyte> restoredManaged = (delegate* managed<uint, sbyte>)(void*)managedAddress;
        delegate* unmanaged<uint, sbyte> restoredUnmanaged = (delegate* unmanaged<uint, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to sbyte return");
    }

    private static byte ManagedFunctionUInt32ToByte(uint value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionUInt32ToByte(uint value) => (byte)value;

    private static unsafe void VerifyFunctionPointerUInt32ToByte()
    {
        uint input = (uint)1;
        byte expected = (byte)1;
        delegate* managed<uint, byte> managed = &ManagedFunctionUInt32ToByte;
        delegate* unmanaged<uint, byte> unmanaged = &UnmanagedFunctionUInt32ToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, byte> restoredManaged = (delegate* managed<uint, byte>)(void*)managedAddress;
        delegate* unmanaged<uint, byte> restoredUnmanaged = (delegate* unmanaged<uint, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to byte return");
    }

    private static short ManagedFunctionUInt32ToInt16(uint value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionUInt32ToInt16(uint value) => (short)value;

    private static unsafe void VerifyFunctionPointerUInt32ToInt16()
    {
        uint input = (uint)1;
        short expected = (short)1;
        delegate* managed<uint, short> managed = &ManagedFunctionUInt32ToInt16;
        delegate* unmanaged<uint, short> unmanaged = &UnmanagedFunctionUInt32ToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, short> restoredManaged = (delegate* managed<uint, short>)(void*)managedAddress;
        delegate* unmanaged<uint, short> restoredUnmanaged = (delegate* unmanaged<uint, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to short return");
    }

    private static ushort ManagedFunctionUInt32ToUInt16(uint value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionUInt32ToUInt16(uint value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerUInt32ToUInt16()
    {
        uint input = (uint)1;
        ushort expected = (ushort)1;
        delegate* managed<uint, ushort> managed = &ManagedFunctionUInt32ToUInt16;
        delegate* unmanaged<uint, ushort> unmanaged = &UnmanagedFunctionUInt32ToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, ushort> restoredManaged = (delegate* managed<uint, ushort>)(void*)managedAddress;
        delegate* unmanaged<uint, ushort> restoredUnmanaged = (delegate* unmanaged<uint, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to ushort return");
    }

    private static char ManagedFunctionUInt32ToChar(uint value) => (char)value;

    private static unsafe void VerifyFunctionPointerUInt32ToChar()
    {
        uint input = (uint)1;
        char expected = (char)1;
        delegate* managed<uint, char> managed = &ManagedFunctionUInt32ToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<uint, char> restoredManaged = (delegate* managed<uint, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer uint parameter to char return");
    }

    private static int ManagedFunctionUInt32ToInt32(uint value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionUInt32ToInt32(uint value) => (int)value;

    private static unsafe void VerifyFunctionPointerUInt32ToInt32()
    {
        uint input = (uint)1;
        int expected = (int)1;
        delegate* managed<uint, int> managed = &ManagedFunctionUInt32ToInt32;
        delegate* unmanaged<uint, int> unmanaged = &UnmanagedFunctionUInt32ToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, int> restoredManaged = (delegate* managed<uint, int>)(void*)managedAddress;
        delegate* unmanaged<uint, int> restoredUnmanaged = (delegate* unmanaged<uint, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to int return");
    }

    private static uint ManagedFunctionUInt32ToUInt32(uint value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionUInt32ToUInt32(uint value) => (uint)value;

    private static unsafe void VerifyFunctionPointerUInt32ToUInt32()
    {
        uint input = (uint)1;
        uint expected = (uint)1;
        delegate* managed<uint, uint> managed = &ManagedFunctionUInt32ToUInt32;
        delegate* unmanaged<uint, uint> unmanaged = &UnmanagedFunctionUInt32ToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, uint> restoredManaged = (delegate* managed<uint, uint>)(void*)managedAddress;
        delegate* unmanaged<uint, uint> restoredUnmanaged = (delegate* unmanaged<uint, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to uint return");
    }

    private static long ManagedFunctionUInt32ToInt64(uint value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionUInt32ToInt64(uint value) => (long)value;

    private static unsafe void VerifyFunctionPointerUInt32ToInt64()
    {
        uint input = (uint)1;
        long expected = (long)1;
        delegate* managed<uint, long> managed = &ManagedFunctionUInt32ToInt64;
        delegate* unmanaged<uint, long> unmanaged = &UnmanagedFunctionUInt32ToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, long> restoredManaged = (delegate* managed<uint, long>)(void*)managedAddress;
        delegate* unmanaged<uint, long> restoredUnmanaged = (delegate* unmanaged<uint, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to long return");
    }

    private static ulong ManagedFunctionUInt32ToUInt64(uint value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionUInt32ToUInt64(uint value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerUInt32ToUInt64()
    {
        uint input = (uint)1;
        ulong expected = (ulong)1;
        delegate* managed<uint, ulong> managed = &ManagedFunctionUInt32ToUInt64;
        delegate* unmanaged<uint, ulong> unmanaged = &UnmanagedFunctionUInt32ToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, ulong> restoredManaged = (delegate* managed<uint, ulong>)(void*)managedAddress;
        delegate* unmanaged<uint, ulong> restoredUnmanaged = (delegate* unmanaged<uint, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to ulong return");
    }

    private static nint ManagedFunctionUInt32ToNativeInt(uint value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionUInt32ToNativeInt(uint value) => (nint)value;

    private static unsafe void VerifyFunctionPointerUInt32ToNativeInt()
    {
        uint input = (uint)1;
        nint expected = (nint)1;
        delegate* managed<uint, nint> managed = &ManagedFunctionUInt32ToNativeInt;
        delegate* unmanaged<uint, nint> unmanaged = &UnmanagedFunctionUInt32ToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, nint> restoredManaged = (delegate* managed<uint, nint>)(void*)managedAddress;
        delegate* unmanaged<uint, nint> restoredUnmanaged = (delegate* unmanaged<uint, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to nint return");
    }

    private static nuint ManagedFunctionUInt32ToNativeUInt(uint value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionUInt32ToNativeUInt(uint value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerUInt32ToNativeUInt()
    {
        uint input = (uint)1;
        nuint expected = (nuint)1;
        delegate* managed<uint, nuint> managed = &ManagedFunctionUInt32ToNativeUInt;
        delegate* unmanaged<uint, nuint> unmanaged = &UnmanagedFunctionUInt32ToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, nuint> restoredManaged = (delegate* managed<uint, nuint>)(void*)managedAddress;
        delegate* unmanaged<uint, nuint> restoredUnmanaged = (delegate* unmanaged<uint, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to nuint return");
    }

    private static float ManagedFunctionUInt32ToSingle(uint value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionUInt32ToSingle(uint value) => (float)value;

    private static unsafe void VerifyFunctionPointerUInt32ToSingle()
    {
        uint input = (uint)1;
        float expected = (float)1;
        delegate* managed<uint, float> managed = &ManagedFunctionUInt32ToSingle;
        delegate* unmanaged<uint, float> unmanaged = &UnmanagedFunctionUInt32ToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, float> restoredManaged = (delegate* managed<uint, float>)(void*)managedAddress;
        delegate* unmanaged<uint, float> restoredUnmanaged = (delegate* unmanaged<uint, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to float return");
    }

    private static double ManagedFunctionUInt32ToDouble(uint value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionUInt32ToDouble(uint value) => (double)value;

    private static unsafe void VerifyFunctionPointerUInt32ToDouble()
    {
        uint input = (uint)1;
        double expected = (double)1;
        delegate* managed<uint, double> managed = &ManagedFunctionUInt32ToDouble;
        delegate* unmanaged<uint, double> unmanaged = &UnmanagedFunctionUInt32ToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<uint, double> restoredManaged = (delegate* managed<uint, double>)(void*)managedAddress;
        delegate* unmanaged<uint, double> restoredUnmanaged = (delegate* unmanaged<uint, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer uint parameter to double return");
    }

    private static sbyte ManagedFunctionInt64ToSByte(long value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionInt64ToSByte(long value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerInt64ToSByte()
    {
        long input = (long)1;
        sbyte expected = (sbyte)1;
        delegate* managed<long, sbyte> managed = &ManagedFunctionInt64ToSByte;
        delegate* unmanaged<long, sbyte> unmanaged = &UnmanagedFunctionInt64ToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, sbyte> restoredManaged = (delegate* managed<long, sbyte>)(void*)managedAddress;
        delegate* unmanaged<long, sbyte> restoredUnmanaged = (delegate* unmanaged<long, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to sbyte return");
    }

    private static byte ManagedFunctionInt64ToByte(long value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionInt64ToByte(long value) => (byte)value;

    private static unsafe void VerifyFunctionPointerInt64ToByte()
    {
        long input = (long)1;
        byte expected = (byte)1;
        delegate* managed<long, byte> managed = &ManagedFunctionInt64ToByte;
        delegate* unmanaged<long, byte> unmanaged = &UnmanagedFunctionInt64ToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, byte> restoredManaged = (delegate* managed<long, byte>)(void*)managedAddress;
        delegate* unmanaged<long, byte> restoredUnmanaged = (delegate* unmanaged<long, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to byte return");
    }

    private static short ManagedFunctionInt64ToInt16(long value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionInt64ToInt16(long value) => (short)value;

    private static unsafe void VerifyFunctionPointerInt64ToInt16()
    {
        long input = (long)1;
        short expected = (short)1;
        delegate* managed<long, short> managed = &ManagedFunctionInt64ToInt16;
        delegate* unmanaged<long, short> unmanaged = &UnmanagedFunctionInt64ToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, short> restoredManaged = (delegate* managed<long, short>)(void*)managedAddress;
        delegate* unmanaged<long, short> restoredUnmanaged = (delegate* unmanaged<long, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to short return");
    }

    private static ushort ManagedFunctionInt64ToUInt16(long value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionInt64ToUInt16(long value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerInt64ToUInt16()
    {
        long input = (long)1;
        ushort expected = (ushort)1;
        delegate* managed<long, ushort> managed = &ManagedFunctionInt64ToUInt16;
        delegate* unmanaged<long, ushort> unmanaged = &UnmanagedFunctionInt64ToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, ushort> restoredManaged = (delegate* managed<long, ushort>)(void*)managedAddress;
        delegate* unmanaged<long, ushort> restoredUnmanaged = (delegate* unmanaged<long, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to ushort return");
    }

    private static char ManagedFunctionInt64ToChar(long value) => (char)value;

    private static unsafe void VerifyFunctionPointerInt64ToChar()
    {
        long input = (long)1;
        char expected = (char)1;
        delegate* managed<long, char> managed = &ManagedFunctionInt64ToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<long, char> restoredManaged = (delegate* managed<long, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer long parameter to char return");
    }

    private static int ManagedFunctionInt64ToInt32(long value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionInt64ToInt32(long value) => (int)value;

    private static unsafe void VerifyFunctionPointerInt64ToInt32()
    {
        long input = (long)1;
        int expected = (int)1;
        delegate* managed<long, int> managed = &ManagedFunctionInt64ToInt32;
        delegate* unmanaged<long, int> unmanaged = &UnmanagedFunctionInt64ToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, int> restoredManaged = (delegate* managed<long, int>)(void*)managedAddress;
        delegate* unmanaged<long, int> restoredUnmanaged = (delegate* unmanaged<long, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to int return");
    }

    private static uint ManagedFunctionInt64ToUInt32(long value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionInt64ToUInt32(long value) => (uint)value;

    private static unsafe void VerifyFunctionPointerInt64ToUInt32()
    {
        long input = (long)1;
        uint expected = (uint)1;
        delegate* managed<long, uint> managed = &ManagedFunctionInt64ToUInt32;
        delegate* unmanaged<long, uint> unmanaged = &UnmanagedFunctionInt64ToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, uint> restoredManaged = (delegate* managed<long, uint>)(void*)managedAddress;
        delegate* unmanaged<long, uint> restoredUnmanaged = (delegate* unmanaged<long, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to uint return");
    }

    private static long ManagedFunctionInt64ToInt64(long value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionInt64ToInt64(long value) => (long)value;

    private static unsafe void VerifyFunctionPointerInt64ToInt64()
    {
        long input = (long)1;
        long expected = (long)1;
        delegate* managed<long, long> managed = &ManagedFunctionInt64ToInt64;
        delegate* unmanaged<long, long> unmanaged = &UnmanagedFunctionInt64ToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, long> restoredManaged = (delegate* managed<long, long>)(void*)managedAddress;
        delegate* unmanaged<long, long> restoredUnmanaged = (delegate* unmanaged<long, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to long return");
    }

    private static ulong ManagedFunctionInt64ToUInt64(long value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionInt64ToUInt64(long value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerInt64ToUInt64()
    {
        long input = (long)1;
        ulong expected = (ulong)1;
        delegate* managed<long, ulong> managed = &ManagedFunctionInt64ToUInt64;
        delegate* unmanaged<long, ulong> unmanaged = &UnmanagedFunctionInt64ToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, ulong> restoredManaged = (delegate* managed<long, ulong>)(void*)managedAddress;
        delegate* unmanaged<long, ulong> restoredUnmanaged = (delegate* unmanaged<long, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to ulong return");
    }

    private static nint ManagedFunctionInt64ToNativeInt(long value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionInt64ToNativeInt(long value) => (nint)value;

    private static unsafe void VerifyFunctionPointerInt64ToNativeInt()
    {
        long input = (long)1;
        nint expected = (nint)1;
        delegate* managed<long, nint> managed = &ManagedFunctionInt64ToNativeInt;
        delegate* unmanaged<long, nint> unmanaged = &UnmanagedFunctionInt64ToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, nint> restoredManaged = (delegate* managed<long, nint>)(void*)managedAddress;
        delegate* unmanaged<long, nint> restoredUnmanaged = (delegate* unmanaged<long, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to nint return");
    }

    private static nuint ManagedFunctionInt64ToNativeUInt(long value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionInt64ToNativeUInt(long value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerInt64ToNativeUInt()
    {
        long input = (long)1;
        nuint expected = (nuint)1;
        delegate* managed<long, nuint> managed = &ManagedFunctionInt64ToNativeUInt;
        delegate* unmanaged<long, nuint> unmanaged = &UnmanagedFunctionInt64ToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, nuint> restoredManaged = (delegate* managed<long, nuint>)(void*)managedAddress;
        delegate* unmanaged<long, nuint> restoredUnmanaged = (delegate* unmanaged<long, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to nuint return");
    }

    private static float ManagedFunctionInt64ToSingle(long value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionInt64ToSingle(long value) => (float)value;

    private static unsafe void VerifyFunctionPointerInt64ToSingle()
    {
        long input = (long)1;
        float expected = (float)1;
        delegate* managed<long, float> managed = &ManagedFunctionInt64ToSingle;
        delegate* unmanaged<long, float> unmanaged = &UnmanagedFunctionInt64ToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, float> restoredManaged = (delegate* managed<long, float>)(void*)managedAddress;
        delegate* unmanaged<long, float> restoredUnmanaged = (delegate* unmanaged<long, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to float return");
    }

    private static double ManagedFunctionInt64ToDouble(long value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionInt64ToDouble(long value) => (double)value;

    private static unsafe void VerifyFunctionPointerInt64ToDouble()
    {
        long input = (long)1;
        double expected = (double)1;
        delegate* managed<long, double> managed = &ManagedFunctionInt64ToDouble;
        delegate* unmanaged<long, double> unmanaged = &UnmanagedFunctionInt64ToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<long, double> restoredManaged = (delegate* managed<long, double>)(void*)managedAddress;
        delegate* unmanaged<long, double> restoredUnmanaged = (delegate* unmanaged<long, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer long parameter to double return");
    }

    private static sbyte ManagedFunctionUInt64ToSByte(ulong value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionUInt64ToSByte(ulong value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerUInt64ToSByte()
    {
        ulong input = (ulong)1;
        sbyte expected = (sbyte)1;
        delegate* managed<ulong, sbyte> managed = &ManagedFunctionUInt64ToSByte;
        delegate* unmanaged<ulong, sbyte> unmanaged = &UnmanagedFunctionUInt64ToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, sbyte> restoredManaged = (delegate* managed<ulong, sbyte>)(void*)managedAddress;
        delegate* unmanaged<ulong, sbyte> restoredUnmanaged = (delegate* unmanaged<ulong, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to sbyte return");
    }

    private static byte ManagedFunctionUInt64ToByte(ulong value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionUInt64ToByte(ulong value) => (byte)value;

    private static unsafe void VerifyFunctionPointerUInt64ToByte()
    {
        ulong input = (ulong)1;
        byte expected = (byte)1;
        delegate* managed<ulong, byte> managed = &ManagedFunctionUInt64ToByte;
        delegate* unmanaged<ulong, byte> unmanaged = &UnmanagedFunctionUInt64ToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, byte> restoredManaged = (delegate* managed<ulong, byte>)(void*)managedAddress;
        delegate* unmanaged<ulong, byte> restoredUnmanaged = (delegate* unmanaged<ulong, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to byte return");
    }

    private static short ManagedFunctionUInt64ToInt16(ulong value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionUInt64ToInt16(ulong value) => (short)value;

    private static unsafe void VerifyFunctionPointerUInt64ToInt16()
    {
        ulong input = (ulong)1;
        short expected = (short)1;
        delegate* managed<ulong, short> managed = &ManagedFunctionUInt64ToInt16;
        delegate* unmanaged<ulong, short> unmanaged = &UnmanagedFunctionUInt64ToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, short> restoredManaged = (delegate* managed<ulong, short>)(void*)managedAddress;
        delegate* unmanaged<ulong, short> restoredUnmanaged = (delegate* unmanaged<ulong, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to short return");
    }

    private static ushort ManagedFunctionUInt64ToUInt16(ulong value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionUInt64ToUInt16(ulong value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerUInt64ToUInt16()
    {
        ulong input = (ulong)1;
        ushort expected = (ushort)1;
        delegate* managed<ulong, ushort> managed = &ManagedFunctionUInt64ToUInt16;
        delegate* unmanaged<ulong, ushort> unmanaged = &UnmanagedFunctionUInt64ToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, ushort> restoredManaged = (delegate* managed<ulong, ushort>)(void*)managedAddress;
        delegate* unmanaged<ulong, ushort> restoredUnmanaged = (delegate* unmanaged<ulong, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to ushort return");
    }

    private static char ManagedFunctionUInt64ToChar(ulong value) => (char)value;

    private static unsafe void VerifyFunctionPointerUInt64ToChar()
    {
        ulong input = (ulong)1;
        char expected = (char)1;
        delegate* managed<ulong, char> managed = &ManagedFunctionUInt64ToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<ulong, char> restoredManaged = (delegate* managed<ulong, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer ulong parameter to char return");
    }

    private static int ManagedFunctionUInt64ToInt32(ulong value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionUInt64ToInt32(ulong value) => (int)value;

    private static unsafe void VerifyFunctionPointerUInt64ToInt32()
    {
        ulong input = (ulong)1;
        int expected = (int)1;
        delegate* managed<ulong, int> managed = &ManagedFunctionUInt64ToInt32;
        delegate* unmanaged<ulong, int> unmanaged = &UnmanagedFunctionUInt64ToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, int> restoredManaged = (delegate* managed<ulong, int>)(void*)managedAddress;
        delegate* unmanaged<ulong, int> restoredUnmanaged = (delegate* unmanaged<ulong, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to int return");
    }

    private static uint ManagedFunctionUInt64ToUInt32(ulong value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionUInt64ToUInt32(ulong value) => (uint)value;

    private static unsafe void VerifyFunctionPointerUInt64ToUInt32()
    {
        ulong input = (ulong)1;
        uint expected = (uint)1;
        delegate* managed<ulong, uint> managed = &ManagedFunctionUInt64ToUInt32;
        delegate* unmanaged<ulong, uint> unmanaged = &UnmanagedFunctionUInt64ToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, uint> restoredManaged = (delegate* managed<ulong, uint>)(void*)managedAddress;
        delegate* unmanaged<ulong, uint> restoredUnmanaged = (delegate* unmanaged<ulong, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to uint return");
    }

    private static long ManagedFunctionUInt64ToInt64(ulong value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionUInt64ToInt64(ulong value) => (long)value;

    private static unsafe void VerifyFunctionPointerUInt64ToInt64()
    {
        ulong input = (ulong)1;
        long expected = (long)1;
        delegate* managed<ulong, long> managed = &ManagedFunctionUInt64ToInt64;
        delegate* unmanaged<ulong, long> unmanaged = &UnmanagedFunctionUInt64ToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, long> restoredManaged = (delegate* managed<ulong, long>)(void*)managedAddress;
        delegate* unmanaged<ulong, long> restoredUnmanaged = (delegate* unmanaged<ulong, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to long return");
    }

    private static ulong ManagedFunctionUInt64ToUInt64(ulong value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionUInt64ToUInt64(ulong value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerUInt64ToUInt64()
    {
        ulong input = (ulong)1;
        ulong expected = (ulong)1;
        delegate* managed<ulong, ulong> managed = &ManagedFunctionUInt64ToUInt64;
        delegate* unmanaged<ulong, ulong> unmanaged = &UnmanagedFunctionUInt64ToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, ulong> restoredManaged = (delegate* managed<ulong, ulong>)(void*)managedAddress;
        delegate* unmanaged<ulong, ulong> restoredUnmanaged = (delegate* unmanaged<ulong, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to ulong return");
    }

    private static nint ManagedFunctionUInt64ToNativeInt(ulong value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionUInt64ToNativeInt(ulong value) => (nint)value;

    private static unsafe void VerifyFunctionPointerUInt64ToNativeInt()
    {
        ulong input = (ulong)1;
        nint expected = (nint)1;
        delegate* managed<ulong, nint> managed = &ManagedFunctionUInt64ToNativeInt;
        delegate* unmanaged<ulong, nint> unmanaged = &UnmanagedFunctionUInt64ToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, nint> restoredManaged = (delegate* managed<ulong, nint>)(void*)managedAddress;
        delegate* unmanaged<ulong, nint> restoredUnmanaged = (delegate* unmanaged<ulong, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to nint return");
    }

    private static nuint ManagedFunctionUInt64ToNativeUInt(ulong value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionUInt64ToNativeUInt(ulong value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerUInt64ToNativeUInt()
    {
        ulong input = (ulong)1;
        nuint expected = (nuint)1;
        delegate* managed<ulong, nuint> managed = &ManagedFunctionUInt64ToNativeUInt;
        delegate* unmanaged<ulong, nuint> unmanaged = &UnmanagedFunctionUInt64ToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, nuint> restoredManaged = (delegate* managed<ulong, nuint>)(void*)managedAddress;
        delegate* unmanaged<ulong, nuint> restoredUnmanaged = (delegate* unmanaged<ulong, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to nuint return");
    }

    private static float ManagedFunctionUInt64ToSingle(ulong value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionUInt64ToSingle(ulong value) => (float)value;

    private static unsafe void VerifyFunctionPointerUInt64ToSingle()
    {
        ulong input = (ulong)1;
        float expected = (float)1;
        delegate* managed<ulong, float> managed = &ManagedFunctionUInt64ToSingle;
        delegate* unmanaged<ulong, float> unmanaged = &UnmanagedFunctionUInt64ToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, float> restoredManaged = (delegate* managed<ulong, float>)(void*)managedAddress;
        delegate* unmanaged<ulong, float> restoredUnmanaged = (delegate* unmanaged<ulong, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to float return");
    }

    private static double ManagedFunctionUInt64ToDouble(ulong value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionUInt64ToDouble(ulong value) => (double)value;

    private static unsafe void VerifyFunctionPointerUInt64ToDouble()
    {
        ulong input = (ulong)1;
        double expected = (double)1;
        delegate* managed<ulong, double> managed = &ManagedFunctionUInt64ToDouble;
        delegate* unmanaged<ulong, double> unmanaged = &UnmanagedFunctionUInt64ToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<ulong, double> restoredManaged = (delegate* managed<ulong, double>)(void*)managedAddress;
        delegate* unmanaged<ulong, double> restoredUnmanaged = (delegate* unmanaged<ulong, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer ulong parameter to double return");
    }

    private static sbyte ManagedFunctionNativeIntToSByte(nint value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionNativeIntToSByte(nint value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerNativeIntToSByte()
    {
        nint input = (nint)1;
        sbyte expected = (sbyte)1;
        delegate* managed<nint, sbyte> managed = &ManagedFunctionNativeIntToSByte;
        delegate* unmanaged<nint, sbyte> unmanaged = &UnmanagedFunctionNativeIntToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, sbyte> restoredManaged = (delegate* managed<nint, sbyte>)(void*)managedAddress;
        delegate* unmanaged<nint, sbyte> restoredUnmanaged = (delegate* unmanaged<nint, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to sbyte return");
    }

    private static byte ManagedFunctionNativeIntToByte(nint value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionNativeIntToByte(nint value) => (byte)value;

    private static unsafe void VerifyFunctionPointerNativeIntToByte()
    {
        nint input = (nint)1;
        byte expected = (byte)1;
        delegate* managed<nint, byte> managed = &ManagedFunctionNativeIntToByte;
        delegate* unmanaged<nint, byte> unmanaged = &UnmanagedFunctionNativeIntToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, byte> restoredManaged = (delegate* managed<nint, byte>)(void*)managedAddress;
        delegate* unmanaged<nint, byte> restoredUnmanaged = (delegate* unmanaged<nint, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to byte return");
    }

    private static short ManagedFunctionNativeIntToInt16(nint value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionNativeIntToInt16(nint value) => (short)value;

    private static unsafe void VerifyFunctionPointerNativeIntToInt16()
    {
        nint input = (nint)1;
        short expected = (short)1;
        delegate* managed<nint, short> managed = &ManagedFunctionNativeIntToInt16;
        delegate* unmanaged<nint, short> unmanaged = &UnmanagedFunctionNativeIntToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, short> restoredManaged = (delegate* managed<nint, short>)(void*)managedAddress;
        delegate* unmanaged<nint, short> restoredUnmanaged = (delegate* unmanaged<nint, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to short return");
    }

    private static ushort ManagedFunctionNativeIntToUInt16(nint value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionNativeIntToUInt16(nint value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerNativeIntToUInt16()
    {
        nint input = (nint)1;
        ushort expected = (ushort)1;
        delegate* managed<nint, ushort> managed = &ManagedFunctionNativeIntToUInt16;
        delegate* unmanaged<nint, ushort> unmanaged = &UnmanagedFunctionNativeIntToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, ushort> restoredManaged = (delegate* managed<nint, ushort>)(void*)managedAddress;
        delegate* unmanaged<nint, ushort> restoredUnmanaged = (delegate* unmanaged<nint, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to ushort return");
    }

    private static char ManagedFunctionNativeIntToChar(nint value) => (char)value;

    private static unsafe void VerifyFunctionPointerNativeIntToChar()
    {
        nint input = (nint)1;
        char expected = (char)1;
        delegate* managed<nint, char> managed = &ManagedFunctionNativeIntToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<nint, char> restoredManaged = (delegate* managed<nint, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer nint parameter to char return");
    }

    private static int ManagedFunctionNativeIntToInt32(nint value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionNativeIntToInt32(nint value) => (int)value;

    private static unsafe void VerifyFunctionPointerNativeIntToInt32()
    {
        nint input = (nint)1;
        int expected = (int)1;
        delegate* managed<nint, int> managed = &ManagedFunctionNativeIntToInt32;
        delegate* unmanaged<nint, int> unmanaged = &UnmanagedFunctionNativeIntToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, int> restoredManaged = (delegate* managed<nint, int>)(void*)managedAddress;
        delegate* unmanaged<nint, int> restoredUnmanaged = (delegate* unmanaged<nint, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to int return");
    }

    private static uint ManagedFunctionNativeIntToUInt32(nint value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionNativeIntToUInt32(nint value) => (uint)value;

    private static unsafe void VerifyFunctionPointerNativeIntToUInt32()
    {
        nint input = (nint)1;
        uint expected = (uint)1;
        delegate* managed<nint, uint> managed = &ManagedFunctionNativeIntToUInt32;
        delegate* unmanaged<nint, uint> unmanaged = &UnmanagedFunctionNativeIntToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, uint> restoredManaged = (delegate* managed<nint, uint>)(void*)managedAddress;
        delegate* unmanaged<nint, uint> restoredUnmanaged = (delegate* unmanaged<nint, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to uint return");
    }

    private static long ManagedFunctionNativeIntToInt64(nint value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionNativeIntToInt64(nint value) => (long)value;

    private static unsafe void VerifyFunctionPointerNativeIntToInt64()
    {
        nint input = (nint)1;
        long expected = (long)1;
        delegate* managed<nint, long> managed = &ManagedFunctionNativeIntToInt64;
        delegate* unmanaged<nint, long> unmanaged = &UnmanagedFunctionNativeIntToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, long> restoredManaged = (delegate* managed<nint, long>)(void*)managedAddress;
        delegate* unmanaged<nint, long> restoredUnmanaged = (delegate* unmanaged<nint, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to long return");
    }

    private static ulong ManagedFunctionNativeIntToUInt64(nint value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionNativeIntToUInt64(nint value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerNativeIntToUInt64()
    {
        nint input = (nint)1;
        ulong expected = (ulong)1;
        delegate* managed<nint, ulong> managed = &ManagedFunctionNativeIntToUInt64;
        delegate* unmanaged<nint, ulong> unmanaged = &UnmanagedFunctionNativeIntToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, ulong> restoredManaged = (delegate* managed<nint, ulong>)(void*)managedAddress;
        delegate* unmanaged<nint, ulong> restoredUnmanaged = (delegate* unmanaged<nint, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to ulong return");
    }

    private static nint ManagedFunctionNativeIntToNativeInt(nint value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionNativeIntToNativeInt(nint value) => (nint)value;

    private static unsafe void VerifyFunctionPointerNativeIntToNativeInt()
    {
        nint input = (nint)1;
        nint expected = (nint)1;
        delegate* managed<nint, nint> managed = &ManagedFunctionNativeIntToNativeInt;
        delegate* unmanaged<nint, nint> unmanaged = &UnmanagedFunctionNativeIntToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, nint> restoredManaged = (delegate* managed<nint, nint>)(void*)managedAddress;
        delegate* unmanaged<nint, nint> restoredUnmanaged = (delegate* unmanaged<nint, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to nint return");
    }

    private static nuint ManagedFunctionNativeIntToNativeUInt(nint value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionNativeIntToNativeUInt(nint value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerNativeIntToNativeUInt()
    {
        nint input = (nint)1;
        nuint expected = (nuint)1;
        delegate* managed<nint, nuint> managed = &ManagedFunctionNativeIntToNativeUInt;
        delegate* unmanaged<nint, nuint> unmanaged = &UnmanagedFunctionNativeIntToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, nuint> restoredManaged = (delegate* managed<nint, nuint>)(void*)managedAddress;
        delegate* unmanaged<nint, nuint> restoredUnmanaged = (delegate* unmanaged<nint, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to nuint return");
    }

    private static float ManagedFunctionNativeIntToSingle(nint value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionNativeIntToSingle(nint value) => (float)value;

    private static unsafe void VerifyFunctionPointerNativeIntToSingle()
    {
        nint input = (nint)1;
        float expected = (float)1;
        delegate* managed<nint, float> managed = &ManagedFunctionNativeIntToSingle;
        delegate* unmanaged<nint, float> unmanaged = &UnmanagedFunctionNativeIntToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, float> restoredManaged = (delegate* managed<nint, float>)(void*)managedAddress;
        delegate* unmanaged<nint, float> restoredUnmanaged = (delegate* unmanaged<nint, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to float return");
    }

    private static double ManagedFunctionNativeIntToDouble(nint value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionNativeIntToDouble(nint value) => (double)value;

    private static unsafe void VerifyFunctionPointerNativeIntToDouble()
    {
        nint input = (nint)1;
        double expected = (double)1;
        delegate* managed<nint, double> managed = &ManagedFunctionNativeIntToDouble;
        delegate* unmanaged<nint, double> unmanaged = &UnmanagedFunctionNativeIntToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nint, double> restoredManaged = (delegate* managed<nint, double>)(void*)managedAddress;
        delegate* unmanaged<nint, double> restoredUnmanaged = (delegate* unmanaged<nint, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nint parameter to double return");
    }

    private static sbyte ManagedFunctionNativeUIntToSByte(nuint value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionNativeUIntToSByte(nuint value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToSByte()
    {
        nuint input = (nuint)1;
        sbyte expected = (sbyte)1;
        delegate* managed<nuint, sbyte> managed = &ManagedFunctionNativeUIntToSByte;
        delegate* unmanaged<nuint, sbyte> unmanaged = &UnmanagedFunctionNativeUIntToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, sbyte> restoredManaged = (delegate* managed<nuint, sbyte>)(void*)managedAddress;
        delegate* unmanaged<nuint, sbyte> restoredUnmanaged = (delegate* unmanaged<nuint, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to sbyte return");
    }

    private static byte ManagedFunctionNativeUIntToByte(nuint value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionNativeUIntToByte(nuint value) => (byte)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToByte()
    {
        nuint input = (nuint)1;
        byte expected = (byte)1;
        delegate* managed<nuint, byte> managed = &ManagedFunctionNativeUIntToByte;
        delegate* unmanaged<nuint, byte> unmanaged = &UnmanagedFunctionNativeUIntToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, byte> restoredManaged = (delegate* managed<nuint, byte>)(void*)managedAddress;
        delegate* unmanaged<nuint, byte> restoredUnmanaged = (delegate* unmanaged<nuint, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to byte return");
    }

    private static short ManagedFunctionNativeUIntToInt16(nuint value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionNativeUIntToInt16(nuint value) => (short)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToInt16()
    {
        nuint input = (nuint)1;
        short expected = (short)1;
        delegate* managed<nuint, short> managed = &ManagedFunctionNativeUIntToInt16;
        delegate* unmanaged<nuint, short> unmanaged = &UnmanagedFunctionNativeUIntToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, short> restoredManaged = (delegate* managed<nuint, short>)(void*)managedAddress;
        delegate* unmanaged<nuint, short> restoredUnmanaged = (delegate* unmanaged<nuint, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to short return");
    }

    private static ushort ManagedFunctionNativeUIntToUInt16(nuint value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionNativeUIntToUInt16(nuint value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToUInt16()
    {
        nuint input = (nuint)1;
        ushort expected = (ushort)1;
        delegate* managed<nuint, ushort> managed = &ManagedFunctionNativeUIntToUInt16;
        delegate* unmanaged<nuint, ushort> unmanaged = &UnmanagedFunctionNativeUIntToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, ushort> restoredManaged = (delegate* managed<nuint, ushort>)(void*)managedAddress;
        delegate* unmanaged<nuint, ushort> restoredUnmanaged = (delegate* unmanaged<nuint, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to ushort return");
    }

    private static char ManagedFunctionNativeUIntToChar(nuint value) => (char)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToChar()
    {
        nuint input = (nuint)1;
        char expected = (char)1;
        delegate* managed<nuint, char> managed = &ManagedFunctionNativeUIntToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<nuint, char> restoredManaged = (delegate* managed<nuint, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer nuint parameter to char return");
    }

    private static int ManagedFunctionNativeUIntToInt32(nuint value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionNativeUIntToInt32(nuint value) => (int)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToInt32()
    {
        nuint input = (nuint)1;
        int expected = (int)1;
        delegate* managed<nuint, int> managed = &ManagedFunctionNativeUIntToInt32;
        delegate* unmanaged<nuint, int> unmanaged = &UnmanagedFunctionNativeUIntToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, int> restoredManaged = (delegate* managed<nuint, int>)(void*)managedAddress;
        delegate* unmanaged<nuint, int> restoredUnmanaged = (delegate* unmanaged<nuint, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to int return");
    }

    private static uint ManagedFunctionNativeUIntToUInt32(nuint value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionNativeUIntToUInt32(nuint value) => (uint)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToUInt32()
    {
        nuint input = (nuint)1;
        uint expected = (uint)1;
        delegate* managed<nuint, uint> managed = &ManagedFunctionNativeUIntToUInt32;
        delegate* unmanaged<nuint, uint> unmanaged = &UnmanagedFunctionNativeUIntToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, uint> restoredManaged = (delegate* managed<nuint, uint>)(void*)managedAddress;
        delegate* unmanaged<nuint, uint> restoredUnmanaged = (delegate* unmanaged<nuint, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to uint return");
    }

    private static long ManagedFunctionNativeUIntToInt64(nuint value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionNativeUIntToInt64(nuint value) => (long)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToInt64()
    {
        nuint input = (nuint)1;
        long expected = (long)1;
        delegate* managed<nuint, long> managed = &ManagedFunctionNativeUIntToInt64;
        delegate* unmanaged<nuint, long> unmanaged = &UnmanagedFunctionNativeUIntToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, long> restoredManaged = (delegate* managed<nuint, long>)(void*)managedAddress;
        delegate* unmanaged<nuint, long> restoredUnmanaged = (delegate* unmanaged<nuint, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to long return");
    }

    private static ulong ManagedFunctionNativeUIntToUInt64(nuint value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionNativeUIntToUInt64(nuint value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToUInt64()
    {
        nuint input = (nuint)1;
        ulong expected = (ulong)1;
        delegate* managed<nuint, ulong> managed = &ManagedFunctionNativeUIntToUInt64;
        delegate* unmanaged<nuint, ulong> unmanaged = &UnmanagedFunctionNativeUIntToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, ulong> restoredManaged = (delegate* managed<nuint, ulong>)(void*)managedAddress;
        delegate* unmanaged<nuint, ulong> restoredUnmanaged = (delegate* unmanaged<nuint, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to ulong return");
    }

    private static nint ManagedFunctionNativeUIntToNativeInt(nuint value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionNativeUIntToNativeInt(nuint value) => (nint)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToNativeInt()
    {
        nuint input = (nuint)1;
        nint expected = (nint)1;
        delegate* managed<nuint, nint> managed = &ManagedFunctionNativeUIntToNativeInt;
        delegate* unmanaged<nuint, nint> unmanaged = &UnmanagedFunctionNativeUIntToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, nint> restoredManaged = (delegate* managed<nuint, nint>)(void*)managedAddress;
        delegate* unmanaged<nuint, nint> restoredUnmanaged = (delegate* unmanaged<nuint, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to nint return");
    }

    private static nuint ManagedFunctionNativeUIntToNativeUInt(nuint value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionNativeUIntToNativeUInt(nuint value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToNativeUInt()
    {
        nuint input = (nuint)1;
        nuint expected = (nuint)1;
        delegate* managed<nuint, nuint> managed = &ManagedFunctionNativeUIntToNativeUInt;
        delegate* unmanaged<nuint, nuint> unmanaged = &UnmanagedFunctionNativeUIntToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, nuint> restoredManaged = (delegate* managed<nuint, nuint>)(void*)managedAddress;
        delegate* unmanaged<nuint, nuint> restoredUnmanaged = (delegate* unmanaged<nuint, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to nuint return");
    }

    private static float ManagedFunctionNativeUIntToSingle(nuint value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionNativeUIntToSingle(nuint value) => (float)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToSingle()
    {
        nuint input = (nuint)1;
        float expected = (float)1;
        delegate* managed<nuint, float> managed = &ManagedFunctionNativeUIntToSingle;
        delegate* unmanaged<nuint, float> unmanaged = &UnmanagedFunctionNativeUIntToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, float> restoredManaged = (delegate* managed<nuint, float>)(void*)managedAddress;
        delegate* unmanaged<nuint, float> restoredUnmanaged = (delegate* unmanaged<nuint, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to float return");
    }

    private static double ManagedFunctionNativeUIntToDouble(nuint value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionNativeUIntToDouble(nuint value) => (double)value;

    private static unsafe void VerifyFunctionPointerNativeUIntToDouble()
    {
        nuint input = (nuint)1;
        double expected = (double)1;
        delegate* managed<nuint, double> managed = &ManagedFunctionNativeUIntToDouble;
        delegate* unmanaged<nuint, double> unmanaged = &UnmanagedFunctionNativeUIntToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<nuint, double> restoredManaged = (delegate* managed<nuint, double>)(void*)managedAddress;
        delegate* unmanaged<nuint, double> restoredUnmanaged = (delegate* unmanaged<nuint, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer nuint parameter to double return");
    }

    private static sbyte ManagedFunctionSingleToSByte(float value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionSingleToSByte(float value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerSingleToSByte()
    {
        float input = (float)1;
        sbyte expected = (sbyte)1;
        delegate* managed<float, sbyte> managed = &ManagedFunctionSingleToSByte;
        delegate* unmanaged<float, sbyte> unmanaged = &UnmanagedFunctionSingleToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, sbyte> restoredManaged = (delegate* managed<float, sbyte>)(void*)managedAddress;
        delegate* unmanaged<float, sbyte> restoredUnmanaged = (delegate* unmanaged<float, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to sbyte return");
    }

    private static byte ManagedFunctionSingleToByte(float value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionSingleToByte(float value) => (byte)value;

    private static unsafe void VerifyFunctionPointerSingleToByte()
    {
        float input = (float)1;
        byte expected = (byte)1;
        delegate* managed<float, byte> managed = &ManagedFunctionSingleToByte;
        delegate* unmanaged<float, byte> unmanaged = &UnmanagedFunctionSingleToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, byte> restoredManaged = (delegate* managed<float, byte>)(void*)managedAddress;
        delegate* unmanaged<float, byte> restoredUnmanaged = (delegate* unmanaged<float, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to byte return");
    }

    private static short ManagedFunctionSingleToInt16(float value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionSingleToInt16(float value) => (short)value;

    private static unsafe void VerifyFunctionPointerSingleToInt16()
    {
        float input = (float)1;
        short expected = (short)1;
        delegate* managed<float, short> managed = &ManagedFunctionSingleToInt16;
        delegate* unmanaged<float, short> unmanaged = &UnmanagedFunctionSingleToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, short> restoredManaged = (delegate* managed<float, short>)(void*)managedAddress;
        delegate* unmanaged<float, short> restoredUnmanaged = (delegate* unmanaged<float, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to short return");
    }

    private static ushort ManagedFunctionSingleToUInt16(float value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionSingleToUInt16(float value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerSingleToUInt16()
    {
        float input = (float)1;
        ushort expected = (ushort)1;
        delegate* managed<float, ushort> managed = &ManagedFunctionSingleToUInt16;
        delegate* unmanaged<float, ushort> unmanaged = &UnmanagedFunctionSingleToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, ushort> restoredManaged = (delegate* managed<float, ushort>)(void*)managedAddress;
        delegate* unmanaged<float, ushort> restoredUnmanaged = (delegate* unmanaged<float, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to ushort return");
    }

    private static char ManagedFunctionSingleToChar(float value) => (char)value;

    private static unsafe void VerifyFunctionPointerSingleToChar()
    {
        float input = (float)1;
        char expected = (char)1;
        delegate* managed<float, char> managed = &ManagedFunctionSingleToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<float, char> restoredManaged = (delegate* managed<float, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer float parameter to char return");
    }

    private static int ManagedFunctionSingleToInt32(float value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionSingleToInt32(float value) => (int)value;

    private static unsafe void VerifyFunctionPointerSingleToInt32()
    {
        float input = (float)1;
        int expected = (int)1;
        delegate* managed<float, int> managed = &ManagedFunctionSingleToInt32;
        delegate* unmanaged<float, int> unmanaged = &UnmanagedFunctionSingleToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, int> restoredManaged = (delegate* managed<float, int>)(void*)managedAddress;
        delegate* unmanaged<float, int> restoredUnmanaged = (delegate* unmanaged<float, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to int return");
    }

    private static uint ManagedFunctionSingleToUInt32(float value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionSingleToUInt32(float value) => (uint)value;

    private static unsafe void VerifyFunctionPointerSingleToUInt32()
    {
        float input = (float)1;
        uint expected = (uint)1;
        delegate* managed<float, uint> managed = &ManagedFunctionSingleToUInt32;
        delegate* unmanaged<float, uint> unmanaged = &UnmanagedFunctionSingleToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, uint> restoredManaged = (delegate* managed<float, uint>)(void*)managedAddress;
        delegate* unmanaged<float, uint> restoredUnmanaged = (delegate* unmanaged<float, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to uint return");
    }

    private static long ManagedFunctionSingleToInt64(float value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionSingleToInt64(float value) => (long)value;

    private static unsafe void VerifyFunctionPointerSingleToInt64()
    {
        float input = (float)1;
        long expected = (long)1;
        delegate* managed<float, long> managed = &ManagedFunctionSingleToInt64;
        delegate* unmanaged<float, long> unmanaged = &UnmanagedFunctionSingleToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, long> restoredManaged = (delegate* managed<float, long>)(void*)managedAddress;
        delegate* unmanaged<float, long> restoredUnmanaged = (delegate* unmanaged<float, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to long return");
    }

    private static ulong ManagedFunctionSingleToUInt64(float value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionSingleToUInt64(float value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerSingleToUInt64()
    {
        float input = (float)1;
        ulong expected = (ulong)1;
        delegate* managed<float, ulong> managed = &ManagedFunctionSingleToUInt64;
        delegate* unmanaged<float, ulong> unmanaged = &UnmanagedFunctionSingleToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, ulong> restoredManaged = (delegate* managed<float, ulong>)(void*)managedAddress;
        delegate* unmanaged<float, ulong> restoredUnmanaged = (delegate* unmanaged<float, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to ulong return");
    }

    private static nint ManagedFunctionSingleToNativeInt(float value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionSingleToNativeInt(float value) => (nint)value;

    private static unsafe void VerifyFunctionPointerSingleToNativeInt()
    {
        float input = (float)1;
        nint expected = (nint)1;
        delegate* managed<float, nint> managed = &ManagedFunctionSingleToNativeInt;
        delegate* unmanaged<float, nint> unmanaged = &UnmanagedFunctionSingleToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, nint> restoredManaged = (delegate* managed<float, nint>)(void*)managedAddress;
        delegate* unmanaged<float, nint> restoredUnmanaged = (delegate* unmanaged<float, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to nint return");
    }

    private static nuint ManagedFunctionSingleToNativeUInt(float value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionSingleToNativeUInt(float value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerSingleToNativeUInt()
    {
        float input = (float)1;
        nuint expected = (nuint)1;
        delegate* managed<float, nuint> managed = &ManagedFunctionSingleToNativeUInt;
        delegate* unmanaged<float, nuint> unmanaged = &UnmanagedFunctionSingleToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, nuint> restoredManaged = (delegate* managed<float, nuint>)(void*)managedAddress;
        delegate* unmanaged<float, nuint> restoredUnmanaged = (delegate* unmanaged<float, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to nuint return");
    }

    private static float ManagedFunctionSingleToSingle(float value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionSingleToSingle(float value) => (float)value;

    private static unsafe void VerifyFunctionPointerSingleToSingle()
    {
        float input = (float)1;
        float expected = (float)1;
        delegate* managed<float, float> managed = &ManagedFunctionSingleToSingle;
        delegate* unmanaged<float, float> unmanaged = &UnmanagedFunctionSingleToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, float> restoredManaged = (delegate* managed<float, float>)(void*)managedAddress;
        delegate* unmanaged<float, float> restoredUnmanaged = (delegate* unmanaged<float, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to float return");
    }

    private static double ManagedFunctionSingleToDouble(float value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionSingleToDouble(float value) => (double)value;

    private static unsafe void VerifyFunctionPointerSingleToDouble()
    {
        float input = (float)1;
        double expected = (double)1;
        delegate* managed<float, double> managed = &ManagedFunctionSingleToDouble;
        delegate* unmanaged<float, double> unmanaged = &UnmanagedFunctionSingleToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<float, double> restoredManaged = (delegate* managed<float, double>)(void*)managedAddress;
        delegate* unmanaged<float, double> restoredUnmanaged = (delegate* unmanaged<float, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer float parameter to double return");
    }

    private static sbyte ManagedFunctionDoubleToSByte(double value) => (sbyte)value;
    [UnmanagedCallersOnly]
    private static sbyte UnmanagedFunctionDoubleToSByte(double value) => (sbyte)value;

    private static unsafe void VerifyFunctionPointerDoubleToSByte()
    {
        double input = (double)1;
        sbyte expected = (sbyte)1;
        delegate* managed<double, sbyte> managed = &ManagedFunctionDoubleToSByte;
        delegate* unmanaged<double, sbyte> unmanaged = &UnmanagedFunctionDoubleToSByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, sbyte> restoredManaged = (delegate* managed<double, sbyte>)(void*)managedAddress;
        delegate* unmanaged<double, sbyte> restoredUnmanaged = (delegate* unmanaged<double, sbyte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to sbyte return");
    }

    private static byte ManagedFunctionDoubleToByte(double value) => (byte)value;
    [UnmanagedCallersOnly]
    private static byte UnmanagedFunctionDoubleToByte(double value) => (byte)value;

    private static unsafe void VerifyFunctionPointerDoubleToByte()
    {
        double input = (double)1;
        byte expected = (byte)1;
        delegate* managed<double, byte> managed = &ManagedFunctionDoubleToByte;
        delegate* unmanaged<double, byte> unmanaged = &UnmanagedFunctionDoubleToByte;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, byte> restoredManaged = (delegate* managed<double, byte>)(void*)managedAddress;
        delegate* unmanaged<double, byte> restoredUnmanaged = (delegate* unmanaged<double, byte>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to byte return");
    }

    private static short ManagedFunctionDoubleToInt16(double value) => (short)value;
    [UnmanagedCallersOnly]
    private static short UnmanagedFunctionDoubleToInt16(double value) => (short)value;

    private static unsafe void VerifyFunctionPointerDoubleToInt16()
    {
        double input = (double)1;
        short expected = (short)1;
        delegate* managed<double, short> managed = &ManagedFunctionDoubleToInt16;
        delegate* unmanaged<double, short> unmanaged = &UnmanagedFunctionDoubleToInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, short> restoredManaged = (delegate* managed<double, short>)(void*)managedAddress;
        delegate* unmanaged<double, short> restoredUnmanaged = (delegate* unmanaged<double, short>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to short return");
    }

    private static ushort ManagedFunctionDoubleToUInt16(double value) => (ushort)value;
    [UnmanagedCallersOnly]
    private static ushort UnmanagedFunctionDoubleToUInt16(double value) => (ushort)value;

    private static unsafe void VerifyFunctionPointerDoubleToUInt16()
    {
        double input = (double)1;
        ushort expected = (ushort)1;
        delegate* managed<double, ushort> managed = &ManagedFunctionDoubleToUInt16;
        delegate* unmanaged<double, ushort> unmanaged = &UnmanagedFunctionDoubleToUInt16;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, ushort> restoredManaged = (delegate* managed<double, ushort>)(void*)managedAddress;
        delegate* unmanaged<double, ushort> restoredUnmanaged = (delegate* unmanaged<double, ushort>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to ushort return");
    }

    private static char ManagedFunctionDoubleToChar(double value) => (char)value;

    private static unsafe void VerifyFunctionPointerDoubleToChar()
    {
        double input = (double)1;
        char expected = (char)1;
        delegate* managed<double, char> managed = &ManagedFunctionDoubleToChar;
        nint managedAddress = (nint)(void*)managed;
        delegate* managed<double, char> restoredManaged = (delegate* managed<double, char>)(void*)managedAddress;
        if (managed(input) != expected || restoredManaged(input) != expected)
            Fail("generated function pointer double parameter to char return");
    }

    private static int ManagedFunctionDoubleToInt32(double value) => (int)value;
    [UnmanagedCallersOnly]
    private static int UnmanagedFunctionDoubleToInt32(double value) => (int)value;

    private static unsafe void VerifyFunctionPointerDoubleToInt32()
    {
        double input = (double)1;
        int expected = (int)1;
        delegate* managed<double, int> managed = &ManagedFunctionDoubleToInt32;
        delegate* unmanaged<double, int> unmanaged = &UnmanagedFunctionDoubleToInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, int> restoredManaged = (delegate* managed<double, int>)(void*)managedAddress;
        delegate* unmanaged<double, int> restoredUnmanaged = (delegate* unmanaged<double, int>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to int return");
    }

    private static uint ManagedFunctionDoubleToUInt32(double value) => (uint)value;
    [UnmanagedCallersOnly]
    private static uint UnmanagedFunctionDoubleToUInt32(double value) => (uint)value;

    private static unsafe void VerifyFunctionPointerDoubleToUInt32()
    {
        double input = (double)1;
        uint expected = (uint)1;
        delegate* managed<double, uint> managed = &ManagedFunctionDoubleToUInt32;
        delegate* unmanaged<double, uint> unmanaged = &UnmanagedFunctionDoubleToUInt32;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, uint> restoredManaged = (delegate* managed<double, uint>)(void*)managedAddress;
        delegate* unmanaged<double, uint> restoredUnmanaged = (delegate* unmanaged<double, uint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to uint return");
    }

    private static long ManagedFunctionDoubleToInt64(double value) => (long)value;
    [UnmanagedCallersOnly]
    private static long UnmanagedFunctionDoubleToInt64(double value) => (long)value;

    private static unsafe void VerifyFunctionPointerDoubleToInt64()
    {
        double input = (double)1;
        long expected = (long)1;
        delegate* managed<double, long> managed = &ManagedFunctionDoubleToInt64;
        delegate* unmanaged<double, long> unmanaged = &UnmanagedFunctionDoubleToInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, long> restoredManaged = (delegate* managed<double, long>)(void*)managedAddress;
        delegate* unmanaged<double, long> restoredUnmanaged = (delegate* unmanaged<double, long>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to long return");
    }

    private static ulong ManagedFunctionDoubleToUInt64(double value) => (ulong)value;
    [UnmanagedCallersOnly]
    private static ulong UnmanagedFunctionDoubleToUInt64(double value) => (ulong)value;

    private static unsafe void VerifyFunctionPointerDoubleToUInt64()
    {
        double input = (double)1;
        ulong expected = (ulong)1;
        delegate* managed<double, ulong> managed = &ManagedFunctionDoubleToUInt64;
        delegate* unmanaged<double, ulong> unmanaged = &UnmanagedFunctionDoubleToUInt64;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, ulong> restoredManaged = (delegate* managed<double, ulong>)(void*)managedAddress;
        delegate* unmanaged<double, ulong> restoredUnmanaged = (delegate* unmanaged<double, ulong>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to ulong return");
    }

    private static nint ManagedFunctionDoubleToNativeInt(double value) => (nint)value;
    [UnmanagedCallersOnly]
    private static nint UnmanagedFunctionDoubleToNativeInt(double value) => (nint)value;

    private static unsafe void VerifyFunctionPointerDoubleToNativeInt()
    {
        double input = (double)1;
        nint expected = (nint)1;
        delegate* managed<double, nint> managed = &ManagedFunctionDoubleToNativeInt;
        delegate* unmanaged<double, nint> unmanaged = &UnmanagedFunctionDoubleToNativeInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, nint> restoredManaged = (delegate* managed<double, nint>)(void*)managedAddress;
        delegate* unmanaged<double, nint> restoredUnmanaged = (delegate* unmanaged<double, nint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to nint return");
    }

    private static nuint ManagedFunctionDoubleToNativeUInt(double value) => (nuint)value;
    [UnmanagedCallersOnly]
    private static nuint UnmanagedFunctionDoubleToNativeUInt(double value) => (nuint)value;

    private static unsafe void VerifyFunctionPointerDoubleToNativeUInt()
    {
        double input = (double)1;
        nuint expected = (nuint)1;
        delegate* managed<double, nuint> managed = &ManagedFunctionDoubleToNativeUInt;
        delegate* unmanaged<double, nuint> unmanaged = &UnmanagedFunctionDoubleToNativeUInt;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, nuint> restoredManaged = (delegate* managed<double, nuint>)(void*)managedAddress;
        delegate* unmanaged<double, nuint> restoredUnmanaged = (delegate* unmanaged<double, nuint>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to nuint return");
    }

    private static float ManagedFunctionDoubleToSingle(double value) => (float)value;
    [UnmanagedCallersOnly]
    private static float UnmanagedFunctionDoubleToSingle(double value) => (float)value;

    private static unsafe void VerifyFunctionPointerDoubleToSingle()
    {
        double input = (double)1;
        float expected = (float)1;
        delegate* managed<double, float> managed = &ManagedFunctionDoubleToSingle;
        delegate* unmanaged<double, float> unmanaged = &UnmanagedFunctionDoubleToSingle;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, float> restoredManaged = (delegate* managed<double, float>)(void*)managedAddress;
        delegate* unmanaged<double, float> restoredUnmanaged = (delegate* unmanaged<double, float>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to float return");
    }

    private static double ManagedFunctionDoubleToDouble(double value) => (double)value;
    [UnmanagedCallersOnly]
    private static double UnmanagedFunctionDoubleToDouble(double value) => (double)value;

    private static unsafe void VerifyFunctionPointerDoubleToDouble()
    {
        double input = (double)1;
        double expected = (double)1;
        delegate* managed<double, double> managed = &ManagedFunctionDoubleToDouble;
        delegate* unmanaged<double, double> unmanaged = &UnmanagedFunctionDoubleToDouble;
        nint managedAddress = (nint)(void*)managed;
        nint unmanagedAddress = (nint)(void*)unmanaged;
        delegate* managed<double, double> restoredManaged = (delegate* managed<double, double>)(void*)managedAddress;
        delegate* unmanaged<double, double> restoredUnmanaged = (delegate* unmanaged<double, double>)(void*)unmanagedAddress;
        if (managed(input) != expected || unmanaged(input) != expected ||
            restoredManaged(input) != expected || restoredUnmanaged(input) != expected)
            Fail("generated function pointer double parameter to double return");
    }

    private static void VerifyGeneratedPointerFunctionPointers()
    {
        VerifyPointerFunctionSByteAndSByte();
        VerifyPointerFunctionSByteAndByte();
        VerifyPointerFunctionSByteAndInt16();
        VerifyPointerFunctionSByteAndUInt16();
        VerifyPointerFunctionSByteAndChar();
        VerifyPointerFunctionSByteAndInt32();
        VerifyPointerFunctionSByteAndUInt32();
        VerifyPointerFunctionSByteAndInt64();
        VerifyPointerFunctionSByteAndUInt64();
        VerifyPointerFunctionSByteAndNativeInt();
        VerifyPointerFunctionSByteAndNativeUInt();
        VerifyPointerFunctionSByteAndSingle();
        VerifyPointerFunctionSByteAndDouble();
        VerifyPointerFunctionSByteAndBoolean();
        VerifyPointerFunctionByteAndSByte();
        VerifyPointerFunctionByteAndByte();
        VerifyPointerFunctionByteAndInt16();
        VerifyPointerFunctionByteAndUInt16();
        VerifyPointerFunctionByteAndChar();
        VerifyPointerFunctionByteAndInt32();
        VerifyPointerFunctionByteAndUInt32();
        VerifyPointerFunctionByteAndInt64();
        VerifyPointerFunctionByteAndUInt64();
        VerifyPointerFunctionByteAndNativeInt();
        VerifyPointerFunctionByteAndNativeUInt();
        VerifyPointerFunctionByteAndSingle();
        VerifyPointerFunctionByteAndDouble();
        VerifyPointerFunctionByteAndBoolean();
        VerifyPointerFunctionInt16AndSByte();
        VerifyPointerFunctionInt16AndByte();
        VerifyPointerFunctionInt16AndInt16();
        VerifyPointerFunctionInt16AndUInt16();
        VerifyPointerFunctionInt16AndChar();
        VerifyPointerFunctionInt16AndInt32();
        VerifyPointerFunctionInt16AndUInt32();
        VerifyPointerFunctionInt16AndInt64();
        VerifyPointerFunctionInt16AndUInt64();
        VerifyPointerFunctionInt16AndNativeInt();
        VerifyPointerFunctionInt16AndNativeUInt();
        VerifyPointerFunctionInt16AndSingle();
        VerifyPointerFunctionInt16AndDouble();
        VerifyPointerFunctionInt16AndBoolean();
        VerifyPointerFunctionUInt16AndSByte();
        VerifyPointerFunctionUInt16AndByte();
        VerifyPointerFunctionUInt16AndInt16();
        VerifyPointerFunctionUInt16AndUInt16();
        VerifyPointerFunctionUInt16AndChar();
        VerifyPointerFunctionUInt16AndInt32();
        VerifyPointerFunctionUInt16AndUInt32();
        VerifyPointerFunctionUInt16AndInt64();
        VerifyPointerFunctionUInt16AndUInt64();
        VerifyPointerFunctionUInt16AndNativeInt();
        VerifyPointerFunctionUInt16AndNativeUInt();
        VerifyPointerFunctionUInt16AndSingle();
        VerifyPointerFunctionUInt16AndDouble();
        VerifyPointerFunctionUInt16AndBoolean();
        VerifyPointerFunctionCharAndSByte();
        VerifyPointerFunctionCharAndByte();
        VerifyPointerFunctionCharAndInt16();
        VerifyPointerFunctionCharAndUInt16();
        VerifyPointerFunctionCharAndChar();
        VerifyPointerFunctionCharAndInt32();
        VerifyPointerFunctionCharAndUInt32();
        VerifyPointerFunctionCharAndInt64();
        VerifyPointerFunctionCharAndUInt64();
        VerifyPointerFunctionCharAndNativeInt();
        VerifyPointerFunctionCharAndNativeUInt();
        VerifyPointerFunctionCharAndSingle();
        VerifyPointerFunctionCharAndDouble();
        VerifyPointerFunctionCharAndBoolean();
        VerifyPointerFunctionInt32AndSByte();
        VerifyPointerFunctionInt32AndByte();
        VerifyPointerFunctionInt32AndInt16();
        VerifyPointerFunctionInt32AndUInt16();
        VerifyPointerFunctionInt32AndChar();
        VerifyPointerFunctionInt32AndInt32();
        VerifyPointerFunctionInt32AndUInt32();
        VerifyPointerFunctionInt32AndInt64();
        VerifyPointerFunctionInt32AndUInt64();
        VerifyPointerFunctionInt32AndNativeInt();
        VerifyPointerFunctionInt32AndNativeUInt();
        VerifyPointerFunctionInt32AndSingle();
        VerifyPointerFunctionInt32AndDouble();
        VerifyPointerFunctionInt32AndBoolean();
        VerifyPointerFunctionUInt32AndSByte();
        VerifyPointerFunctionUInt32AndByte();
        VerifyPointerFunctionUInt32AndInt16();
        VerifyPointerFunctionUInt32AndUInt16();
        VerifyPointerFunctionUInt32AndChar();
        VerifyPointerFunctionUInt32AndInt32();
        VerifyPointerFunctionUInt32AndUInt32();
        VerifyPointerFunctionUInt32AndInt64();
        VerifyPointerFunctionUInt32AndUInt64();
        VerifyPointerFunctionUInt32AndNativeInt();
        VerifyPointerFunctionUInt32AndNativeUInt();
        VerifyPointerFunctionUInt32AndSingle();
        VerifyPointerFunctionUInt32AndDouble();
        VerifyPointerFunctionUInt32AndBoolean();
        VerifyPointerFunctionInt64AndSByte();
        VerifyPointerFunctionInt64AndByte();
        VerifyPointerFunctionInt64AndInt16();
        VerifyPointerFunctionInt64AndUInt16();
        VerifyPointerFunctionInt64AndChar();
        VerifyPointerFunctionInt64AndInt32();
        VerifyPointerFunctionInt64AndUInt32();
        VerifyPointerFunctionInt64AndInt64();
        VerifyPointerFunctionInt64AndUInt64();
        VerifyPointerFunctionInt64AndNativeInt();
        VerifyPointerFunctionInt64AndNativeUInt();
        VerifyPointerFunctionInt64AndSingle();
        VerifyPointerFunctionInt64AndDouble();
        VerifyPointerFunctionInt64AndBoolean();
        VerifyPointerFunctionUInt64AndSByte();
        VerifyPointerFunctionUInt64AndByte();
        VerifyPointerFunctionUInt64AndInt16();
        VerifyPointerFunctionUInt64AndUInt16();
        VerifyPointerFunctionUInt64AndChar();
        VerifyPointerFunctionUInt64AndInt32();
        VerifyPointerFunctionUInt64AndUInt32();
        VerifyPointerFunctionUInt64AndInt64();
        VerifyPointerFunctionUInt64AndUInt64();
        VerifyPointerFunctionUInt64AndNativeInt();
        VerifyPointerFunctionUInt64AndNativeUInt();
        VerifyPointerFunctionUInt64AndSingle();
        VerifyPointerFunctionUInt64AndDouble();
        VerifyPointerFunctionUInt64AndBoolean();
        VerifyPointerFunctionNativeIntAndSByte();
        VerifyPointerFunctionNativeIntAndByte();
        VerifyPointerFunctionNativeIntAndInt16();
        VerifyPointerFunctionNativeIntAndUInt16();
        VerifyPointerFunctionNativeIntAndChar();
        VerifyPointerFunctionNativeIntAndInt32();
        VerifyPointerFunctionNativeIntAndUInt32();
        VerifyPointerFunctionNativeIntAndInt64();
        VerifyPointerFunctionNativeIntAndUInt64();
        VerifyPointerFunctionNativeIntAndNativeInt();
        VerifyPointerFunctionNativeIntAndNativeUInt();
        VerifyPointerFunctionNativeIntAndSingle();
        VerifyPointerFunctionNativeIntAndDouble();
        VerifyPointerFunctionNativeIntAndBoolean();
        VerifyPointerFunctionNativeUIntAndSByte();
        VerifyPointerFunctionNativeUIntAndByte();
        VerifyPointerFunctionNativeUIntAndInt16();
        VerifyPointerFunctionNativeUIntAndUInt16();
        VerifyPointerFunctionNativeUIntAndChar();
        VerifyPointerFunctionNativeUIntAndInt32();
        VerifyPointerFunctionNativeUIntAndUInt32();
        VerifyPointerFunctionNativeUIntAndInt64();
        VerifyPointerFunctionNativeUIntAndUInt64();
        VerifyPointerFunctionNativeUIntAndNativeInt();
        VerifyPointerFunctionNativeUIntAndNativeUInt();
        VerifyPointerFunctionNativeUIntAndSingle();
        VerifyPointerFunctionNativeUIntAndDouble();
        VerifyPointerFunctionNativeUIntAndBoolean();
        VerifyPointerFunctionSingleAndSByte();
        VerifyPointerFunctionSingleAndByte();
        VerifyPointerFunctionSingleAndInt16();
        VerifyPointerFunctionSingleAndUInt16();
        VerifyPointerFunctionSingleAndChar();
        VerifyPointerFunctionSingleAndInt32();
        VerifyPointerFunctionSingleAndUInt32();
        VerifyPointerFunctionSingleAndInt64();
        VerifyPointerFunctionSingleAndUInt64();
        VerifyPointerFunctionSingleAndNativeInt();
        VerifyPointerFunctionSingleAndNativeUInt();
        VerifyPointerFunctionSingleAndSingle();
        VerifyPointerFunctionSingleAndDouble();
        VerifyPointerFunctionSingleAndBoolean();
        VerifyPointerFunctionDoubleAndSByte();
        VerifyPointerFunctionDoubleAndByte();
        VerifyPointerFunctionDoubleAndInt16();
        VerifyPointerFunctionDoubleAndUInt16();
        VerifyPointerFunctionDoubleAndChar();
        VerifyPointerFunctionDoubleAndInt32();
        VerifyPointerFunctionDoubleAndUInt32();
        VerifyPointerFunctionDoubleAndInt64();
        VerifyPointerFunctionDoubleAndUInt64();
        VerifyPointerFunctionDoubleAndNativeInt();
        VerifyPointerFunctionDoubleAndNativeUInt();
        VerifyPointerFunctionDoubleAndSingle();
        VerifyPointerFunctionDoubleAndDouble();
        VerifyPointerFunctionDoubleAndBoolean();
        VerifyPointerFunctionBooleanAndSByte();
        VerifyPointerFunctionBooleanAndByte();
        VerifyPointerFunctionBooleanAndInt16();
        VerifyPointerFunctionBooleanAndUInt16();
        VerifyPointerFunctionBooleanAndChar();
        VerifyPointerFunctionBooleanAndInt32();
        VerifyPointerFunctionBooleanAndUInt32();
        VerifyPointerFunctionBooleanAndInt64();
        VerifyPointerFunctionBooleanAndUInt64();
        VerifyPointerFunctionBooleanAndNativeInt();
        VerifyPointerFunctionBooleanAndNativeUInt();
        VerifyPointerFunctionBooleanAndSingle();
        VerifyPointerFunctionBooleanAndDouble();
        VerifyPointerFunctionBooleanAndBoolean();
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndSByte(sbyte* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndSByte(sbyte* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndSByte()
    {
        sbyte sourceValue = (sbyte)-11;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<sbyte*, sbyte*, sbyte*> function = &ManagedPointerFunctionSByteAndSByte;
        delegate* unmanaged<sbyte*, sbyte*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and sbyte* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndByte(sbyte* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndByte(sbyte* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndByte()
    {
        sbyte sourceValue = (sbyte)-11;
        byte destinationValue = (byte)211;
        delegate* managed<sbyte*, byte*, sbyte*> function = &ManagedPointerFunctionSByteAndByte;
        delegate* unmanaged<sbyte*, byte*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and byte* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndInt16(sbyte* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndInt16(sbyte* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndInt16()
    {
        sbyte sourceValue = (sbyte)-11;
        short destinationValue = (short)-1234;
        delegate* managed<sbyte*, short*, sbyte*> function = &ManagedPointerFunctionSByteAndInt16;
        delegate* unmanaged<sbyte*, short*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and short* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndUInt16(sbyte* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndUInt16(sbyte* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndUInt16()
    {
        sbyte sourceValue = (sbyte)-11;
        ushort destinationValue = (ushort)54321;
        delegate* managed<sbyte*, ushort*, sbyte*> function = &ManagedPointerFunctionSByteAndUInt16;
        delegate* unmanaged<sbyte*, ushort*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and ushort* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndChar(sbyte* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndChar(sbyte* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndChar()
    {
        sbyte sourceValue = (sbyte)-11;
        char destinationValue = 'K';
        delegate* managed<sbyte*, char*, sbyte*> function = &ManagedPointerFunctionSByteAndChar;
        delegate* unmanaged<sbyte*, char*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and char* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndInt32(sbyte* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndInt32(sbyte* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndInt32()
    {
        sbyte sourceValue = (sbyte)-11;
        int destinationValue = -1234567;
        delegate* managed<sbyte*, int*, sbyte*> function = &ManagedPointerFunctionSByteAndInt32;
        delegate* unmanaged<sbyte*, int*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and int* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndUInt32(sbyte* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndUInt32(sbyte* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndUInt32()
    {
        sbyte sourceValue = (sbyte)-11;
        uint destinationValue = 3456789012u;
        delegate* managed<sbyte*, uint*, sbyte*> function = &ManagedPointerFunctionSByteAndUInt32;
        delegate* unmanaged<sbyte*, uint*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and uint* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndInt64(sbyte* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndInt64(sbyte* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndInt64()
    {
        sbyte sourceValue = (sbyte)-11;
        long destinationValue = -1234567890123L;
        delegate* managed<sbyte*, long*, sbyte*> function = &ManagedPointerFunctionSByteAndInt64;
        delegate* unmanaged<sbyte*, long*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and long* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndUInt64(sbyte* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndUInt64(sbyte* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndUInt64()
    {
        sbyte sourceValue = (sbyte)-11;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<sbyte*, ulong*, sbyte*> function = &ManagedPointerFunctionSByteAndUInt64;
        delegate* unmanaged<sbyte*, ulong*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and ulong* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndNativeInt(sbyte* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndNativeInt(sbyte* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndNativeInt()
    {
        sbyte sourceValue = (sbyte)-11;
        nint destinationValue = (nint)0x123456;
        delegate* managed<sbyte*, nint*, sbyte*> function = &ManagedPointerFunctionSByteAndNativeInt;
        delegate* unmanaged<sbyte*, nint*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and nint* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndNativeUInt(sbyte* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndNativeUInt(sbyte* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndNativeUInt()
    {
        sbyte sourceValue = (sbyte)-11;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<sbyte*, nuint*, sbyte*> function = &ManagedPointerFunctionSByteAndNativeUInt;
        delegate* unmanaged<sbyte*, nuint*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and nuint* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndSingle(sbyte* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndSingle(sbyte* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndSingle()
    {
        sbyte sourceValue = (sbyte)-11;
        float destinationValue = 12.25f;
        delegate* managed<sbyte*, float*, sbyte*> function = &ManagedPointerFunctionSByteAndSingle;
        delegate* unmanaged<sbyte*, float*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and float* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndDouble(sbyte* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndDouble(sbyte* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndDouble()
    {
        sbyte sourceValue = (sbyte)-11;
        double destinationValue = -33.5;
        delegate* managed<sbyte*, double*, sbyte*> function = &ManagedPointerFunctionSByteAndDouble;
        delegate* unmanaged<sbyte*, double*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and double* parameters");
    }

    private static unsafe sbyte* ManagedPointerFunctionSByteAndBoolean(sbyte* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe sbyte* UnmanagedPointerFunctionSByteAndBoolean(sbyte* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSByteAndBoolean()
    {
        sbyte sourceValue = (sbyte)-11;
        bool destinationValue = true;
        delegate* managed<sbyte*, bool*, sbyte*> function = &ManagedPointerFunctionSByteAndBoolean;
        delegate* unmanaged<sbyte*, bool*, sbyte*> unmanagedFunction = &UnmanagedPointerFunctionSByteAndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with sbyte* and bool* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndSByte(byte* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndSByte(byte* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndSByte()
    {
        byte sourceValue = (byte)211;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<byte*, sbyte*, byte*> function = &ManagedPointerFunctionByteAndSByte;
        delegate* unmanaged<byte*, sbyte*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and sbyte* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndByte(byte* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndByte(byte* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndByte()
    {
        byte sourceValue = (byte)211;
        byte destinationValue = (byte)211;
        delegate* managed<byte*, byte*, byte*> function = &ManagedPointerFunctionByteAndByte;
        delegate* unmanaged<byte*, byte*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and byte* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndInt16(byte* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndInt16(byte* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndInt16()
    {
        byte sourceValue = (byte)211;
        short destinationValue = (short)-1234;
        delegate* managed<byte*, short*, byte*> function = &ManagedPointerFunctionByteAndInt16;
        delegate* unmanaged<byte*, short*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and short* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndUInt16(byte* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndUInt16(byte* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndUInt16()
    {
        byte sourceValue = (byte)211;
        ushort destinationValue = (ushort)54321;
        delegate* managed<byte*, ushort*, byte*> function = &ManagedPointerFunctionByteAndUInt16;
        delegate* unmanaged<byte*, ushort*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and ushort* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndChar(byte* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndChar(byte* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndChar()
    {
        byte sourceValue = (byte)211;
        char destinationValue = 'K';
        delegate* managed<byte*, char*, byte*> function = &ManagedPointerFunctionByteAndChar;
        delegate* unmanaged<byte*, char*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and char* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndInt32(byte* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndInt32(byte* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndInt32()
    {
        byte sourceValue = (byte)211;
        int destinationValue = -1234567;
        delegate* managed<byte*, int*, byte*> function = &ManagedPointerFunctionByteAndInt32;
        delegate* unmanaged<byte*, int*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and int* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndUInt32(byte* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndUInt32(byte* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndUInt32()
    {
        byte sourceValue = (byte)211;
        uint destinationValue = 3456789012u;
        delegate* managed<byte*, uint*, byte*> function = &ManagedPointerFunctionByteAndUInt32;
        delegate* unmanaged<byte*, uint*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and uint* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndInt64(byte* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndInt64(byte* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndInt64()
    {
        byte sourceValue = (byte)211;
        long destinationValue = -1234567890123L;
        delegate* managed<byte*, long*, byte*> function = &ManagedPointerFunctionByteAndInt64;
        delegate* unmanaged<byte*, long*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and long* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndUInt64(byte* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndUInt64(byte* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndUInt64()
    {
        byte sourceValue = (byte)211;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<byte*, ulong*, byte*> function = &ManagedPointerFunctionByteAndUInt64;
        delegate* unmanaged<byte*, ulong*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and ulong* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndNativeInt(byte* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndNativeInt(byte* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndNativeInt()
    {
        byte sourceValue = (byte)211;
        nint destinationValue = (nint)0x123456;
        delegate* managed<byte*, nint*, byte*> function = &ManagedPointerFunctionByteAndNativeInt;
        delegate* unmanaged<byte*, nint*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and nint* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndNativeUInt(byte* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndNativeUInt(byte* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndNativeUInt()
    {
        byte sourceValue = (byte)211;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<byte*, nuint*, byte*> function = &ManagedPointerFunctionByteAndNativeUInt;
        delegate* unmanaged<byte*, nuint*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and nuint* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndSingle(byte* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndSingle(byte* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndSingle()
    {
        byte sourceValue = (byte)211;
        float destinationValue = 12.25f;
        delegate* managed<byte*, float*, byte*> function = &ManagedPointerFunctionByteAndSingle;
        delegate* unmanaged<byte*, float*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and float* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndDouble(byte* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndDouble(byte* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndDouble()
    {
        byte sourceValue = (byte)211;
        double destinationValue = -33.5;
        delegate* managed<byte*, double*, byte*> function = &ManagedPointerFunctionByteAndDouble;
        delegate* unmanaged<byte*, double*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and double* parameters");
    }

    private static unsafe byte* ManagedPointerFunctionByteAndBoolean(byte* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe byte* UnmanagedPointerFunctionByteAndBoolean(byte* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionByteAndBoolean()
    {
        byte sourceValue = (byte)211;
        bool destinationValue = true;
        delegate* managed<byte*, bool*, byte*> function = &ManagedPointerFunctionByteAndBoolean;
        delegate* unmanaged<byte*, bool*, byte*> unmanagedFunction = &UnmanagedPointerFunctionByteAndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with byte* and bool* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndSByte(short* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndSByte(short* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndSByte()
    {
        short sourceValue = (short)-1234;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<short*, sbyte*, short*> function = &ManagedPointerFunctionInt16AndSByte;
        delegate* unmanaged<short*, sbyte*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and sbyte* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndByte(short* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndByte(short* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndByte()
    {
        short sourceValue = (short)-1234;
        byte destinationValue = (byte)211;
        delegate* managed<short*, byte*, short*> function = &ManagedPointerFunctionInt16AndByte;
        delegate* unmanaged<short*, byte*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and byte* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndInt16(short* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndInt16(short* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndInt16()
    {
        short sourceValue = (short)-1234;
        short destinationValue = (short)-1234;
        delegate* managed<short*, short*, short*> function = &ManagedPointerFunctionInt16AndInt16;
        delegate* unmanaged<short*, short*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and short* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndUInt16(short* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndUInt16(short* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndUInt16()
    {
        short sourceValue = (short)-1234;
        ushort destinationValue = (ushort)54321;
        delegate* managed<short*, ushort*, short*> function = &ManagedPointerFunctionInt16AndUInt16;
        delegate* unmanaged<short*, ushort*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and ushort* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndChar(short* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndChar(short* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndChar()
    {
        short sourceValue = (short)-1234;
        char destinationValue = 'K';
        delegate* managed<short*, char*, short*> function = &ManagedPointerFunctionInt16AndChar;
        delegate* unmanaged<short*, char*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and char* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndInt32(short* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndInt32(short* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndInt32()
    {
        short sourceValue = (short)-1234;
        int destinationValue = -1234567;
        delegate* managed<short*, int*, short*> function = &ManagedPointerFunctionInt16AndInt32;
        delegate* unmanaged<short*, int*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and int* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndUInt32(short* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndUInt32(short* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndUInt32()
    {
        short sourceValue = (short)-1234;
        uint destinationValue = 3456789012u;
        delegate* managed<short*, uint*, short*> function = &ManagedPointerFunctionInt16AndUInt32;
        delegate* unmanaged<short*, uint*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and uint* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndInt64(short* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndInt64(short* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndInt64()
    {
        short sourceValue = (short)-1234;
        long destinationValue = -1234567890123L;
        delegate* managed<short*, long*, short*> function = &ManagedPointerFunctionInt16AndInt64;
        delegate* unmanaged<short*, long*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and long* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndUInt64(short* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndUInt64(short* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndUInt64()
    {
        short sourceValue = (short)-1234;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<short*, ulong*, short*> function = &ManagedPointerFunctionInt16AndUInt64;
        delegate* unmanaged<short*, ulong*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and ulong* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndNativeInt(short* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndNativeInt(short* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndNativeInt()
    {
        short sourceValue = (short)-1234;
        nint destinationValue = (nint)0x123456;
        delegate* managed<short*, nint*, short*> function = &ManagedPointerFunctionInt16AndNativeInt;
        delegate* unmanaged<short*, nint*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and nint* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndNativeUInt(short* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndNativeUInt(short* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndNativeUInt()
    {
        short sourceValue = (short)-1234;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<short*, nuint*, short*> function = &ManagedPointerFunctionInt16AndNativeUInt;
        delegate* unmanaged<short*, nuint*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and nuint* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndSingle(short* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndSingle(short* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndSingle()
    {
        short sourceValue = (short)-1234;
        float destinationValue = 12.25f;
        delegate* managed<short*, float*, short*> function = &ManagedPointerFunctionInt16AndSingle;
        delegate* unmanaged<short*, float*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and float* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndDouble(short* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndDouble(short* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndDouble()
    {
        short sourceValue = (short)-1234;
        double destinationValue = -33.5;
        delegate* managed<short*, double*, short*> function = &ManagedPointerFunctionInt16AndDouble;
        delegate* unmanaged<short*, double*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and double* parameters");
    }

    private static unsafe short* ManagedPointerFunctionInt16AndBoolean(short* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe short* UnmanagedPointerFunctionInt16AndBoolean(short* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt16AndBoolean()
    {
        short sourceValue = (short)-1234;
        bool destinationValue = true;
        delegate* managed<short*, bool*, short*> function = &ManagedPointerFunctionInt16AndBoolean;
        delegate* unmanaged<short*, bool*, short*> unmanagedFunction = &UnmanagedPointerFunctionInt16AndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with short* and bool* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndSByte(ushort* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndSByte(ushort* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndSByte()
    {
        ushort sourceValue = (ushort)54321;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<ushort*, sbyte*, ushort*> function = &ManagedPointerFunctionUInt16AndSByte;
        delegate* unmanaged<ushort*, sbyte*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and sbyte* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndByte(ushort* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndByte(ushort* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndByte()
    {
        ushort sourceValue = (ushort)54321;
        byte destinationValue = (byte)211;
        delegate* managed<ushort*, byte*, ushort*> function = &ManagedPointerFunctionUInt16AndByte;
        delegate* unmanaged<ushort*, byte*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and byte* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndInt16(ushort* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndInt16(ushort* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndInt16()
    {
        ushort sourceValue = (ushort)54321;
        short destinationValue = (short)-1234;
        delegate* managed<ushort*, short*, ushort*> function = &ManagedPointerFunctionUInt16AndInt16;
        delegate* unmanaged<ushort*, short*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and short* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndUInt16(ushort* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndUInt16(ushort* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndUInt16()
    {
        ushort sourceValue = (ushort)54321;
        ushort destinationValue = (ushort)54321;
        delegate* managed<ushort*, ushort*, ushort*> function = &ManagedPointerFunctionUInt16AndUInt16;
        delegate* unmanaged<ushort*, ushort*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and ushort* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndChar(ushort* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndChar(ushort* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndChar()
    {
        ushort sourceValue = (ushort)54321;
        char destinationValue = 'K';
        delegate* managed<ushort*, char*, ushort*> function = &ManagedPointerFunctionUInt16AndChar;
        delegate* unmanaged<ushort*, char*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and char* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndInt32(ushort* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndInt32(ushort* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndInt32()
    {
        ushort sourceValue = (ushort)54321;
        int destinationValue = -1234567;
        delegate* managed<ushort*, int*, ushort*> function = &ManagedPointerFunctionUInt16AndInt32;
        delegate* unmanaged<ushort*, int*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and int* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndUInt32(ushort* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndUInt32(ushort* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndUInt32()
    {
        ushort sourceValue = (ushort)54321;
        uint destinationValue = 3456789012u;
        delegate* managed<ushort*, uint*, ushort*> function = &ManagedPointerFunctionUInt16AndUInt32;
        delegate* unmanaged<ushort*, uint*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and uint* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndInt64(ushort* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndInt64(ushort* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndInt64()
    {
        ushort sourceValue = (ushort)54321;
        long destinationValue = -1234567890123L;
        delegate* managed<ushort*, long*, ushort*> function = &ManagedPointerFunctionUInt16AndInt64;
        delegate* unmanaged<ushort*, long*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and long* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndUInt64(ushort* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndUInt64(ushort* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndUInt64()
    {
        ushort sourceValue = (ushort)54321;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<ushort*, ulong*, ushort*> function = &ManagedPointerFunctionUInt16AndUInt64;
        delegate* unmanaged<ushort*, ulong*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and ulong* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndNativeInt(ushort* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndNativeInt(ushort* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndNativeInt()
    {
        ushort sourceValue = (ushort)54321;
        nint destinationValue = (nint)0x123456;
        delegate* managed<ushort*, nint*, ushort*> function = &ManagedPointerFunctionUInt16AndNativeInt;
        delegate* unmanaged<ushort*, nint*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and nint* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndNativeUInt(ushort* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndNativeUInt(ushort* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndNativeUInt()
    {
        ushort sourceValue = (ushort)54321;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<ushort*, nuint*, ushort*> function = &ManagedPointerFunctionUInt16AndNativeUInt;
        delegate* unmanaged<ushort*, nuint*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and nuint* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndSingle(ushort* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndSingle(ushort* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndSingle()
    {
        ushort sourceValue = (ushort)54321;
        float destinationValue = 12.25f;
        delegate* managed<ushort*, float*, ushort*> function = &ManagedPointerFunctionUInt16AndSingle;
        delegate* unmanaged<ushort*, float*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and float* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndDouble(ushort* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndDouble(ushort* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndDouble()
    {
        ushort sourceValue = (ushort)54321;
        double destinationValue = -33.5;
        delegate* managed<ushort*, double*, ushort*> function = &ManagedPointerFunctionUInt16AndDouble;
        delegate* unmanaged<ushort*, double*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and double* parameters");
    }

    private static unsafe ushort* ManagedPointerFunctionUInt16AndBoolean(ushort* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ushort* UnmanagedPointerFunctionUInt16AndBoolean(ushort* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt16AndBoolean()
    {
        ushort sourceValue = (ushort)54321;
        bool destinationValue = true;
        delegate* managed<ushort*, bool*, ushort*> function = &ManagedPointerFunctionUInt16AndBoolean;
        delegate* unmanaged<ushort*, bool*, ushort*> unmanagedFunction = &UnmanagedPointerFunctionUInt16AndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ushort* and bool* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndSByte(char* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndSByte(char* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndSByte()
    {
        char sourceValue = 'K';
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<char*, sbyte*, char*> function = &ManagedPointerFunctionCharAndSByte;
        delegate* unmanaged<char*, sbyte*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and sbyte* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndByte(char* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndByte(char* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndByte()
    {
        char sourceValue = 'K';
        byte destinationValue = (byte)211;
        delegate* managed<char*, byte*, char*> function = &ManagedPointerFunctionCharAndByte;
        delegate* unmanaged<char*, byte*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and byte* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndInt16(char* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndInt16(char* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndInt16()
    {
        char sourceValue = 'K';
        short destinationValue = (short)-1234;
        delegate* managed<char*, short*, char*> function = &ManagedPointerFunctionCharAndInt16;
        delegate* unmanaged<char*, short*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and short* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndUInt16(char* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndUInt16(char* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndUInt16()
    {
        char sourceValue = 'K';
        ushort destinationValue = (ushort)54321;
        delegate* managed<char*, ushort*, char*> function = &ManagedPointerFunctionCharAndUInt16;
        delegate* unmanaged<char*, ushort*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and ushort* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndChar(char* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndChar(char* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndChar()
    {
        char sourceValue = 'K';
        char destinationValue = 'K';
        delegate* managed<char*, char*, char*> function = &ManagedPointerFunctionCharAndChar;
        delegate* unmanaged<char*, char*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and char* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndInt32(char* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndInt32(char* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndInt32()
    {
        char sourceValue = 'K';
        int destinationValue = -1234567;
        delegate* managed<char*, int*, char*> function = &ManagedPointerFunctionCharAndInt32;
        delegate* unmanaged<char*, int*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and int* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndUInt32(char* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndUInt32(char* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndUInt32()
    {
        char sourceValue = 'K';
        uint destinationValue = 3456789012u;
        delegate* managed<char*, uint*, char*> function = &ManagedPointerFunctionCharAndUInt32;
        delegate* unmanaged<char*, uint*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and uint* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndInt64(char* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndInt64(char* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndInt64()
    {
        char sourceValue = 'K';
        long destinationValue = -1234567890123L;
        delegate* managed<char*, long*, char*> function = &ManagedPointerFunctionCharAndInt64;
        delegate* unmanaged<char*, long*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and long* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndUInt64(char* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndUInt64(char* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndUInt64()
    {
        char sourceValue = 'K';
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<char*, ulong*, char*> function = &ManagedPointerFunctionCharAndUInt64;
        delegate* unmanaged<char*, ulong*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and ulong* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndNativeInt(char* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndNativeInt(char* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndNativeInt()
    {
        char sourceValue = 'K';
        nint destinationValue = (nint)0x123456;
        delegate* managed<char*, nint*, char*> function = &ManagedPointerFunctionCharAndNativeInt;
        delegate* unmanaged<char*, nint*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and nint* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndNativeUInt(char* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndNativeUInt(char* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndNativeUInt()
    {
        char sourceValue = 'K';
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<char*, nuint*, char*> function = &ManagedPointerFunctionCharAndNativeUInt;
        delegate* unmanaged<char*, nuint*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and nuint* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndSingle(char* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndSingle(char* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndSingle()
    {
        char sourceValue = 'K';
        float destinationValue = 12.25f;
        delegate* managed<char*, float*, char*> function = &ManagedPointerFunctionCharAndSingle;
        delegate* unmanaged<char*, float*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and float* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndDouble(char* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndDouble(char* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndDouble()
    {
        char sourceValue = 'K';
        double destinationValue = -33.5;
        delegate* managed<char*, double*, char*> function = &ManagedPointerFunctionCharAndDouble;
        delegate* unmanaged<char*, double*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and double* parameters");
    }

    private static unsafe char* ManagedPointerFunctionCharAndBoolean(char* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe char* UnmanagedPointerFunctionCharAndBoolean(char* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionCharAndBoolean()
    {
        char sourceValue = 'K';
        bool destinationValue = true;
        delegate* managed<char*, bool*, char*> function = &ManagedPointerFunctionCharAndBoolean;
        delegate* unmanaged<char*, bool*, char*> unmanagedFunction = &UnmanagedPointerFunctionCharAndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with char* and bool* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndSByte(int* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndSByte(int* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndSByte()
    {
        int sourceValue = -1234567;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<int*, sbyte*, int*> function = &ManagedPointerFunctionInt32AndSByte;
        delegate* unmanaged<int*, sbyte*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and sbyte* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndByte(int* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndByte(int* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndByte()
    {
        int sourceValue = -1234567;
        byte destinationValue = (byte)211;
        delegate* managed<int*, byte*, int*> function = &ManagedPointerFunctionInt32AndByte;
        delegate* unmanaged<int*, byte*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and byte* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndInt16(int* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndInt16(int* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndInt16()
    {
        int sourceValue = -1234567;
        short destinationValue = (short)-1234;
        delegate* managed<int*, short*, int*> function = &ManagedPointerFunctionInt32AndInt16;
        delegate* unmanaged<int*, short*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and short* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndUInt16(int* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndUInt16(int* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndUInt16()
    {
        int sourceValue = -1234567;
        ushort destinationValue = (ushort)54321;
        delegate* managed<int*, ushort*, int*> function = &ManagedPointerFunctionInt32AndUInt16;
        delegate* unmanaged<int*, ushort*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and ushort* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndChar(int* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndChar(int* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndChar()
    {
        int sourceValue = -1234567;
        char destinationValue = 'K';
        delegate* managed<int*, char*, int*> function = &ManagedPointerFunctionInt32AndChar;
        delegate* unmanaged<int*, char*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and char* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndInt32(int* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndInt32(int* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndInt32()
    {
        int sourceValue = -1234567;
        int destinationValue = -1234567;
        delegate* managed<int*, int*, int*> function = &ManagedPointerFunctionInt32AndInt32;
        delegate* unmanaged<int*, int*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and int* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndUInt32(int* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndUInt32(int* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndUInt32()
    {
        int sourceValue = -1234567;
        uint destinationValue = 3456789012u;
        delegate* managed<int*, uint*, int*> function = &ManagedPointerFunctionInt32AndUInt32;
        delegate* unmanaged<int*, uint*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and uint* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndInt64(int* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndInt64(int* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndInt64()
    {
        int sourceValue = -1234567;
        long destinationValue = -1234567890123L;
        delegate* managed<int*, long*, int*> function = &ManagedPointerFunctionInt32AndInt64;
        delegate* unmanaged<int*, long*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and long* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndUInt64(int* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndUInt64(int* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndUInt64()
    {
        int sourceValue = -1234567;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<int*, ulong*, int*> function = &ManagedPointerFunctionInt32AndUInt64;
        delegate* unmanaged<int*, ulong*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and ulong* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndNativeInt(int* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndNativeInt(int* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndNativeInt()
    {
        int sourceValue = -1234567;
        nint destinationValue = (nint)0x123456;
        delegate* managed<int*, nint*, int*> function = &ManagedPointerFunctionInt32AndNativeInt;
        delegate* unmanaged<int*, nint*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and nint* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndNativeUInt(int* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndNativeUInt(int* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndNativeUInt()
    {
        int sourceValue = -1234567;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<int*, nuint*, int*> function = &ManagedPointerFunctionInt32AndNativeUInt;
        delegate* unmanaged<int*, nuint*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and nuint* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndSingle(int* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndSingle(int* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndSingle()
    {
        int sourceValue = -1234567;
        float destinationValue = 12.25f;
        delegate* managed<int*, float*, int*> function = &ManagedPointerFunctionInt32AndSingle;
        delegate* unmanaged<int*, float*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and float* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndDouble(int* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndDouble(int* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndDouble()
    {
        int sourceValue = -1234567;
        double destinationValue = -33.5;
        delegate* managed<int*, double*, int*> function = &ManagedPointerFunctionInt32AndDouble;
        delegate* unmanaged<int*, double*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and double* parameters");
    }

    private static unsafe int* ManagedPointerFunctionInt32AndBoolean(int* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe int* UnmanagedPointerFunctionInt32AndBoolean(int* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt32AndBoolean()
    {
        int sourceValue = -1234567;
        bool destinationValue = true;
        delegate* managed<int*, bool*, int*> function = &ManagedPointerFunctionInt32AndBoolean;
        delegate* unmanaged<int*, bool*, int*> unmanagedFunction = &UnmanagedPointerFunctionInt32AndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with int* and bool* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndSByte(uint* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndSByte(uint* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndSByte()
    {
        uint sourceValue = 3456789012u;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<uint*, sbyte*, uint*> function = &ManagedPointerFunctionUInt32AndSByte;
        delegate* unmanaged<uint*, sbyte*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and sbyte* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndByte(uint* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndByte(uint* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndByte()
    {
        uint sourceValue = 3456789012u;
        byte destinationValue = (byte)211;
        delegate* managed<uint*, byte*, uint*> function = &ManagedPointerFunctionUInt32AndByte;
        delegate* unmanaged<uint*, byte*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and byte* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndInt16(uint* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndInt16(uint* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndInt16()
    {
        uint sourceValue = 3456789012u;
        short destinationValue = (short)-1234;
        delegate* managed<uint*, short*, uint*> function = &ManagedPointerFunctionUInt32AndInt16;
        delegate* unmanaged<uint*, short*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and short* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndUInt16(uint* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndUInt16(uint* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndUInt16()
    {
        uint sourceValue = 3456789012u;
        ushort destinationValue = (ushort)54321;
        delegate* managed<uint*, ushort*, uint*> function = &ManagedPointerFunctionUInt32AndUInt16;
        delegate* unmanaged<uint*, ushort*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and ushort* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndChar(uint* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndChar(uint* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndChar()
    {
        uint sourceValue = 3456789012u;
        char destinationValue = 'K';
        delegate* managed<uint*, char*, uint*> function = &ManagedPointerFunctionUInt32AndChar;
        delegate* unmanaged<uint*, char*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and char* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndInt32(uint* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndInt32(uint* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndInt32()
    {
        uint sourceValue = 3456789012u;
        int destinationValue = -1234567;
        delegate* managed<uint*, int*, uint*> function = &ManagedPointerFunctionUInt32AndInt32;
        delegate* unmanaged<uint*, int*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and int* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndUInt32(uint* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndUInt32(uint* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndUInt32()
    {
        uint sourceValue = 3456789012u;
        uint destinationValue = 3456789012u;
        delegate* managed<uint*, uint*, uint*> function = &ManagedPointerFunctionUInt32AndUInt32;
        delegate* unmanaged<uint*, uint*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and uint* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndInt64(uint* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndInt64(uint* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndInt64()
    {
        uint sourceValue = 3456789012u;
        long destinationValue = -1234567890123L;
        delegate* managed<uint*, long*, uint*> function = &ManagedPointerFunctionUInt32AndInt64;
        delegate* unmanaged<uint*, long*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and long* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndUInt64(uint* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndUInt64(uint* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndUInt64()
    {
        uint sourceValue = 3456789012u;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<uint*, ulong*, uint*> function = &ManagedPointerFunctionUInt32AndUInt64;
        delegate* unmanaged<uint*, ulong*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and ulong* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndNativeInt(uint* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndNativeInt(uint* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndNativeInt()
    {
        uint sourceValue = 3456789012u;
        nint destinationValue = (nint)0x123456;
        delegate* managed<uint*, nint*, uint*> function = &ManagedPointerFunctionUInt32AndNativeInt;
        delegate* unmanaged<uint*, nint*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and nint* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndNativeUInt(uint* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndNativeUInt(uint* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndNativeUInt()
    {
        uint sourceValue = 3456789012u;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<uint*, nuint*, uint*> function = &ManagedPointerFunctionUInt32AndNativeUInt;
        delegate* unmanaged<uint*, nuint*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and nuint* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndSingle(uint* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndSingle(uint* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndSingle()
    {
        uint sourceValue = 3456789012u;
        float destinationValue = 12.25f;
        delegate* managed<uint*, float*, uint*> function = &ManagedPointerFunctionUInt32AndSingle;
        delegate* unmanaged<uint*, float*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and float* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndDouble(uint* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndDouble(uint* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndDouble()
    {
        uint sourceValue = 3456789012u;
        double destinationValue = -33.5;
        delegate* managed<uint*, double*, uint*> function = &ManagedPointerFunctionUInt32AndDouble;
        delegate* unmanaged<uint*, double*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and double* parameters");
    }

    private static unsafe uint* ManagedPointerFunctionUInt32AndBoolean(uint* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe uint* UnmanagedPointerFunctionUInt32AndBoolean(uint* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt32AndBoolean()
    {
        uint sourceValue = 3456789012u;
        bool destinationValue = true;
        delegate* managed<uint*, bool*, uint*> function = &ManagedPointerFunctionUInt32AndBoolean;
        delegate* unmanaged<uint*, bool*, uint*> unmanagedFunction = &UnmanagedPointerFunctionUInt32AndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with uint* and bool* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndSByte(long* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndSByte(long* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndSByte()
    {
        long sourceValue = -1234567890123L;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<long*, sbyte*, long*> function = &ManagedPointerFunctionInt64AndSByte;
        delegate* unmanaged<long*, sbyte*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and sbyte* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndByte(long* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndByte(long* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndByte()
    {
        long sourceValue = -1234567890123L;
        byte destinationValue = (byte)211;
        delegate* managed<long*, byte*, long*> function = &ManagedPointerFunctionInt64AndByte;
        delegate* unmanaged<long*, byte*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and byte* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndInt16(long* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndInt16(long* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndInt16()
    {
        long sourceValue = -1234567890123L;
        short destinationValue = (short)-1234;
        delegate* managed<long*, short*, long*> function = &ManagedPointerFunctionInt64AndInt16;
        delegate* unmanaged<long*, short*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and short* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndUInt16(long* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndUInt16(long* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndUInt16()
    {
        long sourceValue = -1234567890123L;
        ushort destinationValue = (ushort)54321;
        delegate* managed<long*, ushort*, long*> function = &ManagedPointerFunctionInt64AndUInt16;
        delegate* unmanaged<long*, ushort*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and ushort* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndChar(long* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndChar(long* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndChar()
    {
        long sourceValue = -1234567890123L;
        char destinationValue = 'K';
        delegate* managed<long*, char*, long*> function = &ManagedPointerFunctionInt64AndChar;
        delegate* unmanaged<long*, char*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and char* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndInt32(long* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndInt32(long* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndInt32()
    {
        long sourceValue = -1234567890123L;
        int destinationValue = -1234567;
        delegate* managed<long*, int*, long*> function = &ManagedPointerFunctionInt64AndInt32;
        delegate* unmanaged<long*, int*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and int* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndUInt32(long* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndUInt32(long* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndUInt32()
    {
        long sourceValue = -1234567890123L;
        uint destinationValue = 3456789012u;
        delegate* managed<long*, uint*, long*> function = &ManagedPointerFunctionInt64AndUInt32;
        delegate* unmanaged<long*, uint*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and uint* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndInt64(long* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndInt64(long* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndInt64()
    {
        long sourceValue = -1234567890123L;
        long destinationValue = -1234567890123L;
        delegate* managed<long*, long*, long*> function = &ManagedPointerFunctionInt64AndInt64;
        delegate* unmanaged<long*, long*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and long* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndUInt64(long* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndUInt64(long* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndUInt64()
    {
        long sourceValue = -1234567890123L;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<long*, ulong*, long*> function = &ManagedPointerFunctionInt64AndUInt64;
        delegate* unmanaged<long*, ulong*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and ulong* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndNativeInt(long* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndNativeInt(long* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndNativeInt()
    {
        long sourceValue = -1234567890123L;
        nint destinationValue = (nint)0x123456;
        delegate* managed<long*, nint*, long*> function = &ManagedPointerFunctionInt64AndNativeInt;
        delegate* unmanaged<long*, nint*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and nint* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndNativeUInt(long* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndNativeUInt(long* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndNativeUInt()
    {
        long sourceValue = -1234567890123L;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<long*, nuint*, long*> function = &ManagedPointerFunctionInt64AndNativeUInt;
        delegate* unmanaged<long*, nuint*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and nuint* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndSingle(long* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndSingle(long* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndSingle()
    {
        long sourceValue = -1234567890123L;
        float destinationValue = 12.25f;
        delegate* managed<long*, float*, long*> function = &ManagedPointerFunctionInt64AndSingle;
        delegate* unmanaged<long*, float*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and float* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndDouble(long* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndDouble(long* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndDouble()
    {
        long sourceValue = -1234567890123L;
        double destinationValue = -33.5;
        delegate* managed<long*, double*, long*> function = &ManagedPointerFunctionInt64AndDouble;
        delegate* unmanaged<long*, double*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and double* parameters");
    }

    private static unsafe long* ManagedPointerFunctionInt64AndBoolean(long* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe long* UnmanagedPointerFunctionInt64AndBoolean(long* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionInt64AndBoolean()
    {
        long sourceValue = -1234567890123L;
        bool destinationValue = true;
        delegate* managed<long*, bool*, long*> function = &ManagedPointerFunctionInt64AndBoolean;
        delegate* unmanaged<long*, bool*, long*> unmanagedFunction = &UnmanagedPointerFunctionInt64AndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with long* and bool* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndSByte(ulong* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndSByte(ulong* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndSByte()
    {
        ulong sourceValue = 12345678901234567890UL;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<ulong*, sbyte*, ulong*> function = &ManagedPointerFunctionUInt64AndSByte;
        delegate* unmanaged<ulong*, sbyte*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and sbyte* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndByte(ulong* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndByte(ulong* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndByte()
    {
        ulong sourceValue = 12345678901234567890UL;
        byte destinationValue = (byte)211;
        delegate* managed<ulong*, byte*, ulong*> function = &ManagedPointerFunctionUInt64AndByte;
        delegate* unmanaged<ulong*, byte*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and byte* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndInt16(ulong* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndInt16(ulong* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndInt16()
    {
        ulong sourceValue = 12345678901234567890UL;
        short destinationValue = (short)-1234;
        delegate* managed<ulong*, short*, ulong*> function = &ManagedPointerFunctionUInt64AndInt16;
        delegate* unmanaged<ulong*, short*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and short* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndUInt16(ulong* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndUInt16(ulong* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndUInt16()
    {
        ulong sourceValue = 12345678901234567890UL;
        ushort destinationValue = (ushort)54321;
        delegate* managed<ulong*, ushort*, ulong*> function = &ManagedPointerFunctionUInt64AndUInt16;
        delegate* unmanaged<ulong*, ushort*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and ushort* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndChar(ulong* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndChar(ulong* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndChar()
    {
        ulong sourceValue = 12345678901234567890UL;
        char destinationValue = 'K';
        delegate* managed<ulong*, char*, ulong*> function = &ManagedPointerFunctionUInt64AndChar;
        delegate* unmanaged<ulong*, char*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and char* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndInt32(ulong* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndInt32(ulong* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndInt32()
    {
        ulong sourceValue = 12345678901234567890UL;
        int destinationValue = -1234567;
        delegate* managed<ulong*, int*, ulong*> function = &ManagedPointerFunctionUInt64AndInt32;
        delegate* unmanaged<ulong*, int*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and int* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndUInt32(ulong* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndUInt32(ulong* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndUInt32()
    {
        ulong sourceValue = 12345678901234567890UL;
        uint destinationValue = 3456789012u;
        delegate* managed<ulong*, uint*, ulong*> function = &ManagedPointerFunctionUInt64AndUInt32;
        delegate* unmanaged<ulong*, uint*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and uint* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndInt64(ulong* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndInt64(ulong* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndInt64()
    {
        ulong sourceValue = 12345678901234567890UL;
        long destinationValue = -1234567890123L;
        delegate* managed<ulong*, long*, ulong*> function = &ManagedPointerFunctionUInt64AndInt64;
        delegate* unmanaged<ulong*, long*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and long* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndUInt64(ulong* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndUInt64(ulong* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndUInt64()
    {
        ulong sourceValue = 12345678901234567890UL;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<ulong*, ulong*, ulong*> function = &ManagedPointerFunctionUInt64AndUInt64;
        delegate* unmanaged<ulong*, ulong*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and ulong* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndNativeInt(ulong* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndNativeInt(ulong* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndNativeInt()
    {
        ulong sourceValue = 12345678901234567890UL;
        nint destinationValue = (nint)0x123456;
        delegate* managed<ulong*, nint*, ulong*> function = &ManagedPointerFunctionUInt64AndNativeInt;
        delegate* unmanaged<ulong*, nint*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and nint* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndNativeUInt(ulong* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndNativeUInt(ulong* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndNativeUInt()
    {
        ulong sourceValue = 12345678901234567890UL;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<ulong*, nuint*, ulong*> function = &ManagedPointerFunctionUInt64AndNativeUInt;
        delegate* unmanaged<ulong*, nuint*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and nuint* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndSingle(ulong* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndSingle(ulong* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndSingle()
    {
        ulong sourceValue = 12345678901234567890UL;
        float destinationValue = 12.25f;
        delegate* managed<ulong*, float*, ulong*> function = &ManagedPointerFunctionUInt64AndSingle;
        delegate* unmanaged<ulong*, float*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and float* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndDouble(ulong* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndDouble(ulong* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndDouble()
    {
        ulong sourceValue = 12345678901234567890UL;
        double destinationValue = -33.5;
        delegate* managed<ulong*, double*, ulong*> function = &ManagedPointerFunctionUInt64AndDouble;
        delegate* unmanaged<ulong*, double*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and double* parameters");
    }

    private static unsafe ulong* ManagedPointerFunctionUInt64AndBoolean(ulong* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe ulong* UnmanagedPointerFunctionUInt64AndBoolean(ulong* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionUInt64AndBoolean()
    {
        ulong sourceValue = 12345678901234567890UL;
        bool destinationValue = true;
        delegate* managed<ulong*, bool*, ulong*> function = &ManagedPointerFunctionUInt64AndBoolean;
        delegate* unmanaged<ulong*, bool*, ulong*> unmanagedFunction = &UnmanagedPointerFunctionUInt64AndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with ulong* and bool* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndSByte(nint* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndSByte(nint* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndSByte()
    {
        nint sourceValue = (nint)0x123456;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<nint*, sbyte*, nint*> function = &ManagedPointerFunctionNativeIntAndSByte;
        delegate* unmanaged<nint*, sbyte*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and sbyte* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndByte(nint* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndByte(nint* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndByte()
    {
        nint sourceValue = (nint)0x123456;
        byte destinationValue = (byte)211;
        delegate* managed<nint*, byte*, nint*> function = &ManagedPointerFunctionNativeIntAndByte;
        delegate* unmanaged<nint*, byte*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and byte* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndInt16(nint* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndInt16(nint* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndInt16()
    {
        nint sourceValue = (nint)0x123456;
        short destinationValue = (short)-1234;
        delegate* managed<nint*, short*, nint*> function = &ManagedPointerFunctionNativeIntAndInt16;
        delegate* unmanaged<nint*, short*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and short* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndUInt16(nint* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndUInt16(nint* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndUInt16()
    {
        nint sourceValue = (nint)0x123456;
        ushort destinationValue = (ushort)54321;
        delegate* managed<nint*, ushort*, nint*> function = &ManagedPointerFunctionNativeIntAndUInt16;
        delegate* unmanaged<nint*, ushort*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and ushort* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndChar(nint* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndChar(nint* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndChar()
    {
        nint sourceValue = (nint)0x123456;
        char destinationValue = 'K';
        delegate* managed<nint*, char*, nint*> function = &ManagedPointerFunctionNativeIntAndChar;
        delegate* unmanaged<nint*, char*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and char* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndInt32(nint* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndInt32(nint* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndInt32()
    {
        nint sourceValue = (nint)0x123456;
        int destinationValue = -1234567;
        delegate* managed<nint*, int*, nint*> function = &ManagedPointerFunctionNativeIntAndInt32;
        delegate* unmanaged<nint*, int*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and int* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndUInt32(nint* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndUInt32(nint* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndUInt32()
    {
        nint sourceValue = (nint)0x123456;
        uint destinationValue = 3456789012u;
        delegate* managed<nint*, uint*, nint*> function = &ManagedPointerFunctionNativeIntAndUInt32;
        delegate* unmanaged<nint*, uint*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and uint* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndInt64(nint* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndInt64(nint* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndInt64()
    {
        nint sourceValue = (nint)0x123456;
        long destinationValue = -1234567890123L;
        delegate* managed<nint*, long*, nint*> function = &ManagedPointerFunctionNativeIntAndInt64;
        delegate* unmanaged<nint*, long*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and long* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndUInt64(nint* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndUInt64(nint* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndUInt64()
    {
        nint sourceValue = (nint)0x123456;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<nint*, ulong*, nint*> function = &ManagedPointerFunctionNativeIntAndUInt64;
        delegate* unmanaged<nint*, ulong*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and ulong* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndNativeInt(nint* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndNativeInt(nint* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndNativeInt()
    {
        nint sourceValue = (nint)0x123456;
        nint destinationValue = (nint)0x123456;
        delegate* managed<nint*, nint*, nint*> function = &ManagedPointerFunctionNativeIntAndNativeInt;
        delegate* unmanaged<nint*, nint*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and nint* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndNativeUInt(nint* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndNativeUInt(nint* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndNativeUInt()
    {
        nint sourceValue = (nint)0x123456;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<nint*, nuint*, nint*> function = &ManagedPointerFunctionNativeIntAndNativeUInt;
        delegate* unmanaged<nint*, nuint*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and nuint* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndSingle(nint* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndSingle(nint* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndSingle()
    {
        nint sourceValue = (nint)0x123456;
        float destinationValue = 12.25f;
        delegate* managed<nint*, float*, nint*> function = &ManagedPointerFunctionNativeIntAndSingle;
        delegate* unmanaged<nint*, float*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and float* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndDouble(nint* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndDouble(nint* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndDouble()
    {
        nint sourceValue = (nint)0x123456;
        double destinationValue = -33.5;
        delegate* managed<nint*, double*, nint*> function = &ManagedPointerFunctionNativeIntAndDouble;
        delegate* unmanaged<nint*, double*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and double* parameters");
    }

    private static unsafe nint* ManagedPointerFunctionNativeIntAndBoolean(nint* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nint* UnmanagedPointerFunctionNativeIntAndBoolean(nint* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeIntAndBoolean()
    {
        nint sourceValue = (nint)0x123456;
        bool destinationValue = true;
        delegate* managed<nint*, bool*, nint*> function = &ManagedPointerFunctionNativeIntAndBoolean;
        delegate* unmanaged<nint*, bool*, nint*> unmanagedFunction = &UnmanagedPointerFunctionNativeIntAndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nint* and bool* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndSByte(nuint* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndSByte(nuint* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndSByte()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<nuint*, sbyte*, nuint*> function = &ManagedPointerFunctionNativeUIntAndSByte;
        delegate* unmanaged<nuint*, sbyte*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and sbyte* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndByte(nuint* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndByte(nuint* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndByte()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        byte destinationValue = (byte)211;
        delegate* managed<nuint*, byte*, nuint*> function = &ManagedPointerFunctionNativeUIntAndByte;
        delegate* unmanaged<nuint*, byte*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and byte* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndInt16(nuint* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndInt16(nuint* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndInt16()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        short destinationValue = (short)-1234;
        delegate* managed<nuint*, short*, nuint*> function = &ManagedPointerFunctionNativeUIntAndInt16;
        delegate* unmanaged<nuint*, short*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and short* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndUInt16(nuint* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndUInt16(nuint* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndUInt16()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        ushort destinationValue = (ushort)54321;
        delegate* managed<nuint*, ushort*, nuint*> function = &ManagedPointerFunctionNativeUIntAndUInt16;
        delegate* unmanaged<nuint*, ushort*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and ushort* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndChar(nuint* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndChar(nuint* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndChar()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        char destinationValue = 'K';
        delegate* managed<nuint*, char*, nuint*> function = &ManagedPointerFunctionNativeUIntAndChar;
        delegate* unmanaged<nuint*, char*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and char* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndInt32(nuint* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndInt32(nuint* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndInt32()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        int destinationValue = -1234567;
        delegate* managed<nuint*, int*, nuint*> function = &ManagedPointerFunctionNativeUIntAndInt32;
        delegate* unmanaged<nuint*, int*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and int* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndUInt32(nuint* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndUInt32(nuint* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndUInt32()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        uint destinationValue = 3456789012u;
        delegate* managed<nuint*, uint*, nuint*> function = &ManagedPointerFunctionNativeUIntAndUInt32;
        delegate* unmanaged<nuint*, uint*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and uint* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndInt64(nuint* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndInt64(nuint* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndInt64()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        long destinationValue = -1234567890123L;
        delegate* managed<nuint*, long*, nuint*> function = &ManagedPointerFunctionNativeUIntAndInt64;
        delegate* unmanaged<nuint*, long*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and long* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndUInt64(nuint* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndUInt64(nuint* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndUInt64()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<nuint*, ulong*, nuint*> function = &ManagedPointerFunctionNativeUIntAndUInt64;
        delegate* unmanaged<nuint*, ulong*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and ulong* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndNativeInt(nuint* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndNativeInt(nuint* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndNativeInt()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        nint destinationValue = (nint)0x123456;
        delegate* managed<nuint*, nint*, nuint*> function = &ManagedPointerFunctionNativeUIntAndNativeInt;
        delegate* unmanaged<nuint*, nint*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and nint* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndNativeUInt(nuint* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndNativeUInt(nuint* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndNativeUInt()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<nuint*, nuint*, nuint*> function = &ManagedPointerFunctionNativeUIntAndNativeUInt;
        delegate* unmanaged<nuint*, nuint*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and nuint* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndSingle(nuint* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndSingle(nuint* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndSingle()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        float destinationValue = 12.25f;
        delegate* managed<nuint*, float*, nuint*> function = &ManagedPointerFunctionNativeUIntAndSingle;
        delegate* unmanaged<nuint*, float*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and float* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndDouble(nuint* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndDouble(nuint* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndDouble()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        double destinationValue = -33.5;
        delegate* managed<nuint*, double*, nuint*> function = &ManagedPointerFunctionNativeUIntAndDouble;
        delegate* unmanaged<nuint*, double*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and double* parameters");
    }

    private static unsafe nuint* ManagedPointerFunctionNativeUIntAndBoolean(nuint* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe nuint* UnmanagedPointerFunctionNativeUIntAndBoolean(nuint* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionNativeUIntAndBoolean()
    {
        nuint sourceValue = (nuint)0xabcdefu;
        bool destinationValue = true;
        delegate* managed<nuint*, bool*, nuint*> function = &ManagedPointerFunctionNativeUIntAndBoolean;
        delegate* unmanaged<nuint*, bool*, nuint*> unmanagedFunction = &UnmanagedPointerFunctionNativeUIntAndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with nuint* and bool* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndSByte(float* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndSByte(float* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndSByte()
    {
        float sourceValue = 12.25f;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<float*, sbyte*, float*> function = &ManagedPointerFunctionSingleAndSByte;
        delegate* unmanaged<float*, sbyte*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and sbyte* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndByte(float* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndByte(float* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndByte()
    {
        float sourceValue = 12.25f;
        byte destinationValue = (byte)211;
        delegate* managed<float*, byte*, float*> function = &ManagedPointerFunctionSingleAndByte;
        delegate* unmanaged<float*, byte*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and byte* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndInt16(float* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndInt16(float* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndInt16()
    {
        float sourceValue = 12.25f;
        short destinationValue = (short)-1234;
        delegate* managed<float*, short*, float*> function = &ManagedPointerFunctionSingleAndInt16;
        delegate* unmanaged<float*, short*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and short* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndUInt16(float* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndUInt16(float* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndUInt16()
    {
        float sourceValue = 12.25f;
        ushort destinationValue = (ushort)54321;
        delegate* managed<float*, ushort*, float*> function = &ManagedPointerFunctionSingleAndUInt16;
        delegate* unmanaged<float*, ushort*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and ushort* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndChar(float* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndChar(float* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndChar()
    {
        float sourceValue = 12.25f;
        char destinationValue = 'K';
        delegate* managed<float*, char*, float*> function = &ManagedPointerFunctionSingleAndChar;
        delegate* unmanaged<float*, char*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and char* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndInt32(float* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndInt32(float* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndInt32()
    {
        float sourceValue = 12.25f;
        int destinationValue = -1234567;
        delegate* managed<float*, int*, float*> function = &ManagedPointerFunctionSingleAndInt32;
        delegate* unmanaged<float*, int*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and int* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndUInt32(float* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndUInt32(float* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndUInt32()
    {
        float sourceValue = 12.25f;
        uint destinationValue = 3456789012u;
        delegate* managed<float*, uint*, float*> function = &ManagedPointerFunctionSingleAndUInt32;
        delegate* unmanaged<float*, uint*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and uint* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndInt64(float* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndInt64(float* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndInt64()
    {
        float sourceValue = 12.25f;
        long destinationValue = -1234567890123L;
        delegate* managed<float*, long*, float*> function = &ManagedPointerFunctionSingleAndInt64;
        delegate* unmanaged<float*, long*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and long* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndUInt64(float* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndUInt64(float* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndUInt64()
    {
        float sourceValue = 12.25f;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<float*, ulong*, float*> function = &ManagedPointerFunctionSingleAndUInt64;
        delegate* unmanaged<float*, ulong*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and ulong* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndNativeInt(float* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndNativeInt(float* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndNativeInt()
    {
        float sourceValue = 12.25f;
        nint destinationValue = (nint)0x123456;
        delegate* managed<float*, nint*, float*> function = &ManagedPointerFunctionSingleAndNativeInt;
        delegate* unmanaged<float*, nint*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and nint* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndNativeUInt(float* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndNativeUInt(float* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndNativeUInt()
    {
        float sourceValue = 12.25f;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<float*, nuint*, float*> function = &ManagedPointerFunctionSingleAndNativeUInt;
        delegate* unmanaged<float*, nuint*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and nuint* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndSingle(float* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndSingle(float* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndSingle()
    {
        float sourceValue = 12.25f;
        float destinationValue = 12.25f;
        delegate* managed<float*, float*, float*> function = &ManagedPointerFunctionSingleAndSingle;
        delegate* unmanaged<float*, float*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and float* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndDouble(float* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndDouble(float* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndDouble()
    {
        float sourceValue = 12.25f;
        double destinationValue = -33.5;
        delegate* managed<float*, double*, float*> function = &ManagedPointerFunctionSingleAndDouble;
        delegate* unmanaged<float*, double*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and double* parameters");
    }

    private static unsafe float* ManagedPointerFunctionSingleAndBoolean(float* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe float* UnmanagedPointerFunctionSingleAndBoolean(float* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionSingleAndBoolean()
    {
        float sourceValue = 12.25f;
        bool destinationValue = true;
        delegate* managed<float*, bool*, float*> function = &ManagedPointerFunctionSingleAndBoolean;
        delegate* unmanaged<float*, bool*, float*> unmanagedFunction = &UnmanagedPointerFunctionSingleAndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with float* and bool* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndSByte(double* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndSByte(double* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndSByte()
    {
        double sourceValue = -33.5;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<double*, sbyte*, double*> function = &ManagedPointerFunctionDoubleAndSByte;
        delegate* unmanaged<double*, sbyte*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and sbyte* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndByte(double* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndByte(double* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndByte()
    {
        double sourceValue = -33.5;
        byte destinationValue = (byte)211;
        delegate* managed<double*, byte*, double*> function = &ManagedPointerFunctionDoubleAndByte;
        delegate* unmanaged<double*, byte*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and byte* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndInt16(double* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndInt16(double* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndInt16()
    {
        double sourceValue = -33.5;
        short destinationValue = (short)-1234;
        delegate* managed<double*, short*, double*> function = &ManagedPointerFunctionDoubleAndInt16;
        delegate* unmanaged<double*, short*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and short* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndUInt16(double* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndUInt16(double* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndUInt16()
    {
        double sourceValue = -33.5;
        ushort destinationValue = (ushort)54321;
        delegate* managed<double*, ushort*, double*> function = &ManagedPointerFunctionDoubleAndUInt16;
        delegate* unmanaged<double*, ushort*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and ushort* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndChar(double* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndChar(double* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndChar()
    {
        double sourceValue = -33.5;
        char destinationValue = 'K';
        delegate* managed<double*, char*, double*> function = &ManagedPointerFunctionDoubleAndChar;
        delegate* unmanaged<double*, char*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and char* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndInt32(double* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndInt32(double* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndInt32()
    {
        double sourceValue = -33.5;
        int destinationValue = -1234567;
        delegate* managed<double*, int*, double*> function = &ManagedPointerFunctionDoubleAndInt32;
        delegate* unmanaged<double*, int*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and int* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndUInt32(double* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndUInt32(double* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndUInt32()
    {
        double sourceValue = -33.5;
        uint destinationValue = 3456789012u;
        delegate* managed<double*, uint*, double*> function = &ManagedPointerFunctionDoubleAndUInt32;
        delegate* unmanaged<double*, uint*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and uint* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndInt64(double* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndInt64(double* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndInt64()
    {
        double sourceValue = -33.5;
        long destinationValue = -1234567890123L;
        delegate* managed<double*, long*, double*> function = &ManagedPointerFunctionDoubleAndInt64;
        delegate* unmanaged<double*, long*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and long* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndUInt64(double* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndUInt64(double* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndUInt64()
    {
        double sourceValue = -33.5;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<double*, ulong*, double*> function = &ManagedPointerFunctionDoubleAndUInt64;
        delegate* unmanaged<double*, ulong*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and ulong* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndNativeInt(double* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndNativeInt(double* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndNativeInt()
    {
        double sourceValue = -33.5;
        nint destinationValue = (nint)0x123456;
        delegate* managed<double*, nint*, double*> function = &ManagedPointerFunctionDoubleAndNativeInt;
        delegate* unmanaged<double*, nint*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and nint* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndNativeUInt(double* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndNativeUInt(double* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndNativeUInt()
    {
        double sourceValue = -33.5;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<double*, nuint*, double*> function = &ManagedPointerFunctionDoubleAndNativeUInt;
        delegate* unmanaged<double*, nuint*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and nuint* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndSingle(double* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndSingle(double* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndSingle()
    {
        double sourceValue = -33.5;
        float destinationValue = 12.25f;
        delegate* managed<double*, float*, double*> function = &ManagedPointerFunctionDoubleAndSingle;
        delegate* unmanaged<double*, float*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and float* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndDouble(double* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndDouble(double* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndDouble()
    {
        double sourceValue = -33.5;
        double destinationValue = -33.5;
        delegate* managed<double*, double*, double*> function = &ManagedPointerFunctionDoubleAndDouble;
        delegate* unmanaged<double*, double*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and double* parameters");
    }

    private static unsafe double* ManagedPointerFunctionDoubleAndBoolean(double* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe double* UnmanagedPointerFunctionDoubleAndBoolean(double* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionDoubleAndBoolean()
    {
        double sourceValue = -33.5;
        bool destinationValue = true;
        delegate* managed<double*, bool*, double*> function = &ManagedPointerFunctionDoubleAndBoolean;
        delegate* unmanaged<double*, bool*, double*> unmanagedFunction = &UnmanagedPointerFunctionDoubleAndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with double* and bool* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndSByte(bool* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndSByte(bool* source, sbyte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndSByte()
    {
        bool sourceValue = true;
        sbyte destinationValue = (sbyte)-11;
        delegate* managed<bool*, sbyte*, bool*> function = &ManagedPointerFunctionBooleanAndSByte;
        delegate* unmanaged<bool*, sbyte*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndSByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and sbyte* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndByte(bool* source, byte* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndByte(bool* source, byte* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndByte()
    {
        bool sourceValue = true;
        byte destinationValue = (byte)211;
        delegate* managed<bool*, byte*, bool*> function = &ManagedPointerFunctionBooleanAndByte;
        delegate* unmanaged<bool*, byte*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndByte;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and byte* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndInt16(bool* source, short* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndInt16(bool* source, short* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndInt16()
    {
        bool sourceValue = true;
        short destinationValue = (short)-1234;
        delegate* managed<bool*, short*, bool*> function = &ManagedPointerFunctionBooleanAndInt16;
        delegate* unmanaged<bool*, short*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and short* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndUInt16(bool* source, ushort* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndUInt16(bool* source, ushort* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndUInt16()
    {
        bool sourceValue = true;
        ushort destinationValue = (ushort)54321;
        delegate* managed<bool*, ushort*, bool*> function = &ManagedPointerFunctionBooleanAndUInt16;
        delegate* unmanaged<bool*, ushort*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndUInt16;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and ushort* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndChar(bool* source, char* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndChar(bool* source, char* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndChar()
    {
        bool sourceValue = true;
        char destinationValue = 'K';
        delegate* managed<bool*, char*, bool*> function = &ManagedPointerFunctionBooleanAndChar;
        delegate* unmanaged<bool*, char*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndChar;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and char* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndInt32(bool* source, int* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndInt32(bool* source, int* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndInt32()
    {
        bool sourceValue = true;
        int destinationValue = -1234567;
        delegate* managed<bool*, int*, bool*> function = &ManagedPointerFunctionBooleanAndInt32;
        delegate* unmanaged<bool*, int*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and int* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndUInt32(bool* source, uint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndUInt32(bool* source, uint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndUInt32()
    {
        bool sourceValue = true;
        uint destinationValue = 3456789012u;
        delegate* managed<bool*, uint*, bool*> function = &ManagedPointerFunctionBooleanAndUInt32;
        delegate* unmanaged<bool*, uint*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndUInt32;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and uint* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndInt64(bool* source, long* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndInt64(bool* source, long* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndInt64()
    {
        bool sourceValue = true;
        long destinationValue = -1234567890123L;
        delegate* managed<bool*, long*, bool*> function = &ManagedPointerFunctionBooleanAndInt64;
        delegate* unmanaged<bool*, long*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and long* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndUInt64(bool* source, ulong* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndUInt64(bool* source, ulong* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndUInt64()
    {
        bool sourceValue = true;
        ulong destinationValue = 12345678901234567890UL;
        delegate* managed<bool*, ulong*, bool*> function = &ManagedPointerFunctionBooleanAndUInt64;
        delegate* unmanaged<bool*, ulong*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndUInt64;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and ulong* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndNativeInt(bool* source, nint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndNativeInt(bool* source, nint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndNativeInt()
    {
        bool sourceValue = true;
        nint destinationValue = (nint)0x123456;
        delegate* managed<bool*, nint*, bool*> function = &ManagedPointerFunctionBooleanAndNativeInt;
        delegate* unmanaged<bool*, nint*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndNativeInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and nint* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndNativeUInt(bool* source, nuint* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndNativeUInt(bool* source, nuint* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndNativeUInt()
    {
        bool sourceValue = true;
        nuint destinationValue = (nuint)0xabcdefu;
        delegate* managed<bool*, nuint*, bool*> function = &ManagedPointerFunctionBooleanAndNativeUInt;
        delegate* unmanaged<bool*, nuint*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndNativeUInt;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and nuint* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndSingle(bool* source, float* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndSingle(bool* source, float* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndSingle()
    {
        bool sourceValue = true;
        float destinationValue = 12.25f;
        delegate* managed<bool*, float*, bool*> function = &ManagedPointerFunctionBooleanAndSingle;
        delegate* unmanaged<bool*, float*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndSingle;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and float* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndDouble(bool* source, double* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndDouble(bool* source, double* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndDouble()
    {
        bool sourceValue = true;
        double destinationValue = -33.5;
        delegate* managed<bool*, double*, bool*> function = &ManagedPointerFunctionBooleanAndDouble;
        delegate* unmanaged<bool*, double*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndDouble;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and double* parameters");
    }

    private static unsafe bool* ManagedPointerFunctionBooleanAndBoolean(bool* source, bool* destination)
    {
        return destination == null ? null : source;
    }
    [UnmanagedCallersOnly]
    private static unsafe bool* UnmanagedPointerFunctionBooleanAndBoolean(bool* source, bool* destination)
    {
        return destination == null ? null : source;
    }

    private static unsafe void VerifyPointerFunctionBooleanAndBoolean()
    {
        bool sourceValue = true;
        bool destinationValue = true;
        delegate* managed<bool*, bool*, bool*> function = &ManagedPointerFunctionBooleanAndBoolean;
        delegate* unmanaged<bool*, bool*, bool*> unmanagedFunction = &UnmanagedPointerFunctionBooleanAndBoolean;
        if (function(&sourceValue, &destinationValue) != &sourceValue || function(&sourceValue, null) != null ||
            unmanagedFunction(&sourceValue, &destinationValue) != &sourceValue || unmanagedFunction(&sourceValue, null) != null)
            Fail("generated function pointer with bool* and bool* parameters");
    }
    // </generated-pointer-tests>
}
