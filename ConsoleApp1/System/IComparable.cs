namespace System
{
    public interface IComparable
    {
        int CompareTo(object value);
    }

    public interface IComparable<in T>
    {
        int CompareTo(T value);
    }
}
