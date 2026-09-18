using System;
using System.Collections.Generic;

internal static class GarbageCollectionValidation
{
    private sealed class Value
    {
        public int Number;
        public Value Link;
    }

    private class NodeBase
    {
        public Value BaseValue;
    }

    private sealed class Node : NodeBase
    {
        public Value Value;
        public Node Next;
        public Node Self;
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

    // A closed generic type is a reference type too. Keeping this as a static
    // root exercises the compiler's generic-reference classification directly.
    private sealed class GenericHolder<T>
    {
        public static Value StaticReference;
        public static ValueRoot StaticValue;
        public static Value[] StaticArray;
        public static List<Value> StaticList;
        public Value Instance;
    }

    private struct PairRoot
    {
        public Value First;
        public Value Second;
    }

    private static Value s_staticReferenceRoot;
    private static ValueRoot s_staticValueRoot;
    private static GenericHolder<Value> s_staticGenericHolder;
    private static List<Value> s_staticList;

    public static void Run()
    {
        ValidateStringListInitializer();
        ValidateDynamicStrings();

        s_staticReferenceRoot = new Value { Number = 8 };
        s_staticValueRoot = new ValueRoot { Value = new Value { Number = 12 } };
        GenericHolder<Value>.StaticReference = new Value { Number = 23 };
        GenericHolder<Value>.StaticValue = new ValueRoot { Value = new Value { Number = 24 } };
        GenericHolder<Value>.StaticArray = new Value[]
        {
            new Value { Number = 30 },
            null,
            new Value { Number = 31 }
        };
        GenericHolder<Value>.StaticList = new List<Value>
        {
            new Value { Number = 32 },
            new Value { Number = 33 }
        };
        s_staticGenericHolder = new GenericHolder<Value>
        {
            Instance = new Value { Number = 25 }
        };
        s_staticList = new List<Value>
        {
            new Value { Number = 26 },
            new Value { Number = 27 }
        };
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
        root.Self = root;
        root.Next.Next = root;
        root.Value.Link = root.Next.BaseValue;
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

        PairRoot pair = new PairRoot
        {
            First = new Value { Number = 34 },
            Second = new Value { Number = 35 }
        };
        Value[][] jaggedReferences = new Value[2][];
        jaggedReferences[0] = new Value[] { new Value { Number = 36 }, null };
        jaggedReferences[1] = new Value[] { new Value { Number = 37 }, new Value { Number = 38 } };
        Node[] objectArray = new Node[2];
        objectArray[0] = root;
        objectArray[1] = root.Next;

        Value[] nullableReferences = new Value[4];
        nullableReferences[1] = new Value { Number = 28 };
        nullableReferences[3] = new Value { Number = 29 };

        for (int index = 0; index < 16; index++)
        {
            AllocateGarbageBatch(index, 256);
            GC.Collect();
        }

        GC.Collect();

        Ensure(s_staticReferenceRoot.Number == 8, "static reference root");
        Ensure(s_staticValueRoot.Value.Number == 12, "static value root");
        Ensure(root.BaseValue.Number == 9 && root.Value.Number == 9, "base and derived fields");
        Ensure(root.Next.BaseValue.Number == 10 && root.Next.Value.Number == 11, "object graph");
        Ensure(root.Self == root && root.Next.Next == root, "cyclic object graph");
        Ensure(root.Value.Link == root.Next.BaseValue, "cross-linked object graph");
        Ensure(value.Value.Number == 13, "value root");
        Ensure(nested.Nested.Value.Number == 14 && nested.Direct.Number == 15, "nested value root");
        Ensure(referenceValues[0].Number == 16 && referenceValues[1].Number == 17, "reference array");
        Ensure(valueRoots[0].Value.Number == 13 && valueRoots[1].Value.Number == 18, "value array");
        Ensure(nestedValueRoots[0].Nested.Value.Number == 14 && nestedValueRoots[0].Direct.Number == 15,
            "nested value array");
        Ensure(matrix[0, 0].Value.Number == 19 && matrix[0, 1].Value.Number == 20 &&
            matrix[1, 0].Value.Number == 21 && matrix[1, 1].Value.Number == 22, "multidimensional value array");
        Ensure(nullableReferences[0] == null && nullableReferences[1].Number == 28 &&
            nullableReferences[2] == null && nullableReferences[3].Number == 29, "null reference array entries");
        Ensure(GenericHolder<Value>.StaticReference.Number == 23, "generic static reference root");
        Ensure(GenericHolder<Value>.StaticValue.Value.Number == 24, "generic static value root");
        Ensure(GenericHolder<Value>.StaticArray[0].Number == 30 &&
            GenericHolder<Value>.StaticArray[1] == null &&
            GenericHolder<Value>.StaticArray[2].Number == 31, "generic static reference array root");
        Ensure(GenericHolder<Value>.StaticList[0].Number == 32 &&
            GenericHolder<Value>.StaticList[1].Number == 33, "generic static collection root");
        Ensure(s_staticGenericHolder.Instance.Number == 25, "generic object static root");
        Ensure(s_staticList[0].Number == 26 && s_staticList[1].Number == 27, "generic collection static root");
        Ensure(pair.First.Number == 34 && pair.Second.Number == 35, "multi-reference value root");
        Ensure(jaggedReferences[0][0].Number == 36 && jaggedReferences[0][1] == null &&
            jaggedReferences[1][0].Number == 37 && jaggedReferences[1][1].Number == 38,
            "jagged reference array");
        Ensure(objectArray[0] == root && objectArray[1] == root.Next, "object reference array");

        s_staticReferenceRoot = null;
        s_staticValueRoot = default;
        GenericHolder<Value>.StaticReference = null;
        GenericHolder<Value>.StaticValue = default;
        GenericHolder<Value>.StaticArray = null;
        GenericHolder<Value>.StaticList = null;
        s_staticGenericHolder = null;
        s_staticList = null;
        GC.Collect();

        Ensure(root.BaseValue.Number == 9 && root.Next.Value.Number == 11, "object graph after static root removal");
        Ensure(root.Self == root && root.Next.Next == root && root.Value.Link == root.Next.BaseValue,
            "cyclic object graph after static root removal");
        Ensure(value.Value.Number == 13 && valueRoots[1].Value.Number == 18, "value roots after static root removal");
        Ensure(matrix[1, 1].Value.Number == 22, "multidimensional root after static root removal");
        Ensure(nullableReferences[1].Number == 28 && nullableReferences[3].Number == 29,
            "null reference array after static root removal");
        Ensure(pair.First.Number == 34 && pair.Second.Number == 35, "value root after static root removal");
        Ensure(jaggedReferences[1][0].Number == 37 && jaggedReferences[1][1].Number == 38,
            "jagged root after static root removal");
        Ensure(objectArray[0] == root && objectArray[1] == root.Next,
            "object array after static root removal");
        for (int index = 0; index < 16; index++)
        {
            AllocateGarbageBatch(100 + index, 256);
            GC.Collect();
        }
        GC.Collect();
        Ensure(root.Self == root && root.Next.Next == root && root.Value.Link == root.Next.BaseValue,
            "cyclic object graph after repeated collection");
        Console.WriteLine("Garbage collection validation passed.");
    }

    private static void ValidateStringListInitializer()
    {
        List<string> values = new()
        {
            "string-list-value-0",
            "string-list-value-1-longer",
            "string-list-value-2",
            "string-list-value-3-longer",
            "string-list-value-4",
            "string-list-value-5-longer",
            "string-list-value-6",
            "string-list-value-7-longer",
            "string-list-value-8",
            "string-list-value-9-longer"
        };

        for (int pass = 0; pass < 8; pass++)
        {
            AllocateGarbageBatch(200 + pass, 256);
            GC.Collect();
        }

        Ensure(values.Count == 10, "string list count");
        Ensure(object.ReferenceEquals("string-list-value-4", "string-list-value-4"), "interned string literal");
        Ensure(values[0] == "string-list-value-0", "string list item 0");
        Ensure(values[1] == "string-list-value-1-longer", "string list item 1");
        Ensure(values[2] == "string-list-value-2", "string list item 2");
        Ensure(values[3] == "string-list-value-3-longer", "string list item 3");
        Ensure(values[4] == "string-list-value-4", "string list item 4");
        Ensure(values[5] == "string-list-value-5-longer", "string list item 5");
        Ensure(values[6] == "string-list-value-6", "string list item 6");
        Ensure(values[7] == "string-list-value-7-longer", "string list item 7");
        Ensure(values[8] == "string-list-value-8", "string list item 8");
        Ensure(values[9] == "string-list-value-9-longer", "string list item 9");
    }

    private static void ValidateDynamicStrings()
    {
        string single = CreateDynamicString(0);
        GC.Collect();
        ValidateDynamicString(single, 0, "dynamic string local");

        string[] array = new string[10];
        for (int index = 0; index < array.Length; index++)
            array[index] = CreateDynamicString(index);

        for (int pass = 0; pass < 8; pass++)
        {
            AllocateGarbageBatch(300 + pass, 256);
            GC.Collect();
        }

        for (int index = 0; index < array.Length; index++)
            ValidateDynamicString(array[index], index, "dynamic string array");

        List<string> values = new();
        for (int index = 0; index < 10; index++)
            values.Add(CreateDynamicString(index));

        for (int pass = 0; pass < 8; pass++)
        {
            AllocateGarbageBatch(400 + pass, 256);
            GC.Collect();
        }

        Ensure(values.Count == 10, "dynamic string list count");
        Ensure(!object.ReferenceEquals(values[0], "dynamic-0"), "dynamic string allocation");
        for (int index = 0; index < values.Count; index++)
            ValidateDynamicString(values[index], index, "dynamic string list");
    }

    private static string CreateDynamicString(int index)
    {
        char[] characters =
        [
            'd', 'y', 'n', 'a', 'm', 'i', 'c', '-', (char)('0' + index)
        ];
        return new string(characters);
    }

    private static void ValidateDynamicString(string value, int index, string name)
    {
        Ensure(value.Length == 9, name + " length");
        Ensure(value[0] == 'd', name + " first character");
        Ensure(value[7] == '-', name + " separator");
        Ensure(value[8] == (char)('0' + index), name + " final character");
    }

    private static void Ensure(bool condition, string name)
    {
        if (!condition)
            throw new Exception("Garbage collection validation failed: " + name);
    }

    private static void AllocateGarbageBatch(int seed, int count)
    {
        Value[] values = new Value[count];
        for (int index = 0; index < count; index++)
        {
            Value value = new Value { Number = seed * count + index };
            value.Link = new Value { Number = -value.Number };
            values[index] = value;
        }

        // Make a separate unreachable cycle in every batch. A tracing
        // collector must not keep it alive merely because it is cyclic.
        Value first = new Value { Number = seed };
        Value second = new Value { Number = -seed };
        first.Link = second;
        second.Link = first;
    }
}
