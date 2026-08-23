using System.Collections.Generic;

namespace System.Linq
{
    public static partial class Enumerable
    {
        public static List<TSource> ToList<TSource>(this TSource[] source)
        {
            if (source == null)
                throw new ArgumentNullException();
            List<TSource> result = new List<TSource>(source.Length);
            for (int i = 0; i < source.Length; i++)
                result.Add(source[i]);
            return result;
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException();
            List<TSource> result = new List<TSource>();
            IEnumerator<TSource> enumerator = source.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    result.Add(enumerator.Current);
            }
            finally { enumerator.Dispose(); }
            return result;
        }

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
            => ToList(source).ToArray();
    }
}
