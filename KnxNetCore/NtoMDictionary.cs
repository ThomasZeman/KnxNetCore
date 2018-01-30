using System.Collections.Generic;
using System.Collections.Immutable;

namespace KnxNetCore
{
    public class NtoMDictionary<T1, T2>
    {
        private ImmutableDictionary<T1, ImmutableList<T2>> _mapping1 = ImmutableDictionary<T1, ImmutableList<T2>>.Empty;
        private ImmutableDictionary<T2, ImmutableList<T1>> _mapping2 = ImmutableDictionary<T2, ImmutableList<T1>>.Empty;

        public ImmutableDictionary<T1, ImmutableList<T2>> Mapping1 => _mapping1;

        public ImmutableDictionary<T2, ImmutableList<T1>> Mapping2 => _mapping2;

        public void Add(T1 items1, T2 items2)
        {
            _mapping1 = _mapping1.AddToListOfValues(items1, items2);
            _mapping2 = _mapping2.AddToListOfValues(items2, items1);
        }

        public void Add(ICollection<T1> items1, ICollection<T2> items2)
        {
            _mapping1 = _mapping1.AddNtoM(items1, items2);
            _mapping2 = _mapping2.AddNtoM(items2, items1);
        }
    }
}
