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
    public class GitChangesProvider : IChangesProvider, IDisposable
    {
        private readonly AffectedOptions options;
        private readonly Repository repository;
        internal static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public GitChangesProvider(AffectedOptions options)
        {
            this.options = options;
            this.repository = new Repository(options.RepositoryPath);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetChangedFiles(string directory, string from, string to)
        {
            var changes = GetChangesForRange<TreeChanges>();
            return TreeChangesToPaths(changes);
        }

        /// <inheritdoc />
        public Project? LoadProject(string pathToFile, string? commitRef, bool fallbackToHead)
        {
            Commit? commit;

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
            using var fs = new EagerCachingMsBuildGitFileSystem(repository, commit);
            return fs.FileExists(pathToFile) ? fs.CreateProjectAndEagerLoadChildren(pathToFile) : null;
        }
        
        /// <inheritdoc />
        public MSBuildFileSystemBase CreateMsBuildFileSystem()
        {
            var (fromCommit, toCommit) = ParseRevisionRanges();
            
            return fromCommit is null
                ? new EagerCachingMsBuildGitFileSystem(repository, toCommit)
                : new EagerCachingMsBuildGitFileSystem(repository, fromCommit);
        }

        private (Commit? From, Commit To) ParseRevisionRanges()
        {
            // Find the To Commit or use HEAD.
            var toCommit = GetCommitOrHead(repository, options.ToRef);

            // No from: compare against working directory
            if (string.IsNullOrWhiteSpace(options.FromRef))
            {
                // this.WriteLine($"Finding changes from working directory against {to}");
                return (null, toCommit);
            }

            var fromCommit = GetCommitOrThrow(repository, options.FromRef);
            return (fromCommit, toCommit);
        }

        private T GetChangesForRange<T>()
            where T : class, IDiffResult
        {
            var (fromCommit, toCommit) = ParseRevisionRanges();

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

        private IEnumerable<string> TreeChangesToPaths(
            TreeChanges changes)
        {
            foreach (var change in changes)
            {
                if (change == null) continue;

                var currentPath = Path.Combine(options.RepositoryPath, change.Path);

                yield return currentPath;
            }
        }

        public void Dispose()
        {
            repository.Dispose();
        }
    }
}
