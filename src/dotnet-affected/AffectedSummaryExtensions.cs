using Affected.Cli;
using DotnetAffected.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace DotnetAffected.Core
{
    internal static class AffectedSummaryExtensions
    {
        /// <summary>
        /// Converts the <see cref="AffectedSummary"/> to a list of IProjectInfo, including all affected, changed and excluded projects.
        /// </summary>
        public static IEnumerable<IProjectInfo> GetAllProjects(this AffectedSummary summary)
        {
            var affectedProjects = GetAffectedProjects(summary);
            var changedProjects = GetChangedProjects(summary);
            var excludedProjects = GetExcludedProjects(summary);
            
            var allProjects = changedProjects
                .Concat(affectedProjects)
                .Concat(excludedProjects);

            return allProjects;
        }

        /// <summary>
        /// Converts the <see cref="AffectedSummary"/> to a list of IProjectInfo, including all affected projects.
        /// </summary>
        public static IEnumerable<IProjectInfo> GetAffectedProjects(this AffectedSummary summary)
        {
            var affectedProjects = summary.AffectedProjects
                .Select(p => new ProjectInfo(p, ProjectStatus.Affected));

            return affectedProjects;
        }
        
        /// <summary>
        /// Converts the <see cref="AffectedSummary"/> to a list of IProjectInfo, including all changed projects.
        /// </summary>
        public static IEnumerable<IProjectInfo> GetChangedProjects(this AffectedSummary summary)
        {
            var changedProjects = summary.ProjectsWithChangedFiles
                .Select(p => new ProjectInfo(p, ProjectStatus.Changed));

            return changedProjects;
        }
        
        /// <summary>
        /// Converts the <see cref="AffectedSummary"/> to a list of IProjectInfo, including all excluded projects.
        /// </summary>
        public static IEnumerable<IProjectInfo> GetExcludedProjects(this AffectedSummary summary)
        {
            var excludedProjects = summary.ExcludedProjects
                .Select(p => new ProjectInfo(p, ProjectStatus.Excluded));

            return excludedProjects;
        }
    }
}
