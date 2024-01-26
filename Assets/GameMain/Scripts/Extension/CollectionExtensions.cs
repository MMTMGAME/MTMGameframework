using System;
using System.Collections.Generic;
using System.Linq;

public static class CollectionExtensions
{
    private static Random random = new Random();

    public static T RandomNonEmptyElement<T>(this IEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var nonEmptyElements = source.Where(e => !EqualityComparer<T>.Default.Equals(e, default(T))).ToList();

        if (nonEmptyElements.Count == 0)
        {
            throw new InvalidOperationException("Cannot select a random element from an empty collection.");
        }

        int index = random.Next(nonEmptyElements.Count);
        return nonEmptyElements[index];
    }
}