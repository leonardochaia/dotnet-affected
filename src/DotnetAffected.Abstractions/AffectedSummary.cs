using Microsoft.Build.Graph;
using System;

namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// The output of calculating affected projects.
    /// </summary>
    public class AffectedSummary
    {
        /// <summary>
        /// Creates a default Affected Summary.
        /// </summary>
        /// <param name="filesThatChanged"></param>
        /// <param name="projectsWithChangedFiles"></param>
        /// <param name="affectedProjects"></param>
        /// <param name="excludedProjects"></param>
        /// <param name="changedPackages"></param>
        /// <param name="projectsExcludedFromDiscovery"></param>
        public AffectedSummary(
            string[] filesThatChanged,
            ProjectGraphNode[] projectsWithChangedFiles,
            ProjectGraphNode[] affectedProjects,
            ProjectGraphNode[] excludedProjects,
            PackageChange[] changedPackages,
            string[]? projectsExcludedFromDiscovery = null)
        {
            FilesThatChanged = filesThatChanged;
            ProjectsWithChangedFiles = projectsWithChangedFiles;
            AffectedProjects = affectedProjects;
            ExcludedProjects = excludedProjects;
            ChangedPackages = changedPackages;
            ProjectsExcludedFromDiscovery = projectsExcludedFromDiscovery ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets the list of files that have changed.
        /// </summary>
        public string[] FilesThatChanged { get; }

        /// <summary>
        /// Gets the list of projects that own the changed files.
        /// </summary>
        public ProjectGraphNode[] ProjectsWithChangedFiles { get; }

        /// <summary>
        /// Gets a list of projects that are affected by the <see cref="FilesThatChanged"/>.
        /// </summary>
        public ProjectGraphNode[] AffectedProjects { get; }

        /// <summary>
        /// Gets a list of projects that had changes or were affected but were kept out of the output.
        /// </summary>
        public ProjectGraphNode[] ExcludedProjects { get; }

        /// <summary>
        /// Gets the paths of projects that were never discovered, and therefore never evaluated.
        /// Paths rather than projects: nothing evaluated them, which is the point of excluding them.
        /// </summary>
        public string[] ProjectsExcludedFromDiscovery { get; }

        /// <summary>
        /// Gets the list of packages that changed.
        /// </summary>
        public PackageChange[] ChangedPackages { get; }
    }
}
