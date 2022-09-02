using Microsoft.Build.Graph;

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
        /// <param name="changedPackages"></param>
        public AffectedSummary(
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
