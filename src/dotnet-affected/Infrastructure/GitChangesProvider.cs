using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Affected.Cli
{
    internal class GitChangesProvider : IChangesProvider
    {
        public IEnumerable<string> GetChangedFiles(string directory, string from, string to)
        {
            using var repository = new Repository(directory);

            // Find the To Commit or use HEAD.
            var toCommit = GetCommitOrHead(repository, to);

            // No from: compare against working directory
            if (string.IsNullOrWhiteSpace(from))
            {
                // this.WriteLine($"Finding changes from working directory against {to}");

                return GetChangesAgainstWorkingDirectory(repository, toCommit.Tree, directory);
            }

            var fromCommit = GetCommitOrThrow(repository, @from);
            // this.WriteLine($"Finding changes from {from} against {to}");

            // Compare the two commits.
            return GetChangesBetweenTrees(
                repository,
                fromCommit.Tree,
                toCommit.Tree,
                directory);
        }

        private static IEnumerable<string> GetChangesAgainstWorkingDirectory(
            Repository repository,
            Tree tree,
            string repositoryRootPath)
        {
            var changes = repository.Diff.Compare<TreeChanges>(
                tree,
                DiffTargets.Index | DiffTargets.WorkingDirectory);

            return TreeChangesToPaths(changes, repositoryRootPath);
        }

        private static IEnumerable<string> GetChangesBetweenTrees(Repository repository,
            Tree fromTree,
            Tree toTree,
            string repositoryRootPath)
        {
            var changes = repository.Diff.Compare<TreeChanges>(
                fromTree,
                toTree);

            return TreeChangesToPaths(changes, repositoryRootPath);
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
