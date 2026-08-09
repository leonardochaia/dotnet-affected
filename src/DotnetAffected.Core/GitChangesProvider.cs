using DotnetAffected.Abstractions;
using DotnetAffected.Core.FileSystem;
using LibGit2Sharp;
using Microsoft.Build.Evaluation;
using Microsoft.Build.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DotnetAffected.Core
{
    /// <summary>
    /// Detects changes using Git.
    /// </summary>
    public class GitChangesProvider : IChangesProvider
    {
        internal static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <inheritdoc />
        public IEnumerable<string> GetChangedFiles(string directory, string from, string to)
        {
            using var repository = new Repository(directory);

            var changes = GetChangesForRange<TreeChanges>(repository, from, to);

            return TreeChangesToPaths(changes, directory);
        }

        /// <inheritdoc />
        public Project? LoadDirectoryPackagePropsProject(string directory, string pathToFile, string? commitRef,
            bool fallbackToHead)
        {
            var project = LoadProject(directory, pathToFile, commitRef, fallbackToHead);
            if (project is null)
            {
                var fi = new FileInfo(pathToFile);
                var parent = fi.Directory?.Parent?.FullName;
                if (parent is not null && parent.Length >= directory.Length)
                    return LoadDirectoryPackagePropsProject(directory, Path.Combine(parent, "Directory.Packages.props"),
                        commitRef, fallbackToHead);
            }

            return project;
        }

        /// <inheritdoc />
        public Project? LoadProject(string directory, string pathToFile, string? commitRef, bool fallbackToHead)
        {
            return LoadProjectCore(directory, pathToFile, commitRef, fallbackToHead);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, byte[]> ReadFilesAt(
            string directory,
            string? commitRef,
            IReadOnlyCollection<string> filePaths)
        {
            var contents = new Dictionary<string, byte[]>();
            if (filePaths.Count == 0)
                return contents;

            using var repository = new Repository(directory);

            var commit = string.IsNullOrWhiteSpace(commitRef)
                ? repository.Head.Tip
                : GetCommitOrThrow(repository, commitRef);

            foreach (var path in filePaths)
            {
                // REMARKS: relative to the directory we were given, not to
                // repository.Info.WorkingDirectory. The two differ whenever the repository is
                // reached through a symlink, which is the normal case for a temp directory on
                // macOS, and git reports the resolved form. Mixing them yields a relative path
                // that matches no blob, and the miss is silent: the deleted file is never restored
                // and the project owning it is never reported as changed.
                var relativePath = Path.GetRelativePath(directory, path);
                if (IsWindows)
                    relativePath = relativePath.Replace('\\', '/');

                if (commit[relativePath]?.Target is not Blob blob)
                    continue;

                using var stream = blob.GetContentStream();
                using var buffer = new MemoryStream();
                stream.CopyTo(buffer);

                contents[Path.GetFullPath(path)] = buffer.ToArray();
            }

            return contents;
        }

        private Project? LoadProjectCore(string directory, string pathToFile, string? commitRef, bool fallbackToHead)
        {
            Commit? commit;

            using var repository = new Repository(directory);

            if (string.IsNullOrWhiteSpace(commitRef))
                commit = fallbackToHead ? repository.Head.Tip : null;
            else
                commit = GetCommitOrThrow(repository, commitRef);

            /* TODO: Uncomment if/when https://github.com/dotnet/msbuild/issues/7956 is fixed. */
            // using var projectFactory = new ProjectFactory(new MsBuildGitFileSystem(repository, commit), new ProjectCollection());
            // return projectFactory.FileSystem.FileExists(pathToFile)
            //     ? projectFactory.CreateProject(pathToFile)
            //     : null;

            /* Workaround for https://github.com/dotnet/msbuild/issues/7956
               For more information, see comments in EagerCachingMsBuildGitFileSystem
               TODO: Delete EagerCachingMsBuildGitFileSystem and this code if/when 7956 is fixed. */
            // Paths arrive rooted at the directory we were given, so normalize against that rather
            // than the resolved one git reports. See the remarks on MsBuildGitFileSystem.
            using var fs = new EagerCachingMsBuildGitFileSystem(repository, commit, directory);
            return fs.FileExists(pathToFile) ? fs.CreateProjectAndEagerLoadChildren(pathToFile) : null;
        }

        private static (Commit? From, Commit To) ParseRevisionRanges(
            Repository repository,
            string from,
            string to)
        {
            // Find the To Commit or use HEAD.
            var toCommit = GetCommitOrHead(repository, to);

            // No from: compare against working directory
            if (string.IsNullOrWhiteSpace(from))
            {
                // this.WriteLine($"Finding changes from working directory against {to}");
                return (null, toCommit);
            }

            var fromCommit = GetCommitOrThrow(repository, @from);
            return (fromCommit, toCommit);
        }

        private static T GetChangesForRange<T>(
            Repository repository,
            string from,
            string to)
            where T : class, IDiffResult
        {
            var (fromCommit, toCommit) = ParseRevisionRanges(repository, from, to);

            return fromCommit is null
                ? GetChangesAgainstWorkingDirectory<T>(repository, toCommit.Tree)
                : GetChangesBetweenTrees<T>(repository, fromCommit.Tree, toCommit.Tree);
        }

        private static T GetChangesAgainstWorkingDirectory<T>(
            Repository repository,
            Tree tree,
            IEnumerable<string>? files = null)
            where T : class, IDiffResult
        {
            return repository.Diff.Compare<T>(
                tree,
                DiffTargets.Index | DiffTargets.WorkingDirectory,
                files);
        }

        private static T GetChangesBetweenTrees<T>(
            Repository repository,
            Tree fromTree,
            Tree toTree,
            IEnumerable<string>? files = null)
            where T : class, IDiffResult
        {
            return repository.Diff.Compare<T>(
                fromTree,
                toTree,
                files);
        }

        private static Commit GetCommitOrHead(Repository repository, string name)
        {
            return string.IsNullOrWhiteSpace(name) ? repository.Head.Tip : GetCommitOrThrow(repository, name);
        }

        private static Commit GetCommitOrThrow(Repository repo, string name)
        {
            var commit = repo.Lookup<Commit>(name);
            if (commit != null)
            {
                return commit;
            }

            var branch = repo.Branches[name];
            if (branch != null)
            {
                return branch.Tip;
            }

            throw new InvalidOperationException(
                $"Couldn't find Git Commit or Branch with name {name} in repository {repo.Info.Path}");
        }

        private static IEnumerable<string> TreeChangesToPaths(
            TreeChanges changes,
            string repositoryRootPath)
        {
            foreach (var change in changes)
            {
                if (change == null) continue;

                var currentPath = Path.Combine(repositoryRootPath, change.Path);

                yield return currentPath;
            }
        }
    }
}
