#pragma warning disable
global using static NativeLib;
#pragma warning restore

using System;
using System.Runtime;
using System.Runtime.InteropServices;

internal unsafe class NativeLib
{
    [DllImport("*")]
    public static extern double MathSqrt(double value);

    [DllImport("*")]
    public static extern double MathAbs(double value);

    [DllImport("*")]
    public static extern double MathMax(double left, double right);

    [DllImport("*")]
    public static extern double MathMin(double left, double right);

    [DllImport("*")]
    public static extern float MathAbsSingle(float value);

    [DllImport("*")]
    public static extern float MathMaxSingle(float left, float right);

    [DllImport("*")]
    public static extern float MathMinSingle(float left, float right);

    [DllImport("*")]
    public static extern double MathFloor(double value);

    [DllImport("*")]
    public static extern double MathCeiling(double value);

    [DllImport("*")]
    public static extern double MathTruncate(double value);

    [DllImport("*")]
    public static extern double MathRound(double value);

    [DllImport("*")]
    public static extern int SupportRdrand();

    [DllImport("*")]
    public static extern int IsTcg();

    [DllImport("*")]
    public static extern int Rdrand64(out ulong value);

    [DllImport("*", EntryPoint = "vsnprintf_")]
    private static extern int vsnprintf(byte* buffer, int count, void* format, NativeVariableArgument* va);

    public static int snprintf(byte* buffer, int count, void* format, params VariableArgument[] va)
    {
        NativeVariableArgument[] arguments = GetNativeArguments(va);
        fixed (NativeVariableArgument* pointer = arguments)
            return vsnprintf(buffer, count, format, pointer);
    }

    public static int snprintf(byte* buffer, int count, ReadOnlySpan<byte> format, params VariableArgument[] va)
    {
        fixed (byte* pointer = format)
            return snprintf(buffer, count, pointer, va);
    }

    private static NativeVariableArgument[] GetNativeArguments(VariableArgument[] arguments)
    {
        NativeVariableArgument[] nativeArguments = new NativeVariableArgument[arguments.Length];
        for (int index = 0; index < arguments.Length; index++)
        {
            nativeArguments[index] = arguments[index].Value;
            byte[] buffer = arguments[index].Buffer;
            if (buffer != null)
            {
                fixed (byte* pointer = buffer)
                    nativeArguments[index].PointerValue = pointer;
            }
        }
        return nativeArguments;
    }

    [DllImport("*", EntryPoint = "printf_")]
    public static extern int printf(ByReference<byte> format, __arglist);

    static char lastCharacter;

    [RuntimeExport("_putchar")]
    public static void _putchar(char character)
    {
        if (character == '\n' && lastCharacter != '\r')
        {
            Console.Write('\r');
        }
        Console.Write(lastCharacter = character);
    }
}

internal unsafe struct VariableArgument
{
    internal NativeVariableArgument Value;
    internal byte[] Buffer;

    public static implicit operator VariableArgument(sbyte value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(short value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(int value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(long value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(byte value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(ushort value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(uint value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(ulong value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(float value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(double value) => new VariableArgument() { Value = value };
    public static implicit operator VariableArgument(byte[] value)
    {
        if (value == null)
            return default;
        byte[] buffer = new byte[value.Length + 1];
        for (int index = 0; index < value.Length; index++)
            buffer[index] = value[index];
        return new VariableArgument() { Buffer = buffer };
    }
}

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct NativeVariableArgument
{
    [FieldOffset(0)]
    public long SignedValue;
    [FieldOffset(0)]
    public ulong UnsignedValue;
    [FieldOffset(0)]
    public double FloatValue;
    [FieldOffset(0)]
    public void* PointerValue;

    public static implicit operator NativeVariableArgument(sbyte value) => new NativeVariableArgument() { SignedValue = value };
    public static implicit operator NativeVariableArgument(short value) => new NativeVariableArgument() { SignedValue = value };
    public static implicit operator NativeVariableArgument(int value) => new NativeVariableArgument() { SignedValue = value };
    public static implicit operator NativeVariableArgument(long value) => new NativeVariableArgument() { SignedValue = value };
    public static implicit operator NativeVariableArgument(byte value) => new NativeVariableArgument() { UnsignedValue = value };
    public static implicit operator NativeVariableArgument(ushort value) => new NativeVariableArgument() { UnsignedValue = value };
    public static implicit operator NativeVariableArgument(uint value) => new NativeVariableArgument() { UnsignedValue = value };
    public static implicit operator NativeVariableArgument(ulong value) => new NativeVariableArgument() { UnsignedValue = value };
    public static implicit operator NativeVariableArgument(float value) => new NativeVariableArgument() { FloatValue = value };
    public static implicit operator NativeVariableArgument(double value) => new NativeVariableArgument() { FloatValue = value };
}
