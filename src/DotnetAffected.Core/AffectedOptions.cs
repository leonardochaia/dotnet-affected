using DotnetAffected.Abstractions;
using System;
using System.IO;

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
        /// <param name="repositoryPath">Will default to <see cref="Environment.CurrentDirectory"/> if not provided</param>
        /// <param name="solutionPath"></param>
        /// <param name="fromRef"></param>
        /// <param name="toRef"></param>
        public AffectedOptions(
            string? repositoryPath = null,
            string? solutionPath = null,
            string? fromRef = null,
            string? toRef = null)
        {
            RepositoryPath = DetermineRepositoryPath(repositoryPath, solutionPath);
            SolutionPath = solutionPath;
            FromRef = fromRef ?? string.Empty;
            ToRef = toRef ?? string.Empty;
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

        private static string DetermineRepositoryPath(string? repositoryPath, string? solutionPath)
        {
            // the argument takes precedence.
            if (!string.IsNullOrWhiteSpace(repositoryPath))
            {
                return repositoryPath;
            }

            // if no arguments, then use current directory
            if (string.IsNullOrWhiteSpace(solutionPath))
            {
                return Environment.CurrentDirectory;
            }

            // When using solution, and no path specified, assume the solution's directory
            var solutionDirectory = Path.GetDirectoryName(solutionPath);
            if (string.IsNullOrWhiteSpace(solutionDirectory))
            {
                throw new InvalidOperationException(
                    $"Failed to determine directory from solution path {solutionPath}");
            }

            return solutionDirectory;
        }
    }
}
