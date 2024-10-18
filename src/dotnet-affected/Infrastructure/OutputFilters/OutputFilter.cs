using Affected.Cli.Commands;
using DotnetAffected.Abstractions;
using DotnetAffected.Core;
using System.Collections.Generic;
using System.Linq;

namespace Affected.Cli
{
    internal class OutputFilter
    {
        private readonly AffectedCommandOutputOptions _options;

        public OutputFilter(AffectedCommandOutputOptions options)
        {
            _options = options;
        }

        public IList<IProjectInfo> GetFilteredProjects(AffectedSummary summary)
        {
            List<IProjectInfo> projectsToInclude = new List<IProjectInfo>();

            HashSet<ProjectStatus> appliedFilters =
                new HashSet<ProjectStatus>(_options.OutputFilters.Select(f => OutputFilters.StatusMap[f]));

            if (appliedFilters.Contains(ProjectStatus.Affected))
            {
                projectsToInclude.AddRange(summary.GetAffectedProjects());
            }
            
            if (appliedFilters.Contains(ProjectStatus.Changed))
            {
                projectsToInclude.AddRange(summary.GetChangedProjects());
            }
            
            if (appliedFilters.Contains(ProjectStatus.Excluded))
            {
                projectsToInclude.AddRange(summary.GetExcludedProjects());
            }
            
            return projectsToInclude;
        }
    }
}
