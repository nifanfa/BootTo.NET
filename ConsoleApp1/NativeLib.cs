#pragma warning disable
global using static NativeLib;
using Internal.Runtime.CompilerServices;

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
    public static extern int snprintf(byte* buffer, int count, void* format, params VariableArgument[] va);

    [DllImport("*", EntryPoint = "vprintf_")]
    public static extern int printf(void* format, params VariableArgument[] va);

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

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct VariableArgument
{
    [FieldOffset(0)]
    public long SignedValue;
    [FieldOffset(0)]
    public ulong UnsignedValue;
    [FieldOffset(0)]
    public double FloatValue;
    [FieldOffset(0)]
    public void* PointerValue;

    public static implicit operator VariableArgument(sbyte value) => new VariableArgument() { SignedValue = value };
    public static implicit operator VariableArgument(short value) => new VariableArgument() { SignedValue = value };
    public static implicit operator VariableArgument(int value) => new VariableArgument() { SignedValue = value };
    public static implicit operator VariableArgument(long value) => new VariableArgument() { SignedValue = value };
    public static implicit operator VariableArgument(byte value) => new VariableArgument() { UnsignedValue = value };
    public static implicit operator VariableArgument(ushort value) => new VariableArgument() { UnsignedValue = value };
    public static implicit operator VariableArgument(uint value) => new VariableArgument() { UnsignedValue = value };
    public static implicit operator VariableArgument(ulong value) => new VariableArgument() { UnsignedValue = value };
    public static implicit operator VariableArgument(float value) => new VariableArgument() { FloatValue = value };
    public static implicit operator VariableArgument(double value) => new VariableArgument() { FloatValue = value };
    public static implicit operator VariableArgument(byte[] value) => new VariableArgument() { PointerValue = Unsafe.AsPointer(ref value[0]) };
}
