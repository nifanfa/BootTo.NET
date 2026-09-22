using System.Collections.Generic;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System
{
    public class Object
    {
        internal Type m_pType;

        public Object() { }

        public virtual bool Equals(object other) => this == other;
        public static bool ReferenceEquals(object left, object right) => left == right;
        public static bool Equals(object left, object right) => left == null ? right == null : left.Equals(right);
        public virtual int GetHashCode() => 1;
        public virtual string ToString() => GetType().FullName;
        public Type GetType() => m_pType;
    }

    public static class GC
    {
        public static int Collect() => GCHeap.Collect();
    }

    public struct Void { }
    public interface IComparable
    {
        int CompareTo(object value);
    }
    public interface IComparable<in T>
    {
        int CompareTo(T value);
    }
    public interface IEquatable<T>
    {
        bool Equals(T other);
    }

    public interface IFormatProvider
    {
        object GetFormat(Type formatType);
    }
    public struct Boolean
    {
        public override string ToString() => this ? "True" : "False";
    }
    public struct Char
    {
        public const char MinValue = (char)0;
        public const char MaxValue = (char)0xffff;
        public override string ToString() => new string(new char[] { this });
    }
    public partial struct SByte
    {
        public const sbyte MinValue = -128;
        public const sbyte MaxValue = 127;
        public override string ToString() => Number.Format(this);
    }
    public partial struct Byte
    {
        public const byte MinValue = 0;
        public const byte MaxValue = 255;
        public override string ToString() => Number.Format((ulong)this);
    }
    public partial struct Int16
    {
        public const short MinValue = -32768;
        public const short MaxValue = 32767;
        public override string ToString() => Number.Format(this);
    }
    public partial struct UInt16
    {
        public const ushort MinValue = 0;
        public const ushort MaxValue = 65535;
        public override string ToString() => Number.Format((ulong)this);
    }
    public partial struct Int32 : IEquatable<int>
    {
        public const int MinValue = -2147483648;
        public const int MaxValue = 2147483647;
        public bool Equals(int other) => this == other;
        public override bool Equals(object other) => other is int value && Equals(value);
        public override int GetHashCode() => this;
        public override string ToString() => Number.Format(this);
    }
    public partial struct UInt32
    {
        public const uint MinValue = 0;
        public const uint MaxValue = 0xffffffff;
        public override string ToString() => Number.Format((ulong)this);
    }
    public partial struct Int64
    {
        public const long MinValue = -9223372036854775808;
        public const long MaxValue = 9223372036854775807;
        public override string ToString() => Number.Format(this);
    }
    public partial struct UInt64
    {
        public const ulong MinValue = 0;
        public const ulong MaxValue = 0xffffffffffffffff;
        public override string ToString() => Number.Format(this);
    }
    public struct IntPtr
    {
        unsafe private void* _value;

        public static readonly IntPtr Zero;

        public unsafe IntPtr(void* value)
        {
            _value = value;
        }

        public unsafe IntPtr(int value)
        {
            _value = (void*)value;
        }

        public unsafe IntPtr(long value)
        {
            _value = (void*)value;
        }

        public static explicit operator IntPtr(int value) => new IntPtr(value);
        public static explicit operator IntPtr(long value) => new IntPtr(value);
        public static unsafe explicit operator IntPtr(void* value) => new IntPtr(value);
        public static unsafe explicit operator void*(IntPtr value) => value._value;
        public static unsafe explicit operator int(IntPtr value) => unchecked((int)value._value);
        public static unsafe explicit operator long(IntPtr value) => unchecked((long)value._value);
        public static unsafe bool operator ==(IntPtr value1, IntPtr value2) => value1._value == value2._value;
        public static unsafe bool operator !=(IntPtr value1, IntPtr value2) => value1._value != value2._value;
        public override bool Equals(object other) => other is IntPtr value && this == value;
        public override unsafe int GetHashCode() => unchecked((int)(long)_value);
    }
    public struct UIntPtr
    {
        public static readonly UIntPtr Zero;
        public override string ToString() => "0";
    }
    public readonly struct Index
    {
        private readonly int _value;

        public Index(int value, bool fromEnd = false)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException();
            _value = fromEnd ? ~value : value;
        }

        private Index(int value)
        {
            _value = value;
        }

        public static Index Start => new Index(0);
        public static Index End => new Index(~0);
        public int Value => _value < 0 ? ~_value : _value;
        public bool IsFromEnd => _value < 0;
        public int GetOffset(int length) => _value < 0 ? length + ~_value : _value;
        public static Index FromStart(int value) => new Index(value);
        public static Index FromEnd(int value) => new Index(~value);
        public static implicit operator Index(int value) => FromStart(value);
    }

    public readonly struct TimeSpan
    {
        public const long TicksPerMillisecond = 10_000;
        public const long TicksPerSecond = TicksPerMillisecond * 1000;
        public const long TicksPerMinute = TicksPerSecond * 60;
        public const long TicksPerHour = TicksPerMinute * 60;
        public const long TicksPerDay = TicksPerHour * 24;

        public static readonly TimeSpan MinValue = new TimeSpan(long.MinValue);
        public static readonly TimeSpan MaxValue = new TimeSpan(long.MaxValue);
        public static readonly TimeSpan Zero = new TimeSpan(0);
        private readonly long _ticks;

        public TimeSpan(long ticks) => _ticks = ticks;
        public TimeSpan(int hours, int minutes, int seconds)
            : this(0, hours, minutes, seconds, 0) { }
        public TimeSpan(int days, int hours, int minutes, int seconds)
            : this(days, hours, minutes, seconds, 0) { }
        public TimeSpan(int days, int hours, int minutes, int seconds, int milliseconds)
            => _ticks = (long)days * TicksPerDay + (long)hours * TicksPerHour +
                (long)minutes * TicksPerMinute + (long)seconds * TicksPerSecond +
                (long)milliseconds * TicksPerMillisecond;

        public long Ticks => _ticks;
        public int Days => (int)(_ticks / TicksPerDay);
        public int Hours => (int)((_ticks / TicksPerHour) % 24);
        public int Minutes => (int)((_ticks / TicksPerMinute) % 60);
        public int Seconds => (int)((_ticks / TicksPerSecond) % 60);
        public int Milliseconds => (int)((_ticks / TicksPerMillisecond) % 1000);
        public long TotalDays => _ticks / TicksPerDay;
        public long TotalHours => _ticks / TicksPerHour;
        public long TotalMinutes => _ticks / TicksPerMinute;
        public long TotalSeconds => _ticks / TicksPerSecond;
        public long TotalMilliseconds => _ticks / TicksPerMillisecond;

        public static TimeSpan FromDays(long value) => new TimeSpan(value * TicksPerDay);
        public static TimeSpan FromHours(long value) => new TimeSpan(value * TicksPerHour);
        public static TimeSpan FromMinutes(long value) => new TimeSpan(value * TicksPerMinute);
        public static TimeSpan FromSeconds(long value) => new TimeSpan(value * TicksPerSecond);
        public static TimeSpan FromMilliseconds(long value) => new TimeSpan(value * TicksPerMillisecond);
        public override string ToString() => string.Format("{0}:{1:D2}:{2:D2}", Hours, Minutes, Seconds);
        public override bool Equals(object other) => other is TimeSpan value && value._ticks == _ticks;
        public override int GetHashCode() => (int)(_ticks ^ (_ticks >> 32));
        public static TimeSpan operator +(TimeSpan left, TimeSpan right) => new TimeSpan(left._ticks + right._ticks);
        public static TimeSpan operator -(TimeSpan left, TimeSpan right) => new TimeSpan(left._ticks - right._ticks);
        public static TimeSpan operator -(TimeSpan value) => new TimeSpan(-value._ticks);
        public static bool operator ==(TimeSpan left, TimeSpan right) => left._ticks == right._ticks;
        public static bool operator !=(TimeSpan left, TimeSpan right) => left._ticks != right._ticks;
    }

    public enum DateTimeKind
    {
        Unspecified,
        Utc,
        Local
    }

    public enum DayOfWeek
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday
    }

    public readonly struct DateTime
    {
        private const long TicksAtUnixEpoch = 621355968000000000;
        private const long MaxTicks = 3155378975999999999;
        private const ulong TicksMask = 0x3fffffffffffffff;
        private const int KindShift = 62;
        private readonly ulong _dateData;

        public static readonly DateTime MinValue = new DateTime(0);
        public static readonly DateTime MaxValue = new DateTime(MaxTicks);

        [DllImport("*", EntryPoint = "GetCurrentTimeMilliseconds")]
        private static extern long GetCurrentTimeMilliseconds();

        public DateTime(int year, int month, int day)
            : this(year, month, day, 0, 0, 0, 0) { }

        public DateTime(int year, int month, int day, int hour, int minute, int second)
            : this(year, month, day, hour, minute, second, 0) { }

        public DateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
            : this(year, month, day, hour, minute, second, 0, kind) { }

        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
            : this(year, month, day, hour, minute, second, millisecond, DateTimeKind.Unspecified) { }

        public DateTime(int year, int month, int day, int hour, int minute, int second, int millisecond,
            DateTimeKind kind)
        {
            Validate(year, month, day, hour, minute, second, millisecond);
            if (kind < DateTimeKind.Unspecified || kind > DateTimeKind.Local)
                throw new ArgumentException();
            _dateData = (ulong)(GetTicks(year, month, day) + (long)hour * TimeSpan.TicksPerHour +
                (long)minute * TimeSpan.TicksPerMinute + (long)second * TimeSpan.TicksPerSecond +
                (long)millisecond * TimeSpan.TicksPerMillisecond) | ((ulong)kind << KindShift);
        }

        public DateTime(long ticks)
            : this(ticks, DateTimeKind.Unspecified) { }

        public DateTime(long ticks, DateTimeKind kind)
        {
            if (ticks < 0 || ticks > MaxTicks)
                throw new ArgumentOutOfRangeException();
            if (kind < DateTimeKind.Unspecified || kind > DateTimeKind.Local)
                throw new ArgumentException();
            _dateData = (ulong)ticks | ((ulong)kind << KindShift);
        }

        public long Ticks => (long)(_dateData & TicksMask);
        public int Year => GetDatePart(0);
        public int Month => GetDatePart(1);
        public int Day => GetDatePart(2);
        public int Hour => (int)((Ticks / TimeSpan.TicksPerHour) % 24);
        public int Minute => (int)((Ticks / TimeSpan.TicksPerMinute) % 60);
        public int Second => (int)((Ticks / TimeSpan.TicksPerSecond) % 60);
        public int Millisecond => (int)((Ticks / TimeSpan.TicksPerMillisecond) % 1000);
        public DateTime Date => new DateTime(Ticks - Ticks % TimeSpan.TicksPerDay, Kind);
        public TimeSpan TimeOfDay => new TimeSpan(Ticks % TimeSpan.TicksPerDay);
        public DayOfWeek DayOfWeek => (DayOfWeek)((Ticks / TimeSpan.TicksPerDay + 1) % 7);
        public DateTimeKind Kind => (DateTimeKind)(_dateData >> KindShift);

        public static DateTime UtcNow => new DateTime(
            TicksAtUnixEpoch + GetCurrentTimeMilliseconds() * TimeSpan.TicksPerMillisecond,
            DateTimeKind.Utc);
        public static DateTime Now => UtcNow;
        public static DateTime Today => Now.Date;
        public static DateTime FromFileTime(long fileTime) => new DateTime(fileTime + TicksAtUnixEpoch);
        public long ToFileTime() => Ticks - TicksAtUnixEpoch;
        public DateTime AddTicks(long value) => new DateTime(Ticks + value, Kind);
        public DateTime Add(TimeSpan value) => AddTicks(value.Ticks);
        public DateTime AddMilliseconds(long value) => Add(TimeSpan.FromMilliseconds(value));
        public DateTime AddSeconds(long value) => Add(TimeSpan.FromSeconds(value));
        public DateTime AddMinutes(long value) => Add(TimeSpan.FromMinutes(value));
        public DateTime AddHours(long value) => Add(TimeSpan.FromHours(value));
        public DateTime AddDays(long value) => Add(TimeSpan.FromDays(value));
        public DateTime Subtract(TimeSpan value) => new DateTime(Ticks - value.Ticks, Kind);
        public TimeSpan Subtract(DateTime value) => new TimeSpan(Ticks - value.Ticks);
        public static int Compare(DateTime left, DateTime right)
            => left.Ticks < right.Ticks ? -1 : left.Ticks > right.Ticks ? 1 : 0;
        public int CompareTo(DateTime other) => Compare(this, other);
        public static DateTime SpecifyKind(DateTime value, DateTimeKind kind) => new DateTime(value.Ticks, kind);
        public DateTime ToUniversalTime() => SpecifyKind(this, DateTimeKind.Utc);
        public DateTime ToLocalTime() => this;
        public override bool Equals(object other) => other is DateTime value && value.Ticks == Ticks;
        public override int GetHashCode() => (int)(Ticks ^ (Ticks >> 32));
        public static DateTime operator +(DateTime value, TimeSpan span) => value.Add(span);
        public static DateTime operator -(DateTime value, TimeSpan span) => value.Subtract(span);
        public static TimeSpan operator -(DateTime left, DateTime right) => left.Subtract(right);
        public static bool operator ==(DateTime left, DateTime right) => left.Ticks == right.Ticks;
        public static bool operator !=(DateTime left, DateTime right) => left.Ticks != right.Ticks;
        public static bool operator <(DateTime left, DateTime right) => left.Ticks < right.Ticks;
        public static bool operator >(DateTime left, DateTime right) => left.Ticks > right.Ticks;
        public static bool operator <=(DateTime left, DateTime right) => left.Ticks <= right.Ticks;
        public static bool operator >=(DateTime left, DateTime right) => left.Ticks >= right.Ticks;
        public override string ToString()
            => string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}", Year, Month, Day, Hour, Minute, Second);

        public static bool IsLeapYear(int year)
        {
            if (year < 1 || year > 9999)
                throw new ArgumentOutOfRangeException();
            return year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
        }

        public static int DaysInMonth(int year, int month)
        {
            if (year < 1 || year > 9999 || month < 1 || month > 12)
                throw new ArgumentOutOfRangeException();
            if (month == 2)
                return IsLeapYear(year) ? 29 : 28;
            return 30 + ((0xAD5 >> (month - 1)) & 1);
        }

        private int GetDatePart(int part)
        {
            long days = Ticks / TimeSpan.TicksPerDay;
            int low = 1;
            int high = 9999;
            while (low < high)
            {
                int mid = (low + high + 1) / 2;
                if (GetTicks(mid, 1, 1) / TimeSpan.TicksPerDay <= days) low = mid;
                else high = mid - 1;
            }
            int year = low;
            long dayOfYear = days - GetTicks(year, 1, 1) / TimeSpan.TicksPerDay;
            if (part == 0) return year;
            int month = 1;
            while (month < 12 && dayOfYear >= DaysInMonth(year, month))
            {
                dayOfYear -= DaysInMonth(year, month++);
            }
            return part == 1 ? month : (int)dayOfYear + 1;
        }

        private static long GetTicks(int year, int month, int day)
        {
            long y = year - 1;
            long days = y * 365 + y / 4 - y / 100 + y / 400;
            days += (367 * month - 362) / 12;
            if (month > 2)
                days -= IsLeapYear(year) ? 1 : 2;
            return (days + day - 1) * TimeSpan.TicksPerDay;
        }

        private static void Validate(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            if (year < 1 || year > 9999 || month < 1 || month > 12 || day < 1 || day > DaysInMonth(year, month) ||
                hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59 ||
                millisecond < 0 || millisecond > 999)
                throw new ArgumentOutOfRangeException();
        }
    }

    public partial struct Single
    {
        public const float MinValue = -3.4028234663852886E+38F;
        public const float MaxValue = 3.4028234663852886E+38F;
    }
    public partial struct Double
    {
        public const double MinValue = -1.7976931348623157E+308;
        public const double MaxValue = 1.7976931348623157E+308;
    }

    internal static partial class Number
    {
        public static string Format(long value)
        {
            bool negative = value < 0;
            ulong magnitude = negative ? (ulong)(-(value + 1)) + 1 : (ulong)value;
            return Format(magnitude, negative);
        }

        public static string Format(ulong value) => Format(value, false);

        private static string Format(ulong value, bool negative)
        {
            char[] buffer = new char[negative ? 21 : 20];
            int index = buffer.Length;
            do
            {
                buffer[--index] = (char)('0' + (value % 10));
                value /= 10;
            }
            while (value != 0);
            if (negative)
                buffer[--index] = '-';
            char[] result = new char[buffer.Length - index];
            for (int i = 0; i < result.Length; i++)
                result[i] = buffer[index + i];
            return new string(result);
        }
    }

    public abstract class ValueType { }
    public abstract class Enum : ValueType
    {
        internal ulong m_value;

        public override string ToString()
        {
            Type type = GetType();
            string[] names = type.EnumNames;
            ulong[] values = type.EnumValues;
            ulong value = m_value;
            for (int index = 0; index < values.Length; index++)
                if (values[index] == value)
                    return names[index];

            if (type.IsFlagsEnum && value != 0)
            {
                ulong remaining = value;
                string result = null;
                for (int index = values.Length - 1; index >= 0; index--)
                {
                    ulong candidate = values[index];
                    if (candidate == 0 || (remaining & candidate) != candidate)
                        continue;
                    remaining &= ~candidate;
                    result = result == null ? names[index] : string.Concat(names[index], ", ", result);
                }
                if (remaining == 0 && result != null)
                    return result;
            }

            return type.IsSignedEnum ? Number.Format((long)value) : Number.Format(value);
        }
    }
    public struct Nullable<T> where T : struct
    {
        private bool _hasValue;
        private T _value;
        public Nullable(T value) { _value = value; _hasValue = true; }
        public bool HasValue => _hasValue;
        public T Value => _hasValue ? _value : throw new InvalidOperationException();
        public T GetValueOrDefault() => _value;
        public T GetValueOrDefault(T defaultValue) => _hasValue ? _value : defaultValue;
        public static implicit operator T?(T value) => new T?(value);
        public static explicit operator T(T? value) => value.Value;
    }
    public abstract unsafe class Array
    {
        internal int _length;
        public int Length => _length;
        private int[] _lengths;
        internal int m_elementSize;
        internal byte* m_pData;

        public virtual int Rank => _lengths == null ? 1 : _lengths.Length;
        public virtual int GetLength(int dimension)
        {
            if ((uint)dimension >= (uint)Rank)
                throw new IndexOutOfRangeException("The array dimension is outside the array rank.");
            if (_lengths == null)
                return Length;
            return _lengths[dimension];
        }
        public virtual int GetLowerBound(int dimension)
        {
            if ((uint)dimension >= (uint)Rank)
                throw new IndexOutOfRangeException("The array dimension is outside the array rank.");
            return 0;
        }
        public virtual int GetUpperBound(int dimension) => GetLength(dimension) - 1;

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
                throw new ArgumentException("The array size cannot be negative.");
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
            for (int i = 0; i < count; i++)
                if (Equals(array[startIndex + i], value))
                    return startIndex + i;
            return -1;
        }

        public static void Reverse<T>(T[] array) => Reverse(array, 0, array == null ? 0 : array.Length);

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

        public static void Sort<T>(T[] array) => Sort(array, 0, array == null ? 0 : array.Length, null);
        public static void Sort<T>(T[] array, IComparer<T> comparer)
            => Sort(array, 0, array == null ? 0 : array.Length, comparer);

        public static void Sort<T>(T[] array, int index, int length, IComparer<T> comparer)
        {
            ValidateRange(array, index, length);
            for (int i = index + 1; i < index + length; i++)
            {
                T value = array[i];
                int j = i - 1;
                while (j >= index && comparer != null && comparer.Compare(array[j], value) > 0)
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

    public readonly ref struct ByReference<T>
    {
        private readonly ref T _value;

        internal ByReference(ref T value)
        {
            _value = ref value;
        }

        internal unsafe ByReference(void* pointer)
        {
            _value = ref *(T*)pointer;
        }

        public ref T Value => ref _value;

        public ref T ElementAt(int index)
        {
            unsafe
            {
                fixed (T* value = &_value)
                    return ref value[index];
            }
        }

        public static implicit operator ByReference<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                return default;
            return new ByReference<T>(ref array[0]);
        }

        public static implicit operator ByReference<T>(Span<T> span)
        {
            if (span.Length == 0)
                return default;
            return new ByReference<T>(ref span[0]);
        }

        public static implicit operator ByReference<T>(ReadOnlySpan<T> span)
        {
            if (span.Length == 0)
                return default;
            return span._pointer;
        }
    }

    public ref struct Span<T>
    {
        private ByReference<T> _pointer;
        private int _length;

        public Span(T[] array)
            : this(array, 0, array == null ? 0 : array.Length) { }

        public Span(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                    throw new ArgumentException("The span range is invalid.");
                _pointer = default;
                _length = 0;
                return;
            }
            if (start < 0 || length < 0 || start > array.Length - length)
                throw new ArgumentException("The span range is invalid.");
            _pointer = length == 0 ? default : new ByReference<T>(ref array[start]);
            _length = length;
        }

        public unsafe Span(void* pointer, int length)
        {
            if (length < 0)
                throw new ArgumentException("The span length is invalid.");
            _pointer = length == 0 ? default : new ByReference<T>(pointer);
            _length = length;
        }

        internal Span(ref T reference, int length)
        {
            if (length < 0)
                throw new ArgumentException("The span length is invalid.");
            _pointer = length == 0 ? default : new ByReference<T>(ref reference);
            _length = length;
        }

        public int Length => _length;
        public bool IsEmpty => _length == 0;

        public ref T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_length)
                    throw new IndexOutOfRangeException("The span index is outside the span.");
                return ref _pointer.ElementAt(index);
            }
        }

        public Span<T> Slice(int start) => Slice(start, _length - start);

        public Span<T> Slice(int start, int length)
        {
            if (start < 0 || length < 0 || start > _length - length)
                throw new ArgumentException("The span range is invalid.");
            return length == 0 ? default : new Span<T>(ref _pointer.ElementAt(start), length);
        }

        public ref T GetPinnableReference() => ref _pointer.Value;

        public static implicit operator Span<T>(T[] array) => new Span<T>(array);
        public static implicit operator ReadOnlySpan<T>(Span<T> span)
            => new ReadOnlySpan<T>(ref span._pointer.Value, span._length);
    }

    public readonly ref partial struct ReadOnlySpan<T>
    {
        internal readonly ByReference<T> _pointer;
        private readonly int _length;

        public ReadOnlySpan(T[] array)
            : this(array, 0, array == null ? 0 : array.Length) { }

        public ReadOnlySpan(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                    throw new ArgumentException("The span range is invalid.");
                _pointer = default;
                _length = 0;
                return;
            }
            if (start < 0 || length < 0 || start > array.Length - length)
                throw new ArgumentException("The span range is invalid.");
            _pointer = length == 0 ? default : new ByReference<T>(ref array[start]);
            _length = length;
        }

        internal ReadOnlySpan(ref T reference, int length)
        {
            if (length < 0)
                throw new ArgumentException("The span length is invalid.");
            _pointer = length == 0 ? default : new ByReference<T>(ref reference);
            _length = length;
        }

        public unsafe ReadOnlySpan(void* pointer, int length)
        {
            if (length < 0)
                throw new ArgumentException("The span length is invalid.");
            _pointer = length == 0 ? default : new ByReference<T>(pointer);
            _length = length;
        }

        public int Length => _length;
        public bool IsEmpty => _length == 0;

        public ref readonly T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_length)
                    throw new IndexOutOfRangeException("The span index is outside the span.");
                return ref _pointer.ElementAt(index);
            }
        }

        public ReadOnlySpan<T> Slice(int start) => Slice(start, _length - start);

        public ReadOnlySpan<T> Slice(int start, int length)
        {
            if (start < 0 || length < 0 || start > _length - length)
                throw new ArgumentException("The span range is invalid.");
            return length == 0 ? default : new ReadOnlySpan<T>(ref _pointer.ElementAt(start), length);
        }

        public ref readonly T GetPinnableReference() => ref _pointer.Value;

        public static implicit operator ReadOnlySpan<T>(T[] array) => new ReadOnlySpan<T>(array);
    }

    public sealed class ArrayEnumerator<T> : IEnumerator<T>
    {
        private T[] _array;
        private int _index = -1;

        public ArrayEnumerator(T[] array) { _array = array; }
        public T Current => _array[_index];
        object Collections.IEnumerator.Current => Current;
        public bool MoveNext() => ++_index < _array.Length;
        public void Reset() { _index = -1; }
        public void Dispose() { }
    }

    public sealed partial class String
    {
        public int Length;
        private char[] _chars;
        public static readonly string Empty = "";

        public String() { }
        public String(char[] value)
        {
            int length = value == null ? 0 : value.Length;
            _chars = new char[length + 1];
            for (int index = 0; index < length; index++)
                _chars[index] = value[index];
            Length = length;
        }

        public char this[int index] => _chars[index];
        public static implicit operator ByReference<char>(string value)
        {
            if (value == null || value.Length == 0)
                return default;
            return new ByReference<char>(ref value._chars[0]);
        }
        public ref char GetPinnableReference() => ref _chars[0];
        public override string ToString() => this;
        public override bool Equals(object other) => other is string value && Equals(this, value);
        public bool Equals(string other) => Equals(this, other);
        public override int GetHashCode()
        {
            int hash = 5381;
            for (int index = 0; index < Length; index++)
                hash = ((hash << 5) + hash) ^ this[index];
            return hash;
        }
        public static bool Equals(string left, string right)
        {
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return ReferenceEquals(left, right);
            if (left.Length != right.Length)
                return false;
            for (int index = 0; index < left.Length; index++)
                if (left[index] != right[index])
                    return false;
            return true;
        }
        public static bool operator ==(string left, string right) => Equals(left, right);
        public static bool operator !=(string left, string right) => !Equals(left, right);
        public static string Concat(string left, string right)
        {
            if (ReferenceEquals(left, null))
                return right;
            if (ReferenceEquals(right, null))
                return left;
            char[] value = new char[left.Length + right.Length];
            for (int index = 0; index < left.Length; index++)
                value[index] = left[index];
            for (int index = 0; index < right.Length; index++)
                value[left.Length + index] = right[index];
            return new string(value);
        }

        public static string Concat(string first, string second, string third)
            => Concat(Concat(first, second), third);

        public static string Concat(string first, string second, string third, string fourth)
            => Concat(Concat(first, second), Concat(third, fourth));

        public static string Format(string format, object arg0)
            => Format(format, new object[] { arg0 });

        public static string Format(string format, object arg0, object arg1)
            => Format(format, new object[] { arg0, arg1 });

        public static string Format(string format, object arg0, object arg1, object arg2)
            => Format(format, new object[] { arg0, arg1, arg2 });

        public static string Format(string format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException("The format string cannot be null.");
            if (args == null)
                throw new ArgumentNullException("The format arguments cannot be null.");

            StringBuilder result = new StringBuilder(format.Length + 16);
            int index = 0;
            while (index < format.Length)
            {
                char current = format[index++];
                if (current == '{')
                {
                    if (index < format.Length && format[index] == '{')
                    {
                        result.Append('{');
                        index++;
                        continue;
                    }

                    int argumentIndex = 0;
                    int digits = 0;
                    while (index < format.Length && format[index] >= '0' && format[index] <= '9')
                    {
                        argumentIndex = argumentIndex * 10 + format[index++] - '0';
                        digits++;
                    }
                    if (digits == 0 || argumentIndex >= args.Length)
                        throw new FormatException("The format contains an invalid argument index.");

                    while (index < format.Length && format[index] == ' ')
                        index++;

                    string specifier = null;
                    if (index < format.Length && format[index] == ':')
                    {
                        int start = ++index;
                        while (index < format.Length && format[index] != '}')
                            index++;
                        if (index > start)
                            specifier = format.Substring(start, index - start);
                    }
                    if (index >= format.Length || format[index++] != '}')
                        throw new FormatException("The format item is missing its closing brace.");

                    result.Append(FormatValue(args[argumentIndex], specifier));
                    continue;
                }

                if (current == '}')
                {
                    if (index < format.Length && format[index] == '}')
                    {
                        result.Append('}');
                        index++;
                        continue;
                    }
                    throw new FormatException("The format contains an unmatched closing brace.");
                }
                result.Append(current);
            }
            return result.ToString();
        }

        private static string FormatValue(object value, string specifier)
        {
            if (value == null)
                return Empty;
            if (IsNullOrEmpty(specifier))
                return value.ToString();

            char type = specifier[0];
            if (type == 'x' || type == 'X')
            {
                int width = ParseWidth(specifier);
                if (value is byte) return FormatUnsigned((byte)value, type == 'X', width);
                if (value is ushort) return FormatUnsigned((ushort)value, type == 'X', width);
                if (value is uint) return FormatUnsigned((uint)value, type == 'X', width);
                if (value is ulong) return FormatUnsigned((ulong)value, type == 'X', width);
                if (value is sbyte) return FormatUnsigned(unchecked((byte)(sbyte)value), type == 'X', width < 2 ? 2 : width);
                if (value is short) return FormatUnsigned(unchecked((ushort)(short)value), type == 'X', width < 4 ? 4 : width);
                if (value is int) return FormatUnsigned(unchecked((uint)(int)value), type == 'X', width < 8 ? 8 : width);
                if (value is long) return FormatUnsigned(unchecked((ulong)(long)value), type == 'X', width < 16 ? 16 : width);
            }

            if (type == 'd' || type == 'D')
            {
                int width = ParseWidth(specifier);
                string text = value.ToString();
                int sign = text.Length > 0 && text[0] == '-' ? 1 : 0;
                StringBuilder padded = new StringBuilder(text.Length > width + sign ? text.Length : width + sign);
                if (sign != 0)
                    padded.Append('-');
                for (int index = text.Length - sign; index < width; index++)
                    padded.Append('0');
                for (int index = sign; index < text.Length; index++)
                    padded.Append(text[index]);
                return padded.ToString();
            }

            throw new FormatException("The format specifier is not supported.");
        }

        private static int ParseWidth(string specifier)
        {
            int width = 0;
            for (int index = 1; index < specifier.Length; index++)
            {
                if (specifier[index] < '0' || specifier[index] > '9')
                    throw new FormatException("The numeric format width is invalid.");
                width = width * 10 + specifier[index] - '0';
            }
            return width;
        }

        private static string FormatUnsigned(ulong value, bool upper, int width)
        {
            char[] digits = new char[32];
            int position = digits.Length;
            do
            {
                int digit = (int)(value & 0xf);
                digits[--position] = (char)(digit < 10 ? '0' + digit : (upper ? 'A' : 'a') + digit - 10);
                value >>= 4;
            }
            while (value != 0);

            int count = digits.Length - position;
            int total = count > width ? count : width;
            char[] result = new char[total];
            int padding = total - count;
            for (int index = 0; index < padding; index++)
                result[index] = '0';
            for (int index = 0; index < count; index++)
                result[padding + index] = digits[position + index];
            return new string(result);
        }

        public static string Format(IFormatProvider provider, string format, params object[] args)
            => Format(format, args);

        public static bool IsNullOrEmpty(string value) => value == null || value.Length == 0;

        public bool StartsWith(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The value cannot be null.");
            if (value.Length > Length)
                return false;
            for (int index = 0; index < value.Length; index++)
                if (this[index] != value[index])
                    return false;
            return true;
        }

        public bool EndsWith(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The value cannot be null.");
            if (value.Length > Length)
                return false;
            int offset = Length - value.Length;
            for (int index = 0; index < value.Length; index++)
                if (this[offset + index] != value[index])
                    return false;
            return true;
        }

        public int IndexOf(char value)
        {
            for (int index = 0; index < Length; index++)
                if (this[index] == value)
                    return index;
            return -1;
        }

        public int IndexOf(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The value cannot be null.");
            if (value.Length == 0)
                return 0;
            for (int index = 0; index <= Length - value.Length; index++)
            {
                bool match = true;
                for (int offset = 0; offset < value.Length; offset++)
                    if (this[index + offset] != value[offset])
                    {
                        match = false;
                        break;
                    }
                if (match)
                    return index;
            }
            return -1;
        }

        public string Substring(int startIndex)
            => Substring(startIndex, Length - startIndex);

        public string Substring(int startIndex, int length)
        {
            if (startIndex < 0 || length < 0 || startIndex > Length - length)
                throw new ArgumentException("The string range is invalid.");
            char[] value = new char[length];
            for (int index = 0; index < length; index++)
                value[index] = this[startIndex + index];
            return new string(value);
        }

        public string Trim()
        {
            int start = 0;
            int end = Length - 1;
            while (start <= end && this[start] <= ' ')
                start++;
            while (end >= start && this[end] <= ' ')
                end--;
            return start == 0 && end == Length - 1 ? this : Substring(start, end - start + 1);
        }

        public string Replace(char oldValue, char newValue)
        {
            char[] value = new char[Length];
            for (int index = 0; index < Length; index++)
                value[index] = this[index] == oldValue ? newValue : this[index];
            return new string(value);
        }

        public string[] Split(char separator)
        {
            int count = 1;
            for (int index = 0; index < Length; index++)
                if (this[index] == separator)
                    count++;
            string[] result = new string[count];
            int start = 0;
            int part = 0;
            for (int index = 0; index <= Length; index++)
            {
                if (index != Length && this[index] != separator)
                    continue;
                result[part++] = Substring(start, index - start);
                start = index + 1;
            }
            return result;
        }

        public static string Join(string separator, string[] values)
        {
            if (values == null)
                throw new ArgumentNullException("The values cannot be null.");
            string result = Empty;
            for (int index = 0; index < values.Length; index++)
            {
                if (index != 0)
                    result = Concat(result, separator);
                result = Concat(result, values[index]);
            }
            return result;
        }
    }

    public class Exception
    {
        public string Message;
        public Exception InnerException;
        public Exception() { }
        public Exception(string message) { Message = message; }
        public Exception(string message, Exception innerException)
        {
            Message = message;
            InnerException = innerException;
        }

        public override string ToString()
        {
            string result = GetType().FullName;
            if (Message != null)
                result = string.Concat(result, ": ", Message);
            if (InnerException != null)
                result = string.Concat(result, "\n ---> ", InnerException.ToString(),
                    "\n   --- End of inner exception stack trace ---");
            return result;
        }
    }

    public class NotSupportedException : Exception
    {
        public NotSupportedException() : base("Specified method is not supported.") { }
        public NotSupportedException(string message) : base(message) { }
    }

    public class ArgumentException : Exception
    {
        public ArgumentException() : base("Value does not fall within the expected range.") { }
        public ArgumentException(string message) : base(message) { }
    }

    public class ArgumentNullException : ArgumentException
    {
        public ArgumentNullException() : base("Value cannot be null.") { }
        public ArgumentNullException(string message) : base(message) { }
    }

    public class ArgumentOutOfRangeException : ArgumentException
    {
        public ArgumentOutOfRangeException() : base("Specified argument was out of the range of valid values.") { }
        public ArgumentOutOfRangeException(string message) : base(message) { }
    }

    public class FormatException : Exception
    {
        public FormatException() : base("Input string was not in a correct format.") { }
        public FormatException(string message) : base(message) { }
    }

    public class OperationCanceledException : Exception
    {
        public OperationCanceledException() : base("The operation was canceled.") { }
        public OperationCanceledException(string message) : base(message) { }
    }

    public class IndexOutOfRangeException : Exception
    {
        public IndexOutOfRangeException() : base("Index was outside the bounds of the array.") { }
        public IndexOutOfRangeException(string message) : base(message) { }
    }

    public class InvalidProgramException : Exception
    {
        public InvalidProgramException() : base("Common Language Runtime detected an invalid program.") { }
        public InvalidProgramException(string message) : base(message) { }
    }

    public class ArithmeticException : Exception
    {
        public ArithmeticException() : base("Overflow or underflow in the arithmetic operation.") { }
        public ArithmeticException(string message) : base(message) { }
    }
    public class OverflowException : ArithmeticException
    {
        public OverflowException() : base("Arithmetic operation resulted in an overflow.") { }
        public OverflowException(string message) : base(message) { }
    }

    public class DivideByZeroException : ArithmeticException
    {
        public DivideByZeroException() : base("Attempted to divide by zero.") { }
        public DivideByZeroException(string message) : base(message) { }
    }

    public class TypeLoadException : Exception
    {
        public TypeLoadException() : base("Failure has occurred while loading a type.") { }
        public TypeLoadException(string message) : base(message) { }
    }

    public class InvalidCastException : Exception
    {
        public InvalidCastException() : base("Specified cast is not valid.") { }
        public InvalidCastException(string message) : base(message) { }
    }

    public class NullReferenceException : Exception
    {
        public NullReferenceException() : base("Object reference not set to an instance of an object.") { }
        public NullReferenceException(string message) : base(message) { }
    }

    public class InvalidOperationException : Exception
    {
        public InvalidOperationException() : base("The operation is not valid due to the current state of the object.") { }
        public InvalidOperationException(string message) : base(message) { }
    }

    public class KeyNotFoundException : Exception
    {
        public KeyNotFoundException() : base("The given key was not present in the dictionary.") { }
        public KeyNotFoundException(string message) : base(message) { }
    }

    public class AggregateException : Exception
    {
        private Exception[] _innerExceptions;
        public AggregateException(Exception[] innerExceptions)
        {
            _innerExceptions = innerExceptions;
        }
        public Exception[] InnerExceptions => _innerExceptions;
    }

    public interface IDisposable
    {
        void Dispose();
    }

    public delegate void Action();
    public delegate void Action<T>(T arg);
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate TResult Func<TResult>();
    public delegate TResult Func<T, TResult>(T arg);
    public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);
    public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public class Delegate
    {
        private IntPtr _function;
        private object _target;
        private Delegate _next;

        public static Delegate Combine(Delegate left, Delegate right)
        {
            if (left == null)
                return right;
            if (right == null)
                return left;
            Delegate current = left;
            while (current._next != null)
                current = current._next;
            current._next = right;
            return left;
        }

        public static Delegate Remove(Delegate source, Delegate value)
        {
            if (source == null || value == null)
                return source;
            if (ReferenceEquals(source, value))
                return source._next;
            Delegate current = source;
            while (current._next != null)
            {
                if (ReferenceEquals(current._next, value))
                {
                    current._next = current._next._next;
                    break;
                }
                current = current._next;
            }
            return source;
        }
    }
    public class MulticastDelegate : Delegate { }

    public sealed class Type
    {
        public string Name;
        public string Namespace;
        public string FullName;
        internal int RuntimeTypeId;
        internal int[] ObjectReferenceOffsets;
        internal int[] ArrayElementReferenceOffsets;
        internal string[] EnumNames;
        internal ulong[] EnumValues;
        internal bool IsFlagsEnum;
        internal bool IsSignedEnum;
        internal Delegate Factory;

        public static Type GetTypeFromHandle(RuntimeTypeHandle handle) => handle.Type;
        public static bool operator ==(Type left, Type right) => ReferenceEquals(left, right);
        public static bool operator !=(Type left, Type right) => !ReferenceEquals(left, right);
        public override bool Equals(object other) => ReferenceEquals(this, other);
        public override int GetHashCode() => RuntimeTypeId;
    }

    public static class Activator
    {
        public static T CreateInstance<T>()
        {
            Func<T> factory = (Func<T>)typeof(T).Factory;
            if (factory == null)
                throw new InvalidOperationException("The type cannot be created.");
            return factory();
        }
    }

    public struct RuntimeTypeHandle
    {
        internal Type Type;
    }

    public struct RuntimeMethodHandle { }
    internal unsafe struct VariableArgument
    {
        internal void* Value;
        internal RuntimeTypeHandle Type;
    }

    public ref struct RuntimeArgumentHandle
    {
        internal ByReference<VariableArgument> Arguments;
        internal int Count;
    }

    public unsafe ref struct ArgIterator
    {
        private ByReference<VariableArgument> _current;
        private int _remaining;

        public ArgIterator(RuntimeArgumentHandle handle)
        {
            _current = handle.Arguments;
            _remaining = handle.Count;
        }

        public int GetRemainingCount() => _remaining;

        public RuntimeTypeHandle GetNextArgType()
        {
            if (_remaining == 0)
                throw new InvalidOperationException("The argument list is empty.");
            return _current.Value.Type;
        }

        public TypedReference GetNextArg()
        {
            if (_remaining == 0)
                throw new InvalidOperationException("The argument list is empty.");
            TypedReference result = default;
            result.Value = _current.Value.Value;
            result.Type = _current.Value.Type;
            _current = new ByReference<VariableArgument>(ref _current.ElementAt(1));
            _remaining--;
            return result;
        }
    }

    public unsafe struct TypedReference
    {
        internal void* Value;
        internal RuntimeTypeHandle Type;
    }

    public unsafe struct RuntimeFieldHandle
    {
        internal byte* Data;
        internal int Length;
    }

    public class Attribute { }
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class FlagsAttribute : Attribute { }
    [Flags]
    public enum AttributeTargets
    {
        Assembly = 1,
        Module = 2,
        Class = 4,
        Struct = 8,
        Enum = 16,
        Constructor = 32,
        Method = 64,
        Property = 128,
        Field = 256,
        Event = 512,
        Interface = 1024,
        Parameter = 2048,
        Delegate = 4096,
        ReturnValue = 8192,
        GenericParameter = 16384,
        All = 32767
    }
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public sealed class AttributeUsageAttribute : Attribute
    {
        private readonly AttributeTargets _validOn;

        public AttributeUsageAttribute(AttributeTargets validOn)
        {
            _validOn = validOn;
            Inherited = true;
        }

        public AttributeTargets ValidOn => _validOn;
        public bool AllowMultiple { get; set; }
        public bool Inherited { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class ParamArrayAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public sealed class ObsoleteAttribute : Attribute
    {
        public ObsoleteAttribute() { }
        public ObsoleteAttribute(string message) { Message = message; }
        public ObsoleteAttribute(string message, bool error)
        {
            Message = message;
            IsError = error;
        }

        public string Message { get; }
        public bool IsError { get; }
        public string DiagnosticId { get; set; }
        public string UrlFormat { get; set; }
    }
    public static partial class Console
    {
        [DllImport("*")]
        public static extern void Write(ByReference<char> value);
        [DllImport("*")]
        public static extern void WriteLine(ByReference<char> value);
        [DllImport("*")]
        public static extern void Write(ByReference<byte> value);
        [DllImport("*")]
        public static extern void WriteLine(ByReference<byte> value);
    }
}

namespace System.Runtime.InteropServices
{
    /// <summary>
    /// Imports a native function. By default, the native function name is the C# method name.
    /// Same-name overloads imported from "*" use their full method names. Set <see cref="EntryPoint"/> to choose another name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DllImportAttribute : Attribute
    {
        public DllImportAttribute(string dllName) { }
        public string EntryPoint { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public Type[] CallConvs { get; set; }
        public string EntryPoint { get; set; }
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class InAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class OutAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class StructLayoutAttribute : Attribute
    {
        public StructLayoutAttribute(LayoutKind layoutKind) { Value = layoutKind; }
        public LayoutKind Value { get; }
        public int Pack;
        public int Size;
    }
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FieldOffsetAttribute : Attribute
    {
        public FieldOffsetAttribute(int value) { Value = value; }
        public int Value { get; }
    }
    public enum LayoutKind
    {
        Sequential = 0,
        Explicit = 2,
        Auto = 3
    }
    public enum CallingConvention
    {
        Winapi = 1,
        Cdecl = 2,
        StdCall = 3,
        ThisCall = 4,
        FastCall = 5
    }

    public enum UnmanagedType
    {
        Bool = 2,
        I1 = 3,
        U1 = 4,
        I2 = 5,
        U2 = 6,
        I4 = 7,
        U4 = 8,
        I8 = 9,
        U8 = 10,
        R4 = 11,
        R8 = 12,
        LPStr = 20,
        LPWStr = 21,
        LPTStr = 22,
        ByValTStr = 23,
        IUnknown = 25,
        Struct = 27,
        Interface = 28,
        SafeArray = 29,
        ByValArray = 30,
        SysInt = 31,
        SysUInt = 32,
        VBByRefStr = 34,
        AnsiBStr = 35,
        TBStr = 36,
        VariantBool = 37,
        FunctionPtr = 38,
        AsAny = 40,
        LPArray = 42,
        LPStruct = 43,
        CustomMarshaler = 44,
        Error = 45,
        IInspectable = 46,
        HString = 47,
        LPUTF8Str = 48,
    }

    public static unsafe class Marshal
    {
        [DllImport("*", EntryPoint = "malloc")]
        public static extern IntPtr AllocHGlobal(nint byteCount);

        [DllImport("*", EntryPoint = "free")]
        public static extern void FreeHGlobal(IntPtr value);
    }
}

namespace System.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RuntimeExportAttribute : Attribute
    {
        public RuntimeExportAttribute(string entry) { }
    }

}

namespace System.Runtime.CompilerServices
{
    public sealed class CompilerGeneratedAttribute : Attribute { }
    public sealed class RequiredMemberAttribute : Attribute { }
    public sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName) { FeatureName = featureName; }
        public string FeatureName { get; }
    }
    public sealed class IsExternalInit { }
    public sealed class IsVolatile { }
    public sealed class IsByRefLikeAttribute : Attribute { }
    public sealed class IsReadOnlyAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PreserveBaseOverridesAttribute : Attribute { }
    public static class RuntimeFeature
    {
        public const string ByRefFields = nameof(ByRefFields);
        public const string CovariantReturnsOfClasses = nameof(CovariantReturnsOfClasses);
        public const string DefaultImplementationsOfInterfaces = nameof(DefaultImplementationsOfInterfaces);
        public const string UnmanagedSignatureCallingConvention = nameof(UnmanagedSignatureCallingConvention);
        public const string VirtualStaticsInInterfaces = nameof(VirtualStaticsInInterfaces);
    }

    public sealed class CallConvCdecl { }
    public sealed class CallConvFastcall { }
    public sealed class CallConvStdcall { }
    public sealed class CallConvSuppressGCTransition { }
    public sealed class CallConvThiscall { }

    public static unsafe class RuntimeHelpers
    {
        public static void InitializeArray(Array array, RuntimeFieldHandle fieldHandle)
        {
            if (array == null || fieldHandle.Data == null || fieldHandle.Length == 0)
                return;
            Unsafe.CopyBlock(array.m_pData, fieldHandle.Data, (uint)fieldHandle.Length);
        }

        public static ReadOnlySpan<T> CreateSpan<T>(RuntimeFieldHandle fieldHandle)
        {
            return new ReadOnlySpan<T>(fieldHandle.Data, fieldHandle.Length / sizeof(T));
        }
    }

    public static unsafe class Unsafe
    {
        [DllImport("*", EntryPoint = "memcpy")]
        public static extern void CopyBlock(void* destination, void* source, uint byteCount);

        [DllImport("*", EntryPoint = "memset")]
        public static extern void InitBlock(void* startAddress, byte value, uint byteCount);
    }

    public sealed class ExtensionAttribute : Attribute { }
    public class StateMachineAttribute : Attribute
    {
        public StateMachineAttribute(Type type) { }
    }
    public sealed class AsyncStateMachineAttribute : StateMachineAttribute
    {
        public AsyncStateMachineAttribute(Type type) : base(type) { }
    }
    public sealed class AsyncMethodBuilderAttribute : Attribute
    {
        public AsyncMethodBuilderAttribute(Type type) { }
    }
    public sealed class IteratorStateMachineAttribute : StateMachineAttribute
    {
        public IteratorStateMachineAttribute(Type type) : base(type) { }
    }

    public interface IAsyncStateMachine
    {
        void MoveNext();
        void SetStateMachine(IAsyncStateMachine stateMachine);
    }

    public struct AsyncTaskMethodBuilder
    {
        private Threading.Tasks.Task _task;
        public static AsyncTaskMethodBuilder Create() => new AsyncTaskMethodBuilder { _task = new Threading.Tasks.Task() };
        public Threading.Tasks.Task Task => _task;
        public void SetStateMachine(IAsyncStateMachine stateMachine) { }
        public void SetResult() { _task?.SetResult(); }
        public void SetException(Exception exception) { _task?.SetException(exception); }
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => stateMachine.MoveNext();
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            AsyncStateMachineRunner<TStateMachine> runner = new AsyncStateMachineRunner<TStateMachine>(stateMachine);
            awaiter.UnsafeOnCompleted(runner.MoveNext);
        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            AsyncStateMachineRunner<TStateMachine> runner = new AsyncStateMachineRunner<TStateMachine>(stateMachine);
            awaiter.OnCompleted(runner.MoveNext);
        }
    }

    public struct AsyncTaskMethodBuilder<TResult>
    {
        private Threading.Tasks.Task<TResult> _task;
        public static AsyncTaskMethodBuilder<TResult> Create()
            => new AsyncTaskMethodBuilder<TResult> { _task = new Threading.Tasks.Task<TResult>() };
        public Threading.Tasks.Task<TResult> Task => _task;
        public void SetStateMachine(IAsyncStateMachine stateMachine) { }
        public void SetResult(TResult result) { _task?.SetResult(result); }
        public void SetException(Exception exception) { _task?.SetException(exception); }
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
            => stateMachine.MoveNext();
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            AsyncStateMachineRunner<TStateMachine> runner = new AsyncStateMachineRunner<TStateMachine>(stateMachine);
            awaiter.UnsafeOnCompleted(runner.MoveNext);
        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            AsyncStateMachineRunner<TStateMachine> runner = new AsyncStateMachineRunner<TStateMachine>(stateMachine);
            awaiter.OnCompleted(runner.MoveNext);
        }
    }

    internal sealed class AsyncStateMachineRunner<TStateMachine> where TStateMachine : IAsyncStateMachine
    {
        private TStateMachine _stateMachine;

        internal AsyncStateMachineRunner(TStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        internal void MoveNext() => _stateMachine.MoveNext();
    }

    public interface INotifyCompletion
    {
        void OnCompleted(Action continuation);
    }

    public interface ICriticalNotifyCompletion : INotifyCompletion
    {
        void UnsafeOnCompleted(Action continuation);
    }
}

namespace System.Runtime
{
    internal unsafe struct GCRoot
    {
        public Object** Address;
        public Type Type;
    }

    internal unsafe struct GCFrame
    {
        public GCFrame* Previous;
        public GCRoot* Roots;
        public int RootCount;
    }

    internal unsafe struct GCAllocation
    {
        public GCAllocation* Next;
        public GCAllocation* MarkNext;
        public nuint Size;
        public int Marked;
    }

    internal unsafe struct GCStaticRoot
    {
        public GCStaticRoot* Next;
        public Object** Address;
        public Type Type;
    }

    internal static unsafe class GCHeap
    {
        private static GCAllocation* s_allocations;
        private static GCAllocation* s_pendingMarks;
        private static GCFrame* s_frames;
        private static GCStaticRoot* s_staticRoots;
        private static nuint s_allocatedBytes;
        private static nuint s_collectionThreshold;

        public static Object* Allocate(nuint size)
        {
            if (s_allocatedBytes >= s_collectionThreshold ||
                size > s_collectionThreshold - s_allocatedBytes)
                Collect();

            nuint headerSize = (nuint)sizeof(GCAllocation);
            if (size > ~(nuint)0 - headerSize)
                ExceptionRuntime.Abort();
            nuint allocationSize = size + headerSize;
            GCAllocation* allocation = (GCAllocation*)(void*)Marshal.AllocHGlobal((nint)allocationSize);
            if (allocation == null)
                ExceptionRuntime.Abort();
            byte* clearAddress = (byte*)allocation;
            nuint clearRemaining = allocationSize;
            while (clearRemaining != 0)
            {
                uint clearLength = clearRemaining > uint.MaxValue ? uint.MaxValue : (uint)clearRemaining;
                Unsafe.InitBlock(clearAddress, 0, clearLength);
                clearAddress += clearLength;
                clearRemaining -= clearLength;
            }
            allocation->Next = s_allocations;
            allocation->Size = size;
            s_allocations = allocation;
            s_allocatedBytes += size;
            return (Object*)((byte*)allocation + sizeof(GCAllocation));
        }

        public static void Push(GCFrame* frame, GCRoot* roots, int rootCount)
        {
            frame->Previous = s_frames;
            frame->Roots = roots;
            frame->RootCount = rootCount;
            s_frames = frame;
        }

        public static void Pop(GCFrame* frame)
        {
            s_frames = frame->Previous;
        }

        public static GCFrame* GetTopFrame() => s_frames;

        public static void UnwindTo(GCFrame* frame) => s_frames = frame;

        public static int Collect()
        {
            for (GCStaticRoot* root = s_staticRoots; root != null; root = root->Next)
                MarkRoot(root->Address, root->Type);
            for (GCFrame* frame = GetTopFrame(); frame != null; frame = frame->Previous)
                for (int index = 0; index < frame->RootCount; index++)
                    MarkRoot(frame->Roots[index].Address, frame->Roots[index].Type);
            while (s_pendingMarks != null)
            {
                GCAllocation* pending = s_pendingMarks;
                s_pendingMarks = pending->MarkNext;
                pending->MarkNext = null;
                Object* objectAddress = (Object*)((byte*)pending + sizeof(GCAllocation));
                Object objectValue = *(Object*)&objectAddress;
                Type type = objectValue.m_pType;
                if (type != null)
                    ScanObject(objectAddress, type);
            }

            GCAllocation* previous = null;
            GCAllocation* allocation = s_allocations;
            nuint liveBytes = 0;
            int collected = 0;
            while (allocation != null)
            {
                if (allocation->Marked != 0)
                {
                    allocation->Marked = 0;
                    liveBytes += allocation->Size;
                    previous = allocation;
                    allocation = allocation->Next;
                }
                else
                {
                    GCAllocation* next = allocation->Next;
                    if (previous == null)
                        s_allocations = next;
                    else
                        previous->Next = next;
                    Marshal.FreeHGlobal((IntPtr)allocation);
                    collected++;
                    allocation = next;
                }
            }
            s_allocatedBytes = liveBytes;
            if (liveBytes > s_collectionThreshold)
            {
                nuint expandedThreshold = liveBytes * 2;
                s_collectionThreshold = expandedThreshold > liveBytes ? expandedThreshold : liveBytes;
            }
            return collected;
        }

        private static void MarkRoot(Object** address, Type type)
        {
            if (address == null)
                return;
            if (type == null)
                MarkObject(*address);
            else
                ScanValue((byte*)address, type);
        }

        private static void MarkObject(Object* value)
        {
            if (value == null)
                return;
            GCAllocation* allocation = s_allocations;
            while (allocation != null)
            {
                byte* start = (byte*)allocation + sizeof(GCAllocation);
                if ((byte*)value >= start && (byte*)value < start + allocation->Size)
                    break;
                allocation = allocation->Next;
            }
            if (allocation == null)
                return;
            if (allocation->Marked != 0)
                return;
            allocation->Marked = 1;
            allocation->MarkNext = s_pendingMarks;
            s_pendingMarks = allocation;
        }

        private static void ScanValue(byte* value, Type type)
        {
            int[] objectReferences = type.ObjectReferenceOffsets;
            for (int index = 0; index < objectReferences.Length; index++)
                MarkObject(*(Object**)(value + objectReferences[index]));
        }

        private static void ScanObject(Object* objectAddress, Type type)
        {
            ScanValue((byte*)objectAddress + sizeof(nuint), type);

            int[] elementReferences = type.ArrayElementReferenceOffsets;
            if (elementReferences.Length == 0)
                return;

            Array array = (Array)(*(Object*)&objectAddress);
            int length = array.Length;
            int elementSize = array.m_elementSize;

            byte* elements = array.m_pData;
            for (int elementIndex = 0; elementIndex < length; elementIndex++)
            {
                byte* element = elements + (nint)elementIndex * elementSize;
                for (int index = 0; index < elementReferences.Length; index++)
                    MarkObject(*(Object**)(element + elementReferences[index]));
            }
        }

    }

    internal struct StackPointer { }

    [StructLayout(LayoutKind.Sequential, Size = 256)]
    internal struct JumpBuffer { }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ExceptionFrame
    {
        public ExceptionFrame* Previous;
        public JumpBuffer* Buffer;
        public GCFrame* GCFrame;
    }

    internal static unsafe class ExceptionRuntime
    {
        private static ExceptionFrame* _top;
        private static Exception _current;

        public static void Push(ExceptionFrame* frame, JumpBuffer* buffer)
        {
            frame->Previous = _top;
            frame->Buffer = buffer;
            frame->GCFrame = GCHeap.GetTopFrame();
            _top = frame;
        }

        public static void Pop(ExceptionFrame* frame)
        {
            if (_top == frame)
                _top = frame->Previous;
        }

        public static JumpBuffer* GetBuffer(ExceptionFrame* frame) => frame->Buffer;

        public static ExceptionFrame* GetTop() => _top;

        public static Exception GetCurrent() => _current;

        public static void SetCurrent(Exception exception) => _current = exception;

        public static void Restore(ExceptionFrame* top, Exception current)
        {
            _top = top;
            _current = current;
        }

        [DllImport("*", EntryPoint = "setjmp")]
        public static extern int SetJump(JumpBuffer* buffer, StackPointer* stackPointer);
        [DllImport("*", EntryPoint = "longjmp")]
        public static extern void LongJump(JumpBuffer* buffer, int value);
        [DllImport("*", EntryPoint = "abort")]
        public static extern void Abort();

        public static void Throw(Exception exception)
        {
            // The CLI specifies that throwing a null reference produces a NullReferenceException.
            if (exception == null)
                exception = new NullReferenceException();
            SetCurrent(exception);
            ExceptionFrame* top = GetTop();
            if (top == null)
            {
                Console.WriteLine("Unhandled exception. " + _current.ToString());
                Abort();
            }
            else
            {
                GCHeap.UnwindTo(top->GCFrame);
                LongJump(top->Buffer, 1);
            }
        }
    }
}

namespace System.Reflection
{
    public sealed class DefaultMemberAttribute : Attribute
    {
        public DefaultMemberAttribute(string memberName) { }
    }
}

namespace System.Collections
{
    public interface IEnumerator
    {
        bool MoveNext();
        object Current { get; }
        void Reset();
    }

    public interface IEnumerable
    {
        IEnumerator GetEnumerator();
    }
}

namespace System.Collections.Generic
{
    public delegate bool Predicate<T>(T value);
    public delegate int Comparison<T>(T left, T right);

    public interface ICollection<T> : IEnumerable<T>
    {
        int Count { get; }
        bool IsReadOnly { get; }
        void Add(T item);
        void Clear();
        bool Contains(T item);
        void CopyTo(T[] array, int arrayIndex);
        bool Remove(T item);
    }

    public interface IReadOnlyCollection<out T> : IEnumerable<T>
    {
        int Count { get; }
    }

    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>
    {
        T this[int index] { get; }
    }

    public interface IList<T> : ICollection<T>
    {
        T this[int index] { get; set; }
        int IndexOf(T item);
        void Insert(int index, T item);
        void RemoveAt(int index);
    }

    public interface IComparer<in T>
    {
        int Compare(T left, T right);
    }

    public interface IEqualityComparer<in T>
    {
        bool Equals(T left, T right);
        int GetHashCode(T value);
    }

    public struct KeyValuePair<TKey, TValue>
    {
        private TKey _key;
        private TValue _value;
        public KeyValuePair(TKey key, TValue value) { _key = key; _value = value; }
        public TKey Key => _key;
        public TValue Value => _value;
    }

    public interface IDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
    {
        ICollection<TKey> Keys { get; }
        ICollection<TValue> Values { get; }
        TValue this[TKey key] { get; set; }
        void Add(TKey key, TValue value);
        bool ContainsKey(TKey key);
        bool Remove(TKey key);
        bool TryGetValue(TKey key, out TValue value);
    }

    public interface ISet<T> : ICollection<T>
    {
        new bool Add(T item);
        void ExceptWith(IEnumerable<T> other);
        void IntersectWith(IEnumerable<T> other);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
        void SymmetricExceptWith(IEnumerable<T> other);
        void UnionWith(IEnumerable<T> other);
    }

    public abstract class Comparer<T> : IComparer<T>
    {
        public static Comparer<T> Default => new DefaultComparer();
        public abstract int Compare(T left, T right);

        private sealed class DefaultComparer : Comparer<T>
        {
            public override int Compare(T left, T right)
            {
                if (ReferenceEquals(left, right))
                    return 0;
                if (left == null)
                    return -1;
                if (right == null)
                    return 1;
                if (left is IComparable<T> generic)
                    return generic.CompareTo(right);
                if (left is IComparable comparable)
                    return comparable.CompareTo(right);
                throw new InvalidOperationException("The values cannot be compared.");
            }
        }
    }

    public abstract class EqualityComparer<T> : IEqualityComparer<T>
    {
        public static EqualityComparer<T> Default => new DefaultEqualityComparer();
        public abstract bool Equals(T left, T right);
        public abstract int GetHashCode(T value);

        private sealed class DefaultEqualityComparer : EqualityComparer<T>
        {
            public override bool Equals(T left, T right) => Object.Equals(left, right);
            public override int GetHashCode(T value) => value == null ? 0 : value.GetHashCode();
        }
    }

    public interface IEnumerator<out T> : IDisposable, IEnumerator
    {
        new T Current { get; }
    }

    public interface IEnumerable<out T> : IEnumerable
    {
        new IEnumerator<T> GetEnumerator();
    }

    public class List<T> : IList<T>, IReadOnlyList<T>
    {
        private T[] _items;
        private int _count;

        public List()
        {
            _items = new T[0];
        }

        public List(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException("The list capacity cannot be negative.");
            _items = new T[capacity];
        }

        public List(T[] values)
        {
            if (values == null)
                throw new ArgumentNullException("The source items cannot be null.");
            _items = new T[values.Length];
            for (int index = 0; index < values.Length; index++)
                _items[index] = values[index];
            _count = values.Length;
        }

        public List(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException("The source items cannot be null.");
            _items = new T[0];
            AddRange(values);
        }

        public int Count => _count;
        public bool IsReadOnly => false;
        public int Capacity
        {
            get => _items.Length;
            set
            {
                if (value < _count)
                    throw new ArgumentException("The list capacity cannot be less than Count.");
                if (value != _items.Length)
                    Resize(value);
            }
        }
        public T this[int index]
        {
            get
            {
                ValidateIndex(index);
                return _items[index];
            }
            set
            {
                ValidateIndex(index);
                _items[index] = value;
            }
        }

        public void Add(T value)
        {
            EnsureCapacity(_count + 1);
            _items[_count++] = value;
        }

        public void AddRange(T[] values)
        {
            if (values == null)
                throw new ArgumentNullException("The source items cannot be null.");
            EnsureCapacity(_count + values.Length);
            for (int index = 0; index < values.Length; index++)
                _items[_count + index] = values[index];
            _count += values.Length;
        }

        public void AddRange(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException("The source items cannot be null.");
            foreach (T value in values)
                Add(value);
        }

        public void Insert(int index, T value)
        {
            if ((uint)index > (uint)_count)
                throw new ArgumentException("The insertion index is outside the list.");
            EnsureCapacity(_count + 1);
            for (int i = _count; i > index; i--)
                _items[i] = _items[i - 1];
            _items[index] = value;
            _count++;
        }

        public void RemoveAt(int index)
        {
            ValidateIndex(index);
            _count--;
            for (int i = index; i < _count; i++)
                _items[i] = _items[i + 1];
            _items[_count] = default;
        }

        public int IndexOf(T value)
        {
            for (int index = 0; index < _count; index++)
                if ((object)_items[index] == (object)value)
                    return index;
            return -1;
        }

        public bool Contains(T value) => IndexOf(value) >= 0;

        public bool Remove(T value)
        {
            int index = IndexOf(value);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The destination array cannot be null.");
            if (arrayIndex < 0 || arrayIndex > array.Length - _count)
                throw new ArgumentException("The destination array range is invalid.");
            for (int index = 0; index < _count; index++)
                array[arrayIndex + index] = _items[index];
        }

        public int RemoveAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("The match predicate cannot be null.");
            int write = 0;
            for (int index = 0; index < _count; index++)
                if (!match(_items[index]))
                    _items[write++] = _items[index];
            for (int index = write; index < _count; index++)
                _items[index] = default;
            int removed = _count - write;
            _count = write;
            return removed;
        }

        public T Find(Predicate<T> match)
        {
            int index = FindIndex(match);
            return index < 0 ? default : _items[index];
        }

        public int FindIndex(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("The match predicate cannot be null.");
            for (int index = 0; index < _count; index++)
                if (match(_items[index]))
                    return index;
            return -1;
        }

        public void ForEach(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("The action cannot be null.");
            for (int index = 0; index < _count; index++)
                action(_items[index]);
        }

        public void Clear()
        {
            for (int index = 0; index < _count; index++)
                _items[index] = default;
            _count = 0;
        }

        public T[] ToArray()
        {
            T[] values = new T[_count];
            for (int index = 0; index < _count; index++)
                values[index] = _items[index];
            return values;
        }

        public void Reverse()
        {
            int left = 0;
            int right = _count - 1;
            while (left < right)
            {
                T value = _items[left];
                _items[left++] = _items[right];
                _items[right--] = value;
            }
        }

        public void Sort()
        {
            Sort((IComparer<T>)null);
        }

        public void Sort(IComparer<T> comparer)
        {
            if (comparer == null)
                return;

            for (int index = 1; index < _count; index++)
            {
                T value = _items[index];
                int position = index - 1;
                while (position >= 0 && comparer.Compare(_items[position], value) > 0)
                {
                    _items[position + 1] = _items[position];
                    position--;
                }
                _items[position + 1] = value;
            }
        }
        public void Sort(Comparison<T> comparison)
        {
            Sort((IComparer<T>)null);
        }

        public List<T> GetRange(int index, int count)
        {
            if (index < 0 || count < 0 || index > _count - count)
                throw new ArgumentException("The list range is invalid.");
            List<T> result = new List<T>(count);
            for (int i = 0; i < count; i++)
                result.Add(_items[index + i]);
            return result;
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void EnsureCapacity(int minimum)
        {
            if (_items.Length >= minimum)
                return;
            int capacity = _items.Length == 0 ? 4 : _items.Length * 2;
            if (capacity < minimum)
                capacity = minimum;
            Resize(capacity);
        }

        private void Resize(int capacity)
        {
            T[] values = new T[capacity];
            for (int index = 0; index < _count; index++)
                values[index] = _items[index];
            _items = values;
        }

        private void ValidateIndex(int index)
        {
            if ((uint)index >= (uint)_count)
                throw new IndexOutOfRangeException("The list index is outside the collection.");
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly List<T> _list;
            private int _index;
            private T _current;
            internal Enumerator(List<T> list) { _list = list; _index = 0; _current = default; }
            public T Current => _current;
            object IEnumerator.Current => Current;
            public bool MoveNext()
            {
                if (_index >= _list._count)
                {
                    _current = default;
                    return false;
                }
                _current = _list._items[_index++];
                return true;
            }
            public void Reset() { _index = 0; _current = default; }
            public void Dispose() { }
        }
    }

    public sealed class ComparisonComparer<T> : Comparer<T>
    {
        private readonly Comparison<T> _comparison;
        public ComparisonComparer(Comparison<T> comparison) { _comparison = comparison; }
        public override int Compare(T left, T right) => _comparison(left, right);
    }

    public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private KeyValuePair<TKey, TValue>[] _items;
        private int _count;
        private readonly IEqualityComparer<TKey> _comparer;

        public Dictionary() : this(0, null) { }
        public Dictionary(int capacity) : this(capacity, null) { }
        public Dictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }
        public Dictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            if (capacity < 0)
                throw new ArgumentException("The dictionary capacity cannot be negative.");
            _items = new KeyValuePair<TKey, TValue>[capacity];
            _comparer = comparer ?? new DictionaryComparer<TKey>();
        }

        public int Count => _count;
        public bool IsReadOnly => false;

        public ICollection<TKey> Keys
        {
            get
            {
                List<TKey> result = new List<TKey>(_count);
                for (int index = 0; index < _count; index++)
                    result.Add(_items[index].Key);
                return result;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                List<TValue> result = new List<TValue>(_count);
                for (int index = 0; index < _count; index++)
                    result.Add(_items[index].Value);
                return result;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                int index = FindIndex(key);
                if (index < 0)
                    throw new InvalidOperationException("The requested key was not found in the dictionary.");
                return _items[index].Value;
            }
            set
            {
                int index = FindIndex(key);
                if (index < 0)
                {
                    Add(key, value);
                    return;
                }
                _items[index] = new KeyValuePair<TKey, TValue>(key, value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
                throw new ArgumentException("An item with the same key has already been added.");
            EnsureCapacity(_count + 1);
            _items[_count++] = new KeyValuePair<TKey, TValue>(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public bool TryAdd(TKey key, TValue value)
        {
            if (ContainsKey(key))
                return false;
            Add(key, value);
            return true;
        }

        public bool ContainsKey(TKey key) => FindIndex(key) >= 0;

        public bool ContainsValue(TValue value)
        {
            for (int index = 0; index < _count; index++)
                if (Equals(_items[index].Value, value))
                    return true;
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = FindIndex(key);
            if (index >= 0)
            {
                value = _items[index].Value;
                return true;
            }
            value = default;
            return false;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
            => TryGetValue(item.Key, out TValue value) && Equals(value, item.Value);

        public bool Remove(KeyValuePair<TKey, TValue> item) => Contains(item) && Remove(item.Key);

        public bool Remove(TKey key)
        {
            int index = FindIndex(key);
            if (index < 0)
                return false;
            _count--;
            for (int i = index; i < _count; i++)
                _items[i] = _items[i + 1];
            _items[_count] = default;
            return true;
        }

        public void Clear()
        {
            for (int index = 0; index < _count; index++)
                _items[index] = default;
            _count = 0;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("The destination array cannot be null.");
            if (arrayIndex < 0 || arrayIndex > array.Length - _count)
                throw new ArgumentException("The destination array range is invalid.");
            for (int index = 0; index < _count; index++)
                array[arrayIndex + index] = _items[index];
        }

        public Enumerator GetEnumerator() => new Enumerator(this);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private int FindIndex(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException("The dictionary key cannot be null.");
            for (int index = 0; index < _count; index++)
                if (_comparer.Equals(_items[index].Key, key))
                    return index;
            return -1;
        }

        private void EnsureCapacity(int minimum)
        {
            if (_items.Length >= minimum)
                return;
            int capacity = _items.Length == 0 ? 4 : _items.Length * 2;
            if (capacity < minimum)
                capacity = minimum;
            KeyValuePair<TKey, TValue>[] values = new KeyValuePair<TKey, TValue>[capacity];
            for (int index = 0; index < _count; index++)
                values[index] = _items[index];
            _items = values;
        }

        private sealed class DictionaryComparer<T> : EqualityComparer<T>
        {
            public override bool Equals(T left, T right) => Object.Equals(left, right);
            public override int GetHashCode(T value) => value == null ? 0 : value.GetHashCode();
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly Dictionary<TKey, TValue> _dictionary;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;
            internal Enumerator(Dictionary<TKey, TValue> dictionary)
            {
                _dictionary = dictionary;
                _index = 0;
                _current = default;
            }
            public KeyValuePair<TKey, TValue> Current => _current;
            object IEnumerator.Current => _current;
            public bool MoveNext()
            {
                if (_index >= _dictionary._count)
                {
                    _current = default;
                    return false;
                }
                _current = _dictionary._items[_index++];
                return true;
            }
            public void Reset() { _index = 0; _current = default; }
            public void Dispose() { }
        }
    }

    public class HashSet<T> : ISet<T>
    {
        private readonly List<T> _items;
        private readonly IEqualityComparer<T> _comparer;

        public HashSet() : this((IEqualityComparer<T>)null) { }

        public HashSet(IEqualityComparer<T> comparer)
        {
            _items = new List<T>();
            _comparer = comparer ?? new SetComparer<T>();
        }

        public HashSet(IEnumerable<T> values) : this(values, null) { }

        public HashSet(IEnumerable<T> values, IEqualityComparer<T> comparer) : this(comparer)
        {
            if (values == null)
                throw new ArgumentNullException("The source collection cannot be null.");
            UnionWith(values);
        }

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public bool Add(T value)
        {
            if (Contains(value))
                return false;
            _items.Add(value);
            return true;
        }

        void ICollection<T>.Add(T value) => Add(value);

        public bool Contains(T value)
        {
            for (int index = 0; index < _items.Count; index++)
                if (_comparer.Equals(_items[index], value))
                    return true;
            return false;
        }

        public bool Remove(T value)
        {
            for (int index = 0; index < _items.Count; index++)
                if (_comparer.Equals(_items[index], value))
                {
                    _items.RemoveAt(index);
                    return true;
                }
            return false;
        }

        public void Clear() => _items.Clear();
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public T[] ToArray() => _items.ToArray();
        public void UnionWith(IEnumerable<T> other) { foreach (T value in other) Add(value); }
        public void ExceptWith(IEnumerable<T> other) { foreach (T value in other) Remove(value); }

        public void IntersectWith(IEnumerable<T> other)
        {
            HashSet<T> set = new HashSet<T>(other, _comparer);
            for (int index = _items.Count - 1; index >= 0; index--)
                if (!set.Contains(_items[index]))
                    _items.RemoveAt(index);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            HashSet<T> set = new HashSet<T>(other, _comparer);
            foreach (T value in set)
                if (!Remove(value))
                    Add(value);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            HashSet<T> set = new HashSet<T>(other, _comparer);
            for (int index = 0; index < Count; index++)
                if (!set.Contains(_items[index]))
                    return false;
            return true;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            HashSet<T> set = new HashSet<T>(other, _comparer);
            return Count < set.Count && IsSubsetOf(set);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            foreach (T value in other)
                if (!Contains(value))
                    return false;
            return true;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            HashSet<T> set = new HashSet<T>(other, _comparer);
            return Count > set.Count && IsSupersetOf(set);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            foreach (T value in other)
                if (Contains(value))
                    return true;
            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            HashSet<T> set = new HashSet<T>(other, _comparer);
            return Count == set.Count && IsSubsetOf(set);
        }

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class SetComparer<TValue> : EqualityComparer<TValue>
        {
            public override bool Equals(TValue left, TValue right) => Object.Equals(left, right);
            public override int GetHashCode(TValue value) => value == null ? 0 : value.GetHashCode();
        }
    }

    public class Queue<T> : IEnumerable<T>
    {
        private readonly List<T> _items = new List<T>();
        public int Count => _items.Count;
        public void Enqueue(T value) => _items.Add(value);
        public T Dequeue()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("The queue is empty.");
            T value = _items[0];
            _items.RemoveAt(0);
            return value;
        }
        public T Peek()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("The queue is empty.");
            return _items[0];
        }
        public bool TryDequeue(out T value)
        {
            if (_items.Count == 0) { value = default; return false; }
            value = Dequeue();
            return true;
        }
        public void Clear() => _items.Clear();
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Stack<T> : IEnumerable<T>
    {
        private readonly List<T> _items = new List<T>();
        public int Count => _items.Count;
        public void Push(T value) => _items.Add(value);
        public T Pop()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("The stack is empty.");
            int index = _items.Count - 1;
            T value = _items[index];
            _items.RemoveAt(index);
            return value;
        }
        public T Peek()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("The stack is empty.");
            return _items[_items.Count - 1];
        }
        public void Clear() => _items.Clear();
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            if (source == null || selector == null)
                throw new ArgumentNullException("The source and selector cannot be null.");
            List<TResult> result = new List<TResult>();
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                    result.Add(selector(iterator.Current));
            }
            finally { iterator.Dispose(); }
            return result;
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            if (source == null || selector == null)
                throw new ArgumentNullException("The source and selector cannot be null.");
            List<TResult> result = new List<TResult>(source.Length);
            for (int index = 0; index < source.Length; index++)
                result.Add(selector(source[index]));
            return result;
        }

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source and predicate cannot be null.");
            List<TSource> result = new List<TSource>();
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                    if (predicate(iterator.Current))
                        result.Add(iterator.Current);
            }
            finally { iterator.Dispose(); }
            return result;
        }

        public static IEnumerable<TSource> Where<TSource>(this TSource[] source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source and predicate cannot be null.");
            List<TSource> result = new List<TSource>();
            for (int index = 0; index < source.Length; index++)
                if (predicate(source[index]))
                    result.Add(source[index]);
            return result;
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source cannot be null.");
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try { return iterator.MoveNext(); }
            finally { iterator.Dispose(); }
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source and predicate cannot be null.");
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                    if (predicate(iterator.Current))
                        return true;
                return false;
            }
            finally { iterator.Dispose(); }
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            if (source == null)
                throw new ArgumentNullException("The source cannot be null.");
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                    if (Equals(iterator.Current, value))
                        return true;
                return false;
            }
            finally { iterator.Dispose(); }
        }

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source cannot be null.");
            List<TSource> result = new List<TSource>();
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                    result.Add(iterator.Current);
            }
            finally { iterator.Dispose(); }
            return result.ToArray();
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
            => new List<TSource>(source);

        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source cannot be null.");
            int count = 0;
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                    count++;
            }
            finally { iterator.Dispose(); }
            return count;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source and predicate cannot be null.");
            int count = 0;
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                    if (predicate(iterator.Current))
                        count++;
            }
            finally { iterator.Dispose(); }
            return count;
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source cannot be null.");
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try
            {
                if (iterator.MoveNext())
                    return iterator.Current;
            }
            finally { iterator.Dispose(); }
            throw new InvalidOperationException("The source contains no elements.");
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source cannot be null.");
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try { return iterator.MoveNext() ? iterator.Current : default; }
            finally { iterator.Dispose(); }
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException("The source and predicate cannot be null.");
            IEnumerator<TSource> iterator = source.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                    if (predicate(iterator.Current))
                        return iterator.Current;
                return default;
            }
            finally { iterator.Dispose(); }
        }

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null)
                throw new ArgumentNullException("The source cannot be null.");
            if (count < 0)
                throw new ArgumentException("The count cannot be negative.");
            List<TSource> result = new List<TSource>();
            int index = 0;
            foreach (TSource value in source)
                if (index++ >= count)
                    result.Add(value);
            return result;
        }

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {
            if (source == null)
                throw new ArgumentNullException("The source cannot be null.");
            if (count < 0)
                throw new ArgumentException("The count cannot be negative.");
            List<TSource> result = new List<TSource>();
            int index = 0;
            foreach (TSource value in source)
            {
                if (index++ >= count)
                    break;
                result.Add(value);
            }
            return result;
        }

        public static int Sum(this IEnumerable<int> source)
        {
            if (source == null)
                throw new ArgumentNullException("The source cannot be null.");
            int sum = 0;
            foreach (int value in source)
                sum += value;
            return sum;
        }
    }
}

namespace System.Threading
{
    public delegate void ThreadStart();

    [Obsolete("Use Task-based APIs instead. Thread is a cooperative green-thread implementation.")]
    public sealed unsafe partial class Thread
    {
        private const int AutomaticYieldInterval = 65536;
        private const int StackRestoreSafetyMargin = 256;
        private const nuint StackRootRangeSlack = 65536;

        private static Thread _current;
        private static int _initialCriticalRegionCount;
        private static int _automaticYieldCounter;
        private static byte* _stackTop;

        private ThreadStart _start;
        private Thread _next;
        private JumpBuffer* _context;
        private byte* _stackCopy;
        private nuint _stackSize;
        private nuint _stackCapacity;
        private GCFrame* _gcFrame;
        private Object[] _rootSnapshot;
        private int _rootCount;
        private ExceptionFrame* _exceptionFrame;
        private Exception _currentException;
        private long _sleepUntil;
        private int _criticalRegionCount;

        public Thread(ThreadStart start)
        {
            if (start == null)
                throw new ArgumentNullException("The thread start delegate cannot be null.");
            _start = start;
        }

        private Thread() { }

        public static Thread CurrentThread
        {
            get
            {
                EnsureInitialized();
                return _current;
            }
        }

        public bool IsAlive => _context != null;

        public void Start()
        {
            if (_next != null)
                throw new InvalidOperationException("The thread has already been started.");
            EnsureInitialized();

            _context = (JumpBuffer*)(void*)Marshal.AllocHGlobal((nint)sizeof(JumpBuffer));
            if (_context == null)
                ExceptionRuntime.Abort();
            _next = _current._next;
            _current._next = this;
        }

        public void Join()
        {
            if (_next == null)
                throw new InvalidOperationException("The thread has not been started.");
            if (this == _current)
                throw new InvalidOperationException("A thread cannot join itself.");
            while (_context != null)
                Yield();
        }

        public static bool Yield(bool generated = false)
        {
            if (generated)
            {
                if (++_automaticYieldCounter < AutomaticYieldInterval)
                    return false;
                _automaticYieldCounter = 0;
            }

            return YieldCore();

            static bool YieldCore()
            {
                Thread current = _current;
                if (current == null || current._next == current || current._criticalRegionCount != 0)
                    return false;

                Thread next = FindRunnable(current._next, false);
                if (next == current)
                    return false;
                byte* stackTopMarker = stackalloc byte[1];
                SwitchTo(next, stackTopMarker + 1);
                return true;
            }
        }

        internal static void EnterCriticalRegion()
        {
            if (_current == null)
                _initialCriticalRegionCount++;
            else
                _current._criticalRegionCount++;
        }

        internal static void ExitCriticalRegion()
        {
            if (_current == null)
                _initialCriticalRegionCount--;
            else
                _current._criticalRegionCount--;
        }

        public static void Sleep(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
                throw new ArgumentOutOfRangeException();
            EnsureInitialized();
            if (millisecondsTimeout == 0)
            {
                Yield();
                return;
            }

            _current._sleepUntil = millisecondsTimeout == -1
                ? long.MaxValue
                : GetCurrentTimeMilliseconds() + millisecondsTimeout;
            Thread next = FindRunnable(_current._next, true);
            if (next != _current)
            {
                byte* stackTopMarker = stackalloc byte[1];
                SwitchTo(next, stackTopMarker + 1);
            }
            _current._sleepUntil = 0;
        }

        private static void EnsureInitialized()
        {
            if (_current != null)
                return;

            Thread main = new Thread();
            main._criticalRegionCount = _initialCriticalRegionCount;
            _initialCriticalRegionCount = 0;
            main._context = (JumpBuffer*)(void*)Marshal.AllocHGlobal((nint)sizeof(JumpBuffer));
            if (main._context == null)
                ExceptionRuntime.Abort();
            main._next = main;
            _current = main;
        }

        private static Thread FindRunnable(Thread first, bool wait)
        {
            while (true)
            {
                long now = 0;
                bool hasCurrentTime = false;
                Thread candidate = first;
                do
                {
                    if (candidate._sleepUntil == 0)
                        return candidate;
                    if (!hasCurrentTime)
                    {
                        now = GetCurrentTimeMilliseconds();
                        hasCurrentTime = true;
                    }
                    if (candidate._sleepUntil <= now)
                        return candidate;
                    candidate = candidate._next;
                }
                while (candidate != first);

                if (!wait)
                    return _current;
            }
        }

        private static void SwitchTo(Thread next, byte* stackTopHint)
        {
            Thread current = _current;
            int resumed = ExceptionRuntime.SetJump(current._context, null);
            if (resumed != 0)
                return;

            GCFrame* gcFrame = GCHeap.GetTopFrame();
            byte* stackBottom = stackalloc byte[1];
            if (_stackTop == null)
                _stackTop = FindStackTop(stackBottom, stackTopHint, gcFrame,
                    ExceptionRuntime.GetTop());
            if (stackBottom >= _stackTop)
                ExceptionRuntime.Abort();

            nuint stackSize = (nuint)(_stackTop - stackBottom);
            CaptureRoots(current, gcFrame);
            EnsureStackCapacity(current, stackSize);
            Unsafe.CopyBlock(current._stackCopy, stackBottom, (uint)stackSize);
            current._stackSize = stackSize;
            current._gcFrame = gcFrame;
            current._exceptionFrame = ExceptionRuntime.GetTop();
            current._currentException = ExceptionRuntime.GetCurrent();

            Activate(next);
        }

        private static byte* FindStackTop(byte* stackBottom, byte* hint,
            GCFrame* gcFrame, ExceptionFrame* exceptionFrame)
        {
            byte* top = hint;
            for (GCFrame* frame = gcFrame; frame != null; frame = frame->Previous)
            {
                top = Max(top, (byte*)(frame + 1));
                top = Max(top, (byte*)(frame->Roots + frame->RootCount));
            }
            for (ExceptionFrame* frame = exceptionFrame; frame != null; frame = frame->Previous)
            {
                top = Max(top, (byte*)(frame + 1));
                top = Max(top, (byte*)(frame->Buffer + 1));
            }

            byte* rootLimit = top + StackRootRangeSlack;
            for (GCFrame* frame = gcFrame; frame != null; frame = frame->Previous)
            {
                for (int index = 0; index < frame->RootCount; index++)
                {
                    GCRoot* root = frame->Roots + index;
                    byte* address = (byte*)root->Address;
                    if (address >= stackBottom && address < rootLimit)
                        top = Max(top, address + GetRootValueSize(root));
                }
            }
            return top;
        }

        private static byte* Max(byte* left, byte* right) => left >= right ? left : right;

        private static nuint GetRootValueSize(GCRoot* root)
        {
            if (root->Type == null)
                return (nuint)sizeof(Object*);
            int[] offsets = root->Type.ObjectReferenceOffsets;
            nuint size = 0;
            for (int index = 0; index < offsets.Length; index++)
            {
                nuint end = (nuint)offsets[index] + (nuint)sizeof(Object*);
                if (end > size)
                    size = end;
            }
            return size;
        }

        private static void CaptureRoots(Thread thread, GCFrame* frame)
        {
            thread._rootCount = 0;
            while (frame != null)
            {
                for (int index = 0; index < frame->RootCount; index++)
                {
                    GCRoot* root = frame->Roots + index;
                    if (root->Address == null)
                        continue;

                    Type type = root->Type;
                    if (type == null)
                    {
                        Object* value = *root->Address;
                        if (value != null)
                            AppendRoot(thread, *(Object*)&value);
                        continue;
                    }

                    int[] offsets = type.ObjectReferenceOffsets;
                    for (int offsetIndex = 0; offsetIndex < offsets.Length; offsetIndex++)
                    {
                        Object* value = *(Object**)((byte*)root->Address + offsets[offsetIndex]);
                        if (value != null)
                            AppendRoot(thread, *(Object*)&value);
                    }
                }
                frame = frame->Previous;
            }

            if (thread._rootSnapshot != null)
                for (int index = thread._rootCount; index < thread._rootSnapshot.Length; index++)
                    thread._rootSnapshot[index] = null;
        }

        private static void AppendRoot(Thread thread, Object value)
        {
            if (thread._rootSnapshot == null)
                thread._rootSnapshot = new Object[16];
            else if (thread._rootCount == thread._rootSnapshot.Length)
            {
                Object[] roots = new Object[thread._rootSnapshot.Length * 2];
                for (int index = 0; index < thread._rootCount; index++)
                    roots[index] = thread._rootSnapshot[index];
                thread._rootSnapshot = roots;
            }
            thread._rootSnapshot[thread._rootCount++] = value;
        }

        private static void EnsureStackCapacity(Thread thread, nuint size)
        {
            if (size <= thread._stackCapacity)
                return;
            byte* stackCopy = (byte*)(void*)Marshal.AllocHGlobal((nint)size);
            if (stackCopy == null)
                ExceptionRuntime.Abort();
            if (thread._stackCopy != null)
                Marshal.FreeHGlobal((IntPtr)thread._stackCopy);
            thread._stackCopy = stackCopy;
            thread._stackCapacity = size;
        }

        private static void Activate(Thread thread)
        {
            _current = thread;
            thread._sleepUntil = 0;
            GCHeap.UnwindTo(null);
            ThreadStart start = thread._start;
            if (start != null)
            {
                thread._start = null;
                ExceptionRuntime.Restore(null, null);
                start();
                CompleteCurrent();
            }

            RestoreStack(thread);
        }

        private static void CompleteCurrent()
        {
            Thread completed = _current;

            Thread previous = completed._next;
            while (previous._next != completed)
                previous = previous._next;
            previous._next = completed._next;

            Thread next = FindRunnable(completed._next, true);
            if (completed._stackCopy != null)
                Marshal.FreeHGlobal((IntPtr)completed._stackCopy);
            Marshal.FreeHGlobal((IntPtr)completed._context);
            completed._context = null;
            completed._rootSnapshot = null;

            Activate(next);
            ExceptionRuntime.Abort();
        }

        private static void RestoreStack(Thread thread)
        {
            byte* stackPointer = stackalloc byte[1];
            byte* stackBottom = _stackTop - thread._stackSize;
            if (stackPointer + StackRestoreSafetyMargin >= stackBottom)
            {
                RestoreStack(thread);
                ExceptionRuntime.Abort();
            }

            SpillRegisterWindows();
            Unsafe.CopyBlock(stackBottom, thread._stackCopy, (uint)thread._stackSize);
            ExceptionRuntime.Restore(thread._exceptionFrame, thread._currentException);
            GCHeap.UnwindTo(thread._gcFrame);
            ExceptionRuntime.LongJump(thread._context, 1);
        }

        /// <summary>
        /// Spills live register windows to the stack before the active stack is overwritten.
        /// Platforms that use register windows, such as the Xtensa windowed ABI, must provide
        /// an implementation. This is an optional C# partial method so platform-specific projects
        /// can supply the implementation without adding platform dependencies to CoreLib. If a
        /// platform does not implement it, the C# compiler removes the declaration and its calls.
        /// </summary>
        static partial void SpillRegisterWindows();

        [DllImport("*", EntryPoint = "GetCurrentTimeMilliseconds")]
        private static extern long GetCurrentTimeMilliseconds();
    }

    public static class Monitor
    {
        private sealed class MonitorEntry
        {
            internal object Value;
            internal Thread Owner;
            internal int RecursionCount;
            internal MonitorEntry Next;
        }

        private static MonitorEntry s_entries;

        public static void Enter(object value)
        {
            bool lockTaken = false;
            Enter(value, ref lockTaken);
        }

        public static void Enter(object value, ref bool lockTaken)
        {
            if (lockTaken)
                throw new InvalidOperationException("The lock is already held.");
            if (value == null)
                throw new ArgumentNullException("The lock object cannot be null.");

            Thread owner = Thread.CurrentThread;
            while (true)
            {
                Thread.EnterCriticalRegion();
                MonitorEntry entry = FindEntry(value, out _);
                if (entry == null)
                {
                    s_entries = new MonitorEntry
                    {
                        Value = value,
                        Owner = owner,
                        RecursionCount = 1,
                        Next = s_entries
                    };
                    Thread.ExitCriticalRegion();
                    lockTaken = true;
                    return;
                }

                if (entry.Owner == owner)
                {
                    entry.RecursionCount++;
                    Thread.ExitCriticalRegion();
                    lockTaken = true;
                    return;
                }

                Thread.ExitCriticalRegion();
                Thread.Yield();
            }
        }

        public static void Exit(object value)
        {
            if (value == null)
                throw new ArgumentNullException("The lock object cannot be null.");

            Thread owner = Thread.CurrentThread;
            Thread.EnterCriticalRegion();
            MonitorEntry previous;
            MonitorEntry entry = FindEntry(value, out previous);
            if (entry == null || entry.Owner != owner)
            {
                Thread.ExitCriticalRegion();
                throw new SynchronizationLockException();
            }

            if (--entry.RecursionCount == 0)
            {
                if (previous == null)
                    s_entries = entry.Next;
                else
                    previous.Next = entry.Next;
            }
            Thread.ExitCriticalRegion();
        }

        private static MonitorEntry FindEntry(object value, out MonitorEntry previous)
        {
            previous = null;
            MonitorEntry entry = s_entries;
            while (entry != null)
            {
                if (entry.Value == value)
                    return entry;
                previous = entry;
                entry = entry.Next;
            }
            return null;
        }
    }

    public class SynchronizationLockException : Exception
    {
        public SynchronizationLockException()
            : base("Object synchronization method was called from an unsynchronized block of code.") { }

        public SynchronizationLockException(string message) : base(message) { }
    }
}

namespace System.Threading.Tasks
{
    public enum TaskStatus
    {
        Created,
        WaitingForActivation,
        WaitingToRun,
        Running,
        WaitingForChildrenToComplete,
        RanToCompletion,
        Canceled,
        Faulted
    }

    public partial class Task
    {
        private const int Pending = 0;
        private const int Completed = 1;
        private const int Faulted = 2;
        private const int Canceled = 3;
        private volatile int _state;
        private Exception _exception;
        private Action _continuation;
        internal Task() { }
        public bool IsCompleted => _state != Pending;
        public bool IsCompletedSuccessfully => _state == Completed;
        public bool IsFaulted => _state == Faulted;
        public bool IsCanceled => _state == Canceled;
        public Exception Exception => _exception;
        public TaskStatus Status => _state == Pending ? TaskStatus.WaitingForActivation :
            (_state == Completed ? TaskStatus.RanToCompletion :
             (_state == Canceled ? TaskStatus.Canceled : TaskStatus.Faulted));
        public TaskAwaiter GetAwaiter() => new TaskAwaiter(this);
        public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) => new ConfiguredTaskAwaitable(this);
        public void SetResult() => TrySetResult();
        public void SetException(Exception exception) => TrySetException(exception);
        public void SetCanceled() => TrySetCanceled();
        internal void OnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException("The task continuation cannot be null.");
            if (IsCompleted)
                continuation();
            else
                _continuation += continuation;
        }
        internal void GetResult()
        {
            Wait();
        }
        public void Wait()
        {
            while (_state == Pending)
                WaitForCompletion();
            if (_state == Faulted)
                throw _exception;
            if (_state == Canceled)
                throw new OperationCanceledException();
        }
        static partial void WaitForCompletion();
        internal bool TrySetResult()
        {
            if (_state != Pending)
                return false;
            _state = Completed;
            Action continuation = _continuation;
            _continuation = null;
            if (continuation != null)
                continuation();
            return true;
        }
        internal bool TrySetException(Exception exception)
        {
            if (_state != Pending)
                return false;
            _exception = exception;
            _state = Faulted;
            Action continuation = _continuation;
            _continuation = null;
            if (continuation != null)
                continuation();
            return true;
        }
        internal bool TrySetCanceled()
        {
            if (_state != Pending)
                return false;
            _state = Canceled;
            Action continuation = _continuation;
            _continuation = null;
            if (continuation != null)
                continuation();
            return true;
        }
        public static Task CompletedTask
        {
            get
            {
                Task task = new Task();
                task.TrySetResult();
                return task;
            }
        }
        public static Task FromResult() => CompletedTask;
        public static Task FromException(Exception exception)
        {
            Task task = new Task();
            task.TrySetException(exception);
            return task;
        }
        public static Task<TResult> FromResult<TResult>(TResult result)
        {
            Task<TResult> task = new Task<TResult>();
            task.SetResult(result);
            return task;
        }
        public static Task<TResult> FromException<TResult>(Exception exception)
        {
            Task<TResult> task = new Task<TResult>();
            task.SetException(exception);
            return task;
        }
    }

    public class Task<TResult> : Task
    {
        private TResult _result;
        public new TaskAwaiter<TResult> GetAwaiter() => new TaskAwaiter<TResult>(this);
        public new ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext)
            => new ConfiguredTaskAwaitable<TResult>(this);
        public TResult Result => GetResult();
        public void SetResult(TResult result) { _result = result; SetResult(); }
        internal bool TrySetResult(TResult result) { _result = result; return TrySetResult(); }
        public new TResult GetResult() { base.GetResult(); return _result; }
        public static Task<TResult> FromResult(TResult result)
        {
            Task<TResult> task = new Task<TResult>();
            task.SetResult(result);
            return task;
        }
    }

    public class TaskCompletionSource
    {
        private readonly Task _task = new Task();
        public Task Task => _task;
        public void SetResult() => _task.SetResult();
        public void SetException(Exception exception) => _task.SetException(exception);
        public void SetCanceled() => _task.SetCanceled();
        public bool TrySetResult() => _task.TrySetResult();
        public bool TrySetException(Exception exception) => _task.TrySetException(exception);
        public bool TrySetCanceled() => _task.TrySetCanceled();
    }

    public class TaskCompletionSource<TResult>
    {
        private readonly Task<TResult> _task = new Task<TResult>();
        public Task<TResult> Task => _task;
        public void SetResult(TResult result) => _task.SetResult(result);
        public void SetException(Exception exception) => _task.SetException(exception);
        public void SetCanceled() => _task.SetCanceled();
        public bool TrySetResult(TResult result) => _task.TrySetResult(result);
        public bool TrySetException(Exception exception) => _task.TrySetException(exception);
        public bool TrySetCanceled() => _task.TrySetCanceled();
    }

    public struct TaskAwaiter : ICriticalNotifyCompletion
    {
        private readonly Task _task;
        public TaskAwaiter(Task task) { _task = task; }
        public bool IsCompleted => _task.IsCompleted;
        public void GetResult() => _task.GetResult();
        public void OnCompleted(Action continuation) => _task.OnCompleted(continuation);
        public void UnsafeOnCompleted(Action continuation) => _task.OnCompleted(continuation);
    }

    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion
    {
        private readonly Task<TResult> _task;
        public TaskAwaiter(Task<TResult> task) { _task = task; }
        public bool IsCompleted => _task.IsCompleted;
        public TResult GetResult() => _task.GetResult();
        public void OnCompleted(Action continuation) => _task.OnCompleted(continuation);
        public void UnsafeOnCompleted(Action continuation) => _task.OnCompleted(continuation);
    }

    public struct ConfiguredTaskAwaitable
    {
        private readonly Task _task;
        public ConfiguredTaskAwaitable(Task task) { _task = task; }
        public TaskAwaiter GetAwaiter() => _task.GetAwaiter();
    }

    public struct ConfiguredTaskAwaitable<TResult>
    {
        private readonly Task<TResult> _task;
        public ConfiguredTaskAwaitable(Task<TResult> task) { _task = task; }
        public TaskAwaiter<TResult> GetAwaiter() => _task.GetAwaiter();
    }
}

namespace System.Text
{
    public sealed class StringBuilder
    {
        private char[] _buffer;
        private int _length;

        public StringBuilder()
            : this(16)
        {
        }

        public StringBuilder(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException("The StringBuilder capacity cannot be negative.");
            _buffer = new char[capacity == 0 ? 1 : capacity];
        }

        public StringBuilder(string value)
        {
            if (value == null)
                throw new ArgumentNullException("The initial string cannot be null.");
            _buffer = new char[value.Length == 0 ? 1 : value.Length];
            Append(value);
        }

        public StringBuilder(string value, int capacity)
        {
            if (value == null)
                throw new ArgumentNullException("The initial string cannot be null.");
            if (capacity < value.Length)
                capacity = value.Length;
            _buffer = new char[capacity == 0 ? 1 : capacity];
            Append(value);
        }

        public int Length
        {
            get => _length;
            set
            {
                if (value < 0)
                    throw new ArgumentException("The StringBuilder length cannot be negative.");
                EnsureCapacity(value);
                if (value > _length)
                    for (int index = _length; index < value; index++)
                        _buffer[index] = '\0';
                _length = value;
            }
        }

        public int Capacity
        {
            get => _buffer.Length;
            set
            {
                if (value < _length)
                    throw new ArgumentException("The StringBuilder capacity cannot be less than Length.");
                if (value == 0)
                    value = 1;
                if (value == _buffer.Length)
                    return;
                char[] buffer = new char[value];
                for (int index = 0; index < _length; index++)
                    buffer[index] = _buffer[index];
                _buffer = buffer;
            }
        }

        public char this[int index]
        {
            get
            {
                ValidateIndex(index);
                return _buffer[index];
            }
            set
            {
                ValidateIndex(index);
                _buffer[index] = value;
            }
        }

        public StringBuilder Append(char value)
        {
            EnsureCapacity(_length + 1);
            _buffer[_length++] = value;
            return this;
        }

        public StringBuilder Append(char value, int repeatCount)
        {
            if (repeatCount < 0)
                throw new ArgumentException("The repeat count cannot be negative.");
            EnsureCapacity(_length + repeatCount);
            for (int index = 0; index < repeatCount; index++)
                _buffer[_length++] = value;
            return this;
        }

        public StringBuilder Append(string value)
        {
            if (value == null)
                return this;
            EnsureCapacity(_length + value.Length);
            for (int index = 0; index < value.Length; index++)
                _buffer[_length++] = value[index];
            return this;
        }

        public StringBuilder Append(string value, int startIndex, int count)
        {
            if (value == null)
                return this;
            if (startIndex < 0 || count < 0 || startIndex > value.Length - count)
                throw new ArgumentException("The source string range is invalid.");
            EnsureCapacity(_length + count);
            for (int index = 0; index < count; index++)
                _buffer[_length++] = value[startIndex + index];
            return this;
        }

        public StringBuilder Append(char[] value)
        {
            if (value == null)
                return this;
            return Append(value, 0, value.Length);
        }

        public StringBuilder Append(char[] value, int startIndex, int count)
        {
            if (value == null)
                return this;
            if (startIndex < 0 || count < 0 || startIndex > value.Length - count)
                throw new ArgumentException("The source character array range is invalid.");
            EnsureCapacity(_length + count);
            for (int index = 0; index < count; index++)
                _buffer[_length++] = value[startIndex + index];
            return this;
        }

        public StringBuilder Append(object value) => Append(value == null ? null : value.ToString());
        public StringBuilder Append(int value) => Append(value.ToString());
        public StringBuilder Append(uint value) => Append(value.ToString());
        public StringBuilder Append(long value) => Append(value.ToString());
        public StringBuilder Append(ulong value) => Append(value.ToString());
        public StringBuilder Append(bool value) => Append(value.ToString());

        public StringBuilder AppendLine() => Append('\r').Append('\n');
        public StringBuilder AppendLine(string value) => Append(value).Append('\r').Append('\n');

        public StringBuilder Clear()
        {
            _length = 0;
            return this;
        }

        public StringBuilder Remove(int startIndex, int length)
        {
            if (startIndex < 0 || length < 0 || startIndex > _length - length)
                throw new ArgumentException("The StringBuilder range is invalid.");
            for (int index = startIndex; index < _length - length; index++)
                _buffer[index] = _buffer[index + length];
            _length -= length;
            return this;
        }

        public StringBuilder Replace(char oldChar, char newChar)
        {
            for (int index = 0; index < _length; index++)
                if (_buffer[index] == oldChar)
                    _buffer[index] = newChar;
            return this;
        }

        public override string ToString()
        {
            if (_length == 0)
                return string.Empty;
            char[] value = new char[_length];
            for (int index = 0; index < _length; index++)
                value[index] = _buffer[index];
            return new string(value);
        }

        public string ToString(int startIndex, int length)
        {
            if (startIndex < 0 || length < 0 || startIndex > _length - length)
                throw new ArgumentException("The StringBuilder range is invalid.");
            char[] value = new char[length];
            for (int index = 0; index < length; index++)
                value[index] = _buffer[startIndex + index];
            return new string(value);
        }

        private void EnsureCapacity(int required)
        {
            if (required <= _buffer.Length)
                return;
            int capacity = _buffer.Length * 2;
            if (capacity < required)
                capacity = required;
            char[] buffer = new char[capacity];
            for (int index = 0; index < _length; index++)
                buffer[index] = _buffer[index];
            _buffer = buffer;
        }

        private void ValidateIndex(int index)
        {
            if ((uint)index >= (uint)_length)
                throw new IndexOutOfRangeException("The StringBuilder index is outside the current contents.");
        }
    }
}
