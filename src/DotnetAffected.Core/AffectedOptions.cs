using DotnetAffected.Abstractions;

namespace DotnetAffected.Core
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
        /// <param name="fromRef"></param>
        /// <param name="toRef"></param>
        public AffectedOptions(
            string repositoryPath,
            string? solutionPath = null,
            string fromRef = "",
            string toRef = "")
        {
            RepositoryPath = repositoryPath;
            SolutionPath = solutionPath;
            FromRef = fromRef;
            ToRef = toRef;
        }

        /// <summary>
        /// Gets the path to the repository root.
        /// </summary>
        public string RepositoryPath { get; }

        /// <summary>
        /// Gets the path to the solution file, if any.
        /// </summary>
        public string? SolutionPath { get; }

        /// <summary>
        /// Gets the reference from which to compare changes to.
        /// </summary>
        public string FromRef { get; }

        /// <summary>
        /// Gets the reference up to which changes will be compared from.
        /// </summary>
        public string ToRef { get; }
    }
}
