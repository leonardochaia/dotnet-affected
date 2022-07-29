using Microsoft.Build.Graph;

namespace Affected.Cli
{
    /// <summary>
    /// The output of calculating affected projcets.
    /// </summary>
    public class AffectedSummary
    {
        internal AffectedSummary(
            string[] filesThatChanged,
            ProjectGraphNode[] projectsWithChangedFiles,
            ProjectGraphNode[] affectedProjects,
            PackageChange[] changedPackages)
        {
            FilesThatChanged = filesThatChanged;
            ProjectsWithChangedFiles = projectsWithChangedFiles;
            AffectedProjects = affectedProjects;
            ChangedPackages = changedPackages;
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
        /// Gets the list of packages that changed.
        /// </summary>
        public PackageChange[] ChangedPackages { get; }
    }
}
