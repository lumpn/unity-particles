using System.Collections.Generic;

namespace Lumpn.Particles
{
    internal static class ListExtensions
    {
        public static void RemoveUnorderedAt<T>(this IList<T> list, int index)
        {
            var last = list.Count - 1;
            list[index] = list[last];
            list.RemoveAt(last);
        }
    }
}
