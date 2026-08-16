// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Internal.Runtime.CompilerServices;

namespace System
{
    public readonly unsafe ref struct ReadOnlySpan<T>
    {
        /// <summary>A byref or a native ptr.</summary>
        internal readonly IntPtr _pointer;
        /// <summary>The number of elements this ReadOnlySpan contains.</summary>
        private readonly int _length;

        public ReadOnlySpan(T[]? array, int start, int length)
        {
            _pointer = (IntPtr)Unsafe.AsPointer(ref array[start]);
            _length = length;
        }

        /// <summary>
        /// The number of items in the read-only span.
        /// </summary>
        public int Length
        {
            get => _length;
        }
    }
}