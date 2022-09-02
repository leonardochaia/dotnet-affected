namespace Affected.Cli
{
    /// <summary>
    /// Options for executing Dotnet Affected
    /// </summary>
    public class AffectedOptions : IDiscoveryOptions
    {
        /// <summary>
        /// Creates a new instance of <see cref="AffectedOptions"/>.
        /// </summary>
        /// <param name="repositoryPath"></param>
        /// <param name="solutionPath"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public AffectedOptions(
            string repositoryPath,
            string? solutionPath = null,
            string from = "",
            string to = "")
        {
            RepositoryPath = repositoryPath;
            SolutionPath = solutionPath;
            From = from;
            To = to;
        }

        /// <summary>
        /// Gets the path to the repository root.
        /// </summary>
        public string RepositoryPath { get; }

        /// <summary>
        /// Gets the path to the solution file, if any.
        /// </summary>
        public string? SolutionPath { get; }

        public string From { get; }
        public string To { get; }
    }
}
