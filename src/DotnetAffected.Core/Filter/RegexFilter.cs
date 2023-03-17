using System.Text.RegularExpressions;

namespace DotnetAffected.Core.Filter
{
    /// <summary>
    /// A filter class based on the System.Text.RegularExpressions.
    /// </summary>
    public class RegexFilter : IFilter<string>
    {
        private readonly Regex _filter;

        /// <summary>
        /// Constructor for the RegexFilter
        /// </summary>
        /// <param name="filter">The string to build the match regex</param>
        public RegexFilter(string filter)
        {
            _filter = new Regex(filter);
        }

        /// <summary>
        /// Filters string with regex
        /// </summary>
        /// <param name="str">String which will be inspected</param>
        /// <returns></returns>
        public bool Matches(string str) => _filter.IsMatch(str);
    }
}
