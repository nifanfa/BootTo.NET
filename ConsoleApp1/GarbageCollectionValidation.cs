using System;

internal static class GarbageCollectionValidation
{
    private sealed class Value
    {
        public int Number;
    }

    private class NodeBase
    {
        public Value BaseValue;
    }

    private sealed class Node : NodeBase
    {
        public Value Value;
        public Node Next;
    }

    private struct ValueRoot
    {
        public Value Value;
    }

    private struct NestedValueRoot
    {
        public ValueRoot Nested;
        public Value Direct;
    }

    private static Value s_staticReferenceRoot;
    private static ValueRoot s_staticValueRoot;

    public static void Run()
    {
        s_staticReferenceRoot = new Value { Number = 8 };
        s_staticValueRoot = new ValueRoot { Value = new Value { Number = 12 } };
        Node root = new Node
        {
            BaseValue = new Value { Number = 9 },
            Value = new Value { Number = 9 },
            Next = new Node
            {
                BaseValue = new Value { Number = 10 },
                Value = new Value { Number = 11 }
            }
        };
        ValueRoot value = new ValueRoot { Value = new Value { Number = 13 } };
        NestedValueRoot nested = new NestedValueRoot
        {
            Nested = new ValueRoot { Value = new Value { Number = 14 } },
            Direct = new Value { Number = 15 }
        };
        Value[] referenceValues =
        [
            new Value { Number = 16 },
            new Value { Number = 17 }
        ];
        ValueRoot[] valueRoots = new ValueRoot[2];
        valueRoots[0] = value;
        valueRoots[1] = new ValueRoot { Value = new Value { Number = 18 } };
        NestedValueRoot[] nestedValueRoots = new NestedValueRoot[1];
        nestedValueRoots[0] = nested;
        ValueRoot[,] matrix = new ValueRoot[2, 2];
        matrix[0, 0] = new ValueRoot { Value = new Value { Number = 19 } };
        matrix[0, 1] = new ValueRoot { Value = new Value { Number = 20 } };
        matrix[1, 0] = new ValueRoot { Value = new Value { Number = 21 } };
        matrix[1, 1] = new ValueRoot { Value = new Value { Number = 22 } };

        GC.Collect();

        Ensure(s_staticReferenceRoot.Number == 8, "static reference root");
        Ensure(s_staticValueRoot.Value.Number == 12, "static value root");
        Ensure(root.BaseValue.Number == 9 && root.Value.Number == 9, "base and derived fields");
        Ensure(root.Next.BaseValue.Number == 10 && root.Next.Value.Number == 11, "object graph");
        Ensure(value.Value.Number == 13, "value root");
        Ensure(nested.Nested.Value.Number == 14 && nested.Direct.Number == 15, "nested value root");
        Ensure(referenceValues[0].Number == 16 && referenceValues[1].Number == 17, "reference array");
        Ensure(valueRoots[0].Value.Number == 13 && valueRoots[1].Value.Number == 18, "value array");
        Ensure(nestedValueRoots[0].Nested.Value.Number == 14 && nestedValueRoots[0].Direct.Number == 15,
            "nested value array");
        Ensure(matrix[0, 0].Value.Number == 19 && matrix[0, 1].Value.Number == 20 &&
            matrix[1, 0].Value.Number == 21 && matrix[1, 1].Value.Number == 22, "multidimensional value array");

        s_staticReferenceRoot = null;
        s_staticValueRoot = default;
        GC.Collect();

        Ensure(root.BaseValue.Number == 9 && root.Next.Value.Number == 11, "object graph after static root removal");
        Ensure(value.Value.Number == 13 && valueRoots[1].Value.Number == 18, "value roots after static root removal");
        Ensure(matrix[1, 1].Value.Number == 22, "multidimensional root after static root removal");
        Console.WriteLine("Garbage collection validation passed.");
    }

    private static void Ensure(bool condition, string name)
    {
        if (!condition)
            throw new Exception("Garbage collection validation failed: " + name);
    }
}
