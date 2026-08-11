using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DotnetAffected.Core
{
    /// <summary>
    /// In memory representation of a Solution Filter file (<c>.slnf</c>).
    /// </summary>
    public sealed class SolutionFilter
    {
        private static readonly JsonSerializerOptions ReadOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true
        };

        /// <summary>
        /// Creates a new instance of <see cref="SolutionFilter"/>.
        /// </summary>
        /// <param name="solutionPath">Path to the solution being filtered.</param>
        /// <param name="projectPaths">Paths to the projects included by the filter.</param>
        public SolutionFilter(string solutionPath, IEnumerable<string> projectPaths)
        {
            if (string.IsNullOrWhiteSpace(solutionPath))
            {
                throw new ArgumentException("A path to the solution is required.", nameof(solutionPath));
            }

            if (projectPaths is null)
            {
                throw new ArgumentNullException(nameof(projectPaths));
            }

            SolutionPath = Path.GetFullPath(solutionPath);
            ProjectPaths = projectPaths
                .Select(Path.GetFullPath)
                .ToArray();
        }

        /// <summary>
        /// Gets the absolute path to the solution being filtered.
        /// </summary>
        public string SolutionPath { get; }

        /// <summary>
        /// Gets the absolute paths to the projects included by the filter.
        /// </summary>
        public IReadOnlyList<string> ProjectPaths { get; }

        /// <summary>
        /// Gets whether a filter can be created referencing <paramref name="filterFilePath"/>.
        /// Inspects the path only, nothing is read.
        /// </summary>
        /// <param name="filterFilePath">Path to a solution or solution filter, if any.</param>
        /// <returns>Whether <see cref="Create"/> would succeed.</returns>
        public static bool CanCreateFrom(string? filterFilePath)
        {
            if (string.IsNullOrWhiteSpace(filterFilePath))
            {
                return false;
            }

            return filterFilePath!.EndsWith(".sln")
                   || filterFilePath.EndsWith(".slnx")
                   || filterFilePath.EndsWith(".slnf");
        }

        /// <summary>
        /// Creates a filter over <paramref name="projectPaths"/>, referencing the
        /// solution behind <paramref name="filterFilePath"/>.
        /// </summary>
        /// <remarks>
        /// Filtering an existing solution filter narrows it down,
        /// both end up referencing the same solution.
        /// </remarks>
        /// <param name="filterFilePath">Path to a solution (.sln, .slnx) or solution filter (.slnf).</param>
        /// <param name="projectPaths">Paths to the projects to include.</param>
        /// <returns>The new <see cref="SolutionFilter"/>, which has not been written anywhere.</returns>
        public static SolutionFilter Create(string? filterFilePath, IEnumerable<string> projectPaths)
        {
            if (!CanCreateFrom(filterFilePath))
            {
                throw new InvalidOperationException(
                    $"Cannot create a solution filter referencing {filterFilePath ?? "nothing"}: " +
                    "a solution (.sln, .slnx) or another solution filter (.slnf) is required.");
            }

            var solutionPath = filterFilePath!.EndsWith(".slnf")
                ? LoadFromFile(filterFilePath).SolutionPath
                : filterFilePath;

            return new SolutionFilter(solutionPath, projectPaths);
        }

        /// <summary>
        /// Reads a solution filter from disk.
        /// </summary>
        /// <param name="filterFilePath">Path to the <c>.slnf</c> file.</param>
        /// <returns>The parsed <see cref="SolutionFilter"/>.</returns>
        public static SolutionFilter LoadFromFile(string filterFilePath)
        {
            if (string.IsNullOrWhiteSpace(filterFilePath))
            {
                throw new ArgumentException("A path to the solution filter is required.", nameof(filterFilePath));
            }

            return Parse(File.ReadAllText(filterFilePath), filterFilePath);
        }

        /// <summary>
        /// Parses a solution filter document.
        /// </summary>
        /// <param name="json">Contents of the <c>.slnf</c> file.</param>
        /// <param name="filterFilePath">
        /// Path the <paramref name="json"/> was read from.
        /// Relative paths inside the document are resolved against its directory.
        /// </param>
        /// <returns>The parsed <see cref="SolutionFilter"/>.</returns>
        public static SolutionFilter Parse(string json, string filterFilePath)
        {
            if (string.IsNullOrWhiteSpace(filterFilePath))
            {
                throw new ArgumentException("A path to the solution filter is required.", nameof(filterFilePath));
            }

            SolutionFilterDocument? document;
            try
            {
                document = JsonSerializer.Deserialize<SolutionFilterDocument>(json, ReadOptions);
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException(
                    $"{filterFilePath} is not a valid solution filter file: the contents are not valid JSON.",
                    exception);
            }

            var solution = document?.Solution;
            if (solution is null || string.IsNullOrWhiteSpace(solution.Path))
            {
                throw new InvalidOperationException(
                    $"{filterFilePath} is not a valid solution filter file: solution.path is missing.");
            }

            var solutionPath = Resolve(GetDirectory(filterFilePath), solution.Path!);
            var solutionDirectory = GetDirectory(solutionPath);

            var projectPaths = (solution.Projects ?? Array.Empty<string>())
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Select(path => Resolve(solutionDirectory, path))
                .ToArray();

            return new SolutionFilter(solutionPath, projectPaths);
        }

        /// <summary>
        /// Formats this filter as a solution filter document.
        /// </summary>
        /// <param name="filterFilePath">
        /// Path the document is meant to be stored at.
        /// The solution's path is made relative to its directory.
        /// </param>
        /// <returns>The JSON contents of the <c>.slnf</c> file.</returns>
        public string ToJson(string filterFilePath)
        {
            if (string.IsNullOrWhiteSpace(filterFilePath))
            {
                throw new ArgumentException("A path to the solution filter is required.", nameof(filterFilePath));
            }

            var filterDirectory = GetDirectory(filterFilePath);
            var solutionDirectory = GetDirectory(SolutionPath);

            var document = new SolutionFilterDocument
            {
                Solution = new SolutionDocument
                {
                    Path = ToFilterSeparators(Path.GetRelativePath(filterDirectory, SolutionPath)),
                    Projects = ProjectPaths
                        .Select(path => ToFilterSeparators(Path.GetRelativePath(solutionDirectory, path)))
                        .ToArray()
                }
            };

            return JsonSerializer.Serialize(document, WriteOptions);
        }

        /// <summary>
        /// Writes this filter to disk as a solution filter document.
        /// </summary>
        /// <param name="filterFilePath">Path to the <c>.slnf</c> file to create.</param>
        /// <param name="cancellationToken">Cancels the write.</param>
        /// <returns>A task that completes once the file has been written.</returns>
        public Task SaveAsync(string filterFilePath, CancellationToken cancellationToken = default)
        {
            return File.WriteAllTextAsync(filterFilePath, ToJson(filterFilePath), cancellationToken);
        }

        private static string Resolve(string baseDirectory, string path)
        {
            var fixedPath = FromFilterSeparators(path);

            return Path.GetFullPath(Path.IsPathRooted(fixedPath)
                ? fixedPath
                : Path.Combine(baseDirectory, fixedPath));
        }

        private static string GetDirectory(string path)
        {
            // GetFullPath always yields a rooted path,
            // so an empty directory means the path is a root itself.
            var directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (string.IsNullOrEmpty(directory))
            {
                throw new InvalidOperationException($"Could not determine the directory of {path}");
            }

            return directory;
        }

        /// <summary>
        /// Mirrors MSBuild's <c>FileUtilities.FixFilePath</c>, which is how solution
        /// filter paths are interpreted. Note it only translates Windows separators,
        /// hence <see cref="ToFilterSeparators"/> must always write those.
        /// </summary>
        private static string FromFilterSeparators(string path)
        {
            return Path.DirectorySeparatorChar == '\\'
                ? path
                : path.Replace('\\', Path.DirectorySeparatorChar);
        }

        private static string ToFilterSeparators(string path)
        {
            // On Windows this is a no-op. Everywhere else every path shares a single
            // root, so GetRelativePath never returns a rooted path we could mangle.
            return path.Replace(Path.DirectorySeparatorChar, '\\');
        }

        private sealed class SolutionFilterDocument
        {
            [JsonPropertyName("solution")]
            public SolutionDocument? Solution { get; set; }
        }

        private sealed class SolutionDocument
        {
            [JsonPropertyName("path")]
            public string? Path { get; set; }

            [JsonPropertyName("projects")]
            public string[]? Projects { get; set; }
        }
    }
}
