using System.Collections.Generic;
using System.Linq;

namespace SubtitleDownloader
{
    public static class IEnumerable
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
        {
            return self.Select((item, index) => (item, index));
        }
    }
}
