using System;
using System.Collections.Generic;
using System.Linq;

public static class CollectionExtensions
{
    private static Random random = new Random();

    public static T RandomNonEmptyElement<T>(this IEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        // 使用 Where 语句排除 null 值。对于非空引用类型和所有值类型，这将保留元素。
        var nonNullElements = source.Where(e => e != null || !typeof(T).IsValueType).ToList();

        if (nonNullElements.Count == 0)
        {
            throw new InvalidOperationException("Cannot select a random element from an empty collection.");
        }

        int index = random.Next(nonNullElements.Count);
        return nonNullElements[index];
    }

}