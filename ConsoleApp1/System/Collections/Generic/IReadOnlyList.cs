namespace System.Collections.Generic
{
    public interface IReadOnlyList<out T> : IReadOnlyCollection<T>
    {
        T this[int index] { get; }
    }
}
