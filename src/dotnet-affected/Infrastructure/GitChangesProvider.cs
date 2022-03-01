using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

        public IEnumerable<string> GetChangedCentrallyManagedNuGetPackages(string directory, string directoryPackagesPropsPath, string @from, string to)
        {
            using var repository = new Repository(directory);

            // Find the To Commit or use HEAD.
            var toCommit = GetCommitOrHead(repository, to);
            
            // No from: compare against working directory
            if (string.IsNullOrWhiteSpace(from))
            {
                return GetChangedNuGetPackagesAgainstWorkingDirectory(
                    repository, 
                    toCommit.Tree, 
                    directoryPackagesPropsPath);
            }

            var fromCommit = GetCommitOrThrow(repository, from);

            return GetChangedNuGetPackagesBetweenTrees(
                repository,
                fromCommit.Tree,
                toCommit.Tree,
                directoryPackagesPropsPath);
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
        
        private static IEnumerable<string> GetChangedNuGetPackagesAgainstWorkingDirectory(
            Repository repository,
            Tree tree,
            string path)
        {
            var patch = repository.Diff.Compare<Patch>(
                tree,
                DiffTargets.Index | DiffTargets.WorkingDirectory,
                new [] { path });

            return PatchToNuGetPackageNames(patch, path);
        }

        private static IEnumerable<string> GetChangedNuGetPackagesBetweenTrees(
            Repository repository,
            Tree fromTree,
            Tree toTree,
            string path)
        {
            var patch = repository.Diff.Compare<Patch>(
                fromTree,
                toTree,
                new [] { path });

            return PatchToNuGetPackageNames(patch, path);
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
        
        private static readonly Regex _packageVersionRegex = 
            new Regex("<PackageVersion Include=\"(.*)\" Version=\"(.*)\"\\s?/>");

        private static IEnumerable<string> PatchToNuGetPackageNames(Patch patch, string path)
        {
            // Get the patch for the Directory.Packages.props file
            var fileName = Path.GetFileName(path);
            var filePatch = patch[fileName];

            // Run through all lines, get the NuGet package name using RegEx and yield unique package names
            var lines = filePatch.AddedLines.Concat(filePatch.DeletedLines);
            var returned = new HashSet<string>();
            foreach (var line in lines)
            {
                var match = _packageVersionRegex.Match(line.Content);
                if (!match.Success) continue;
                
                var packageName = match.Groups[1].Value;
                if (returned.Add(packageName))
                {
                    yield return packageName;
                }
            }
        }
    }
}
