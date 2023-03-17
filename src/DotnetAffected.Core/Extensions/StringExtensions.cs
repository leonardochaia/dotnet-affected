using DotnetAffected.Core.Filter;

namespace DotnetAffected.Core.Extensions
{
    /// <summary>
    /// Extensions over <see cref="string"/>
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Build a regex filter out of the provided regex string
        /// </summary>
        /// <param name="regex">The regex to initialize the regex filter</param>
        /// <returns></returns>
        public static RegexFilter BuildRegexFilter(this string regex) => new RegexFilter(regex);
    }
}
