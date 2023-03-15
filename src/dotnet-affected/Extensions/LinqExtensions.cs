using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Affected.Cli.Extensions
{
    /// <summary>
    /// Extension methods over <see cref="System.Linq"/>.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Filters out projects where the project name matches the <paramref name="regexPattern"/> provided.
        /// <param name="source">The Enumerable to filter in. Must be of type IProjectInfo</param>
        /// <param name="propertySelector">The property to filter</param>
        /// <param name="regexPattern">The regex pattern to match for. If provided null, the <paramref name="source"/> Enumerable will be returned</param>
        /// </summary>
        public static IEnumerable<T> RegexExclude<T>(this IEnumerable<T> source, Func<T,string> propertySelector, string? regexPattern)
        {
            if (regexPattern == null) return source;

            var regex = new Regex(regexPattern);
            return source.Where(obj => !regex.IsMatch(propertySelector.Invoke(obj)));
        }
    }
}
