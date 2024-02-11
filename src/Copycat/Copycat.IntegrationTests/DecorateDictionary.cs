using System.Collections;

namespace Copycat.IntegrationTests;

[Decorate]
public partial class DecorateDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}