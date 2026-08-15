using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Answers whether a path under a repository is ignored by git, so that project discovery
    /// can leave out what git leaves out.
    /// </summary>
    internal sealed class GitIgnoreFilter : IDisposable
    {
        private readonly Repository _repository;
        private readonly string _rootPath;

        private GitIgnoreFilter(Repository repository, string rootPath)
        {
            _repository = repository;
            _rootPath = rootPath;
        }

        /// <summary>
        /// Opens the repository rooted at <paramref name="rootPath"/>, or returns <b>null</b>
        /// when there is no repository there. Discovery is also reachable from places that never
        /// involve git, such as the benchmarks, and those must keep working.
        /// </summary>
        public static GitIgnoreFilter? TryCreate(string rootPath)
        {
            try
            {
                return new GitIgnoreFilter(new Repository(rootPath), rootPath);
            }
            catch (RepositoryNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Whether git ignores <paramref name="path"/>.
        /// </summary>
        /// <param name="path">A path under the root this filter was created for.</param>
        /// <param name="isDirectory">
        /// Directory patterns such as <c>bin/</c> only match once git knows the path is a
        /// directory, which is what the trailing separator tells it.
        /// </param>
        public bool IsIgnored(string path, bool isDirectory)
        {
            var relativePath = ToRepositoryPath(path);

            return _repository.Ignore.IsPathIgnored(isDirectory ? relativePath + "/" : relativePath);
        }

        /// <summary>
        /// Full paths of the files in the index whose repository path satisfies
        /// <paramref name="predicate"/>.
        /// </summary>
        public IEnumerable<string> EnumerateTrackedFiles(Func<string, bool> predicate)
        {
            foreach (var entry in _repository.Index)
            {
                if (!predicate(entry.Path))
                    continue;

                yield return Path.Combine(_rootPath, ToPlatformPath(entry.Path));
            }
        }

        public void Dispose() => _repository.Dispose();

        /// <summary>
        /// REMARKS: relative to the root we were given, not to
        /// <see cref="RepositoryInformation.WorkingDirectory"/>. The two differ whenever the
        /// repository is reached through a symlink, which is the normal case for a temp directory
        /// on macOS, and git reports the resolved form. Mixing them yields a relative path that
        /// escapes the working directory, which git considers ignored, and every project would
        /// silently disappear from discovery.
        /// </summary>
        private string ToRepositoryPath(string path)
        {
            var relativePath = Path.GetRelativePath(_rootPath, path);

            return GitChangesProvider.IsWindows
                ? relativePath.Replace('\\', '/')
                : relativePath;
        }

        private static string ToPlatformPath(string repositoryPath)
            => GitChangesProvider.IsWindows
                ? repositoryPath.Replace('/', Path.DirectorySeparatorChar)
                : repositoryPath;
    }
}
