using System.Collections.Generic;

namespace System
{
    public abstract unsafe partial class Array
    {
        public static T[] Empty<T>() => new T[0];

        public static void Clear<T>(T[] array, int index, int length)
        {
            ValidateRange(array, index, length);
            for (int i = 0; i < length; i++)
                array[index + i] = default;
        }

        public static void Copy<T>(T[] sourceArray, T[] destinationArray, int length)
            => Copy(sourceArray, 0, destinationArray, 0, length);

        public static void Copy<T>(T[] sourceArray, int sourceIndex, T[] destinationArray, int destinationIndex, int length)
        {
            ValidateRange(sourceArray, sourceIndex, length);
            ValidateRange(destinationArray, destinationIndex, length);
            if (length == 0)
                return;

            if (ReferenceEquals(sourceArray, destinationArray) && destinationIndex > sourceIndex &&
                destinationIndex < sourceIndex + length)
            {
                for (int i = length - 1; i >= 0; i--)
                    destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
                return;
            }

            for (int i = 0; i < length; i++)
                destinationArray[destinationIndex + i] = sourceArray[sourceIndex + i];
        }

        public static void Resize<T>(ref T[] array, int newSize)
        {
            if (newSize < 0)
                throw new ArgumentOutOfRangeException("The array size cannot be negative.");

            T[] result = new T[newSize];
            if (array != null)
                Copy(array, 0, result, 0, array.Length < newSize ? array.Length : newSize);
            array = result;
        }

        public static int IndexOf<T>(T[] array, T value)
            => IndexOf(array, value, 0, array == null ? 0 : array.Length);

        public static int IndexOf<T>(T[] array, T value, int startIndex, int count)
        {
            ValidateRange(array, startIndex, count);
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < count; i++)
                if (comparer.Equals(array[startIndex + i], value))
                    return startIndex + i;
            return -1;
        }

        public static void Reverse<T>(T[] array)
            => Reverse(array, 0, array == null ? 0 : array.Length);

        public static void Reverse<T>(T[] array, int index, int length)
        {
            ValidateRange(array, index, length);
            int left = index;
            int right = index + length - 1;
            while (left < right)
            {
                T value = array[left];
                array[left++] = array[right];
                array[right--] = value;
            }
        }

        public static void Sort<T>(T[] array)
            => Sort(array, 0, array == null ? 0 : array.Length, null);

        public static void Sort<T>(T[] array, IComparer<T> comparer)
            => Sort(array, 0, array == null ? 0 : array.Length, comparer);

        public static void Sort<T>(T[] array, int index, int length, IComparer<T> comparer)
        {
            ValidateRange(array, index, length);
            comparer = comparer ?? Comparer<T>.Default;
            for (int i = index + 1; i < index + length; i++)
            {
                T value = array[i];
                int j = i - 1;
                while (j >= index && comparer.Compare(array[j], value) > 0)
                {
                    array[j + 1] = array[j];
                    j--;
                }
                array[j + 1] = value;
            }
        }

        private static void ValidateRange<T>(T[] array, int index, int length)
        {
            if (array == null)
                throw new ArgumentNullException("The array cannot be null.");
            if (index < 0 || length < 0 || index > array.Length - length)
                throw new ArgumentException("The array range is invalid.");
        }
    }
}
