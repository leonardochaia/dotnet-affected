using DotnetAffected.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetAffected.Core
{
    internal class DirectoryProjectDiscoverer : IProjectDiscoverer
    {
        private const string GitDirectoryName = ".git";

        private static readonly string[] ProjectFileExtensions =
        {
            ".csproj", ".fsproj", ".vbproj"
        };

        private static readonly StringComparer PathComparer = GitChangesProvider.IsWindows
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        public IEnumerable<string> DiscoverProjects(IDiscoveryOptions options)
        {
            // Trailing separators would make the paths built from the index differ, as strings,
            // from the ones the walk yields, and the same project would be discovered twice.
            var rootPath = Path.TrimEndingDirectorySeparator(options.RepositoryPath);

            if (!options.HonourGitIgnore)
                return EnumerateProjectFiles(rootPath, null)
                    .ToArray();

            using var filter = GitIgnoreFilter.TryCreate(rootPath);
            if (filter is null)
                return EnumerateProjectFiles(rootPath, null)
                    .ToArray();

            var projects = new HashSet<string>(EnumerateProjectFiles(rootPath, filter), PathComparer);

            // A tracked file is repository content no matter which patterns match it, so
            // `git add -f build/Tool.csproj` keeps the project discoverable. The walk stopped at
            // the ignored directory holding it, so the index is what puts it back.
            foreach (var trackedFile in filter.EnumerateTrackedFiles(IsProjectFile))
            {
                if (File.Exists(trackedFile))
                    projects.Add(trackedFile);
            }

            return projects.ToArray();
        }

        /// <summary>
        /// Walks <paramref name="rootPath"/>, pruning whole directories rather than filtering the
        /// files they hold: the ignored ones are where the bulk of a repository's files live, and
        /// descending into them costs far more than the discovery itself.
        /// </summary>
        private static IEnumerable<string> EnumerateProjectFiles(string rootPath, GitIgnoreFilter? filter)
        {
            var pendingDirectories = new Queue<string>();
            pendingDirectories.Enqueue(rootPath);

            while (pendingDirectories.Count > 0)
            {
                var directory = pendingDirectories.Dequeue();

                foreach (var file in Directory.EnumerateFiles(directory))
                {
                    if (IsProjectFile(file) && filter?.IsIgnored(file, isDirectory: false) != true)
                        yield return file;
                }

                foreach (var childDirectory in Directory.EnumerateDirectories(directory))
                {
                    // Git does not ignore its own directory, it just never looks at it. Neither
                    // should we: it holds no projects and, in a packed repository, most of the
                    // files under the root.
                    if (string.Equals(Path.GetFileName(childDirectory), GitDirectoryName,
                            StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (filter?.IsIgnored(childDirectory, isDirectory: true) == true)
                        continue;

                    pendingDirectories.Enqueue(childDirectory);
                }
            }
        }

        private static bool IsProjectFile(string path)
            => ProjectFileExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    }
}
