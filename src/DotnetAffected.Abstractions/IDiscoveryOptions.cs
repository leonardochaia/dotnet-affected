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
    }
}
