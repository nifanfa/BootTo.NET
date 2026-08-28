namespace System
{
    public delegate bool Predicate<in T>(T obj);
    public delegate int Comparison<in T>(T x, T y);
    public delegate TOutput Converter<in TInput, out TOutput>(TInput input);
}

