using System.Collections.Generic;

namespace Jhulis.Core.Helpers.Extensions
{
    public static class ListExtensions
    {
        internal static void TryAddEmptiableRange<T>(this List<T> list, List<T> range)
        {
            if (range?.Count > 0) list.AddRange(range);
        }
    }
}
