global using static NativeLib;

using System;
using System.Runtime;
using System.Runtime.InteropServices;

internal unsafe class NativeLib
{
    [DllImport("*")]
    public static extern int vsnprintf_(byte* buffer, int count, void* format, params VariableArgument[] va);

    [DllImport("*")]
    public static extern void* memcpy(void* dest, void* src, ulong n);

    [DllImport("*")]
    public static extern void* memset(void* ptr, int value, ulong num);

    [DllImport("*", EntryPoint = "vprintf_")]
    public static extern int printf(void* format, params VariableArgument[] va);

    [RuntimeExport("_putchar")]
    public static void _putchar(char character)
    {
        Console.Write(character);
    }
}