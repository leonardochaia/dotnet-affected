namespace DotnetAffected.Abstractions
{
    /// <summary>
    /// Options for discovering projects.
    /// </summary>
    public interface IDiscoveryOptions
    {
        /// <summary>
        /// Gets the path to the source code repository root.
        /// </summary>
        string RepositoryPath { get; }

        /// <summary>
        /// Gets the path to a filtering file, if any.
        /// This could be any file that the inner <see cref="IProjectDiscoverer"/> supports.
        /// </summary>
        string? FilterFilePath { get; }

        /// <summary>
        /// Gets whether discovery skips paths that git ignores.
        /// Only applies when discovering from the file system, which is the case when no
        /// <see cref="FilterFilePath"/> is provided: every other discoverer is handed an
        /// explicit list of projects.
        /// </summary>
        bool HonourGitIgnore { get; }

        /// <summary>
        /// Gets the regular expression matched against the full path of every discovered project,
        /// to keep matching ones out of discovery altogether. They are never handed to MSBuild, so
        /// a project that cannot be evaluated stops taking the whole run down with it.
        /// </summary>
        string? ExcludeDiscoveryRegex { get; }
    }
}
