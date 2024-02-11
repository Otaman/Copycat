using System.Collections.Generic;

namespace Copycat.IntegrationTests;

[Decorate]
public partial class DecorateDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)]out TValue value) => 
        _decorated.TryGetValue(key, out value); // Decorated method
}