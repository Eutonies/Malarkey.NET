using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Domain.Util;
public static class CollectionExtensions
{

    public static IReadOnlyDictionary<TKey, TValue> ToDictionarySafe<TInp, TKey, TValue>(
        this IEnumerable<TInp> inputs,
        Func<TInp, TKey> keyExtract,
        Func<TInp, TValue> valueExtract) where TKey : notnull

        => inputs.GroupBy(keyExtract)
              .ToDictionary(_ => _.Key, _ => valueExtract(_.First()));

    public static IReadOnlyDictionary<TKey, TInp> ToDictionarySafe<TInp, TKey>(
        this IEnumerable<TInp> inputs,
        Func<TInp, TKey> keyExtract
        ) where TKey : notnull => inputs.ToDictionarySafe(keyExtract, _ => _);


}
