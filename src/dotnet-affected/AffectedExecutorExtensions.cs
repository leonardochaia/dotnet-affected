using DotnetAffected.Abstractions;
using System.Linq;

namespace Affected.Cli
{
    internal static class AffectedExecutorExtensions
    {
        public static void ThrowIfNoChanges(this AffectedSummary summary)
        {
            if (!summary.ProjectsWithChangedFiles.Any() && !summary.AffectedProjects.Any())
            {
                throw new NoChangesException();
            }
        }
    }
}
