using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Affected.Cli
{
    internal class GitChangesProvider : IChangesProvider
    {
        public IEnumerable<string> GetChangedFiles(string directory, string from, string to)
        {
            using var repository = new Repository(directory);

            var changes = GetChangesForRange<TreeChanges>(repository, from, to);

            return TreeChangesToPaths(changes, directory);
        }

        public (string FromText, string ToText) GetTextFileContents(
            string directory,
            string pathToFile,
            string from,
            string to)
        {
            using var repository = new Repository(directory);

            var (fromCommit, toCommit) = ParseRevisionRanges(repository, from, to);
            
            pathToFile = Path.GetRelativePath(directory, pathToFile);

            // Read file from commit or working directory
            var fromText = fromCommit is null
                ? File.ReadAllText(Path.Combine(directory, pathToFile))
                : ReadTextFile(pathToFile, fromCommit);

            var toText = ReadTextFile(pathToFile, toCommit);

            return (fromText, toText);
        }

        private static string ReadTextFile(string pathToFile, Commit commit)
        {
            var blob = (Blob)commit[pathToFile].Target;

            using var content = new StreamReader(blob.GetContentStream(), Encoding.UTF8);
            return content.ReadToEnd();
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
