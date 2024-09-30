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
        /// <param name="filterFilePath"></param>
        /// <param name="fromRef"></param>
        /// <param name="toRef"></param>
        /// <param name="exclusionRegex"></param>
        public AffectedOptions(
            string? repositoryPath = null,
            string? filterFilePath = null,
            string? fromRef = null,
            string? toRef = null,
            string? exclusionRegex = null)
        {
            RepositoryPath = DetermineRepositoryPath(repositoryPath, filterFilePath);
            FilterFilePath = filterFilePath;
            FromRef = fromRef ?? string.Empty;
            ToRef = toRef ?? string.Empty;
            ExclusionRegex = exclusionRegex;
        }

        /// <summary>
        /// Gets the path to the repository root.
        /// </summary>
        public string RepositoryPath { get; }

        /// <summary>
        /// Gets the path to the filter file, if any.
        /// This could be a solution file, or any other file supported by the
        /// <see cref="IProjectDiscoverer"/> implementations.
        /// </summary>
        public string? FilterFilePath { get; }

        /// <summary>
        /// Gets the reference from which to compare changes to.
        /// </summary>
        public string FromRef { get; }

        /// <summary>
        /// Gets the reference up to which changes will be compared from.
        /// </summary>
        public string ToRef { get; }

        /// <summary>
        /// Gets the regular expression to use for excluding projects.
        /// </summary>
        public string? ExclusionRegex { get; }

        private static string DetermineRepositoryPath(string? repositoryPath, string? filterfilePath)
        {
            // the argument takes precedence.
            if (!string.IsNullOrWhiteSpace(repositoryPath))
            {
                return repositoryPath;
            }

            // if no arguments, then use current directory
            if (string.IsNullOrWhiteSpace(filterfilePath))
            {
                return Environment.CurrentDirectory;
            }

            // When using a filter file, and no path specified, assume the filter file's directory
            var filterFileDirectory = Path.GetDirectoryName(filterfilePath);
            if (string.IsNullOrWhiteSpace(filterFileDirectory))
            {
                throw new InvalidOperationException(
                    $"Failed to determine directory from filter file path path. Ensure the path exists: {filterfilePath}");
            }

            return filterFileDirectory;
        }
    }
}
