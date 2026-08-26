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
        VerifyObjectAndGenericFeatures(values);
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

    private static void Increment(ref int value) => value++;

    private static int ReadIn(in int value) => value;

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
