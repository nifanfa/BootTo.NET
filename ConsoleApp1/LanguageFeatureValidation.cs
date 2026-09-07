using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static partial class LanguageFeatureValidation
{
    private const int ExpectedSum = 15;
    internal const string FeatureName = "LanguageFeatureValidation";
    private static readonly object s_lock = new object();
    private static volatile int s_volatileValue;
    private static volatile int s_filterMode;
    private static volatile int s_expectedSum;
    private static volatile int s_runtimeBias = 1;
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
        VerifyNumericOperators();
        VerifyStrings();
        VerifyObjectAndGenericFeatures(values);
        VerifyInheritanceAndInterfaces();
        VerifyGenericFeatures();
        VerifyBoxing();
        VerifyStructures();
        VerifyLatestSyntax();
        VerifyModernLanguageFeatures(values);
        VerifyArrays();
        VerifyDelegatesAndLinq(values);
        VerifyControlFlow(values);
        VerifyReferencesAndPointers(values);
        VerifyExceptionsAndResources();
        VerifyAsync().GetAwaiter().GetResult();
        Console.WriteLine("Language feature validation passed.");
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
        if (intType == null || stringType == null || localType == null || validationType == null)
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
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int EvaluateBase(FeatureBase feature) => feature.Evaluate();

    private static int ReadGenericValue<T>(T value)
        where T : IFeatureValue
        => value.Value;

    private static T PreserveReference<T>(T value)
        where T : class
        => value;

    private static int CombineValues(int left, int right = 4, int multiplier = 1)
        => (left + right) * multiplier;

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

        if (!TryRead(values, 2, out int third))
            Fail("out parameter");

        int inValue = RuntimeValue(4);
        if (ReadIn(in inValue) != RuntimeValue(4) || first != RuntimeValue(2) ||
            third != RuntimeValue(3))
            Fail("ref or in parameter");

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
}
