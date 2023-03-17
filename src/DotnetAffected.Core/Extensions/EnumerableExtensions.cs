using DotnetAffected.Core.Filter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core.Extensions
{
    /// <summary>
    /// Extension methods over <see cref="Enumerable"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Filters out objects where the object matches the filter provided.
        /// <param name="source">The Enumerable to filter in</param>
        /// <param name="propertySelector">The property to filter</param>
        /// <param name="filter">The filter object to filter with</param>
        /// </summary>
        public static IEnumerable<T> Exclude<T, TProperty>(this IEnumerable<T> source,
            Func<T, TProperty> propertySelector, IFilter<TProperty> filter)
        {
            return source.Where(obj => !filter.Matches(propertySelector.Invoke(obj)));
        }
    }
}
