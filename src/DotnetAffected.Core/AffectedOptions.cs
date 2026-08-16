using DotnetAffected.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        /// <param name="assumeChanges"></param>
        /// <param name="excludeDiscoveryRegex"></param>
        /// <param name="honourGitIgnore">Defaults to <b>true</b>.</param>
        /// <param name="uncommittedChanges">Defaults to <see cref="UncommittedChanges.All"/>.</param>
        public AffectedOptions(
            string? repositoryPath = null,
            string? filterFilePath = null,
            string? fromRef = null,
            string? toRef = null,
            string? exclusionRegex = null,
            IEnumerable<string>? assumeChanges = null,
            string? excludeDiscoveryRegex = null,
            bool honourGitIgnore = true,
            UncommittedChanges uncommittedChanges = UncommittedChanges.All)
        {
            RepositoryPath = DetermineRepositoryPath(repositoryPath, filterFilePath);

            // Ensure the provided filter is a rooted path
            if (!string.IsNullOrEmpty(filterFilePath))
            {
                FilterFilePath = Path.IsPathRooted(filterFilePath)
                    ? filterFilePath
                    : Path.Join(Environment.CurrentDirectory, filterFilePath);
            }

            FromRef = fromRef ?? string.Empty;
            ToRef = toRef ?? string.Empty;
            ExclusionRegex = exclusionRegex;
            AssumeChanges = assumeChanges?.ToArray() ?? Array.Empty<string>();
            ExcludeDiscoveryRegex = excludeDiscoveryRegex;
            HonourGitIgnore = honourGitIgnore;
            UncommittedChanges = uncommittedChanges;
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
        /// Gets the commit the working tree is expected to be checked out at.
        /// </summary>
        /// <remarks>
        /// OBSOLETE, removed in v8. Projects are discovered and evaluated from the working tree,
        /// so this is only accepted when it names the commit that is checked out, which makes it
        /// a no-op. Use <see cref="UncommittedChanges"/> to choose what the working tree
        /// contributes.
        /// </remarks>
        public string ToRef { get; }

        /// <summary>
        /// Gets what the working tree contributes on top of the commits between
        /// <see cref="FromRef"/> and the commit that is checked out.
        /// </summary>
        public UncommittedChanges UncommittedChanges { get; }

        /// <summary>
        /// Gets the regular expression to use for excluding projects from the output. Matching
        /// projects are still evaluated, and still carry changes through to the projects that
        /// depend on them. They are only kept out of the results.
        /// </summary>
        public string? ExclusionRegex { get; }

        /// <inheritdoc />
        public string? ExcludeDiscoveryRegex { get; }

        /// <summary>
        /// Gets the projects to treat as changed, instead of determining them from Git.
        /// Each entry is a path to a project file, a project's ProjectName, or a project file name.
        /// </summary>
        public string[] AssumeChanges { get; }

        /// <inheritdoc />
        public bool HonourGitIgnore { get; }

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
                // A relative path to a file may be provided, in such case getting the directory name fails.
                return Environment.CurrentDirectory;
            }

            return filterFileDirectory;
        }
    }
}
