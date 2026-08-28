namespace System
{
    public interface ICloneable
    {
        object Clone();
    }

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

    public interface IFormattable
    {
        string ToString(string format, IFormatProvider formatProvider);
    }
}

