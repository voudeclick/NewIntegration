using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samurai.Integration.Application.Tools
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IList<T>> Chunk<T>(this IEnumerable<T> source, int len)
        {
            if (len <= 0)
                throw new ArgumentNullException();
            var enumer = source.GetEnumerator();
            while (enumer.MoveNext())
            {
                yield return Take(enumer.Current, enumer, len).ToList();
            }
        }
        private static IEnumerable<T> Take<T>(T head, IEnumerator<T> tail, int len)
        {
            while (true)
            {
                yield return head;
                if (--len == 0)
                    break;
                if (tail.MoveNext())
                    head = tail.Current;
                else
                    break;
            }
        }
    }
}
