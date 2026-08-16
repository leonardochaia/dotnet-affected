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
        public IEnumerable<string> GetChangedFiles(string directory, string from, UncommittedChanges uncommitted)
        {
            using var repository = new Repository(directory);

            var changes = GetChangesForRange<TreeChanges>(repository, from, uncommitted);

            return TreeChangesToPaths(changes, directory);
        }

        /// <inheritdoc />
        public string? GetWorkingTreeCommitSha(string directory)
        {
            using var repository = new Repository(directory);

            return repository.Head.Tip?.Sha;
        }

        /// <inheritdoc />
        public string ResolveCommitSha(string directory, string commitRef)
        {
            using var repository = new Repository(directory);

            return GetCommitOrThrow(repository, commitRef)
                .Sha;
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

        /// <summary>
        /// The endpoint of the comparison is never a ref: projects are discovered and evaluated
        /// from the working tree, so the working tree is what the changes have to be measured
        /// against. <paramref name="uncommitted"/> only decides how much of it counts, and
        /// <see cref="UncommittedChanges.None"/> stops at the commit it is checked out at.
        /// </summary>
        private static T GetChangesForRange<T>(
            Repository repository,
            string from,
            UncommittedChanges uncommitted)
            where T : class, IDiffResult
        {
            // No from: compare against the commit that is checked out.
            var fromCommit = GetCommitOrHead(repository, from);

            return uncommitted == UncommittedChanges.None
                ? GetChangesBetweenTrees<T>(repository, fromCommit.Tree, repository.Head.Tip.Tree)
                : GetChangesAgainstWorkingDirectory<T>(repository, fromCommit.Tree, uncommitted);
        }

        private static T GetChangesAgainstWorkingDirectory<T>(
            Repository repository,
            Tree tree,
            UncommittedChanges uncommitted,
            IEnumerable<string>? files = null)
            where T : class, IDiffResult
        {
            // Diffing against the index alone leaves unstaged edits, and files git has not been
            // told about at all, out of the comparison.
            var targets = uncommitted == UncommittedChanges.Staged
                ? DiffTargets.Index
                : DiffTargets.Index | DiffTargets.WorkingDirectory;

            return repository.Diff.Compare<T>(
                tree,
                targets,
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
