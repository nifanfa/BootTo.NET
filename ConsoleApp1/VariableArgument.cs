using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
internal struct VariableArgument
{
    [FieldOffset(0)]
    public long SignedValue;
    [FieldOffset(0)]
    public ulong UnsignedValue;
    [FieldOffset(0)]
    public double FloatValue;

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
}