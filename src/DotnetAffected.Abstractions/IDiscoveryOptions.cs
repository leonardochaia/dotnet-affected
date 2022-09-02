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
        /// Gets the path to the solution file, if any.
        /// </summary>
        string? SolutionPath { get; }
    }
}
