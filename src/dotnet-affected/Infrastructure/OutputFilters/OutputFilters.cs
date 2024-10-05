using System.Collections.Generic;

namespace Affected.Cli
{
    internal static class OutputFilters
    {
        public const string Affected = "affected";
        public const string Changed = "changed";
        public const string Excluded = "excluded";

        public static readonly IReadOnlyDictionary<string, ProjectStatus> StatusMap = new Dictionary<string, ProjectStatus>
        {
            { Affected, ProjectStatus.Affected },
            { Changed, ProjectStatus.Changed },
            { Excluded, ProjectStatus.Excluded }
        };
        public static readonly IReadOnlyList<string> All = new[] { Affected, Changed, Excluded };
    }
}
