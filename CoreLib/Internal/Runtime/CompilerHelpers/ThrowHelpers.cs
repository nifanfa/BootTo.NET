// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Internal.TypeSystem;
using System;

namespace Internal.Runtime.CompilerHelpers
{
    /// <summary>
    /// These methods are used to throw exceptions from generated code. The type and methods
    /// need to be public as they constitute a public contract with the .NET Native toolchain.
    /// </summary>
    public static class ThrowHelpers
    {
        public static void ThrowOverflowException()
        {
            Console.WriteLine("OverflowException");
            for (; ; );
        }

        public static void ThrowIndexOutOfRangeException()
        {
            Console.WriteLine("IndexOutOfRangeException");
            for (; ; );
        }

        public static void ThrowNullReferenceException()
        {
            Console.WriteLine("NullReferenceException");
            for (; ; );
        }

        public static void ThrowDivideByZeroException()
        {
            Console.WriteLine("DivideByZeroException");
            for (; ; );
        }

        public static void ThrowArrayTypeMismatchException()
        {
            Console.WriteLine("ArrayTypeMismatchException");
            for (; ; );
        }

        public static void ThrowPlatformNotSupportedException()
        {
            Console.WriteLine("PlatformNotSupportedException");
            for (; ; );
        }

        public static void ThrowNotImplementedException()
        {
            Console.WriteLine("NotImplementedException");
            for (; ; );
        }

        public static void ThrowNotSupportedException()
        {
            Console.WriteLine("NotSupportedException");
            for (; ; );
        }

        public static void ThrowBadImageFormatException(ExceptionStringID id)
        {
            Console.WriteLine("CreateBadImageFormatException");
            for (; ; );
        }

        public static void ThrowTypeLoadException(ExceptionStringID id, string className, string typeName)
        {
            Console.WriteLine("CreateTypeLoadException");
            for (; ; );
        }

        public static void ThrowTypeLoadExceptionWithArgument(ExceptionStringID id, string className, string typeName, string messageArg)
        {
            Console.WriteLine("CreateTypeLoadException");
            for (; ; );
        }

        public static void ThrowMissingMethodException(ExceptionStringID id, string methodName)
        {
            Console.WriteLine("CreateMissingMethodException");
            for (; ; );
        }

        public static void ThrowMissingFieldException(ExceptionStringID id, string fieldName)
        {
            Console.WriteLine("CreateMissingFieldException");
            for (; ; );
        }

        public static void ThrowFileNotFoundException(ExceptionStringID id, string fileName)
        {
            Console.WriteLine("CreateFileNotFoundException");
            for (; ; );
        }

        public static void ThrowInvalidProgramException(ExceptionStringID id)
        {
            Console.WriteLine("CreateInvalidProgramException");
            for (; ; );
        }

        public static void ThrowInvalidProgramExceptionWithArgument(ExceptionStringID id, string methodName)
        {
            Console.WriteLine("CreateInvalidProgramException");
            for (; ; );
        }

        public static void ThrowArgumentException()
        {
            Console.WriteLine("ArgumentException");
            for (; ; );
        }

        public static void ThrowArgumentOutOfRangeException()
        {
            Console.WriteLine("ArgumentOutOfRangeException");
            for (; ; );
        }
    }
}
