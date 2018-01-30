using System.Collections.Immutable;

namespace KnxNetCore
{
    internal static class ImmutableDictionaryExtensions
    {
        public static ImmutableList<T2> GetOrEmpty<T1, T2>(this ImmutableDictionary<T1, ImmutableList<T2>> dictionary, T1 key)
        {
            return dictionary.TryGetValue(key, out var list) ? list : ImmutableList<T2>.Empty;
        }

        public static ImmutableDictionary<T1, ImmutableList<T2>> AddToListOfValues<T1, T2>(this ImmutableDictionary<T1, ImmutableList<T2>> dictionary, T1 key, T2 item)
        {
            return dictionary.SetItem(key, dictionary.GetOrEmpty(key).Add(item));
        }
    }
}