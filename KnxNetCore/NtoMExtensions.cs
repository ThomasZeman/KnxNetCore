using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace KnxNetCore
{
    internal static class NtoMExtensions
    {
        public static ImmutableDictionary<T1, ImmutableList<T2>> AddNtoM<T1, T2>(this ImmutableDictionary<T1, ImmutableList<T2>> mapping1, ICollection<T1> items1, ICollection<T2> items2)
        {
            return mapping1.SetItems(
                items1.SelectMany(
                    item1 =>
                    {
                        return items2.Select(item2 => new KeyValuePair<T1, ImmutableList<T2>>(item1, mapping1.GetOrEmpty(item1).Add(item2)));
                    }));
        }
    }
}