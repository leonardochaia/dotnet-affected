using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Affected.Cli
{
    internal static class GitUtils
    {
        public static IEnumerable<string> GetChangesAgainstWorkingDirectory(
            Repository repository,
            Tree tree)
        {
            var changes = repository.Diff.Compare<TreeChanges>(
                tree,
                DiffTargets.Index | DiffTargets.WorkingDirectory);

            return TreeChangesToPaths(changes, repository.Info.WorkingDirectory);
        }

        public static IEnumerable<string> GetChangesBetweenTrees(
            Repository repository,
            Tree fromTree,
            Tree toTree)
        {
            var changes = repository.Diff.Compare<TreeChanges>(
                fromTree,
                toTree);

            return TreeChangesToPaths(changes, repository.Info.WorkingDirectory);
        }

        public static Commit GetCommitOrHead(Repository repository, string name)
        {
            if (name is null)
            {
                return repository.Head.Tip;
            }
            else
            {
                return GetCommitOrThrow(repository, name);
            }
        }

        public static Commit GetCommitOrThrow(Repository repo, string name)
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
