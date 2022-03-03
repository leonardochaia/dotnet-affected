using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Affected.Cli
{
    internal static class NugetHelper
    {
        private static readonly Regex PackageVersionRegex =
            new Regex("<PackageVersion Include=\"(.*)\" Version=\"(.*)\"\\s?/>");

        /// <summary>
        /// Parses the provided lines searching for PackageVersion elements
        /// </summary>
        /// <param name="changedLines"></param>
        /// <returns></returns>
        public static IEnumerable<string> ParseNugetPackagesFromLines(IEnumerable<string> changedLines)
        {
            var output = new HashSet<string>();
            foreach (var line in changedLines)
            {
                var match = PackageVersionRegex.Match(line);
                if (!match.Success) continue;

                var packageName = match.Groups[1].Value;
                if (output.Add(packageName))
                {
                    yield return packageName;
                }
            }
        }
    }
}
